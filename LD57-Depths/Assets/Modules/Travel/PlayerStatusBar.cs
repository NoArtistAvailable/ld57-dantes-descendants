using TMPro;
using UnityEngine;

namespace LD57
{
	public class PlayerStatusBar : MonoBehaviour
	{
		public TextMeshProUGUI textField;

		void Start()
		{
			textField.text = $"Life:{PlayerManager.instance.Lives} | Wins: {PlayerManager.instance.Wins}";
		}
	}
}