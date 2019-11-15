using System;

namespace wolfread
{
	internal static class Carmack
	{
		private const byte TagNearPointer = 0xA7;
		private const byte TagFarPointer = 0xA8;

		private static void Copy(byte[] src, int srcIndex, byte[] dst, int dstIndex, int length)
		{
			while (length-- > 0)
			{
				dst[dstIndex++] = src[srcIndex++];
			}
		}

		public static byte[] Inflate(byte[] data)
		{
			byte[] result = null;

			if (data != null && data.Length > 0)
			{
				byte[] buffer = new byte[sizeof(ushort)];

				int inPtr = 0;
				int outPtr = 0;

				ushort length = BitConverter.ToUInt16(data, inPtr);
				inPtr += sizeof(ushort);

				if (length > 0)
				{
					result = new byte[length];

					while ((inPtr < data.Length) && (outPtr < result.Length))
					{
						Copy(data, inPtr, buffer, 0, buffer.Length);
						inPtr += sizeof(ushort);

						int count = buffer[0];
						int offset = 0;

						bool copy = false;

						if (buffer[1] == TagNearPointer)
						{
							if (count > 0)
							{
								offset = data[inPtr++];
								Copy(result,
									outPtr - (offset * sizeof(ushort)),
									result,
									outPtr,
									(count * sizeof(ushort)));
								outPtr += (sizeof(ushort) * count);
							}
							else
							{
								buffer[0] = data[inPtr++];
								copy = true;
							}
						}
						else if (buffer[1] == TagFarPointer)
						{
							if (count > 0)
							{
								offset = BitConverter.ToUInt16(data, inPtr);
								inPtr += sizeof(ushort);
								Copy(result,
									(offset * sizeof(ushort)),
									result,
									outPtr,
									(count * sizeof(ushort)));
								outPtr += (sizeof(ushort) * count);
							}
							else
							{
								buffer[0] = data[inPtr++];
								copy = true;
							}
						}
						else
						{
							copy = true;
						}
						
						if (copy)
						{
							Copy(buffer, 0, result, outPtr, buffer.Length);
							outPtr += sizeof(ushort);
						}
					}
				}
			}

			return result;
		}
	}
}
