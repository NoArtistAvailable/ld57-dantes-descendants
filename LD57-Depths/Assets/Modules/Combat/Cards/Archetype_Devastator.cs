using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
    // LIMBO (Circle 0) - Introduction - Active Card
    public class GroupInsult : ActiveCard
    {
        public override int circleOfHell => 0;
        public override string Name => "Group Insult";
        public override string Description => $"Deals ({damage}) damage to all enemies every {cooldown}s";
        public override float cooldown => 4.5f;
        
        public float damage = 1f;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var enemies = CombatManager.GetEnemies(activator);
            Debug.Log($"{activator.Unit.name} uses {Name} on all enemies");
            
            foreach (var enemy in enemies)
            {
                enemy.Damage(activator.PowerCalc * damage, activator);
            }
        }
    }
    
    // LUST (Circle 1) - Meta Setup - Passive Card
    public class Mod_Overwhelming : Card, IManipulatePower
    {
        public override int circleOfHell => 1;
        public override string Name => "Overwhelming";
        public override string Description => $"Increases power by {powerBoost*100}% per circle of hell";
        private float powerBoost = 0.1f; // 25% power boost
        
        public float ManipulatePower(float value)
        {
            return value + powerBoost * PlayerManager.instance.circleOfHell;
        }
    }
    
    // GLUTTONY (Circle 2) - Center Piece Ability - Active Card
    public class GettingThePointAcross : ActiveCard
    {
        public override int circleOfHell => 2;
        public override string Name => "Getting the point across";
        public override string Description => $"Deals {damage} damage {amount} times every {cooldown} seconds";
        public override float cooldown => 2f;
        
        public float damage = 2f;
        public int amount = 3;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var enemies = CombatManager.GetEnemies(activator).ToList();
            Debug.Log($"{activator.Unit.name} is {Name}");
            async void Do(){
                for (int i = 0; i < amount; i++)
                {
                    enemies.GetRandom().Damage(damage * activator.PowerCalc, activator);
                    await WebTask.Delay(0.2f);
                }
            }
            Do();
        }
    }
    
    // GREED (Circle 3) - Pay Off - Passive Card
    public class Mod_Momentum : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 3;
        public override string Name => "Momentum Mori";
        public override string Description => $"Every time you hurt an enemy, they lose {amount*100}% power";
        private float amount = 0.05f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            foreach (var enemy in CombatManager.GetEnemies(behaviour))
            {
                enemy.OnIGotHurt += (value, source) =>
                {
                    if (source == behaviour) enemy.powerChanges.Add(original => original - amount);
                };
            }
        }
    }
    
    // WRATH (Circle 4) - Power Escalation With Drawback - Passive Card
    public class Mod_Combustion : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 4;
        public override string Name => "Self Immolation";
        public override string Description => $"Every time you use an ability, damage all enemies for {damage} and lose {selfDamage} health";
        private float damage = 1f;
        private float selfDamage = 2f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            async void Immolation(UnitCombatBehaviour unit, Card card)
            {
                await WebTask.Delay(0.1f);
                foreach (var enemy in CombatManager.GetEnemies(behaviour))
                {
                    enemy.Damage(damage * behaviour.PowerCalc, behaviour);
                }
                behaviour.Damage(selfDamage * behaviour.PowerCalc, behaviour);
            }
            behaviour.OnCardActivated += Immolation;
        }
    }
    
    // HERESY (Circle 5) - Introduce A Central Flaw without Any Benefit - Passive Card
    public class Mod_Unfocused : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 5;
        public override string Name => "Unfocused";
        public override string Description => $"After using an ability slow down by {reduction*100}% for {duration}s";
        private float reduction = 0.5f;
        private float duration = 3;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            behaviour.OnCardActivated += (unit, card) =>
            {
                float SlowDown(float original) => original * (1 - reduction);
                
                behaviour.speedChanges.Add(SlowDown);

                async void Remove()
                {
                    await WebTask.Delay(duration);
                    behaviour.speedChanges.Remove(SlowDown);
                }
                Remove();
            };
        }
    }
    
    // VIOLENCE (Circle 6) - Improved Active Abilities - Active Card
    public class Earthquake : ActiveCard
    {
        public override int circleOfHell => 6;
        public override string Name => "Ground-Shaking Rant";
        public override string Description => $"Deals ({initialDamage}) initial damage and ({aftershockDamage}) aftershock damage {aftershockCount} seconds later to all enemies every {cooldown}s";
        public override float cooldown => 10f;
        
        public float initialDamage = 5f;
        public float aftershockDamage = 3f;
        public float aftershockDelay = 3f;
        public int aftershockCount = 1;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var enemies = CombatManager.GetEnemies(activator);
            Debug.Log($"{activator.Unit.name} triggers an {Name}");
            
            // Deal initial damage
            foreach (var enemy in enemies)
            {
                enemy.Damage(activator.PowerCalc * initialDamage, activator);
            }
            
            // Schedule aftershock
            async void TriggerAftershock()
            {
                await WebTask.Delay(aftershockDelay);
                
                // Get current alive enemies
                var remainingEnemies = CombatManager.GetEnemies(activator);
                Debug.Log($"{activator.Unit.name}'s {Name} causes an aftershock");
                
                foreach (var enemy in remainingEnemies)
                {
                    enemy.Damage(activator.PowerCalc * aftershockDamage, activator);
                }
            }
            
            TriggerAftershock();
        }
    }
    
    // FRAUD (Circle 7) - Luck and Deception - Active Card
    public class Mod_FoolsGold : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 7;
        public override string Name => "Fool's Gold";
        public override string Description => $"{triggerChance*100}% chance to heal for {amount} when all allies died";
        private float triggerChance = 0.2f;
        private float amount = 6f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            foreach (var friend in CombatManager.GetFriends(behaviour))
            {
                friend.OnIDied += () =>
                {
                    if (behaviour.currentHealth <= 0) return;
                    if (CombatManager.GetFriends(behaviour).Count(x => x.currentHealth > 0) > 1) return;
                    if(behaviour.random.NextFloat() < triggerChance) behaviour.Heal(amount * behaviour.PowerCalc, behaviour);
                };
            }
        }
    }
    
    // TREACHERY (Circle 8) - Self Curses Without Any Benefits - Passive Card
    public class Mod_BettingTheHouse : Card, IInitializeOnCombat, IManipulateCrit
    {
        public override int circleOfHell => 8;
        public override string Name => "Betting the house";
        public override string Description => $"Increases crit by {critRate * 100}% and when you crit you die";
        private float critRate = 0.5f; // Damage on backfire

        public float ManipulateCrit(float value)
        {
            return value + critRate;
        }

        public void OnCombatStart(UnitCombatBehaviour unitCombatBehaviour)
        {
            unitCombatBehaviour.OnCrit += () =>
                unitCombatBehaviour.Damage(unitCombatBehaviour.currentHealth, unitCombatBehaviour);
        }
    }
}