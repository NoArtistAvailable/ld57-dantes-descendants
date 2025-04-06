using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LD57
{
	public static class OnlineManager
	{
		public static string gameName => "dantes-descendants";
		public const string serverUrl = "https://elzach-gamejams.glitch.me";

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
	}
}