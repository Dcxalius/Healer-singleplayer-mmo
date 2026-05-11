using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project_1.Textures
{
    internal static class TextureManager
    {
        const string FALLBACK_FONT = "Comfortaa";
        static Dictionary<string, Texture2D>[] texturesDict;
        static Dictionary<string, Point>[] textureSizes;
        static Dictionary<string, Color>[] avgColors;
        static Dictionary<string, GameFont> fontDict;

        static ContentManager contentManager;
        static bool initialized;

        public static Effect MsdfTextEffect { get; private set; }

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            contentManager = Game1.ContentManager;
            EnsureContentRoot();
            InitArrays();
            InitFonts();
            TextureCatalog.Init(textureSizes, avgColors);
            MsdfTextEffect = contentManager.Load<Effect>("Effects\\MsdfText");
        }

        static void EnsureContentRoot()
        {
            string currentRoot = Path.Combine(AppContext.BaseDirectory, "Content");
            contentManager.RootDirectory = currentRoot;

            bool hasMsdfText = File.Exists(Path.Combine(currentRoot, "Effects", "MsdfText.xnb"));
            if (hasMsdfText) return;

            throw new DirectoryNotFoundException(
                $"Compiled content was not found in the active build output directory '{currentRoot}'.");
        }

        static void InitFonts()
        {
            //TODO: Move fonts out of proj and into monogame pipeline
            string fontDir = Path.Combine(contentManager.RootDirectory, "Font");
            fontDict = AtlasFontLoader.LoadFonts(fontDir);
            if (!Directory.Exists(fontDir))
            {
                FontCache.Init(fontDict, FALLBACK_FONT);
                return;
            }

            string[] compiledFonts = Directory.GetFiles(fontDir, "*.xnb", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < compiledFonts.Length; i++)
            {
                string assetName = GetContentAssetName(compiledFonts[i]);
                string fontName = Path.GetFileNameWithoutExtension(assetName);
                fontDict[fontName] = new SpriteGameFont(fontName, contentManager.Load<SpriteFont>(assetName));
            }

            LoadRuntimeComfortaa(fontDir);

            if (!fontDict.ContainsKey(FALLBACK_FONT) && fontDict.Count > 0)
            {
                fontDict[FALLBACK_FONT] = fontDict.Values.First();
            }

            DebugManager.Print("Fonts loaded: " + string.Join(", ", fontDict.Keys));
            FontCache.Init(fontDict, FALLBACK_FONT);
        }

        static void LoadRuntimeComfortaa(string fontDir)
        {
            string ttfPath = Path.Combine(fontDir, "Comfortaa.ttf");
            if (!File.Exists(ttfPath)) return;

            try
            {
                byte[] bytes = File.ReadAllBytes(ttfPath);
                TtfFontBakerResult baked = TtfFontBaker.Bake(bytes, 12, 1024, 1024, new[] { CharacterRange.BasicLatin });
                SpriteFont comfortaa = baked.CreateSpriteFont(GraphicsManager.GraphicsDevice);
                fontDict["Comfortaa"] = new SpriteGameFont("Comfortaa", comfortaa);
                fontDict[FALLBACK_FONT] = fontDict["Comfortaa"];
            }
            catch (Exception ex)
            {
                DebugManager.Print("Failed to load Comfortaa.ttf at runtime: " + ex.Message);
            }
        }

        static void InitArrays()
        {
            texturesDict = new Dictionary<string, Texture2D>[(int)GfxType.Count];
            textureSizes = new Dictionary<string, Point>[(int)GfxType.Count];
            avgColors = new Dictionary<string, Color>[(int)GfxType.Count];

            string root = contentManager.RootDirectory + "\\Graphics\\";
            string debug = "Textures loaded: ";

            for (int i = 0; i < texturesDict.Length; i++)
            {
                string path = root + (GfxType)i;
                if (!Directory.Exists(path))
                {
                    texturesDict[i] = new Dictionary<string, Texture2D>();
                    textureSizes[i] = new Dictionary<string, Point>();
                    avgColors[i] = new Dictionary<string, Color>();
                    continue;
                }
                string[] dir = Directory.GetFiles(path);

                texturesDict[i] = new Dictionary<string, Texture2D>();
                textureSizes[i] = new Dictionary<string, Point>();
                avgColors[i] = new Dictionary<string, Color>();

                for (int j = 0; j < dir.Length; j++)
                {
                    string filePath = TrimContentFolderAndImageFileExtention(dir[j]);
                    string textureName = filePath.Split('\\').Last();

                    Texture2D texture = contentManager.Load<Texture2D>(filePath);
                    texturesDict[i].Add(textureName, texture);
                    textureSizes[i].Add(textureName, texture.Bounds.Size);
                    avgColors[i].Add(textureName, ComputeAvgColor(texture));
                    debug += textureName + ", ";
                }

                string[] dirsInDir = Directory.GetDirectories(path);

                for (int j = 0; j < dirsInDir.Length; j++)
                {
                    string[] filesInFolders = Directory.GetFiles(dirsInDir[j]);
                    for (int k = 0; k < filesInFolders.Length; k++)
                    {
                        string filePath = TrimContentFolderAndImageFileExtention(filesInFolders[k]);
                        string textureName = filePath.Split('\\').Last();

                        Texture2D texture = contentManager.Load<Texture2D>(filePath);
                        texturesDict[i].Add(textureName, texture);
                        textureSizes[i].Add(textureName, texture.Bounds.Size);
                        avgColors[i].Add(textureName, ComputeAvgColor(texture));
                        debug += textureName + ", ";
                    }
                }
            }

            DebugManager.Print(debug);
        }

        static Color ComputeAvgColor(Texture2D texture)
        {
            Point bounds = texture.Bounds.Size;
            Color[] data = new Color[bounds.X * bounds.Y];
            texture.GetData(data);
            Color avg = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                avg.R = (byte)((avg.R + data[i].R) / 2);
                avg.G = (byte)((avg.G + data[i].G) / 2);
                avg.B = (byte)((avg.B + data[i].B) / 2);
            }
            avg.A = 255;
            return avg;
        }

        static string TrimContentFolderAndImageFileExtention(string aPath)
        {
            string filePath = aPath.Substring(contentManager.RootDirectory.Length + 1);
            return filePath.Substring(0, filePath.Length - 4);
        }

        static string GetContentAssetName(string filePath)
        {
            string relativePath = Path.GetRelativePath(contentManager.RootDirectory, filePath);
            string extension = Path.GetExtension(relativePath);
            string assetName = relativePath.Substring(0, relativePath.Length - extension.Length);
            return assetName.Replace(Path.DirectorySeparatorChar, '\\');
        }

        public static GameFont GetFont(string fontName)
        {
            if (fontDict == null)
                throw new InvalidOperationException("TextureManager fonts not initialized.");

            try
            {
                return FontCache.GetFont(fontName);
            }
            catch (KeyNotFoundException)
            {
                DebugManager.Print($"Font '{fontName}' not found. Falling back to '{FALLBACK_FONT}'.");
                return FontCache.GetFont(FALLBACK_FONT);
            }
        }

        public static Texture2D GetTexture(GfxPath aGfxPath)
        {
            ThreadAffinity.AssertMainThread();
            if (texturesDict == null)
                throw new InvalidOperationException("TextureManager: texturesDict is null. InitArrays() / static ctor did not run.");

            int typeIndex = (int)aGfxPath.Type;
            if (typeIndex < 0 || typeIndex >= texturesDict.Length)
                throw new ArgumentOutOfRangeException(nameof(aGfxPath), $"Invalid GfxType index: {typeIndex}");

            var dict = texturesDict[typeIndex];

            if (!dict.TryGetValue(aGfxPath.Name, out var texture))
            {
                DebugManager.Print("Texture " + aGfxPath.Name + " from type " + aGfxPath.Type + " was not found.");

                var debugDict = texturesDict[(int)GfxType.Debug];

                if (!debugDict.TryGetValue("MissingTexture", out texture))
                    throw new KeyNotFoundException("Fallback texture 'MissingTexture' not found in GfxType.Debug.");
            }

            return texture;
        }

        public static Point GetTextureSize(GfxPath aGfxPath)
        {
            ThreadAffinity.AssertMainThread();
            return TextureCatalog.GetSize(aGfxPath);
        }

        public static Color GetAvgColor(GfxPath aGfxPath)
        {
            ThreadAffinity.AssertMainThread();
            return TextureCatalog.GetAvgColor(aGfxPath);
        }
    }
}
