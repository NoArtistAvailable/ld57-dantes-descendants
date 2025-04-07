using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LD57
{
    // LIMBO (Circle 0) - Introduction - Active Card
    public class MockingJab : ActiveCard
    {
        public override int circleOfHell => 0;
        public override string Name => "Mocking Jab";
        public override string Description => $"Deals ({damage}) damage and reduces their power by {powerRecution*100}% for {duration}s every {cooldown}s";
        public override float cooldown => 3f;
        
        public float damage = 3f;
        public float powerRecution = 0.2f;
        public float duration = 6;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var target = CombatManager.GetHealthiestEnemy(activator);
            if (!target) return;
            
            Debug.Log($"{activator.Unit.name} uses {Name} on {target.Unit.name}");
            target.Damage(activator.PowerCalc * damage, activator);

            float Debuff(float value) => value * (1 - powerRecution);
            target.powerChanges.Add(Debuff);

            async void Remove()
            {
                await WebTask.Delay(duration);
                target.powerChanges.Remove(Debuff);
            }
            Remove();
        }
    }
    
    // LUST (Circle 1) - Meta Setup - Passive Card
    public class Mod_Unpredictable : Card, IManipulateSpeed
    {
        public override int circleOfHell => 1;
        public override string Name => "Unpredictable";
        public override string Description => $"Increases speed by {speedBoost*100}% per circle of hell";
        private float speedBoost = 0.05f; // 25% speed boost
        
        public float ManipulateSpeed(float value)
        {
            return value + speedBoost * PlayerManager.instance.circleOfHell;
        }
    }
    
    // GLUTTONY (Circle 2) - Center Piece Ability - Active Card
    public class MockingLaugh : ActiveCard
    {
        public override int circleOfHell => 2;
        public override string Name => "Mocking Laugh";
        public override string Description => $"Enemies take {damageIncrease*100}% more damage every {cooldown}s";
        public override float cooldown => 3.5f;
        public override string animName => "Buff";
        
        public float damageIncrease = 0.15f;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var enemies = CombatManager.GetEnemies(activator);
            Debug.Log($"{activator.Unit.name} uses {Name}, taunting all enemies");
            
            foreach (var enemy in enemies)
            {
                enemy.receiveDamageChanges.Add(val => val + damageIncrease);
            }
        }
    }
    
    // GREED (Circle 3) - Pay Off - Passive Card
    public class Mod_TrickstersFavor : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 3;
        public override string Name => "Trickster's Favor";
        public override string Description => $"Every time you act an ally gains a random {amount*100}% increase or an enemy a {amount*100}% decrease";
        private float amount = 0.1f;

        public static void RandomStatChange(UnitCombatBehaviour target, float amount)
        {
            var rand = target.random.NextInt(4);
            if (rand == 3) target.powerChanges.Add(original => original + amount);
            else if(rand == 2) target.critChanges.Add(original => original + amount);
            else if(rand == 1) target.speedChanges.Add(original => original + amount);
            else if(rand == 0) target.receiveDamageChanges.Add(original => original - amount);
        }
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            behaviour.OnCardActivated += (unit, card) =>
            {
                var buffing = behaviour.random.NextFloat() > 0.5f;
                if (buffing)
                {
                    var friends = CombatManager.GetFriends(behaviour).Where(x => x != behaviour).ToList();
                    if (friends.Count == 0) return;
                    var target = friends.GetRandom();
                    RandomStatChange(target, amount);
                }
                else
                {
                    var enemies = CombatManager.GetEnemies(behaviour).Where(x => x != behaviour).ToList();
                    if (enemies.Count == 0) return;
                    var target = enemies.GetRandom();
                    RandomStatChange(target, -amount);
                }
            };
        }
    }
    
    // WRATH (Circle 4) - Power Escalation With Drawback - Passive Card
    public class Mod_WildCard : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 4;
        public override string Name => "Wild Card";
        public override string Description => $"Each action has {actionBoostChance*100}% chance to deal {damageBoost*100}% more damage or {actionFailChance*100}% chance to backfire and deal {backfireDamage} self-damage";
        private float actionBoostChance = 0.4f; // 40% chance for bonus
        private float actionFailChance = 0.2f; // 20% chance for backfire
        private float damageBoost = 0.75f; // 75% more damage
        private float backfireDamage = 5f; // Self-damage on backfire
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            if (behaviour == null) return;
            
            behaviour.OnCardActivated += (unit, card) => {
                // Roll for random effect
                float roll = UnityEngine.Random.Range(0f, 1f);
                
                if (roll < actionBoostChance)
                {
                    // Temporary damage boost for this action
                    behaviour.powerChanges.Add((original) => original * (1 + damageBoost));
                    Debug.Log($"{behaviour.Unit.name}'s {Name} boosts damage by {damageBoost*100}%");
                    
                    // Remove the boost after a short delay
                    async void RemoveBoost()
                    {
                        await WebTask.Delay(0.5f);
                        behaviour.powerChanges.RemoveAt(behaviour.powerChanges.Count - 1);
                    }
                    RemoveBoost();
                }
                else if (roll < actionBoostChance + actionFailChance)
                {
                    // Backfire: damage self
                    Debug.Log($"{behaviour.Unit.name}'s {Name} backfires, causing {backfireDamage} self-damage");
                    behaviour.Damage(backfireDamage, behaviour);
                }
            };
        }
    }
    
    // HERESY (Circle 5) - Introduce A Central Flaw without Any Benefit - Passive Card
    public class Mod_Unstable : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 5;
        public override string Name => "Unstable";
        public override string Description => $"When hurt decrease a random stat by {amount * 100}%";
        private float amount = 0.1f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            behaviour.OnIGotHurt += (damage, source) =>
            {
                if (source == behaviour) return;
                Mod_TrickstersFavor.RandomStatChange(behaviour, -amount);
            };
        }
    }
    
    // VIOLENCE (Circle 6) - Improved Active Abilities - Active Card
    public class ChaosBlast : ActiveCard
    {
        public override int circleOfHell => 6;
        public override string Name => "Confusing Tirade";
        public override string Description => $"Hits two random enemies for {baseDamage} damage every {cooldown}s";
        public override float cooldown => 1f;
        
        public float baseDamage = 1;

        public override void Activate(UnitCombatBehaviour activator)
        {
            var enemies = CombatManager.GetEnemies(activator).ToList();
            Debug.Log($"{activator.Unit.name} unleashes a {Name}");
            if (enemies.Count == 0) return;
            var target = enemies.GetRandom();
            target.Damage(baseDamage * activator.PowerCalc, activator);
            enemies.Remove(target);
            if (enemies.Count == 0) return;
            enemies.GetRandom().Damage(baseDamage * activator.PowerCalc, activator);
        }
    }
    
    // FRAUD (Circle 7) - Luck and Deception - Active Card
    public class Mod_SnakeOil : Card
    {
        public override int circleOfHell => 7;
        public override string Name => "Snake Oil";
        public override string Description => $"Does Nothing";
    }
    
    // TREACHERY (Circle 8) - Self Curses Without Any Benefits - Passive Card
    public class Mod_Jinxed : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 8;
        public override string Name => "Jinxed";
        public override string Description => $"Every time you act, an ally gets a random {debuff*100}% decrease";
        private float debuff = 0.9f;
        
        public static void RandomStatChangeMultiplier(UnitCombatBehaviour target, float amount)
        {
            var rand = target.random.NextInt(4);
            if (rand == 3) target.powerChanges.Add(original => original * (1 + amount));
            else if(rand == 2) target.critChanges.Add(original => original * (1 + amount));
            else if(rand == 1) target.speedChanges.Add(original => original * (1 + amount));
            else if(rand == 0) target.receiveDamageChanges.Add(original => original  * (1 - amount));
        }
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            behaviour.OnCardActivated += (unit, card) =>
            {
                var friends = CombatManager.GetFriends(behaviour).ToList();
                if (friends.Count == 0) return;
                RandomStatChangeMultiplier(friends.GetRandom(), -debuff);
            };
        }
    }
}