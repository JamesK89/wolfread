using System;
using System.Runtime.InteropServices;

namespace wolfread
{
	internal static class Huffman
	{
		public static int RootNodeIndex = 254;

		[StructLayout(LayoutKind.Sequential)]
		public struct Node
		{
			public ushort Left;
			public ushort Right;
		}

		public static byte[] Inflate(byte[] data, int length, Node[] nodes)
		{
			byte[] result = null;

			if (length > 0 &&
				data != null && data.Length > 0 &&
				nodes != null && nodes.Length > 0)
			{
				int srcPtr = 0;
				int dstPtr = 0;
				int nodePtr = RootNodeIndex;

				byte val = data[srcPtr++];
				ushort nodeval = 0;
				byte mask = 0x01;

				result = new byte[length];

				while (true)
				{
					if ((val & mask) == 0)
					{
						nodeval = nodes[nodePtr].Left;
					}
					else
					{
						nodeval = nodes[nodePtr].Right;
					}

					if (mask == 0x80)
					{
						val = data[srcPtr++];
						mask = 0x01;

						if (srcPtr >= data.Length)
						{
							break;
						}
					}
					else
					{
						mask <<= 1;
					}

					if (nodeval < 256)
					{
						result[dstPtr++] = (byte)(nodeval & 0xFF);
						nodePtr = RootNodeIndex;

						if (dstPtr >= result.Length)
						{
							break;
						}
					}
					else
					{
						nodePtr = (nodeval - 256);
					}
				}
			}

			return result;
		}

		public static byte[] Inflate(byte[] data, int length, byte[] dictionary)
		{
			Node[] nodes = ReadNodes(dictionary);
			return Inflate(data, length, nodes);
		}

		private static Node[] ReadNodes(byte[] dictionary)
		{
			Node[] result = null;

			if (dictionary != null && dictionary.Length > 0)
			{
				int nodeSize = Marshal.SizeOf(typeof(Node));

				GCHandle handle = 
					GCHandle.Alloc(dictionary, GCHandleType.Pinned);

				try
				{
					IntPtr pStruct = handle.AddrOfPinnedObject();
					result = new Node[dictionary.Length / nodeSize];

					for (int i = 0; i < result.Length; i++)
					{
						result[i] = 
							(Node)Marshal.PtrToStructure(pStruct, typeof(Node));
						pStruct = IntPtr.Add(pStruct, nodeSize);
					}
				}
				finally
				{
					handle.Free();
				}
			}

			return result;
		}
	}
}
