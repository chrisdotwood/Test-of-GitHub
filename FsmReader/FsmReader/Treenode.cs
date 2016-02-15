using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;

namespace FsmReader {
	public class Treenode : Composite {
		private static readonly char[] TrimChars = new char[] { '\0' };

		#region Properties

		public string Title {
			get;
			set;
		}

		public DataType DataType {
			get;
			set;
		}

		public Flags Flags {
			get;
			set;
		}

		public FlagsExtended FlagsExtended {
			get;
			set;
		}

		private List<Treenode> _NodeChildren = new List<Treenode>();
		/// <summary>
		/// The child nodes that are accessed with the + symbol
		/// </summary>
		public IEnumerable<Treenode> NodeChildren {
			get {
				return _NodeChildren;
			}
		}

		private List<Treenode> _DataChildren = new List<Treenode>();
		/// <summary>
		/// The child nodes that are accessed with the > symbol
		/// </summary>
		public IEnumerable<Treenode> DataChildren {
			get {
				return _DataChildren;
			}
		}

		/// <summary>
		/// An aggregation of both the DataChildren and the NodeChildren. This is to enable searching etc. of the tree.
		/// </summary>
		public override IEnumerable<Composite> Children {
			get {
				return DataChildren.Union(NodeChildren);
			}
		}

		public Treenode Parent {
			get;
			set;
		}

		public string FullPath {
			get {
				string path = "/" + Title;

				Treenode up = Parent;
				while (up != null) {
					path = "/" + up.Title + path;
					up = up.Parent;
				}
				return path;
			}
		}

		private byte[] _Data;
		/// <summary>
		/// The raw data that is associated with this node
		/// </summary>
		public byte[] Data {
			get {
				return _Data;
			}
			private set {
				if (value != _Data) {
					_Data = value;
				}
			}
		}

		/// <summary>
		/// The raw node data converted to an ASCII string
		/// </summary>
		public string DataAsString {
			get {
				if (Data == null) return null;

				return Encoding.ASCII.GetString(Data).TrimEnd(TrimChars);
			}
		}

		/// <summary>
		/// The raw node data converted to a double
		/// </summary>
		public double DataAsDouble {
			get {
				Debug.Assert(DataType == FsmReader.DataType.Float);
				return BitConverter.ToDouble(Data, 0);
			}
			set {
				Debug.Assert(DataType == FsmReader.DataType.Float);

				Data = BitConverter.GetBytes(value);
			}
		}

		#endregion

		public Treenode this[string childName] {
			get {
				Treenode ret = NodeChildren.FirstOrDefault(s => s.Title == childName);
				if (ret != null) {
					return ret;
				} else {
					return DataChildren.FirstOrDefault(s => s.Title == childName);
				}
			}
		}

		/// <summary>
		/// Find a node from a path relative to a specified starting node.
		/// </summary>
		/// <param name="path">The path relative to the relativeTo node of the node to be returned.</param>
		/// <param name="relativeTo">The starting node for the search.</param>
		/// <returns>The Treenode if it is found, null otherwise.</returns>
		public static Treenode NodeFromPath(string path, Treenode relativeTo) {
			if (path == null) throw new ArgumentException("path");
			if (relativeTo == null) throw new ArgumentException("relativeTo");

			string[] parts = path.Split(new char[] { '/' });

			if (parts.Length < 1 || parts[1] != relativeTo.Title) {
				// The root node of the search differs
				return null;
			}

			Treenode node = relativeTo;

			// Start on 2 because of the leading forward slash giving an empty string and the starting nodes being the same
			for (int p = 2; p < parts.Length && node != null; p++) {
				node = node[parts[p]];
			}
			return node;
		}

		#region Serialisation

