using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.Rom
{
	public class RomSegment
	{
		private ushort addressOffset;
		private byte[] bytes;

		public RomSegment(byte[] bytes, ushort addressOffset = 0x0)
		{
			this.addressOffset = addressOffset;
			this.bytes = bytes;
		}

		public byte ByteAt(ushort location)
		{
			return bytes[location];
		}

		public byte ByteAtAddress(ushort address)
		{
			if (address < addressOffset)
			{
				throw new ArgumentException("Invalid address");
			}

			return ByteAt((ushort)(address - addressOffset));
		}

		public ushort WordAt(ushort location)
		{
			return (ushort)(ByteAt((ushort)(location + 1)) * 0x100 + ByteAt(location));
		}

		public ushort WordAtAddress(ushort address)
		{
			if (address < addressOffset)
			{
				throw new ArgumentException("Invalid address");
			}

			return WordAt((ushort)(address - addressOffset));
		}

		public byte BitAt(ushort location, int bitIndex)
		{
			if (bitIndex < 0 || bitIndex > 7)
			{
				throw new ArgumentException("Invalid bit index");
			}

			return (byte)((ByteAt(location) >> bitIndex) % 2);
		}

		public byte BitAtAddress(ushort address, int bitIndex)
		{
			if (address < addressOffset)
			{
				throw new ArgumentException("Invalid address");
			}

			return BitAt((ushort)(address - addressOffset), bitIndex);
		}
	}
}
