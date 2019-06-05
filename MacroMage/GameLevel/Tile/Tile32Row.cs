using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel.Tile
{
	public class Tile32Row : Tile
	{
		public const int PIXEL_WIDTH = Tile32.PIXEL_WIDTH * TILES_COUNT;
		public const int PIXEL_HEIGHT = Tile32.PIXEL_HEIGHT;
		public override int CONST_PIXEL_WIDTH => PIXEL_WIDTH;
		public override int CONST_PIXEL_HEIGHT => PIXEL_HEIGHT;
		public const int UNMIRRORED_TILES_COUNT = 4;
		public const int TILES_COUNT = UNMIRRORED_TILES_COUNT * 2;
		public const int SHIFT_UNIT = Tile16.PIXEL_WIDTH;


		
		public override Level Level { get; set; }
		public override int Id { get; set; }
		public List<int> TileIds { get; set; }
		public int Shift { get; set; }


		
		public Tile32 GetTile(int index)
		{
			return Level.Tile32s[TileIds[index]];
		}

		public override Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			var bitmap = new Bitmap(CONST_PIXEL_WIDTH, CONST_PIXEL_HEIGHT);
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
					new Point(tileIndex * Tile32.PIXEL_WIDTH + Shift * SHIFT_UNIT - CONST_PIXEL_WIDTH, 0)
				);
				graphics.DrawImage(
					tile.GetMirror().AsBitmap(colorMapping),
					new Point((TILES_COUNT - 1 - tileIndex) * Tile32.PIXEL_WIDTH + Shift * SHIFT_UNIT - CONST_PIXEL_WIDTH, 0)
				);
			}

			return bitmap;
		}
	}
}
