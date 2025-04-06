using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LD57
{
	public class DamageNumbers : MonoBehaviour
	{
		public TextMesh numberTemplate;
		public Color damageColor = Color.red;
		public Color healColor = Color.yellow;

		public Vector2 damageForce;
		public Vector2 healForce;

		private List<Rigidbody2D> spawnedBodies = new List<Rigidbody2D>();
		
		private void OnEnable()
		{
			UnitCombatBehaviour.OnHurt += SpawnDamageNumber;
			UnitCombatBehaviour.OnHeal += SpawnHealNumber;
		}
		private void OnDisable()
		{
			UnitCombatBehaviour.OnHurt -= SpawnDamageNumber;
			UnitCombatBehaviour.OnHeal -= SpawnHealNumber;
		}

		void Update()
		{
			for (int i = spawnedBodies.Count - 1; i >= 0; i--)
			{
				if (spawnedBodies[i].position.y <= 0)
				{
					spawnedBodies[i].Despawn();
					spawnedBodies.RemoveAt(i);
				}
			}
		}

		private void SpawnDamageNumber(UnitCombatBehaviour unit, float value)
		{
			SpawnNumber(unit, value, out var rb).color = damageColor;
			rb.AddForce(Vector2.up * damageForce.x, ForceMode2D.Impulse);
			rb.linearDamping = 4f;
			rb.AddTorque(Random.Range(-damageForce.y,damageForce.y), ForceMode2D.Impulse);
		}
		
		private void SpawnHealNumber(UnitCombatBehaviour unit, float value)
		{
			SpawnNumber(unit, value, out var rb).color = healColor;
			rb.AddForce(Vector2.up * healForce.x, ForceMode2D.Impulse);
			rb.linearDamping = 8f;
			rb.AddTorque(Random.Range(-healForce.y,healForce.y), ForceMode2D.Impulse);
		}

		private TextMesh SpawnNumber(UnitCombatBehaviour unit, float value, out Rigidbody2D rb)
		{
			value = Mathf.Round(value);
			var clone = numberTemplate.Spawn();
			clone.gameObject.SetActive(true);
			rb = clone.GetComponent<Rigidbody2D>();
			rb.transform.position = unit.transform.position + Vector3.up * 2f;
			rb.linearVelocity = Vector2.zero;
			rb.angularVelocity = 0f;
			spawnedBodies.Add(rb);
			clone.text = value.ToString();
			return clone;
		}

		
	}
}