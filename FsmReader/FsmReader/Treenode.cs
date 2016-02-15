using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace FsmReader {


	public class Treenode : Composite, INotifyPropertyChanged {
		private Treenode childSizeNode;
		private Treenode dataSizeNode;

		#region Properties
		private Flags _Version;

		private string title = "";
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				if (value != title) {
					title = value;
					FirePropertyChanged("Title");
				}
			}
		}
		private object data;

		private DataType dataType;
		public DataType DataType
		{
			get
			{
				return dataType;
			}
			set
			{
				if (value != dataType) {
					dataType = value;
					FirePropertyChanged("DataType");
				}
			}
		}

		private Flags flags = Flags.ExtendedFlags;
		public Flags Flags
		{
			get
			{
				return flags;
			}
			set
			{
				if (value != flags) {
					flags = value;
					FirePropertyChanged("Flags");
				}
			}
		}

		private FlagsExtended flagsExtended;
		public FlagsExtended FlagsExtended
		{
			get
			{
				return flagsExtended;
			}
			set
			{
				if (value != flagsExtended) {
					flagsExtended = value;
					FirePropertyChanged("FlagsExtended");
				}
			}
		}

		public byte Branch;

		public uint IndexCache;

		public uint CppType;

		public uint Size;

		private List<Treenode> _NodeChildren = new List<Treenode>();

		public IEnumerable<Treenode> NodeChildren
		{
			get
			{
				return _NodeChildren;
			}
		}

		private List<Treenode> _DataChildren = new List<Treenode>();
		public IEnumerable<Treenode> DataChildren
		{
			get
			{
				return _DataChildren;
			}
		}

		public override ReadOnlyCollection<Composite> Children
		{
			get
			{
				List<Composite> c = DataChildren.ToList<Composite>();
				c.AddRange(NodeChildren);

				//TODO This needs to be implemented in a more performant manner
				return new ReadOnlyCollection<Composite>(c);
			}
		}

		public Treenode Parent;

		public string FullPath
		{
			get
			{
				string path = "/" + Title;

				Treenode up = Parent;
				while (up != null) {
					path = "/" + up.Title + path;
					up = up.Parent;
				}
				return path;
			}
		}

		public object Data
		{
			get
			{
				return data;
			}
			set
			{
				if (value != data) {
					data = value;
					FirePropertyChanged("Data");
					FirePropertyChanged("DataAsString");
					FirePropertyChanged("DataAsDouble");
				}
			}
		}

		public string DataAsString
		{
			get
			{
				if (data != null) {
					return data.ToString();
				} else {
					return "";
				}
			}
			set
			{
				Debug.Assert(DataType == FsmReader.DataType.ByteBlock);

				if ((string)data != value) {
					data = value;
					FirePropertyChanged("DataAsString");
				}
			}
		}

		public double DataAsDouble
		{
			get
			{
				Debug.Assert(DataType == FsmReader.DataType.Float);

				return (double)data;
			}
			set
			{
				Debug.Assert(DataType == FsmReader.DataType.Float);

				if ((double)data != value) {
					data = value;
					FirePropertyChanged("DataAsDouble");
				}
			}
		}

		#endregion

		private void FirePropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public Treenode this[int index]
		{
			get
			{
				if (index >= _NodeChildren.Count) return null;
				return _NodeChildren[index];
			}
		}

		public Treenode this[string childName]
		{
			get
			{
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

		public static Treenode Read(Stream stream) {
			int count = 0;

			// Key is node number of target, value is source
			SortedList<int, List<Treenode>> couplings = new SortedList<int, List<Treenode>>();

			SortedList<int, Treenode> nodeArray = new SortedList<int, Treenode>();
			// Load the tree

			Treenode root = _Read(stream, ref count);

			Console.WriteLine("Read " + count + " nodes");

			// Connect the couplings
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



		private static Treenode _Read(Stream stream, ref int count) {
			BinaryReader reader = new BinaryReader(stream);

			Treenode ret = new Treenode();

			count++;

			ret.Flags = (Flags)reader.ReadByte();
			ret.DataType = (DataType)reader.ReadByte();

			if ((ret.Flags & Flags.ExtendedFlags) == Flags.ExtendedFlags) {
				ret.FlagsExtended = (FlagsExtended)reader.ReadInt32();
			}

			int byteBlockSize = reader.ReadInt32();

			if (byteBlockSize > 0) {
				ret.Title = reader.ReadNullTerminatedString((int)byteBlockSize);
			}

			if (ret.DataType == DataType.Float) {
				double value = reader.ReadDouble();
			} else if (ret.DataType == DataType.ByteBlock) {
				int stringLength = reader.ReadInt32();

				ret.Data = reader.ReadNullTerminatedString(stringLength);
			} else if (ret.DataType == DataType.Object) {
				// TODO Still don't know the purpose of this node
				Treenode node = _Read(stream, ref count);

				// The number of object children access with > rather then the normal +
				int numChildren = reader.ReadInt32();

				while (numChildren > 0) {
					Treenode child = _Read(stream, ref count);
					numChildren--;

					ret.AddDataChild(child);
				}
			} else if (ret.DataType == DataType.None || ret.DataType == DataType.Undefined) {
				// Do nothing
			} else if (ret.DataType == DataType.PointerCoupling) {
				int coupling = reader.ReadInt32();
			} else {
				throw new Exception("Data type was not recognised");
			}

			if ((ret.Flags & Flags.HasBranch) == Flags.HasBranch) {
				// TODO Still don't know the purpose of this node
				Treenode node = _Read(stream, ref count);

				int numChildren = reader.ReadInt32();

				while (numChildren > 0) {
					Treenode child = _Read(stream, ref count);
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

		public static void Write(Treenode root, Stream file) {
			BinaryWriter writer = new BinaryWriter(file);

			writer.Write((byte)root.Flags);
			writer.Write((byte)root.FlagsExtended);

			writer.Write(root.Title.Length + 1);
			writer.Write(Encoding.ASCII.GetBytes(root.Title + '\0'));

			writer.Write((uint)root.DataType);

			writer.Write((uint)root.Branch);

			if ((root.Flags & Flags.ExtendedFlags) == Flags.ExtendedFlags) {
				writer.Write((uint)root.FlagsExtended);
			}

			if ((root.FlagsExtended & FlagsExtended.ODTDerivative) == FlagsExtended.ODTDerivative) {
				writer.Write((uint)root.CppType);
			}

			if ((root.FlagsExtended & FlagsExtended.IndexCache) == FlagsExtended.IndexCache) {
				writer.Write((uint)root.IndexCache);
			}

			if (root.DataType == DataType.ByteBlock) {
				string str = (string)root.data;

				writer.Write(str.Length + 1);
				writer.Write(Encoding.ASCII.GetBytes(str + '\0'));
			} else if (root.DataType == DataType.Float) {
				writer.Write((double)root.data);
			} else if (root.DataType == DataType.PointerCoupling) {
				writer.Write((uint)(int)root.data);
			}

			if (root.Branch > 0) {
				IEnumerable<Treenode> children = root.DataType == DataType.Object ? root.DataChildren : root.NodeChildren;

				root.childSizeNode.Size = (uint)children.Count();
				Write(root.childSizeNode, file);

				foreach (Treenode child in children) {
					Treenode.Write(child, file);
				}
			}

			if (root.DataType == DataType.Object) {
				IEnumerable<Treenode> children;

				if (root.Branch == 0) {
					children = root.DataChildren;
				} else {
					children = root.NodeChildren;
				}

				root.dataSizeNode.Size = (uint)children.Count();
				Write(root.dataSizeNode, file);

				foreach (Treenode child in children) {
					Treenode.Write(child, file);
				}
			}

			if ((root.Flags & Flags.HasOwner) != Flags.HasOwner) {
				writer.Write(root.Size);
			}
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;


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
