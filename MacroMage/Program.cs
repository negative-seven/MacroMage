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
			SaveLevel(OUTPUT_DIRECTORY + "level.bmp", level, colorMapping);
			Console.WriteLine("Saving tile image data");
			SaveAllTiles(level, colorMapping);

			Console.WriteLine("Completed");

			Console.ReadKey();
			return;
		}

		private static void SaveLevel(string filepath, Level level, NesColorMapping colorMapping)
		{
			level.AsBitmap(colorMapping).Save(filepath);
		}

		private static void SaveAllTiles(Level level, NesColorMapping colorMapping)
		{
			SaveLevelSegments(level.Tile8s, OUTPUT_DIRECTORY + "tile8s.bmp", colorMapping, 8, 8);
			SaveLevelSegments(level.Tile16s, OUTPUT_DIRECTORY + "tile16s.bmp", colorMapping, 16, 16);
			SaveLevelSegments(level.Tile32s, OUTPUT_DIRECTORY + "tile32s.bmp", colorMapping, 32, 32);
			SaveLevelSegments(level.Tile32s.Select(t => t.GetMirror()), OUTPUT_DIRECTORY + "tile32mirrors.bmp", colorMapping, 32, 32);
			SaveLevelSegments(level.SpriteTiles, OUTPUT_DIRECTORY + "spritetiles.bmp", colorMapping, 8, 8);
			SaveLevelSegments(level.GameObjects, OUTPUT_DIRECTORY + "gameobjects.bmp", colorMapping, 64, 64);
		}

		private static void SaveLevelSegments<T>(IEnumerable<T> elements, string filepath, NesColorMapping colorMapping, int elementWidth, int elementHeight)
			where T : ILevelSegment
		{
			var bitmap = new Bitmap(elementWidth * 16, elementHeight * 16);
			var graphics = Graphics.FromImage(bitmap);
			foreach (var e in elements.Where(e => e != null))
			{
				graphics.DrawImage(e.AsBitmap(colorMapping), new Point((e.Id % 16) * elementWidth, (e.Id / 16) * elementHeight));
			}
			bitmap.Save(filepath);
		}
	}
}
