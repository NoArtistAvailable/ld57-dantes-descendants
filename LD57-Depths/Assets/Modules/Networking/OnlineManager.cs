using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elZach.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace LD57
{
	public static class OnlineManager
	{
		public static string gameName => "dantes-descendants";
		public const string serverUrl = "https://elzach-gamejams.glitch.me";

		public static float DateToScore() => DateTime.Now.Minute + DateTime.Now.Hour * 60 + DateTime.Now.DayOfYear * 60 * 24 + DateTime.Now.Year * 60 * 24 * 356;

		#region Customization
		public static event Action<List<SinnerData>> onGotSinners;
		[Serializable]
		public class SinnerData
		{
			public string name;
			public int seed;
			public string imageBase64;
			// we have this just because we hook into a highscore server
			public float score;
		}

		public class SinnerDataList
		{
			public List<SinnerData> sinners;
		}
		
		public static async void PostSinnerAsync(SinnerData dataObject)
		{
			string url = $"{serverUrl}/highscores/{gameName}-faces";

			// Convert the data object to JSON
			string json = JsonUtility.ToJson(dataObject);
			Debug.Log("Sending JSON: " + json);

			// Create a UnityWebRequest for POST
			UnityWebRequest request = new UnityWebRequest(url, "POST");
			byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");

			// Send the request and await the response
			var operation = request.SendWebRequest();

			while (!operation.isDone)
				await Task.Yield();

			if (request.result == UnityWebRequest.Result.Success) Debug.Log("POST request successful: " + request.downloadHandler.text);
			else Debug.LogError($"POST request failed: {request.error}");
		}
		
		public static async void GetSinnersAsync()
		{
			string url = $"{serverUrl}/allhighscores/{gameName}-faces";
			
			UnityWebRequest request = UnityWebRequest.Get(url);
			var operation = request.SendWebRequest();

			while (!operation.isDone) await Task.Yield();

			if (request.result == UnityWebRequest.Result.Success)
			{
				Debug.Log("GET request successful: " + request.downloadHandler.text);
				var json = request.downloadHandler.text;
				SinnerDataList scoreDataList = JsonUtility.FromJson<SinnerDataList>("{\"sinners\":" + json + "}");
				for (var i = 0; i < scoreDataList.sinners.Count; i++)
				{
					var entry = scoreDataList.sinners[i];
					Debug.Log($"[{i}] {entry.name} : {entry.seed}");
				}
				onGotSinners?.Invoke(scoreDataList.sinners);
			}
			else
			{
				Debug.LogError($"GET request failed: {request.error}");
			}
		}
		#endregion

		#region Squad

		public static event Action<int,List<SquadData>> onGotCircle;
		[Serializable]
		public class SquadData
		{
			public UnitData[] units;
			public string name;
			public float score;
		}

		public static SquadData ToData(List<Unit> units, float score)
		{
			var squadData = new SquadData();
			squadData.score = score;

			var unitDataList = new List<UnitData>();
			foreach (var unit in units)
			{
				var data = new UnitData();
				data.name = unit.name;
				data.seed = unit.seed;
				data.cards = unit.cards.Select(x => x.GetType().Name).ToArray();
				unitDataList.Add(data);
			}
			squadData.units = unitDataList.ToArray();
			squadData.name = $"{unitDataList[0].name}s squad";
			return squadData;
		}

		public static List<Unit> FromData(SquadData squadData)
		{
			var list = new List<Unit>();
			foreach (var unitData in squadData.units)
			{
				var unit = new Unit(unitData.name, unitData.seed);
				var texture = CharacterCreator.unitDataBase != null ? CharacterCreator.unitDataBase.FirstOrDefault(x => x.name == unitData.name)?.faceTexture : null;
				if (!texture) texture = CharacterCreator.GetRandomFaceFromSeed(unitData.seed);
				unit.faceTexture = texture;
				unit.cards = new List<Card>();
				foreach (var cardName in unitData.cards)
				{
					var cardType = CardManager.cardTypes.FirstOrDefault(x => x.Name == cardName);
					if(cardType == null) continue;
					unit.cards.Add(Activator.CreateInstance(cardType) as Card);
				}
				list.Add(unit);
			}
			return list;
		}

		[Serializable]
		public class UnitData
		{
			public string name;
			public int seed;
			// could be made implicitly unit name
			// public string faceName;
			public string[] cards;
		}

		public class SquadDataList
		{
			public List<SquadData> squads;
		}
		
		public static async void PostSquadAsync(List<Unit> runtimeData, int circle)
		{
			var dataObject = ToData(runtimeData, DateToScore());
			string url = $"{serverUrl}/highscores/{gameName}-circle-{circle}";

			// Convert the data object to JSON
			string json = JsonUtility.ToJson(dataObject);
			Debug.Log("Sending JSON: " + json);

			// Create a UnityWebRequest for POST
			UnityWebRequest request = new UnityWebRequest(url, "POST");
			byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");

			// Send the request and await the response
			var operation = request.SendWebRequest();

			while (!operation.isDone)
				await Task.Yield();

			if (request.result == UnityWebRequest.Result.Success) Debug.Log("POST request successful: " + request.downloadHandler.text);
			else Debug.LogError($"POST request failed: {request.error}");
		}
		
		public static async void GetCircleAsync(int circle)
		{
			string url = $"{serverUrl}/allhighscores/{gameName}-circle-{circle}";
			
			UnityWebRequest request = UnityWebRequest.Get(url);
			var operation = request.SendWebRequest();

			while (!operation.isDone) await Task.Yield();

			if (request.result == UnityWebRequest.Result.Success)
			{
				Debug.Log("GET request successful: " + request.downloadHandler.text);
				var json = request.downloadHandler.text;
				SquadDataList scoreDataList = JsonUtility.FromJson<SquadDataList>("{\"squads\":" + json + "}");
				onGotCircle?.Invoke(circle, scoreDataList.squads);
			}
			else
			{
				Debug.LogError($"GET request failed: {request.error}");
			}
		}

		#endregion

	}
}