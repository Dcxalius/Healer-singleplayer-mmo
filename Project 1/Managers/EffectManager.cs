using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    internal static class EffectManager
    {
        static Dictionary<string, Effect> effects;

        static List<IEffects> effectsToProcess;
        static Dictionary<IEffects, RenderTarget2D> rendertargets;
        static SpriteBatch spriteBatch;
        static EffectManager()
        {
            effectsToProcess = new List<IEffects>();
            effects = new Dictionary<string, Effect>();
            rendertargets = new Dictionary<IEffects, RenderTarget2D>();
            spriteBatch = GraphicsManager.CreateSpriteBatch();

            string filePath = SaveManager.Effects;
            string[] files = Directory.GetFiles(filePath);
            string debug = "Effects loaded: ";
            for (int i = 0; i < files.Length; i++)
            {
                string name = SaveManager.TrimToNameOnly(files[i]);
                Effect e = Game1.ContentManager.Load<Effect>(name);
                effects.Add(name, e);
                debug += name + ", ";
            }

            DebugManager.Print(typeof(EffectManager), debug);
        }

        public static void AddEffectToProcess(IEffects aEffect) => effectsToProcess.Add(aEffect);

        public static void EffectDraw()
        {
            foreach (var effectToProcess in effectsToProcess)
            {
                if (!rendertargets.TryGetValue(effectToProcess, out RenderTarget2D curRenderT))
                {
                    
                    rendertargets.Add(effectToProcess, GraphicsManager.CreateRenderTarget(effectToProcess.Size));

                    
                }

                EffectParameter e = effects[effectToProcess.Effect].Parameters[effectToProcess.SimpleEffectParam.Name];
                
                switch(effectToProcess.SimpleEffectParam.GetType().Name)
                {
                    case "float":
                        e.SetValue((float)effectToProcess.SimpleEffectParam.value);
                        break;
                    case "float[]":
                        e.SetValue((float[])effectToProcess.SimpleEffectParam.value);
                        break;
                    case "int":
                        e.SetValue((int)effectToProcess.SimpleEffectParam.value);
                        break;
                }

                
                GraphicsManager.SetRenderTarget(curRenderT);
                spriteBatch.Begin(effect: effects[effectToProcess.Effect]);
                GraphicsManager.ClearScreen(Color.Transparent);

                effectToProcess.Texture.Draw(spriteBatch, Vector2.Zero);

                spriteBatch.End();
                GraphicsManager.SetRenderTarget(null);
                effectToProcess.ReturnedRenderTarget = curRenderT;
            }

            effectsToProcess.Clear();
        }

        public struct SimpleEffectParam
        {
            public object value;
            public string Name;

            public SimpleEffectParam(string aName, object aValue)
            {
                value = aValue;
                Name = aName;
            }
        }
    }


}
