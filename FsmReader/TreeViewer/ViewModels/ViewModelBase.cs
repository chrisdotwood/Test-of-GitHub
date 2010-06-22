using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace TreeViewer {
	public class ViewModelBase : IDisposable, INotifyPropertyChanged {
		public string DisplayName {
			get;
			set;
		}

		public Action<Action> UIAction = ((uiaction) => uiaction());

		/// <summary>
		/// Fire any attached PropertyChanged events for the specified property.
		/// </summary>
		/// <param name="property">The name of the property that changed.</param>
		/// <remarks>
		/// This calls <see cref="ViewModelBase.VerifyPropertyName"/> if DEBUG is set.
		/// </remarks>
		protected void FirePropertyChanged(string property) {
			VerifyPropertyName(property);
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		/// <summary>
		/// Verify that the property name matches a   
		///	public instance property on this object
		/// </summary>
		/// <param name="propertyName">The name of the property to test.</param>
		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		protected void VerifyPropertyName(string propertyName) {
			if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
				string msg = "Invalid property name: " + propertyName;

				Debug.Fail(msg);
			}
		}

		public virtual void Dispose() {}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
