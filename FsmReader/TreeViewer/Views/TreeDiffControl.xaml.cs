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
using ICSharpCode.AvalonEdit.Document;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for TreeDiffControl.xaml
	/// </summary>
	public partial class TreeDiffControl : UserControl {
		public TreeDiffControl() {
			InitializeComponent();

			LeftCodeText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");
			RightCodeText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");

            FsmTreeViewModel lvm = (FsmTreeViewModel)LeftTreeView.DataContext;
            FsmTreeViewModel rvm = (FsmTreeViewModel)RightTreeView.DataContext;

			TreeDiffControlViewModel vm = new TreeDiffControlViewModel(lvm, rvm);
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

	/// <summary>
	/// Converts a string to a <see cref="ICSharpCode.AvalonEdit.Document.TextDocument"/>
	/// </summary>
	public class TextToDocumentConvert : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return new TextDocument(value as string ?? string.Empty);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return ((TextDocument)value).Text;
		}
	}
}
