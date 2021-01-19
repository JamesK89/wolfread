using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace wolfread
{
	public class VSWAP
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct PageHeader
		{
			public ushort ChunksInFile;
			public ushort SpriteIndex;
			public ushort SoundIndex;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DigiInfo
		{
			public int StartPage;
			public int Length;
		}

		public VSWAP(
			string fileName,
			Size textureSize,
			Size spriteSize)
		{
			TextureSize = textureSize;
			SpriteSize = spriteSize;
			ReadPageFile(fileName);
		}

		public VSWAP(string fileName)
			: this(fileName,
				   new Size(64, 64),
				   new Size(64, 64))
		{
		}

		public Size TextureSize
		{
			get;
			private set;
		}

		public Size SpriteSize
		{
			get;
			private set;
		}

		public uint TextureStart
		{
			get
			{
				return 0;
			}
		}

		public uint TextureCount
		{
			get
			{
				return SpriteStart;
			}
		}

		public uint SpriteStart
		{
			get;
			private set;
		}

		public uint SpriteCount
		{
			get
			{
				return (SoundStart - SpriteStart);
			}
		}

		public uint SoundStart
		{
			get;
			private set;
		}

		public uint SoundCount
		{
			get
			{
				return (uint)SoundInfo.Length;
			}
		}

		public DigiInfo[] SoundInfo
		{
			get;
			private set;
		}

		public byte[][] Pages
		{
			get;
			private set;
		}

		public byte[] GetTextureData(uint index)
		{
			return Pages[TextureStart + index];
		}

		public Bitmap GetTextureBitmap(Palette pal, uint index)
		{
			byte[] data = GetTextureData(index);

			Bitmap result = new Bitmap(
				TextureSize.Width, TextureSize.Height, PixelFormat.Format8bppIndexed);

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
						data[(x * TextureSize.Height) + y];
				}
			}

			Marshal.Copy(newBitmapData, 0, bitmapData.Scan0, newBitmapData.Length);

			result.UnlockBits(bitmapData);

			result.Palette = cp;

			return result;
		}

		public byte[] GetTextureRGB(Palette pal, uint index)
		{
			byte[] srcData = GetTextureData(index);
			byte[] dstData = new byte[TextureSize.Width * TextureSize.Height * 3];

			int stride = TextureSize.Width * 3;

			for (int y = 0; y < TextureSize.Height; y++)
			{
				for (int x = 0; x < TextureSize.Width; x++)
				{
					Color clr = pal[srcData[(x * TextureSize.Height) + y]];

					dstData[(y * stride) + (x * 3) + 0] = clr.R;
					dstData[(y * stride) + (x * 3) + 1] = clr.G;
					dstData[(y * stride) + (x * 3) + 2] = clr.B;
				}
			}

			return dstData;
		}

		public byte[] GetSpriteData(uint index)
		{
			byte[] srcData = Pages[SpriteStart + index];
			byte[] dstData = new byte[SpriteSize.Width * SpriteSize.Height];

			for (int i = 0; i < dstData.Length; i++)
			{
				dstData[i] = 0xFF;
			}

			short lftColumn = BitConverter.ToInt16(srcData, sizeof(short) * 0);
			short rgtColumm = BitConverter.ToInt16(srcData, sizeof(short) * 1);

			short[] instructions = new short[3];
			short[] offsets = new short[SpriteSize.Width];

			int offsetIndex = 0;
			int offsetsLength = (sizeof(short) * offsets.Length);

			while ((offsetsLength + (sizeof(short) * 2)) > srcData.Length)
			{
				offsetsLength -= sizeof(short);
			}

			Buffer.BlockCopy(srcData, sizeof(short) * 2, offsets, 0, offsetsLength);

			int pixel = (rgtColumm - lftColumn + 3) * sizeof(short);

			for (int x = lftColumn; x < rgtColumm; x++)
			{
				int idx = 0;

				do
				{
					Buffer.BlockCopy(
						srcData, offsets[offsetIndex] + (idx * sizeof(short)),
						instructions, 0, sizeof(short) * instructions.Length);

					if (instructions[0] != 0)
					{
						for (int y = (instructions[2] >> 1); y < (instructions[0] >> 1); y++)
						{
							dstData[y + (x * SpriteSize.Height)] = srcData[pixel++];
						}

						idx += 3;
					}
				}
				while (instructions[0] != 0);

				offsetIndex++;
			}

			return dstData;
		}

		public byte[] GetSpriteRGB(Palette pal, uint index)
		{
			byte[] srcData = GetSpriteData(index);
			byte[] dstData = new byte[SpriteSize.Width * SpriteSize.Height * 3];

			int stride = SpriteSize.Width * 3;

			for (int y = 0; y < SpriteSize.Height; y++)
			{
				for (int x = 0; x < SpriteSize.Width; x++)
				{
					Color clr = pal[srcData[(x * SpriteSize.Height) + y]];

					dstData[(y * stride) + (x * 3) + 0] = clr.R;
					dstData[(y * stride) + (x * 3) + 1] = clr.G;
					dstData[(y * stride) + (x * 3) + 2] = clr.B;
				}
			}

			return dstData;
		}

		public Bitmap GetSpriteBitmap(Palette pal, uint index)
		{
			byte[] data = GetSpriteData(index);

			Bitmap result = new Bitmap(
				SpriteSize.Width, SpriteSize.Height, PixelFormat.Format8bppIndexed);

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
						data[(x * SpriteSize.Height) + y];
				}
			}

			Marshal.Copy(newBitmapData, 0, bitmapData.Scan0, newBitmapData.Length);

			result.UnlockBits(bitmapData);

			result.Palette = cp;

			return result;
		}

		public byte[] GetSoundData(uint index)
		{
			int length = (int)SoundInfo[index].Length;
			byte[] data = new byte[length];

			int page = SoundInfo[index].StartPage;

			while (length > 0)
			{
				Buffer.BlockCopy(
					Pages[page], 0,
					data, (int)SoundInfo[index].Length - length,
					Pages[page].Length);

				length -= Pages[page].Length;

				page++;
			}

			return data;
		}

		private void ReadPageFile(string fileName)
		{
			using (FileStream stream =
					new FileStream(fileName,
						FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				int headerSize =
					Marshal.SizeOf(typeof(PageHeader));
				byte[] headerBuffer =
					new byte[headerSize];

				PageHeader header = new PageHeader();
				GCHandle handle = GCHandle.Alloc(headerBuffer, GCHandleType.Pinned);

				try
				{
					IntPtr pHeader = handle.AddrOfPinnedObject();
					stream.Read(headerBuffer, 0, headerBuffer.Length);
					header = (PageHeader)Marshal.PtrToStructure(
						pHeader, typeof(PageHeader));
				}
				finally
				{
					handle.Free();
				}

				SpriteStart = (uint)header.SpriteIndex;
				SoundStart = (uint)header.SoundIndex;

				uint[] offsets = new uint[header.ChunksInFile];
				ushort[] lengths = new ushort[header.ChunksInFile];

				byte[] intBuffer = new byte[sizeof(uint)];
				byte[] shtBuffer = new byte[sizeof(ushort)];

				for (int i = 0; i < offsets.Length; i++)
				{
					stream.Read(intBuffer, 0, intBuffer.Length);
					offsets[i] = BitConverter.ToUInt32(intBuffer, 0);
				}

				for (int i = 0; i < lengths.Length; i++)
				{
					stream.Read(shtBuffer, 0, shtBuffer.Length);
					lengths[i] = BitConverter.ToUInt16(shtBuffer, 0);
				}

				ReadChunks(stream, header, offsets, lengths);

				ReadSoundInfo();
			}
		}

		private void ReadChunks(Stream stream, PageHeader header, uint[] offsets, ushort[] lengths)
		{
			Pages = new byte[header.ChunksInFile][];

			for (int i = 0; i < header.ChunksInFile; i++)
			{
				if (offsets[i] > 0)
				{
					uint nextOffset = (i + 1) < header.ChunksInFile ?
						offsets[i + 1] : 0;

					if (nextOffset > 0)
					{
						Pages[i] = new byte[nextOffset - offsets[i]];
					}
					else
					{
						Pages[i] = new byte[lengths[i]];
					}

					if (Pages[i].Length > 0)
					{
						stream.Seek(offsets[i], SeekOrigin.Begin);
						stream.Read(Pages[i], 0, Pages[i].Length);
					}
				}
				else
				{
					Pages[i] = new byte[0];
				}
			}
		}

		private void ReadSoundInfo()
		{
			byte[] sndInfoPage = Pages[Pages.Length - 1];
			int numSounds = sndInfoPage.Length / (sizeof(short) * 2);

			List<DigiInfo> result = new List<DigiInfo>(numSounds);

			for (int i = 0; i < numSounds; i++)
			{
				int startPage = BitConverter.ToInt16(sndInfoPage, (i * 2) * sizeof(short));

				if (startPage >= (Pages.Length - 1))
				{
					numSounds = i;
					break;
				}
				else
				{
					startPage += (int)SoundStart;
				}

				int sndLength = 0;
				int endPage = 0;

				if (i < (numSounds - 1))
				{
					endPage =
					sndLength = BitConverter.ToInt16(
						sndInfoPage, ((i * 2 + 2) * sizeof(short)));

					if (endPage == 0 || (endPage + SoundStart) > (Pages.Length - 1))
					{
						endPage = (Pages.Length - 1);
					}
					else
					{
						endPage += (int)SoundStart;
					}
				}
				else
				{
					endPage = (Pages.Length - 1);
				}

				int size = 0;

				for (int p = startPage; p < endPage; p++)
				{
					size += Pages[p].Length;
				}

				DigiInfo info = new DigiInfo();

				info.StartPage = startPage;
				info.Length = size;

				result.Add(info);

			}

			SoundInfo = result.ToArray();
		}
	}
}
