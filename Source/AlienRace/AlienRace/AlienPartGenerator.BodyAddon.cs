namespace AlienRace
{
    using System.Collections.Generic;
    using System.Linq;
    using BodyAddonSupport;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public partial class AlienPartGenerator
    {
        public class BodyAddon : AbstractBodyAddonGraphic
        {
            public string bodyPart;

            public           string           defaultOffset = "Center";
            [Unsaved] public BodyAddonOffsets defaultOffsets;

            public BodyAddonOffsets offsets                      = new();
            public bool             linkVariantIndexWithPrevious = false;
            public float            angle                        = 0f;
            public bool             inFrontOfBody                = false;
            public bool             layerInvert                  = true;


            public bool drawnOnGround   = true;
            public bool drawnInBed      = true;
            public bool drawnDesiccated = true;
            public bool drawForMale     = true;
            public bool drawForFemale   = true;

            public bool alignWithHead = false;

            public Vector2 drawSize              = Vector2.one;
            public Vector2 drawSizePortrait      = Vector2.zero;
            public bool    drawRotated           = true;
            public bool    scaleWithPawnDrawsize = false;

            private string colorChannel;

            public string ColorChannel
            {
                get => this.colorChannel ??= "skin";
                set => this.colorChannel = value ?? "skin";
            }

            public bool debug = true;

            public List<BodyPartGroupDef> hiddenUnderApparelFor = new List<BodyPartGroupDef>();
            public List<string>           hiddenUnderApparelTag = new List<string>();

            public string backstoryRequirement;
            public string bodyTypeRequirement;

            private ShaderTypeDef shaderType;

            public ShaderTypeDef ShaderType => this.shaderType = this.shaderType ?? ShaderTypeDefOf.Cutout;

            private bool VisibleForGenderOf(BodyAddonPawnWrapper pawn) =>
                pawn.GetGender() == Gender.Female ? this.drawForFemale : this.drawForMale;

            private bool VisibleForBodyTypeOf(BodyAddonPawnWrapper pawn) => this.bodyTypeRequirement.NullOrEmpty() ||
                                                                            pawn.HasBodyTypeNamed(this
                                                                            .bodyTypeRequirement);

            private bool VisibleUnderApparelOf(BodyAddonPawnWrapper pawn) =>
                !pawn.HasApparelGraphics()                                                             ||
                (this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) ||
                !pawn.GetWornApparel().Any(ap => !ap.hatRenderedFrontOfFace &&
                                                 ap.bodyPartGroups.Any(predicate: bpgd => this.hiddenUnderApparelFor
                                                                       .Contains(bpgd)) ||
                                                 ap.tags.Any(s => this.hiddenUnderApparelTag.Contains(s)));

            private bool VisibleForPostureOf(BodyAddonPawnWrapper pawn) =>
                (pawn.GetPosture() == PawnPosture.Standing || this.drawnOnGround) &&
                (pawn.VisibleInBed()                       || this.drawnInBed);


            private bool VisibleForBackstoryOf(BodyAddonPawnWrapper pawn) => this.backstoryRequirement.NullOrEmpty() ||
                                                                             pawn
                                                                             .HasBackstory(this.backstoryRequirement);

            private bool VisibleForRotStageOf(BodyAddonPawnWrapper pawn) =>
                this.drawnDesiccated || pawn.GetRotStage() != RotStage.Dessicated;

            private bool RequiredBodyPartExistsFor(BodyAddonPawnWrapper pawn) =>
                this.bodyPart.NullOrEmpty()          ||
                pawn.HasNamedBodyPart(this.bodyPart) ||
                (this.hediffGraphics?.Any(predicate: bahg => bahg.hediff == HediffDefOf.MissingBodyPart) ?? false);//any missing part textures need to be done on the first branch level

            public virtual bool CanDrawAddon(Pawn pawn) => this.CanDrawAddon(new BodyAddonPawnWrapper(pawn));

            private bool CanDrawAddon(BodyAddonPawnWrapper pawn) =>
                this.VisibleUnderApparelOf(pawn)     &&
                this.VisibleForPostureOf(pawn)       &&
                this.VisibleForBackstoryOf(pawn)     &&
                this.VisibleForRotStageOf(pawn)      &&
                this.RequiredBodyPartExistsFor(pawn) &&
                this.VisibleForGenderOf(pawn)        &&
                this.VisibleForBodyTypeOf(pawn);

            public IBodyAddonGraphic GetBestGraphic(BodyAddonPawnWrapper pawn, string part)
            {
                Pair<int, IBodyAddonGraphic>                     bestGraphic = new(0, this);
                Stack<Pair<int, IEnumerator<IBodyAddonGraphic>>> stack       = new();
                stack.Push(new Pair<int,
                               IEnumerator<
                                   IBodyAddonGraphic>>(1, this.GetSubGraphics(pawn, part))); // generate list of subgraphics

                // Loop through sub trees until we find a deeper match or we run out of alternatives
                while (stack.Count > 0 && (bestGraphic.Second == this || bestGraphic.First < stack.Peek().First))
                {
                    Pair<int, IEnumerator<IBodyAddonGraphic>>
                        currentGraphicSet = stack.Pop(); // get the top of the stack
                    while (currentGraphicSet.Second
                                         .MoveNext()) // exits if iterates through list of subgraphics without advancing
                    {
                        IBodyAddonGraphic current = currentGraphicSet.Second.Current; //current branch of tree
                        if (!(current?.IsApplicable(pawn, part) ?? false)) continue;
                        if (current.GetPath().NullOrEmpty())
                            // add the current layer back to the stack so we can rewind
                            stack.Push(currentGraphicSet);
                        else
                            // Only update best graphic if the current one has a valid path
                            bestGraphic = new Pair<int, IBodyAddonGraphic>(currentGraphicSet.First, current);

                        // enters next layer/branch
                        currentGraphicSet =
                            new Pair<int, IEnumerator<IBodyAddonGraphic>>(currentGraphicSet.First + 1,
                                                                          current.GetSubGraphics(pawn, part));
                    }
                }

                return bestGraphic.Second;
            }

            public virtual Graphic GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                IBodyAddonGraphic
                    bestGraphic =
                        this.GetBestGraphic(new BodyAddonPawnWrapper(pawn), this.bodyPart); //finds deepest match

                string returnPath      = bestGraphic.GetPath() ?? string.Empty;
                int    variantCounting = bestGraphic.GetVariantCount();

                if (variantCounting <= 0)
                    variantCounting = 1;

                ExposableValueTuple<Color, Color> channel = pawn.GetComp<AlienComp>()?.GetChannel(this.ColorChannel)?? new ExposableValueTuple<Color, Color>(Color.white, Color.white);
                int                               tv;

                //Log.Message($"{pawn.Name.ToStringFull}\n{channel.first.ToString()} | {pawn.story.hairColor}");

                return !returnPath.NullOrEmpty()
                           ? GraphicDatabase
                           .Get<
                                   Graphic_Multi_RotationFromData>(returnPath += (tv = (savedIndex.HasValue ? (sharedIndex = savedIndex.Value % variantCounting) : (this.linkVariantIndexWithPrevious ? sharedIndex % variantCounting : (sharedIndex = Rand.Range(min: 0, variantCounting))))) == 0 ? "" : tv.ToString(),
                                                                   ContentFinder<Texture2D>.Get(returnPath + "_northm",
                                                                       reportFailure: false) == null
                                                                       ? this.ShaderType.Shader
                                                                       : ShaderDatabase
                                                                       .CutoutComplex, //ShaderDatabase.Transparent,
                                                                   this.drawSize * 1.5f,
                                                                   channel.first, channel.second, new GraphicData
                                                                       {
                                                                           drawRotated = !this.drawRotated
                                                                       })
                           : null;
            }


            // Top level so always considered applicable
            public override bool IsApplicable(BodyAddonPawnWrapper pawn, string part) => true;
        }

        public enum BodyAddonPrioritization : byte
        {
            Severity,
            Hediff,
            Gender,
            Bodytype,
            Crowntype,
            Backstory,
            Trait,
            Age,
            Damage
        }
    }
}