using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel.Tile
{
	public class Tile8 : Tile
	{
		public const int PIXEL_WIDTH = 8;
		public const int PIXEL_HEIGHT = 8;
		public override int CONST_PIXEL_WIDTH => PIXEL_WIDTH;
		public override int CONST_PIXEL_HEIGHT => PIXEL_HEIGHT;



		public override Level Level { get; set; }
		public override int Id { get; set; }

		public byte[][] Pattern { get; set; }



		public override Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			return AsBitmap(Level.Tile16Palettes.First(), colorMapping);
		}

		public Bitmap AsBitmap(NesPalette palette, NesColorMapping colorMapping)
		{
			var bitmap = new Bitmap(CONST_PIXEL_WIDTH, CONST_PIXEL_HEIGHT);

			for (int y = 0; y < CONST_PIXEL_HEIGHT; y++)
			{
				for (int x = 0; x < CONST_PIXEL_WIDTH; x++)
				{
					Color color;
					switch (Pattern[y][x])
					{
						case 0:
						default:
							color = palette.BgColor.AsArgbColor(colorMapping);
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
