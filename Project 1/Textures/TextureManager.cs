using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Project_1.Textures
{
    internal static class TextureManager
    {
        const string FALLBACK_FONT = "Gloryse";
        static Dictionary<string, Texture2D>[] texturesDict;
        static Dictionary<string, Point>[] textureSizes;
        static Dictionary<string, Color>[] avgColors;
        static Dictionary<string, SpriteFont> fontDict;//TODO: Font is not open source so need to be change at some point

        static ContentManager contentManager;
        static bool initialized;

        public static Effect textOutline;

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
            textOutline = contentManager.Load<Effect>("Effects\\TextOutline");
            //textOutline.Parameters["texelSize"].SetValue()
        }

        static void EnsureContentRoot()
        {
            string baseDir = AppContext.BaseDirectory;
            string currentRoot = Path.GetFullPath(Path.Combine(baseDir, contentManager.RootDirectory));
            string[] candidates =
            {
                currentRoot,
                Path.Combine(baseDir, "Content"),
                Path.Combine(baseDir, "..", "..", "..", "..", "Content", "bin", "DesktopGL", "Content"),
                Path.Combine(baseDir, "..", "..", "..", "..", "Content", "bin", "Windows"),
                Path.Combine(baseDir, "..", "..", "..", "..", "Content", "bin", "Windows", "Content")
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                string candidate = Path.GetFullPath(candidates[i]);
                if (!File.Exists(Path.Combine(candidate, "Effects", "TextOutline.xnb"))) continue;
                contentManager.RootDirectory = candidate;
                return;
            }

            for (int i = 0; i < candidates.Length; i++)
            {
                string candidate = Path.GetFullPath(candidates[i]);
                if (!Directory.Exists(Path.Combine(candidate, "Graphics"))) continue;
                contentManager.RootDirectory = candidate;
                return;
            }
        }

        static void InitFonts()
        {
            fontDict = new Dictionary<string, SpriteFont>();
            string debug = "Fonts loaded: ";

            string fontDir = Path.Combine(contentManager.RootDirectory, "Font");
            if (!Directory.Exists(fontDir))
            {
                FontCache.Init(fontDict, FALLBACK_FONT);
                return;
            }
            string[] dir = Directory.GetFiles(fontDir);


            for (int i = 0; i < dir.Length; i++)
            {
                string filePath = TrimContentFolderAndImageFileExtention(dir[i]);
                string fontName = filePath.Split('\\')[1];

                fontDict.Add(fontName, contentManager.Load<SpriteFont>(filePath));
                debug += fontName + ", ";

            }

            DebugManager.Print(debug);
            FontCache.Init(fontDict, FALLBACK_FONT);
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


        public static SpriteFont GetFont(string fontName)
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
            // Basic safety checks
            if (texturesDict == null)
                throw new InvalidOperationException("TextureManager: texturesDict is null. InitArrays() / static ctor did not run.");

            int typeIndex = (int)aGfxPath.Type;
            if (typeIndex < 0 || typeIndex >= texturesDict.Length)
                throw new ArgumentOutOfRangeException(nameof(aGfxPath), $"Invalid GfxType index: {typeIndex}");

            var dict = texturesDict[typeIndex];

            if (!dict.TryGetValue(aGfxPath.Name, out var texture))
            {
                DebugManager.Print("Texture " + aGfxPath.Name + " from type " + aGfxPath.Type + " was not found."
                );

                // Fallback to MissingTexture in the Debug gfx type
                var debugDict = texturesDict[(int)GfxType.Debug];

                if (!debugDict.TryGetValue("MissingTexture", out texture))
                    throw new KeyNotFoundException(
                        "Fallback texture 'MissingTexture' not found in GfxType.Debug."
                    );
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
