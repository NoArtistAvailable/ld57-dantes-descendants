using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
    // LIMBO (Circle 0) - Introduction - Active Card
    public class ThickSkinned : ActiveCard, IManipulateHealth
    {
        public override int circleOfHell => 0;
        public override string Name => "Thick-Skinned";
        public override string Description => $"Increases health by {healthIncrease*100}% and deals ({damage}) damage every {cooldown}s";
        public override float cooldown => 5f;
        
        public float damage = 5f;
        public float healthIncrease = 0.3f;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var target = CombatManager.GetHealthiestEnemy(activator);
            if (!target) return;
            
            Debug.Log($"{activator.Unit.name} uses {Name} on {target.Unit.name}");
            target.Damage(activator.PowerCalc * damage, activator);
        }

        public float ManipulateHealth(float value)
        {
            return value * (1 + healthIncrease);
        }
    }
    
    // LUST (Circle 1) - Meta Setup - Passive Card
    public class Mod_Thick : Card, IManipulateHealth
    {
        public override int circleOfHell => 1;
        public override string Name => "Heavy Built";
        public override string Description => $"Increases maximum health by {healthBoost*100}% for every circle of hell";
        private float healthBoost = 0.1f; // 30% health boost
        
        public float ManipulateHealth(float value)
        {
            return value * (1 + healthBoost * PlayerManager.instance.circleOfHell);
        }
    }
    
    // GLUTTONY (Circle 2) - Center Piece Ability - Active Card
    public class BlameDeflection : ActiveCard
    {
        public override int circleOfHell => 2;
        public override string Name => "Blame Deflection";
        public override string Description => $"Deals {healthScaling*100}% of max health as damage every {cooldown}s";
        public override float cooldown => 6f;
        public float healthScaling = 0.3f;

        public override void Activate(UnitCombatBehaviour activator)
        {
            var target = CombatManager.GetHealthiestEnemy(activator);
            if (!target) return;
            
            Debug.Log($"{activator.Unit.name} uses {Name} on {target.Unit.name}");
            
            float damageAmount = activator.Unit.Health * healthScaling;
            target.Damage(activator.PowerCalc * damageAmount, activator);
        }
    }
    
    // GREED (Circle 3) - Pay Off - Passive Card
    public class Mod_Fortification : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 3;
        public override string Name => "Growing Stubbornness";
        public override string Description => $"When damaged increase speed and power by {amount*100}%";
        private float amount = 0.05f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            behaviour.OnIGotHurt += (damage, source) =>
            {
                behaviour.powerChanges.Add(original => original + amount);
                behaviour.speedChanges.Add(original => original + amount);
            };
        }
    }
    
    // WRATH (Circle 4) - Power Escalation With Drawback - Passive Card
    public class Mod_Retribution : Card, IInitializeOnCombat, IManipulateHealth
    {
        public override int circleOfHell => 4;
        public override string Name => "Retribution";
        public override string Description => $"Reflects {reflectPercent*100}% of damage taken back to attackers, but reduces maximum health by {healthReduction*100}%";
        private float reflectPercent = 0.3f; // 30% damage reflection
        private float healthReduction = 0.2f; // 20% health reduction
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            async void ReflectDamage(float damage, UnitCombatBehaviour source)
            {
                if (source == behaviour) return;    // dont trigger on self damage, other wise we might loop infinitely
                await WebTask.Delay(0.1f);
                source.Damage(damage * reflectPercent, source);
            }
            // Set up damage reflection
            behaviour.OnIGotHurt += ReflectDamage;
        }

        public float ManipulateHealth(float value)
        {
            return value * (1 - healthReduction);
        }
    }
    
    // HERESY (Circle 5) - Introduce A Central Flaw without Any Benefit - Passive Card
    public class Mod_StoneSkin : Card, IManipulateSpeed
    {
        public override int circleOfHell => 5;
        public override string Name => "Stubborn Slowness";
        public override string Description => $"Reduces speed by {speedReduction*100}%";
        private float speedReduction = 0.35f; // 35% speed reduction

        public float ManipulateSpeed(float value)
        {
            return value - speedReduction;
        }
    }
    
    // VIOLENCE (Circle 6) - Improved Active Abilities - Active Card
    public class VerbalExplosion : ActiveCard
    {
        public override int circleOfHell => 6;
        public override string Name => "Verbal Explosion";
        public override string Description => $"Deals {healthPercent*100}% of current health to all enemies every {cooldown}s";
        public override float cooldown => 10f;
        public float healthPercent = 0.5f;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            foreach (var enemy in CombatManager.GetEnemies(activator))
            {
                enemy.Damage(activator.currentHealth*healthPercent, activator);
            }
        }
    }
    
    // FRAUD (Circle 7) - Borderline useless passive cards
    public class Mod_EmptyPromise : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 7;
        public override string Name => "Empty Promise";
        public override string Description => $"{triggerChance*100}% chance to reduce damage by {damageReduction*100}% on attacks over {damageThreshold} damage";
        private float triggerChance = 0.15f; // 15% chance to activate
        private float damageReduction = 0.2f; // 20% damage reduction when it works
        private float damageThreshold = 12f; // Only works on large hits
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            if (behaviour == null) return;
            
            // Add a highly conditional damage reduction
            behaviour.receiveDamageChanges.Add((original) => {
                // Only works on high damage attacks
                if (original < damageThreshold) return original;
                
                // Small chance to actually work
                float roll = behaviour.random.NextFloat();
                if (roll <= triggerChance)
                {
                    Debug.Log($"{behaviour.Unit.name}'s {Name} actually works for once!");
                    return original * (1 - damageReduction);
                }
                
                Debug.Log($"{behaviour.Unit.name}'s {Name} fails to deliver any protection");
                return original;
            });
        }
    }
    
    // TREACHERY (Circle 8) - Self Curses Without Any Benefits - Passive Card
    public class Mod_Vulnerable : Card, IManipulateHealth
    {
        public override int circleOfHell => 8;
        public override string Name => "Vulnerable";
        public override string Description => $"Reduces Health to a third";

        public float ManipulateHealth(float value)
        {
            return value * 0.3f;
        }
    }
}