using Verse;
using UnityEngine;
namespace AvaliMod
{
    
    public class AvaliGraphic_LinkedCornerFiller : AvaliGraphic_Linked
    {
        private static readonly float CoverSizeCornerCorner = new Vector2(0.5f, 0.5f).magnitude;
        private static readonly float DistCenterCorner = new Vector2(0.5f, 0.5f).magnitude;
        private static readonly float CoverOffsetDist = AvaliGraphic_LinkedCornerFiller.DistCenterCorner - AvaliGraphic_LinkedCornerFiller.CoverSizeCornerCorner * 0.5f;
        private static readonly Vector2[] CornerFillUVs = new Vector2[4]
        {
      new Vector2(0.5f, 0.6f),
      new Vector2(0.5f, 0.6f),
      new Vector2(0.5f, 0.6f),
      new Vector2(0.5f, 0.6f)
        };
        private const float ShiftUp = 0.09f;
        private const float CoverSize = 0.5f;

        public override LinkDrawerType LinkerType
        {
            get
            {
                return LinkDrawerType.CornerFiller;
            }
        }

        public AvaliGraphic_LinkedCornerFiller(AvaliGraphic subGraphic)
          : base(subGraphic)
        {
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            AvaliGraphic_LinkedCornerFiller linkedCornerFiller = new AvaliGraphic_LinkedCornerFiller(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo, newColorThree));
            linkedCornerFiller.data = this.data;
            return (AvaliGraphic)linkedCornerFiller;
        }

        public override void Print(SectionLayer layer, Thing thing)
        {
            base.Print(layer, thing);
            IntVec3 position = thing.Position;
            for (int index = 0; index < 4; ++index)
            {
                IntVec3 c = thing.Position + GenAdj.DiagonalDirectionsAround[index];
                if (this.ShouldLinkWith(c, thing) && (index != 0 || this.ShouldLinkWith(position + IntVec3.West, thing) && this.ShouldLinkWith(position + IntVec3.South, thing)) && ((index != 1 || this.ShouldLinkWith(position + IntVec3.West, thing) && this.ShouldLinkWith(position + IntVec3.North, thing)) && (index != 2 || this.ShouldLinkWith(position + IntVec3.East, thing) && this.ShouldLinkWith(position + IntVec3.North, thing))) && (index != 3 || this.ShouldLinkWith(position + IntVec3.East, thing) && this.ShouldLinkWith(position + IntVec3.South, thing)))
                {
                    Vector3 center = thing.DrawPos + GenAdj.DiagonalDirectionsAround[index].ToVector3().normalized * AvaliGraphic_LinkedCornerFiller.CoverOffsetDist + Altitudes.AltIncVect + new Vector3(0.0f, 0.0f, 0.09f);
                    Vector2 size = new Vector2(0.5f, 0.5f);
                    if (!c.InBounds(thing.Map))
                    {
                        if (c.x == -1)
                        {
                            --center.x;
                            size.x *= 5f;
                        }
                        if (c.z == -1)
                        {
                            --center.z;
                            size.y *= 5f;
                        }
                        if (c.x == thing.Map.Size.x)
                        {
                            ++center.x;
                            size.x *= 5f;
                        }
                        if (c.z == thing.Map.Size.z)
                        {
                            ++center.z;
                            size.y *= 5f;
                        }
                    }
                    Printer_Plane.PrintPlane(layer, center, size, this.LinkedDrawMatFrom(thing, thing.Position), 0.0f, false, AvaliGraphic_LinkedCornerFiller.CornerFillUVs, (Color32[])null, 0.01f, 0.0f);
                }
            }
        }
    }
    public class AvaliGraphic_LinkedTransmitterOverlay : AvaliGraphic_Linked
    {
        public AvaliGraphic_LinkedTransmitterOverlay()
        {
        }

        public AvaliGraphic_LinkedTransmitterOverlay(AvaliGraphic subGraphic)
          : base(subGraphic)
        {
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && parent.Map.powerNetGrid.TransmittedPowerNetAt(c) != null;
        }

        public override void Print(SectionLayer layer, Thing parent)
        {
            foreach (IntVec3 cell in parent.OccupiedRect())
            {
                Vector3 shiftedWithAltitude = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);
                Printer_Plane.PrintPlane(layer, shiftedWithAltitude, new Vector2(1f, 1f), this.LinkedDrawMatFrom(parent, cell), 0.0f, false, (Vector2[])null, (Color32[])null, 0.01f, 0.0f);
            }
        }
    }


    public class AvaliGraphic_LinkedTransmitter : AvaliGraphic_Linked
    {
        public AvaliGraphic_LinkedTransmitter(AvaliGraphic subGraphic)
          : base(subGraphic)
        {
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && (base.ShouldLinkWith(c, parent) || parent.Map.powerNetGrid.TransmittedPowerNetAt(c) != null);
        }

        public override void Print(SectionLayer layer, Thing thing)
        {
            base.Print(layer, thing);
            for (int index = 0; index < 4; ++index)
            {
                IntVec3 intVec3 = thing.Position + GenAdj.CardinalDirections[index];
                if (intVec3.InBounds(thing.Map))
                {
                    Building transmitter = intVec3.GetTransmitter(thing.Map);
                    if (transmitter != null && !transmitter.def.graphicData.Linked)
                    {
                        Material mat = this.LinkedDrawMatFrom(thing, intVec3);
                        Printer_Plane.PrintPlane(layer, intVec3.ToVector3ShiftedWithAltitude(thing.def.Altitude), Vector2.one, mat, 0.0f, false, (Vector2[])null, (Color32[])null, 0.01f, 0.0f);
                    }
                }
            }
        }
    }
}