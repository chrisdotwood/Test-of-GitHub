using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TreeViewer {
	public class CommandViewModel : ViewModelBase {
		public CommandViewModel(string displayName, ICommand command) {
			base.DisplayName = displayName;
			Command = command;
		}

		public ICommand Command {
			get;
			private set;
		}
	}
}
