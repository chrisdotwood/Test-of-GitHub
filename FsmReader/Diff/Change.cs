using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diff {
	public class Change {
		public ChangeType Type;
		public string TextAdded = string.Empty;
		public string TextDeleted = string.Empty;
		
		public int StartPosition1 = -1;
		public int EndPosition1 = -1;
		public int StartPosition2 = -1;
		public int EndPosition2 = -1;


		// BUG This function changes the TextAdded and TextDeleted strings
		///// <summary>
		///// Generate a traditional diff string for a change.
		///// </summary>
		///// <param name="c"></param>
		///// <returns></returns>
		//public override string ToString() {
		//    if (StartPosition1 == -1 || StartPosition2 == -1 || EndPosition1 == -1 || EndPosition2 == -1) {
		//        Console.WriteLine();
		//    }

		//    string loc1 = "", loc2 = "";

		//    if (StartPosition1 == EndPosition1) {
		//        loc1 += StartPosition1;
		//    } else {
		//        loc1 += StartPosition1 + "," + EndPosition1;
		//    }

		//    if (StartPosition2 == EndPosition2) {
		//        loc2 += StartPosition2;
		//    } else {
		//        loc2 += StartPosition2 + "," + EndPosition2;
		//    }

		//    string textAdded = "";
		//    string textDeleted = "";

		//    if(TextAdded.Length >= 2) {
		//        TextAdded = TextAdded.Substring(0, TextAdded.Length - 2);
		//    }

		//    string[] parts = TextAdded.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		//    for (int i = 0; i < parts.Length; i++ ) {
		//        textAdded += "> " + parts[i] + Environment.NewLine;
		//    }

		//    if(TextDeleted.Length >= 2) {
		//        TextDeleted = TextDeleted.Substring(0, TextDeleted.Length - 2);
		//    }
		//    parts = TextDeleted.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		//    for (int i = 0; i < parts.Length; i++) {
		//        textDeleted += "< " + parts[i] + Environment.NewLine;
		//    }

		//    string s = "";
		//    if (Type == ChangeType.Add) {
		//        s = string.Format("{0}a{1}" + Environment.NewLine + "{2}", loc1, loc2, textAdded);
		//    } else if (Type == ChangeType.Remove) {
		//        s = string.Format("{0}d{1}" + Environment.NewLine + "{2}", loc1, loc2, textDeleted);
		//    } else {
		//        s = string.Format("{0}c{1}" + Environment.NewLine + "{2}" + "---" + Environment.NewLine + "{3}",
		//            loc1, loc2, textDeleted, textAdded);
		//    }
		//    return s;
		//}
	}

	public enum ChangeType {
		Add, Remove, Change
	}
}
