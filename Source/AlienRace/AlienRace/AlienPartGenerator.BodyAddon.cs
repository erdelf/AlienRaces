namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using HarmonyLib;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public partial class AlienPartGenerator
    {
        public class BodyAddon
        {
            public string           path;
            public string           bodyPart;
            
            public string           defaultOffset = "Center";
            [Unsaved]
            public BodyAddonOffsets defaultOffsets;

            public BodyAddonOffsets offsets = new BodyAddonOffsets();
            public bool             linkVariantIndexWithPrevious = false;
            public float            angle                        = 0f;
            public bool             inFrontOfBody                = false;
            public bool             layerInvert                  = true;


            public bool drawnOnGround = true;
            public bool drawnInBed = true;
            public bool drawnDesiccated = true;
            public bool drawForMale = true;
            public bool drawForFemale = true;

            public bool alignWithHead = false;

            public Vector2 drawSize = Vector2.one;
            public Vector2 drawSizePortrait = Vector2.zero;
            public bool drawRotated = true;
            public bool scaleWithPawnDrawsize = false;

            private string colorChannel;

            public string ColorChannel => 
                this.colorChannel = this.colorChannel ?? "skin";

            public int variantCount = 0;
            public bool debug = true;

            public List<BodyAddonHediffGraphic> hediffGraphics;
            public List<BodyAddonBackstoryGraphic> backstoryGraphics;

            public List<BodyPartGroupDef> hiddenUnderApparelFor = new List<BodyPartGroupDef>();
            public List<string> hiddenUnderApparelTag = new List<string>();

            public string backstoryRequirement;
            public string bodyTypeRequirement;

            private ShaderTypeDef shaderType;

            public ShaderTypeDef ShaderType => this.shaderType = this.shaderType ?? ShaderTypeDefOf.Cutout;

            private List<BodyAddonPrioritization> prioritization;
            public List<BodyAddonPrioritization> Prioritization => this.prioritization ?? 
                                                                   (this.prioritization = new List<BodyAddonPrioritization> { BodyAddonPrioritization.Hediff, BodyAddonPrioritization.Backstory });



            public virtual bool CanDrawAddon(Pawn pawn) => 
                (pawn.Drawer.renderer.graphics.apparelGraphics.NullOrEmpty() || ((this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) || 
                !pawn.apparel.WornApparel.Any(predicate: ap => ap.def.apparel.bodyPartGroups.Any(predicate: bpgd => this.hiddenUnderApparelFor.Contains(bpgd)) || 
                ap.def.apparel.tags.Any(predicate: s => this.hiddenUnderApparelTag.Contains(s))))) && (pawn.GetPosture() == PawnPosture.Standing || this.drawnOnGround) && ((pawn.CurrentBed()?.def.building.bed_showSleeperBody ?? true) || this.drawnInBed) &&
                    (this.backstoryRequirement.NullOrEmpty() || pawn.story.AllBackstories.Any(predicate: b=> b.identifier == this.backstoryRequirement)) &&   
                    (this.drawnDesiccated || pawn.Corpse?.GetRotStage() != RotStage.Dessicated) &&
                    (this.bodyPart.NullOrEmpty() || 
                     (pawn.health.hediffSet.GetNotMissingParts().Any(predicate: bpr => bpr.untranslatedCustomLabel == this.bodyPart || bpr.def.defName == this.bodyPart)) || 
                     (this.hediffGraphics?.Any(predicate: bahg => bahg.hediff == HediffDefOf.MissingBodyPart) ?? false)) &&
               (pawn.gender == Gender.Female ? this.drawForFemale : this.drawForMale) && (this.bodyTypeRequirement.NullOrEmpty() || pawn.story.bodyType.ToString() == this.bodyTypeRequirement);

            public virtual Graphic GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                string returnPath = string.Empty;
                int variantCounting = 0;

                foreach (BodyAddonPrioritization prio in this.Prioritization)
                {
                    switch(prio)
                    {
                        case BodyAddonPrioritization.Backstory:
                            if (this.backstoryGraphics?.FirstOrDefault(predicate: babgs => pawn.story.AllBackstories.Any(predicate: bs => bs.identifier == babgs.backstory)) is BodyAddonBackstoryGraphic babg)
                            {
                                returnPath      = babg.path;
                                variantCounting = babg.variantCount;
                            }
                            break;
                        case BodyAddonPrioritization.Hediff:
                            if(!this.hediffGraphics.NullOrEmpty())
                                foreach (BodyAddonHediffGraphic bahg in this.hediffGraphics)
                                {

                                    foreach (Hediff h in pawn.health.hediffSet.hediffs.Where(predicate: h => h.def == bahg.hediff &&
                                                                                                             (h.Part == null                                  ||
                                                                                                              this.bodyPart.NullOrEmpty()                     ||
                                                                                                              h.Part.untranslatedCustomLabel == this.bodyPart ||
                                                                                                              h.Part.def.defName             == this.bodyPart)))
                                    {
                                        returnPath      = bahg.path;
                                        variantCounting = bahg.variantCount;

                                        if (!bahg.severity.NullOrEmpty())
                                            foreach (BodyAddonHediffSeverityGraphic bahsg in bahg.severity)
                                            {
                                                if (h.Severity >= bahsg.severity)
                                                {
                                                    returnPath      = bahsg.path;
                                                    variantCounting = bahsg.variantCount;
                                                    break;
                                                }
                                            }
                                        break;
                                    }
                                }

                            break;
                        default: 
                            throw new ArrayTypeMismatchException();
                    }
                    if (!returnPath.NullOrEmpty())
                        break;
                }

                if (returnPath.NullOrEmpty())
                {
                    returnPath = this.path;
                    variantCounting = this.variantCount;
                }

                if (variantCounting <= 0)
                    variantCounting = 1;

                ExposableValueTuple<Color, Color> channel = pawn.GetComp<AlienComp>().GetChannel(this.ColorChannel);
                int tv;

                //Log.Message($"{pawn.Name.ToStringFull}\n{channel.first.ToString()} | {pawn.story.hairColor}");

                return !returnPath.NullOrEmpty() ?
                           GraphicDatabase.Get<Graphic_Multi_RotationFromData>(returnPath += (tv = (savedIndex.HasValue ? (sharedIndex = savedIndex.Value % variantCounting) :
                                                                                             (this.linkVariantIndexWithPrevious ?
                                                                                                  sharedIndex % variantCounting :
                                                                                                  (sharedIndex = Rand.Range(min: 0, variantCounting))))) == 0 ? "" : tv.ToString(),
                                                              ContentFinder<Texture2D>.Get(returnPath + "_northm", reportFailure: false) == null ? this.ShaderType.Shader : ShaderDatabase.CutoutComplex, //ShaderDatabase.Transparent,
                                                              this.drawSize * 1.5f,
                                                              channel.first, channel.second, new GraphicData
                                                                                                   {
                                                                                                       drawRotated = !this.drawRotated
                                                                                                   }) :
                           null;
            }
        }


        public class BodyAddonHediffGraphic
        {
            public HediffDef hediff;
            public string path;
            public int variantCount = 0;
            public List<BodyAddonHediffSeverityGraphic> severity;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                XmlAttribute mayRequire = xmlRoot.Attributes[name: "MayRequire"];
                int index = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.hediff),  xmlRoot.Name.Substring(index, xmlRoot.Name.Length - index), mayRequire?.Value.ToLower());

                this.path = xmlRoot.FirstChild.Value?.Trim();

                Traverse traverse = Traverse.Create(this);
                foreach (XmlNode xmlRootChildNode in xmlRoot.ChildNodes)
                {
                    Traverse field = traverse.Field(xmlRootChildNode.Name);
                    if (field.FieldExists())
                        field.SetValue(field.GetValueType().IsGenericType ?
                                                  DirectXmlToObject.GetObjectFromXmlMethod(field.GetValueType())(xmlRootChildNode, arg2: false) :
                                                  xmlRootChildNode.InnerXml.Trim());
                }
            }
        }

        public class BodyAddonHediffSeverityGraphic
        {
            public float severity;
            public string path;
            public int variantCount = 0;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.severity = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());
                this.path   = xmlRoot.InnerXml.Trim();
            }
        }

        public class BodyAddonBackstoryGraphic
        {
            public string backstory;
            public string path;
            public int variantCount = 0;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.backstory = xmlRoot.Name;

                this.path = xmlRoot.FirstChild.Value;
            }
        }

        public class BodyAddonOffsets
        {
            public RotationOffset GetOffset(Rot4 rotation) =>
                rotation == Rot4.South ? this.south :
                rotation == Rot4.North ? this.north :
                rotation == Rot4.East  ? this.east : this.west;

            public RotationOffset south = new RotationOffset();
            public RotationOffset north = new RotationOffset();
            public RotationOffset east = new RotationOffset();
            public RotationOffset west;
        }

        public class RotationOffset
        {
            public Vector3 GetOffset(bool portrait, BodyTypeDef bodyType, string crownType)
            {
                Vector2 bodyOffset  = (portrait ? this.portraitBodyTypes  ?? this.bodyTypes : this.bodyTypes)?.FirstOrDefault(predicate: to => to.bodyType == bodyType)?.offset  ?? Vector2.zero;
                Vector2 crownOffset = (portrait ? this.portraitCrownTypes ?? this.crownTypes : this.crownTypes)?.FirstOrDefault(predicate: to => to.crownType == crownType)?.offset ?? Vector2.zero;

                return new Vector3(this.offset.x + bodyOffset.x + crownOffset.x, this.layerOffset, this.offset.y + bodyOffset.y + crownOffset.y);
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
            public Vector2 offset = Vector2.zero;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.bodyType), xmlRoot.Name);
                this.offset = (Vector2) ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
            }
        }

        public class CrownTypeOffset
        {
            public string crownType;
            public Vector2 offset = Vector2.zero;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.crownType = xmlRoot.Name;
                this.offset = (Vector2) ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
            }
        }

        public enum BodyAddonPrioritization : byte
        {
            Backstory,
            Hediff
        }
    }
}
