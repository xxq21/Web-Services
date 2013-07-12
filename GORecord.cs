using System;
using System.Data;
using System.Collections;

namespace PathwaysService
{
	/// <summary>
	/// Summary description for GORecord.
	/// </summary>
	public class GORecord
	{
		private string _id, _name, _notes, _ecNumber, _processName;
		private int _treeLevel;
//		private ArrayList _meta = new ArrayList();
		public string ID{get{return _id;}set{_id = value;}}
		public string Name{get{return _name;}set{_name = value;}}
		public string Notes{get{return _notes;}set{_notes = value;}}
		public int TreeLevel{get{return _treeLevel;}set{_treeLevel=value;}}
//		public ArrayList Meta{get{return _meta;}}

		private Guid _processID;
//		private string _processName;
		public string ECNumber{get{return _ecNumber;}set{_ecNumber = value;}}
		public Guid ProcessID{get{return _processID;}set{_processID = value;}}
		public string ProcessName{get{return _processName;}set{_processName = value;}}

		public GORecord(){}
//		public GORecord(string id, string name, string ecNumber)
//		{
//			_id = id;
//			_name = name;
//			_ecNumber = ecNumber;
//		}

		public GORecord(string id, string name)
		{
			_id = id;
			_name = name;
		}

//		public void AddMetaRecord(string key, string val)
//		{
//			_meta.Add(new string[]{key, val});
//		}
//
//		public string[][] GetMetaRecords()
//		{
//			return (string[][])_meta.ToArray(typeof(string[]));
//		}
//
//		public void AddMetaRecords(string[][] records)
//		{
//			foreach(string[] pair in records)
//				AddMetaRecord(pair[0], pair[1]);
//		}

		public static GORecord FromDataRow(DataRow dr)
		{
			GORecord rec = new GORecord();
			if(dr.Table.Columns.Contains("GOID"))
			{
				rec._id = (string)dr["GOID"];
				if(rec._id.StartsWith("GO:")) rec._id = rec._id.Substring("GO:".Length);
			}
			if(dr.Table.Columns.Contains("GOName"))
				rec._name = (string)dr["GOName"];

			//put the rest of the DataRow information into the meta dictionary
//			int colID = dr.Table.Columns["GOID"].Ordinal;
//			int colName = dr.Table.Columns["GOName"].Ordinal;
//			for(int i=0; i<dr.Table.Columns.Count; i++)
//			{
//				if( i == colID || i == colName) continue;
//				string key = dr.Table.Columns[i].ColumnName;
//				string val = dr[i].ToString();
//				rec.AddMetaRecord(key, val);
//			}

			if(dr.Table.Columns.Contains("SwissProID"))
				rec._notes = "linked through Protein: " + (string)dr["protein_name"] ;
			
			if(dr.Table.Columns.Contains("ECNumber"))
			{
				string ec = (string)dr["ECNumber"];
				rec._notes = "linked through EC: " + ec;
				rec._ecNumber = ec;
			}
			if(dr.Table.Columns.Contains("process_ID") && !dr.IsNull("process_ID"))
				rec._processID = (Guid)dr["process_ID"];
			if(dr.Table.Columns.Contains("process_name") && !dr.IsNull("process_name"))
				rec._processName = (string)dr["process_name"];
			if(dr.Table.Columns.Contains("TermLevel") && !dr.IsNull("TermLevel"))
				rec._treeLevel = (int)dr["TermLevel"];
			return rec;
		}

		public static GORecord[] FromDataTable(DataTable dt)
		{
			GORecord[] array = new GORecord[dt.Rows.Count];
			for(int i=0; i<array.Length; i++)
				array[i] = GORecord.FromDataRow(dt.Rows[i]);
			return array;
		}

		public override bool Equals(object obj)
		{
			if(obj.GetType() != typeof(GORecord)) return base.Equals(obj);
			GORecord rec = obj as GORecord;
			return 
				this.ID == rec.ID &&
				this.ECNumber == rec.ECNumber &&
				this.Name == rec.Name &&
				this.Notes == rec.Notes &&
				this.ProcessID == rec.ProcessID &&
				this.ProcessName == rec.ProcessName;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}


	}
}
