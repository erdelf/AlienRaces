using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace AvaliMod
{
    public class AvaliGraphicData
    {
        public Color color = Color.white;
        public Color colorTwo = Color.white;
        public Color colorThree = Color.white;
        public Vector2 drawSize = Vector2.one;
        public Vector3 drawOffset = Vector3.zero;
        public bool drawRotated = true;
        public bool allowFlip = true;
        [NoTranslate]
        public string texPath;
        public System.Type graphicClass;
        public ShaderTypeDef shaderType;
        public List<ShaderParameter> shaderParameters;
        public Vector3? drawOffsetNorth;
        public Vector3? drawOffsetEast;
        public Vector3? drawOffsetSouth;
        public Vector3? drawOffsetWest;
        public float onGroundRandomRotateAngle;
        public float flipExtraRotation;
        public ShadowData shadowData;
        public DamageGraphicData damageData;
        public LinkDrawerType linkType;
        public LinkFlags linkFlags;
        [Unsaved(false)]
        private AvaliGraphic cachedGraphic;

        public bool Linked
        {
            get
            {
                return (uint)this.linkType > 0U;
            }
        }

        public AvaliGraphic Graphic
        {
            get
            {
                if (this.cachedGraphic == null)
                    this.Init();
                return this.cachedGraphic;
            }
        }

        public void CopyFrom(AvaliGraphicData other)
        {
            this.texPath = other.texPath;
            this.graphicClass = other.graphicClass;
            this.shaderType = other.shaderType;
            this.color = other.color;
            this.colorTwo = other.colorTwo;
            this.colorThree = other.colorThree;
            this.drawSize = other.drawSize;
            this.drawOffset = other.drawOffset;
            this.drawOffsetNorth = other.drawOffsetNorth;
            this.drawOffsetEast = other.drawOffsetEast;
            this.drawOffsetSouth = other.drawOffsetSouth;
            this.drawOffsetWest = other.drawOffsetSouth;
            this.onGroundRandomRotateAngle = other.onGroundRandomRotateAngle;
            this.drawRotated = other.drawRotated;
            this.allowFlip = other.allowFlip;
            this.flipExtraRotation = other.flipExtraRotation;
            this.shadowData = other.shadowData;
            this.damageData = other.damageData;
            this.linkType = other.linkType;
            this.linkFlags = other.linkFlags;
            this.cachedGraphic = (AvaliGraphic)null;
        }

        private void Init()
        {
            if (this.graphicClass == (System.Type)null)
            {
                this.cachedGraphic = (AvaliGraphic)null;
            }
            else
            {
                this.cachedGraphic = AvaliGraphicDatabase.Get(this.graphicClass, this.texPath, (this.shaderType ?? ShaderTypeDefOf.Cutout).Shader, this.drawSize, this.color, this.colorTwo, this.colorThree,this, this.shaderParameters);
                if ((double)this.onGroundRandomRotateAngle > 0.00999999977648258)
                    this.cachedGraphic = (AvaliGraphic)new AvaliGraphic_RandomRotated(this.cachedGraphic, this.onGroundRandomRotateAngle);
                if (!this.Linked)
                    return;
                this.cachedGraphic = (AvaliGraphic)AvaliGraphicUtility.WrapLinked(this.cachedGraphic, this.linkType);
            }
        }

        public void ResolveReferencesSpecial()
        {
            if (this.damageData == null)
                return;
            this.damageData.ResolveReferencesSpecial();
        }

        public Vector3 DrawOffsetForRot(Rot4 rot)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return this.drawOffsetNorth ?? this.drawOffset;
                case 1:
                    return this.drawOffsetEast ?? this.drawOffset;
                case 2:
                    return this.drawOffsetSouth ?? this.drawOffset;
                case 3:
                    return this.drawOffsetWest ?? this.drawOffset;
                default:
                    return this.drawOffset;
            }
        }

        public AvaliGraphic GraphicColoredFor(Thing t)
        {
            return t.DrawColor.IndistinguishableFrom(this.Graphic.Color) && t.DrawColorTwo.IndistinguishableFrom(this.Graphic.ColorTwo) ? this.Graphic : this.Graphic.GetColoredVersion(this.Graphic.Shader, t.DrawColor, t.DrawColorTwo, t.DrawColorTwo);
        }

        internal IEnumerable<string> ConfigErrors(ThingDef thingDef)
        {
            if (this.graphicClass == (System.Type)null)
                yield return "graphicClass is null";
            if (this.texPath.NullOrEmpty())
                yield return "texPath is null or empty";
            if (thingDef != null)
            {
                if (thingDef.drawerType == DrawerType.RealtimeOnly && this.Linked)
                    yield return "does not add to map mesh but has a link drawer. Link drawers can only work on the map mesh.";
                if (!thingDef.rotatable && (this.drawOffsetNorth.HasValue || this.drawOffsetEast.HasValue || (this.drawOffsetSouth.HasValue || this.drawOffsetWest.HasValue)))
                    yield return "not rotatable but has rotational draw offset(s).";
            }
            if ((this.shaderType == ShaderTypeDefOf.Cutout || this.shaderType == ShaderTypeDefOf.CutoutComplex) && thingDef.mote != null && ((double)thingDef.mote.fadeInTime > 0.0 || (double)thingDef.mote.fadeOutTime > 0.0))
                yield return "mote fades but uses cutout shader type. It will abruptly disappear when opacity falls under the cutout threshold.";
        }
    }
}