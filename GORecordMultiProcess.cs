using System;

namespace GO2Pathways
{
	public class GORecordMultiProcess
	{
		private string _id, _name;
		private string[] _processNames;
		private Guid[] _processIDs;
		private string[] _ecNumbers;
		private int[] _processLinkStrengths;
		private int _treeLevel;

		//public accessors
		public string ID{get{return _id;}set{_id = value;}}
		public string[] ProcessNames{get{return _processNames;}set{_processNames = value;}}
		public Guid[] ProcessIDs{get{return _processIDs;}set{_processIDs = value;}}
		public string[] ECNumbers{get{return _ecNumbers;}set{_ecNumbers = value;}}
		public int[] LinkStrengths{get{return _processLinkStrengths;}set{_processLinkStrengths = value;}}
		public string Name{get{return _name;}set{_name = value;}}
		public int TreeLevel{get{return _treeLevel;}set{_treeLevel=value;}}

		public int AddProcessAnnotation(Guid processID, string processName, int linkStrength, string ecNumber)
		{
			//add in the new name
			string[] array = new string[_processNames.Length+1];
			_processNames.CopyTo(array, 0);
			array[array.Length-1] = processName;
			_processNames = array;

			//add in the new id
			Guid[] idArray  = new Guid[_processIDs.Length + 1];
			_processIDs.CopyTo(idArray, 0);
			idArray[idArray.Length-1] = processID;
			_processIDs = idArray;

			//add in the new linkStrength
			int[] strengthArray = new int[_processLinkStrengths.Length +1];
			_processLinkStrengths.CopyTo(strengthArray, 0);
			strengthArray[strengthArray.Length-1] = linkStrength;
			_processLinkStrengths = strengthArray;

			//add in the new EC Number
			string[] ecArray = new string[_ecNumbers.Length + 1];
			_ecNumbers.CopyTo(ecArray, 0);
			ecArray[ecArray.Length-1] = ecNumber;
			_ecNumbers = ecArray;

			return _processLinkStrengths.Length-1;
		}


		public int CombinedLinkStrength
		{
			get
			{
				int total =0;
				foreach(int st in _processLinkStrengths)
					total += st;
				return total;
			}
		}


		public GORecordMultiProcess(){}

		public GORecordMultiProcess(string id, string name, int treeLevel, string ecNumber, Guid processID, string processName, int linkStrength)
		{
			_id = id;
			_name = name;
			_treeLevel = treeLevel;
			_processIDs = new Guid[]{processID};
			_processNames = new string[]{processName};
			_processLinkStrengths = new int[]{linkStrength};
			_ecNumbers = new string[]{ecNumber};
		}
	}
}
