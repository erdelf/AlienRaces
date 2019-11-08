using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System.Xml;

namespace AlienRace
{
    using JetBrains.Annotations;

    public partial class AlienPartGenerator
    {
        public class BodyAddon
        {
            public string path;
            public string bodyPart;
            public bool useSkinColor = true;
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

            public int variantCount = 0;

            public List<BodyAddonHediffGraphic> hediffGraphics;
            public List<BodyAddonBackstoryGraphic> backstoryGraphics;

            public List<BodyPartGroupDef> hiddenUnderApparelFor = new List<BodyPartGroupDef>();
            public List<string> hiddenUnderApparelTag = new List<string>();

            public string backstoryRequirement;

            // Added color channel stuff
            public string useColorChannel;
            public List<ChannelGenerator> channelGenerators;

            private ShaderTypeDef shaderType;

            public ShaderTypeDef ShaderType => this.shaderType ?? (this.shaderType = ShaderTypeDefOf.Cutout);

            public Color GetSkinColor(Pawn pawn, bool first = true)
            {
                AlienComp alienComp = pawn.TryGetComp<AlienComp>();
                if (alienComp.skinColor == Color.clear)
                {
                    ColorGenerator skinColorGen = ((ThingDef_AlienRace)pawn.def).alienRace.generalSettings.alienPartGenerator.alienskincolorgen;
                    alienComp.skinColor = skinColorGen?.NewRandomizedColor() ?? PawnSkinColors.GetSkinColor(pawn.story.melanin);
                    alienComp.skinColorSecond = skinColorGen?.NewRandomizedColor() ?? alienComp.skinColor;
                }
                return first ? alienComp.skinColor : alienComp.skinColorSecond;
            }

            public Color GetHairColor(Pawn pawn, bool first = true)
            {
                AlienComp alienComp = pawn.TryGetComp<AlienComp>();
                if (alienComp.hairColorSecond == Color.clear)
                {
                    ColorGenerator hairColorGen = ((ThingDef_AlienRace)pawn.def).alienRace.generalSettings.alienPartGenerator.alienhairsecondcolorgen;
                    alienComp.hairColorSecond = hairColorGen?.NewRandomizedColor() ?? pawn.story.hairColor;
                }
                return first ? pawn.story.hairColor : alienComp.hairColorSecond;
            }

            public Color GetColorFromChannel(Pawn pawn, bool first = true)
            {
                Color returnValue;
                AlienComp alienComp = pawn.TryGetComp<AlienComp>();
                AlienComp.GeneratedChannel channel = alienComp.colorChannels?.Find(i => i.channelName == useColorChannel);
                if (channel == null)
                {
                    channel = new AlienComp.GeneratedChannel(useColorChannel);
                    ChannelGenerator gen = ((ThingDef_AlienRace)pawn.def).alienRace.generalSettings.alienPartGenerator.channelGenerators.Find(i => i.channel == useColorChannel);
                    if (gen == null)
                    {
                        Log.Error($"Config error: no generator named {useColorChannel} found.");
                        returnValue = Color.magenta;
                    }
                    else
                    {
                        if (alienComp.colorChannels == null)
                        {
                            alienComp.colorChannels = new List<AlienComp.GeneratedChannel>();
                        }
                        channel.first = gen.first?.NewRandomizedColor() ?? Color.magenta;
                        if (gen.first == null)
                        {
                            Log.Error("First was null.");
                        }
                        channel.second = gen.second?.NewRandomizedColor() ?? channel.first;
                        if (gen.second == null)
                        {
                            Log.Error("Second was null.");
                        }
                        alienComp.colorChannels.Add(channel);
                        returnValue = first ? channel.first : channel.second;
                    }
                }
                else
                {
                    returnValue = first ? channel.first : channel.second;
                }
                return returnValue;
            }

