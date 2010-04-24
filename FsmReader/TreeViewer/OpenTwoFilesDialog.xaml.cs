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
using Microsoft.Win32;
using System.IO;

namespace TreeViewer {
	public partial class OpenTwoFilesDialog : Window {
		public string LeftPath {
			get { return (string)GetValue(LeftPathProperty); }
			set { SetValue(LeftPathProperty, value); }
		}

		// Using a DependencyProperty as the backing store for LeftPath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LeftPathProperty =
			DependencyProperty.Register("LeftPath", typeof(string), typeof(OpenTwoFilesDialog), new UIPropertyMetadata(""));

		public string RightPath {
			get { return (string)GetValue(RightPathProperty); }
			set { SetValue(RightPathProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RightPath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RightPathProperty =
			DependencyProperty.Register("RightPath", typeof(string), typeof(OpenTwoFilesDialog), new UIPropertyMetadata(""));
		
		private OpenFileDialog ofd = new OpenFileDialog();

		public OpenTwoFilesDialog() {
			InitializeComponent();

			ofd.Filter = "Flexsim Models (*.fsm)|*.fsm|Tree Files (*.t)|*.t|All Files (*.*)|*.*";
			ofd.Multiselect = true;
			ofd.Title = "Select file...";
		}

		private void LeftBrowseButton_Click(object sender, RoutedEventArgs e) {
			Nullable<bool> result = ofd.ShowDialog();
			if (result.HasValue && result.Value == true) {
				LeftPath = ofd.FileName;
				if (ofd.FileNames.Length > 1) {
					RightPath = ofd.FileNames[1];
				}
			}
		}

		private void RightBrowseButton_Click(object sender, RoutedEventArgs e) {
			Nullable<bool> result = ofd.ShowDialog();
			if (result.HasValue && result.Value == true) {
				RightPath = ofd.FileName;
			}
		}

		private void OkButton_Click(object sender, RoutedEventArgs e) {
			if (!File.Exists(LeftPathText.Text)) {
				MessageBox.Show(LeftPathText.Text + Environment.NewLine + "File does not exist, check the path and try again", "File Doesn't Exist", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			if (!File.Exists(LeftPathText.Text)) {
				MessageBox.Show(RightPathText.Text + Environment.NewLine + "File does not exist, check the path and try again", "File Doesn't Exist", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
		
			this.DialogResult = true;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e) {
			this.DialogResult = true;
			Close();
		}
	}
}
