using RimWorld;
using UnityEngine;
using Verse;
namespace AvaliMod
{
    public class AvaliGraphic : Graphic
    {
        public Color colorThree = Color.white;
        public AvaliGraphicData data;
        public string path;
        private Graphic_Shadow cachedShadowGraphicInt;
        private AvaliGraphic cachedShadowlessGraphicInt;

        public Shader Shader
        {
            get
            {
                Material matSingle = this.MatSingle;
                return (Object)matSingle != (Object)null ? matSingle.shader : ShaderDatabase.Cutout;
            }
        }


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


        public virtual void Init(AvaliGraphicRequest req)
        {
            Log.ErrorOnce("Cannot init Graphic of class " + this.GetType().ToString(), 658928, false);
        }

        public virtual Material MatAt(Rot4 rot, Thing thing = null)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return this.MatNorth;
                case 1:
                    return this.MatEast;
                case 2:
                    return this.MatSouth;
                case 3:
                    return this.MatWest;
                default:
                    return BaseContent.BadMat;
            }
        }

        public virtual Mesh MeshAt(Rot4 rot)
        {
            Vector2 vector2 = this.drawSize;
            if (rot.IsHorizontal && !this.ShouldDrawRotated)
                vector2 = vector2.Rotated();
            return rot == Rot4.West && this.WestFlipped || rot == Rot4.East && this.EastFlipped ? MeshPool.GridPlaneFlip(vector2) : MeshPool.GridPlane(vector2);
        }

        public virtual Material MatSingleFor(Thing thing)
        {
            return this.MatSingle;
        }

        public Vector3 DrawOffset(Rot4 rot)
        {
            return this.data == null ? Vector3.zero : this.data.DrawOffsetForRot(rot);
        }

        public void Draw(Vector3 loc, Rot4 rot, Thing thing, float extraRotation = 0.0f)
        {
            this.DrawWorker(loc, rot, thing.def, thing, extraRotation);
        }

        public void DrawFromDef(Vector3 loc, Rot4 rot, ThingDef thingDef, float extraRotation = 0.0f)
        {
            this.DrawWorker(loc, rot, thingDef, (Thing)null, extraRotation);
        }

        public virtual void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            Mesh mesh = this.MeshAt(rot);
            Quaternion quat = this.QuatFromRot(rot);
            if ((double)extraRotation != 0.0)
                quat *= Quaternion.Euler(Vector3.up * extraRotation);
            loc += this.DrawOffset(rot);
            Material mat = this.MatAt(rot, thing);
            this.DrawMeshInt(mesh, loc, quat, mat);
            if (this.ShadowGraphic == null)
                return;
            this.ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
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

        public virtual AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + this.GetType().ToString(), 66300, false);
            return AvaliBaseContent.BadGraphic;
        }

        public virtual AvaliGraphic GetCopy(Vector2 newDrawSize)
        {
            return AvaliGraphicDatabase.Get(this.GetType(),
                                            this.path,
                                            this.Shader,
                                            newDrawSize,
                                            this.color,
                                            this.colorTwo,
                                            this.colorThree);
        }

        public virtual AvaliGraphic GetShadowlessGraphic()
        {
            if (this.data == null || this.data.shadowData == null)
                return this;
            if (this.cachedShadowlessGraphicInt == null)
            {
                AvaliGraphicData graphicData = new AvaliGraphicData();
                graphicData.CopyFrom(this.data);
                graphicData.shadowData = (ShadowData)null;
                this.cachedShadowlessGraphicInt = graphicData.Graphic;
            }
            return this.cachedShadowlessGraphicInt;
        }

        protected float AngleFromRot(Rot4 rot)
        {
            if (!this.ShouldDrawRotated)
                return 0.0f;
            float num = rot.AsAngle + this.DrawRotatedExtraAngleOffset;
            if (rot == Rot4.West && this.WestFlipped || rot == Rot4.East && this.EastFlipped)
                num += 180f;
            return num;
        }

        protected Quaternion QuatFromRot(Rot4 rot)
        {
            float angle = this.AngleFromRot(rot);
            return (double)angle == 0.0 ? Quaternion.identity : Quaternion.AngleAxis(angle, Vector3.up);
        }
    }
}
