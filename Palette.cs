using System;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace wolfread
{
	public class Palette
	{
		private static Regex WolfPalRGBRegEx = new Regex(
			@"RGB\s*\(\s*(?<r>[0-9]+)\s*,\s*(?<g>[0-9]+)\s*,\s*(?<b>[0-9]+)\s*\)\s*,?",
			RegexOptions.Singleline | RegexOptions.Compiled);

		private static int ColorCount = 256;

		public Palette(string fileName)
		{
			LoadPalette(fileName);
		}

		public Palette(Color[] colors)
		{
			InitializeColors();
			Array.Copy(
				colors, 0,
				Colors, 0,
				Math.Min(colors.Length, ColorCount));
		}

		public Palette(byte[] rgbColors)
		{
			InitializeColors();
			LoadPalette(rgbColors);
		}

		public Color[] Colors
		{
			get;
			private set;
		}

		public Color this[int idx]
		{
			get
			{
				return Colors[idx];
			}
			set
			{
				Colors[idx] = value;
			}
		}

		private void InitializeColors()
		{
			Colors = new Color[ColorCount];

			for (int i = 0; i < Colors.Length; i++)
			{
				Colors[i] = Color.Black;
			}
		}

		private void LoadPalette(byte[] rgbColors)
		{
			InitializeColors();

			int numColors = Math.Min(rgbColors.Length / 3, ColorCount);

			int srcIdx = 0;

			for (int i = 0; i < numColors; i++)
			{
				byte r = (byte)
					(Convert.ToInt32(rgbColors[srcIdx + 0]) * (255 / 63));
				byte g = (byte)
					(Convert.ToInt32(rgbColors[srcIdx + 0]) * (255 / 63));
				byte b = (byte)
					(Convert.ToInt32(rgbColors[srcIdx + 0]) * (255 / 63));

				Colors[i] = Color.FromArgb(
					0xFF, r, g, b);
				srcIdx += 3;
			}
		}

		private void LoadPalette(string fileName)
		{
			InitializeColors();

			using (FileStream fs =
				new FileStream(
					fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					string palContents = sr.ReadToEnd();

					MatchCollection mc = WolfPalRGBRegEx.Matches(palContents);

					int idx = 0;

					for (int i = 0; i < mc.Count; i++)
					{
						Match m = mc[i];

						if (m != null && m.Success)
						{
							byte r = (byte)
								(Convert.ToInt32(m.Groups["r"].Value) * (255 / 63));
							byte g = (byte)
								(Convert.ToInt32(m.Groups["g"].Value) * (255 / 63));
							byte b = (byte)
								(Convert.ToInt32(m.Groups["b"].Value) * (255 / 63));

							Colors[idx++] = Color.FromArgb(0xFF, r, g, b);

							if (idx >= Colors.Length)
							{
								break;
							}
						}
					}
				}
			}
		}
	}
}
