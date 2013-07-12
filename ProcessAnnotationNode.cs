using System;
using System.Collections;

namespace GO2Pathways
{
	public class ProcessAnnotationNode
	{
		private Guid _processID;
		private string _processName, _GOID, _GOName;

		private ArrayList _incomingEdges = new ArrayList(0);
		private ArrayList _outgoingEdges = new ArrayList(0);

		public Guid ProcessID{get{return _processID;}}
		public string ProcessName{get{return _processName;}}
		public string GOID{get{return _GOID;}}
		public string GOName{get{return _GOName;}}

		public ArrayList IncomingEdges{get{return _incomingEdges;}}
		public ArrayList OutgoingEdges{get{return _outgoingEdges;}}

		public int AddIncomingEdge(ProcessAnnotationEdge edge)
		{
			return _incomingEdges.Add(edge);
		}

		public int AddOutgoingEdge(ProcessAnnotationEdge edge)
		{
			return _outgoingEdges.Add(edge);
		}

		public ProcessAnnotationNode(Guid processID, string processName, string goID, string goName)
		{
			_processID = processID;
			_processName = processName;
			_GOID = goID;
			_GOName = GOName;
		}
	}
}
