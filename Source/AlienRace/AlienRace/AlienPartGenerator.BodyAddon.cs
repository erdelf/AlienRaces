using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System.Xml;
using System;

namespace AlienRace
{
    public partial class AlienPartGenerator
    {
        public class BodyAddon
        {
            public string path;
            public BodyPartDef bodyPart;
            public bool useSkinColor = true;
            public BodyAddonOffsets offsets;
            public bool linkVariantIndexWithPrevious = false;
            public float angle = 0f;
            public bool inFrontOfBody = false;
            public float layerOffset = 0;

            public int variantCount = 0;

            public List<BodyAddonHediffGraphic> hediffGraphics;
            public List<BodyAddonBackstoryGraphic> backstoryGraphics;

            public List<BodyPartGroupDef> hiddenUnderApparelFor = new List<BodyPartGroupDef>();
            public List<string> hiddenUnderApparelTag = new List<string>();

            public string backstoryRequirement;

            public virtual bool CanDrawAddon(Pawn pawn) => 
                ((this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) || 
                !pawn.apparel.WornApparel.Any(ap => ap.def.apparel.bodyPartGroups.Any(bpgd => this.hiddenUnderApparelFor.Contains(bpgd)) || 
                ap.def.apparel.tags.Any(s => this.hiddenUnderApparelTag.Contains(s)))) &&
                    (this.backstoryRequirement.NullOrEmpty() || pawn.story.AllBackstories.Any(b=> b.identifier == this.backstoryRequirement)) && 
                    RestUtility.CurrentBed(pawn) == null && !pawn.Downed && pawn.GetPosture() == PawnPosture.Standing && !pawn.Dead && 
                    (this.bodyPart == null || pawn.health.hediffSet.GetNotMissingParts().Any(bpr => bpr.def == this.bodyPart));

            public virtual Graphic GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                string path = "";
                int variantCount = 0;

                if (this.backstoryGraphics?.FirstOrDefault(babgs => pawn.story.AllBackstories.Any(bs => bs.identifier == babgs.backstory)) is BodyAddonBackstoryGraphic babg)
                {
                    path = babg.path;
                    variantCount = babg.variantCount;
                }else if(this.hediffGraphics?.FirstOrDefault(bahgs => pawn.health.hediffSet.hediffs.Any(h => h.def.defName == bahgs.hediff)) is BodyAddonHediffGraphic bahg)
                {
                    path = bahg.path;
                    variantCount = bahg.variantCount;
                } else
                {
                    path = this.path;
                    variantCount = this.variantCount;
                }

                int tv;
                return !path.NullOrEmpty() ?
                            GraphicDatabase.Get<Graphic_Multi>(path = (path + ((tv = (savedIndex.HasValue ? (sharedIndex = savedIndex.Value) :
                                    (this.linkVariantIndexWithPrevious ?
                                        sharedIndex % variantCount :
                                        (sharedIndex = Rand.Range(0, variantCount))))) == 0 ? "" : tv.ToString())),
                                ContentFinder<Texture2D>.Get(path + "_backm", false) == null ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, //ShaderDatabase.Transparent,
                                    new Vector3(1, 0, 1),
                                        this.useSkinColor ?
                                            pawn.story.SkinColor :
                                            pawn.story.hairColor,
                                                this.useSkinColor ?
                                                    (pawn.def as ThingDef_AlienRace).alienRace.generalSettings.alienPartGenerator.SkinColor(pawn, false) :
                                                    pawn.story.hairColor) :
                            null;
            }
        }


        public class BodyAddonHediffGraphic
        {
            public string hediff;
            public string path;
            public int variantCount = 0;

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

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.backstory = xmlRoot.Name;
                this.path = xmlRoot.FirstChild.Value;
            }
        }

        public class BodyAddonOffsets
        {
            public RotationOffset front;
            public RotationOffset back;
            public RotationOffset side;
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
            public BodyType bodyType;
            public Vector2 offset = Vector2.zero;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.bodyType = (BodyType) Enum.Parse(typeof(BodyType), xmlRoot.Name);
                this.offset = (Vector2) ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
            }
        }

        public class CrownTypeOffset
        {
            public string crownType;
            public Vector2 offset = Vector2.zero;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.crownType = xmlRoot.Name;
                this.offset = (Vector2) ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
            }
        }
    }
}