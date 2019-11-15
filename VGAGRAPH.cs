using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace wolfread
{
	public class VGAGRAPH
	{
		private const int FontCharacterCount = 256;

		[StructLayout(LayoutKind.Sequential)]
		private struct PicInfo
		{
			public ushort Width;
			public ushort Height;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct FontInfo
		{
			public short height;

			[MarshalAs(UnmanagedType.ByValArray,
				ArraySubType = UnmanagedType.I2,
				SizeConst = FontCharacterCount)]
			public short[] location;

			[MarshalAs(UnmanagedType.ByValArray,
				ArraySubType = UnmanagedType.U1,
				SizeConst = FontCharacterCount)]
			public byte[] width;
		}

		public struct Definitions
		{
			public string VGAHEAD;
			public string VGADICT;
			public string VGAGRAPH;

			public int BLOCK;
			public int MASKBLOCK;

			public int NUMCHUNKS;
			public int NUMFONT;
			public int NUMFONTM;
			public int NUMPICS;
			public int NUMPICM;
			public int NUMSPRITES;
			public int NUMTILE8;
			public int NUMTILE8M;
			public int NUMTILE16;
			public int NUMTILE16M;
			public int NUMTILE32;
			public int NUMTILE32M;
			public int NUMEXTERNS;

			public int STRUCTPIC;
			public int STARTFONT;
			public int STARTFONTM;
			public int STARTPICS;
			public int STARTPICM;
			public int STARTSPRITES;
			public int STARTTILE8;
			public int STARTTILE8M;
			public int STARTTILE16;
			public int STARTTILE16M;
			public int STARTTILE32;
			public int STARTTILE32M;
			public int STARTEXTERNS;

			/// <summary>
			/// Wolfenstein 3D, Shareware
			/// </summary>
			public static Definitions ForWL1()
			{
				Definitions result = new Definitions();

				result.VGAHEAD = "VGAHEAD.WL1";
				result.VGADICT = "VGADICT.WL1";
				result.VGAGRAPH = "VGAGRAPH.WL1";

				result.BLOCK = 64;
				result.MASKBLOCK = 128;

				result.NUMCHUNKS = 556;
				result.NUMFONT = 2;
				result.NUMFONTM = 0;
				result.NUMPICS = 136;
				result.NUMPICM = 0;
				result.NUMSPRITES = 0;
				result.NUMTILE8 = 72;
				result.NUMTILE8M = 0;
				result.NUMTILE16 = 144;
				result.NUMTILE16M = 270;
				result.NUMTILE32 = 0;
				result.NUMTILE32M = 0;
				result.NUMEXTERNS = 2;

				result.STRUCTPIC = 0;

				result.STARTFONT = 1;
				result.STARTFONTM = 3;
				result.STARTPICS = 3;
				result.STARTPICM = 139;
				result.STARTSPRITES = 139;
				result.STARTTILE8 = 139;
				result.STARTTILE8M = 140;
				result.STARTTILE16 = 140;
				result.STARTTILE16M = 284;
				result.STARTTILE32 = 554;
				result.STARTTILE32M = 554;
				result.STARTEXTERNS = 554;

				return result;
			}

			/// <summary>
			/// Wolfenstein 3D, Registered
			/// </summary>
			public static Definitions ForWL6()
			{
				Definitions result = new Definitions();

				result.VGAHEAD = "VGAHEAD.WL6";
				result.VGADICT = "VGADICT.WL6";
				result.VGAGRAPH = "VGAGRAPH.WL6";

				result.BLOCK = 64;
				result.MASKBLOCK = 128;

				result.NUMCHUNKS = 149;
				result.NUMFONT = 2;
				result.NUMFONTM = 0;
				result.NUMPICS = 132;
				result.NUMPICM = 0;
				result.NUMSPRITES = 0;
				result.NUMTILE8 = 72;
				result.NUMTILE8M = 0;
				result.NUMTILE16 = 0;
				result.NUMTILE16M = 0;
				result.NUMTILE32 = 0;
				result.NUMTILE32M = 0;
				result.NUMEXTERNS = 13;

				result.STRUCTPIC = 0;

				result.STARTFONT = 1;
				result.STARTFONTM = 3;
				result.STARTPICS = 3;
				result.STARTPICM = 135;
				result.STARTSPRITES = 135;
				result.STARTTILE8 = 135;
				result.STARTTILE8M = 136;
				result.STARTTILE16 = 136;
				result.STARTTILE16M = 136;
				result.STARTTILE32 = 136;
				result.STARTTILE32M = 136;
				result.STARTEXTERNS = 136;

				return result;
			}

			/// <summary>
			/// Wolfenstein 3D, Spear of Destiny
			/// </summary>
			public static Definitions ForSOD()
			{
				Definitions result = new Definitions();

				result.VGAHEAD = "VGAHEAD.SOD";
				result.VGADICT = "VGADICT.SOD";
				result.VGAGRAPH = "VGAGRAPH.SOD";

				result.BLOCK = 64;
				result.MASKBLOCK = 128;

				result.NUMCHUNKS = 169;
				result.NUMFONT = 2;
				result.NUMFONTM = 0;
				result.NUMPICS = 147;
				result.NUMPICM = 0;
				result.NUMSPRITES = 0;
				result.NUMTILE8 = 72;
				result.NUMTILE8M = 0;
				result.NUMTILE16 = 0;
				result.NUMTILE16M = 0;
				result.NUMTILE32 = 0;
				result.NUMTILE32M = 0;
				result.NUMEXTERNS = 18;

				result.STRUCTPIC = 0;

				result.STARTFONT = 1;
				result.STARTFONTM = 3;
				result.STARTPICS = 3;
				result.STARTPICM = 150;
				result.STARTSPRITES = 150;
				result.STARTTILE8 = 150;
				result.STARTTILE8M = 151;
				result.STARTTILE16 = 151;
				result.STARTTILE16M = 151;
				result.STARTTILE32 = 151;
				result.STARTTILE32M = 151;
				result.STARTEXTERNS = 151;

				return result;
			}
		}

		byte[] _huffDictionary;
		private uint[] _chunkOffsets;
		private Dictionary<int, byte[]> _chunkData;
		private PicInfo[] _picInfos;

		Definitions _defs;

		public VGAGRAPH(Definitions defs)
		{
			_defs = defs;
			_chunkData = new Dictionary<int, byte[]>();

			ReadOffsets(_defs.VGAHEAD);
			ReadDictionary(_defs.VGADICT);
			ReadChunks(_defs.VGAGRAPH);

			_chunkOffsets = null;
			_huffDictionary = null;

			if (_chunkData.ContainsKey(_defs.STRUCTPIC))
			{
				_chunkData.Remove(_defs.STRUCTPIC);
			}
		}

		public string GetB800Text(int index)
		{
			byte[] data = GetChunkData(index);
			return GetB800Text(data);
		}

		public Palette GetPalette(int index)
		{
			byte[] data = GetChunkData(index);
			return new Palette(data);
		}

		public Bitmap GetFont(int index)
		{
			byte[] data = GetChunkData(index);
			FontInfo font = new FontInfo();

			int fontSize = Marshal.SizeOf(typeof(FontInfo));

			GCHandle handle = 
				GCHandle.Alloc(data, GCHandleType.Pinned);

			try
			{
				font = (FontInfo)Marshal.PtrToStructure(
					handle.AddrOfPinnedObject(), typeof(FontInfo));
			}
			finally
			{
				handle.Free();
			}

			int numChars = font.width.Length;

			int totalWidth = 0;

			for (int i = 0; i < numChars; i++)
			{
				totalWidth += font.width[i];
			}

			Bitmap result = new Bitmap(
				totalWidth, font.height, PixelFormat.Format32bppArgb);

			BitmapData bitmapData =
				result.LockBits(
					new Rectangle(Point.Empty, result.Size),
					ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			byte[] newBitmapData = new byte[bitmapData.Stride * bitmapData.Height];

			int w = 0;

			for (int i = 0; i < numChars; i++)
			{
				int srcIdx = font.location[i];

				if (srcIdx > 0)
				{
					for (int x = 0; x < font.width[i]; x++)
					{
						for (int y = 0; y < font.height; y++)
						{
							byte bits = data[srcIdx + (y * font.width[i])];

							int dstIdx =
								(((y * bitmapData.Stride) + (w * sizeof(int)))) + (x * sizeof(int));

							byte val = (byte)(bits != 0 ? 0xFF : 0x00);

							newBitmapData[dstIdx++] = val;
							newBitmapData[dstIdx++] = val;
							newBitmapData[dstIdx++] = val;
							newBitmapData[dstIdx++] = val;
						}

						srcIdx++;
					}
				}

				w += font.width[i];
			}

			Marshal.Copy(newBitmapData, 0, bitmapData.Scan0, newBitmapData.Length);

			result.UnlockBits(bitmapData);

			return result;
		}

		public Bitmap GetPicture(Palette pal, int index)
		{
			byte[] data = GetChunkData(index + _defs.STARTPICS);
			PicInfo info = _picInfos[index];

			Bitmap result = new Bitmap(
				info.Width, info.Height, PixelFormat.Format8bppIndexed);

			ColorPalette cp = result.Palette;
			Color[] palette = cp.Entries;

			for (int i = 0; i < palette.Length; i++)
			{
				palette[i] = pal.Colors[i];
			}

			BitmapData bitmapData =
				result.LockBits(
					new Rectangle(Point.Empty, new Size(result.Width, result.Height)),
					ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

			byte[] newBitmapData = new byte[bitmapData.Stride * bitmapData.Height];

			for (int y = 0; y < result.Height; y++)
			{
				for (int x = 0; x < result.Width; x++)
				{
					newBitmapData[(y * bitmapData.Stride) + x] =
						data[(y * (info.Width >> 2) + (x >> 2)) + (x & 3) * (info.Width >> 2) * info.Height];
				}
			}

			Marshal.Copy(newBitmapData, 0, bitmapData.Scan0, newBitmapData.Length);

			result.UnlockBits(bitmapData);

			result.Palette = cp;

			return result;
		}

		private void ReadPicInfos()
		{
			int picInfoSize = Marshal.SizeOf(typeof(PicInfo));

			byte[] picInfoChunk = _chunkData[_defs.STRUCTPIC];

			_picInfos = new PicInfo[
				picInfoChunk.Length / picInfoSize];

			GCHandle handle = GCHandle.Alloc(picInfoChunk, GCHandleType.Pinned);

			try
			{
				IntPtr pStruct = handle.AddrOfPinnedObject();

				for (int i = 0; i < _picInfos.Length; i++)
				{
					_picInfos[i] = (PicInfo)Marshal.PtrToStructure(
						pStruct, typeof(PicInfo));

					pStruct = IntPtr.Add(pStruct, picInfoSize);
				}
			}
			finally
			{
				handle.Free();
			}
		}

		private void ReadDictionary(string fileName)
		{
			using (FileStream fs = new FileStream(
				fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				fs.Seek(0, SeekOrigin.End);
				long fsLength = fs.Position;

				_huffDictionary = new byte[fsLength];

				fs.Seek(0, SeekOrigin.Begin);
				fs.Read(_huffDictionary, 0, _huffDictionary.Length);
			}
		}

		private long GetChunkOffset(int idx)
		{
			long result = 0;

			if (_chunkOffsets[idx] == 0x00FFFFFF)
			{
				result = -1;
			}
			else
			{
				result = _chunkOffsets[idx];
			}

			return result;
		}

		private int GetChunkImplicitLength(int idx)
		{
			int result = -1;

			if (idx >= _defs.STARTTILE8 && idx < _defs.STARTEXTERNS)
			{
				if (idx < _defs.STARTTILE8M)
				{
					result = _defs.BLOCK * _defs.NUMTILE8;
				}
				else if (idx < _defs.STARTTILE16)
				{
					result = _defs.MASKBLOCK * _defs.NUMTILE8M;
				}
				else if (idx < _defs.STARTTILE16M)
				{
					result = _defs.BLOCK * 4;
				}
				else if (idx < _defs.STARTTILE32)
				{
					result = _defs.MASKBLOCK * 4;
				}
				else if (idx < _defs.STARTTILE32M)
				{
					result = _defs.BLOCK * 16;
				}
				else
				{
					result = _defs.MASKBLOCK * 16;
				}
			}

			return result;
		}

		private byte[] ReadChunkData(Stream s, int index)
		{
			int next = index + 1;

			long chunkStartOffset = GetChunkOffset(index);
			long chunkEndOffset = 0;

			byte[] result = null;

			while ((chunkEndOffset = GetChunkOffset(next)) == -1)
			{
				next++;
			}

			long dataLength = (chunkEndOffset - chunkStartOffset - sizeof(int));

			if (dataLength > 0)
			{
				s.Seek(chunkStartOffset, SeekOrigin.Begin);
				
				int inflatedLength = GetChunkImplicitLength(index);

				if (inflatedLength == -1)
				{
					byte[] intBuffer = new byte[sizeof(int)];
					s.Read(intBuffer, 0, intBuffer.Length);
					inflatedLength = BitConverter.ToInt32(intBuffer, 0);
				}

				if (inflatedLength > 0)
				{
					result = new byte[dataLength];
					s.Read(result, 0, result.Length);

					result = Huffman.Inflate(result, inflatedLength, _huffDictionary);
				}
			}

			return result;
		}

		public byte[] GetChunkData(int index)
		{
			byte[] result = 
				_chunkData.ContainsKey(index) ? _chunkData[index] : null;

			return result;
		}

		private string GetB800Text(byte[] data)
		{
			StringBuilder sb = new StringBuilder();

			int srcIdx = 7;

			for (int y = 0; y < 25; y++)
			{
				for (int x = 0; x < 80; x++)
				{
					sb.Append((char)data[srcIdx++]);
					srcIdx++;
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}

		private void ReadOffsets(string fileName)
		{
			using (FileStream fs = new FileStream(
				fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				fs.Seek(0, SeekOrigin.End);
				long fsLength = fs.Position;

				long numOffsets = (fsLength / 3);
				_chunkOffsets = new uint[numOffsets];

				fs.Seek(0, SeekOrigin.Begin);

				byte[] buffer = new byte[sizeof(uint)];

				for (int i = 0; i < _chunkOffsets.Length; i++)
				{
					int offset = i * 3;

					fs.Seek(offset, SeekOrigin.Begin);
					fs.Read(buffer, 0, buffer.Length);

					_chunkOffsets[i] =
						BitConverter.ToUInt32(buffer, 0) & 0x00FFFFFF;
				}
			}
		}

		private void ReadChunks(string fileName)
		{
			using (FileStream fs = new FileStream(
				fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				for (int i = 0; i < _defs.NUMCHUNKS; i++)
				{
					long offs = GetChunkOffset(i);

					if (offs > -1)
					{
						byte[] data = ReadChunkData(fs, i);
						_chunkData.Add(i, data);
					}
				}

				ReadPicInfos();
			}
		}
	}
}
