using System.Drawing;
using MacroMage.Palette;

namespace MacroMage.GameLevel.Object
{
	public class LevelObject : ILevelSegment
	{
		public Level Level { get; set; }
		public int Id { get; set; }

		public int PixelWidth => GameObject.PixelWidth;
		public int PixelHeight => GameObject.PixelHeight;

		public int GameObjectId { get; set; }
		public GameObject GameObject => Level.GameObjects[GameObjectId];
		public int TileX { get; set; }
		public int TileY { get; set; }
		public bool FlipX { get; set; }
		public bool FlipY { get; set; }



		public Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			var bitmap = Level.GameObjects[GameObjectId].AsBitmap(colorMapping);

			if (FlipX && FlipY)
			{
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);
			}
			else if (FlipX)
			{
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
			}
			else if (FlipY)
			{
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			}

			return bitmap;
		}
	}
}
