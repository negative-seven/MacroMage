using MacroMage.GameLevel;
using MacroMage.GameLevel.Tile;
using MacroMage.Palette;
using MacroMage.Rom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MacroMage
{
	public class LevelDataParser
	{
		private const int GLOBAL_TILE32_COUNT = 0x24;
		private const int GLOBAL_TILE16_COUNT = 0x4c;



		private GameRom gameRom;
		private int levelId;

		private Level level;


		
		public Level Run(GameRom gameRom, int levelId)
		{
			this.gameRom = gameRom;
			this.levelId = levelId;

			level = new Level(levelId);
			level.Tile8s = GetTile8s().ToList();
			level.Tile16Palettes = GetTile16Palettes(level).ToList();
			level.Tile16s = GetTile16s(level).ToList();
			level.Tile32s = GetTile32s(level).ToList();
			level.Rows = GetTile32Rows(level).ToList();

			return level;
		}

		private IList<Tile32Row> GetTile32Rows(Level level)
		{
			var rows = new List<Tile32Row>();

			var tileDataPointer = (ushort)(gameRom.PrgRom.WordAtAddress((ushort)(0xdad0 + levelId * 2)) + 1);

			for (int tileRowIndex = 0; tileRowIndex < Level.ROWS_COUNT; tileRowIndex++)
			{
				var tileRow = new Tile32Row()
				{
					Level = level,
					Id = tileRowIndex,
					TileIds = new List<int>(),
				};

				var tileRowShift = 0;
				for (int tileIndex = Tile32Row.UNMIRRORED_TILES_COUNT - 1; tileIndex >= 0; tileIndex--) // tiles within a row are stored in reverse
				{
					var tile32Id = gameRom.PrgRom.ByteAtAddress((ushort)(tileDataPointer + tileRowIndex * Tile32Row.UNMIRRORED_TILES_COUNT + tileIndex));
					tileRowShift |= (tile32Id & 0b1) << (Tile32Row.UNMIRRORED_TILES_COUNT - 1 - tileIndex);
					tile32Id >>= 1;
					tileRow.TileIds.Add(tile32Id);
				}
				tileRow.Shift = tileRowShift;

				rows.Add(tileRow);
			}

			return rows;
		}
		
		private IList<Tile32> GetTile32s(Level level)
		{
			var tiles = new List<Tile32>();
			
			var localTileDataPointer = gameRom.PrgRom.WordAtAddress((ushort)(0xaf10 + level.World * 2));

			var localTile16Count = gameRom.PrgRom.ByteAtAddress(localTileDataPointer++);
			var localTile32Count = gameRom.PrgRom.ByteAtAddress(localTileDataPointer++);
			localTileDataPointer += (ushort)(localTile16Count * 4); // skip tile16 corner data
			localTileDataPointer += (ushort)((localTile16Count + GLOBAL_TILE16_COUNT + 3) / 4); // skip tile16 palette data

			var localTileCornerDataPointer = (ushort)(localTileDataPointer - GLOBAL_TILE32_COUNT);
			var globalTileCornerDataPointersPointer = (ushort)0xf88b;

			for (var id = 0; id < GLOBAL_TILE32_COUNT + localTile32Count; id++)
			{
				ushort topLeftPointer, topRightPointer, bottomLeftPointer, bottomRightPointer;
				if (id < GLOBAL_TILE32_COUNT)
				{
					topLeftPointer     = gameRom.PrgRom.WordAtAddress((ushort)(globalTileCornerDataPointersPointer + 0));
					topRightPointer    = gameRom.PrgRom.WordAtAddress((ushort)(globalTileCornerDataPointersPointer + 2));
					bottomLeftPointer  = gameRom.PrgRom.WordAtAddress((ushort)(globalTileCornerDataPointersPointer + 4));
					bottomRightPointer = gameRom.PrgRom.WordAtAddress((ushort)(globalTileCornerDataPointersPointer + 6));
				}
				else
				{
					topLeftPointer     = (ushort)(localTileCornerDataPointer + localTile32Count * 0);
					topRightPointer    = (ushort)(localTileCornerDataPointer + localTile32Count * 1);
					bottomLeftPointer  = (ushort)(localTileCornerDataPointer + localTile32Count * 2);
					bottomRightPointer = (ushort)(localTileCornerDataPointer + localTile32Count * 3);
				}

				var topLeftId     = gameRom.PrgRom.ByteAtAddress((ushort)(topLeftPointer + id));
				var topRightId    = gameRom.PrgRom.ByteAtAddress((ushort)(topRightPointer + id));
				var bottomLeftId  = gameRom.PrgRom.ByteAtAddress((ushort)(bottomLeftPointer + id));
				var bottomRightId = gameRom.PrgRom.ByteAtAddress((ushort)(bottomRightPointer + id));
				
				tiles.Add(new Tile32()
				{
					Level = level,
					Id = id,
					TopLeftId = topLeftId,
					TopRightId = topRightId,
					BottomLeftId = bottomLeftId,
					BottomRightId = bottomRightId,
				});
			}

			return tiles;
		}

		private IList<Tile16> GetTile16s(Level level)
		{
			var tiles = new List<Tile16>();

			var globalTileCornerDataPointer = gameRom.PrgRom.WordAtAddress(0xf883);

			var localTileDataPointer = gameRom.PrgRom.WordAtAddress((ushort)(0xaf10 + level.World * 2));
			var localTile16Count = gameRom.PrgRom.ByteAtAddress(localTileDataPointer++);
			localTileDataPointer++; // this address stores the number of local tile32s, not needed here
			var localTileCornerDataPointer = (ushort)(localTileDataPointer - GLOBAL_TILE16_COUNT);
			localTileDataPointer += (ushort)(localTile16Count * 4);
			var palettesPointer = localTileDataPointer;

			var explicitMirrorPairs = new List<Tuple<int, int>>();
			const int explicitMirrorPairsCount = 6;
			for (var pairIndex = 0; pairIndex < explicitMirrorPairsCount; pairIndex++)
			{
				var first = (int)gameRom.PrgRom.ByteAtAddress((ushort)(0xaf00 + pairIndex));
				var second = (int)gameRom.PrgRom.ByteAtAddress((ushort)(0xaf00 + explicitMirrorPairsCount + pairIndex));
				explicitMirrorPairs.Add(Tuple.Create(first, second));
			}

			for (var id = 0; id < GLOBAL_TILE16_COUNT + localTile16Count; id++)
			{
				// Corners
				ushort topLeftPointer, topRightPointer, bottomLeftPointer, bottomRightPointer;
				if (id < GLOBAL_TILE16_COUNT)
				{
					topLeftPointer     = (ushort)(globalTileCornerDataPointer + GLOBAL_TILE16_COUNT * 0);
					topRightPointer    = (ushort)(globalTileCornerDataPointer + GLOBAL_TILE16_COUNT * 1);
					bottomLeftPointer  = (ushort)(globalTileCornerDataPointer + GLOBAL_TILE16_COUNT * 2);
					bottomRightPointer = (ushort)(globalTileCornerDataPointer + GLOBAL_TILE16_COUNT * 3);
				}
				else
				{
					topLeftPointer     = (ushort)(localTileCornerDataPointer + localTile16Count * 0);
					topRightPointer    = (ushort)(localTileCornerDataPointer + localTile16Count * 1);
					bottomLeftPointer  = (ushort)(localTileCornerDataPointer + localTile16Count * 2);
					bottomRightPointer = (ushort)(localTileCornerDataPointer + localTile16Count * 3);
				}

				var topLeftId     = gameRom.PrgRom.ByteAtAddress((ushort)(topLeftPointer + id));
				var topRightId    = gameRom.PrgRom.ByteAtAddress((ushort)(topRightPointer + id));
				var bottomLeftId  = gameRom.PrgRom.ByteAtAddress((ushort)(bottomLeftPointer + id));
				var bottomRightId = gameRom.PrgRom.ByteAtAddress((ushort)(bottomRightPointer + id));

				// Palette
				var paletteId = gameRom.PrgRom.ByteAtAddress((ushort)(palettesPointer + id / 4));
				paletteId >>= (id & 0b11) * 2;
				paletteId &= 0b11;

				// Mirror
				var selfSymmetryUpperBound = gameRom.PrgRom.ByteAtAddress((ushort)(0xaf0c + level.World));
				int mirrorTile;
				if (id == 0x11)
				{
					mirrorTile = 0x0;
				}
				else if (id < 0x1e || id > selfSymmetryUpperBound)
				{
					mirrorTile = id;

					foreach (var pair in explicitMirrorPairs)
					{
						if (id == pair.Item1)
						{
							mirrorTile = pair.Item2;
							break;
						}
						else if (id == pair.Item2)
						{
							mirrorTile = pair.Item1;
							break;
						}
					}
				}
				else
				{
					mirrorTile = id ^ 0b1;
				}

				tiles.Add(new Tile16()
				{
					Level = level,
					Id = id,
					PaletteId = paletteId,
					MirrorId = mirrorTile,
					TopLeftId = topLeftId,
					TopRightId = topRightId,
					BottomLeftId = bottomLeftId,
					BottomRightId = bottomRightId,
				});
			}

			return tiles;
		}

		private IList<Tile8> GetTile8s()
		{
			var tiles = new List<Tile8>();

			for (var id = 0; id < 0x100; id++)
			{
				var pattern = new byte[8][];

				for (int columnIndex = 0; columnIndex < 8; columnIndex++)
				{
					var row = new byte[8];

					for (int rowIndex = 0; rowIndex < 8; rowIndex++)
					{
						var lowerBitPlaneAddress = (ushort)((id << 4) | (columnIndex << 0));
						var upperBitPlaneAddress = (ushort)(lowerBitPlaneAddress | (1 << 3));

						row[rowIndex] = (byte)(
							gameRom.ChrRom.BitAt(upperBitPlaneAddress, (7 - rowIndex)) << 1 |
							gameRom.ChrRom.BitAt(lowerBitPlaneAddress, (7 - rowIndex))
						);
					}

					pattern[columnIndex] = row;
				}
				
				tiles.Add(new Tile8()
				{
					Level = level,
					Id = id,
					Pattern = pattern,
				});
			}

			return tiles;
		}

		private IList<NesPalette> GetTile16Palettes(Level level)
		{
			var localTileDataPointer = gameRom.PrgRom.WordAtAddress((ushort)(0xaf10 + level.World * 2));

			var localTile16Count = gameRom.PrgRom.ByteAtAddress(localTileDataPointer++);
			var localTile32Count = gameRom.PrgRom.ByteAtAddress(localTileDataPointer++);
			localTileDataPointer += (ushort)(localTile16Count * 4); // skip tile16 corner data
			localTileDataPointer += (ushort)((localTile16Count + 0x4c + 3) / 4); // skip tile16 palette data
			localTileDataPointer += (ushort)(localTile32Count * 4); // skip tile32 corner data

			var palettesPointer = localTileDataPointer;

			BigInteger bits = 0;
			for (var i = 8; i >= 0; i--)
			{
				var dataByte = gameRom.PrgRom.ByteAtAddress((ushort)(palettesPointer + i));
				for (var j = 0; j < 8; j++)
				{
					bits <<= 1;
					bits |= (dataByte & 0b1);
					dataByte >>= 1;
				}
			}

			var colorData = Enumerable.Repeat((byte)0, 12).ToArray();
			for (var i = 0; i < 12; i++)
			{
				for (var j = 0; j < 6; j++)
				{
					colorData[i] <<= 1;
					colorData[i] |= (byte)(bits & 0b1);
					bits >>= 1;
				}
			}

			var palettes = new List<NesPalette>();
			for (var paletteId = 0; paletteId < 4; paletteId++)
			{
				var palette = new NesPalette()
				{
					BgColor = new NesColor(0xf),
					Color1 = new NesColor(colorData[paletteId * 3 + 0]),
					Color2 = new NesColor(colorData[paletteId * 3 + 1]),
					Color3 = new NesColor(colorData[paletteId * 3 + 2]),
				};

				palettes.Add(palette);
			}

			return palettes;
		}
	}
}
