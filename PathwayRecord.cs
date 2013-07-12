using System;
using System.Data;

namespace GO2Pathways
{
	public class PathwayRecord
	{
		string _name, _type, _notes, _status, _goID, _goName;
		Guid _id, _processID;
		public string Name{get{return _name;}set{_name = value;}}
		public string Type{get{return _type;}set{_type = value;}}
		public string Notes{get{return _notes;}set{_notes = value;}}
		public string Status{get{return _status;}set{_status=value;}}
		public Guid ID{get{return _id;}set{_id = value;}}
		public string GOID{get{return _goID;}set{ _goID = value;}}
		public string GOName{get{return _goName;}set{_goName = value;}}
		public Guid ProcessID{get{return _processID;}set{_processID = value;}}
 
		public PathwayRecord()
		{
		}

		public PathwayRecord(Guid id, string name, string type, string notes, string status)
		{
			_id = id;
			_name = name;
			_type = type;
			_name = notes;
			_status = status;
		}

		public static PathwayRecord FromDataRow(DataRow dr)
		{
			PathwayRecord rec = new PathwayRecord();
			rec.ID = (Guid)dr["id"];
			rec.Name = (string)dr["name"];

			if(dr["type"] != DBNull.Value)
				rec.Type = (string)dr["type"];
			if(dr["status"] != DBNull.Value)
				rec.Status = (string)dr["status"];
			if(dr["notes"] != DBNull.Value)
				rec.Notes = (string)dr["notes"];
			if(dr.Table.Columns.Contains("GOID"))
				rec._goID = (string)dr["GOID"];
			if(dr.Table.Columns.Contains("process_id"))
				rec._processID = (Guid)dr["process_id"];
			if(dr.Table.Columns.Contains("GOName"))
				rec._goName = (string)dr["GOName"];
			return rec;
		}
	}
}
