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
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using TreeViewer.ViewModels;

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for FsmTreeView.xaml
	/// </summary>
	public partial class FsmTreeView : UserControl {
		FsmTreeViewModel vm;

		public FsmTreeView() {
			InitializeComponent();
			
			vm = new FsmTreeViewModel();
			vm.UIAction = (s) => this.Dispatcher.BeginInvoke(s, null);

			this.DataContext = vm;
		}

		private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			vm.SelectedItem = (TreenodeViewModel)e.NewValue;
		}

	
	}


	public class BooleanToVisibilityConverter : IValueConverter {
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if ((bool)value) {
				return Visibility.Visible;
			} else {
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
