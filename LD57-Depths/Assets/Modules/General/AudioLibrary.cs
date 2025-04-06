using System.Collections.Generic;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	public class AudioLibrary : ScriptableObject
	{
		public static AudioLibrary instance => _instance ??= Resources.Load<AudioLibrary>("AudioLibrary");
		private static AudioLibrary _instance;
		
		public List<AudioClip> swearWords = new List<AudioClip>();
		private List<AudioClip> swearQueue = new List<AudioClip>();
		public static AudioClip GetSwear()
		{
			if(instance.swearQueue.Count == 0) instance.swearQueue.AddRange(instance.swearWords);
			var random = instance.swearQueue.GetRandom();
			instance.swearQueue.Remove(random);
			return random;
		}
	}
}