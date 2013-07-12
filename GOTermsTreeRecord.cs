using System;
using System.Data;

namespace GO2Pathways
{
	public struct GOTermsTreeRecord
	{
		public string parentID, childID, relationType;
		public int treeLevel;

		public GOTermsTreeRecord(string parentID, string childID, string relationType, int treeLevel)
		{
			this.parentID = parentID;
			this.childID = childID;
			this.relationType = relationType;
			this.treeLevel = treeLevel;
		}

		public static GOTermsTreeRecord FromDataRow(DataRow dr)
		{
			return new GOTermsTreeRecord((string)dr["ParentID"], (string)dr["ChildID"], (string)dr["Type"], (int)dr["TermLevel"]);
		}
	}
}
