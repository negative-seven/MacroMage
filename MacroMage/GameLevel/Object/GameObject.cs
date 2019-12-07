using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel.Object
{
	public partial class GameObject : ILevelSegment
	{
		public class TileData : ILevelSegment
		{
			public const int PIXEL_WIDTH = PatternTableTile.PIXEL_WIDTH;
			public const int PIXEL_HEIGHT = PatternTableTile.PIXEL_HEIGHT;



			public Level Level { get; set; }
			public int Id { get; set; }

			public int PixelWidth => PIXEL_WIDTH;
			public int PixelHeight => PIXEL_HEIGHT;
			public Point PixelOffset { get; set; }

			public bool FlipX { get; set; }
			public bool FlipY { get; set; }


			
			public Bitmap AsBitmap(NesColorMapping colorMapping)
			{
				return AsBitmap(Level.SpritePalettes.FirstOrDefault(), colorMapping);
			}

			public Bitmap AsBitmap(NesPalette palette, NesColorMapping colorMapping)
			{
				var bitmap = Level.SpriteTiles[Id].AsBitmap(palette, colorMapping, true);
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



		public Level Level { get; set; }
		public int Id { get; set; } // TODO

		public int PixelWidth => TilesData.Count > 0 ? (TilesData.Select(d => d.PixelOffset.X).Max() + PatternTableTile.PIXEL_WIDTH) : 1;
		public int PixelHeight => TilesData.Count > 0 ? (TilesData.Select(d => d.PixelOffset.Y).Max() + PatternTableTile.PIXEL_HEIGHT) : 1;
		public Point PixelOffset { get; set; }



		
		public List<TileData> TilesData { get; set; }
		public int PaletteId { get; set; }
		public NesPalette Palette => Level.SpritePalettes[PaletteId];

		

		public Bitmap AsBitmap(NesColorMapping colorMapping)
		{
			var bitmap = new Bitmap(PixelWidth, PixelHeight);
			var graphics = Graphics.FromImage(bitmap);

			foreach (var tileData in TilesData)
			{
				graphics.DrawImage(tileData.AsBitmap(Palette, colorMapping), new Point(tileData.PixelOffset.X, tileData.PixelOffset.Y));
			}

			return bitmap;
		}
	}
}
