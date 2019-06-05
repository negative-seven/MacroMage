using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.Palette
{
	public class NesColorMapping
	{
		public const int COLOR_COUNT = 0x40;



		private Color[] argbColors;



		private NesColorMapping()
		{
			argbColors = new Color[COLOR_COUNT];
		}

		public static NesColorMapping FromFile(string path)
		{
			var result = new NesColorMapping();

			var bytes = File.ReadAllBytes(path);
			for (int colorId = 0; colorId < COLOR_COUNT; colorId++)
			{
				result.argbColors[colorId] = Color.FromArgb(
					bytes[colorId * 3 + 0],
					bytes[colorId * 3 + 1],
					bytes[colorId * 3 + 2]
				);
			}

			return result;
		}

		public Color Get(int colorId)
		{
			return argbColors[colorId];
		}
	}
}
