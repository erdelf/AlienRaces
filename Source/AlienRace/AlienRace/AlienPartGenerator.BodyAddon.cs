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

            public virtual bool CanDrawAddon(Pawn pawn) => 
                ((this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) || 
                !pawn.apparel.WornApparel.Any(predicate: ap => ap.def.apparel.bodyPartGroups.Any(predicate: bpgd => this.hiddenUnderApparelFor.Contains(item: bpgd)) || 
                ap.def.apparel.tags.Any(predicate: s => this.hiddenUnderApparelTag.Contains(item: s)))) && (pawn.GetPosture() == PawnPosture.Standing || (pawn.InBed() && this.drawnInBed) || this.drawnOnGround) &&
                    (this.backstoryRequirement.NullOrEmpty() || pawn.story.AllBackstories.Any(predicate: b=> b.identifier == this.backstoryRequirement)) &&   
                    (this.bodyPart.NullOrEmpty() || pawn.health.hediffSet.GetNotMissingParts().Any(predicate: bpr => bpr.untranslatedCustomLabel == this.bodyPart || bpr.def.defName == this.bodyPart)) || 
               ( drawForFemale && drawForMale ? true : (pawn.gender == Gender.Female) ? drawForFemale : drawForMale );

            public virtual Graphic GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                string returnPath;
                int variantCounting;

                if (this.backstoryGraphics?.FirstOrDefault(predicate: babgs => pawn.story.AllBackstories.Any(predicate: bs => bs.identifier == babgs.backstory)) is BodyAddonBackstoryGraphic babg)
                {
                    returnPath = babg.path;
                    variantCounting = babg.variantCount;
                }else if(this.hediffGraphics?.FirstOrDefault(predicate: bahgs => pawn.health.hediffSet.hediffs.Any(predicate: h => h.def.defName == bahgs.hediff)) is BodyAddonHediffGraphic bahg)
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
                                shader: ContentFinder<Texture2D>.Get(itemPath: returnPath + "_northm", reportFailure: false) == null ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, //ShaderDatabase.Transparent,
                                    drawSize: this.drawSize * 1.5f,
                                        color: this.useSkinColor ?
                                            pawn.story.SkinColor :
                                            pawn.story.hairColor,
                                                colorTwo: this.useSkinColor ?
                                                    ((ThingDef_AlienRace) pawn.def).alienRace.generalSettings.alienPartGenerator.SkinColor(alien: pawn, first: false) :
                                                    pawn.story.hairColor) :
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