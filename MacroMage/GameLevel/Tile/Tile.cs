using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel.Tile
{
	public abstract class Tile
	{
		public abstract int CONST_PIXEL_WIDTH { get; }
		public abstract int CONST_PIXEL_HEIGHT { get; }



		public abstract Level Level { get; set; }
		public abstract int Id { get; set; }



		public abstract Bitmap AsBitmap(NesColorMapping colorMapping);
	}
}
