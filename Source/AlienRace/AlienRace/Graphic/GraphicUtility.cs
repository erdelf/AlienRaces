using RimWorld;
using System;
using Verse;
using UnityEngine;
namespace AvaliMod
{
    public static class AvaliGraphicUtility
    {
        public static AvaliGraphic ExtractInnerGraphicFor(this AvaliGraphic outerGraphic, Thing thing)
        {
            switch (outerGraphic)
            {
                case AvaliGraphic_Random graphicRandom:
                    return graphicRandom.SubGraphicFor(thing);
                case AvaliGraphic_Appearances graphicAppearances:
                    return graphicAppearances.SubGraphicFor(thing);
                default:
                    return outerGraphic;
            }
        }

        public static AvaliGraphic_Linked WrapLinked(
          AvaliGraphic subGraphic,
          LinkDrawerType linkDrawerType)
        {
            switch (linkDrawerType)
            {
                case LinkDrawerType.None:
                    return (AvaliGraphic_Linked)null;
                case LinkDrawerType.Basic:
                    return new AvaliGraphic_Linked(subGraphic);
                case LinkDrawerType.CornerFiller:
                    return (AvaliGraphic_Linked)new AvaliGraphic_LinkedCornerFiller(subGraphic);
                case LinkDrawerType.Transmitter:
                    return (AvaliGraphic_Linked)new AvaliGraphic_LinkedTransmitter(subGraphic);
                case LinkDrawerType.TransmitterOverlay:
                    return (AvaliGraphic_Linked)new AvaliGraphic_LinkedTransmitterOverlay(subGraphic);
                default:
                    throw new ArgumentException();
            }
        }
    }
    public class AvaliGraphic_Linked : AvaliGraphic
    {
        protected AvaliGraphic subGraphic;

        public virtual LinkDrawerType LinkerType
        {
            get
            {
                return LinkDrawerType.Basic;
            }
        }

        public override Material MatSingle
        {
            get
            {
                return MaterialAtlasPool.SubMaterialFromAtlas(this.subGraphic.MatSingle, LinkDirections.None);
            }
        }

        public AvaliGraphic_Linked()
        {
        }

        public AvaliGraphic_Linked(AvaliGraphic subGraphic)
        {
            this.subGraphic = subGraphic;
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            Log.Message("couldbethisone");
            AvaliGraphic_Linked graphicLinked = new AvaliGraphic_Linked(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo, newColorThree));
            graphicLinked.data = this.data;
            return (AvaliGraphic)graphicLinked;
        }

        public override void Print(SectionLayer layer, Thing thing)
        {
            Material mat = this.LinkedDrawMatFrom(thing, thing.Position);
            Printer_Plane.PrintPlane(layer, thing.TrueCenter(), new Vector2(1f, 1f), mat, 0.0f, false, (Vector2[])null, (Color32[])null, 0.01f, 0.0f);
        }

        public override Material MatSingleFor(Thing thing)
        {
            return this.LinkedDrawMatFrom(thing, thing.Position);
        }

        protected Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
        {
            int num1 = 0;
            int num2 = 1;
            for (int index = 0; index < 4; ++index)
            {
                if (this.ShouldLinkWith(cell + GenAdj.CardinalDirections[index], parent))
                    num1 += num2;
                num2 *= 2;
            }
            LinkDirections LinkSet = (LinkDirections)num1;
            return MaterialAtlasPool.SubMaterialFromAtlas(this.subGraphic.MatSingleFor(parent), LinkSet);
        }

        public virtual bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            if (!parent.Spawned)
                return false;
            return !c.InBounds(parent.Map) ? (uint)(parent.def.graphicData.linkFlags & LinkFlags.MapEdge) > 0U : (uint)(parent.Map.linkGrid.LinkFlagsAt(c) & parent.def.graphicData.linkFlags) > 0U;
        }
    }

    public class AvaliGraphic_RandomRotated : AvaliGraphic
    {
        private AvaliGraphic subGraphic;
        private float maxAngle;

        public override Material MatSingle
        {
            get
            {
                return this.subGraphic.MatSingle;
            }
        }

        public AvaliGraphic_RandomRotated(AvaliGraphic subGraphic, float maxAngle)
        {
            this.subGraphic = subGraphic;
            this.maxAngle = maxAngle;
            this.drawSize = subGraphic.drawSize;
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            Mesh mesh = this.MeshAt(rot);
            float num = 0.0f;
            if (thing != null)
                num = (float)(-(double)this.maxAngle + (double)(thing.thingIDNumber * 542) % ((double)this.maxAngle * 2.0));
            float angle = num + extraRotation;
            Material matSingle = this.subGraphic.MatSingle;
            Vector3 position = loc;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Material material = matSingle;
            Graphics.DrawMesh(mesh, position, rotation, material, 0, (Camera)null, 0);
        }

        public override string ToString()
        {
            return "RandomRotated(subGraphic=" + this.subGraphic.ToString() + ")";
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            AvaliGraphic_RandomRotated graphicRandomRotated = new AvaliGraphic_RandomRotated(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo, newColorThree), this.maxAngle);
            graphicRandomRotated.data = this.data;
            graphicRandomRotated.drawSize = this.drawSize;
            return (AvaliGraphic)graphicRandomRotated;
        }
    }
}