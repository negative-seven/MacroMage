using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.Palette
{
	public struct NesColor
	{
		public byte Id { get; private set; }



		public NesColor(byte id)
		{
			this.Id = id;
		}

		public Color AsArgbColor(NesColorMapping mapping)
		{
			return mapping.Get(Id);
		}
	}
}
