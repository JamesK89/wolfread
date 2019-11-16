using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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

		public uint Textures
		{
			get;
			private set;
		}

		public uint Sprites
		{
			get;
			private set;
		}

		public uint Sounds
		{
			get;
			private set;
		}

		public byte[][] Pages
		{
			get;
			private set;
		}

		public Bitmap GetTexture(Palette pal, uint index)
		{
			byte[] data = Pages[Textures + index];

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

		public Bitmap GetSprite(Palette pal, uint index)
		{
			byte[] data = Pages[Sprites + index];

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

			for (int i = 0; i < newBitmapData.Length; i++)
			{
				newBitmapData[i] = 0xFF;
			}

			short lftColumn = BitConverter.ToInt16(data, sizeof(short) * 0);
			short rgtColumm = BitConverter.ToInt16(data, sizeof(short) * 1);

			short[] instructions = new short[3];

			short[] offsets = new short[SpriteSize.Width];
			int offsetIndex = 0;
			int offsetsLength = (sizeof(short) * offsets.Length);

			while ((offsetsLength + (sizeof(short) * 2)) > data.Length)
			{
				offsetsLength -= sizeof(short);
			}

			Buffer.BlockCopy(data, sizeof(short) * 2, offsets, 0, offsetsLength);

			int pixel = (rgtColumm - lftColumn + 3) * sizeof(short);

			for (int x = lftColumn; x < rgtColumm; x++)
			{
				int idx = 0;

				do
				{
					Buffer.BlockCopy(
						data, offsets[offsetIndex] + (idx * sizeof(short)),
						instructions, 0, sizeof(short) * instructions.Length);

					if (instructions[0] != 0)
					{
						for (int y = (instructions[2] >> 1); y < (instructions[0] >> 1); y++)
						{
							newBitmapData[x + (y * SpriteSize.Height)] = data[pixel++];
						}

						idx += 3;
					}
				}
				while (instructions[0] != 0);

				offsetIndex++;
			}

			Marshal.Copy(newBitmapData, 0, bitmapData.Scan0, newBitmapData.Length);

			result.UnlockBits(bitmapData);

			result.Palette = cp;

			return result;
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

				Sprites = (uint)header.SpriteIndex;
				Sounds = (uint)header.SoundIndex;

				uint[] offsets = new uint[header.ChunksInFile + 1];
				ushort[] lengths = new ushort[header.ChunksInFile + 1];

				byte[] intBuffer = new byte[sizeof(uint)];
				byte[] shtBuffer = new byte[sizeof(ushort)];

				for (int i = 0; i < (offsets.Length - 1); i++)
				{
					stream.Read(intBuffer, 0, intBuffer.Length);
					offsets[i] = BitConverter.ToUInt32(intBuffer, 0);
				}

				for (int i = 0; i < (lengths.Length - 1); i++)
				{
					stream.Read(shtBuffer, 0, shtBuffer.Length);
					lengths[i] = BitConverter.ToUInt16(shtBuffer, 0);
				}

				offsets[offsets.Length - 1] = 0;
				lengths[offsets.Length - 1] = 0;

				ReadChunks(stream, offsets, lengths);
			}
		}

		private void ReadChunks(Stream stream, uint[] offsets, ushort[] lengths)
		{
			Pages = new byte[offsets.Length][];

			for (int i = 0; i < offsets.Length; i++)
			{
				uint offset = offsets[i];
				uint length = lengths[i];

				if (offset > 0 && length > 0)
				{
					Pages[i] = new byte[length];
					stream.Seek((long)offset, SeekOrigin.Begin);
					stream.Read(Pages[i], 0, (int)length);
				}
			}
		}
	}
}
