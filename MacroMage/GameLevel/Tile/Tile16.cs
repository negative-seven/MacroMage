using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel.Tile
{
	public class Tile16 : Tile
	{
		public const int PIXEL_WIDTH = Tile8.PIXEL_WIDTH * 2;
		public const int PIXEL_HEIGHT = Tile8.PIXEL_HEIGHT * 2;
		public override int CONST_PIXEL_WIDTH => PIXEL_WIDTH;
		public override int CONST_PIXEL_HEIGHT => PIXEL_HEIGHT;



		public override Level Level { get; set; }
		public override int Id { get; set; }

		public int TopLeftId { get; set; }
		public int TopRightId { get; set; }
		public int BottomLeftId { get; set; }
		public int BottomRightId { get; set; }
		
		public int PaletteId { get; set; }
		public int MirrorId { get; set; }

		public Tile8 TopLeftTile { get { return Level.Tile8s[TopLeftId]; } }
		public Tile8 TopRightTile { get { return Level.Tile8s[TopRightId]; } }
		public Tile8 BottomLeftTile { get { return Level.Tile8s[BottomLeftId]; } }
		public Tile8 BottomRightTile { get { return Level.Tile8s[BottomRightId]; } }
		public NesPalette Palette { get { return Level.Tile16Palettes[PaletteId]; } }
		public Tile16 Mirror { get { return Level.Tile16s[MirrorId];  } }


		
		public override Bitmap AsBitmap(NesColorMapping colorMapping)
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
