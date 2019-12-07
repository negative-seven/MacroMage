using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel
{
	public class PatternTableTile : ILevelSegment
	{
		public const int PIXEL_WIDTH = 8;
		public const int PIXEL_HEIGHT = 8;



		public Level Level { get; set; }
		public int Id { get; set; }

		public int PixelWidth => PIXEL_WIDTH;
		public int PixelHeight => PIXEL_HEIGHT;

		public byte[][] Pattern { get; set; }

		


		public Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			return AsBitmap(Level.Tile16Palettes.First(), colorMapping);
		}

		public Bitmap AsBitmap(NesPalette palette, NesColorMapping colorMapping, bool alpha = false)
		{
			var bitmap = new Bitmap(PIXEL_WIDTH, PIXEL_HEIGHT);

			for (int y = 0; y < PIXEL_HEIGHT; y++)
			{
				for (int x = 0; x < PIXEL_WIDTH; x++)
				{
					Color color;
					switch (Pattern[y][x])
					{
						case 0:
						default:
							color = alpha ? Color.Transparent : palette.BgColor.AsArgbColor(colorMapping);
							break;

						case 1:
							color = palette.Color1.AsArgbColor(colorMapping);
							break;

						case 2:
							color = palette.Color2.AsArgbColor(colorMapping);
							break;

						case 3:
							color = palette.Color3.AsArgbColor(colorMapping);
							break;
					}

					bitmap.SetPixel(x, y, color);
				}
			}

			return bitmap;
		}
	}
}
