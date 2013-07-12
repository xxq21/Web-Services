using System;

namespace GO2Pathways
{
	public class ProcessAnnotationEdge
	{
		private ProcessAnnotationNode _parentAnnotation;
		private ProcessAnnotationNode _childAnnotation;
		private double _linkStrength;

		public ProcessAnnotationNode ParentAnnotation{get{return _parentAnnotation;}}
		public ProcessAnnotationNode ChildAnnotation{get{return _childAnnotation;}}
		public double LinkStrength{get{return _linkStrength;}}

		public ProcessAnnotationEdge(ref ProcessAnnotationNode parentAnnotation, ref ProcessAnnotationNode childAnnotation, double linkStrength)
		{
			_parentAnnotation = parentAnnotation;
			_childAnnotation = childAnnotation;
			_linkStrength = linkStrength;
		}
	}
}
