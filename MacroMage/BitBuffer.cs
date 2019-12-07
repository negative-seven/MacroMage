using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage
{
	public class BitBuffer
	{
		private Func<int, byte> pollFunction { get; set; }
		private byte currentByte;
		private int currentByteIndex;
		private int currentBitCount;

		public BitBuffer(Func<int, byte> pollFunction)
		{
			this.pollFunction = pollFunction;

			currentByteIndex = 0;
			currentBitCount = 0;
		}

		public BigInteger Get(int bitCount)
		{
			var result = new BigInteger();
			
			for (int bitIndex = 0; bitIndex < bitCount; bitIndex++)
			{
				if (currentBitCount == 0)
				{
					currentByte = pollFunction.Invoke(currentByteIndex++);
					currentBitCount = 8;
				}

				result <<= 1;
				result |= (currentByte & 0b10000000) >> 7;
				currentByte <<= 1;
				currentBitCount--;
			}

			return result;
		}
	}
}
