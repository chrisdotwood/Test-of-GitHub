using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diff;

namespace RunDiff {
	
	class Program {
		static void Main(string[] args) {
			string s1 = @"
This part of the
document has stayed the
same from version to
version.  It shouldn't
be shown if it doesn't
change.  Otherwise, that
would not be helping to
compress the size of the
changes.

This paragraph contains
text that is outdated.
It will be deleted in the
near future.

It is important to spell
check this dokument. On
the other hand, a
misspelled word isn't
the end of the world.
Nothing in the rest of
this paragraph needs to
be changed. Things can
be added after it.";

			string s2 = @"
This is an important
notice! It should
therefore be located at
the beginning of this
document!

This part of the
document has stayed the
same from version to
version.  It shouldn't
be shown if it doesn't
change.  Otherwise, that
would not be helping to
compress anything.

It is important to spell
check this document. On
the other hand, a
misspelled word isn't
the end of the world.
Nothing in the rest of
this paragraph needs to
be changed. Things can
be added after it.

This paragraph contains
important new additions
to this document.";

			string[] a = new string[] {@"
a
b
d
e"
			};

			string[] b = new string[] {@"
a
f
c
d"
			};

			string[] resultAb = new string[] { @"2a3
> c" };

			string[] resultBa = new string[] { @"3d2
< c" };
			


			DiffDocument d1 = new DiffDocument(a[0]);
			DiffDocument d2 = new DiffDocument(b[0]);

			Console.WriteLine("new Lcs().PrintDiff(d1, d2);");
			foreach (Change c in new Lcs().Diff(d1, d2)) {
				Console.WriteLine(c);
			}
			
			Console.WriteLine("new Lcs().PrintDiff(d2, d1);");
			foreach (Change c in new Lcs().Diff(d2, d1)) {
				Console.WriteLine(c);
			}
			Console.ReadKey();
		}
	}
}
