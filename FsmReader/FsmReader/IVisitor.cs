using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FsmReader {
	public interface IVisitor {
		bool VisitEnter(Composite composite);
		bool VisitExit(Composite composite);
	}
}
