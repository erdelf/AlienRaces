namespace AlienRace;

using System.Collections.Generic;
using System.Linq;
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
        public Vector3 GetOffset(bool portrait, BodyTypeDef bodyType, string crownType)
        {
            Vector2 bodyOffset =
                (portrait ? this.portraitBodyTypes ?? this.bodyTypes : this.bodyTypes)
            ?.FirstOrDefault(predicate: to => to.bodyType == bodyType)?.offset ?? Vector2.zero;
            Vector2 crownOffset =
                (portrait ? this.portraitCrownTypes ?? this.crownTypes : this.crownTypes)
            ?.FirstOrDefault(predicate: to => to.crownType == crownType)?.offset ?? Vector2.zero;

            return new Vector3(this.offset.x + bodyOffset.x + crownOffset.x, this.layerOffset,
                               this.offset.y + bodyOffset.y + crownOffset.y);
        }

        public float                 layerOffset;
        public Vector2               offset;
        public List<BodyTypeOffset>  portraitBodyTypes;
        public List<BodyTypeOffset>  bodyTypes;
        public List<CrownTypeOffset> portraitCrownTypes;
        public List<CrownTypeOffset> crownTypes;
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

    public class CrownTypeOffset
    {
        public string  crownType;
        public Vector2 offset = Vector2.zero;

        [UsedImplicitly]
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.crownType = xmlRoot.Name;
            this.offset    = (Vector2)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
        }
    }
}