using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// FRAUD
	public class Mod_Deceitful : Card, IManipulateCrit, IManipulatePower
	{
		public override int circleOfHell => 7;
		public override string Name => "Deceitful";
		public override string Description => $"Attacks have {critChance*100}% chance to critically strike, but deal {powerReduction*100}% less damage normally";
		private float critChance = 0.4f; // 40% crit chance
		private float powerReduction = 0.15f; // 15% less normal damage
		
		public float ManipulateCrit(float value)
		{
			return value + critChance;
		}
		public float ManipulatePower(float value)
		{
			return value * (1 - powerReduction);
		}
	}
	
	public class Mod_Duplicitous : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 7;
		public override string Name => "Duplicitous";
		public override string Description => $"Every third attack strikes twice";
		private Dictionary<UnitCombatBehaviour, int> attackCounter = new Dictionary<UnitCombatBehaviour, int>();
		
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Initialize attack counter for this unit
			if (!attackCounter.ContainsKey(behaviour))
			{
				attackCounter[behaviour] = 0;
				behaviour.OnCardActivated += OnCardUsed;
			}
		}
		
		private void OnCardUsed(UnitCombatBehaviour unit, ActiveCard card)
		{
			if (!attackCounter.ContainsKey(unit)) return;
			
			// Increment counter
			attackCounter[unit] = (attackCounter[unit] + 1) % 3;
			
			// On third attack, trigger double strike
			if (attackCounter[unit] == 0)
			{
				Debug.Log($"{unit.Unit.name}'s Duplicitous ability triggers a second strike!");
				// Delay the second activation slightly
				async void SecondStrike()
				{
					await WebTask.Delay(0.3f);
					card.Activate(unit);
				}
				SecondStrike();
			}
		}
	}
	
	public class Mod_Illusionist : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 7;
		public override string Name => "Illusionist";
		public override string Description => $"Has a {dodgeChance*100}% chance to completely avoid damage from any attack";
		private float dodgeChance = 0.15f; // 15% chance to avoid damage completely
		
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Add a damage reduction calculation that has a chance to completely nullify damage
			behaviour.receiveDamageChanges.Add((original) => {
				float roll = UnityEngine.Random.Range(0f, 1f);
				if (roll < dodgeChance)
				{
					Debug.Log($"{behaviour.Unit.name}'s Illusionist ability allows them to dodge an attack!");
					return 0f; // Complete dodge
				}
				return original;
			});
		}
	}
	
	public class Mod_Swindler : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 7;
		public override string Name => "Swindler";
		public override string Description => $"First attack in combat deals {firstHitBonus*100}% more damage and gains {speedBoost*100}% speed for the battle";
		private float firstHitBonus = 1.0f; // 100% more damage on first attack
		private float speedBoost = 0.25f; // 25% speed boost after first attack
		private Dictionary<UnitCombatBehaviour, bool> firstAttackDone = new Dictionary<UnitCombatBehaviour, bool>();
		
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Track first attack status
			firstAttackDone[behaviour] = false;
			
			behaviour.OnCardActivated += (unit, card) => {
				// Check if this is the first attack
				if (!firstAttackDone[unit])
				{
					// Apply temporary damage boost for first attack
					float FirstHitBoost(float original) => original * (1 + firstHitBonus);
					unit.powerChanges.Add(FirstHitBoost);
					
					// Mark first attack as done
					firstAttackDone[unit] = true;
					
					// Add permanent speed boost
					unit.speedChanges.Add((original) => original * (1 + speedBoost));
					
					// Remove the first hit bonus after a short delay
					async void RemoveFirstHitBonus()
					{
						await WebTask.Delay(0.5f);
						unit.powerChanges.Remove(FirstHitBoost);
					}
					RemoveFirstHitBonus();
					
					Debug.Log($"{unit.Unit.name}'s Swindler ability activates on first attack!");
				}
			};
		}
	}
	
	public class Deception : ActiveCard
	{
		public override int circleOfHell => 7;
		public override string Name => "Deception";
		public override string Description => $"Creates a false opening | Deals ({damage}) damage and steals {stealPercent*100}% of target's current power for {duration}s every {cooldown}s";
		public override float cooldown => 9f;
		public float damage = 8f;
		public float stealPercent = 0.4f; // 40% power steal
		public float duration = 6f;
		
		public override void Activate(UnitCombatBehaviour activator)
		{
			var target = CombatManager.GetHealthiestEnemy(activator);
			if (!target) return;
			
			Debug.Log($"{activator.Unit.name} uses {Name} on {target.Unit.name}");
			
			// Deal damage
			target.Damage(activator.PowerCalc * damage);
			
			// Calculate power transfer based on target's power
			float stolenPowerAmount = target.PowerCalc * stealPercent;
			
			// Create power reduction for target
			float PowerReduction(float original) => original * (1 - stealPercent);
			target.powerChanges.Add(PowerReduction);
			
			// Create power boost for self
			float PowerBoost(float original) => original + stolenPowerAmount;
			activator.powerChanges.Add(PowerBoost);
			
			// Remove effects after duration
			async void RemoveEffects()
			{
				await WebTask.Delay(duration);
				target.powerChanges.Remove(PowerReduction);
				activator.powerChanges.Remove(PowerBoost);
			}
			RemoveEffects();
		}
	}
}