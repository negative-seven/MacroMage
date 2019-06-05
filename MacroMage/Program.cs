using MacroMage.GameLevel;
using MacroMage.GameLevel.Tile;
using MacroMage.Palette;
using MacroMage.Rom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage
{
	public class Program
	{
		private static string OUTPUT_DIRECTORY = @"output\";

		public static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.Error.WriteLine("Missing level ID argument");
				return;
			}
			if (!int.TryParse(args[0], out int levelId) || levelId < 0 || levelId > 12)
			{
				Console.Error.WriteLine("Invalid level ID given");
				return;
			}

			Console.WriteLine("Parsing level data");
			var rom = GameRom.FromFile("Micro Mages.nes");
			var colorMapping = NesColorMapping.FromFile("ntscpalette.pal");
			var level = new LevelDataParser().Run(rom, levelId);

			Directory.CreateDirectory(OUTPUT_DIRECTORY);
			Console.WriteLine("Saving level image data");
			SaveAllTiles(level, colorMapping);
			Console.WriteLine("Saving tile image data");
			SaveLevel(OUTPUT_DIRECTORY + "level.bmp", level, colorMapping);

			Console.WriteLine("Completed");
			return;
		}

		private static void SaveLevel(string filepath, Level level, NesColorMapping colorMapping)
		{
			level.AsBitmap(colorMapping).Save(filepath);
		}

		private static void SaveAllTiles(Level level, NesColorMapping colorMapping)
		{
			SaveTiles(level.Tile8s, OUTPUT_DIRECTORY + "tile8s.bmp", colorMapping);
			SaveTiles(level.Tile16s, OUTPUT_DIRECTORY + "tile16s.bmp", colorMapping);
			SaveTiles(level.Tile32s, OUTPUT_DIRECTORY + "tile32s.bmp", colorMapping);
			SaveTiles(level.Tile32s.Select(t => t.GetMirror()), OUTPUT_DIRECTORY + "tile32mirrors.bmp", colorMapping);
		}

		private static void SaveTiles<T>(IEnumerable<T> tiles, string filepath, NesColorMapping colorMapping)
			where T : Tile
		{
			var bitmap = new Bitmap(tiles.First().CONST_PIXEL_WIDTH * 16, tiles.First().CONST_PIXEL_HEIGHT * 16);
			var graphics = Graphics.FromImage(bitmap);
			foreach (var tile in tiles)
			{
				graphics.DrawImage(tile.AsBitmap(colorMapping), new Point((tile.Id % 16) * tile.CONST_PIXEL_WIDTH, (tile.Id / 16) * tile.CONST_PIXEL_HEIGHT));
			}
			bitmap.Save(filepath);
		}
	}
}
