using UnityEngine;

namespace LD57
{
	public class Shout : ActiveCard
	{
		public override int circleOfHell => 0;
		public override string Name => "Shout";
		public override string Description => $"Angrily blames someone every {cooldown} seconds. ({damage})";
		public override float cooldown => 3;

		public float damage = 6;

		public override void Activate(Unit activator)
		{
			// Get A Random Enemy
			Debug.Log("Shouting");
		}
	}
	public class Annoy : ActiveCard
	{
		public override int circleOfHell => 0;
		public override string Name => "Annoy";
		public override string Description => $"Annoys someone every {cooldown} seconds. ({damage})";
		public override float cooldown => 1;

		public float damage = 2;

		public override void Activate(Unit activator)
		{
			// Get A Random Enemy
			Debug.Log("Annoys");
		}
	}
	public class Scream : ActiveCard
	{
		public override int circleOfHell => 0;
		public override string Name => "Scream";
		public override string Description => $"Screams at every opponent every {cooldown} seconds. ({damage})";
		public override float cooldown => 2;

		public float damage = 1;

		public override void Activate(Unit activator)
		{
			// Get A Random Enemy
			Debug.Log("Screams");
		}
	}
	public class Exploit : ActiveCard
	{
		public override int circleOfHell => 0;
		public override string Name => "Exploit";
		public override string Description => $"Attempts to exploit a character flaw every {cooldown} seconds. Damage: ({damage}) Crit: ({critChance*100f}%)";
		public override float cooldown => 2.5f;

		public float damage = 4;
		public float critChance = 0.25f;

		public override void Activate(Unit activator)
		{
			// Get A Random Enemy
			Debug.Log("Exploit");
		}
	}
}