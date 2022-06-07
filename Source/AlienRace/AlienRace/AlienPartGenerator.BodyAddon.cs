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
        static string returnPath = string.Empty;
        static int variantCounting = 0;

        public static string ReturnPath => returnPath;

        public static int VariantCounting => variantCounting;


        public class BodyAddon
        {
            public string path;
            public string bodyPart;

            public string defaultOffset = "Center";
            [Unsaved]
            public BodyAddonOffsets defaultOffsets;

            public BodyAddonOffsets offsets = new BodyAddonOffsets();
            public bool linkVariantIndexWithPrevious = false;
            public float angle = 0f;
            public bool inFrontOfBody = false;
            public bool layerInvert = true;


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

            public string ColorChannel
            {
                get => this.colorChannel = this.colorChannel ?? "skin";
                set => this.colorChannel = value             ?? "skin";
            }

            public int variantCount = 0;
            public bool debug = true;

            public List<BodyAddonHediffGraphic> hediffGraphics;
            public List<BodyAddonBackstoryGraphic> backstoryGraphics;
            public List<BodyAddonAgeGraphic> ageGraphics;
            public List<BodyAddonDamageGraphic> damageGraphics;

            public List<BodyPartGroupDef> hiddenUnderApparelFor = new List<BodyPartGroupDef>();
            public List<string> hiddenUnderApparelTag = new List<string>();

            public string backstoryRequirement;
            public string bodyTypeRequirement;

            private ShaderTypeDef shaderType;

            public ShaderTypeDef ShaderType => this.shaderType = this.shaderType ?? ShaderTypeDefOf.Cutout;

            private List<BodyAddonPrioritization> prioritization;
            public List<BodyAddonPrioritization> Prioritization => this.prioritization ??
                                                                   (this.prioritization = new List<BodyAddonPrioritization> { BodyAddonPrioritization.Hediff, BodyAddonPrioritization.Backstory, BodyAddonPrioritization.Age, BodyAddonPrioritization.Damage});



            public virtual bool CanDrawAddon(Pawn pawn) =>
                (pawn.Drawer.renderer.graphics.apparelGraphics.NullOrEmpty() || ((this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) ||
                !pawn.apparel.WornApparel.Any(predicate: ap => !ap.def.apparel.hatRenderedFrontOfFace && ap.def.apparel.bodyPartGroups.Any(predicate: bpgd => this.hiddenUnderApparelFor.Contains(bpgd)) ||
                ap.def.apparel.tags.Any(predicate: s => this.hiddenUnderApparelTag.Contains(s))))) && (pawn.GetPosture() == PawnPosture.Standing || this.drawnOnGround) && ((pawn.CurrentBed()?.def.building.bed_showSleeperBody ?? true) || this.drawnInBed) &&
                    (this.backstoryRequirement.NullOrEmpty() || pawn.story.AllBackstories.Any(predicate: b => b.identifier == this.backstoryRequirement)) &&
                    (this.drawnDesiccated || pawn.Corpse?.GetRotStage() != RotStage.Dessicated) &&
                    (this.bodyPart.NullOrEmpty() ||
                     (pawn.health.hediffSet.GetNotMissingParts().Any(predicate: bpr => bpr.untranslatedCustomLabel == this.bodyPart || bpr.def.defName == this.bodyPart)) ||
                     (this.hediffGraphics?.Any(predicate: bahg => bahg.hediff == HediffDefOf.MissingBodyPart) ?? false)) &&
               (pawn.gender == Gender.Female ? this.drawForFemale : this.drawForMale) && (this.bodyTypeRequirement.NullOrEmpty() || pawn.story.bodyType.ToString() == this.bodyTypeRequirement);

            public class BodyAddonPawnWrapper
            {
                private Pawn WrappedPawn { get; set; }

                public BodyAddonPawnWrapper(Pawn pawn) => this.WrappedPawn = pawn;
                public BodyAddonPawnWrapper(){}

                public virtual bool HasBackStoryWithIdentifier(string backstoryId) => this.WrappedPawn.story.AllBackstories.Any(bs => bs.identifier == backstoryId);

                public virtual IEnumerable<float> SeverityOfHediffsOnPart(HediffDef hediffDef, string part) =>
                    this.WrappedPawn.health.hediffSet.hediffs.Where(h => h.def == hediffDef &&
                                                                         (h.Part == null                         ||
                                                                          part.NullOrEmpty()                     ||
                                                                          h.Part.untranslatedCustomLabel == part ||
                                                                          h.Part.def.defName             == part))
                     .Select(h => h.Severity);

                public virtual LifeStageDef CurrentLifeStageDef => this.WrappedPawn.ageTracker.CurLifeStage;

                public virtual bool HasHediffOnPartBelowHealthThreshold(string part, float healthThreshold)
                {
                    // look for part where a given hediff has a part matching defined part
                    return this.WrappedPawn.health.hediffSet.hediffs
                     .Where(predicate: h => h.Part.untranslatedCustomLabel == part ||
                                            h.Part.def.defName             == part)
                         //check if part health is less than health texture limit, needs to config ascending
                     .Any(h => healthThreshold >= this.WrappedPawn.health.hediffSet.GetPartHealth(h.Part));
                }
            }

            public void GraphicCycle(BodyAddonPawnWrapper pawn, string bodyPart)
            {
                foreach (BodyAddonPrioritization prio in this.Prioritization)
                {
                    switch (prio)
                    {
                        case BodyAddonPrioritization.Backstory:
                            if (this.backstoryGraphics?.FirstOrDefault(predicate: babgs => pawn.HasBackStoryWithIdentifier(babgs.backstory)) is { } babg)
                            {
                                returnPath = babg.path;
                                variantCounting = babg.variantCount;
                                babg.GraphicCycle(pawn,bodyPart);//check backstory, set path to default path, then check deeper in tree
                            }
                            break;
                        case BodyAddonPrioritization.Hediff:
                            if (!this.hediffGraphics.NullOrEmpty())
                                foreach (BodyAddonHediffGraphic bahg in this.hediffGraphics)
                                {
                                    foreach (float severity in pawn.SeverityOfHediffsOnPart(bahg.hediff, bodyPart))
                                    {
                                        returnPath = bahg.path;//set path to default path
                                        variantCounting = bahg.variantCount;

                                        if (!bahg.severity.NullOrEmpty())//is there severity?
                                        {
                                            foreach (BodyAddonHediffSeverityGraphic bahsg in
                                                     bahg.severity.Where(bahsg => severity >= bahsg.severity))
                                            {
                                                returnPath      = bahsg.path; //set path as default severity path
                                                variantCounting = bahsg.variantCount;
                                                bahsg.GraphicCycle(pawn, bodyPart); //check deeper in tree under severity
                                                break;
                                            }

                                            break;
                                        }
                                        bahg.GraphicCycle(pawn,bodyPart);//check deeper in tree without severity
                                        break;
                                    }
                                }
                            break;
                        case BodyAddonPrioritization.Age:
                            if (!this.ageGraphics.NullOrEmpty())
                                foreach (BodyAddonAgeGraphic baag in this.ageGraphics)
                                {
                                    if (baag.age == pawn.CurrentLifeStageDef)//compare current age to age marked under agegraphics
                                    {
                                        returnPath = baag.path;//set path as default age path
                                        variantCounting = baag.variantCount;
                                        baag.GraphicCycle(pawn,bodyPart);//check deeper into tree
                                        break;
                                    }
                                }
                            break;
                        case BodyAddonPrioritization.Damage:
                            if (!this.damageGraphics.NullOrEmpty())
                            {
                                BodyAddonDamageGraphic matchingBadg = this.damageGraphics.Find(badg =>
                                                             pawn.HasHediffOnPartBelowHealthThreshold(bodyPart,
                                                                 badg.damage));
                                if (matchingBadg != null)
                                {
                                    returnPath      = matchingBadg.path; //set path to damaged path, dont continue down tree
                                    variantCounting = matchingBadg.variantCount;
                                }
                            }
                            break;
                        default:
                            throw new ArrayTypeMismatchException();
                    }
                    if (!returnPath.NullOrEmpty())
                        break;
                }
            }


            public virtual Graphic GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                variantCounting = 0;
                returnPath = string.Empty;//reset path to empty when method called
                this.GraphicCycle(new BodyAddonPawnWrapper(pawn),this.bodyPart);

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
        public class BodyAddonDamageGraphic : BodyAddon
        {
            public float damage;
            public new string path;
            public new int variantCount = 0;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.damage = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());
                this.path = xmlRoot.InnerXml.Trim();
            }
        }
        public class BodyAddonAgeGraphic : BodyAddon
        {
            public LifeStageDef age;
            public new string path;
            public new int variantCount = 0;
            public new List<BodyAddonHediffGraphic> hediffGraphics;
            public new List<BodyAddonBackstoryGraphic> backstoryGraphics;
            public new List<BodyAddonAgeGraphic> ageGraphics;
            public new List<BodyAddonDamageGraphic> damageGraphics;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                XmlAttribute mayRequire = xmlRoot.Attributes[name: "MayRequire"];
                int index = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.age), xmlRoot.Name.Substring(index, xmlRoot.Name.Length - index), mayRequire?.Value.ToLower());

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
        public class BodyAddonHediffGraphic:BodyAddon
        {
            public HediffDef hediff;
            public new string path;
            public new int variantCount = 0;
            public List<BodyAddonHediffSeverityGraphic> severity;
            public new List<BodyAddonHediffGraphic> hediffGraphics;
            public new List<BodyAddonBackstoryGraphic> backstoryGraphics;
            public new List<BodyAddonAgeGraphic> ageGraphics;
            public new List<BodyAddonDamageGraphic> damageGraphics;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                XmlAttribute mayRequire = xmlRoot.Attributes[name: "MayRequire"];
                int index = mayRequire != null ? xmlRoot.Name.LastIndexOf(value: '\"') + 1 : 0;
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.hediff), xmlRoot.Name.Substring(index, xmlRoot.Name.Length - index), mayRequire?.Value.ToLower());

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

        public class BodyAddonHediffSeverityGraphic:BodyAddon
        {
            public float severity;
            public new string path;
            public new int variantCount = 0;
            public new List<BodyAddonHediffGraphic> hediffGraphics;
            public new List<BodyAddonBackstoryGraphic> backstoryGraphics;
            public new List<BodyAddonAgeGraphic> ageGraphics;
            public new List<BodyAddonDamageGraphic> damageGraphics;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.severity = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());
                this.path = xmlRoot.InnerXml.Trim();
            }
        }

        public class BodyAddonBackstoryGraphic:BodyAddon
        {
            public string backstory;
            public new string path;
            public new int variantCount = 0;
            public new List<BodyAddonHediffGraphic> hediffGraphics;
            public new List<BodyAddonBackstoryGraphic> backstoryGraphics;
            public new List<BodyAddonAgeGraphic> ageGraphics;
            public new List<BodyAddonDamageGraphic> damageGraphics;
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
                rotation == Rot4.East ? this.east : this.west;

            public RotationOffset south = new RotationOffset();
            public RotationOffset north = new RotationOffset();
            public RotationOffset east = new RotationOffset();
            public RotationOffset west;
        }

        public class RotationOffset
        {
            public Vector3 GetOffset(bool portrait, BodyTypeDef bodyType, string crownType)
            {
                Vector2 bodyOffset = (portrait ? this.portraitBodyTypes ?? this.bodyTypes : this.bodyTypes)?.FirstOrDefault(predicate: to => to.bodyType == bodyType)?.offset ?? Vector2.zero;
                Vector2 crownOffset = (portrait ? this.portraitCrownTypes ?? this.crownTypes : this.crownTypes)?.FirstOrDefault(predicate: to => to.crownType == crownType)?.offset ?? Vector2.zero;

                return new Vector3(this.offset.x + bodyOffset.x + crownOffset.x, this.layerOffset, this.offset.y + bodyOffset.y + crownOffset.y);
            }

            public float layerOffset;
            public Vector2 offset;
            public List<BodyTypeOffset> portraitBodyTypes;
            public List<BodyTypeOffset> bodyTypes;
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
                this.offset = (Vector2)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
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
                this.offset = (Vector2)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
            }
        }

        public enum BodyAddonPrioritization : byte
        {
            Backstory,
            Hediff,
            Age,
            Damage
        }
    }
}