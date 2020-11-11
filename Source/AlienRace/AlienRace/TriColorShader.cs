using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace AlienRace
{
    public class TriColorGraphic : Graphic
        {

            public Color colorThree = Color.white;

            public Color ColorThree
            {
                get
                {
                    return this.colorThree;
                }
            }

            public override Material MatSingle
            {
                get
                {
                    return BaseContent.BadMat;
                }
            }

            public override Material MatWest
            {
                get
                {
                    return this.MatSingle;
                }
            }

            public override Material MatSouth
            {
                get
                {
                    return this.MatSingle;
                }
            }

            public override Material MatEast
            {
                get
                {
                    return this.MatSingle;
                }
            }

            public override Material MatNorth
            {
                get
                {
                    return this.MatSingle;
                }
            }

            public override bool WestFlipped
            {
                get
                {
                    return this.DataAllowsFlip && !this.ShouldDrawRotated;
                }
            }
            public override bool ShouldDrawRotated
            {
                get
                {
                    return false;
                }
            }

            public override float DrawRotatedExtraAngleOffset
            {
                get
                {
                    return 0.0f;
                }
            }

            public override bool UseSameGraphicForGhost
            {
                get
                {
                    return false;
                }
            }


            public virtual void Init(TriColorGraphicRequest req)
            {
                Log.ErrorOnce("Cannot init Graphic of class " + this.GetType().ToString(), 658928, false);
            }

            protected override void DrawMeshInt(Mesh mesh, Vector3 loc, Quaternion quat, Material mat)
            {
                Graphics.DrawMesh(mesh, loc, quat, mat, 0);
            }

            public override void Print(SectionLayer layer, Thing thing)
            {
                Vector2 size;
                bool flipUv;
                if (this.ShouldDrawRotated)
                {
                    size = this.drawSize;
                    flipUv = false;
                }
                else
                {
                    size = thing.Rotation.IsHorizontal ? this.drawSize.Rotated() : this.drawSize;
                    flipUv = thing.Rotation == Rot4.West && this.WestFlipped || thing.Rotation == Rot4.East && this.EastFlipped;
                }
                float rot = this.AngleFromRot(thing.Rotation);
                if (flipUv && this.data != null)
                    rot += this.data.flipExtraRotation;
                Vector3 center = thing.TrueCenter() + this.DrawOffset(thing.Rotation);
                Printer_Plane.PrintPlane(layer, center, size, this.MatAt(thing.Rotation, thing), rot, flipUv, (Vector2[])null, (Color32[])null, 0.01f, 0.0f);
                if (this.ShadowGraphic == null || thing == null)
                    return;
                this.ShadowGraphic.Print(layer, thing);
            }

            public virtual TriColorGraphic GetColoredVersion(
              Shader newShader,
              Color newColor,
              Color newColorTwo,
              Color newColorThree)
            {
                Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + this.GetType().ToString(), 66300, false);
                return (TriColorGraphic)BaseContent.BadGraphic;
            }

            public virtual TriColorGraphic GetCopy(Vector2 newDrawSize)
            {
                return TriColorGraphicDatabase.Get(this.GetType(),
                                                this.path,
                                                this.Shader,
                                                newDrawSize,
                                                this.color,
                                                this.colorTwo,
                                                this.colorThree);
            }
        }
    public class TriColorGraphic_Multi : TriColorGraphic
    {
        private Material[] mats = new Material[4];
        private bool westFlipped;
        private bool eastFlipped;
        private float drawRotatedExtraAngleOffset;

        public string GraphicPath
        {
            get
            {
                return this.path;
            }
        }

        public override Material MatSingle
        {
            get
            {

                return this.MatSouth;
            }
        }

        public override Material MatWest
        {
            get
            {
                return this.mats[3];
            }
        }

        public override Material MatSouth
        {
            get
            {
                return this.mats[2];
            }
        }

        public override Material MatEast
        {
            get
            {
                return this.mats[1];
            }
        }

        public override Material MatNorth
        {
            get
            {
                return this.mats[0];
            }
        }

        public override bool WestFlipped
        {
            get
            {
                return this.westFlipped;
            }
        }

        public override bool EastFlipped
        {
            get
            {
                return this.eastFlipped;
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                if (this.data != null && !this.data.drawRotated)
                    return false;
                return this.MatEast == this.MatNorth || this.MatWest == this.MatNorth;
            }
        }

        public override float DrawRotatedExtraAngleOffset
        {
            get
            {
                return this.drawRotatedExtraAngleOffset;
            }
        }

        public override void Init(TriColorGraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.colorThree = req.colorThree;
            this.drawSize = req.drawSize;
            Texture2D[] texture2DArray1 = new Texture2D[this.mats.Length];
            texture2DArray1[0] = ContentFinder<Texture2D>.Get(req.path + "_north", false);
            texture2DArray1[1] = ContentFinder<Texture2D>.Get(req.path + "_east", false);
            texture2DArray1[2] = ContentFinder<Texture2D>.Get(req.path + "_south", false);
            texture2DArray1[3] = ContentFinder<Texture2D>.Get(req.path + "_west", false);
            if (texture2DArray1[0] == null)
            {
                if (texture2DArray1[2] != null)
                {
                    texture2DArray1[0] = texture2DArray1[2];
                    this.drawRotatedExtraAngleOffset = 180f;
                }
                else if (texture2DArray1[1] != null)
                {
                    texture2DArray1[0] = texture2DArray1[1];
                    this.drawRotatedExtraAngleOffset = -90f;
                }
                else if (texture2DArray1[3] != null)
                {
                    texture2DArray1[0] = texture2DArray1[3];
                    this.drawRotatedExtraAngleOffset = 90f;
                }
                else
                    texture2DArray1[0] = ContentFinder<Texture2D>.Get(req.path, false);
            }
            if (texture2DArray1[0] == null)
            {
                Log.Error("Failed to find any textures at " + req.path + " while constructing " + this.ToStringSafe<TriColorGraphic_Multi>(), false);
            }

            else
            {
                if (texture2DArray1[2] == null)
                    texture2DArray1[2] = texture2DArray1[0];
                if (texture2DArray1[1] == null)
                {
                    if (texture2DArray1[3] != null)
                    {
                        texture2DArray1[1] = texture2DArray1[3];
                        this.eastFlipped = this.DataAllowsFlip;
                    }
                    else
                        texture2DArray1[1] = texture2DArray1[0];
                }
                if (texture2DArray1[3] == null)
                {
                    if (texture2DArray1[1] != null)
                    {
                        texture2DArray1[3] = texture2DArray1[1];
                        this.westFlipped = this.DataAllowsFlip;
                    }
                    else
                        texture2DArray1[3] = texture2DArray1[0];
                }
                Texture2D[] texture2DArray2 = new Texture2D[this.mats.Length];
                if (req.shader == TriColorShaderDatabase.Tricolor)
                {
                    texture2DArray2[0] = ContentFinder<Texture2D>.Get(req.path + "_northm", false);
                    texture2DArray2[1] = ContentFinder<Texture2D>.Get(req.path + "_eastm", false);
                    texture2DArray2[2] = ContentFinder<Texture2D>.Get(req.path + "_southm", false);
                    texture2DArray2[3] = ContentFinder<Texture2D>.Get(req.path + "_westm", false);
                    if (texture2DArray2[0] == null)
                    {
                        if (texture2DArray2[2] != null)
                            texture2DArray2[0] = texture2DArray2[2];
                        else if (texture2DArray2[1] != null)
                            texture2DArray2[0] = texture2DArray2[1];
                        else if (texture2DArray2[3] != null)
                            texture2DArray2[0] = texture2DArray2[3];
                    }
                    if (texture2DArray2[2] == null)
                        texture2DArray2[2] = texture2DArray2[0];
                    if (texture2DArray2[1] == null)
                        texture2DArray2[1] = !(texture2DArray2[3] != null) ? texture2DArray2[0] : texture2DArray2[3];
                    if (texture2DArray2[3] == null)
                        texture2DArray2[3] = !(texture2DArray2[1] != null) ? texture2DArray2[0] : texture2DArray2[1];
                }
                for (int index = 0; index < this.mats.Length; ++index)
                {
                    this.mats[index] = TriColorMaterialPool.MatFrom(new TriColorMaterialRequest()
                    {
                        mainTex = texture2DArray1[index],
                        shader = req.shader,
                        color = this.color,
                        colorTwo = this.colorTwo,
                        colorThree = this.colorThree,
                        maskTex = texture2DArray2[index],
                        shaderParameters = req.shaderParameters
                    });

                };
            }
        }

        public override TriColorGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            return TriColorGraphicDatabase.Get<TriColorGraphic_Multi>(this.path, newShader, this.drawSize, newColor, newColorTwo, newColorThree, this.data);
        }

        public override string ToString()
        {
            return "Multi(initPath=" + this.path + ", color=" + (object)this.color + ", colorTwo=" + (object)this.colorTwo + ")";
        }

        public override int GetHashCode()
        {
            return Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombine<string>(0, this.path), this.color), this.colorTwo), this.ColorThree);
        }
    }
    public static class TriColorGraphicDatabase
    {
        private static Dictionary<TriColorGraphicRequest, TriColorGraphic> allGraphics = new Dictionary<TriColorGraphicRequest, TriColorGraphic>();
        /*
        public static AvaliGraphic Get<T>(string path) where T : AvaliGraphic, new()
        {
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white, Color.white, (GraphicData)null, 0, (List<ShaderParameter>)null));
        }

        public static AvaliGraphic Get<T>(string path, Shader shader) where T : AvaliGraphic, new()
        {
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, Vector2.one, Color.white, Color.white, Color.white, (GraphicData)null, 0, (List<ShaderParameter>)null));
        }

        public static AvaliGraphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color) where T : AvaliGraphic, new()
        {
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, Color.white, (GraphicData)null, 0, (List<ShaderParameter>)null));
        }

        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          int renderQueue)
          where T : AvaliGraphic, new()
        {
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, Color.white, (GraphicData)null, renderQueue, (List<ShaderParameter>)null));
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
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, (GraphicData)null, 0, (List<ShaderParameter>)null));
        }
        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo)
          where T : AvaliGraphic, new()
        {
            return (AvaliGraphic)AvaliGraphicDatabase.GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, Color.white, (GraphicData)null, 0, (List<ShaderParameter>)null));
        }
        */
        public static TriColorGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          GraphicData data)
          where T : TriColorGraphic, new()
        {
            return (TriColorGraphic)TriColorGraphicDatabase.GetInner<T>(new TriColorGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, data, 0, (List<ShaderParameter>)null));
        }

        public static TriColorGraphic Get(
          System.Type graphicClass,
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree)
        {
            return TriColorGraphicDatabase.Get(graphicClass, path, shader, drawSize, color, colorTwo, colorThree, (GraphicData)null, (List<ShaderParameter>)null);
        }

        public static TriColorGraphic Get(
          System.Type graphicClass,
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          GraphicData data,
          List<ShaderParameter> shaderParameters)
        {
            TriColorGraphicRequest req = new TriColorGraphicRequest(graphicClass, path, shader, drawSize, color, colorTwo, colorThree, data, 0, shaderParameters);
            if (req.graphicClass == typeof(Graphic_Multi))
            {
                return (TriColorGraphic)TriColorGraphicDatabase.GetInner<TriColorGraphic_Multi>(req);
            }
            try
            {
                return (TriColorGraphic)GenGeneric.InvokeStaticGenericMethod(typeof(TriColorGraphicDatabase), req.graphicClass, "GetInner", (object)req);
            }
            catch (Exception ex)
            {
                Log.Error("Exception getting " + (object)graphicClass + " at " + path + ": " + ex.ToString(), false);
            }
            return (TriColorGraphic)BaseContent.BadGraphic;
        }

        private static T GetInner<T>(TriColorGraphicRequest req) where T : TriColorGraphic, new()
        {

            req.color = (Color)(Color32)req.color;
            req.colorTwo = (Color)(Color32)req.colorTwo;
            req.colorThree = (Color)(Color32)req.colorThree;
            TriColorGraphic graphic;
            if (!TriColorGraphicDatabase.allGraphics.TryGetValue(req, out graphic))
            {
                graphic = (TriColorGraphic)new T();
                graphic.Init(req);
                TriColorGraphicDatabase.allGraphics.Add(req, graphic);
            }
            return (T)graphic;
        }
        /*
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
        */
    }
    public struct TriColorGraphicRequest : IEquatable<TriColorGraphicRequest>
    {
        public System.Type graphicClass;
        public string path;
        public Shader shader;
        public Vector2 drawSize;
        public Color color;
        public Color colorTwo;
        public Color colorThree;
        public GraphicData graphicData;
        public int renderQueue;
        public List<ShaderParameter> shaderParameters;

        public TriColorGraphicRequest(
          System.Type graphicClass,
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          GraphicData graphicData,
          int renderQueue,
          List<ShaderParameter> shaderParameters)
        {
            this.graphicClass = graphicClass;
            this.path = path;
            this.shader = shader;
            this.drawSize = drawSize;
            this.color = color;
            this.colorTwo = colorTwo;
            this.colorThree = colorThree;
            this.graphicData = graphicData;
            this.renderQueue = renderQueue;
            this.shaderParameters = shaderParameters.NullOrEmpty<ShaderParameter>() ? (List<ShaderParameter>)null : shaderParameters;
        }

        public override int GetHashCode()
        {
            if (this.path == null)
                this.path = BaseContent.BadTexPath;
            return Gen.HashCombine<List<ShaderParameter>>(Gen.HashCombine<int>(Gen.HashCombine<GraphicData>(Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Vector2>(Gen.HashCombine<Shader>(Gen.HashCombine<string>(Gen.HashCombine<System.Type>(0, this.graphicClass), this.path), this.shader), this.drawSize), this.color), this.colorTwo), this.graphicData), this.renderQueue), this.shaderParameters);
        }

        public override bool Equals(object obj)
        {
            return obj is TriColorGraphicRequest other && this.Equals(other);
        }

        public bool Equals(TriColorGraphicRequest other)
        {
            return this.graphicClass == other.graphicClass && this.path == other.path && ((UnityEngine.Object)this.shader == (UnityEngine.Object)other.shader && this.drawSize == other.drawSize) && (this.color == other.color && this.colorTwo == other.colorTwo && (this.graphicData == other.graphicData && this.renderQueue == other.renderQueue)) && this.shaderParameters == other.shaderParameters;
        }

        public static bool operator ==(TriColorGraphicRequest lhs, TriColorGraphicRequest rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TriColorGraphicRequest lhs, TriColorGraphicRequest rhs)
        {
            return !(lhs == rhs);
        }
    }
    public static class TriColorMaterialPool
    {
        public static Material MatFrom(TriColorMaterialRequest req)
        {
            if (!UnityData.IsInMainThread)
            {
                Log.Error("Tried to get a material from a different thread.", false);
                return null;
            }
            if (req.mainTex == null)
            {
                Log.Error("MatFrom with null sourceTex.", false);
                return BaseContent.BadMat;
            }
            if (req.shader == null)
            {
                Log.Warning("Matfrom with null shader.", false);
                return BaseContent.BadMat;
            }
            Material material;
            if (!TriColorMaterialPool.matDictionary.TryGetValue(req, out material))
            {
                material = new Material(req.shader);
                material.name = req.shader.name + "_" + req.mainTex.name;
                material.mainTexture = req.mainTex;
                material.color = req.color;
                material.SetTexture(ShaderPropertyIDs.MaskTex, req.maskTex);
                material.SetColor(ShaderPropertyIDs.ColorTwo, req.colorTwo);
                material.SetColor(TriColorShaderDatabase.ColorThree, req.colorThree);
                material.SetTexture(ShaderPropertyIDs.MaskTex, req.maskTex);
                if (req.renderQueue != 0)
                {
                    material.renderQueue = req.renderQueue;
                }
                if (!req.shaderParameters.NullOrEmpty<ShaderParameter>())
                {
                    for (int i = 0; i < req.shaderParameters.Count; i++)
                    {
                        req.shaderParameters[i].Apply(material);
                    }
                }
                TriColorMaterialPool.matDictionary.Add(req, material);
                if (req.shader == ShaderDatabase.CutoutPlant || req.shader == ShaderDatabase.TransparentPlant)
                {
                    WindManager.Notify_PlantMaterialCreated(material);
                }
            }
            return material;
        }

        private static Dictionary<TriColorMaterialRequest, Material> matDictionary = new Dictionary<TriColorMaterialRequest, Material>();
    }
    public struct TriColorMaterialRequest : IEquatable<TriColorMaterialRequest>
    {
        /*
        public string BaseTexPath
        {
            set
            {
                this.mainTex = ContentFinder<Texture2D>.Get(value, true);
            }
        }

        public AvaliMaterialRequest(Texture2D tex)
        {
            this.shader = ShaderDatabase.Cutout;
            this.mainTex = tex;
            this.color = Color.red;
            this.colorTwo = Color.green;
            this.colorThree = Color.blue;
            this.maskTex = null;
            this.renderQueue = 0;
            this.shaderParameters = null;
        }

        public AvaliMaterialRequest(Texture2D tex, Shader shader)
        {
            this.shader = shader;
            this.mainTex = tex;
            this.color = Color.green;
            this.colorTwo = Color.blue;
            this.colorThree = Color.red;
            this.maskTex = null;
            this.renderQueue = 0;
            this.shaderParameters = null;
        }

        public AvaliMaterialRequest(Texture2D tex, Shader shader, Color color)
        {
            this.shader = shader;
            this.mainTex = tex;
            this.color = color;
            this.colorTwo = Color.red;
            this.colorThree = Color.blue;
            this.maskTex = null;
            this.renderQueue = 0;
            this.shaderParameters = null;
        }
        */
        public override int GetHashCode()
        {
            return Gen.HashCombine<List<ShaderParameter>>(Gen.HashCombineInt(Gen.HashCombine<Texture2D>(Gen.HashCombine<Texture2D>(Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombine<Shader>(0, this.shader), this.color), this.colorTwo), this.mainTex), this.maskTex), this.renderQueue), this.shaderParameters);
        }

        public override bool Equals(object obj)
        {
            return obj is TriColorMaterialRequest && this.Equals((TriColorMaterialRequest)obj);
        }

        public bool Equals(TriColorMaterialRequest other)
        {
            return other.shader == this.shader && other.mainTex == this.mainTex && other.color == this.color && other.colorTwo == this.colorTwo && other.maskTex == this.maskTex && other.renderQueue == this.renderQueue && other.shaderParameters == this.shaderParameters;
        }

        public static bool operator ==(TriColorMaterialRequest lhs, TriColorMaterialRequest rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TriColorMaterialRequest lhs, TriColorMaterialRequest rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "AvaliMaterialRequest(",
                this.shader.name,
                ", ",
                this.mainTex.name,
                ", ",
                this.color.ToString(),
                ", ",
                this.colorTwo.ToString(),
                ", ",
                this.colorThree.ToString(),
                ",",
                this.maskTex.ToString(),
                ", ",
                this.renderQueue.ToString(),
                ")"
            });
        }

        public Shader shader;
        public Texture2D mainTex;
        public Color color;
        public Color colorTwo;
        public Texture2D maskTex;
        public int renderQueue;
        public List<ShaderParameter> shaderParameters;
        public Color colorThree;
    }
    /*
    [StaticConstructorOnStartup]
    public static class AvaliShaderPropertyIDs
    {
        
        static AvaliShaderPropertyIDs()
        {
            AvaliShaderPropertyIDs.MaskTexName = "_MaskTex";
            AvaliShaderPropertyIDs.SwayHeadName = "_SwayHead";
            AvaliShaderPropertyIDs.ShockwaveSpanName = "_ShockwaveSpan";
            AvaliShaderPropertyIDs.AgeSecsName = "_AgeSecs";
            AvaliShaderPropertyIDs.PlanetSunLightDirection = Shader.PropertyToID(AvaliShaderPropertyIDs.PlanetSunLightDirectionName);
            AvaliShaderPropertyIDs.PlanetSunLightEnabled = Shader.PropertyToID(AvaliShaderPropertyIDs.PlanetSunLightEnabledName);
            AvaliShaderPropertyIDs.PlanetRadius = Shader.PropertyToID(AvaliShaderPropertyIDs.PlanetRadiusName);
            AvaliShaderPropertyIDs.MapSunLightDirection = Shader.PropertyToID(AvaliShaderPropertyIDs.MapSunLightDirectionName);
            AvaliShaderPropertyIDs.GlowRadius = Shader.PropertyToID(AvaliShaderPropertyIDs.GlowRadiusName);
            AvaliShaderPropertyIDs.GameSeconds = Shader.PropertyToID(AvaliShaderPropertyIDs.GameSecondsName);
            AvaliShaderPropertyIDs.AgeSecs = Shader.PropertyToID(AvaliShaderPropertyIDs.AgeSecsName);
            AvaliShaderPropertyIDs.Color = Shader.PropertyToID(AvaliShaderPropertyIDs.ColorName);
            AvaliShaderPropertyIDs.ColorTwo = Shader.PropertyToID(AvaliShaderPropertyIDs.ColorTwoName);
            //AvaliShaderPropertyIDs.ColorThree = Shader.PropertyToID(AvaliShaderPropertyIDs.ColorThreeName);
            AvaliShaderPropertyIDs.MaskTex = Shader.PropertyToID(AvaliShaderPropertyIDs.MaskTexName);
            AvaliShaderPropertyIDs.SwayHead = Shader.PropertyToID(AvaliShaderPropertyIDs.SwayHeadName);
            AvaliShaderPropertyIDs.ShockwaveColor = Shader.PropertyToID("_ShockwaveColor");
            AvaliShaderPropertyIDs.ShockwaveSpan = Shader.PropertyToID(AvaliShaderPropertyIDs.ShockwaveSpanName);
            AvaliShaderPropertyIDs.WaterCastVectSun = Shader.PropertyToID("_WaterCastVectSun");
            AvaliShaderPropertyIDs.WaterCastVectMoon = Shader.PropertyToID("_WaterCastVectMoon");
            AvaliShaderPropertyIDs.WaterOutputTex = Shader.PropertyToID("_WaterOutputTex");
            AvaliShaderPropertyIDs.WaterOffsetTex = Shader.PropertyToID("_WaterOffsetTex");
            AvaliShaderPropertyIDs.ShadowCompositeTex = Shader.PropertyToID("_ShadowCompositeTex");
            AvaliShaderPropertyIDs.FallIntensity = Shader.PropertyToID("_FallIntensity");
        }

        private static readonly string PlanetSunLightDirectionName = "_PlanetSunLightDirection";
        private static readonly string PlanetSunLightEnabledName = "_PlanetSunLightEnabled";
        private static readonly string PlanetRadiusName = "_PlanetRadius";
        private static readonly string MapSunLightDirectionName = "_CastVect";
        private static readonly string GlowRadiusName = "_GlowRadius";
        private static readonly string GameSecondsName = "_GameSeconds";
        private static readonly string ColorName = "_Color";
        private static readonly string ColorTwoName = "_ColorTwo";
        private static readonly string MaskTexName;
        private static readonly string SwayHeadName;
        private static readonly string ShockwaveSpanName;
        private static readonly string AgeSecsName;
        public static int PlanetSunLightDirection;
        public static int PlanetSunLightEnabled;
        public static int PlanetRadius;
        public static int MapSunLightDirection;
        public static int GlowRadius;
        public static int GameSeconds;
        public static int AgeSecs;
        public static int Color;
        public static int ColorTwo;
        public static int MaskTex;
        public static int SwayHead;
        public static int ShockwaveColor;
        public static int ShockwaveSpan;
        public static int WaterCastVectSun;
        public static int WaterCastVectMoon;
        public static int WaterOutputTex;
        public static int WaterOffsetTex;
        public static int ShadowCompositeTex;
        public static int FallIntensity;
        //private static readonly string ColorThreeName = "_ColorThree";
        //public static int ColorThree;
        
    }
        */
    [StaticConstructorOnStartup]
    public class TriColorShaderDatabase
    {

        static TriColorShaderDatabase()
        {
            string dir = RimValiUtility.dir;

            string path = dir + "/Shader/TriColorShader";
            AssetBundle bundle = RimValiUtility.shaderLoader(path);
            Tricolor = (Shader)bundle.LoadAsset("assets/resources/materials/avalishader.shader");
        }

        public static Shader Tricolor;
        public static int ColorThree = Shader.PropertyToID("_ColorThree");
        public static Dictionary<string, Shader> lookup;

        public static Shader DefaultShader
        {
            get
            {
                return TriColorShaderDatabase.Tricolor;
            }
        }
    }
    public static class RimValiUtility
    {
        public static string dir = AlienRaceMod.settings.Mod.Content.RootDir.ToString(); //this.mod.RootDir.ToString();
        public static AssetBundle shaderLoader(string info)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(info);
            return assetBundle;
        }
    }

}

