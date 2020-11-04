using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
namespace AvaliMod
{
    public class AvaliGraphic_Appearances : AvaliGraphic
    {
        protected AvaliGraphic[] subGraphics;

        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[(int)StuffAppearanceDefOf.Smooth.index].MatSingle;
            }
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return this.SubGraphicFor(thing).MatAt(rot, thing);
        }

        public override void Init(AvaliGraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.drawSize = req.drawSize;
            List<StuffAppearanceDef> defsListForReading = DefDatabase<StuffAppearanceDef>.AllDefsListForReading;
            this.subGraphics = new AvaliGraphic[defsListForReading.Count];
            for (int index = 0; index < this.subGraphics.Length; ++index)
            {
                StuffAppearanceDef stuffAppearance = defsListForReading[index];
                string folderPath = req.path;
                if (!stuffAppearance.pathPrefix.NullOrEmpty())
                    folderPath = stuffAppearance.pathPrefix + "/" + ((IEnumerable<string>)folderPath.Split('/')).Last<string>();
                Texture2D texture2D = ContentFinder<Texture2D>.GetAllInFolder(folderPath).Where<Texture2D>((Func<Texture2D, bool>)(x => x.name.EndsWith(stuffAppearance.defName))).FirstOrDefault<Texture2D>();
                if ((UnityEngine.Object)texture2D != (UnityEngine.Object)null)
                    this.subGraphics[index] = AvaliGraphicDatabase.Get<AvaliGraphic_Single>(folderPath + "/" + texture2D.name, req.shader, this.drawSize, this.color);
            }
            for (int index = 0; index < this.subGraphics.Length; ++index)
            {
                if (this.subGraphics[index] == null)
                    this.subGraphics[index] = this.subGraphics[(int)StuffAppearanceDefOf.Smooth.index];
            }
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            Log.Message("treidmoasrasd");
            if (newColorTwo != Color.white)
                Log.ErrorOnce("Cannot use Graphic_Appearances.GetColoredVersion with a non-white colorTwo.", 9910251, false);
            return AvaliGraphicDatabase.Get<AvaliGraphic_Appearances>(this.path, newShader, this.drawSize, newColor, Color.white, Color.white,this.data);
        }

        public override Material MatSingleFor(Thing thing)
        {
            return this.SubGraphicFor(thing).MatSingleFor(thing);
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            this.SubGraphicFor(thing).DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public AvaliGraphic SubGraphicFor(Thing thing)
        {
            StuffAppearanceDef smooth = StuffAppearanceDefOf.Smooth;
            return thing != null ? this.SubGraphicFor(thing.Stuff) : this.subGraphics[(int)smooth.index];
        }

        public AvaliGraphic SubGraphicFor(ThingDef stuff)
        {
            StuffAppearanceDef stuffAppearanceDef = StuffAppearanceDefOf.Smooth;
            if (stuff != null && stuff.stuffProps.appearance != null)
                stuffAppearanceDef = stuff.stuffProps.appearance;
            return this.subGraphics[(int)stuffAppearanceDef.index];
        }

        public override string ToString()
        {
            return "Appearance(path=" + this.path + ", color=" + (object)this.color + ", colorTwo=unsupported)";
        }
    }

    public class AvaliGraphic_Cluster : AvaliGraphic_Collection
    {
        private const float PositionVariance = 0.45f;
        private const float SizeVariance = 0.2f;
        private const float SizeFactorMin = 0.8f;
        private const float SizeFactorMax = 1.2f;

        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[Rand.Range(0, this.subGraphics.Length)].MatSingle;
            }
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            Log.ErrorOnce("Graphic_Scatter cannot draw realtime.", 9432243, false);
        }

        public override void Print(SectionLayer layer, Thing thing)
        {
            Vector3 vector3 = thing.TrueCenter();
            Rand.PushState();
            Rand.Seed = thing.Position.GetHashCode();
            int num = thing is Filth filth ? filth.thickness : 3;
            for (int index = 0; index < num; ++index)
            {
                Material matSingle = this.MatSingle;
                Vector3 center = vector3 + new Vector3(Rand.Range(-0.45f, 0.45f), 0.0f, Rand.Range(-0.45f, 0.45f));
                Vector2 size = new Vector2(Rand.Range(this.data.drawSize.x * 0.8f, this.data.drawSize.x * 1.2f), Rand.Range(this.data.drawSize.y * 0.8f, this.data.drawSize.y * 1.2f));
                float rot = (float)Rand.RangeInclusive(0, 360);
                bool flipUv = (double)Rand.Value < 0.5;
                Printer_Plane.PrintPlane(layer, center, size, matSingle, rot, flipUv, (Vector2[])null, (Color32[])null, 0.01f, 0.0f);
            }
            Rand.PopState();
        }

        public override string ToString()
        {
            return "Scatter(subGraphic[0]=" + this.subGraphics[0].ToString() + ", count=" + (object)this.subGraphics.Length + ")";
        }
    }

    public abstract class AvaliGraphic_Collection : AvaliGraphic
    {
        protected AvaliGraphic[] subGraphics;

        public override void Init(AvaliGraphicRequest req)
        {
            this.data = req.graphicData;
            if (req.path.NullOrEmpty())
                throw new ArgumentNullException("folderPath");
            if ((UnityEngine.Object)req.shader == (UnityEngine.Object)null)
                throw new ArgumentNullException("shader");
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            List<Texture2D> list = ContentFinder<Texture2D>.GetAllInFolder(req.path).Where<Texture2D>((Func<Texture2D, bool>)(x => !x.name.EndsWith(Graphic_Single.MaskSuffix))).OrderBy<Texture2D, string>((Func<Texture2D, string>)(x => x.name)).ToList<Texture2D>();
            if (list.NullOrEmpty<Texture2D>())
            {
                Log.Error("Collection cannot init: No textures found at path " + req.path, false);
                this.subGraphics = new AvaliGraphic[1]
                {
          AvaliBaseContent.BadGraphic
                };
            }
            else
            {
                this.subGraphics = new AvaliGraphic[list.Count];
                for (int index = 0; index < list.Count; ++index)
                {
                    string path = req.path + "/" + list[index].name;
                    this.subGraphics[index] = AvaliGraphicDatabase.Get(typeof(Graphic_Single), path, req.shader, this.drawSize, this.color, this.colorTwo,this.colorThree ,(AvaliGraphicData)null, req.shaderParameters);
                }
            }
        }
    }

    public class AvaliGraphic_Single : AvaliGraphic
    {
        public static readonly string MaskSuffix = "_m";
        protected Material mat;

        public override Material MatSingle
        {
            get
            {
                return this.mat;
            }
        }

        public override Material MatWest
        {
            get
            {
                return this.mat;
            }
        }

        public override Material MatSouth
        {
            get
            {
                return this.mat;
            }
        }

        public override Material MatEast
        {
            get
            {
                return this.mat;
            }
        }

        public override Material MatNorth
        {
            get
            {
                return this.mat;
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                return this.data == null || this.data.drawRotated;
            }
        }

        public override void Init(AvaliGraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            MaterialRequest req1 = new MaterialRequest();
            req1.mainTex = ContentFinder<Texture2D>.Get(req.path, true);
            req1.shader = req.shader;
            req1.color = this.color;
            req1.colorTwo = this.colorTwo;
            req1.renderQueue = req.renderQueue;
            req1.shaderParameters = req.shaderParameters;
            if (req.shader.SupportsMaskTex())
                req1.maskTex = ContentFinder<Texture2D>.Get(req.path + Graphic_Single.MaskSuffix, false);
            this.mat = MaterialPool.MatFrom(req1);
        }

        public override AvaliGraphic GetColoredVersion(
        Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            Log.Message("actuallythisone");
            return AvaliGraphicDatabase.Get<AvaliGraphic_Single>(this.path, newShader, this.drawSize, newColor, newColorTwo, Color.white,this.data);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return this.mat;
        }

        public override string ToString()
        {
            return "Single(path=" + this.path + ", color=" + (object)this.color + ", colorTwo=" + (object)this.colorTwo + ")";
        }
    }

    public abstract class AvaliGraphic_WithPropertyBlock : AvaliGraphic_Single
    {
        protected MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        protected override void DrawMeshInt(Mesh mesh, Vector3 loc, Quaternion quat, Material mat)
        {
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(loc, quat, new Vector3(this.drawSize.x, 1f, this.drawSize.y)), mat, 0, (Camera)null, 0, this.propertyBlock);
        }
    }

    public class AvaliGraphic_FadesInOut : AvaliGraphic_WithPropertyBlock
    {
        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            CompFadesInOut comp = thing.TryGetComp<CompFadesInOut>();
            if (comp == null)
            {
                Log.ErrorOnce(thingDef.defName + ": Graphic_FadesInOut requires CompFadesInOut.", 5643893, false);
            }
            else
            {
                this.propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(this.color.r, this.color.g, this.color.b, this.color.a * comp.Opacity()));
                base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            }
        }
    }
    public class AvaliGraphic_Terrain : AvaliGraphic_Single
    {
        public override void Init(AvaliGraphicRequest req)
        {
            base.Init(req);
        }

        public override string ToString()
        {
            return "Terrain(path=" + this.path + ", shader=" + (object)this.Shader + ", color=" + (object)this.color + ")";
        }
    }
    [StaticConstructorOnStartup]
    public class AvaliGraphic_Mote : AvaliGraphic_Single
    {
        protected static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        protected virtual bool ForcePropertyBlock
        {
            get
            {
                return false;
            }
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            this.DrawMoteInternal(loc, rot, thingDef, thing, 0);
        }

        public void DrawMoteInternal(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          int layer)
        {
            Mote mote = (Mote)thing;
            float alpha = mote.Alpha;
            if ((double)alpha <= 0.0)
                return;
            Color colA = this.Color * mote.instanceColor;
            colA.a *= alpha;
            Vector3 exactScale = mote.exactScale;
            exactScale.x *= this.data.drawSize.x;
            exactScale.z *= this.data.drawSize.y;
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(mote.DrawPos, Quaternion.AngleAxis(mote.exactRotation, Vector3.up), exactScale);
            Material matSingle = this.MatSingle;
            if (!this.ForcePropertyBlock && colA.IndistinguishableFrom(matSingle.color))
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, layer, (Camera)null, 0);
            }
            else
            {
                AvaliGraphic_Mote.propertyBlock.SetColor(ShaderPropertyIDs.Color, colA);
                Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, layer, (Camera)null, 0, AvaliGraphic_Mote.propertyBlock);
            }
        }

        public override string ToString()
        {
            return "Mote(path=" + this.path + ", shader=" + (object)this.Shader + ", color=" + (object)this.color + ", colorTwo=unsupported)";
        }
    }


    public class AvaliGraphic_Multi : AvaliGraphic
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
                //this.mats[2].shader = ShaderDatabase.CutoutSkin; //No mask
                //this.mats[2].shader = ShaderDatabase.CutoutComplex; //No change
                //this.mats[2].shader = ShaderDatabase.Cutout;
                //this.mats[2].SetColor("_Color", Color.red);
                //this.mats[2].SetColor("_ColorTwo", Color.blue);
                //this.mats[2].SetColor("_ColorThree", Color.green);
                //Log.Message("Mat with shader: " + this.mats[2].shader.name + "C: " + this.mats[2].GetColor("_Color"));
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

        public override void Init(AvaliGraphicRequest req)
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
                Log.Error("Failed to find any textures at " + req.path + " while constructing " + this.ToStringSafe<AvaliGraphic_Multi>(), false);
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
                //if (req.shader.SupportsMaskTex())
                if (req.shader == AvaliShaderDatabase.Tricolor)
                {
                    //Log.Message("Generating MaskTex");
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
                    //this.mats[index] = MaterialPool.MatFrom(new MaterialRequest()
                    this.mats[index] = AvaliMaterialPool.MatFrom(new AvaliMaterialRequest()
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

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            //Log.Message("Imtryingtogetthis");
            return AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(this.path, newShader, this.drawSize, newColor, newColorTwo, newColorThree,this.data);
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

    public class AvaliGraphic_Random : AvaliGraphic_Collection
    {
        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[Rand.Range(0, this.subGraphics.Length)].MatSingle;
            }
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            //Log.Message("butalsothert");
            if (newColorTwo != Color.white)
                Log.ErrorOnce("Cannot use Graphic_Random.GetColoredVersion with a non-white colorTwo.", 9910251, false);
            return AvaliGraphicDatabase.Get < AvaliGraphic_Random>(this.path, newShader, this.drawSize, newColor, Color.white, Color.white,this.data);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return thing == null ? this.MatSingle : this.MatSingleFor(thing);
        }

        public override Material MatSingleFor(Thing thing)
        {
            return thing == null ? this.MatSingle : this.SubGraphicFor(thing).MatSingle;
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            (thing == null ? this.subGraphics[0] : this.SubGraphicFor(thing)).DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public AvaliGraphic SubGraphicFor(Thing thing)
        {
            return thing == null ? this.subGraphics[0] : this.subGraphics[thing.thingIDNumber % this.subGraphics.Length];
        }

        public AvaliGraphic FirstSubgraphic()
        {
            return this.subGraphics[0];
        }

        public override string ToString()
        {
            return "Random(path=" + this.path + ", count=" + (object)this.subGraphics.Length + ")";
        }
    }

    public class AvaliGraphic_Flicker : AvaliGraphic_Collection
    {
        private const int BaseTicksPerFrameChange = 15;
        private const int ExtraTicksPerFrameChange = 10;
        private const float MaxOffset = 0.05f;

        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[Rand.Range(0, this.subGraphics.Length)].MatSingle;
            }
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            if (thingDef == null)
                Log.ErrorOnce("Fire DrawWorker with null thingDef: " + (object)loc, 3427324, false);
            else if (this.subGraphics == null)
            {
                Log.ErrorOnce("Graphic_Flicker has no subgraphics " + (object)thingDef, 358773632, false);
            }
            else
            {
                int ticksGame = Find.TickManager.TicksGame;
                if (thing != null)
                    ticksGame += Mathf.Abs(thing.thingIDNumber ^ 8453458);
                int num1 = ticksGame / 15;
                int index = Mathf.Abs(num1 ^ (thing != null ? thing.thingIDNumber : 0) * 391) % this.subGraphics.Length;
                float num2 = 1f;
                CompProperties_FireOverlay propertiesFireOverlay = (CompProperties_FireOverlay)null;
                if (thing is Fire fire)
                    num2 = fire.fireSize;
                else if (thingDef != null)
                {
                    propertiesFireOverlay = thingDef.GetCompProperties<CompProperties_FireOverlay>();
                    if (propertiesFireOverlay != null)
                        num2 = propertiesFireOverlay.fireSize;
                }
                if (index < 0 || index >= this.subGraphics.Length)
                {
                    Log.ErrorOnce("Fire drawing out of range: " + (object)index, 7453435, false);
                    index = 0;
                }
                AvaliGraphic subGraphic = this.subGraphics[index];
                float num3 = Mathf.Min(num2 / 1.2f, 1.2f);
                Vector3 vector3 = GenRadial.RadialPattern[num1 % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius * 0.05f;
                Vector3 pos = loc + vector3 * num2;
                if (propertiesFireOverlay != null)
                    pos += propertiesFireOverlay.offset;
                Vector3 s = new Vector3(num3, 1f, num3);
                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetTRS(pos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, subGraphic.MatSingle, 0);
            }
        }

        public override string ToString()
        {
            return "Flicker(subGraphic[0]=" + this.subGraphics[0].ToString() + ", count=" + (object)this.subGraphics.Length + ")";
        }
    }

    public class AvaliGraphic_StackCount : AvaliGraphic_Collection
    {
        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[this.subGraphics.Length - 1].MatSingle;
            }
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            //Log.Message("ormaybhetisone");
            return AvaliGraphicDatabase.Get<AvaliGraphic_StackCount>(this.path, newShader, this.drawSize, newColor, newColorTwo, newColorThree
                ,this.data);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return thing == null ? this.MatSingle : this.MatSingleFor(thing);
        }

        public override Material MatSingleFor(Thing thing)
        {
            return thing == null ? this.MatSingle : this.SubGraphicFor(thing).MatSingle;
        }

        public AvaliGraphic SubGraphicFor(Thing thing)
        {
            return this.SubGraphicForStackCount(thing.stackCount, thing.def);
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            (thing == null ? this.subGraphics[0] : this.SubGraphicFor(thing)).DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public AvaliGraphic SubGraphicForStackCount(int stackCount, ThingDef def)
        {
            switch (this.subGraphics.Length)
            {
                case 1:
                    return this.subGraphics[0];
                case 2:
                    return stackCount == 1 ? this.subGraphics[0] : this.subGraphics[1];
                case 3:
                    if (stackCount == 1)
                        return this.subGraphics[0];
                    return stackCount == def.stackLimit ? this.subGraphics[2] : this.subGraphics[1];
                default:
                    if (stackCount == 1)
                        return this.subGraphics[0];
                    return stackCount == def.stackLimit ? this.subGraphics[this.subGraphics.Length - 1] : this.subGraphics[Mathf.Min(1 + Mathf.RoundToInt((float)((double)stackCount / (double)def.stackLimit * ((double)this.subGraphics.Length - 3.0) + 9.99999974737875E-06)), this.subGraphics.Length - 2)];
            }
        }

        public override string ToString()
        {
            return "StackCount(path=" + this.path + ", count=" + (object)this.subGraphics.Length + ")";
        }
    }
}