namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using ExtendedGraphics;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public partial class AlienPartGenerator
    {
        public class ExtendedGraphicTop : AbstractExtendedGraphic
        {

            public bool debug = true;
            public bool Debug => this.debug && (!this.path.NullOrEmpty() || this.GetSubGraphics().Any());

            public bool linkVariantIndexWithPrevious = false;

            public Vector2 drawSize         = Vector2.one;
            public Vector2 drawSizePortrait = Vector2.zero;

            public int variantCountMax;

            public int VariantCountMax
            {
                get => this.variantCountMax;
                set => this.variantCountMax = Mathf.Max(this.VariantCountMax, value);
            }
            
            private const string REWIND_PATH = "void";

            [Unsaved]
            public ResolveData resolveData;


            public IExtendedGraphic GetBestGraphic(ExtendedGraphicsPawnWrapper pawn, ResolveData data)
            {
                Pair<int, IExtendedGraphic> bestGraphic = new(0, this);
                Stack<Pair<int, IEnumerator<IExtendedGraphic>>> stack = new();
                stack.Push(new Pair<int, IEnumerator<IExtendedGraphic>>(1, this.GetSubGraphics(pawn, data).GetEnumerator())); // generate list of subgraphics

                // Loop through sub trees until we find a deeper match or we run out of alternatives
                while (stack.Count > 0 && (bestGraphic.Second == this || bestGraphic.First < stack.Peek().First))
                {
                    Pair<int, IEnumerator<IExtendedGraphic>> currentGraphicSet = stack.Pop(); // get the top of the stack

                    while (currentGraphicSet.Second.MoveNext()) // exits if iterates through list of subgraphics without advancing
                    {
                        IExtendedGraphic current = currentGraphicSet.Second.Current; //current branch of tree
                        //Log.ResetMessageCount();
                        //Log.Message(HarmonyLib.Traverse.Create(pawn).Property("WrappedPawn").GetValue<Pawn>().NameShortColored + ": " + HarmonyLib.AccessTools.GetDeclaredFields(current.GetType())[0].GetValue(current) + " | " + current.GetType().FullName + " | " + current.GetPath());
                        if (!(current?.IsApplicable(pawn, ref data) ?? false))
                            continue;
                        
                        //Log.Message("applicable");
                        //Log.Message((!current.GetPath().NullOrEmpty()).ToString());
                        //Log.Message(current.GetVariantCount().ToString());
                        if (current.GetPath() == REWIND_PATH)
                            // add the current layer back to the stack so we can rewind
                        {
                            stack.Push(currentGraphicSet);
                            continue;
                        }

                        //Log.Message(bestGraphic.Second.GetPath());

                        IEnumerable<IExtendedGraphic> subGraphics = current.GetSubGraphics(pawn, data);
                        if (subGraphics.Any())
                            currentGraphicSet = new Pair<int, IEnumerator<IExtendedGraphic>>(currentGraphicSet.First + 1, subGraphics.GetEnumerator());
                        else
                            stack.Push(currentGraphicSet);

                        if (!current.GetPath().NullOrEmpty() && current.GetVariantCount() > 0)
                            // Only update best graphic if the current one has a valid path
                        {
                            bestGraphic = new Pair<int, IExtendedGraphic>(currentGraphicSet.First, current);
                            //Log.Message(bestGraphic.Second.GetPath());
                            if (!subGraphics.Any())
                                break;
                        }
                    }
                }

                //Log.Message("Alternative Stack:" + stack.Count);

                return bestGraphic.Second;
            }

            public virtual string GetPath(Pawn pawn, ref int sharedIndex, int? savedIndex = new(), string pathAppendix = null)
            {
                IExtendedGraphic bestGraphic = this.GetBestGraphic(new ExtendedGraphicsPawnWrapper(pawn), this.resolveData); //finds deepest match

                int    variantCounting = bestGraphic.GetVariantCount();

                if (variantCounting <= 0)
                    variantCounting = 1;
                
                savedIndex ??= this.linkVariantIndexWithPrevious ? sharedIndex % this.VariantCountMax : Rand.Range(0, this.VariantCountMax);

                sharedIndex = savedIndex.Value;

                int    actualIndex = sharedIndex % variantCounting;
                string returnPath  = bestGraphic.GetPathFromVariant(ref actualIndex, out bool zero) ?? string.Empty;

                return returnPath + pathAppendix + (zero ? "" : actualIndex.ToString());
            }
            
            // Top level so always considered applicable
            public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) => true;
            

        }

        public class BodyAddon : ExtendedGraphicTop
        {
            private string name;
            public string Name => this.name ??= Path.GetFileName(this.path);

            public           string           defaultOffset = "Center";
            [Unsaved] public DirectionalOffset defaultOffsets;

            public DirectionalOffset offsets                      = new();
            public DirectionalOffset femaleOffsets                = null;
            
            public float            angle                        = 0f;
            public bool             inFrontOfBody                = false;
            public bool             layerInvert                  = true;

            public List<Condition> conditions = [];
            
            public bool alignWithHead = false;

            public bool    drawRotated           = true;
            public bool    scaleWithPawnDrawsize = false;

            public List<RenderSkipFlagDef> useSkipFlags = [];


            private string colorChannel;

            public string ColorChannel
            {
                get => this.colorChannel ??= "skin";
                set => this.colorChannel = value ?? "skin";
            }
            public Color? colorOverrideOne;
            public Color? colorOverrideTwo;
            public float  colorPostFactor = 1f;

            public bool userCustomizable = true;
            public bool allowColorOverride = false;


            private ShaderTypeDef shaderType;

            public ShaderTypeDef ShaderType
            {
                get => this.shaderType ??= ShaderTypeDefOf.Cutout;
                set => this.shaderType = value ?? ShaderTypeDefOf.Cutout;
            }
            
            /*
            private bool RequiredBodyPartExistsFor(ExtendedGraphicsPawnWrapper pawn) =>
                (pawn.HasNamedBodyPart(this.bodyPart, this.bodyPartLabel) || pawn.LinkToCorePart(this.drawWithoutPart, this.alignWithHead, this.bodyPart, this.bodyPartLabel)) ||
                this.extendedGraphics.OfType<ExtendedHediffGraphic>().Any(predicate: bahg => bahg.hediff == HediffDefOf.MissingBodyPart);
                //any missing part textures need to be done on the first branch level
            */
            public virtual bool CanDrawAddon(Pawn pawn) => 
                this.CanDrawAddon(new ExtendedGraphicsPawnWrapper(pawn));

            private bool CanDrawAddon(ExtendedGraphicsPawnWrapper pawn) => 
                this.conditions.TrueForAll(c => c.Satisfied(pawn, ref this.resolveData));

            /*
            this.VisibleUnderApparelOf(pawn)     &&
            this.VisibleForPostureOf(pawn)       &&
            this.VisibleForRotStageOf(pawn)      &&
            this.VisibleForDrafted(pawn)         &&
            this.RequiredBodyPartExistsFor(pawn) &&
            this.VisibleForJob(pawn)             &&
            this.CanDrawAddonStatic(pawn);
            */
            public virtual bool CanDrawAddonStatic(Pawn pawn) =>
                this.CanDrawAddonStatic(new ExtendedGraphicsPawnWrapper(pawn));

            private bool CanDrawAddonStatic(ExtendedGraphicsPawnWrapper pawn) => 
                this.conditions.TrueForAll(c => !c.Static || c.Satisfied(pawn, ref this.resolveData));
            /*
                this.VisibleForGenderOf(pawn)    &&
                this.VisibleForBodyTypeOf(pawn)  &&
                this.VisibleWithGene(pawn)       &&
                this.VisibleForBackstoryOf(pawn) &&
                this.VisibleForRace(pawn);
                */

            public virtual Graphic GetGraphic(Pawn pawn, ref int sharedIndex, int? savedIndex = new int?())
            {
                ExposableValueTuple<Color, Color> channel = pawn.GetComp<AlienComp>()?.GetChannel(this.ColorChannel) ?? new ExposableValueTuple<Color, Color>(Color.white, Color.white);

                Color first  = this.ColorChannel == "skin" ? 
                                   pawn.story?.skinColorOverride.HasValue ?? false ? 
                                       pawn.story.skinColorOverride.Value : 
                                       channel.first : 
                                   channel.first;

                Color second = channel.second;

                if (this.colorOverrideOne.HasValue) 
                    first = this.colorOverrideOne.Value;

                if (this.colorOverrideTwo.HasValue) 
                    second = this.colorOverrideTwo.Value;

                if (Math.Abs(this.colorPostFactor - 1f) > float.Epsilon)
                {
                    first  *= this.colorPostFactor;
                    second *= this.colorPostFactor;
                }
                
                string returnPath = this.GetPath(pawn, ref sharedIndex, savedIndex);

                return !returnPath.NullOrEmpty() ?
                           GraphicDatabase.Get<Graphic_Multi_RotationFromData>(returnPath, ContentFinder<Texture2D>.Get(returnPath + "_southm", reportFailure: false) == null ? 
                                                                                               this.ShaderType.Shader : ShaderDatabase.CutoutComplex, 
                                                                               Vector2.one, first, second, new GraphicData { drawRotated = !this.drawRotated }) :
                           null;
            }

            public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                foreach (XmlNode childNode in xmlRoot.ChildNodes)
                {
                    if (childNode.Name.Equals(nameof(this.conditions)))
                    {
                        foreach (XmlNode node in childNode.ChildNodes)
                        {
                            if (Condition.XmlNameParseKeys.TryGetValue(node.Name, out string classTag))
                            {
                                XmlAttribute attribute = xmlRoot.OwnerDocument!.CreateAttribute("Class");
                                attribute.Value = classTag;
                                node.Attributes!.SetNamedItem(attribute);
                            }
                        }
                    }
                }

                base.LoadDataFromXmlCustom(xmlRoot);
            }
        }
    }
}