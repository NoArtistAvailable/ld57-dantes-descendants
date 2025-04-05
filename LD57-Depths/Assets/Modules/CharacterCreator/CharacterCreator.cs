using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	public class CharacterCreator : MonoBehaviour
	{
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

		public static Unit GetRandomUnitAtCircle(int circleLevel)
		{
			var newUnit = new Unit(RandomNames.GetRandom(), Random.Range(int.MinValue, int.MaxValue));
			newUnit.faceTexture = RandomFaces.GetRandom();
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

	}
}