using MacroMage.Palette;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.GameLevel
{
	public interface ILevelSegment
	{
		Level Level { get; }
		int Id { get; }

		int PixelWidth { get; }
		int PixelHeight { get; }



		Bitmap AsBitmap(NesColorMapping colorMapping);
	}
}
