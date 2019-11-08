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

            /// <summary> Helper function to get the skin color of a pawn. If no skin color is found, this function generates it before returning the desired data. </summary>
            /// <param name="pawn"> The pawn to retrieve skin data from. </param>
            /// <param name="first"> Flag to return the primary skin color when <c>true</c> or the secondary color when <c>false</c> (Defaults to <c>true</c>). </param>
            /// <returns> The primary or secondary skin color of the pawn. </returns>
            public Color GetSkinColor(Pawn pawn, bool first = true)
            {
                AlienComp alienComp = pawn.TryGetComp<AlienComp>();
                if (alienComp.skinColor == Color.clear)
                {
                    ColorGenerator skinColorGen = ((ThingDef_AlienRace)pawn.def).alienRace.generalSettings.alienPartGenerator.alienskincolorgen;
                    alienComp.skinColor = skinColorGen?.NewRandomizedColor() ?? PawnSkinColors.GetSkinColor(pawn.story.melanin);
                    ColorGenerator skinSecondColorGen = ((ThingDef_AlienRace)pawn.def).alienRace.generalSettings.alienPartGenerator.alienskinsecondcolorgen;
                    alienComp.skinColorSecond = skinSecondColorGen?.NewRandomizedColor() ?? alienComp.skinColor;
                }
                return first ? alienComp.skinColor : alienComp.skinColorSecond;
            }

            /// <summary> Helper function to get the hair color of a pawn. If no hair color is found, this function generates it before returning the desired data. </summary>
            /// <param name="pawn"> The pawn to retrieve hair data from. </param>
            /// <param name="first"> Flag to return the primary hair color when <c>true</c> or the secondary color when <c>false</c> (Defaults to <c>true</c>). </param>
            /// <returns> The primary or secondary hair color of the pawn. </returns>
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

            /// <summary>
            /// Helper function to get the color from this bodyAddon's desired <c>useColorChannel</c>.<br/>
            /// If the channel named <c>useColorChannel</c> is not saved on the pawn, this function first generates its color data from the pawn's def.<br/>
            /// If the channel named <c>useColorChannel</c> is not found in the pawn's def, this function pushes an error message to the log and returns <c>Color.clear</c>.<br/>
            /// If the channel does not specify a <c>first</c> color generator in def of <c>useColorChannel</c>, this function pushes an error message to the log and returns <c>Color.clear</c>.<br/>
            /// </summary>
            /// <param name="pawn"> The pawn to retrieve color data from. </param>
            /// <param name="first"> Flag to return the primary color when <c>true</c> or the secondary color when <c>false</c> (Defaults to <c>true</c>). </param>
            /// <returns> The primary or secondary color of the <c>useColorChannel</c>. If this function encounters an error at any time, returns <c>Color.clear</c>. </returns>
            public Color GetColorFromChannel(Pawn pawn, bool first = true)
            {
                Color returnValue; // My current CS instructor is rather insistant on only having one 
                                   // return in the whole function. According to him alot of the errors
                                   // he encountered in industry were caused by returning at odd times.
                AlienComp alienComp = pawn.TryGetComp<AlienComp>();

                if (alienComp.colorChannels == null) // Initialize the list if it is null.
                    alienComp.colorChannels = new List<AlienComp.GeneratedChannel>();

                AlienComp.GeneratedChannel channel = alienComp.colorChannels.Find(i => i.channelName == useColorChannel);

                if (channel == null) // If no channel with a channelName of useColorChannel was found, we need to generate it.
                {
                    // Make sure first that there even is supposed to be a channel with this name.
                    ChannelGenerator gen = ((ThingDef_AlienRace)pawn.def).alienRace.generalSettings.alienPartGenerator.channelGenerators.Find(i => i.channel == useColorChannel);
                    if (gen == null)
                    {
                        Log.Error($"Config error: no colorGenerator named {useColorChannel} found.");
                        returnValue = Color.clear;
                    }
                    else
                    {
                        // Make sure we acutally have some color generation data to work with.
                        // We don't have to repeat this proccess for the second channel as leaving it undefined is allowed behavior.
                        if (gen.first == null)
                        {
                            Log.Error($"Config error: colorGenerator {gen.channel} does not have a first.");
                            returnValue = Color.clear;
                        }
                        else // If we actually get through all those checks, we can finally set the colors.
                        {
                            // Instantiate a new color channel.
                            channel = new AlienComp.GeneratedChannel(useColorChannel);

                            // Generate some colors for the channel and set them.
                            channel.first = gen.first.NewRandomizedColor();
                            channel.second = gen.second?.NewRandomizedColor() ?? channel.first;

                            // Make sure that the new channel gets put somewhere before returning.
                            alienComp.colorChannels.Add(channel);

                            returnValue = first ? channel.first : channel.second;
                        }
                    }
                }
                else
                {
                    returnValue = first ? channel.first : channel.second;
                }
                return returnValue;
            }

            /// <summary>
            /// Helper function to get the color data from the channel specified by <c>useColorChannel</c>.<br/>
            /// If <c>useColorChannel</c> is not specified, this function defaults to old functionality (<c>useSkinColor</c> flag).<br/>
            /// This function also has two special flags to use the pawn's <c>skin</c> or <c>hair</c> color without reverting to the old system.<br/>
            /// A third special flag returns <c>Color.white</c>, allowing for an apparently non-colorized <c>bodyAddon</c>.
            /// </summary>
            /// <param name="pawn"> The pawn to retrieve color data from. </param>
            /// <param name="first"> Flag to return the <c>useColorChannel</c>'s primary color when <c>true</c> or the secondary color when <c>false</c> (Defaults to <c>true</c>). </param>
            /// <returns> The primary or secondary color of the <c>useColorChannel</c>. </returns>
            public Color GetColor(Pawn pawn, bool first = true)
            {
                Color returnValue;
                if (useColorChannel == null)
                    if (useSkinColor)
                        returnValue = GetSkinColor(pawn, first);
                    else
                        returnValue = GetHairColor(pawn, first);
                else if (useColorChannel == "skin")
                    returnValue = GetSkinColor(pawn, first);
                else if (useColorChannel == "hair")
                    returnValue = GetHairColor(pawn, first);
                else if (useColorChannel == "base")
                    returnValue = Color.white;
                else
                    returnValue = GetColorFromChannel(pawn, first);
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