﻿using System;
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

namespace TreeViewer {
	/// <summary>
	/// Interaction logic for TreenodeFlagControl.xaml
	/// </summary>
	public partial class TreenodeFlagControl : UserControl {
		public TreenodeFlagControl() {
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {

		}
	}

	public class FlagsToCheckedConverter : IValueConverter {
		private Flags flags;

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			flags = (Flags)value;

			return ((Flags)Enum.Parse(typeof(Flags), parameter.ToString()) | flags) == flags;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			Flags setting = (Flags)Enum.Parse(typeof(Flags), parameter.ToString());
			flags ^= setting;
			return flags;
		}
	}

	public class FlagsExtendedToCheckedConverter : IValueConverter {
		private FlagsExtended flags;

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			flags = (FlagsExtended)value;

			return ((FlagsExtended)Enum.Parse(typeof(FlagsExtended), parameter.ToString()) | flags) == flags;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			FlagsExtended setting = (FlagsExtended)Enum.Parse(typeof(FlagsExtended), parameter.ToString());
			flags ^= setting;
			return flags;
		}
	}

	public class DataTypeToCheckedConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return parameter.ToString() == Enum.GetName(typeof(DataType), value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value != null) {
				return Enum.Parse(typeof(DataType), parameter.ToString());
			} else {
				return null;
			}
		}
	}
}
