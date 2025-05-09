﻿using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LD57
{
	public class CombatManager : MonoBehaviour
	{
		public static CombatManager instance => _instance.OrSet(ref _instance, FindAnyObjectByType<CombatManager>);
		private static CombatManager _instance;

		public static bool Active = false;
		
		public static List<Unit> setPlayerSquad { get; set; }
		public static List<Unit> setEnemySquad { get; set; }
		public static int setCircle { get; set; } = -1;

		public Transform playerSquadParent, enemySquadParent;
		public UnitCombatBehaviour[] playerSquad { get; set; }
		public UnitCombatBehaviour[] enemySquad { get; set; }

		public int testCircle = 0;
		public Button startButton, endButton;

		public static IEnumerable<UnitCombatBehaviour> GetEnemies(UnitCombatBehaviour unit) =>
			unit.isPlayerSquad ? instance.enemySquad.Where(x=>x.currentHealth > 0) : instance.playerSquad.Where(x=>x.currentHealth > 0);
		public static IEnumerable<UnitCombatBehaviour> GetFriends(UnitCombatBehaviour unit) =>
			unit.isPlayerSquad ? instance.playerSquad.Where(x=>x.currentHealth > 0) : instance.enemySquad.Where(x=>x.currentHealth > 0);

		public static UnitCombatBehaviour GetRandomEnemy(UnitCombatBehaviour unit)
		{
			var targets = GetEnemies(unit).ToList();
			if (targets.Count == 0) return null;
			var target = targets.GetRandom();
			return target;
		}
		public static UnitCombatBehaviour GetHealthiestEnemy(UnitCombatBehaviour unit)
		{
			try
			{
				return GetEnemies(unit).MaxBy(x => x.currentHealth);
			}
			catch (Exception e)
			{
				return null;
			}
		}
		
		void Start()
		{
			Active = false;
			InitCombat();
			startButton.onClick.AddListener(() => Active = true);
			endButton.onClick.AddListener(LevelManager.instance.LoadNext);
			UnitCombatBehaviour.OnDeath += OnUnitDied;
		}

		private void OnDestroy()
		{
			UnitCombatBehaviour.OnDeath -= OnUnitDied;
		}

		private void OnUnitDied(UnitCombatBehaviour unitBehaviour)
		{
			var playerDied = playerSquad.All(x => x.currentHealth <= 0);
			var enemyDied = enemySquad.All(x => x.currentHealth <= 0);
			if (playerDied)
			{
				PlayerManager.instance.Lives--;
			}

			if (enemyDied)
			{
				PlayerManager.instance.Wins++;
			}
			if (playerDied || enemyDied) EndCombat();
		}

		public void EndCombat()
		{
			Active = false;
			if (PlayerManager.instance.squad == null || PlayerManager.instance.squad.Count == 0)
			{
				PlayerManager.instance.squad = setPlayerSquad;
				PlayerManager.instance.playerUnit = setPlayerSquad[0];
			}

			if (PlayerManager.instance.circleOfHell == 8)
			{
				GameOverScreen.CallGameWon();
				return;
			}

			if (PlayerManager.instance.Lives == 0)
			{
				GameOverScreen.CallGameOver();
				return;
			}
			
			PlayerManager.instance.circleOfHell++;
			endButton.GetComponent<Animatable>().PlayAt(1);
			
		}
		
		public void InitCombat()
		{
			if (setCircle < 0) setCircle = testCircle;
			if (setPlayerSquad == null) setPlayerSquad = CharacterCreator.GetSquadAtCircle(setCircle);
			if (setEnemySquad == null) setEnemySquad = CharacterCreator.GetSquadAtCircle(setCircle);

			playerSquad = playerSquadParent.GetComponentsInChildren<UnitCombatBehaviour>();
			enemySquad = enemySquadParent.GetComponentsInChildren<UnitCombatBehaviour>();

			for (int i = 0; i < playerSquad.Length; i++)
			{
				playerSquad[i].Init(setPlayerSquad[i]);
				playerSquad[i].chargeTime = 0.3f - i * 0.1f;
			}

			for (int i = 0; i < enemySquad.Length; i++)
			{
				enemySquad[i].Init(setEnemySquad[i]);
				enemySquad[i].chargeTime = 0.3f - i * 0.1f;
			}
		}
		#if UNITY_EDITOR
		[CustomEditor(typeof(CombatManager))]
		public class Inspector : Editor
		{
			public override bool RequiresConstantRepaint() => true;

			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();
				var t = target as CombatManager;
				// EditorGUILayout.LabelField("Player Squad");
				// if (t.playerSquad == null) return;
				// foreach (var unitBehaviour in t.playerSquad)
				// {
				// 	EditorGUILayout.PropertyField(
				// 		new SerializedObject(unitBehaviour).FindProperty(nameof(UnitCombatBehaviour.unit)));
				// }
				// EditorGUILayout.LabelField("Enemy Squad");
				// foreach (var unitBehaviour in t.enemySquad)
				// {
				// 	EditorGUILayout.PropertyField(
				// 		new SerializedObject(unitBehaviour).FindProperty(nameof(UnitCombatBehaviour.unit)));
				// }
			}
		}		
		#endif
	}
}