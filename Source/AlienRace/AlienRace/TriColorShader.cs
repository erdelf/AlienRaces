using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace AlienRace
{
    // 4 classes and 2 structs is probably the leanest I can make it, without turning everything unreadable.
    public class TriColorGraphic_Multi : Graphic
    {
        /*
        // Replaces the vanilla Rimworld Pawn graphic class for alien races, is basically identical with the addition of a third color
        // and allowing our shader to be used with masks. The shader itself does not require a third color to be specified,
        // so it works just as well with vanilla pawns and HAR pawns without a third channel, and hasn't caused trouble with any race mod I've tried.
        */
        public Color colorThree = Color.white;
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
        public void Init(TriColorGraphicRequest req)

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
        public TriColorGraphic_Multi GetColoredVersion(

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
        /*
        // RimWorld sure does have a lot of ways to ask for basically the same thing!
        // Not a lot has changed compared to vanilla, we're just using our graphic class for the relevant methods.
        // This is here so RimWorld doesn't have to re-generate the same request for every pawn.
        */
        private static Dictionary<TriColorGraphicRequest, TriColorGraphic_Multi> allGraphics = new Dictionary<TriColorGraphicRequest, TriColorGraphic_Multi>();
        public static TriColorGraphic_Multi Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          GraphicData data)
          where T : TriColorGraphic_Multi, new()
        {
            return (TriColorGraphic_Multi)TriColorGraphicDatabase.GetInner<T>(new TriColorGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, data, 0, (List<ShaderParameter>)null));
        }
        public static TriColorGraphic_Multi Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree)
          where T : TriColorGraphic_Multi, new()
        {
            return (TriColorGraphic_Multi)TriColorGraphicDatabase.GetInner<T>(new TriColorGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, (GraphicData)null, 0, (List<ShaderParameter>)null));
        }
        public static TriColorGraphic_Multi Get(
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

        public static TriColorGraphic_Multi Get(
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
                return (TriColorGraphic_Multi)TriColorGraphicDatabase.GetInner<TriColorGraphic_Multi>(req);
            }
            try
            {
                return (TriColorGraphic_Multi)GenGeneric.InvokeStaticGenericMethod(typeof(TriColorGraphicDatabase), req.graphicClass, "GetInner", (object)req);
            }
            catch (Exception ex)
            {
                Log.Error("Exception getting " + (object)graphicClass + " at " + path + ": " + ex.ToString(), false);
            }
            return (TriColorGraphic_Multi)BaseContent.BadGraphic;
        }

        private static T GetInner<T>(TriColorGraphicRequest req) where T : TriColorGraphic_Multi, new()
        {

            req.color = (Color)(Color32)req.color;
            req.colorTwo = (Color)(Color32)req.colorTwo;
            req.colorThree = (Color)(Color32)req.colorThree;
            TriColorGraphic_Multi graphic;
            if (!TriColorGraphicDatabase.allGraphics.TryGetValue(req, out graphic))
            {
                graphic = (TriColorGraphic_Multi)new T();
                graphic.Init(req);
                TriColorGraphicDatabase.allGraphics.Add(req, graphic);
            }
            return (T)graphic;
        }
    }
    public static class TriColorMaterialPool
    {
        /*
        // Another class that exists so that RimWorld doesn't have to re-generate requests.
        // And again, almost identical to the original code, just have to allow our shader to be used and supply the third color property to the shader.
        // If there's a fast way to tie the request to a specific pawn, none of this would really be needed, but I can't find a good angle to attack that from.
        */
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
        // Struct for the material with a third color.
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
    public struct TriColorGraphicRequest : IEquatable<TriColorGraphicRequest>
    {
        /*
        // Struct for the Graphic with a third color.
        */

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

    [StaticConstructorOnStartup]
    public class TriColorShaderDatabase
    {
        /*
        // Loads and makes the shaderID available to the rest of the mod. 
        */

        public static string dir = AlienRaceMod.settings.Mod.Content.RootDir.ToString();
        public static AssetBundle shaderLoader(string info)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(info);
            return assetBundle;
        }
        static TriColorShaderDatabase()
        {
            // This is where you'd change the path it loads the shader from. 
            string path = dir + "/Shader/TriColorShader";
            AssetBundle bundle = shaderLoader(path);

                                                  // internal assetbundle name, do not change.
            Tricolor = (Shader)bundle.LoadAsset("assets/resources/materials/avalishader.shader");
        }

        public static Shader Tricolor;
        public static int ColorThree = Shader.PropertyToID("_ColorThree");

        public static Shader DefaultShader
        {
            get
            { 
                return TriColorShaderDatabase.Tricolor;
            }
        }
    }
}

