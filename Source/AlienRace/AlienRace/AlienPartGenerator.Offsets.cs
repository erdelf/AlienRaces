namespace AlienRace;

using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

public partial class AlienPartGenerator
{
    public class BodyAddonOffsets
    {
        public RotationOffset GetOffset(Rot4 rotation) =>
            rotation == Rot4.South ? this.south :
            rotation == Rot4.North ? this.north :
            rotation == Rot4.East  ? this.east : this.west;

        public RotationOffset south = new RotationOffset();
        public RotationOffset north = new RotationOffset();
        public RotationOffset east  = new RotationOffset();
        public RotationOffset west;
    }

    public class RotationOffset
    {
        public Vector3 GetOffset(bool portrait, BodyTypeDef bodyType, HeadTypeDef headType)
        {
            Vector2 bodyOffset =
                (portrait ? this.portraitBodyTypes ?? this.bodyTypes : this.bodyTypes)
            ?.FirstOrDefault(predicate: to => to.bodyType == bodyType)?.offset ?? Vector2.zero;
            Vector2 headOffset =
                (portrait ? this.portraitHeadTypes ?? this.headTypes : this.headTypes)
            ?.FirstOrDefault(predicate: to => to.headType == headType)?.offset ?? Vector2.zero;

            return new Vector3(this.offset.x + bodyOffset.x + headOffset.x, this.layerOffset,
                               this.offset.y + bodyOffset.y + headOffset.y);
        }

        public float                 layerOffset;
        public Vector2               offset;
        public List<BodyTypeOffset>  portraitBodyTypes;
        public List<BodyTypeOffset>  bodyTypes;
        public List<HeadTypeOffsets> portraitHeadTypes;
        public List<HeadTypeOffsets> headTypes;
    }

    public class BodyTypeOffset
    {
        public BodyTypeDef bodyType;
        public Vector2     offset = Vector2.zero;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.bodyType), xmlRoot.Name);
            this.offset = (Vector2)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
        }
    }

    public class HeadTypeOffsets
    {
        public HeadTypeDef headType;
        public Vector2 offset = Vector2.zero;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.headType), xmlRoot.Name);
            this.offset   = (Vector2)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
        }
    }
}