using System;
using elZach.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LD57
{
	public class UnitInspectorBehaviour : MonoBehaviour
	{
		public static UnitInspectorBehaviour instance =>
			_instance.OrSet(ref _instance, FindAnyObjectByType<UnitInspectorBehaviour>);
		private static UnitInspectorBehaviour _instance;
		
		public Animatable anim;
		public TextMeshProUGUI unitNameField;
		public Transform cardContentParent;
		public CardBehaviour cardBehaviourPrefab;
		public Unit toShow { get; private set; }
		void Start()
		{
			UnitShopBehaviour.OnShowShopUnit += Show;
			UnitCombatBehaviour.OnShowCombatUnit += Show;
		}

		private void OnDestroy()
		{
			UnitShopBehaviour.OnShowShopUnit -= Show;
			UnitCombatBehaviour.OnShowCombatUnit -= Show;
		}

		private void Show(UnitShopBehaviour obj) => Show(obj.unit);

		private void Show(UnitCombatBehaviour obj)
		{
			// dont show if combat is over
			Show(obj.Unit);
		}

		public async void Show(Unit unit)
		{
			toShow = unit;
			await anim.Play(0);
			if (toShow != unit) return;
			unitNameField.text = $"{unit.name} <size=16> HP: {unit.Health} SPD: {unit.Speed*100}% POW: {unit.Power*100}% CRT: {unit.Crit * 100}% </size>" ;
			
			cardContentParent.ClearChildren();
			foreach (var card in unit.cards)
			{
				var clone = Instantiate(cardBehaviourPrefab, cardContentParent);
				clone.Init(card);
			}
			
			anim.PlayAt(1);
		}

		public async void Close()
		{
			toShow = null;
			await anim.Play(0);
			if (toShow != null) return;
			cardContentParent.ClearChildren();
		}
	}
}