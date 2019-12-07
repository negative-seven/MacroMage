using MacroMage.GameLevel.Object;
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
	public partial class Level : ILevelSegment
	{
		public const int PIXEL_WIDTH = Tile32Row.PIXEL_WIDTH;
		public const int PIXEL_HEIGHT = Tile32Row.PIXEL_HEIGHT * ROWS_COUNT;
		public const int ROWS_COUNT = 0x20;



		Level ILevelSegment.Level => this;
		public int Id { get; private set; }

		public int PixelWidth => PIXEL_WIDTH;
		public int PixelHeight => PIXEL_HEIGHT;

		public int World => Id == 12 ? 3 : Id / 3;
		public int WorldLevel => Id == 12 ? 3 : Id % 3;



		public List<Tile32Row> Tile32Rows { get; set; }
		public List<Tile32> Tile32s { get; set; }
		public List<Tile16> Tile16s { get; set; }
		public List<PatternTableTile> Tile8s { get; set; }
		public List<NesPalette> Tile16Palettes { get; set; }
		public List<PatternTableTile> SpriteTiles { get; set; }
		public List<GameObject> GameObjects { get; set; }
		public List<LevelObject> LevelObjects { get; set; }
		public List<NesPalette> SpritePalettes { get; set; }



		public Level(int id)
		{
			this.Id = id;
		}

		public Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			var bitmap = AsBitmapWithoutEnemies(colorMapping);
			var graphics = Graphics.FromImage(bitmap);

			foreach (var levelObject in LevelObjects)
			{
				graphics.DrawImage(levelObject.AsBitmap(colorMapping), new Point(levelObject.TileX * 8 + levelObject.GameObject.PixelOffset.X, levelObject.TileY * 8 + levelObject.GameObject.PixelOffset.Y));
			}

			return bitmap;
		}

		public Bitmap AsBitmapWithoutEnemies(NesColorMapping colorMapping)
		{
			var bitmap = new Bitmap(PIXEL_WIDTH, PIXEL_HEIGHT);
			var graphics = Graphics.FromImage(bitmap);

			for (var rowIndex = 0; rowIndex < ROWS_COUNT; rowIndex++)
			{
				var row = Tile32Rows[rowIndex];
				graphics.DrawImage(row.AsBitmap(colorMapping), new Point(0, Tile32Row.PIXEL_HEIGHT * (Tile32Rows.Count - 1 - rowIndex)));
			}

			return bitmap;
		}
	}
}
