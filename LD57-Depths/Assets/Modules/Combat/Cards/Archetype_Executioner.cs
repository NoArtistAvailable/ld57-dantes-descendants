using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
    // LIMBO (Circle 0) - Introduction - Active Card
    public class SharpInsult : ActiveCard, IManipulateCrit
    {
        public override int circleOfHell => 0;
        public override string Name => "Sharp Insult";
        public override string Description => $"Increases Crit Chance by {critChance*100}%\nDeals ({damage}) damage every {cooldown}s";
        public override float cooldown => 3.5f;
        
        public float damage = 4f;
        public float critChance = 0.1f; // 10% bonus crit chance
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var target = CombatManager.GetHealthiestEnemy(activator);
            if (!target) return;
            // Deal damage
            target.Damage(activator.PowerCalc * damage, activator);
        }

        public float ManipulateCrit(float value)
        {
            return value + critChance;
        }
    }
    
    // LUST (Circle 1) - Meta Setup - Passive Card
    public class Mod_Precise : Card, IManipulateCrit
    {
        public override int circleOfHell => 1;
        public override string Name => "Precise";
        public override string Description => $"Increases critical hit chance by {critBoost*100}% per circle of hell";
        private float critBoost = 0.2f; // 20% crit chance boost
        
        public float ManipulateCrit(float value)
        {
            return value + critBoost * PlayerManager.instance.circleOfHell;
        }
    }
    
    // GLUTTONY (Circle 2) - Center Piece Ability - Active Card
    public class Bully : ActiveCard
    {
        public override int circleOfHell => 2;
        public override string Name => "Bully";
        public override string Description => $"Deals {baseDamage} damage to the weakest enemy every {cooldown}s";
        public override float cooldown => 2f;
        
        public float baseDamage = 3f;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var target = CombatManager.GetEnemies(activator)
                .MinBy(x => x.currentHealth / x.Unit.Health); // Target lowest % health enemy
                
            if (!target) return;
            
            Debug.Log($"{activator.Unit.name} attempts to {Name} {target.Unit.name}");
            
        }
    }
    
    // GREED (Circle 3) - Pay Off - Passive Card
    public class Mod_BloodScent : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 3;
        public override string Name => "Blood Scent";
        public override string Description => $"On every crit, increases the speed by {amount*100}%";
        private float amount = 0.1f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            behaviour.OnCrit += () => behaviour.critChanges.Add(original => original + amount);
        }
    }
    
    // WRATH (Circle 4) - Power Escalation With Drawback - Passive Card
    public class Mod_Frenzy : Card, IManipulateSpeed, IManipulateHealth
    {
        public override int circleOfHell => 4;
        public override string Name => "Frenzy";
        public override string Description => $"Battle rage increases speed by {speedBoost*100}% but reduces maximum health by {healthReduction*100}%";
        private float speedBoost = 0.4f; // 40% speed boost
        private float healthReduction = 0.3f; // 30% health reduction
        
        public float ManipulateHealth(float value)
        {
            return value * (1 - healthReduction);
        }

        public float ManipulateSpeed(float value)
        {
            return value + speedBoost;
        }
    }
    
    // HERESY (Circle 5) - Introduce A Central Flaw without Any Benefit - Passive Card
    public class Mod_Distrust : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 5;
        public override string Name => "Distrust";
        public override string Description => $"Unit can not be healed";
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            behaviour.OnIGotHeal += (value, source) => behaviour.Damage(value, behaviour);
        }
    }
    
    // VIOLENCE (Circle 6) - Improved Active Abilities - Active Card
    public class Execution : ActiveCard
    {
        public override int circleOfHell => 6;
        public override string Name => "Execution";
        public override string Description => $"Brutal finishing move | Deals ({baseDamage} + {missingHealthMultiplier*100}% of target's missing health) damage every {cooldown}s";
        public override float cooldown => 9f;
        
        public float baseDamage = 7f;
        public float missingHealthMultiplier = 0.5f; // 50% of missing health as bonus damage
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var target = CombatManager.GetEnemies(activator)
                .MinBy(x => x.currentHealth / x.Unit.Health); // Target lowest % health enemy
                
            if (!target) return;
            
            Debug.Log($"{activator.Unit.name} uses {Name} on {target.Unit.name}");
            
            // Calculate missing health bonus
            float missingHealth = target.Unit.Health - target.currentHealth;
            float bonusDamage = missingHealth * missingHealthMultiplier;
            
            // Deal damage with bonus
            target.Damage(activator.PowerCalc * (baseDamage + bonusDamage), activator);
        }
    }
    
    // FRAUD (Circle 7) - Luck and Deception - Active Card
    public class Mod_EmptyThreat : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 7;
        public override string Name => "Empty Threat";
        public override string Description => $"First attack in combat has {triggerChance*100}% chance for +{critBonus*100}% crit chance";
        private float triggerChance = 0.25f; // 25% chance to activate
        private float critBonus = 0.35f; // 35% crit bonus when it works
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            if (behaviour == null) return;
            
            // Track if already used
            bool hasTriggered = false;
            
            // Subscribe to card activation
            behaviour.OnCardActivated += (unit, card) => {
                // Only for first attack
                if (hasTriggered) return;
                hasTriggered = true;
                
                // Roll for chance using unit-specific random
                float roll = behaviour.random.NextFloat();
                if (roll <= triggerChance)
                {
                    Debug.Log($"{behaviour.Unit.name}'s {Name} actually provides crit bonus!");
                    
                    // Add crit bonus for this attack
                    behaviour.critChanges.Add((original) => original + critBonus);
                    
                    // Remove bonus after a short delay
                    async void RemoveBonus()
                    {
                        await WebTask.Delay(0.5f);
                        behaviour.critChanges.RemoveAt(behaviour.critChanges.Count - 1);
                    }
                    RemoveBonus();
                }
                else
                {
                    Debug.Log($"{behaviour.Unit.name}'s {Name} fails to provide any benefit");
                }
            };
        }
    }
    
    // TREACHERY (Circle 8) - Self Curses Without Any Benefits - Passive Card
    public class Mod_DeathWish : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 8;
        public override string Name => "Death Wish";
        public override string Description => $"Takes {selfDamagePerAttack} damage after every ability use";
        private float selfDamagePerAttack = 5f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            if (behaviour == null) return;
            
            // Subscribe to card activation events
            behaviour.OnCardActivated += (unit, card) => {
                Debug.Log($"{behaviour.Unit.name}'s {Name} causes {selfDamagePerAttack} self-damage");
                behaviour.Damage(selfDamagePerAttack, behaviour);
            };
        }
    }
}