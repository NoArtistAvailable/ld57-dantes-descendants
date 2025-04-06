using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// LIMBO
	public class Shout : ActiveCard
	{
		public override int circleOfHell => 0;
		public override string Name => "Shout";
		public override string Description => $"Angrily blames someone every {cooldown} seconds. ({damage})";
		public override float cooldown => 6;

		public float damage = 6;

		public override void Activate(UnitCombatBehaviour activator)
		{
			// Get A Random Enemy
			var target = CombatManager.GetHealthiestEnemy(activator);
			if (!target) return;
			Debug.Log($"{activator.Unit.name} {Name}s at {target.Unit.name}");
			target.Damage(activator.PowerCalc * damage);
		}
	}
	public class Annoy : ActiveCard
	{
		public override int circleOfHell => 0;
		public override string Name => "Annoy";
		public override string Description => $"Annoys someone every {cooldown} seconds. ({damage})";
		public override float cooldown => 2;

		public float damage = 2;

		public override void Activate(UnitCombatBehaviour activator)
		{
			// Get A Random Enemy
			var target = CombatManager.GetHealthiestEnemy(activator);
			if (!target) return;
			Debug.Log($"{activator.Unit.name} {Name}s at {target.Unit.name}");
			target.Damage(activator.PowerCalc * damage);
		}
	}
	public class Scream : ActiveCard
	{
		public override int circleOfHell => 0;
		public override string Name => "Scream";
		public override string Description => $"Screams at every opponent every {cooldown} seconds. ({damage})";
		public override float cooldown => 4;

		public float damage = 1;

		public override void Activate(UnitCombatBehaviour activator)
		{
			// Get A Random Enemy
			var targets = CombatManager.GetEnemies(activator);
			Debug.Log($"{activator.Unit.name} {Name}s at everyone");
			foreach(var target in targets) target.Damage(activator.PowerCalc * damage);
		}
	}
	public class Exploit : ActiveCard
	{
		public override int circleOfHell => 0;
		public override string Name => "Exploit";
		public override string Description => $"Attempts to exploit a character flaw every {cooldown} seconds. Damage: ({damage}) Crit: ({critChance*100f}%)";
		public override float cooldown => 5f;

		public float damage = 4;
		public float critChance = 0.25f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var target = CombatManager.GetHealthiestEnemy(activator);
			if (!target) return;
			Debug.Log($"{activator.Unit.name} {Name}s at {target.Unit.name}");
			target.Damage(activator.PowerCalc * damage);
		}
	}
}