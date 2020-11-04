using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
namespace AvaliMod
{
    public static class AvaliGraphicDatabase
    {
        private static Dictionary<AvaliGraphicRequest, AvaliGraphic> allGraphics = new Dictionary<AvaliGraphicRequest, AvaliGraphic>();

        private static Shader shadertest;

        public static AvaliGraphic Get<T>(string path) where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got8");
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white, Color.white, (AvaliGraphicData)null, 0, (List<ShaderParameter>)null));
        }

        public static AvaliGraphic Get<T>(string path, Shader shader) where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got7" + shader.name + " Color3: ");
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, Vector2.one, Color.white, Color.white, Color.white, (AvaliGraphicData)null, 0, (List<ShaderParameter>)null));
        }

        public static AvaliGraphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color) where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got6" + shader.name + " Color3: ");
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, Color.white, (AvaliGraphicData)null, 0, (List<ShaderParameter>)null));
        }

        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          int renderQueue)
          where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got5");
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, Color.white, (AvaliGraphicData)null, renderQueue, (List<ShaderParameter>)null));
        }

        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree)
          where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got4");
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, (AvaliGraphicData)null, 0, (List<ShaderParameter>)null));
        }
        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo)
          where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got3: " + shader.name + " Color3: ");
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, Color.white, (AvaliGraphicData)null, 0, (List<ShaderParameter>)null));
        }
        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          AvaliGraphicData data)
          where T : AvaliGraphic, new()
        {
            //if (shader.name != "Custom/Cutout") { shadertest = shader; }
            //if (shader.name == "Custom/Cutout") { shader = shadertest; }
            //Log.Message("Was Got2: " + shader.name + " Color3: " + colorThree);
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, data, 0, (List<ShaderParameter>)null));
        }

        public static AvaliGraphic Get(
          System.Type graphicClass,
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree)
        {
            //Log.Message("Was Got1");
            return AvaliGraphicDatabase.Get(graphicClass, path, shader, drawSize, color, colorTwo, colorThree, (AvaliGraphicData)null, (List<ShaderParameter>)null);
        }

        public static AvaliGraphic Get(
          System.Type graphicClass,
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          AvaliGraphicData data,
          List<ShaderParameter> shaderParameters)
        {
            AvaliGraphicRequest req = new AvaliGraphicRequest(graphicClass, path, shader, drawSize, color, colorTwo, colorThree, data, 0, shaderParameters);
            // liQdComment 2 This is what the game requests 
            if (req.graphicClass == typeof(Graphic_Multi))
            {
                //Log.Message("AvaliGraphic request of type Graphic_Multi");
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_Multi>(req);
            }
            if (req.graphicClass == typeof(AvaliGraphic_Single))
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_Single>(req);
            if (req.graphicClass == typeof(AvaliGraphic_Terrain))
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_Terrain>(req);
            if (req.graphicClass == typeof(AvaliGraphic_Multi))
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_Multi>(req);
            if (req.graphicClass == typeof(AvaliGraphic_Mote))
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_Mote>(req);
            if (req.graphicClass == typeof(AvaliGraphic_Random))
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_Random>(req);
            if (req.graphicClass == typeof(AvaliGraphic_Flicker))
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_Flicker>(req);
            if (req.graphicClass == typeof(AvaliGraphic_Appearances))
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_Appearances>(req);
            if (req.graphicClass == typeof(AvaliGraphic_StackCount))
                return (AvaliGraphic)AvaliGraphicDatabase.GetInner<AvaliGraphic_StackCount>(req);
            try
            {
                return (AvaliGraphic)GenGeneric.InvokeStaticGenericMethod(typeof(AvaliGraphicDatabase), req.graphicClass, "GetInner", (object)req);
            }
            catch (Exception ex)
            {
                Log.Error("Exception getting " + (object)graphicClass + " at " + path + ": " + ex.ToString(), false);
            }
            return AvaliBaseContent.BadGraphic;
        }
        
        private static T GetInner<T>(AvaliGraphicRequest req) where T : AvaliGraphic, new()
        {

            req.color = (Color)(Color32)req.color;
            req.colorTwo = (Color)(Color32)req.colorTwo;
            req.colorThree = (Color)(Color32)req.colorThree;
            AvaliGraphic graphic;
            if (!AvaliGraphicDatabase.allGraphics.TryGetValue(req, out graphic))
            {
                graphic = (AvaliGraphic)new T();
                graphic.Init(req);
                AvaliGraphicDatabase.allGraphics.Add(req, graphic);
            }
            return (T)graphic;
        }
        
        public static void Clear()
        {
            AvaliGraphicDatabase.allGraphics.Clear();
        }

        [DebugOutput("System", false)]
        public static void AllGraphicsLoaded()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("There are " + (object)AvaliGraphicDatabase.allGraphics.Count + " graphics loaded.");
            int num = 0;
            foreach (AvaliGraphic graphic in AvaliGraphicDatabase.allGraphics.Values)
            {
                stringBuilder.AppendLine(num.ToString() + " - " + graphic.ToString());
                if (num % 50 == 49)
                {
                    Log.Message(stringBuilder.ToString(), false);
                    stringBuilder = new StringBuilder();
                }
                ++num;
            }
            Log.Message(stringBuilder.ToString(), false);
        }
    }
}