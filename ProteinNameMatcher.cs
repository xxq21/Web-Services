using System;

namespace GO2Pathways
{
	public class ProteinNameMatcher
	{
		public static bool IsMatch(string left, string right, bool ignoreCase)
		{
			string modLeft, modRight;
			if(ignoreCase)
			{
				modLeft = left.ToLower();
				modRight = right.ToLower();
			}
			else
			{
				modLeft = left;
				modRight = right;
			}

			modLeft = StripString(modLeft);
			modRight = StripString(modRight);

			if(modLeft == modRight) 
				System.Diagnostics.Debug.WriteLine(string.Format("MATCH: {0} = {1}", left, right));
			return modLeft == modRight;
		}

		/// <summary>
		/// Returns the string with all non-alphanumeric characters cleared from it
		/// </summary>
		/// <param name="s">The string we're working on</param>
		/// <returns>The string, modified</returns>
		public static string StripString(string s)
		{
			//clear both strings of non-alphanumeric characters
			string modS = string.Empty;
			for(int i=0; i<s.Length; i++)
				if(char.IsLetterOrDigit(s[i])) modS += s[i];
			return modS;
		}
	}
}
