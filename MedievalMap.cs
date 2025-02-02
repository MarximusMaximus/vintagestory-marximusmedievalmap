using System;
using System.Reflection;
using HarmonyLib;
using SkiaSharp;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace MedievalMap
{
    public class MedievalMapConfig
    {
        public string ink_colour = "#483018";
        public string land_colour = "#AC8858";
        public string desert_colour = "#C4A468";
        public string forest_colour = "#98844C";
        public string road_colour = "#805030";
        public string plant_colour = "#808650";
        public string water_colour = "#CCC890";
        public string ice_colour = "#E0E0C0";
        public string lava_colour = "#C05840";

        public string high_colour = "#FFF4D4";
        public string low_colour = "#503818";

        public string water_edge_colour = "#483018";

        public string generic_shadow_colour = "#483018";
        public string plant_shadow_colour = "#505820";

        public string generic_grid_colour = "#483018";
        public string water_grid_colour = "#604848";
        public float generic_grid_opacity = 0.1f;
        public float ocean_grid_opacity = 0.25f;

        public bool ocean_textured = true;
    }

    public class MedievalMapSystem : ModSystem
    {

        public const string harmonyId = "medievalmap";
        public const string configFilename = harmonyId + ".json";

        public static int inkColour;
        public static int landColour;
        public static int desertColour;
        public static int forestColour;
        public static int roadColour;
        public static int plantColour;
        public static int waterColour;
        public static int iceColour;
        public static int lavaColour;

        public static int highColour;
        public static int lowColour;

        public static int waterEdgeColour;

        public static int genericShadowColour;
        public static int plantShadowColour;

        public static int genericGridColour;
        public static int waterGridColour;

        public static float genericGridOpacity;
        public static float oceanGridOpacity;

        public static BitmapExternal oceanTex;

        public static MedievalMapConfig config;

        static protected ICoreClientAPI capi;
        protected Harmony harmony;

        private int convert(string hex)
        {
            int r = int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            int a = hex.Length < 8 ? 255 : int.Parse(hex.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);

            // NOTE: ColorUtil.ToRgba is misdocumented, and uses ABGR.
            return ColorUtil.ToRgba(a, b, g, r);
        }

        private BitmapExternal loadTexture(string filename)
        {
            byte[] pngData = MedievalMapSystem.capi.Assets.Get(new AssetLocation(harmonyId, "textures/" + filename + ".png")).Data;
            return MedievalMapSystem.capi.Render.BitmapCreateFromPng(pngData);
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);
            MedievalMapSystem.capi = capi;

            // Load or create config file.
            try
            {
                MedievalMapSystem.config = capi.LoadModConfig<MedievalMapConfig>(MedievalMapSystem.configFilename);
            }
            catch (Exception) { }
            if (MedievalMapSystem.config != null)
            {
                // Save config file in case of missing fields.
                capi.StoreModConfig<MedievalMapConfig>(MedievalMapSystem.config, MedievalMapSystem.configFilename);
            }
            else
            {
                // Create new config file.
                MedievalMapSystem.config = new MedievalMapConfig();
                capi.StoreModConfig<MedievalMapConfig>(MedievalMapSystem.config, MedievalMapSystem.configFilename);
            }

            // Convert data.
            MedievalMapSystem.inkColour = convert(MedievalMapSystem.config.ink_colour);
            MedievalMapSystem.landColour = convert(MedievalMapSystem.config.land_colour);
            MedievalMapSystem.desertColour = convert(MedievalMapSystem.config.desert_colour);
            MedievalMapSystem.forestColour = convert(MedievalMapSystem.config.forest_colour);
            MedievalMapSystem.roadColour = convert(MedievalMapSystem.config.road_colour);
            MedievalMapSystem.plantColour = convert(MedievalMapSystem.config.plant_colour);
            MedievalMapSystem.waterColour = convert(MedievalMapSystem.config.water_colour);
            MedievalMapSystem.iceColour = convert(MedievalMapSystem.config.ice_colour);
            MedievalMapSystem.lavaColour = convert(MedievalMapSystem.config.lava_colour);
            MedievalMapSystem.highColour = convert(MedievalMapSystem.config.high_colour);
            MedievalMapSystem.lowColour = convert(MedievalMapSystem.config.low_colour);

            MedievalMapSystem.waterEdgeColour = convert(MedievalMapSystem.config.water_edge_colour);

            MedievalMapSystem.genericShadowColour = convert(MedievalMapSystem.config.generic_shadow_colour);
            MedievalMapSystem.plantShadowColour = convert(MedievalMapSystem.config.plant_shadow_colour);

            MedievalMapSystem.genericGridColour = convert(MedievalMapSystem.config.generic_grid_colour);
            MedievalMapSystem.waterGridColour = convert(MedievalMapSystem.config.water_grid_colour);

            MedievalMapSystem.genericGridOpacity = MedievalMapSystem.config.generic_grid_opacity;
            MedievalMapSystem.oceanGridOpacity = MedievalMapSystem.config.ocean_grid_opacity;

            MedievalMapSystem.oceanTex = MedievalMapSystem.config.ocean_textured ? loadTexture("ocean") : null;

            this.harmony = new Harmony(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void Dispose()
        {
            base.Dispose();

            if (MedievalMapSystem.oceanTex != null)
            {
                MedievalMapSystem.oceanTex.Dispose();
            }

            if (harmony != null)
            {
                this.harmony.UnpatchAll(harmonyId);
            }
        }

        public static int getColour(Block block, int x, int y, int z, int worldHalfHeight, int seaY, float slope, int cx, int cz, int csize)
        {
            int colour = inkColour;
            int shadow = genericShadowColour;
            int gridColour = genericGridColour;
            float gridOpacity = genericGridOpacity;
            float heightOpacity = 0;
            BitmapExternal bmp = null;

            switch (block.BlockMaterial)
            {
                case EnumBlockMaterial.Soil:
                    {
                        switch (block.FirstCodePart())
                        {
                            case "farmland":
                                {
                                    colour = roadColour;
                                    heightOpacity = 0.5f;
                                    break;
                                }

                            case "forestfloor":
                                {
                                    colour = forestColour;
                                    heightOpacity = 1;
                                    break;
                                }

                            default:
                                {
                                    colour = landColour;
                                    heightOpacity = 1;
                                    break;
                                }
                        }
                        break;
                    }

                case EnumBlockMaterial.Sand:
                    {
                        colour = desertColour;
                        heightOpacity = 1;
                        break;
                    }

                case EnumBlockMaterial.Ore:
                    {
                        colour = landColour;
                        heightOpacity = 1;
                        break;
                    }

                case EnumBlockMaterial.Gravel:
                    {
                        switch (block.FirstCodePart())
                        {
                            case "stonepath":
                            case "stonepathslab":
                            case "stonepathstairs":
                                {
                                    colour = roadColour;
                                    heightOpacity = 0.5f;
                                    break;
                                }

                            default:
                                {
                                    colour = desertColour;
                                    heightOpacity = 1;
                                    break;
                                }
                        }
                        break;
                    }

                case EnumBlockMaterial.Stone:
                    {
                        switch (block.FirstCodePart())
                        {
                            case "crackedrock":
                            case "looseboulders":
                            case "looseflints":
                            case "loosestones":
                            case "looseores":
                            case "rock":
                                {
                                    colour = landColour;
                                    heightOpacity = 1;
                                    break;
                                }
                        }
                        break;
                    }

                case EnumBlockMaterial.Leaves:
                    {
                        colour = plantColour;
                        shadow = plantShadowColour;
                        heightOpacity = 0;
                        break;
                    }

                case EnumBlockMaterial.Plant:
                    {
                        switch (block.FirstCodePart())
                        {
                            case "slantedroofing":
                            case "slantedroofingcornerinner":
                            case "slantedroofingcornerouter":
                            case "slantedroofingridge":
                            case "slantedroofingtip":
                                {
                                    break;
                                }

                            default:
                                {
                                    colour = plantColour;
                                    shadow = plantShadowColour;
                                    heightOpacity = 0;
                                    break;
                                }
                        }
                        break;
                    }

                case EnumBlockMaterial.Wood:
                    {
                        if (block.GetType() == typeof(BlockLog))
                        {
                            // Natural blocks.
                            colour = plantColour;
                            shadow = plantShadowColour;
                            heightOpacity = 0;
                        }
                        else
                        {
                            colour = inkColour;
                        }
                        break;
                    }

                case EnumBlockMaterial.Snow:
                    {
                        colour = iceColour;
                        heightOpacity = 1;
                        break;
                    }

                case EnumBlockMaterial.Liquid:
                    {
                        colour = waterColour;
                        gridColour = waterGridColour;

                        if (block.FirstCodePart() == "saltwater")
                        {
                            gridOpacity = oceanGridOpacity;
                            bmp = MedievalMapSystem.oceanTex;
                        }
                        break;
                    }

                case EnumBlockMaterial.Ice:
                    {
                        if (block.GetType() == typeof(BlockGlacierIce))
                        {
                            // Permanent ice.
                            colour = iceColour;
                            heightOpacity = 1;
                        }
                        else
                        {
                            // Frozen water.
                            colour = waterColour;
                        }
                        break;
                    }

                case EnumBlockMaterial.Lava:
                    {
                        // Unusued, but here just in case.
                        colour = lavaColour;
                        break;
                    }
            }

            if (heightOpacity > 0)
            {
                if (y > seaY)
                {
                    colour = ColorUtil.ColorOverlay(colour, highColour, (GameMath.Clamp(y - seaY, 0, worldHalfHeight) / (float)worldHalfHeight) * heightOpacity);
                }
                else if (y < seaY)
                {
                    colour = ColorUtil.ColorOverlay(colour, lowColour, (GameMath.Clamp(seaY - y, 0, worldHalfHeight) / (float)worldHalfHeight) * heightOpacity);
                }
            }

            if (bmp != null)
            {
                // Render texture.
                SKColor pixel = bmp.GetPixel((x + (cx * csize)) % bmp.Width,
                        (z + (cz * csize)) % bmp.Height);
                colour = ColorUtil.ColorOverlay(colour, ColorUtil.ToRgba(pixel.Alpha, pixel.Blue, pixel.Green, pixel.Red), pixel.Alpha / 255f);
            }

            if (x == 0 || z == 0)
            {
                // Render grid.
                return ColorUtil.ColorOverlay(MedievalMapSystem.shade(colour, shadow, slope),
                        gridColour,
                        gridOpacity);
            }
            else
            {
                return MedievalMapSystem.shade(colour, shadow, slope);
            }
        }

        protected static int shade(int colour, int shadow, float slope)
        {
            return ColorUtil.ColorOverlay(colour, shadow, slope);
        }
    }

    [HarmonyPatch(typeof(ChunkMapLayer), "GenerateChunkImage")]
    public class MM_CML_GenerateChunkImage
    {
        private static bool isWater(Block block)
        {
            return block.BlockMaterial == EnumBlockMaterial.Liquid
                    || (block.BlockMaterial == EnumBlockMaterial.Ice && block.GetType() != typeof(BlockGlacierIce));
        }

        static bool Prefix(ChunkMapLayer __instance, ref bool __result, Vec2i chunkPos, IMapChunk mc, ref int[] tintedImage, bool colorAccurate = false)
        {
            Traverse self = Traverse.Create(__instance);
            ICoreAPI api = self.Field("api").GetValue() as ICoreAPI;
            int chunksize = (int)self.Field("chunksize").GetValue();
            IWorldChunk[] chunksTmp = self.Field("chunksTmp").GetValue() as IWorldChunk[];
            ICoreClientAPI capi = self.Field("capi").GetValue() as ICoreClientAPI;

            int worldHalfHeight = (capi.World.BlockAccessor.MapSizeY / 2);
            Vec2i localpos = new Vec2i();

            // Prefetch chunks.
            for (int cy = 0; cy < chunksTmp.Length; cy++)
            {
                chunksTmp[cy] = capi.World.BlockAccessor.GetChunk(chunkPos.X, cy, chunkPos.Y);
                if (chunksTmp[cy] == null || !(chunksTmp[cy] as IClientChunk).LoadedFromServer)
                {
                    __result = false;
                    return false;
                }
            }

            // Prefetch map chunks.
            IMapChunk nwChunk = capi.World.BlockAccessor.GetMapChunk(chunkPos.X - 1, chunkPos.Y - 1);
            IMapChunk wChunk = capi.World.BlockAccessor.GetMapChunk(chunkPos.X - 1, chunkPos.Y);
            IMapChunk nChunk = capi.World.BlockAccessor.GetMapChunk(chunkPos.X, chunkPos.Y - 1);

            for (int i = 0; i < tintedImage.Length; i++)
            {
                int y = mc.RainHeightMap[i];
                int cy = y / chunksize;
                if (cy < chunksTmp.Length)
                {
                    MapUtil.PosInt2d(i, chunksize, localpos);

                    int blockId = chunksTmp[cy].UnpackAndReadBlock(MapUtil.Index3d(localpos.X, y % chunksize, localpos.Y, chunksize, chunksize), BlockLayersAccess.FluidOrSolid);
                    Block block = api.World.Blocks[blockId];

                    if (block.BlockMaterial == EnumBlockMaterial.Snow && block.GetType() != typeof(BlockSnow))
                    {
                        // Check under snow layer.
                        y--;
                        cy = y / chunksize;
                        blockId = chunksTmp[cy].UnpackAndReadBlock(MapUtil.Index3d(localpos.X, y % chunksize, localpos.Y, chunksize, chunksize), BlockLayersAccess.FluidOrSolid);
                        block = api.World.Blocks[blockId];
                    }

                    if (isWater(block))
                    {
                        // Water.
                        IWorldChunk lChunk = chunksTmp[cy];
                        IWorldChunk rChunk = chunksTmp[cy];
                        IWorldChunk tChunk = chunksTmp[cy];
                        IWorldChunk bChunk = chunksTmp[cy];

                        int leftX = localpos.X - 1;
                        int rightX = localpos.X + 1;
                        int topY = localpos.Y - 1;
                        int bottomY = localpos.Y + 1;

                        if (leftX < 0)
                        {
                            lChunk = capi.World.BlockAccessor.GetChunk(chunkPos.X - 1, cy, chunkPos.Y);
                        }
                        if (rightX >= chunksize)
                        {
                            rChunk = capi.World.BlockAccessor.GetChunk(chunkPos.X + 1, cy, chunkPos.Y);
                        }
                        if (topY < 0)
                        {
                            tChunk = capi.World.BlockAccessor.GetChunk(chunkPos.X, cy, chunkPos.Y - 1);
                        }
                        if (bottomY >= chunksize)
                        {
                            bChunk = capi.World.BlockAccessor.GetChunk(chunkPos.X, cy, chunkPos.Y + 1);
                        }

                        if (lChunk != null && rChunk != null && tChunk != null && bChunk != null)
                        {
                            leftX = GameMath.Mod(leftX, chunksize);
                            rightX = GameMath.Mod(rightX, chunksize);
                            topY = GameMath.Mod(topY, chunksize);
                            bottomY = GameMath.Mod(bottomY, chunksize);

                            Block lBlock = api.World.Blocks[lChunk.UnpackAndReadBlock(MapUtil.Index3d(leftX, y % chunksize, localpos.Y, chunksize, chunksize), BlockLayersAccess.FluidOrSolid)];
                            Block rBlock = api.World.Blocks[rChunk.UnpackAndReadBlock(MapUtil.Index3d(rightX, y % chunksize, localpos.Y, chunksize, chunksize), BlockLayersAccess.FluidOrSolid)];
                            Block tBlock = api.World.Blocks[tChunk.UnpackAndReadBlock(MapUtil.Index3d(localpos.X, y % chunksize, topY, chunksize, chunksize), BlockLayersAccess.FluidOrSolid)];
                            Block bBlock = api.World.Blocks[bChunk.UnpackAndReadBlock(MapUtil.Index3d(localpos.X, y % chunksize, bottomY, chunksize, chunksize), BlockLayersAccess.FluidOrSolid)];

                            if (isWater(lBlock) && isWater(rBlock) && isWater(tBlock) && isWater(bBlock))
                            {
                                // Water.
                                tintedImage[i] = MedievalMapSystem.getColour(block, localpos.X, y, localpos.Y, worldHalfHeight, capi.World.SeaLevel, 0, chunkPos.X, chunkPos.Y, chunksize);
                            }
                            else
                            {
                                // Border.
                                tintedImage[i] = MedievalMapSystem.waterEdgeColour;
                            }
                        }
                        else
                        {
                            // Default to water until chunks are loaded.
                            tintedImage[i] = MedievalMapSystem.getColour(block, localpos.X, y, localpos.Y, worldHalfHeight, capi.World.SeaLevel, 0, chunkPos.X, chunkPos.Y, chunksize);
                        }
                    }
                    else
                    {
                        float slope = 0;
                        int tlHeight, blHeight, trHeight;

                        IMapChunk tlChunk = mc;
                        IMapChunk blChunk = mc;
                        IMapChunk trChunk = mc;

                        int leftX = localpos.X - 1;
                        int rightX = localpos.X;
                        int topY = localpos.Y - 1;
                        int bottomY = localpos.Y;

                        if (leftX < 0)
                        {
                            if (topY < 0)
                            {
                                tlChunk = nwChunk;
                                blChunk = wChunk;
                                trChunk = nChunk;
                            }
                            else
                            {
                                tlChunk = wChunk;
                                blChunk = wChunk;
                            }
                        }
                        else
                        {
                            if (topY < 0)
                            {
                                tlChunk = nChunk;
                                trChunk = nChunk;
                            }
                        }

                        leftX = GameMath.Mod(leftX, chunksize);
                        topY = GameMath.Mod(topY, chunksize);

                        tlHeight = tlChunk == null ? 0 : (y - tlChunk.RainHeightMap[topY * chunksize + leftX]);
                        blHeight = blChunk == null ? 0 : (y - blChunk.RainHeightMap[bottomY * chunksize + leftX]);
                        trHeight = trChunk == null ? 0 : (y - trChunk.RainHeightMap[topY * chunksize + rightX]);

                        float slopedir = (Math.Sign(tlHeight) + Math.Sign(blHeight) + Math.Sign(trHeight));
                        float steepness = Math.Max(Math.Max(Math.Abs(tlHeight), Math.Abs(blHeight)), Math.Abs(trHeight));

                        if (slopedir > 0)
                        {
                            slope = Math.Min(slope + Math.Min(0.75f, steepness / 10f), 1);
                        }
                        if (slopedir < 0)
                        {
                            slope = Math.Min(slope + Math.Min(0.75f, steepness / 10f), 1);
                        }

                        tintedImage[i] = MedievalMapSystem.getColour(block, localpos.X, y, localpos.Y, worldHalfHeight, capi.World.SeaLevel, slope, chunkPos.X, chunkPos.Y, chunksize);
                    }
                }
            }

            for (int cy = 0; cy < chunksTmp.Length; cy++)
            {
                chunksTmp[cy] = null;
            }

            __result = true;

            // Skip original method.
            return false;
        }
    }
}