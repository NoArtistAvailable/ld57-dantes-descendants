using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// GREED
	public class Mod_Miser : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 3;
		public override string Name => "Miser";
		public override string Description => "Conserves energy and strikes harder every third attack (300%)";
		private Dictionary<UnitCombatBehaviour, int> attackCounter = new Dictionary<UnitCombatBehaviour, int>();
		private float powerBoost = 3f; // 80% power boost on third attacks

		// Called when the combat behavior is set up
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Initialize if not already tracked
			if (!attackCounter.ContainsKey(behaviour))
			{
				attackCounter[behaviour] = 0;
				behaviour.OnCardActivated += OnCardUsed;
				
				// Add a power calculation function that checks the counter
				behaviour.powerChanges.Add(CalculatePowerBoost);
			}
		}

		private float CalculatePowerBoost(float original)
		{
			// Find the unit this calculation is for
			UnitCombatBehaviour unit = attackCounter.Keys.FirstOrDefault();
			if (unit == null) return original;
			
			// Check if this is a boosted attack
			if (attackCounter[unit] == 2) // Third attack (0-indexed)
			{
				return original * (1 + powerBoost);
			}
			
			return original;
		}

		private void OnCardUsed(UnitCombatBehaviour unit, ActiveCard card)
		{
			if (!attackCounter.ContainsKey(unit)) return;
			
			// Increment counter and reset after third attack
			attackCounter[unit] = (attackCounter[unit] + 1) % 3;
		}
	}

	public class Mod_Hoarder : Card, IManipulateHealth, IInitializeOnCombat
	{
		public override int circleOfHell => 3;
		public override string Name => "Hoarder";
		public override string Description => "Starts with 25% more health but 15% less power. Each ability use increases power by 5% (stacks up to 5 times)";
		private Dictionary<UnitCombatBehaviour, int> useCounter = new Dictionary<UnitCombatBehaviour, int>();
		private float healthBoost = 0.25f; // 25% health boost
		private float initialPowerReduction = 0.15f; // 15% power reduction
		private float powerBoostPerUse = 0.05f; // 5% power boost per ability use
		private int maxUseStacks = 5;
		private IInitializeOnCombat _initializeOnCombatImplementation;

		public float ManipulateHealth(float value)
		{
			return value * (1 + healthBoost);
		}

		// This function will be called when the card is added to a unit
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;

			// Initialize tracking
			if (!useCounter.ContainsKey(behaviour))
			{
				useCounter[behaviour] = 0;
				
				// Apply initial power reduction
				behaviour.powerChanges.Add((original) => original * (1 - initialPowerReduction));
				
				// Subscribe to card activation event
				behaviour.OnCardActivated += OnCardUsed;
			}
		}

		private void OnCardUsed(UnitCombatBehaviour unit, ActiveCard card)
		{
			if (!useCounter.ContainsKey(unit)) return;
			
			// Only increment if below max stacks
			if (useCounter[unit] < maxUseStacks)
			{
				useCounter[unit]++;
				
				// Add a new power boost for this stack
				unit.powerChanges.Add((original) => original * (1 + powerBoostPerUse));
			}
		}
	}
	
	public class Mod_GoldFever : Card
	{
		public override int circleOfHell => 3;
		public override string Name => "Gold Fever";
		public override string Description => "Speed increases with each ability use, up to 75% faster (stacks 5% per use, resets after combat)";
		private Dictionary<UnitCombatBehaviour, int> useCounter = new Dictionary<UnitCombatBehaviour, int>();
		private float speedBoostPerUse = 0.05f; // 5% speed boost per use
		private int maxUseStacks = 15; // Up to 75% faster

		public void OnAddToCombat(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Initialize tracking
			if (!useCounter.ContainsKey(behaviour))
			{
				useCounter[behaviour] = 0;
				behaviour.OnCardActivated += OnCardUsed;
			}
		}

		private void OnCardUsed(UnitCombatBehaviour unit, ActiveCard card)
		{
			if (!useCounter.ContainsKey(unit)) return;
			
			// Increment counter up to max stacks
			if (useCounter[unit] < maxUseStacks)
			{
				useCounter[unit]++;
				
				// Add a new speed boost for this stack
				unit.speedChanges.Add((original) => original * (1 + speedBoostPerUse));
			}
		}
	}

	public class Extort : ActiveCard
	{
		public override int circleOfHell => 3;
		public override string Name => "Extort";
		public override string Description => $"Demand payment or face consequences | Deals ({damage}) damage and steals ({speedDrain*100}%) speed for {drainDuration}s every {cooldown}s";
		public override float cooldown => 4f;
		public float damage = 6f;
		public float speedDrain = 0.3f; // 30% speed drain
		public float drainDuration = 3f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var target = CombatManager.GetHealthiestEnemy(activator);
			if (!target) return;
			
			Debug.Log($"{activator.Unit.name} {Name}s from {target.Unit.name}");
			
			// Deal damage
			target.Damage(activator.PowerCalc * damage);
			
			// Apply speed drain
			float SpeedDebuff(float original) => original * (1 - speedDrain);
			target.speedChanges.Add(SpeedDebuff);
			
			// Add temporary speed boost to self from stolen speed
			float SpeedBuff(float original) => original * (1 + speedDrain/2);
			activator.speedChanges.Add(SpeedBuff);
			
			// Remove effects after duration
			async void RemoveEffects()
			{
				await WebTask.Delay(drainDuration);
				target.speedChanges.Remove(SpeedDebuff);
				activator.speedChanges.Remove(SpeedBuff);
			}
			RemoveEffects();
		}
	}
	
	public class Hoard : ActiveCard
	{
		public override int circleOfHell => 3;
		public override string Name => "Hoard";
		public override string Description => $"Stockpile resources selfishly | Grants ({damageBoost*100}%) damage boost to self every {cooldown}s";
		public override float cooldown => 6f;
		public float damageBoost = 1f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			Debug.Log($"{activator.Unit.name} {Name}s power");
			float PowerBoost(float original) => original * (1 + damageBoost);
			activator.powerChanges.Add(PowerBoost);
		}
	}
	
	public class TaxCollector : ActiveCard
	{
		public override int circleOfHell => 3;
		public override string Name => "Tax Collector";
		public override string Description => $"Collect tax from all enemies | Deals ({damagePerEnemy}) damage to each enemy and increases allies' power by ({powerBoost*100}%) for {boostDuration}s every {cooldown}s";
		public override float cooldown => 7f;
		public float damagePerEnemy = 4f;
		public float powerBoost = 0.2f; // 20% power boost to allies
		public float boostDuration = 3f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			// Target all enemies
			var enemies = CombatManager.GetEnemies(activator);
			var friends = CombatManager.GetFriends(activator);
			
			Debug.Log($"{activator.Unit.name} collects {Name} from all enemies");
			
			// Deal damage to all enemies
			foreach (var enemy in enemies)
			{
				enemy.Damage(activator.PowerCalc * damagePerEnemy);
			}
			
			// Apply power boost to all allies
			Dictionary<UnitCombatBehaviour, Func<float, float>> boostFunctions = 
				new Dictionary<UnitCombatBehaviour, Func<float, float>>();
			
			foreach (var friend in friends)
			{
				// Create unique boost function for each ally
				float BoostFunction(float original) => original * (1 + powerBoost);
				boostFunctions[friend] = BoostFunction;
				
				// Apply boost
				friend.powerChanges.Add(boostFunctions[friend]);
			}
			
			// Remove boost after duration
			async void RemoveBoosts()
			{
				await WebTask.Delay(boostDuration);
				foreach (var pair in boostFunctions)
				{
					pair.Key.powerChanges.Remove(pair.Value);
				}
			}
			RemoveBoosts();
		}
	}
	
	public class GoldenTouch : ActiveCard
	{
		public override int circleOfHell => 3;
		public override string Name => "Golden Touch";
		public override string Description => $"Turn enemies to gold | Deals ({damage}) damage and freezes target for {freezeDuration}s every {cooldown}s";
		public override float cooldown => 6f;
		public float damage = 9f;
		public float freezeDuration = 2f;
		
		public override void Activate(UnitCombatBehaviour activator)
		{
			var target = CombatManager.GetRandomEnemy(activator);
			if (!target) return;
			
			Debug.Log($"{activator.Unit.name} uses {Name} on {target.Unit.name}");
			
			// Deal damage
			target.Damage(activator.PowerCalc * damage);
			
			// Create a freeze function that reduces speed to 0
			float FreezeEffect(float original)
			{
				return 0f; // Freeze completely
			}
			
			// Apply freeze
			target.speedChanges.Add(FreezeEffect);
			
			// Remove freeze after duration
			async void RemoveFreeze()
			{
				await WebTask.Delay(freezeDuration);
				target.speedChanges.Remove(FreezeEffect);
			}
			RemoveFreeze();
		}
	}
}