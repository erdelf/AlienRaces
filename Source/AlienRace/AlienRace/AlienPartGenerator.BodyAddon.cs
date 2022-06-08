namespace AlienRace
{
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
        public class BodyAddon: GenericBodyAddonGraphic
        {
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

            public bool debug = true;

            public List<BodyPartGroupDef> hiddenUnderApparelFor = new List<BodyPartGroupDef>();
            public List<string> hiddenUnderApparelTag = new List<string>();

            public string backstoryRequirement;
            public string bodyTypeRequirement;

            private ShaderTypeDef shaderType;

            public ShaderTypeDef ShaderType => this.shaderType = this.shaderType ?? ShaderTypeDefOf.Cutout;

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

                private bool IsHediffOfDefAndPart(Hediff hediff, HediffDef hediffDef, string part) =>
                    hediff.def == hediffDef &&
                    (hediff.Part == null                         ||
                     part.NullOrEmpty()                          ||
                     hediff.Part.untranslatedCustomLabel == part ||
                     hediff.Part.def.defName             == part);
                
                public virtual IEnumerable<float> SeverityOfHediffsOnPart(HediffDef hediffDef, string part) =>
                    this.WrappedPawn.health.hediffSet.hediffs
                     .Where(h => IsHediffOfDefAndPart(h, hediffDef, part))
                     .Select(h => h.Severity);

                public virtual bool HasHediffOfDefAndPart(HediffDef hediffDef, string part) => this.WrappedPawn.health.hediffSet.hediffs
                     .Any(h => IsHediffOfDefAndPart(h, hediffDef, part));
                
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

            public IBodyAddonGraphic GetBestGraphic(BodyAddonPawnWrapper pawn, string bodyPart)
            {
                IBodyAddonGraphic bestGraphic = this;
                IEnumerator<IBodyAddonGraphic> currentGraphicSet = this.GetSubGraphics(pawn, bodyPart);
                while (currentGraphicSet.MoveNext())
                {
                    IBodyAddonGraphic current = currentGraphicSet.Current;
                    if (current?.IsApplicable(pawn, bodyPart) ?? false)
                    {
                        bestGraphic                = current;
                        currentGraphicSet          = current.GetSubGraphics(pawn, bodyPart);
                    }
                }

                return bestGraphic;
            }


            public virtual Graphic GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                IBodyAddonGraphic bestGraphic = this.GetBestGraphic(new BodyAddonPawnWrapper(pawn),this.bodyPart);
                
                string returnPath = bestGraphic.GetPath() ?? string.Empty;
                int    variantCounting = bestGraphic.GetVariantCount();

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


            // Top level so always considered applicable
            public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) => true;
        }
        
        public interface IBodyAddonGraphic
        {
            public string GetPath();
            public int    GetVariantCount();
            public int    IncrementVariantCount();

            public IEnumerator<IBodyAddonGraphic> GetSubGraphics(BodyAddon.BodyAddonPawnWrapper pawn, string part);
            public bool                           IsApplicable(BodyAddon.BodyAddonPawnWrapper             pawn, string part);
        }

        public abstract class AbstractBodyAddonGraphic: IBodyAddonGraphic
        {
            public string path;
            public int    variantCount;
            public string GetPath() => this.path;

            public int GetVariantCount()       => this.variantCount;
            public int IncrementVariantCount() => this.variantCount++;

            public abstract IEnumerator<IBodyAddonGraphic> GetSubGraphics(
                BodyAddon.BodyAddonPawnWrapper pawn, string part);
            
            public abstract bool IsApplicable(BodyAddon.BodyAddonPawnWrapper pawn, string part);
        }

        public abstract class GenericBodyAddonGraphic : AbstractBodyAddonGraphic
        {
            public List<BodyAddonHediffGraphic>    hediffGraphics;
            public List<BodyAddonBackstoryGraphic> backstoryGraphics;
            public List<BodyAddonAgeGraphic>       ageGraphics;
            public List<BodyAddonDamageGraphic>    damageGraphics;

            public override IEnumerator<IBodyAddonGraphic> GetSubGraphics(
                BodyAddon.BodyAddonPawnWrapper pawn, string part)
            {
                foreach (IBodyAddonGraphic graphic in this.backstoryGraphics ?? Enumerable.Empty<IBodyAddonGraphic>()) { yield return graphic; }
                foreach (IBodyAddonGraphic graphic in this.hediffGraphics ?? Enumerable.Empty<IBodyAddonGraphic>()) { yield return graphic; }
                foreach (IBodyAddonGraphic graphic in this.ageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>()) { yield return graphic; }
                foreach (IBodyAddonGraphic graphic in this.damageGraphics ?? Enumerable.Empty<IBodyAddonGraphic>()) { yield return graphic; }
            }
        }
        
        public class BodyAddonDamageGraphic : AbstractBodyAddonGraphic
        {
            public float damage;
            
            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.damage = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());
                this.path = xmlRoot.InnerXml.Trim();
            }

            public override IEnumerator<IBodyAddonGraphic> GetSubGraphics(
                BodyAddon.BodyAddonPawnWrapper pawn, string part) => Enumerable.Empty<IBodyAddonGraphic>().GetEnumerator();
            public override bool                           IsApplicable(BodyAddon.BodyAddonPawnWrapper   pawn, string part) => pawn.HasHediffOnPartBelowHealthThreshold(part, this.damage);
        }
        public class BodyAddonAgeGraphic : GenericBodyAddonGraphic
        {
            public LifeStageDef age;

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

            public override bool IsApplicable(BodyAddon.BodyAddonPawnWrapper pawn, string part) => pawn.CurrentLifeStageDef == this.age;
        }
        public class BodyAddonHediffGraphic: GenericBodyAddonGraphic
        {
            public HediffDef hediff;
            public List<BodyAddonHediffSeverityGraphic> severity;

            public override IEnumerator<IBodyAddonGraphic> GetSubGraphics(BodyAddon.BodyAddonPawnWrapper pawn, string part)
            {
                float maxSeverityOfHediff = pawn.SeverityOfHediffsOnPart(this.hediff, part).Max();
                foreach (BodyAddonHediffSeverityGraphic graphic in this.severity?.Where(s => maxSeverityOfHediff >= s.severity) ?? Enumerable.Empty<BodyAddonHediffSeverityGraphic>()) yield return graphic;

                IEnumerator<IBodyAddonGraphic> genericSubGraphics = base.GetSubGraphics(pawn, part);
                while(genericSubGraphics.MoveNext())
                {
                    yield return genericSubGraphics.Current;
                }
            }

            public override bool IsApplicable(BodyAddon.BodyAddonPawnWrapper pawn, string part) => pawn.HasHediffOfDefAndPart(this.hediff, part);

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

        public class BodyAddonHediffSeverityGraphic:GenericBodyAddonGraphic
        {
            public float severity;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.severity = float.Parse(xmlRoot.Name.Substring(startIndex: 1).Trim());
                this.path = xmlRoot.InnerXml.Trim();
            }

            // In isolation severity graphics must always be considered applicable because we don't have the hediff
            // As severityGraphics are only valid nested It is expected that the list will only contain applicable instances. 
            public override bool IsApplicable(BodyAddon.BodyAddonPawnWrapper pawn, string part) => true;
        }

        public class BodyAddonBackstoryGraphic: GenericBodyAddonGraphic
        {
            public string backstory;
            
            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.backstory = xmlRoot.Name;

                this.path = xmlRoot.FirstChild.Value;
            }

            public override bool IsApplicable(BodyAddon.BodyAddonPawnWrapper pawn, string part) => pawn.HasBackStoryWithIdentifier(this.backstory);
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
    }
}