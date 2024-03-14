using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienRace
{
    using HarmonyLib;
    using RimWorld;
    using System.Reflection.Emit;
    using System.Reflection;
    using UnityEngine;
    using Verse;
    using Verse.Sound;

    public static class AlienRenderTreePatches
    {
        private static readonly Type patchType = typeof(AlienRenderTreePatches);
        public static void HarmonyInit(Harmony harmony)
        {
            
            harmony.Patch(AccessTools.Method(typeof(MeshPool), nameof(MeshPool.GetMeshSetForWidth), [typeof(float), typeof(float)]), transpiler: new HarmonyMethod(patchType, nameof(GetMeshSetForWidthTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn)), transpiler: new HarmonyMethod(patchType, nameof(GetHumanlikeBodySetForPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn)), transpiler: new HarmonyMethod(patchType, nameof(GetHumanlikeHeadSetForPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn)), transpiler: new HarmonyMethod(patchType, nameof(GetHumanlikeHairSetForPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBeardSetForPawn)), transpiler: new HarmonyMethod(patchType, nameof(GetHumanlikeHairSetForPawnTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(PawnRenderTree), "SetupDynamicNodes"), postfix: new HarmonyMethod(patchType, nameof(SetupDynamicNodesPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnRenderNode), nameof(PawnRenderNode.GetMesh)), transpiler: new HarmonyMethod(patchType, nameof(RenderNodeGetMeshTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderTree), "TrySetupGraphIfNeeded"),        new HarmonyMethod(patchType,             nameof(TrySetupGraphIfNeededPrefix)));
        }

        public class PawnRenderResolveData
        {
            //public static Tuple<Pawn, ThingDef_AlienRace, AlienPartGenerator.AlienComp, LifeStageAgeAlien, int> pawnRenderData;

            public Pawn pawn;
            public ThingDef_AlienRace alienProps;
            public AlienPartGenerator.AlienComp alienComp;
            public LifeStageAgeAlien lsaa;
        }

        public static PawnRenderResolveData pawnRenderResolveData;


        public static void TrySetupGraphIfNeededPrefix(PawnRenderTree __instance)
        {
            if (__instance.Resolved)
                return;
            
            Pawn alien = __instance.pawn;

            if (alien.def is ThingDef_AlienRace alienProps && alien.story != null)
            {
                AlienPartGenerator.AlienComp alienComp = __instance.pawn.GetComp<AlienPartGenerator.AlienComp>();
                
                if (alienComp != null)
                {
                    
                    if (alienComp.fixGenderPostSpawn)
                    {
                        float? maleGenderProbability = alien.kindDef.GetModExtension<Info>()?.maleGenderProbability ?? alienProps.alienRace.generalSettings.maleGenderProbability;
                        __instance.pawn.gender = Rand.Value >= maleGenderProbability ? Gender.Female : Gender.Male;
                        __instance.pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(__instance.pawn);

                        alienComp.fixGenderPostSpawn = false;
                    }

                    GraphicPaths graphicPaths = alienProps.alienRace.graphicPaths;
                    LifeStageAgeAlien lsaa = (alien.ageTracker.CurLifeStageRace as LifeStageAgeAlien)!;


                    if (alien.gender == Gender.Female)
                    {
                        alienComp.customDrawSize = lsaa.customFemaleDrawSize.Equals(Vector2.zero) ? lsaa.customDrawSize : lsaa.customFemaleDrawSize;
                        alienComp.customHeadDrawSize = lsaa.customFemaleHeadDrawSize.Equals(Vector2.zero) ? lsaa.customHeadDrawSize : lsaa.customFemaleHeadDrawSize;
                        alienComp.customPortraitDrawSize = lsaa.customFemalePortraitDrawSize.Equals(Vector2.zero) ? lsaa.customPortraitDrawSize : lsaa.customFemalePortraitDrawSize;
                        alienComp.customPortraitHeadDrawSize = lsaa.customFemalePortraitHeadDrawSize.Equals(Vector2.zero) ? lsaa.customPortraitHeadDrawSize : lsaa.customFemalePortraitHeadDrawSize;
                    }
                    else
                    {
                        alienComp.customDrawSize = lsaa.customDrawSize;
                        alienComp.customHeadDrawSize = lsaa.customHeadDrawSize;
                        alienComp.customPortraitDrawSize = lsaa.customPortraitDrawSize;
                        alienComp.customPortraitHeadDrawSize = lsaa.customPortraitHeadDrawSize;
                    }

                    alienComp.OverwriteColorChannel("hair", alien.story.HairColor);
                    alienComp.OverwriteColorChannel("skin", alien.story.SkinColor);
                    alienComp.OverwriteColorChannel("skinBase", alien.story.SkinColorBase);
                    alienComp.OverwriteColorChannel("favorite", alien.story.favoriteColor);
                    alienComp.OverwriteColorChannel("favorite", second: alienComp.ColorChannels["favorite"].second != Color.clear ? null : alien.story.favoriteColor);

                    if (alien.Corpse?.GetRotStage() == RotStage.Rotting)
                        alienComp.OverwriteColorChannel("skin", PawnRenderUtility.GetRottenColor(alien.story.SkinColor));

                    alienComp.RegenerateColorChannelLinks();

                    pawnRenderResolveData = new PawnRenderResolveData
                                            {
                                                pawn = alien,
                                                alienProps = alienProps,
                                                alienComp = alienComp,
                                                lsaa = lsaa
                                            };
                    
                    portraitRender = new Pair<WeakReference, bool>(new WeakReference(alien), false);

                    /*
                    int sharedIndex = 0;

                    string bodyPath = graphicPaths.body.GetPath(alien, ref sharedIndex, alienComp.bodyVariant < 0 ? null : alienComp.bodyVariant);
                    alienComp.bodyVariant = sharedIndex;
                    string bodyMask = graphicPaths.bodyMasks.GetPath(alien, ref sharedIndex, alienComp.bodyMaskVariant < 0 ? null : alienComp.bodyMaskVariant);
                    alienComp.bodyMaskVariant = sharedIndex;

                    Shader skinShader = graphicPaths.skinShader?.Shader ?? ShaderDatabase.CutoutSkin;

                    if (skinShader == ShaderDatabase.CutoutSkin && alien.story.SkinColorOverriden)
                        skinShader = ShaderDatabase.CutoutSkinColorOverride;


                    __instance.nakedGraphic = !bodyPath.NullOrEmpty() ?
                                                  CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi),
                                                                                                bodyPath, bodyMask.NullOrEmpty() && ContentFinder<Texture2D>.Get(bodyPath + "_northm", reportFailure: false) == null ?
                                                                                                              skinShader : ShaderDatabase.CutoutComplex,
                                                                                                Vector2.one, alien.story.SkinColor, apg.SkinColor(alien, first: false), null,
                                                                                                0, [graphicPaths.SkinColoringParameter], bodyMask)) :
                                                  null;

                    __instance.rottingGraphic = !bodyPath.NullOrEmpty() ?
                                                GraphicDatabase.Get<Graphic_Multi>(bodyPath, ContentFinder<Texture2D>.Get(bodyPath + "_northm", reportFailure: false) == null ?
                                                                                                 skinShader : ShaderDatabase.CutoutComplex,
                                                                                   Vector2.one, PawnRenderUtility.GetRottenColor(alien.story.SkinColor), PawnRenderUtility.GetRottenColor(apg.SkinColor(alien, first: false)), null, bodyMask) :
                                                null;

                    string skeletonPath = graphicPaths.skeleton.GetPath(alien, ref sharedIndex, alienComp.bodyVariant);
                    __instance.dessicatedGraphic = !skeletonPath.NullOrEmpty() ?
                                                       GraphicDatabase.Get<Graphic_Multi>(skeletonPath, ShaderDatabase.Cutout) :
                                                       null;

                    string headPath = graphicPaths.head.GetPath(alien, ref sharedIndex, alienComp.headVariant < 0 ? null : alienComp.headVariant);
                    alienComp.headVariant = sharedIndex;

                    string headMask = graphicPaths.headMasks.GetPath(alien, ref sharedIndex, alienComp.headMaskVariant < 0 ? null : alienComp.headMaskVariant);
                    alienComp.headMaskVariant = sharedIndex;

                    __instance.headGraphic = alien.health.hediffSet.HasHead && !headPath.NullOrEmpty() ?
                                                 CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi),
                                                                                               headPath, headMask.NullOrEmpty() && ContentFinder<Texture2D>.Get(headPath + "_northm", reportFailure: false) == null ?
                                                                                                             skinShader : ShaderDatabase.CutoutComplex,
                                                                                               Vector2.one, alien.story.SkinColor, apg.SkinColor(alien, first: false), null,
                                                                                               0, new List<ShaderParameter> { graphicPaths.SkinColoringParameter }, headMask))
                                                 : null;

                    __instance.desiccatedHeadGraphic = alien.health.hediffSet.HasHead && !headPath.NullOrEmpty() ?
                                                           GraphicDatabase.Get<Graphic_Multi>(headPath, ShaderDatabase.Cutout, Vector2.one, PawnRenderUtility.GetRottenColor(Color.white)) :
                                                           null;

                    string skullPath = graphicPaths.skull.GetPath(alien, ref sharedIndex, alienComp.headVariant);
                    __instance.skullGraphic = alien.health.hediffSet.HasHead && !skullPath.NullOrEmpty()
                                                  ? GraphicDatabase.Get<Graphic_Multi>(skullPath, ShaderDatabase.Cutout, Vector2.one, Color.white)
                                                  : null;


                    __instance.hairGraphic = !(__instance.pawn.story.hairDef?.noGraphic ?? true) && alienProps.alienRace.styleSettings[typeof(HairDef)].hasStyle ? GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.hairDef.texPath, ContentFinder<Texture2D>.Get(__instance.pawn.story.hairDef.texPath + "_northm", reportFailure: false) == null ?
                                                                                                                           (alienProps.alienRace.styleSettings[typeof(HairDef)].shader?.Shader ?? ShaderDatabase.Transparent) :
                                                                                                                           ShaderDatabase.CutoutComplex, Vector2.one, alien.story.HairColor,
                                                                                alienComp.GetChannel(channel: "hair").second) : null;

                    string stumpPath = graphicPaths.stump.GetPath(alien, ref sharedIndex, alienComp.headVariant);
                    __instance.headStumpGraphic = !stumpPath.NullOrEmpty() ?
                                                      GraphicDatabase.Get<Graphic_Multi>(stumpPath, alien.story.SkinColor == apg.SkinColor(alien, first: false) ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex,
                                                                                                                Vector2.one, alien.story.SkinColor, apg.SkinColor(alien, first: false))
                                                      : null;

                    __instance.desiccatedHeadStumpGraphic = !stumpPath.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(stumpPath, ShaderDatabase.Cutout, Vector2.one, PawnRenderUtility.GetRottenColor(Color.white)) : null;

                    if (ModLister.BiotechInstalled)
                    {
                        __instance.furCoveredGraphic = alien.story.furDef != null ? GraphicDatabase.Get<Graphic_Multi>(alien.story.furDef.GetFurBodyGraphicPath(alien), ShaderDatabase.CutoutSkinOverlay, Vector2.one, alien.story.HairColor, alienComp.GetChannel(channel: "hair").second) : null;
                    }
                    if (ModsConfig.BiotechActive)
                    {
                        __instance.swaddledBabyGraphic = GraphicDatabase.Get<Graphic_Multi>(graphicPaths.swaddle.GetPath(alien, ref sharedIndex, alien.HashOffset()), ShaderDatabase.Cutout, Vector2.one, CachedData.swaddleColor(__instance));
                    }

                    if (alien.style != null && ModsConfig.IdeologyActive && (!ModLister.BiotechInstalled || alien.genes == null || !alien.genes.GenesListForReading.Any(x => x.def.tattoosVisible && x.Active)))
                    {
                        AlienPartGenerator.ExposableValueTuple<Color, Color> tattooColor = alienComp.GetChannel("tattoo");

                        if (alien.style.FaceTattoo != null && alien.style.FaceTattoo != TattooDefOf.NoTattoo_Face)
                            __instance.faceTattooGraphic = GraphicDatabase.Get<Graphic_Multi>(alien.style.FaceTattoo.texPath,
                                                                                              (alienProps.alienRace.styleSettings[typeof(TattooDef)].shader?.Shader ??
                                                                                               ShaderDatabase.CutoutSkinOverlay),
                                                                                              Vector2.one, tattooColor.first, tattooColor.second, null, headPath);
                        else
                            __instance.faceTattooGraphic = null;

                        if (alien.style.BodyTattoo != null && alien.style.BodyTattoo != TattooDefOf.NoTattoo_Body)
                            __instance.bodyTattooGraphic = GraphicDatabase.Get<Graphic_Multi>(alien.style.BodyTattoo.texPath,
                                                                                              (alienProps.alienRace.styleSettings[typeof(TattooDef)].shader?.Shader ??
                                                                                               ShaderDatabase.CutoutSkinOverlay),
                                                                                              Vector2.one, tattooColor.first, tattooColor.second, null, __instance.nakedGraphic.path);
                        else
                            __instance.bodyTattooGraphic = null;
                    }

                    if (!(alien.style?.beardDef?.texPath?.NullOrEmpty() ?? true))
                        __instance.beardGraphic = GraphicDatabase.Get<Graphic_Multi>(alien.style.beardDef.texPath, alienProps.alienRace.styleSettings[typeof(BeardDef)].shader?.Shader ?? ShaderDatabase.Transparent,
                                                                                     Vector2.one, alien.story.HairColor);

                    alienComp.addonGraphics = [];
                    alienComp.addonVariants ??= [];
                    alienComp.addonColors ??= [];

                    sharedIndex = 0;

                    using (IEnumerator<AlienPartGenerator.BodyAddon> bodyAddons = apg.bodyAddons.Concat(Utilities.UniversalBodyAddons).GetEnumerator())
                    {
                        int addonIndex = 0;
                        while (bodyAddons.MoveNext())
                        {
                            bool colorInsertActive = false;

                            AlienPartGenerator.BodyAddon addon = bodyAddons.Current;
                            if (alienComp.addonColors.Count > addonIndex)
                            {
                                AlienPartGenerator.ExposableValueTuple<Color?, Color?> addonColor = alienComp.addonColors[addonIndex];
                                if (addonColor.first.HasValue)
                                {
                                    addon.colorOverrideOne = addonColor.first;
                                    colorInsertActive = true;
                                }

                                if (addonColor.second.HasValue)
                                {
                                    addon.colorOverrideTwo = addonColor.second;
                                    colorInsertActive = true;
                                }
                            }

                            Graphic g = addon.GetGraphic(alien, ref sharedIndex, alienComp.addonVariants.Count > addonIndex ? alienComp.addonVariants[addonIndex] : null);
                            alienComp.addonGraphics.Add(g);
                            if (alienComp.addonVariants.Count <= addonIndex)
                                alienComp.addonVariants.Add(sharedIndex);

                            if (alienComp.addonColors.Count <= addonIndex)
                            {
                                alienComp.addonColors.Add(new AlienPartGenerator.ExposableValueTuple<Color?, Color?>(null, null));
                            }
                            else if (colorInsertActive)
                            {
                                addon.colorOverrideOne = null;
                                addon.colorOverrideTwo = null;
                            }

                            addonIndex++;
                        }
                    }

                    __instance.ResolveApparelGraphics();
                    __instance.ResolveGeneGraphics();

                    PortraitsCache.SetDirty(alien);
                    GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(alien);

                    return false;

                    */
                }
            }
            else
            {
                AnimalComp comp = alien.GetComp<AnimalComp>();
                if (comp != null)
                {
                    AnimalBodyAddons extension = alien.def.GetModExtension<AnimalBodyAddons>();
                    if (extension != null)
                    {
                        comp.addonGraphics =   new List<Graphic>();
                        comp.addonVariants ??= new List<int>();
                        int sharedIndex = 0;
                        for (int i = 0; i < extension.bodyAddons.Count; i++)
                        {
                            Graphic path = extension.bodyAddons[i].GetGraphic(alien, ref sharedIndex, comp.addonVariants.Count > i ? comp.addonVariants[i] : null);
                            comp.addonGraphics.Add(path);
                            if (comp.addonVariants.Count <= i)
                                comp.addonVariants.Add(sharedIndex);
                        }
                    }
                }
            }
        }




        public static void SetupDynamicNodesPostfix(PawnRenderTree __instance)
        {
            if (__instance.pawn.RaceProps.Humanlike && __instance.pawn == pawnRenderResolveData.pawn)
            {
                AlienPartGenerator.AlienComp alienComp = pawnRenderResolveData.alienComp;

                alienComp.addonGraphics =   [];
                alienComp.addonVariants ??= [];
                alienComp.addonColors   ??= [];

                int sharedIndex = 0;

                using (IEnumerator<AlienPartGenerator.BodyAddon> bodyAddons = pawnRenderResolveData.alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons.Concat(Utilities.UniversalBodyAddons).GetEnumerator())
                {
                    int addonIndex = 0;
                    while (bodyAddons.MoveNext())
                    {
                        bool colorInsertActive = false;

                        AlienPartGenerator.BodyAddon addon = bodyAddons.Current!;
                        if (alienComp.addonColors.Count > addonIndex)
                        {
                            AlienPartGenerator.ExposableValueTuple<Color?, Color?> addonColor = alienComp.addonColors[addonIndex];
                            if (addonColor.first.HasValue)
                            {
                                addon.colorOverrideOne = addonColor.first;
                                colorInsertActive      = true;
                            }

                            if (addonColor.second.HasValue)
                            {
                                addon.colorOverrideTwo = addonColor.second;
                                colorInsertActive      = true;
                            }
                        }

                        Graphic g = addon.GetGraphic(pawnRenderResolveData.pawn, ref sharedIndex, alienComp.addonVariants.Count > addonIndex ? alienComp.addonVariants[addonIndex] : null);
                        alienComp.addonGraphics.Add(g);
                        if (alienComp.addonVariants.Count <= addonIndex)
                            alienComp.addonVariants.Add(sharedIndex);

                        if (alienComp.addonColors.Count <= addonIndex)
                        {
                            alienComp.addonColors.Add(new AlienPartGenerator.ExposableValueTuple<Color?, Color?>(null, null));
                        }
                        else if (colorInsertActive)
                        {
                            addon.colorOverrideOne = null;
                            addon.colorOverrideTwo = null;
                        }

                        addonIndex++;


                        AlienPawnRenderNodeProperties_BodyAddon nodeProps = new()
                                                                            {
                                                                                addon = addon,
                                                                                addonIndex = addonIndex,
                                                                                graphic = g,
                                                                                parentTagDef = addon.alignWithHead ? PawnRenderNodeTagDefOf.Head : PawnRenderNodeTagDefOf.Body,
                                                                                pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                                                                                workerClass = typeof(AlienPawnRenderNodeWorker_BodyAddon),
                                                                                nodeClass = typeof(AlienPawnRenderNode_BodyAddon),
                                                                                drawData = DrawData.NewWithData(new DrawData.RotationalData { rotationOffset = addon.angle },
                                                                                                                new DrawData.RotationalData { rotationOffset = -addon.angle, rotation = Rot4.East},
                                                                                                                new DrawData.RotationalData { rotationOffset = 0, rotation = Rot4.North }),
                                                                                useGraphic = true,
                                                                                alienComp = alienComp,
                                                                                debugLabel = addon.Name
                                                                            };
                        PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(nodeProps.nodeClass, pawnRenderResolveData.pawn, nodeProps, pawnRenderResolveData.pawn.Drawer.renderer.renderTree);
                        CachedData.renderTreeAddChild(pawnRenderResolveData.pawn.Drawer.renderer.renderTree, pawnRenderNode, null);
                    }
                }
            }
        }



        #region MeshSets

        public static IEnumerable<CodeInstruction> RenderNodeGetMeshTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            FieldInfo meshSetInfo       = AccessTools.Field(typeof(PawnRenderNode), "meshSet");

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int index = 0; index < instructionList.Count; index++)
            {
                CodeInstruction instruction = instructionList[index];
                yield return instruction;
                if (instruction.LoadsField(meshSetInfo) && instructionList[index+1].opcode == OpCodes.Ldarg_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.Call(patchType, nameof(RenderNodeGetMeshHelper));
                }
            }
        }

        public static GraphicMeshSet RenderNodeGetMeshHelper(GraphicMeshSet meshSet, PawnRenderNode node, PawnDrawParms parms)
        {
            if (!parms.Portrait)
                return meshSet;

            portraitRender = new Pair<WeakReference, bool>(new WeakReference(parms.pawn), true);
            return node.MeshSetFor(parms.pawn);
        }

        public static IEnumerable<CodeInstruction> GetMeshSetForWidthTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Newobj)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(GraphicMeshSet), [typeof(float), typeof(float)]));
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static Pair<WeakReference, bool> portraitRender = new(new WeakReference(new Pawn()), false);

        public static IEnumerable<CodeInstruction> GetHumanlikeHairSetForPawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo helperInfo = AccessTools.Method(patchType, nameof(GetHumanlikeHairSetForPawnHelper));


            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (instruction.IsLdloc() && instructionList[i - 1].IsStloc())
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, helperInfo);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return new CodeInstruction(instruction);
                }
            }
        }

        public static Vector2 GetHumanlikeHairSetForPawnHelper(Vector2 headFactor, Pawn pawn)
        {
            Vector2 drawSize = (portraitRender.First?.Target as Pawn == pawn && portraitRender.Second ?
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitHeadDrawSize :
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customHeadDrawSize) ?? Vector2.one;
            return drawSize * headFactor;
        }

        public static IEnumerable<CodeInstruction> GetHumanlikeHeadSetForPawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getMeshInfo = AccessTools.Method(typeof(MeshPool), nameof(MeshPool.GetMeshSetForWidth), new[] { typeof(float) });
            FieldInfo humanlikeBodyInfo = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeHeadSet));
            MethodInfo helperInfo = AccessTools.Method(patchType, nameof(GetHumanlikeHeadSetForPawnHelper));


            List<CodeInstruction> instructionList = instructions.ToList();

            foreach (CodeInstruction instruction in instructionList)
            {
                if (instruction.Calls(getMeshInfo))
                {
                    yield return new CodeInstruction(OpCodes.Box, typeof(float));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, helperInfo);
                }
                else if (instruction.LoadsField(humanlikeBodyInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 1.5f).WithLabels(instruction.ExtractLabels());
                    yield return new CodeInstruction(OpCodes.Box, typeof(float));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, helperInfo);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static GraphicMeshSet GetHumanlikeHeadSetForPawnHelper(object lifestageFactor, Pawn pawn)
        {
            Vector2 drawSize = (portraitRender.First?.Target as Pawn == pawn && portraitRender.Second ?
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitHeadDrawSize :
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customHeadDrawSize) ?? Vector2.one;

            Vector2 scaleFactor = lifestageFactor switch
            {
                Vector2 lifestageFactorV2 => lifestageFactorV2,
                float lifestageFactorF => new Vector2(lifestageFactorF, lifestageFactorF),
                _ => Vector2.one
            };

            return MeshPool.GetMeshSetForWidth(drawSize.x * scaleFactor.x, drawSize.y * scaleFactor.y);
        }

        public static IEnumerable<CodeInstruction> GetHumanlikeBodySetForPawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getMeshInfo = AccessTools.Method(typeof(MeshPool), nameof(MeshPool.GetMeshSetForWidth), new[] { typeof(float) });
            FieldInfo humanlikeBodyInfo = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeBodySet));
            MethodInfo helperInfo = AccessTools.Method(patchType, nameof(GetHumanlikeBodySetForPawnHelper));


            List<CodeInstruction> instructionList = instructions.ToList();

            foreach (CodeInstruction instruction in instructionList)
            {
                if (instruction.Calls(getMeshInfo))
                {
                    yield return new CodeInstruction(OpCodes.Box, typeof(float));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, helperInfo);
                }
                else if (instruction.LoadsField(humanlikeBodyInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 1.5f).WithLabels(instruction.ExtractLabels());
                    yield return new CodeInstruction(OpCodes.Box, typeof(float));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, helperInfo);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static GraphicMeshSet GetHumanlikeBodySetForPawnHelper(object lifestageFactor, Pawn pawn)
        {
            Vector2 drawSize = (portraitRender.First?.Target as Pawn == pawn && portraitRender.Second ?
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitDrawSize :
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customDrawSize) ?? Vector2.one;
            
            Vector2 scaleFactor = lifestageFactor switch
            {
                Vector2 lifestageFactorV2 => lifestageFactorV2,
                float lifestageFactorF => new Vector2(lifestageFactorF, lifestageFactorF),
                _ => Vector2.one
            };

            return MeshPool.GetMeshSetForWidth(drawSize.x * scaleFactor.x, drawSize.y * scaleFactor.y);
        }

        #endregion
    }

    public class AlienPawnRenderNodeProperties_BodyAddon : PawnRenderNodeProperties
    {
        public AlienPartGenerator.BodyAddon addon;
        public int                          addonIndex;
        public Graphic                      graphic;

        public AlienPartGenerator.AlienComp alienComp;
    }

    public class AlienPawnRenderNode_BodyAddon(Pawn pawn, AlienPawnRenderNodeProperties_BodyAddon props, PawnRenderTree tree) : PawnRenderNode(pawn, props, tree)
    {
        public new AlienPawnRenderNodeProperties_BodyAddon props = props;

        public override Graphic GraphicFor(Pawn pawn) => this.props.graphic;

        public override Mesh GetMesh(PawnDrawParms parms)
        {
            AlienPartGenerator.BodyAddon            ba    = this.props.addon;

            Vector3 scale = (parms.Portrait && ba.drawSizePortrait != Vector2.zero ? ba.drawSizePortrait : ba.drawSize) *
                            (ba.scaleWithPawnDrawsize ?
                                 (ba.alignWithHead ?
                                      parms.Portrait ?
                                          this.props.alienComp.customPortraitHeadDrawSize :
                                          this.props.alienComp.customHeadDrawSize :
                                      parms.Portrait ?
                                          this.props.alienComp.customPortraitDrawSize :
                                          this.props.alienComp.customDrawSize) *
                                 (ModsConfig.BiotechActive ? parms.pawn.ageTracker.CurLifeStage.bodyWidth ?? 1.5f : 1.5f) :
                                 Vector2.one * 1.5f);

            this.props.graphic.drawSize = scale;

            return this.props.graphic.MeshAt(parms.facing);
        }
    }

    public class AlienPawnRenderNodeWorker_BodyAddon : PawnRenderNodeWorker
    {
        public static AlienPawnRenderNodeProperties_BodyAddon PropsFromNode(PawnRenderNode node) =>
            ((AlienPawnRenderNode_BodyAddon)node).props;

        public static AlienPartGenerator.BodyAddon AddonFromNode(PawnRenderNode node) => 
            ((AlienPawnRenderNode_BodyAddon)node).props.addon;

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            AlienPartGenerator.BodyAddon addonFromNode = AddonFromNode(node);
            return addonFromNode.CanDrawAddon(parms.pawn);
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            AlienPawnRenderNodeProperties_BodyAddon props = PropsFromNode(node);
            AlienPartGenerator.BodyAddon            ba    = props.addon;

            ThingDef_AlienRace alienProps = (ThingDef_AlienRace) parms.pawn.def;
            if (props.addonIndex >= alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons.Count)
                ba.defaultOffsets = alienProps.alienRace.generalSettings.alienPartGenerator.offsetDefaultsDictionary[ba.defaultOffset].offsets;

            AlienPartGenerator.DirectionalOffset offsets = (parms.pawn.gender == Gender.Female ? ba.femaleOffsets : ba.offsets) ?? ba.offsets;
            Vector3 offsetVector = (ba.defaultOffsets.GetOffset(parms.facing)?.GetOffset(parms.Portrait, parms.pawn.story?.bodyType ?? BodyTypeDefOf.Male, parms.pawn.story?.headType ?? HeadTypeDefOf.Skull) ?? Vector3.zero) +
                                   (offsets.GetOffset(parms.facing)?.GetOffset(parms.Portrait, parms.pawn.story?.bodyType ?? BodyTypeDefOf.Male, parms.pawn.story?.headType ?? HeadTypeDefOf.Skull) ?? Vector3.zero);

            offsetVector.y = ba.inFrontOfBody ? 
                                 0.3f + offsetVector.y : 
                                 -0.3f - offsetVector.y;
            
            if (parms.facing == Rot4.North)
            {
                if (ba.layerInvert)
                    offsetVector.y = -offsetVector.y;
            }

            if (parms.facing == Rot4.East) 
                offsetVector.x = -offsetVector.x;

            return base.OffsetFor(node, parms, out pivot) + offsetVector;
        }
    }
}
