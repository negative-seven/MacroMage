using MacroMage.Rom.Exception;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage.Rom
{
	public class GameRom
	{
		private const string MD5_CHECKSUM = "1062DF5838A11E0E17ED590BDC1095C6";
		private const int HEADER_SIZE = 0x10;
		private const int PRG_SIZE = 0x8000;
		private const int CHR_SIZE = 0x2000;
		


		private byte[] header;
		public ReadOnlyCollection<byte> Header
		{
			get
			{
				return Array.AsReadOnly(header);
			}
		}

		public RomSegment PrgRom;
		public RomSegment ChrRom;



		private GameRom()
		{
		}

		public static GameRom FromFile(string path)
		{
			GameRom gameRom = new GameRom();

			if (!VerifyRomChecksum(path))
			{
				throw new ChecksumMismatchException("The ROM checksum does not match.");
			}

			using (var reader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				gameRom.header = reader.ReadBytes(HEADER_SIZE);
				gameRom.PrgRom = new RomSegment(reader.ReadBytes(PRG_SIZE), 0x8000);
				gameRom.ChrRom = new RomSegment(reader.ReadBytes(CHR_SIZE));
			}

			return gameRom;
		}

		private static bool VerifyRomChecksum(string filePath)
		{
			return VerifyRomChecksum(File.ReadAllBytes(filePath));
		}

		private static bool VerifyRomChecksum(byte[] bytes)
		{
			var fileChecksum = string.Join("", MD5.Create().ComputeHash(bytes).Select(b => b.ToString("X2")));
			return string.Equals(fileChecksum, MD5_CHECKSUM, StringComparison.OrdinalIgnoreCase);
		}
	}
}
