using System;
using System.IO;
using System.Runtime.InteropServices;

namespace wolfread
{
	public class Map
	{
		public const int PlaneCount = 3;
		public const int NameLength = 16;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		internal struct Header
		{
			[MarshalAs(UnmanagedType.ByValArray,
				ArraySubType = UnmanagedType.I4, SizeConst = PlaneCount)]
			public int[] Offset;

			[MarshalAs(UnmanagedType.ByValArray,
				ArraySubType = UnmanagedType.U2, SizeConst = PlaneCount)]
			public ushort[] Length;

			public ushort Width;
			public ushort Height;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NameLength)]
			public string Name;
		}

		public class Plane
		{
			public Plane(Map map)
			{
				Map = map;
				Width = map.Width;
				Height = map.Height;
				Data = new ushort[Height, Width];
			}

			public Map Map
			{
				get;
				private set;
			}

			public int Index
			{
				get;
				private set;
			}

			public int Width
			{
				get;
				private set;
			}

			public int Height
			{
				get;
				private set;
			}

			public ushort[,] Data
			{
				get;
				private set;
			}

			public ushort this[int y, int x]
			{
				get
				{
					return Data[y, x];
				}
			}
		}

		internal Map(Header header, ushort RLEWtag, Stream gameMapStream)
		{
			Name = header.Name;
			Width = header.Width;
			Height = header.Height;

			Planes = new Plane[PlaneCount];

			for (int i = 0; i < Planes.Length; i++)
			{
				Planes[i] = LoadPlane(
					header.Offset[i], header.Length[i],
					RLEWtag, gameMapStream);
			}
		}

		private Plane LoadPlane(
			int offset,
			int length,
			ushort RLEWtag,
			Stream gameMapStream)
		{
			Plane result = new Plane(this);

			if (offset > 0 && length > 0 && gameMapStream != null)
			{
				gameMapStream.Seek(offset, SeekOrigin.Begin);

				byte[] compressedData = new byte[length];
				gameMapStream.Read(compressedData, 0, compressedData.Length);

				compressedData = Carmack.Inflate(compressedData);

				byte[] decompressedData = RLEW.Inflate(
					RLEWtag,
					((Width * Height) * sizeof(ushort)) + sizeof(ushort),
					compressedData);

				for (int y = 0; y < Height; y++)
				{
					for (int x = 0; x < Width; x++)
					{
						int idx =
							sizeof(ushort) +
							((Width * y) * sizeof(ushort)) +
							(x * sizeof(ushort));

						result.Data[y, x] =
							BitConverter.ToUInt16(decompressedData, idx);
					}
				}
			}

			return result;
		}

		public ushort Width
		{
			get;
			private set;
		}

		public ushort Height
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public Plane[] Planes
		{
			get;
			private set;
		}
	}

	public class GAMEMAPS
	{
		private int _mapCount;

		private ushort _mapRLEWTag = 0x0000;
		private int[] _mapHeaderOffsets;

		private Map[] _maps;

		public GAMEMAPS(
			int mapCount,
			string mapHeadFileName,
			string gameMapsFileName)
		{
			_mapCount = mapCount;

			ReadMapHead(mapHeadFileName);
			ReadGameMaps(gameMapsFileName);
		}

		public Map this[int index]
		{
			get
			{
				return _maps[index];
			}
		}

		public int Count
		{
			get
			{
				return _maps.Length;
			}
		}

		private void ReadMapHead(string fileName)
		{
			using (FileStream fs = new FileStream(
				fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (BinaryReader reader = new BinaryReader(fs))
				{
					_mapRLEWTag = reader.ReadUInt16();
					_maps = new Map[_mapCount];
					_mapHeaderOffsets = new int[_mapCount];

					for (int i = 0; i < _mapHeaderOffsets.Length; i++)
					{
						_mapHeaderOffsets[i] = reader.ReadInt32();
					}
				}
			}
		}

		private void ReadGameMaps(string fileName)
		{
			using (FileStream fs = new FileStream(
				fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				byte[] buffer = new byte[Marshal.SizeOf(typeof(Map.Header))];
				GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

				try
				{
					for (int i = 0; i < _mapHeaderOffsets.Length; i++)
					{
						if (_mapHeaderOffsets[i] != 0)
						{
							fs.Seek(_mapHeaderOffsets[i], SeekOrigin.Begin);
							fs.Read(buffer, 0, buffer.Length);

							Map.Header header = 
								(Map.Header)Marshal.PtrToStructure(
									handle.AddrOfPinnedObject(), typeof(Map.Header));

							_maps[i] = new Map(
								header, _mapRLEWTag, fs);
						}
					}
				}
				finally
				{
					handle.Free();
				}
			}
		}
	}
}
