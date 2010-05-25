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
using System.Windows.Shapes;
using FsmReader;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for SearchDialog.xaml
	/// </summary>
	public partial class SearchDialog : Window {
		public SearchDialog() {
			InitializeComponent();
		}
		
		public SearchDialog(Treenode root) {
			InitializeComponent();

			this.DataContext = new SearchViewModel(root);
		}
	}
}
