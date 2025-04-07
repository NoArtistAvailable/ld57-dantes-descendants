using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// WRATH
	public class Mod_Vengeful : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 4;
		public override string Name => "Vengeful";
		public override string Description => "Gain power when damaged (10% per hit, stacks up to 5 times)";
		private Dictionary<UnitCombatBehaviour, int> damageCounter = new Dictionary<UnitCombatBehaviour, int>();
		private float powerBoostPerHit = 0.1f; // 10% power boost per hit taken
		private int maxHitStacks = 5; // Maximum 50% power boost

		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			void OnDamaged(float damageAmount)
			{
				if (!damageCounter.ContainsKey(behaviour)) return;
			
				// Only increment if below max stacks
				if (damageCounter[behaviour] < maxHitStacks)
				{
					damageCounter[behaviour]++;
				
					// Add a new power boost for this stack
					behaviour.powerChanges.Add((original) => original * (1 + powerBoostPerHit));
				
					Debug.Log($"{behaviour.Unit.name} becomes more Vengeful! Power boost: {damageCounter[behaviour] * powerBoostPerHit * 100}%");
				}
			}
			
			// Initialize if not already tracked
			if (!damageCounter.ContainsKey(behaviour))
			{
				damageCounter[behaviour] = 0;
				behaviour.OnIGotHurt += OnDamaged;
			}
		}

		
	}

	public class Mod_Berserker : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 4;
		public override string Name => "Berserker";
		public override string Description => $"Attack speed increases as health decreases (up to {maxSpeedBoost*100}% faster at low health)";
		private float maxSpeedBoost = 1.0f; // 100% speed boost at minimum health

		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Add a speed calculation function based on current health
			behaviour.speedChanges.Add(CalculateSpeedBoost);
		}

		private float CalculateSpeedBoost(float original)
		{
			// Find the unit this calculation is for
			UnitCombatBehaviour unit = CombatManager.instance.playerSquad
				.FirstOrDefault(x => x.Unit.cards.Contains(this));
				
			if (unit == null) return original;
			
			// Calculate speed boost based on missing health percentage
			float healthPercent = unit.currentHealth / unit.Unit.Health;
			float boost = Mathf.Lerp(maxSpeedBoost, 0f, healthPercent);
			
			return original * (1f + boost);
		}
	}

	public class Mod_Fury : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 4;
		public override string Name => "Fury";
		public override string Description => "Critical hit multiplier increases as the battle progresses (up to 3.5x)";
		private float baseCritMultiplier = 2.0f; // Default crit multiplier is 2x
		private float maxCritMultiplier = 3.5f; // Maximum 3.5x crit multiplier
		private float rampUpTime = 30f; // Time in seconds to reach maximum effect
		private float combatStartTime;

		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Record combat start time
			combatStartTime = Time.time;
			
			// Add a crit multiplier calculation function
			behaviour.critChanges.Add(CalculateCritMultiplier);
		}

		private float CalculateCritMultiplier(float original)
		{
			// Calculate time factor (0 to 1) based on elapsed time
			float timeElapsed = Time.time - combatStartTime;
			float timeFactor = Mathf.Clamp01(timeElapsed / rampUpTime);
			
			// Interpolate between base and max crit multiplier
			float newMultiplier = Mathf.Lerp(baseCritMultiplier, maxCritMultiplier, timeFactor);
			
			// The existing "original" value might be the base crit chance rather than multiplier,
			// so this implementation may need adjustment based on how your crit system works
			return newMultiplier;
		}
	}

	public class Rampage : ActiveCard
	{
		public override int circleOfHell => 4;
		public override string Name => "Rampage";
		public override string Description => $"Unleash fury on all enemies | Deals ({damage}) damage to all and stuns for ({stunDuration})s every {cooldown}s";
		public override float cooldown => 7f;
		public float damage = 7f;
		public float stunDuration = 1.5f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var enemies = CombatManager.GetEnemies(activator);
			Debug.Log($"{activator.Unit.name} goes on a {Name} against everyone!");
			
			foreach(var enemy in enemies)
			{
				// Deal damage
				enemy.Damage(activator.PowerCalc * damage);
				
				// Apply stun effect
				float StunEffect(float original) => 0f; // Reduce speed to 0 (stun)
				enemy.speedChanges.Add(StunEffect);
				
				// Remove stun after duration
				async void RemoveStun()
				{
					await WebTask.Delay(stunDuration);
					enemy.speedChanges.Remove(StunEffect);
				}
				RemoveStun();
			}
		}
	}

	public class Vendetta : ActiveCard
	{
		public override int circleOfHell => 4;
		public override string Name => "Vendetta";
		public override string Description => $"Mark an enemy for vengeance | Target takes ({damageMult*100}%) extra damage from all allies for {markDuration}s every {cooldown}s";
		public override float cooldown => 6f;
		public float damageMult = 0.5f; // 50% extra damage
		public float markDuration = 10f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var target = CombatManager.GetHealthiestEnemy(activator);
			if (!target) return;
			
			Debug.Log($"{activator.Unit.name} marks {target.Unit.name} for {Name}!");
			
			// Create damage amplification function
			float VendettaEffect(float original) => original * (1 + damageMult);
			
			// Apply mark by adding to target's damage received calculation
			target.receiveDamageChanges.Add(VendettaEffect);
			// Remove mark after duration
			async void RemoveMark()
			{
				await WebTask.Delay(markDuration);
				target.receiveDamageChanges.Remove(VendettaEffect);
			}
			RemoveMark();
		}
	}
}