using System;
using System.Threading.Tasks;
using elZach.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LD57
{
	public class GameOverScreen : MonoBehaviour
	{
		public static GameOverScreen instance => _instance.OrSetToActive(ref _instance, FindAnyObjectByType<GameOverScreen>);
		private static GameOverScreen _instance;

		public GameObject gameOverPanel, gameWonPanel;
		public TextMeshProUGUI gameOverText, gameWonText;
		public Button restartButton1, restartButton2;

		public SceneReference restartScene;
		
		private void Start()
		{
			restartButton1.onClick.AddListener(() =>
			{
				PlayerManager.Reset();
				LevelManager.instance.LoadScene(restartScene);
			});
			restartButton2.onClick.AddListener(() =>
			{
				PlayerManager.Reset();
				LevelManager.instance.LoadScene(restartScene);
			});
		}

		public static async void CallGameOver()
		{
			instance.gameOverPanel.SetActive(true);
			instance.gameOverText.text = $"After {PlayerManager.instance.circleOfHell+1} glorious battles\nyou succumbed to {PlayerManager.CircleNames[PlayerManager.instance.circleOfHell]}";
			var image = instance.gameOverPanel.GetComponentInChildren<Image>();
			float progress = 0f;
			while (progress <= 1f)
			{
				var mapped = progress.Remap(0f, 1f, 1f, 0.2f);
				image.material.SetFloat("_Progress", mapped);
				progress += Time.deltaTime;
				await Task.Yield();
			}
			instance.gameOverPanel.GetComponent<AnimatableChildren>().PlayAt(1);
		}
		
		public static async void CallGameWon()
		{
			instance.gameWonPanel.SetActive(true);
			instance.gameWonText.text = $"After {PlayerManager.instance.circleOfHell+1} glorious battles\nyou finally prevailed!";
			var image = instance.gameWonPanel.GetComponentInChildren<Image>();
			float progress = 0f;
			while (progress <= 1f)
			{
				var mapped = progress.Remap(0f, 1f, 1f, 0.2f);
				image.material.SetFloat("_Progress", mapped);
				progress += Time.deltaTime;
				await Task.Yield();
			}
			instance.gameWonPanel.GetComponent<AnimatableChildren>().PlayAt(1);
		}

	}
}