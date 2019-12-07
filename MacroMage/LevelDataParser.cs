using MacroMage.GameLevel;
using MacroMage.GameLevel.Object;
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



		private GameRom gameRom { get; set; }
		private Level level { get; set; }


		
		public Level Run(GameRom gameRom, int levelId)
		{
			this.gameRom = gameRom;

			level = new Level(levelId);
			level.Tile8s = GetTile8s();
			level.Tile16Palettes = GetTile16Palettes(level);
			level.Tile16s = GetTile16s(level);
			level.Tile32s = GetTile32s(level);
			level.Tile32Rows = GetTile32Rows(level);
			level.SpriteTiles = GetSpriteTiles(level);
			level.SpritePalettes = GetSpritePalettes(level);
			level.GameObjects = GetGameObjects(level);
			level.LevelObjects = GetLevelObjects(level);

			return level;
		}

		private List<Tile32Row> GetTile32Rows(Level level)
		{
			var rows = new List<Tile32Row>();

			var tileDataPointer = (ushort)(gameRom.PrgRom.WordAtAddress((ushort)(0xdad0 + level.Id * 2)) + 1);

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
		
		private List<Tile32> GetTile32s(Level level)
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

		private List<Tile16> GetTile16s(Level level)
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

		private List<PatternTableTile> GetTile8s()
		{
			var tiles = new List<PatternTableTile>();

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
				
				tiles.Add(new PatternTableTile()
				{
					Level = level,
					Id = id,
					Pattern = pattern,
				});
			}

			return tiles;
		}

		private List<NesPalette> GetTile16Palettes(Level level)
		{
			var localTileDataPointer = gameRom.PrgRom.WordAtAddress((ushort)(0xaf10 + level.World * 2));

			var localTile16Count = gameRom.PrgRom.ByteAtAddress(localTileDataPointer++);
			var localTile32Count = gameRom.PrgRom.ByteAtAddress(localTileDataPointer++);
			localTileDataPointer += (ushort)(localTile16Count * 4); // skip tile16 corner data
			localTileDataPointer += (ushort)((localTile16Count + 0x4c + 3) / 4); // skip tile16 palette data
			localTileDataPointer += (ushort)(localTile32Count * 4); // skip tile32 corner data

			var palettesPointer = localTileDataPointer;
			
			var bitBuffer = new BitBuffer(i => gameRom.PrgRom.ByteAtAddress((ushort)(palettesPointer + i)));

			var palettes = new List<NesPalette>();
			for (var paletteId = 0; paletteId < 4; paletteId++)
			{
				var palette = new NesPalette()
				{
					BgColor = new NesColor(0xf),
					Color1 = new NesColor((byte)bitBuffer.Get(6)),
					Color2 = new NesColor((byte)bitBuffer.Get(6)),
					Color3 = new NesColor((byte)bitBuffer.Get(6)),
				};

				palettes.Add(palette);
			}

			return palettes;
		}
		
		private List<GameObject> GetGameObjects(Level level)
		{
			var objects = new List<GameObject>();
			objects.Add(null);

			for (var id = 1; id < 0x34 /* TODO */; id++)
			{
				var sizeId = gameRom.PrgRom.ByteAtAddress((ushort)(0xbd67 + id));
				var tilesData = new List<GameObject.TileData>();

				if (id == 0x30 || id == 0x2e || id == 0x31 || id == 0x33) // special case for teleporters and boss doors
				{
					// do nothing
				}
				else if (sizeId == 0) // single-tile sprite
				{
					tilesData.Add(new GameObject.TileData()
					{
						Level = level,
						Id = gameRom.PrgRom.ByteAtAddress((ushort)(0xbdbb + id)) + 1,

						FlipX = false,
						FlipY = false,
						PixelOffset = new Point(-4, -4),
					});
				}
				else
				{
					var sizeDataPointer = (ushort)(
						gameRom.PrgRom.ByteAtAddress((ushort)(0xf138 + sizeId)) << 8 |
						gameRom.PrgRom.ByteAtAddress((ushort)(0xf12f + sizeId))
					);
					var tilesCount = gameRom.PrgRom.ByteAtAddress(sizeDataPointer++);

					if (tilesCount > 0)
					{
						var spriteDataPointer = (ushort)(
							gameRom.PrgRom.ByteAtAddress((ushort)(0xbde8 + id)) << 8 |
							gameRom.PrgRom.ByteAtAddress((ushort)(0xbdbb + id))
						);
						var baseSpriteId = gameRom.PrgRom.ByteAtAddress(spriteDataPointer++);

						for (var tileId = 0; tileId < tilesCount; tileId++)
						{
							var data = gameRom.PrgRom.ByteAtAddress(spriteDataPointer++);
							var spriteIdOffset = data & 0b111111;

							if (spriteIdOffset != 0)
							{
								tilesData.Add(new GameObject.TileData()
								{
									Level = level,
									Id = baseSpriteId + spriteIdOffset,

									PixelOffset = new Point(
									(sbyte)gameRom.PrgRom.ByteAtAddress(sizeDataPointer++),
									(sbyte)gameRom.PrgRom.ByteAtAddress(sizeDataPointer++)
								),
									FlipX = (data & 0b1000000) > 0,
									FlipY = (data & 0b10000000) > 0,
								});
							}
						}
					}
				}

				Point pixelOffset = Point.Empty;
				if (tilesData.Count > 0)
				{
					pixelOffset = new Point(
						tilesData.Select(d => (sbyte)d.PixelOffset.X).Min(),
						tilesData.Select(d => (sbyte)d.PixelOffset.Y).Min()
					);
					for (int tileDataIndex = 0; tileDataIndex < tilesData.Count; tileDataIndex++)
					{
						var offset = tilesData[tileDataIndex].PixelOffset;
						offset.Offset(-pixelOffset.X, -pixelOffset.Y);
						tilesData[tileDataIndex].PixelOffset = offset;
					}
				}
				if (id == 0x1a) // special case for sideways fan
				{
					pixelOffset.X += 4;
				}
				pixelOffset.Y += (sbyte)gameRom.PrgRom.ByteAtAddress((ushort)(0xbd4d + sizeId));

				var paletteId = gameRom.PrgRom.ByteAtAddress((ushort)(0xbe6d + id)) & 0b11;

				objects.Add(new GameObject()
				{
					Level = level,
					Id = id,
					PixelOffset = pixelOffset,
					TilesData = tilesData,
					PaletteId = paletteId,
				});
			}

			return objects;
		}

		private List<LevelObject> GetLevelObjects(Level level)
		{
			var levelDataPointer = gameRom.PrgRom.WordAtAddress((ushort)(0xdad0 + level.Id * 2));
			var someCounter = gameRom.PrgRom.ByteAtAddress(levelDataPointer++); // TODO: what is this counter?
			levelDataPointer += Level.ROWS_COUNT * Tile32Row.UNMIRRORED_TILES_COUNT; // skip level tile data
			levelDataPointer += someCounter; // TODO: what is this skipping?

			var objectDataPointer = levelDataPointer;

			var bitBuffer = new BitBuffer(i => gameRom.PrgRom.ByteAtAddress((ushort)(objectDataPointer + i)));

			var objects = new List<LevelObject>();
			var tileY = Level.ROWS_COUNT * 4;
			while (tileY >= 0)
			{
				var instruction = (int)bitBuffer.Get(2);

				bool newObject = false;
				int objectId = 0;
				int gameObjectId = -1;
				int tileX = -1;
				bool flipX = false, flipY = false;
				switch (instruction)
				{
					case 0: // skip rows
						tileY -= (int)bitBuffer.Get(3) + 1;
						break;

					case 1: // object
						newObject = true;

						flipY = bitBuffer.Get(1) == 1;
						flipX = bitBuffer.Get(1) == 1;
						tileX = (int)bitBuffer.Get(5);
						gameObjectId = gameRom.PrgRom.ByteAtAddress((ushort)(0xdab1 + bitBuffer.Get(5)));
						break;

					case 2: // object (compressed)
						newObject = true;

						flipY = false;
						flipX = false;
						tileX = (int)bitBuffer.Get(4) * 2 + 1;
						gameObjectId = gameRom.PrgRom.ByteAtAddress((ushort)(0xdab1 + bitBuffer.Get(4)));
						break;

					case 3: // end of data
						tileY = -1;
						break;
				}

				if (newObject)
				{
					objects.Add(new LevelObject()
					{
						Level = level,
						Id = objectId,

						GameObjectId = gameObjectId,
						TileX = tileX,
						TileY = tileY,
						FlipX = flipX,
						FlipY = flipY,
					});

					objectId++;
				}
			}

			return objects;
		}

		private List<PatternTableTile> GetSpriteTiles(Level level)
		{
			var tiles = new List<PatternTableTile>();

			for (int id = 0; id < 0x100; id++)
			{
				var pattern = new byte[8][];

				for (int columnIndex = 0; columnIndex < 8; columnIndex++)
				{
					var row = new byte[8];

					for (int rowIndex = 0; rowIndex < 8; rowIndex++)
					{
						var lowerBitPlaneAddress = (ushort)(0x1000 + (id << 4) | (columnIndex << 0));
						var upperBitPlaneAddress = (ushort)(lowerBitPlaneAddress | (1 << 3));

						row[rowIndex] = (byte)(
							gameRom.ChrRom.BitAt(upperBitPlaneAddress, (7 - rowIndex)) << 1 |
							gameRom.ChrRom.BitAt(lowerBitPlaneAddress, (7 - rowIndex))
						);
					}

					pattern[columnIndex] = row;
				}

				tiles.Add(new PatternTableTile()
				{
					Level = level,
					Id = id,
					Pattern = pattern,
				});
			}

			return tiles;
		}

		private List<NesPalette> GetSpritePalettes(Level level)
		{
			var bitBuffer = new BitBuffer(i => gameRom.PrgRom.ByteAtAddress((ushort)(0xbd5f + i)));

			var palettes = new List<NesPalette>();
			for (var paletteId = 0; paletteId < 4; paletteId++)
			{
				var palette = new NesPalette()
				{
					BgColor = new NesColor(0xf),
					Color1 = new NesColor((byte)bitBuffer.Get(6)),
					Color2 = new NesColor((byte)bitBuffer.Get(6)),
					Color3 = new NesColor((byte)bitBuffer.Get(6)),
				};

				palettes.Add(palette);
			}

			return palettes;
		}
	}
}
