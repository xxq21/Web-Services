using System;

namespace GO2Pathways
{
	public class ProcessToProcessRelationship
	{
		private Guid _leftProcessID, _rightProcessID;
		private string _roleLeft, _roleRight;
		private string _leftProcessName, _rightProcessName;

		public Guid LeftProcessID{get{return _leftProcessID;}set{_leftProcessID=value;}}
		public Guid RightProcessID{get{return _rightProcessID;}set{_rightProcessID=value;}}
		public string LeftProcessName{get{return _leftProcessName;}set{_leftProcessName=value;}}
		public string RightProcessName{get{return _rightProcessName;}set{_rightProcessName=value;}}
		public string RoleLeft{get{return _roleLeft;}set{_roleLeft=value;}}
		public string RoleRight{get{return _roleRight;}set{_roleRight=value;}}

		public ProcessToProcessRelationship(){}
		public ProcessToProcessRelationship(Guid leftProcessID, Guid rightProcessID, string leftProcessName, string rightProcessName, string roleLeft, string roleRight)
		{
			_leftProcessID = leftProcessID;
			_rightProcessID = rightProcessID;
			_leftProcessName =  leftProcessName;
			_rightProcessName = rightProcessName;
			_roleLeft = roleLeft;
			_roleRight = roleRight;
		}
	}
}
