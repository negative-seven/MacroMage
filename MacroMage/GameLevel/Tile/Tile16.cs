using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel.Tile
{
	public class Tile16 : ILevelSegment
	{
		public const int PIXEL_WIDTH = PatternTableTile.PIXEL_WIDTH * 2;
		public const int PIXEL_HEIGHT = PatternTableTile.PIXEL_HEIGHT * 2;



		public Level Level { get; set; }
		public int Id { get; set; }

		public int PixelWidth => PIXEL_WIDTH;
		public int PixelHeight => PIXEL_HEIGHT;

		public int TopLeftId { get; set; }
		public int TopRightId { get; set; }
		public int BottomLeftId { get; set; }
		public int BottomRightId { get; set; }
		
		public int PaletteId { get; set; }
		public int MirrorId { get; set; }

		public PatternTableTile TopLeftTile { get { return Level.Tile8s[TopLeftId]; } }
		public PatternTableTile TopRightTile { get { return Level.Tile8s[TopRightId]; } }
		public PatternTableTile BottomLeftTile { get { return Level.Tile8s[BottomLeftId]; } }
		public PatternTableTile BottomRightTile { get { return Level.Tile8s[BottomRightId]; } }
		public NesPalette Palette { get { return Level.Tile16Palettes[PaletteId]; } }
		public Tile16 Mirror { get { return Level.Tile16s[MirrorId];  } }


		
		public Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			var bitmap = new Bitmap(PIXEL_WIDTH, PIXEL_HEIGHT);
			var graphics = Graphics.FromImage(bitmap);

			graphics.DrawImage(TopLeftTile.AsBitmap(Palette, colorMapping), new Point(0, 0));
			graphics.DrawImage(TopRightTile.AsBitmap(Palette, colorMapping), new Point(PIXEL_WIDTH / 2, 0));
			graphics.DrawImage(BottomLeftTile.AsBitmap(Palette, colorMapping), new Point(0, PIXEL_HEIGHT / 2));
			graphics.DrawImage(BottomRightTile.AsBitmap(Palette, colorMapping), new Point(PIXEL_WIDTH / 2, PIXEL_HEIGHT / 2));

			return bitmap;
		}
	}
}
