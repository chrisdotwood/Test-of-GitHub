using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace TreeViewer {
	public class ViewModelBase : DependencyObject, INotifyPropertyChanged, IDisposable {
		public event PropertyChangedEventHandler PropertyChanged;

		public string DisplayName {
			get;
			set;
		}

		public Action<Action> UIAction = ((uiaction) => uiaction());

		protected virtual void OnPropertyChanged(string propertyName) {
			this.VerifyPropertyName(propertyName);

			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null) {
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}

		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		public void VerifyPropertyName(string propertyName) {
			// Verify that the property name matches a real,  
			// public, instance property on this object.
			if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
				string msg = "Invalid property name: " + propertyName;

				Debug.Fail(msg);
			}
		}

		public virtual void Dispose() {}
	}
}
