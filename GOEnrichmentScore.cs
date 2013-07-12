using System;

namespace GO2Pathways
{
	[Serializable]
	public class GOEnrichmentScore
	{
		private float _score;
		private GOEnrichmentType _type;

		public float Score{get{return _score;}set{_score = value;}}
		public GOEnrichmentType EnrichmentType{get{return _type;}set{_type = value;}}

		public GOEnrichmentScore(float score, GOEnrichmentType type)
		{
			_score = score;
			_type = type;
		}
		
		public GOEnrichmentScore(){}

		public override string ToString()
		{
			return _type == GOEnrichmentType.Underrepresented ? "-" : "" + _score.ToString();
		}

	}

	[Serializable]
	public enum GOEnrichmentType
	{
		Underrepresented,
		Overrepresented,
		Expected
	}
}
