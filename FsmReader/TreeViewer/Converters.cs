using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using FsmReader;

namespace TreeViewer {
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

	public class BoolInverterConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return !(bool)value;
		}
	}
}
