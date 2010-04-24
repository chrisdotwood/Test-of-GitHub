using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FsmReader {
	#region Enums

	public enum DataType {
		None = 0,
		Float = 1,
		ByteBlock = 2,
		PointerCoupling = 3,
		Object = 4,
		Particle = 5
	}

	[Flags]
	public enum Flags {
		Expanded = 0x01,
		HasOwner = 0x02,
		CppFunc = 0x04,
		Selected = 0x08,
		HideConnectors = 0x10,
		HideLabel = 0x20,
		FlagsExtended = 0x40,
		FlagsExtendedA = 0x80
	}

	[Flags]
	public enum FlagsExtended {
		ShowObject = 0x00000001,
		Selected = 0x00000002,
		FlexScript = 0x00000004,
		Null = 0x00000008,

		FunctionDisabled = 0x00000010,
		Keyword = 0x00000020,
		StateLocked = 0x00000040, // first used for port state-change masking
		Hidden = 0x00000080, // prevent user viewing

		Protected = 0x00000100, // prevent user editing
		HideShape = 0x00000200,
		ODTDerivative = 0x00000400,
		HideBase = 0x00000800,

		HideContent = 0x00001000,
		StatStag = 0x00002000,
		IndexCache = 0x00004000,
		MaintainArray = 0x00008000,

		DLLFunc = 0x00010000,
		CustomDisplay = 0x00020000,
		GlobalCPPFunc = 0x00040000,
		ExecutingNow = 0x00080000
	}

	#endregion

	public class Treenode : Composite, INotifyPropertyChanged {
		public Treenode() {
			NodeChildren = new Collection<Treenode>();
			DataChildren = new Collection<Treenode>();
		}

		private Treenode dummy;
		private Treenode dataDummy;



		#region Properties

		public byte Version {
			get;
			set;
		}

		private Flags flags = Flags.FlagsExtended;
		public Flags Flags {
			get {
				return flags;
			}
			set {
				if (value != flags) {
					flags = value;
					FirePropertyChanged("Flags");
				}
			}
		}
		private bool isData = false;

		private string title = "";
		public string Title {
			get {
				return title;
			}
			set {
				if (value != title) {
					title = value;
					FirePropertyChanged("Title");
				}
			}
		}
		private object data;

		private DataType dataType;
		public DataType DataType {
			get {
				return dataType;
			}
			set {
				if (value != dataType) {
					dataType = value;
					FirePropertyChanged("DataType");
				}
			}
		}

		private void FirePropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public uint Branch { get; set; }

		public FlagsExtended FlagsExtended { get; set; }

		private uint indexCache;
		public uint IndexCache {
			get {
				return indexCache;
			}
			set {
				indexCache = value;
			}
		}

		public uint CppType { get; set; }

		public uint Size { get; set; }

		//public List<Treenode> dataChildren = new List<Treenode>();
		public Collection<Treenode> NodeChildren {
			get;
			private set;
		}

		public Collection<Treenode> DataChildren {
			get;
			private set;
		}

		public override ReadOnlyCollection<Composite> Children {
			get {
				List<Composite> c = NodeChildren.ToList<Composite>();
				c.AddRange(DataChildren);

				//TODO This needs to be implemented in a more performant manner
				return new ReadOnlyCollection<Composite>(c);
			}
		}

		public Treenode Parent { get; set; }

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


		#endregion

		public string DataAsString() {
			if (data != null) {
				return data.ToString();
			} else {
				return "";
			}
		}

		public Treenode this[int index] {
			get {
				if (index >= NodeChildren.Count) return null;
				return NodeChildren[index];
			}
		}

		public Treenode this[string childName] {
			get {
				return NodeChildren.FirstOrDefault(s => s.Title == childName);
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

		public int NodeNumber { get; set; }

		#region Serialisation

		public static Treenode Read(Stream stream) {
			int count = -1;
			return _Read(stream, ref count);
		}

		private static Treenode _Read(Stream stream, ref int count) {
			BinaryReader reader = new BinaryReader(stream);

			Treenode ret = new Treenode();
			ret.NodeNumber = count++;

			ret.Version = reader.ReadByte();
			ret.Flags = (Flags)Enum.ToObject(typeof(DataType), reader.ReadByte());

			int len = reader.ReadInt32();
			if (len > 0) {
				ret.Title = new string(reader.ReadChars((int)len)).Replace("\0", "");
			}

			ret.DataType = (DataType)Enum.ToObject(typeof(DataType), reader.ReadUInt32());

			ret.Branch = reader.ReadUInt32();

			if ((ret.Flags & Flags.FlagsExtended) == Flags.FlagsExtended) {
				ret.FlagsExtended = (FlagsExtended)Enum.ToObject(typeof(FlagsExtended), reader.ReadUInt32());
			}

			if ((ret.FlagsExtended & FlagsExtended.ODTDerivative) == FlagsExtended.ODTDerivative) {
				ret.CppType = reader.ReadUInt32();
			}

			if ((ret.FlagsExtended & FlagsExtended.IndexCache) == FlagsExtended.IndexCache) {
				ret.IndexCache = reader.ReadUInt32();
			}

			if (ret.DataType == DataType.ByteBlock) {
				len = reader.ReadInt32();
				if (len > 0) {
					byte[] buf = new byte[len];
					reader.Read(buf, 0, len);

					ret.data = Encoding.UTF8.GetString(buf).Replace("\0", "");
				}
			} else if (ret.DataType == DataType.Float) {
				ret.data = reader.ReadDouble();
			} else if (ret.DataType == DataType.PointerCoupling) {
				ret.data = reader.ReadUInt32();
			}

			if (ret.Branch > 0) {
				ret.dummy =  _Read(stream, ref count);

				uint dataNodesToRead = ret.dummy.Size;
				
				while (dataNodesToRead-- > 0) {
					Treenode node = _Read(stream, ref count);
					node.Parent = ret;

					if (ret.DataType == DataType.Object) {
						node.isData = true;
						ret.DataChildren.Add(node);
					} else {
						ret.NodeChildren.Add(node);
					}
				}
			}

			if (ret.DataType == DataType.Object) {
				ret.dataDummy = _Read(stream, ref count);

				uint nodesToRead = ret.dataDummy.Size;

				while (nodesToRead-- > 0) {
					Treenode node = _Read(stream, ref count);
					node.Parent = ret;

					if (ret.Branch > 0) {
						ret.NodeChildren.Add(node);
					} else {
						node.isData = true;
						ret.DataChildren.Add(node);
					}
				}
			} else if (ret.DataType == DataType.Particle) {
				Console.WriteLine("ERROR: Unknown datatype: " + ret.DataType.ToString() + " at " + stream.Position);
			}

			if ((ret.Flags & Flags.HasOwner) != Flags.HasOwner) {
				ret.Size = reader.ReadUInt32();
			}
			return ret;
		}

		public static void Write(Treenode root, Stream file) {
			BinaryWriter writer = new BinaryWriter(file);

			writer.Write(root.Version);
			writer.Write((byte)root.Flags);

			writer.Write(root.Title.Length + 1);
			writer.Write(Encoding.ASCII.GetBytes(root.Title + '\0'));

			writer.Write((uint)root.DataType);

			writer.Write(root.Branch);

			if ((root.Flags & Flags.FlagsExtended) == Flags.FlagsExtended) {
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
				writer.Write((uint)root.data);
			}

			if (root.Branch > 0) {
				Collection<Treenode> children = root.DataType == DataType.Object ? root.DataChildren : root.NodeChildren;

				root.dummy.Size = (uint)children.Count;
				Write(root.dummy, file);

				foreach (Treenode child in children) {
					Treenode.Write(child, file);
				}
			}

			if (root.DataType == DataType.Object) {
				Collection<Treenode> children;

				if (root.Branch == 0) {
					children = root.DataChildren;
				} else {
					children = root.NodeChildren;
				}

				root.dataDummy.Size = (uint)children.Count;
				Write(root.dataDummy, file);

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

	}
}
