using System;

namespace GO2Pathways
{
	public class SpellChecker
	{
		#region Edit Distance
		// Copyright (c) 2003, Paul Welter
		// All rights reserved.
		// http://sourceforge.net/project/showfiles.php?group_id=76171

		/// <summary>
		///     Calculates the minimum number of change, inserts or deletes
		///     required to change firstWord into secondWord
		/// </summary>
		/// <param name="source" type="string">
		///     <para>
		///         The first word to calculate
		///     </para>
		/// </param>
		/// <param name="target" type="string">
		///     <para>
		///         The second word to calculate
		///     </para>
		/// </param>
		/// <param name="positionPriority" type="bool">
		///     <para>
		///         set to true if the first and last char should have priority
		///     </para>
		/// </param>
		/// <returns>
		///     The number of edits to make firstWord equal secondWord
		/// </returns>
		public static int EditDistance(string source, string target, bool positionPriority, bool ignoreCase)
		{
			if(ignoreCase)
			{
				source = source.ToLower();
				target = target.ToLower();
			}

			// i.e. 2-D array
			Array matrix = Array.CreateInstance(typeof(int), source.Length+1, target.Length+1);

			// boundary conditions
			matrix.SetValue(0, 0, 0); 

			for(int j=1; j <= target.Length; j++)
			{
				// boundary conditions
				int val = (int)matrix.GetValue(0,j-1);
				matrix.SetValue(val+1, 0, j);
			}

			// outer loop
			for(int i=1; i <= source.Length; i++)                            
			{ 
				// boundary conditions
				int val = (int)matrix.GetValue(i-1, 0);
				matrix.SetValue(val+1, i, 0); 

				// inner loop
				for(int j=1; j <= target.Length; j++)                         
				{ 
					int diag = (int)matrix.GetValue(i-1, j-1);

					if(source.Substring(i-1, 1) != target.Substring(j-1, 1)) 
					{
						char srcChar = source.Substring(i-1, 1)[0];
						char tgtChar = target.Substring(j-1, 1)[0];
						
						if(char.IsLetterOrDigit(srcChar) || char.IsLetterOrDigit(tgtChar))
							diag += DISTANCE_LETTER_DIFFERENCE;
						else
							diag += DISTANCE_NON_LETTER_DIFFERENCE;

						//modified from the original version
//						diag++;
					}

					int deletion = (int)matrix.GetValue(i-1, j);
					int insertion = (int)matrix.GetValue(i, j-1);
					//TODO: change this so that the larger scores will come through in the results
					int match = Math.Min(deletion+1, insertion+1);		
					matrix.SetValue(Math.Min(diag, match), i, j);
//					matrix.SetValue(Math.Max(diag, match), i, j);
				}//for j
			}//for i

			int dist = (int)matrix.GetValue(source.Length, target.Length);

			// extra edit on first and last chars
			if (positionPriority)	//this really makes no difference
			{
				if (source[0] != target[0]) dist++;
				if (source[source.Length-1] != target[target.Length-1]) dist++;
			}
			return dist;
		}
		
		/// <summary>
		///     Calculates the minimum number of change, inserts or deletes
		///     required to change firstWord into secondWord
		/// </summary>
		/// <param name="source" type="string">
		///     <para>
		///         The first word to calculate
		///     </para>
		/// </param>
		/// <param name="target" type="string">
		///     <para>
		///         The second word to calculate
		///     </para>
		/// </param>
		/// <returns>
		///     The number of edits to make firstWord equal secondWord
		/// </returns>
		/// <remarks>
		///		This method automatically gives priority to matching the first and last char and ignores character casing
		/// </remarks>
		public static int EditDistance(string source, string target)
		{
			return EditDistance(source, target, true, true);
		}

		private static readonly int DISTANCE_NON_LETTER_DIFFERENCE = 1;
		private static readonly int DISTANCE_LETTER_DIFFERENCE = 4;
		#endregion
	}
}
