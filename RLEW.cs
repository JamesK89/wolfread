using System;
using System.IO;

namespace wolfread
{
	internal static class RLEW
	{
		public static byte[] Inflate(ushort tag, int length, byte[] data)
		{
			byte[] result = null;

			if (data != null && data.Length > 0)
			{
				result = new byte[length];

				int srcIdx = 0;
				int dstIdx = 0;

				while (dstIdx < length)
				{
					ushort w = GetWord(srcIdx++, data);

					if (w != tag)
					{
						byte[] n = BitConverter.GetBytes(w);

						for (int j = 0; j < n.Length; j++)
						{
							result[dstIdx++] = n[j];
						}
					}
					else
					{
						ushort count = GetWord(srcIdx++, data);
						ushort what = GetWord(srcIdx++, data);

						byte[] n = BitConverter.GetBytes(what);

						for (int i = 0; i < count; i++)
						{
							for (int j = 0; j < n.Length; j++)
							{
								result[dstIdx++] = n[j];
							}
						}
					}
				}
			}

			return result;
		}

		private static ushort GetWord(int index, byte[] data)
		{
			int idx = (index * sizeof(ushort));

			return BitConverter.ToUInt16(data, idx);
		}
	}
}
