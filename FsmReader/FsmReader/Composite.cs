using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace FsmReader {
    public abstract class Composite {
        public abstract ReadOnlyCollection<Composite> Children {
            get;
        }

        public bool Accept(IVisitor visitor, Stack<int> stack) {
            Stack<int> inStack = new Stack<int>();

            // Reverse the stack
            Stack<int> resumeStack = new Stack<int>(stack.AsEnumerable());

			bool result = Accept(visitor, inStack, resumeStack);

			stack.Clear();
			foreach (int i in inStack) {
				stack.Push(i);
			}
            return result;
        }

        private bool Accept(IVisitor visitor, Stack<int> inStack, Stack<int> resumeStack) {
            if (visitor.VisitEnter(this)) {

                int firstChild = 0;

                if (resumeStack.Count > 0) {
                    firstChild = resumeStack.Pop();
                    if (resumeStack.Count == 0) {
                        // If this is the node we found last time then start looking at the next one
                        firstChild++;
                    }
                }

                for (int i = firstChild; i < Children.Count; i++) {
                    Composite c = Children[i];
					if (!c.Accept(visitor, inStack, resumeStack)) {
						inStack.Push(i);
                        break;
                    }
                }
            }

            return visitor.VisitExit(this);
        }
    }
}
