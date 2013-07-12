using System;
using System.Data;

namespace GO2Pathways
{
	/// <summary>
	/// Summary description for ProcessRecord.
	/// </summary>
	public class ProcessRecord
	{
		Guid _id, _genericProcessID;
		string _name, _location, _type, _notes;
		bool _reversible;

		public Guid ID{get{return _id;} set{_id = value;}}
		public Guid GenericProcessID{get{return _genericProcessID;} set{_genericProcessID = value;}}
		public string Name{get{return _name;} set{_name = value;}}
		public string Location{get{return _location;} set{_location = value;}}
		public string Type{get{return _type;} set{_type = value;}}
		public string Notes{get{return _notes;} set{_notes = value;}}
		public bool Reversible{get{return _reversible;} set{_reversible = value;}}

		public ProcessRecord(){}

		public static ProcessRecord FromDataRow(DataRow dr)
		{
			ProcessRecord rec = new ProcessRecord();
			rec._id = (Guid)dr["id"];
			rec._name = (string)dr["name"];
			if(dr["reversible"] != DBNull.Value)
				rec.Reversible = (bool)dr["reversible"];
			if(dr["location"] != DBNull.Value)
				rec.Location= (string)dr["location"];
			if(dr["type"] != DBNull.Value)
				rec.Type = (string)dr["type"];
			if(dr["notes"] != DBNull.Value)
				rec.Notes = (string)dr["notes"];
			if(dr["generic_process_id"] != DBNull.Value)
				rec.GenericProcessID = (Guid)dr["generic_process_id"];
			return rec;
		}
	}
}
