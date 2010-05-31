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
using System.ComponentModel;
using System.IO;
using FsmReader;
using ICSharpCode.AvalonEdit.Highlighting;
using Diff;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit;
using System.Diagnostics;
using TreeViewer.ViewModels;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for TreeDiffControl.xaml
	/// </summary>
	public partial class TreeDiffControl : UserControl {
		public TreeDiffControl() {
			InitializeComponent();

			LeftCodeText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");
			//RightCodeText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");

			TreeDiffControlViewModel vm = new TreeDiffControlViewModel();
			vm.UIAction = (s) => this.Dispatcher.BeginInvoke(s, null);

			this.DataContext = vm;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			LeftCodeText.ScrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewer_ScrollChanged);
		}

		void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
			//throw new NotImplementedException();
		}
	}
}
