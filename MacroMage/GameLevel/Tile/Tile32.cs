using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel.Tile
{
	public class Tile32 : Tile
	{
		public const int PIXEL_WIDTH = Tile16.PIXEL_WIDTH * 2;
		public const int PIXEL_HEIGHT = Tile16.PIXEL_HEIGHT * 2;
		public override int CONST_PIXEL_WIDTH => PIXEL_WIDTH;
		public override int CONST_PIXEL_HEIGHT => PIXEL_HEIGHT;



		public override Level Level { get; set; }
		public override int Id { get; set; }

		public int TopLeftId { get; set; }
		public int TopRightId { get; set; }
		public int BottomLeftId { get; set; }
		public int BottomRightId { get; set; }

		public Tile16 TopLeftTile { get { return Level.Tile16s[TopLeftId]; } }
		public Tile16 TopRightTile { get { return Level.Tile16s[TopRightId]; } }
		public Tile16 BottomLeftTile { get { return Level.Tile16s[BottomLeftId]; } }
		public Tile16 BottomRightTile { get { return Level.Tile16s[BottomRightId]; } }


		
		public override Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			var bitmap = new Bitmap(PIXEL_WIDTH, PIXEL_HEIGHT);
			var graphics = Graphics.FromImage(bitmap);

			graphics.DrawImage(TopLeftTile.AsBitmap(colorMapping), new Point(0, 0));
			graphics.DrawImage(TopRightTile.AsBitmap(colorMapping), new Point(PIXEL_WIDTH / 2, 0));
			graphics.DrawImage(BottomLeftTile.AsBitmap(colorMapping), new Point(0, PIXEL_HEIGHT / 2));
			graphics.DrawImage(BottomRightTile.AsBitmap(colorMapping), new Point(PIXEL_WIDTH / 2, PIXEL_HEIGHT / 2));

			return bitmap;
		}

		public Tile32 GetMirror()
		{
			return new Tile32()
			{
				Level = Level,
				Id = Id,
				TopLeftId = TopRightTile.MirrorId,
				TopRightId = TopLeftTile.MirrorId,
				BottomLeftId = BottomRightTile.MirrorId,
				BottomRightId = BottomLeftTile.MirrorId,
			};
		}
	}
}
