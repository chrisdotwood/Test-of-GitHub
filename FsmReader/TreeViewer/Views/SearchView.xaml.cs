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
	/// Interaction logic for SearchView.xaml
	/// </summary>
	public partial class SearchView : Window {
		public SearchView() {
			InitializeComponent();
		}

		public SearchView(TreenodeViewModel root) {
			InitializeComponent();

			SearchViewModel svm = new SearchViewModel(root);
			svm.UIAction = ((uiaction) => Dispatcher.BeginInvoke(uiaction));

			this.DataContext = svm;
		}

	}
}