		/// <summary>
		/// Read the tree from a stream pointing to either a .t or a .fsm file. Flexsim 5.x format only.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		/// <remarks>
		/// The fsm file for Flexsim 5.x is 0x48 bytes of preamble followed by a gzipped serialised tree.
		/// </remarks>
		public static Treenode Read(Stream stream) {
			using (BinaryReader preambleReader = new BinaryReader(stream)) {
				byte[] preamble = preambleReader.ReadBytes(0x48);

				string magicText = "flexsimtree";

				string fileMagicText = Encoding.ASCII.GetString(preamble, 0, magicText.Length);

				if (magicText != fileMagicText) {
					throw new Exception("File doesn't appear to be a valid Flexsim .t or .fsm file");
				}

				byte fileVersion = preamble[0x1c];

				// 5.x is 0x03. 7.7 is 0x04
				if (fileVersion != 0x03) {
					throw new Exception("Only Flexsim 5.x files are currently supported");
				}

				// Key is node number of target, value is source
				SortedList<int, List<Treenode>> couplings = new SortedList<int, List<Treenode>>();

				SortedList<int, Treenode> nodeArray = new SortedList<int, Treenode>();

				// The remaining file is gzipped
				using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress)) {
					using (BinaryReader zipReader = new BinaryReader(zipStream)) {

						int numberOfNodesRead = 0;

						Treenode root = _Read(zipReader, ref numberOfNodesRead);

						// TODO Connect the couplings
						// May need to check bi-directionality of these
						//foreach (KeyValuePair<int, List<Treenode>> kv in couplings) {
						//    Treenode target = nodeArray[kv.Key];

						//    if (target != null) {
						//        foreach (Treenode source in kv.Value) {
						//            source.data = target;
						//        }
						//    } else {
						//        Console.WriteLine("WARNING: File may be corrupt. Target node not found for the following couplings:");
						//        foreach (Treenode source in kv.Value) {
						//            Console.WriteLine(source.FullPath);
						//        }
						//    }
						//}

						return root;
					}
				}
			}
		}

		private static Treenode _Read(BinaryReader reader, ref int count) {
			Treenode ret = new Treenode();

			count++;

			ret.Flags = (Flags)reader.ReadByte();
			ret.DataType = (DataType)reader.ReadByte();

			if ((ret.Flags & Flags.ExtendedFlags) == Flags.ExtendedFlags) {
				ret.FlagsExtended = (FlagsExtended)reader.ReadInt32();
			}

			int titleLength = reader.ReadInt32();

			if (titleLength > 0) {
				ret.Title = Encoding.ASCII.GetString(reader.ReadBytes(titleLength)).TrimEnd(Treenode.TrimChars);
			}

			if (ret.DataType == DataType.Float) {
				double value = reader.ReadDouble();
			} else if (ret.DataType == DataType.ByteBlock) {
				int stringLength = reader.ReadInt32();

				ret.Data = reader.ReadBytes(stringLength);
			} else if (ret.DataType == DataType.Object) {
				// If the node is an object then the next 4 bytes are an Int32 containing the
				// number of child data nodes that it has (those accessed through the > symbol
				// that is specific to objects). The next node to be read is the first of these
				// which may itself have child nodes. This is how the tree is serialised.

				// TODO Still don't know the purpose of this node
				Treenode node = _Read(reader, ref count);

				// The number of object children access with > rather then the normal +
				int numChildren = reader.ReadInt32();

				while (numChildren > 0) {
					Treenode child = _Read(reader, ref count);
					numChildren--;

					ret.AddDataChild(child);
				}
			} else if (ret.DataType == DataType.PointerCoupling) {
				int coupling = reader.ReadInt32();
			} else if (ret.DataType == DataType.None || ret.DataType == DataType.Undefined) {
				// Do nothing
			} else {
				throw new Exception("Data type was not recognised");
			}

			// If the HasBranch flag is set then this node is followed by an Int32 containing the
			// number of child nodes that it has. The next node to be read is the first of these
			// which may itself have child nodes. This is how the tree is serialised
			if ((ret.Flags & Flags.HasBranch) == Flags.HasBranch) {
				// TODO Still don't know the purpose of this node
				Treenode node = _Read(reader, ref count);

				int numChildren = reader.ReadInt32();

				while (numChildren > 0) {
					Treenode child = _Read(reader, ref count);
					numChildren--;

					ret.AddNodeChild(child);
				}
			}

			return ret;
		}

		private void AddDataChild(Treenode child) {
			_DataChildren.Add(child);
			child.Parent = this;
		}

		private void AddNodeChild(Treenode child) {
			_NodeChildren.Add(child);
			child.Parent = this;
		}

		#endregion

		public static void PrintTree(Treenode t, Stream s) {
			using (StreamWriter sw = new StreamWriter(s)) {
				_PrintTree(t, sw);
			}
		}

		private static void _PrintTree(Treenode t, StreamWriter sw) {
			sw.WriteLine(t.FullPath + " " + t.DataAsString);
			foreach (Treenode child in t.Children) {
				_PrintTree(child, sw);
			}
		}
	}
}
