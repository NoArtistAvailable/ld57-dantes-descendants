using System;
using elZach.Common;
using TMPro;
using UnityEngine;

namespace LD57
{
	public class IntroductionTitle : MonoBehaviour
	{
		public TextMeshProUGUI textField;
		public GameObject unitInspector;

		private async void Start()
		{
			await WebTask.Delay(1f);
			textField.text = $"In the circle of <color=#905>{PlayerManager.CircleNames[PlayerManager.instance.circleOfHell]}</color>\nyou meet <color=#905>{CombatManager.setEnemySquad[0].name}</color>";
			GetComponent<Animatable>()?.PlayAt(1);
			await WebTask.Delay(0.6f);
			unitInspector.gameObject.SetActive(true);
		}
	}
}