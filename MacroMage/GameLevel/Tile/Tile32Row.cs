using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel.Tile
{
	public class Tile32Row : ILevelSegment
	{
		public const int PIXEL_WIDTH = Tile32.PIXEL_WIDTH * TILES_COUNT;
		public const int PIXEL_HEIGHT = Tile32.PIXEL_HEIGHT;
		public const int UNMIRRORED_TILES_COUNT = 4;
		public const int TILES_COUNT = UNMIRRORED_TILES_COUNT * 2;
		public const int SHIFT_UNIT = Tile16.PIXEL_WIDTH;


		
		public Level Level { get; set; }
		public int Id { get; set; }

		public int PixelWidth => PIXEL_WIDTH;
		public int PixelHeight => PIXEL_HEIGHT;

		public List<int> TileIds { get; set; }
		public int Shift { get; set; }


		
		public Tile32 GetTile(int index)
		{
			return Level.Tile32s[TileIds[index]];
		}

		public Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			var bitmap = new Bitmap(PIXEL_WIDTH, PIXEL_HEIGHT);
			var graphics = Graphics.FromImage(bitmap);

			for (var tileIndex = 0; tileIndex < UNMIRRORED_TILES_COUNT; tileIndex++)
			{
				var tile = GetTile(tileIndex);
				graphics.DrawImage(
					tile.AsBitmap(colorMapping),
					new Point(tileIndex * Tile32.PIXEL_WIDTH + Shift * SHIFT_UNIT, 0)
				);
				graphics.DrawImage(
					tile.GetMirror().AsBitmap(colorMapping),
					new Point((TILES_COUNT - 1 - tileIndex) * Tile32.PIXEL_WIDTH + Shift * SHIFT_UNIT, 0)
				);

				// account for tiles that wrap around to the left side
				graphics.DrawImage(
					tile.AsBitmap(colorMapping),
					new Point(tileIndex * Tile32.PIXEL_WIDTH + Shift * SHIFT_UNIT - PIXEL_WIDTH, 0)
				);
				graphics.DrawImage(
					tile.GetMirror().AsBitmap(colorMapping),
					new Point((TILES_COUNT - 1 - tileIndex) * Tile32.PIXEL_WIDTH + Shift * SHIFT_UNIT - PIXEL_WIDTH, 0)
				);
			}

			return bitmap;
		}
	}
}
