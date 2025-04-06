using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LD57
{
	public class CharacterCreator : MonoBehaviour
	{
		public PaintingCanvas painter;
		public TMP_InputField nameInput;
		public Button exitButton;
		#if UNITY_EDITOR
		public bool dontUpload = false;		
		#endif

		void Start()
		{
			exitButton.onClick.AddListener(UploadCharacter);
			exitButton.interactable = false;
			nameInput.onValueChanged.AddListener((value) =>
			{
				exitButton.interactable = !string.IsNullOrEmpty(value);
			});
			GetOnlineDataIfNecessary();
		}

		private void OnDestroy()
		{
			OnlineManager.onGotSinners -= GetOnlineData;
		}

		public static void GetOnlineDataIfNecessary()
		{
			if (unitDataBase == null)
			{
				OnlineManager.onGotSinners += GetOnlineData;
				OnlineManager.GetSinnersAsync();
			}
		}
		private static void GetOnlineData(List<OnlineManager.SinnerData> entries)
		{
			Debug.Log($"Got {entries.Count} entries");
			unitDataBase = new List<Unit>();
			foreach (var data in entries)
			{
				var unit = new Unit(data.name, data.seed)
				{
					faceTexture = PaintingCanvas.DeserializeTextureFromBase64(data.imageBase64)
				};
				unitDataBase.Add(unit);
			}
			unusedPlayerUnits = new List<Unit>();
			unusedPlayerUnits.AddRange(unitDataBase);
		}

		private void UploadCharacter()
		{
			PlayerManager.instance.playerUnit = new Unit(nameInput.text, Random.Range(int.MinValue, int.MaxValue));
			PlayerManager.instance.playerUnit.faceTexture = painter.tex;
			
			var base64Tex = PaintingCanvas.SerializeTextureToBase64(PlayerManager.instance.playerUnit.faceTexture);
			var data = new OnlineManager.SinnerData()
			{
				name = PlayerManager.instance.playerUnit.name,
				seed = PlayerManager.instance.playerUnit.seed,
				imageBase64 = base64Tex,
				score = OnlineManager.DateToScore()
			};
#if UNITY_EDITOR
			if (!dontUpload)			
#endif
			OnlineManager.PostSinnerAsync(data);
			
			LevelManager.instance.LoadNext();
		}

		public static string[] RandomNames =
		{
			"Alessandro", "Bartolomeo", "Cesare", "Domenico", "Elia", "Fabiano", "Giorgio", "Hieronymus", "Innocenzo",
			"Jacopo", "Lorenzo", "Marcello", "Niccolò", "Ottaviano", "Pietro", "Quintiliano", "Raffaele", "Salvatore",
			"Tommaso", "Ugolino", "Valerio", "Zanobi", "Adelmo", "Benedetto", "Corrado", "Ermanno", "Filippo", "Guido",
			"Leonardo", "Taddeo", "Achille", "Amadeo", "Anselmo", "Baldassare", "Celestino", "Daniele", "Ettore",
			"Fortunato", "Gaspare", "Immanuel", "Isidoro", "Lazzaro", "Manfredi", "Nerio", "Onofrio", "Pasquale",
			"Raniero", "Samuele", "Teodoro", "Urbano", "Vincenzo", "Zaccaria", "Agostino", "Berardo", "Costanzo",
			"Enrico", "Fiorenzo", "Gregorio", "Ippolito", "Luciano"
		};

		public static Texture2D[] RandomFaces => _randomFaces ??= Resources.LoadAll<Texture2D>("Customization/");
		private static Texture2D[] _randomFaces;

		public static List<Unit> unitDataBase;
		private static List<Unit> unusedPlayerUnits;

		public static Texture2D GetRandomFaceFromSeed(int seed)
		{
			var random = new Unity.Mathematics.Random((uint)seed);
			return RandomFaces[random.NextInt(0, RandomFaces.Length)];
		}
		
		public static Unit GetRandomUnitAtCircle(int circleLevel)
		{
			var chanceForPlayerCharacter = unusedPlayerUnits?.Count > 0 && Random.value < 0.66f;
			var databaseUnit = chanceForPlayerCharacter ? unusedPlayerUnits.GetRandom() : null;
			if (databaseUnit != null) unusedPlayerUnits.Remove(databaseUnit);
			var newUnit = databaseUnit != null ? new Unit(databaseUnit.name, databaseUnit.seed) : new Unit(RandomNames.GetRandom(), Random.Range(int.MinValue, int.MaxValue));
			newUnit.faceTexture = databaseUnit != null ? databaseUnit.faceTexture :GetRandomFaceFromSeed(newUnit.seed);
			for (int i = 0; i <= circleLevel; i++)
			{
				var chosenCard = CardManager.AllCards.Where(x => x.circleOfHell == i).ToList().GetRandom();
				newUnit.cards.Add(chosenCard);
			}

			return newUnit;
		}

		public static List<Unit> GetSquadAtCircle(int circleLevel)
		{
			return new List<Unit>()
			{
				CharacterCreator.GetRandomUnitAtCircle(circleLevel),
				CharacterCreator.GetRandomUnitAtCircle(circleLevel),
				CharacterCreator.GetRandomUnitAtCircle(circleLevel),
				CharacterCreator.GetRandomUnitAtCircle(circleLevel)
			};
		}
		
		#if UNITY_EDITOR
		[CustomEditor(typeof(CharacterCreator))]
		public class Inspector : Editor
		{
			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();
				EditorGUILayout.LabelField("---Custom---");
				var t = target as CharacterCreator;
				if (unitDataBase != null)
				{
					EditorGUILayout.LabelField($"Unit Database ({unitDataBase.Count})");
					EditorGUI.indentLevel++;
					foreach (var unit in unitDataBase)
					{
						EditorGUILayout.LabelField(unit.name);
						if (unit.faceTexture == null) continue;
						var rect = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
						EditorGUI.DrawTextureTransparent(rect, unit.faceTexture);
					}

					EditorGUI.indentLevel--;
				}
			}
		}		
		#endif

	}
}