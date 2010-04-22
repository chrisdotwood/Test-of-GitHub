using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diff {
	public class DiffDocument {
		private string[] Lines;

		private DiffDocument() { }

		public DiffDocument(string text) {
			Lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		}

		public string this[int index] {
			get { 
				return Lines[index];
			}
		}

		/// <summary>
		/// Get the number of lines in the document
		/// </summary>
		public int Length {
			get {
				return Lines.Length;
			}
		}

		public DiffDocument Substring(int start, int end) {
			DiffDocument ret = new DiffDocument();
			ret.Lines = new string[end - start];

			for (int i = start; i < end; i++) {
				ret.Lines[i] = Lines[i];
			}
			return ret;
		}
	}
}
