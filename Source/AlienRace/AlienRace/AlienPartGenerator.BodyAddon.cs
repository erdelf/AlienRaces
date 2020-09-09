using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System.Xml;

namespace AlienRace
{
    using System;
    using HarmonyLib;
    using JetBrains.Annotations;

    public partial class AlienPartGenerator
    {
        public class BodyAddon
        {
            public string path;
            public string bodyPart;
            public BodyAddonOffsets offsets;
            public bool linkVariantIndexWithPrevious = false;
            public float angle = 0f;
            public bool inFrontOfBody = false;
            public float layerOffset = 0;
            public bool layerInvert = true;
            public bool drawnOnGround = true;
            public bool drawnInBed = true;
            public bool drawForMale = true;
            public bool drawForFemale = true;
            
            public Vector2 drawSize = Vector2.one;
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
            public List<string> bodyTypeRequirement;

            private ShaderTypeDef shaderType;

            public ShaderTypeDef ShaderType => this.shaderType = this.shaderType ?? ShaderTypeDefOf.Cutout;

            private List<BodyAddonPrioritization> prioritization;
            public List<BodyAddonPrioritization> Prioritization => this.prioritization ?? 
                                                                   (this.prioritization = new List<BodyAddonPrioritization> { BodyAddonPrioritization.Hediff, BodyAddonPrioritization.Backstory });



            public virtual bool CanDrawAddon(Pawn pawn) => 
                (pawn.Drawer.renderer.graphics.apparelGraphics.NullOrEmpty() || ((this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) || 
                !pawn.apparel.WornApparel.Any(predicate: ap => ap.def.apparel.bodyPartGroups.Any(predicate: bpgd => this.hiddenUnderApparelFor.Contains(item: bpgd)) || 
                ap.def.apparel.tags.Any(predicate: s => this.hiddenUnderApparelTag.Contains(item: s))))) && (pawn.GetPosture() == PawnPosture.Standing || this.drawnOnGround) && ((pawn.CurrentBed()?.def.building.bed_showSleeperBody ?? true) || this.drawnInBed) &&
                    (this.backstoryRequirement.NullOrEmpty() || pawn.story.AllBackstories.Any(predicate: b=> b.identifier == this.backstoryRequirement)) &&   
                    (this.bodyPart.NullOrEmpty() || 
                     (pawn.health.hediffSet.GetNotMissingParts().Any(predicate: bpr => bpr.untranslatedCustomLabel == this.bodyPart || bpr.def.defName == this.bodyPart)) || 
                     (this.hediffGraphics?.Any(bahg => bahg.hediff == HediffDefOf.MissingBodyPart) ?? false)) &&
               (pawn.gender == Gender.Female ? this.drawForFemale : this.drawForMale) && (this.bodyTypeRequirement.NullOrEmpty() || this.bodyTypeRequirement.Any(predicate: bt => bt == pawn.story.bodyType.ToString()));

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

                                    foreach (Hediff h in pawn.health.hediffSet.hediffs.Where(h => h.def == bahg.hediff &&
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
                           GraphicDatabase.Get<Graphic_Multi>(path: returnPath = (returnPath + ((tv = (savedIndex.HasValue ? (sharedIndex = savedIndex.Value % variantCounting) :
                                                                                                           (this.linkVariantIndexWithPrevious ?
                                                                                                                sharedIndex % variantCounting :
                                                                                                                (sharedIndex = Rand.Range(min: 0, max: variantCounting))))) == 0 ? "" : tv.ToString())),
                                                              shader: ContentFinder<Texture2D>.Get(itemPath: returnPath + "_northm", reportFailure: false) == null ? this.ShaderType.Shader : ShaderDatabase.CutoutComplex, //ShaderDatabase.Transparent,
                                                              drawSize: this.drawSize * 1.5f,
                                                              color: channel.first, channel.second) :
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
                XmlAttribute mayRequire = xmlRoot.Attributes["MayRequire"];
                int index = mayRequire != null ? xmlRoot.Name.LastIndexOf('\"') + 1 : 0;
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.hediff),  xmlRoot.Name.Substring(index, xmlRoot.Name.Length - index), mayRequire?.Value.ToLower());

                this.path = xmlRoot.FirstChild.Value?.Trim();

                Traverse traverse = Traverse.Create(root: this);
                foreach (XmlNode xmlRootChildNode in xmlRoot.ChildNodes)
                {
                    Traverse field = traverse.Field(xmlRootChildNode.Name);
                    if (field.FieldExists())
                        field.SetValue(field.GetValueType().IsGenericType ?
                                       DirectXmlToObject.GetObjectFromXmlMethod(field.GetValueType())(xmlRootChildNode, false) :
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
                this.severity = float.Parse(s: xmlRoot.Name.Substring(1).Trim());
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
            public RotationOffset south;
            public RotationOffset north;
            public RotationOffset east;
            public RotationOffset west;
        }

        public class RotationOffset
        {
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
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(wanter: this, fieldName: nameof(this.bodyType), targetDefName: xmlRoot.Name);
                this.offset = (Vector2) ParseHelper.FromString(str: xmlRoot.FirstChild.Value, itemType: typeof(Vector2));
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
                this.offset = (Vector2) ParseHelper.FromString(str: xmlRoot.FirstChild.Value, itemType: typeof(Vector2));
            }
        }

        public enum BodyAddonPrioritization : byte
        {
            Backstory,
            Hediff
        }
    }
}
