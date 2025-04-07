using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
    // LIMBO (Circle 0) - Introduction - Active Card
    public class GentleReassurance : ActiveCard
    {
        public override int circleOfHell => 0;
        public override string Name => "Gentle Reassurance";
        public override string Description => $"Deals ({damage}) damage and heals friend for {heal} every {cooldown}s";
        public override float cooldown => 4f;
        
        public float damage = 4f;
        public float heal = 1f;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var target = CombatManager.GetHealthiestEnemy(activator);
            if (!target) return;
            
            Debug.Log($"{activator.Unit.name} uses {Name} on {target.Unit.name}");
            target.Damage(activator.PowerCalc * damage, activator);

            var friends = CombatManager.GetFriends(activator).Where(x=>x != activator).ToList();
            if (friends.Count == 0) return;
            friends.GetRandom().Heal(heal, activator);
        }
    }
    
    // LUST (Circle 1) - Meta Setup - Passive Card
    public class Mod_Nurturing : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 1;
        public override string Name => "Nurturing";
        public override string Description => $"Start Of Combat: Increases the speed of friends by {value*100}% for every circle of hell";
        private float value = 0.03f;
        public void OnCombatStart(UnitCombatBehaviour unitCombatBehaviour)
        {
            var friends = CombatManager.GetFriends(unitCombatBehaviour).Where(x=>x != unitCombatBehaviour);
            foreach (var friend in friends) friend.speedChanges.Add(val => val + value);
        }
    }
    
    // GLUTTONY (Circle 2) - Center Piece Ability - Active Card
    public class Rejuvenate : ActiveCard
    {
        public override int circleOfHell => 2;
        public override string Name => "Rejuvenate";
        public override string Description => $"Heals all allies for ({healAmount}) and increases their speed by {speedBoost*100}% for {boostDuration}s every {cooldown}s";
        public override float cooldown => 4f;
        
        public float healAmount = 2f;
        public float speedBoost = 0.2f;
        public float boostDuration = 3f;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var allies = CombatManager.GetFriends(activator);
            Debug.Log($"{activator.Unit.name} uses {Name} to rejuvenate all allies");
            
            foreach (var ally in allies)
            {
                // Apply healing
                ally.Heal(activator.PowerCalc * healAmount, activator);
                
                // Apply speed boost
                float SpeedBuff(float original) => original * (1 + speedBoost);
                ally.speedChanges.Add(SpeedBuff);
                
                // Remove speed boost after duration
                async void RemoveSpeedBoost()
                {
                    await WebTask.Delay(boostDuration);
                    ally.speedChanges.Remove(SpeedBuff);
                }
                RemoveSpeedBoost();
            }
        }
    }
    
    // GREED (Circle 3) - Pay Off - Passive Card
    public class Mod_Blessing : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 3;
        public override string Name => "Blessing";
        public override string Description => $"When allies get healed, increase their power and crit by {amount*100}%";
        private float amount = 0.05f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            foreach (var friend in CombatManager.GetFriends(behaviour).Where(x => x != behaviour))
            {
                friend.OnIGotHeal += (healedFor, source) =>
                {
                    friend.powerChanges.Add(original => original + amount);
                    friend.critChanges.Add(original => original + amount);
                };
            }
        }
    }
    
    // WRATH (Circle 4) - Power Escalation With Drawback - Passive Card
    public class Mod_Martyrdom : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 4;
        public override string Name => "Martyrdom";
        public override string Description => $"When an ally takes damage, you take {damageTransfer*100}% of it but ally's damage is reduced by {damageReduction*100}%";
        private float damageTransfer = 0.1f;
        private float damageReduction = 0.5f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            if (behaviour == null) return;
            
            var allies = CombatManager.GetFriends(behaviour).Where(a => a != behaviour).ToList();
            
            // Subscribe to all allies' damage events
            foreach (var ally in allies)
            {
                ally.OnIGotHurt += (damage, source) => {
                    if (behaviour.currentHealth <= 0) return;
                    
                    // Transfer portion of damage to self
                    float transferredDamage = damage * damageTransfer;
                    Debug.Log($"{behaviour.Unit.name}'s {Name} transfers {transferredDamage} damage from {ally.Unit.name}");
                    
                    behaviour.Damage(transferredDamage, behaviour);
                };
                
                // Reduce damage to allies
                ally.receiveDamageChanges.Add((original) => original * (1 - damageReduction));
            }
        }
    }
    
    // HERESY (Circle 5) - Introduce A Central Flaw without Any Benefit - Passive Card
    public class Mod_Dependent : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 5;
        public override string Name => "Dependent";
        public override string Description => $"Loses {powerLossPerDeath*100}% power for each dead ally";
        private float powerLossPerDeath = 0.3f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            foreach (var friend in CombatManager.GetFriends(behaviour))
            {
                friend.OnIDied += () => behaviour.powerChanges.Add(original => original * (1 - powerLossPerDeath));
            }
        }
    }
    
    // VIOLENCE (Circle 6) - Improved Active Abilities - Active Card
    public class ForcefulSuggestion : ActiveCard
    {
        public override int circleOfHell => 6;
        public override string Name => "Forceful Suggestion";
        public override string Description => $"Immediately activates another friend every {cooldown}s";
        public override float cooldown => 3f;
        
        public override void Activate(UnitCombatBehaviour activator)
        {
            var friends = CombatManager.GetFriends(activator).Where(x => x != activator).ToList();
            if (friends.Count == 0) return;
            var target = friends.GetRandom();
            target.chargeTime = target.NextActiveCard.cooldown - 0.1f;
        }
    }
    
    // FRAUD (Circle 7) - Luck and Deception - Active Card
    public class Mod_FalseHope : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 7;
        public override string Name => "False Hope";
        public override string Description => $"{triggerChance*100}% chance to heal self for {healAmount} when falling below {healthThreshold*100}% health";
        private float triggerChance = 0.2f; // 20% chance to activate
        private float healAmount = 5f; // Small heal amount
        private float healthThreshold = 0.3f; // Only triggers when nearly dead
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            if (behaviour == null) return;
            
            // Track if already triggered
            bool hasTriggered = false;
            
            // Subscribe to damage events
            behaviour.OnIGotHurt += (damage, source) => {
                // Check if already triggered
                if (hasTriggered) return;
                
                // Check health threshold
                float healthPercent = behaviour.currentHealth / behaviour.Unit.Health;
                if (healthPercent >= healthThreshold) return;
                
                // Roll for chance to activate using unit-specific random
                float roll = behaviour.random.NextFloat();
                if (roll <= triggerChance)
                {
                    Debug.Log($"{behaviour.Unit.name}'s {Name} actually provides some healing!");
                    behaviour.Heal(healAmount, behaviour);
                    hasTriggered = true;
                }
                else
                {
                    Debug.Log($"{behaviour.Unit.name}'s {Name} fails to provide any healing");
                    hasTriggered = true; // Still used up even if it fails
                }
            };
        }
    }
    
    // TREACHERY (Circle 8) - Self Curses Without Any Benefits - Passive Card
    public class Mod_CombinedFate : Card, IInitializeOnCombat
    {
        public override int circleOfHell => 8;
        public override string Name => "Combined Fate";
        public override string Description => $"When an ally dies, lose {healthLossPercentage*100}% of current health";
        private float healthLossPercentage = 1f;
        
        public void OnCombatStart(UnitCombatBehaviour behaviour)
        {
            foreach (var friend in CombatManager.GetFriends(behaviour))
                friend.OnIDied += () => behaviour.Damage(behaviour.currentHealth, behaviour);
        }
    }
}