using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.Rom.Exception
{
	public class ChecksumMismatchException : System.Exception
	{
		public ChecksumMismatchException() : base()
		{
		}

		public ChecksumMismatchException(string message) : base(message)
		{
		}

		public ChecksumMismatchException(string message, System.Exception innerException) : base(message, innerException)
		{
		}
	}
}
