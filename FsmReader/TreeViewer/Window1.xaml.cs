using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FsmReader;
using System.IO;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit.Highlighting;
using System.ComponentModel;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window {
		public Window1() {
			InitializeComponent();

			//Treenode m5 = null;
			//using (FileStream fs = new FileStream(@"C:\Users\Chris\Documents\Visual Studio 2010\Projects\Test-of-GitHub\model5.fsm", FileMode.Open, FileAccess.Read)) {
			//    m5 = Treenode.Read(fs);
			//}

			//if (m5 != null) {
			//    using (FileStream fs = new FileStream(@"C:\Users\Chris\Documents\Visual Studio 2010\Projects\Test-of-GitHub\model5.fsm3", FileMode.OpenOrCreate, FileAccess.Write)) {
			//        Treenode.Write(m5, fs);
			//    }
			//}
			//Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
		}

		private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.Handled = e.CanExecute = true;
		}

		private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e) {

		}
	}
}
