namespace AlienRace
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using ExtendedGraphics;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public partial class AlienPartGenerator
    {
        public class ExtendedGraphicTop : AbstractExtendedGraphic
        {
            public bool debug = true;
            public bool Debug => this.debug && (!this.path.NullOrEmpty() || this.GetSubGraphics().MoveNext());

            public bool linkVariantIndexWithPrevious = false;

            public Vector2 drawSize         = Vector2.one;
            public Vector2 drawSizePortrait = Vector2.zero;

            public int variantCountMax;

            public int VariantCountMax
            {
                get => this.variantCountMax;
                set => this.variantCountMax = Mathf.Max(this.VariantCountMax, value);
            }

            public BodyPartDef bodyPart;
            public string      bodyPartLabel;

            private const string REWIND_PATH = "void";

            public IExtendedGraphic GetBestGraphic(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel)
            {
                Pair<int, IExtendedGraphic> bestGraphic = new(0, this);
                Stack<Pair<int, IEnumerator<IExtendedGraphic>>> stack = new();
                stack.Push(new Pair<int, IEnumerator<IExtendedGraphic>>(1, this.GetSubGraphics(pawn, part, partLabel))); // generate list of subgraphics

                // Loop through sub trees until we find a deeper match or we run out of alternatives
                while (stack.Count > 0 && (bestGraphic.Second == this || bestGraphic.First < stack.Peek().First))
                {
                    Pair<int, IEnumerator<IExtendedGraphic>> currentGraphicSet = stack.Pop(); // get the top of the stack

                    while (currentGraphicSet.Second.MoveNext()) // exits if iterates through list of subgraphics without advancing
                    {
                        IExtendedGraphic current = currentGraphicSet.Second.Current; //current branch of tree
                        //Log.ResetMessageCount();
                        //Log.Message(Traverse.Create(pawn).Property("WrappedPawn").GetValue<Pawn>().NameShortColored + ": " + AccessTools.GetDeclaredFields(current.GetType())[0].GetValue(current) + " | " + current.GetType().FullName + " | " + current.GetPath());
                        if (!(current?.IsApplicable(pawn, part, partLabel) ?? false))
                            continue;
                        /*
                        Log.Message("applicable");
                        Log.Message((!current.GetPath().NullOrEmpty()).ToString());
                        Log.Message(current.GetVariantCount().ToString());*/
                        if (current.GetPath() == REWIND_PATH)
                            // add the current layer back to the stack so we can rewind
                            stack.Push(currentGraphicSet);
                        else if(!current.GetPath().NullOrEmpty() && current.GetVariantCount() > 0)
                            // Only update best graphic if the current one has a valid path
                            bestGraphic = new Pair<int, IExtendedGraphic>(currentGraphicSet.First, current);
                        //Log.Message(bestGraphic.Second.GetPath());
                        // enters next layer/branch
                        currentGraphicSet = new Pair<int, IEnumerator<IExtendedGraphic>>(currentGraphicSet.First + 1, current.GetSubGraphics(pawn, part, partLabel));
                    }
                }

                return bestGraphic.Second;
            }

            public virtual string GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?(), string pathAppendix = null)
            {
                IExtendedGraphic bestGraphic = this.GetBestGraphic(new ExtendedGraphicsPawnWrapper(pawn), this.bodyPart, this.bodyPartLabel); //finds deepest match

                int    variantCounting = bestGraphic.GetVariantCount();

                if (variantCounting <= 0)
                    variantCounting = 1;
                
                savedIndex ??= this.linkVariantIndexWithPrevious ? sharedIndex % this.VariantCountMax : Rand.Range(0, this.VariantCountMax);

                sharedIndex = savedIndex.Value % variantCounting;

                int    actualIndex = sharedIndex;
                string returnPath  = bestGraphic.GetPathFromVariant(ref actualIndex, out bool zero) ?? string.Empty;

                return returnPath + pathAppendix + (zero ? "" : actualIndex.ToString());
            }
            
            // Top level so always considered applicable
            public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel) => true;

            [UsedImplicitly]
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
            }
        }

        public class BodyAddon : ExtendedGraphicTop
        {
            public           string           defaultOffset = "Center";
            [Unsaved] public BodyAddonOffsets defaultOffsets;

            public BodyAddonOffsets offsets                      = new();
            
            public float            angle                        = 0f;
            public bool             inFrontOfBody                = false;
            public bool             layerInvert                  = true;


            public bool drawnOnGround   = true;
            public bool drawnInBed      = true;
            public bool drawnDesiccated = true;
            public bool drawForMale     = true;
            public bool drawForFemale   = true;
            public bool drawDrafted     = true;
            public bool drawUndrafted   = true;

            public BodyAddonJobConfig jobs = new BodyAddonJobConfig();

            public bool alignWithHead = false;

            public bool    drawRotated           = true;
            public bool    scaleWithPawnDrawsize = false;


            private string colorChannel;

            public string ColorChannel
            {
                get => this.colorChannel ??= "skin";
                set => this.colorChannel = value ?? "skin";
            }

            public List<BodyPartGroupDef> hiddenUnderApparelFor = new List<BodyPartGroupDef>();
            public List<string>           hiddenUnderApparelTag = new List<string>();

            public BackstoryDef   backstoryRequirement;
            public BodyTypeDef    bodyTypeRequirement;
            public GeneDef        geneRequirement;
            public List<ThingDef> raceRequirement;

            private ShaderTypeDef shaderType;

            public ShaderTypeDef ShaderType => this.shaderType ??= ShaderTypeDefOf.Cutout;
            
            private bool VisibleUnderApparelOf(ExtendedGraphicsPawnWrapper pawn) =>
                !pawn.HasApparelGraphics()                                                             ||
                (this.hiddenUnderApparelTag.NullOrEmpty() && this.hiddenUnderApparelFor.NullOrEmpty()) ||
                !pawn.GetWornApparel().Any(ap => 
                    !ap.hatRenderedFrontOfFace && ap.bodyPartGroups.Any(predicate: bpgd => this.hiddenUnderApparelFor.Contains(bpgd)) || 
                    ap.tags.Any(s => this.hiddenUnderApparelTag.Contains(s)));

            private bool VisibleForPostureOf(ExtendedGraphicsPawnWrapper pawn) =>
                (pawn.GetPosture() == PawnPosture.Standing || this.drawnOnGround) &&
                (pawn.VisibleInBed()                       || this.drawnInBed);

            private bool VisibleForBackstoryOf(ExtendedGraphicsPawnWrapper pawn) => this.backstoryRequirement == null ||
                                                                             pawn.HasBackstory(this.backstoryRequirement);

            private bool VisibleForRotStageOf(ExtendedGraphicsPawnWrapper pawn) =>
                this.drawnDesiccated || pawn.GetRotStage() != RotStage.Dessicated;

            private bool RequiredBodyPartExistsFor(ExtendedGraphicsPawnWrapper pawn) =>
                pawn.HasNamedBodyPart(this.bodyPart, this.bodyPartLabel) ||
                (this.hediffGraphics?.Any(predicate: bahg => bahg.hediff == HediffDefOf.MissingBodyPart) ?? false);
                //any missing part textures need to be done on the first branch level

            private bool VisibleForGenderOf(ExtendedGraphicsPawnWrapper pawn) =>
                pawn.GetGender() == Gender.Female ? this.drawForFemale : this.drawForMale;

            private bool VisibleForBodyTypeOf(ExtendedGraphicsPawnWrapper pawn) => 
                this.bodyTypeRequirement == null || pawn.HasBodyType(this.bodyTypeRequirement);

            private bool VisibleForDrafted(ExtendedGraphicsPawnWrapper pawn) =>
                this.drawDrafted   && this.drawUndrafted ||
                this.drawDrafted   && pawn.Drafted       ||
                this.drawUndrafted && !pawn.Drafted;

            public bool VisibleForJob(ExtendedGraphicsPawnWrapper pawn) => 
                pawn.CurJob == null ? 
                    this.jobs.drawNoJob : 
                    !this.jobs.JobMap.TryGetValue(pawn.CurJob.def, out BodyAddonJobConfig.BodyAddonJobConfigJob jobConfig) || jobConfig.IsApplicable(pawn);

            public bool VisibleWithGene(ExtendedGraphicsPawnWrapper pawn) =>
                !ModsConfig.BiotechActive || this.geneRequirement == null || pawn.HasGene(this.geneRequirement);

            public bool VisibleForRace(ExtendedGraphicsPawnWrapper pawn) =>
                this.raceRequirement.NullOrEmpty() || this.raceRequirement.Any(pawn.IsRace);

            public virtual bool CanDrawAddon(Pawn pawn) => 
                this.CanDrawAddon(new ExtendedGraphicsPawnWrapper(pawn));

            private bool CanDrawAddon(ExtendedGraphicsPawnWrapper pawn) =>
                this.VisibleUnderApparelOf(pawn)     &&
                this.VisibleForPostureOf(pawn)       &&
                this.VisibleForBackstoryOf(pawn)     &&
                this.VisibleForRotStageOf(pawn)      &&
                this.RequiredBodyPartExistsFor(pawn) &&
                this.VisibleForGenderOf(pawn)        &&
                this.VisibleForBodyTypeOf(pawn)      &&
                this.VisibleForDrafted(pawn)         &&
                this.VisibleForJob(pawn)             &&
                this.VisibleWithGene(pawn)           &&
                this.VisibleForRace(pawn);

            public virtual Graphic GetGraphic(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                ExposableValueTuple<Color, Color> channel = pawn.GetComp<AlienComp>()?.GetChannel(this.ColorChannel) ?? new ExposableValueTuple<Color, Color>(Color.white, Color.white);

                //Log.Message($"{pawn.Name.ToStringFull}\n{channel.first.ToString()} | {pawn.story.hairColor}");

                string returnPath = this.GetPath(pawn, ref sharedIndex, savedIndex);

                return !returnPath.NullOrEmpty() ?
                           GraphicDatabase.Get<Graphic_Multi_RotationFromData>(returnPath, ContentFinder<Texture2D>.Get(returnPath + "_northm", reportFailure: false) == null ?
                                                                                               this.ShaderType.Shader : 
                                                                                               ShaderDatabase.CutoutComplex, this.drawSize * 1.5f, channel.first, channel.second, new GraphicData
                                                                                                                                                                                  {
                                                                                                                                                                                      drawRotated = !this.drawRotated
                                                                                                                                                                                  }) :
                           null;
            }

            public class BodyAddonJobConfig
            {
                public bool drawNoJob = true;

                public List<BodyAddonJobConfigJob> jobs = new List<BodyAddonJobConfigJob>();

                private Dictionary<JobDef, BodyAddonJobConfigJob> jobMap;

                public Dictionary<JobDef, BodyAddonJobConfigJob> JobMap => this.jobMap ??= this.jobs.ToDictionary(bajcj => bajcj.job);

                public class BodyAddonJobConfigJob
                {
                    public JobDef                        job;
                    public Dictionary<PawnPosture, bool> drawPostures;
                    public bool                          drawMoving = true;
                    public bool                          drawUnmoving = true;

                    public bool IsApplicable(ExtendedGraphicsPawnWrapper pawn) =>
                        (!this.drawPostures.TryGetValue(pawn.GetPosture(), out bool postureDraw) || postureDraw) && 
                        (this.drawMoving   && pawn.Moving || 
                         this.drawUnmoving && !pawn.Moving);
                }
            }
        }

        public enum ExtendedGraphicsPrioritization : byte
        {
            Severity,
            Hediff,
            Gender,
            Bodytype,
            Headtype,
            Backstory,
            Trait,
            Age,
            Damage,
            Gene,
            Race
        }
    }
}