            public Color GetColor(Pawn pawn, bool first = true)
            {
                Color returnValue;
                if (useColorChannel == null)
                {
                    if (useSkinColor)
                    {
                        returnValue = GetSkinColor(pawn, first);
                    }
                    else
                    {
                        returnValue = GetHairColor(pawn, first);
                    }
                }
                else if (useColorChannel == "skin")
                {
                    returnValue = GetSkinColor(pawn, first);
                }
                else if (useColorChannel == "hair")
                {
                    returnValue = GetHairColor(pawn, first);
                }
                else if (useColorChannel == "base")
                {
                    returnValue = Color.white;
                }
                else
                {
                    returnValue = GetColorFromChannel(pawn, first);
                }
                return returnValue;
            }

            public virtual bool CanDrawAddon(Pawn pawn) => 
                (pawn.Drawer.renderer.graphics.apparelGraphics.NullOrEmpty() || ((this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) || 
                !pawn.apparel.WornApparel.Any(predicate: ap => ap.def.apparel.bodyPartGroups.Any(predicate: bpgd => this.hiddenUnderApparelFor.Contains(item: bpgd)) || 
                ap.def.apparel.tags.Any(predicate: s => this.hiddenUnderApparelTag.Contains(item: s))))) && (pawn.GetPosture() == PawnPosture.Standing || this.drawnOnGround) && ((pawn.CurrentBed()?.def.building.bed_showSleeperBody ?? true) || this.drawnInBed) &&
                    (this.backstoryRequirement.NullOrEmpty() || pawn.story.AllBackstories.Any(predicate: b=> b.identifier == this.backstoryRequirement)) &&   
                    (this.bodyPart.NullOrEmpty() || pawn.health.hediffSet.GetNotMissingParts().Any(predicate: bpr => bpr.untranslatedCustomLabel == this.bodyPart || bpr.def.defName == this.bodyPart)) &&
               (pawn.gender == Gender.Female ? this.drawForFemale : this.drawForMale );

            public virtual Graphic GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                string returnPath;
                int variantCounting;
                if (this.backstoryGraphics?.FirstOrDefault(predicate: babgs => pawn.story.AllBackstories.Any(predicate: bs => bs.identifier == babgs.backstory)) is BodyAddonBackstoryGraphic babg)
                {
                    returnPath = babg.path;
                    variantCounting = babg.variantCount;
                }else if(this.hediffGraphics?.FirstOrDefault(predicate: bahgs => pawn.health.hediffSet.hediffs.Any(predicate: h => h.def.defName == bahgs.hediff && (h.Part == null || this.bodyPart.NullOrEmpty() || (h.Part.untranslatedCustomLabel == this.bodyPart || h.Part.def.defName == this.bodyPart)))) is BodyAddonHediffGraphic bahg)
                {
                    returnPath = bahg.path;
                    variantCounting = bahg.variantCount;
                } else
                {
                    returnPath = this.path;
                    variantCounting = this.variantCount;
                }
                int tv;
                return !returnPath.NullOrEmpty() ?
                            GraphicDatabase.Get<Graphic_Multi>(path: returnPath = (returnPath + ((tv = (savedIndex.HasValue ? (sharedIndex = savedIndex.Value) :
                                    (this.linkVariantIndexWithPrevious ?
                                        sharedIndex % variantCounting :
                                        (sharedIndex = Rand.Range(min: 0, max: variantCounting))))) == 0 ? "" : tv.ToString())),
                                shader: ContentFinder<Texture2D>.Get(itemPath: returnPath + "_northm", reportFailure: false) == null ? this.ShaderType.Shader : ShaderDatabase.CutoutComplex, //ShaderDatabase.Transparent,
                                    drawSize: this.drawSize * 1.5f,
                                        color: GetColor(pawn), colorTwo: GetColor(pawn, false)) :
                            null;
            }
        }


        public class BodyAddonHediffGraphic
        {
            public string hediff;
            public string path;
            public int variantCount = 0;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.hediff = xmlRoot.Name;
                this.path = xmlRoot.FirstChild.Value;
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
    }
}