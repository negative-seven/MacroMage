using MacroMage.GameLevel.Tile;
using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel
{
	public class Level
	{
		public const int PIXEL_WIDTH = Tile32Row.PIXEL_WIDTH;
		public const int PIXEL_HEIGHT = Tile32Row.PIXEL_HEIGHT * ROWS_COUNT;
		public const int ROWS_COUNT = 0x20;



		public int Id { get; private set; }
		public int World => Id == 12 ? 3 : Id / 3;
		public int WorldLevel => Id == 12 ? 3 : Id % 3;

		public List<Tile32Row> Rows;
		public List<Tile32> Tile32s;
		public List<Tile16> Tile16s;
		public List<Tile8> Tile8s;
		public List<NesPalette> Tile16Palettes;



		public Level(int id)
		{
			this.Id = id;
		}

		public Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			var bitmap = new Bitmap(PIXEL_WIDTH, PIXEL_HEIGHT);
			var graphics = Graphics.FromImage(bitmap);

			for (var rowIndex = 0; rowIndex < ROWS_COUNT; rowIndex++)
			{
				var row = Rows[rowIndex];
				graphics.DrawImage(row.AsBitmap(colorMapping), new Point(0, Tile32Row.PIXEL_HEIGHT * (Rows.Count - 1 - rowIndex)));
			}

			return bitmap;
		}
	}
}
