using System;
using System.Collections.Generic;
using System.Linq;

namespace AlienRace
{
    using HarmonyLib;
    using RimWorld;
    using System.Reflection.Emit;
    using System.Reflection;
    using JetBrains.Annotations;
    using UnityEngine;
    using Verse;

    public static class AlienRenderTreePatches
    {
        private static readonly Type patchType = typeof(AlienRenderTreePatches);
        public static void HarmonyInit(AlienHarmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn)), prefix: new HarmonyMethod(patchType, nameof(GetHumanlikeBodySetForPawnPrefix)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn)), prefix: new HarmonyMethod(patchType, nameof(GetHumanlikeHeadSetForPawnPrefix)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn)), prefix: new HarmonyMethod(patchType, nameof(GetHumanlikeHeadSetForPawnPrefix)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBeardSetForPawn)), prefix: new HarmonyMethod(patchType, nameof(GetHumanlikeHeadSetForPawnPrefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnRenderTree), "SetupDynamicNodes"), postfix: new HarmonyMethod(patchType, nameof(SetupDynamicNodesPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnRenderNode), nameof(PawnRenderNode.GetMesh)), transpiler: new HarmonyMethod(patchType, nameof(RenderNodeGetMeshTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderTree), "TrySetupGraphIfNeeded"),        new HarmonyMethod(patchType,             nameof(TrySetupGraphIfNeededPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderTree), nameof(PawnRenderTree.EnsureInitialized)), postfix: new HarmonyMethod(patchType,             nameof(PawnRenderTreeEnsureInitializedPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnRenderNode_Body), nameof(PawnRenderNode.GraphicFor)), prefix: new HarmonyMethod(patchType, nameof(BodyGraphicForPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderNode_Head), nameof(PawnRenderNode.GraphicFor)), prefix: new HarmonyMethod(patchType, nameof(HeadGraphicForPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderNode_Stump), nameof(PawnRenderNode.GraphicFor)), transpiler: new HarmonyMethod(patchType, nameof(StumpGraphicForTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(HairDef),   nameof(HairDef.GraphicFor)),   transpiler: new HarmonyMethod(patchType, nameof(HairDefGraphicForTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(TattooDef), nameof(TattooDef.GraphicFor)), transpiler: new HarmonyMethod(patchType, nameof(TattooDefGraphicForTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(BeardDef), nameof(BeardDef.GraphicFor)), transpiler: new HarmonyMethod(patchType, nameof(BeardDefGraphicForTranspiler)));
        }

        public class PawnRenderResolveData
        {
            public ThingDef_AlienRace           alienProps;
            public AlienPartGenerator.AlienComp alienComp;
            public LifeStageAgeAlien            lsaa;
            public int                          sharedIndex = 0;
        }

        public static PawnRenderResolveData pawnRenderResolveData;

        public static PawnRenderResolveData RegenerateResolveData(Pawn pawn) =>
            pawnRenderResolveData?.alienComp?.parent != pawn ?
                pawnRenderResolveData = new PawnRenderResolveData
                                        {
                                            alienProps  = pawn.def as ThingDef_AlienRace,
                                            alienComp   = pawn.GetComp<AlienPartGenerator.AlienComp>(),
                                            lsaa        = pawn.ageTracker.CurLifeStageRace as LifeStageAgeAlien,
                                            sharedIndex = 0
                                        } :
                pawnRenderResolveData;

        public static Shader CheckMaskShader(string texPath, Shader shader, bool pathCheckOverride = false) =>
            (!shader.SupportsMaskTex() && (pathCheckOverride || ContentFinder<Texture2D>.Get(texPath + "_northm", reportFailure: false) != null)) ?
                ShaderDatabase.CutoutComplex : 
                shader;

        public static void TrySetupGraphIfNeededPrefix(PawnRenderTree __instance)
        {
            if (__instance.Resolved)
                return;

            Pawn alien = __instance.pawn;

            if (alien.def is ThingDef_AlienRace alienProps && alien.story != null)
            {
                Log.Message($"Setup Graph: {alien.NameFullColored}");

                RegenerateResolveData(alien);
                AlienPartGenerator.AlienComp alienComp = pawnRenderResolveData.alienComp;


                if (alienComp != null)
                {
                    
                    if (alienComp.fixGenderPostSpawn)
                    {
                        float? maleGenderProbability = alien.kindDef.GetModExtension<Info>()?.maleGenderProbability ?? alienProps.alienRace.generalSettings.maleGenderProbability;
                        __instance.pawn.gender = Rand.Value >= maleGenderProbability ? Gender.Female : Gender.Male;
                        __instance.pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(__instance.pawn);

                        alienComp.fixGenderPostSpawn = false;
                    }

                    LifeStageAgeAlien lsaa = pawnRenderResolveData.lsaa;


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
                    
                    portraitRender = new Pair<WeakReference, bool>(new WeakReference(alien), false);
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

        public static void PawnRenderTreeEnsureInitializedPostfix(PawnRenderTree __instance) => 
            pawnRenderResolveData = default;
        
        #region Graphics
        public static bool BodyGraphicForPrefix(PawnRenderNode_Body __instance, Pawn pawn, ref Graphic __result)
        {
            if (pawn.def is not ThingDef_AlienRace)
                return true;
            Log.Message($"Body GraphicFor: {pawn.NameFullColored}");

            PawnRenderResolveData pawnRenderData = RegenerateResolveData(pawn);

            //if (pawnRenderResolveData.pawn != pawn) Log.Message($"PAWNS DON'T MATCH: {pawn.NameFullColored} vs {pawnRenderResolveData.pawn.NameFullColored}");
            int                          sharedIndex  = pawnRenderData.sharedIndex;
            GraphicPaths                 graphicPaths = pawnRenderData.alienProps.alienRace.graphicPaths;
            AlienPartGenerator.AlienComp alienComp    = pawnRenderData.alienComp;
            AlienPartGenerator           apg          = pawnRenderData.alienProps.alienRace.generalSettings.alienPartGenerator;

            string bodyPath    = graphicPaths.body.GetPath(pawn, ref sharedIndex, alienComp.bodyVariant < 0 ? null : alienComp.bodyVariant);
            alienComp.bodyVariant = sharedIndex;
            string bodyMask = graphicPaths.bodyMasks.GetPath(pawn, ref sharedIndex, alienComp.bodyMaskVariant < 0 ? null : alienComp.bodyMaskVariant);
            alienComp.bodyMaskVariant = sharedIndex;

            pawnRenderData.sharedIndex = sharedIndex;

            Shader skinShader = graphicPaths.skinShader?.Shader ?? ShaderUtility.GetSkinShader(pawn);

            if (skinShader == ShaderDatabase.CutoutSkin && pawn.story.SkinColorOverriden)
                skinShader = ShaderDatabase.CutoutSkinColorOverride;


            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                string skeletonPath = graphicPaths.skeleton.GetPath(pawn, ref sharedIndex, alienComp.bodyVariant);
                __result = !skeletonPath.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(skeletonPath, ShaderDatabase.Cutout) : null;
                return false;
            }

            __result = !bodyPath.NullOrEmpty() ? 
                           CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi), bodyPath, CheckMaskShader(bodyPath, skinShader, !bodyMask.NullOrEmpty()), 
                                                                         Vector2.one, __instance.ColorFor(pawn), apg.SkinColor(pawn, first: false),
                                                                         null, 0, [graphicPaths.SkinColoringParameter], bodyMask)) :
                                          null;

            return false;
        }

        public static bool HeadGraphicForPrefix(PawnRenderNode_Head __instance, Pawn pawn, ref Graphic __result)
        {
            if (pawn.def is not ThingDef_AlienRace)
                return true;

            Log.Message($"Head GraphicFor: {pawn.NameFullColored}");

            //if (pawnRenderResolveData.pawn != pawn) Log.Message($"PAWNS DON'T MATCH: {pawn.NameFullColored} vs {pawnRenderResolveData.pawn.NameFullColored}");

            PawnRenderResolveData pawnRenderData = RegenerateResolveData(pawn);

            int                          sharedIndex  = pawnRenderData.sharedIndex;
            GraphicPaths                 graphicPaths = pawnRenderData.alienProps.alienRace.graphicPaths;
            AlienPartGenerator.AlienComp alienComp    = pawnRenderData.alienComp;
            AlienPartGenerator           apg          = pawnRenderData.alienProps.alienRace.generalSettings.alienPartGenerator;

            string headPath = graphicPaths.head.GetPath(pawn, ref sharedIndex, alienComp.headVariant < 0 ? null : alienComp.headVariant);
            alienComp.headVariant = sharedIndex;

            string headMask = graphicPaths.headMasks.GetPath(pawn, ref sharedIndex, alienComp.headMaskVariant < 0 ? null : alienComp.headMaskVariant);
            alienComp.headMaskVariant = sharedIndex;

            pawnRenderData.sharedIndex = sharedIndex;

            Shader skinShader = graphicPaths.skinShader?.Shader ?? ShaderUtility.GetSkinShader(pawn);

            if (skinShader == ShaderDatabase.CutoutSkin && pawn.story.SkinColorOverriden)
                skinShader = ShaderDatabase.CutoutSkinColorOverride;


            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                string skullPath = graphicPaths.skull.GetPath(pawn, ref sharedIndex, alienComp.headVariant);
                __result = pawn.health.hediffSet.HasHead && !skullPath.NullOrEmpty() ?
                               GraphicDatabase.Get<Graphic_Multi>(skullPath, ShaderDatabase.Cutout, Vector2.one, Color.white) : 
                               null;
                return false;
            }

            __result = pawn.health.hediffSet.HasHead && !headPath.NullOrEmpty() ?
                           CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi),
                                                                         headPath, CheckMaskShader(headPath, skinShader, !headMask.NullOrEmpty()), Vector2.one, __instance.ColorFor(pawn), 
                                                                         apg.SkinColor(pawn, first: false), null, 0, [graphicPaths.SkinColoringParameter], headMask))
                                         : null;

            return false;
        }

        public static IEnumerable<CodeInstruction> StumpGraphicForTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo baseInfo = AccessTools.Method(typeof(PawnRenderNode), nameof(PawnRenderNode.GraphicFor));

            foreach (CodeInstruction instruction in instructions)
            {
                if(instruction.Calls(baseInfo))
                    yield return CodeInstruction.Call(patchType, nameof(StumpGraphicHelper));
                else
                    yield return instruction;
            }
        }

        public static Graphic StumpGraphicHelper(PawnRenderNode_Stump node, Pawn pawn)
        {
            string path = pawnRenderResolveData.alienProps.alienRace.graphicPaths.stump.GetPath(pawn, ref pawnRenderResolveData.sharedIndex, pawnRenderResolveData.alienComp.headVariant);
            return !path.NullOrEmpty() ?
                       GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, Vector2.one, node.ColorFor(pawn), 
                                                          pawnRenderResolveData.alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(pawn, first: false)) : 
                       null;

        }

        public static IEnumerable<CodeInstruction> HairDefGraphicForTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Call && instructionList[i + 1].opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.Call(patchType, nameof(HairGraphicHelper));
                } else
                {
                    yield return instruction;
                }

                if (instruction.opcode == OpCodes.Brfalse_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,   AccessTools.Field(patchType,                                nameof(pawnRenderResolveData)));
                    yield return new CodeInstruction(OpCodes.Ldfld,    AccessTools.Field(typeof(PawnRenderResolveData),            nameof(PawnRenderResolveData.alienProps)));
                    yield return new CodeInstruction(OpCodes.Ldfld,    AccessTools.Field(typeof(ThingDef_AlienRace),               nameof(ThingDef_AlienRace.alienRace)));
                    yield return new CodeInstruction(OpCodes.Ldfld,    AccessTools.Field(typeof(ThingDef_AlienRace.AlienSettings), nameof(ThingDef_AlienRace.AlienSettings.styleSettings)));
                    yield return new CodeInstruction(OpCodes.Ldtoken,  typeof(HairDef));
                    yield return new CodeInstruction(OpCodes.Call,     AccessTools.Method(typeof(Type),          nameof(Type.GetTypeFromHandle)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Dictionary<Type, StyleSettings>), "get_Item"));
                    yield return new CodeInstruction(OpCodes.Ldfld,    AccessTools.Field(typeof(StyleSettings), nameof(StyleSettings.hasStyle)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, instruction.operand);
                }
            }
        }

        public static Graphic HairGraphicHelper(string texPath, Shader shader, Vector2 size, Color color, Pawn pawn) => 
            GraphicDatabase.Get<Graphic_Multi>(texPath, CheckMaskShader(texPath, RegenerateResolveData(pawn).alienProps.alienRace.styleSettings[typeof(HairDef)].shader?.Shader ?? shader),
                                               size, color, pawnRenderResolveData.alienComp.GetChannel(channel: "hair").second);

        public static IEnumerable<CodeInstruction> TattooDefGraphicForTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            FieldInfo storyInfo    = AccessTools.Field(typeof(Pawn),              nameof(Pawn.story));
            FieldInfo headTypeInfo = AccessTools.Field(typeof(HeadTypeDef), nameof(HeadTypeDef.graphicPath));
            FieldInfo bodyTypeInfo = AccessTools.Field(typeof(BodyTypeDef), nameof(BodyTypeDef.bodyNakedGraphicPath));

            LocalBuilder styleLocal = ilg.DeclareLocal(typeof(StyleSettings));
            LocalBuilder colorLocal = ilg.DeclareLocal(typeof(AlienPartGenerator.ExposableValueTuple<Color, Color>));

            bool conditionJumpDone = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!conditionJumpDone && instruction.opcode == OpCodes.Brfalse_S)
                {
                    conditionJumpDone = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call,     AccessTools.Method(patchType, nameof(RegenerateResolveData)));
                    yield return new CodeInstruction(OpCodes.Ldfld,    AccessTools.Field(typeof(PawnRenderResolveData),            nameof(PawnRenderResolveData.alienProps)));
                    yield return new CodeInstruction(OpCodes.Ldfld,    AccessTools.Field(typeof(ThingDef_AlienRace),               nameof(ThingDef_AlienRace.alienRace)));
                    yield return new CodeInstruction(OpCodes.Ldfld,    AccessTools.Field(typeof(ThingDef_AlienRace.AlienSettings), nameof(ThingDef_AlienRace.AlienSettings.styleSettings)));
                    yield return new CodeInstruction(OpCodes.Ldtoken,  typeof(TattooDef));
                    yield return new CodeInstruction(OpCodes.Call,     AccessTools.Method(typeof(Type),                            nameof(Type.GetTypeFromHandle)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Dictionary<Type, StyleSettings>), "get_Item"));
                    yield return new CodeInstruction(OpCodes.Stloc,    styleLocal.LocalIndex);
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc,    styleLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldfld,    AccessTools.Field(typeof(StyleSettings), nameof(StyleSettings.hasStyle)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, instruction.operand);
                    }
                else if (instruction.opcode == OpCodes.Ldarg_2)
                {
                    i++;
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(patchType,                     nameof(pawnRenderResolveData)));
                    yield return new CodeInstruction(OpCodes.Ldfld,  AccessTools.Field(typeof(PawnRenderResolveData), nameof(PawnRenderResolveData.alienComp)));
                    yield return new CodeInstruction(OpCodes.Ldstr,  "tattoo");
                    yield return new CodeInstruction(OpCodes.Call,   AccessTools.Method(typeof(AlienPartGenerator.AlienComp), nameof(AlienPartGenerator.AlienComp.GetChannel)));
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stloc, colorLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AlienPartGenerator.ExposableValueTuple<Color, Color>), nameof(AlienPartGenerator.ExposableValueTuple<Color,Color>.first)));
                    yield return new CodeInstruction(OpCodes.Ldloc, colorLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AlienPartGenerator.ExposableValueTuple<Color, Color>), nameof(AlienPartGenerator.ExposableValueTuple<Color, Color>.second)));
                }
                else
                {
                    yield return instruction;
                }

                if (instructionList[i].LoadsField(headTypeInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(TattooPathHelper)));
                }
                else if (instructionList[i].LoadsField(bodyTypeInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(TattooPathHelper)));
                }

                if (instruction.opcode == OpCodes.Ldsfld && (instruction.operand as FieldInfo)?.FieldType == typeof(Shader))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc, styleLocal.LocalIndex).MoveLabelsFrom(instructionList[i+1]);
                    yield return CodeInstruction.Call(patchType, nameof(TattooShaderHelper));
                }
            }
        }

        public static Shader TattooShaderHelper(Shader shader, StyleSettings style) =>
            style.shader?.Shader ?? shader;

        public static string TattooPathHelper(string path, Pawn pawn, bool body) =>
            (body ? 
                pawn.Drawer.renderer.BodyGraphic?.path :
                pawn.Drawer.renderer.HeadGraphic?.path) ?? path;

        public static IEnumerable<CodeInstruction> BeardDefGraphicForTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Call && instructionList[i + 1].opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.Call(patchType, nameof(BeardGraphicHelper));
                }
                else
                {
                    yield return instruction;
                }

                if (instruction.opcode == OpCodes.Brfalse_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(patchType, nameof(pawnRenderResolveData)));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderResolveData), nameof(PawnRenderResolveData.alienProps)));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef_AlienRace), nameof(ThingDef_AlienRace.alienRace)));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef_AlienRace.AlienSettings), nameof(ThingDef_AlienRace.AlienSettings.styleSettings)));
                    yield return new CodeInstruction(OpCodes.Ldtoken, typeof(BeardDef));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Dictionary<Type, StyleSettings>), "get_Item"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StyleSettings), nameof(StyleSettings.hasStyle)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, instruction.operand);
                }
            }
        }

        public static Graphic BeardGraphicHelper(string texPath, Shader shader, Vector2 size, Color color, Pawn pawn) =>
            GraphicDatabase.Get<Graphic_Multi>(texPath, CheckMaskShader(texPath, RegenerateResolveData(pawn).alienProps.alienRace.styleSettings[typeof(BeardDef)].shader?.Shader ?? shader),
                                               size, color, pawnRenderResolveData.alienComp.GetChannel(channel: "hair").second);
        #endregion

        public static void SetupDynamicNodesPostfix(PawnRenderTree __instance)
        {
            if (__instance.pawn.RaceProps.Humanlike)
            {
                Log.Message($"Setup Addons: {__instance.pawn.NameFullColored}");

                PawnRenderResolveData pawnRenderData = RegenerateResolveData(__instance.pawn);


                AlienPartGenerator.AlienComp alienComp = pawnRenderData.alienComp;

                alienComp.addonGraphics =   [];
                alienComp.addonVariants ??= [];
                alienComp.addonColors   ??= [];

                int sharedIndex = 0;

                using (IEnumerator<AlienPartGenerator.BodyAddon> bodyAddons = pawnRenderData.alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons.Concat(Utilities.UniversalBodyAddons).GetEnumerator())
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

                        Graphic g = addon.GetGraphic(__instance.pawn, ref sharedIndex, alienComp.addonVariants.Count > addonIndex ? alienComp.addonVariants[addonIndex] : null);
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
                        PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(nodeProps.nodeClass, __instance.pawn, nodeProps, __instance.pawn.Drawer.renderer.renderTree);
                        CachedData.renderTreeAddChild(__instance.pawn.Drawer.renderer.renderTree, pawnRenderNode, null);
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

        public static Pair<WeakReference, bool> portraitRender = new(new WeakReference(new Pawn()), false);
        
        public static void GetHumanlikeHeadSetForPawnPrefix(Pawn pawn, ref float wFactor, ref float hFactor)
        {
            Vector2 drawSize = (portraitRender.First?.Target as Pawn == pawn && portraitRender.Second ?
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitHeadDrawSize :
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customHeadDrawSize) ?? Vector2.one;

            wFactor *= drawSize.x;
            hFactor *= drawSize.y;
        }

        public static void GetHumanlikeBodySetForPawnPrefix(Pawn pawn, ref float wFactor, ref float hFactor)
        {
            Vector2 drawSize = (portraitRender.First?.Target as Pawn == pawn && portraitRender.Second ?
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitDrawSize :
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customDrawSize) ?? Vector2.one;

            wFactor *= drawSize.x;
            hFactor *= drawSize.y;
        }
        #endregion
    }

    [UsedImplicitly]
    public class AlienPawnRenderNode_Swaddle(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : PawnRenderNode_Swaddle(pawn, props, tree)
    {
        public override Graphic GraphicFor(Pawn pawn)
        {
            AlienRenderTreePatches.PawnRenderResolveData pawnRenderData = AlienRenderTreePatches.pawnRenderResolveData;
            return GraphicDatabase.Get<Graphic_Multi>(pawnRenderData.alienProps.alienRace.graphicPaths.swaddle.GetPath(pawn, ref pawnRenderData.sharedIndex, pawn.HashOffset()), this.ShaderFor(pawn), Vector2.one, this.ColorFor(pawn));
        }
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

        public override GraphicMeshSet MeshSetFor(Pawn pawn) => MeshPool.GetMeshSetForSize(Vector2.one);
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

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            AlienPawnRenderNodeProperties_BodyAddon props = PropsFromNode(node);
            AlienPartGenerator.BodyAddon            ba    = props.addon;
            Vector2 scale = (parms.Portrait && ba.drawSizePortrait != Vector2.zero ? ba.drawSizePortrait : ba.drawSize) *
                            (ba.scaleWithPawnDrawsize ?
                                 (ba.alignWithHead ?
                                      parms.Portrait ?
                                          props.alienComp.customPortraitHeadDrawSize :
                                          props.alienComp.customHeadDrawSize :
                                      parms.Portrait ?
                                          props.alienComp.customPortraitDrawSize :
                                          props.alienComp.customDrawSize) *
                                 (ModsConfig.BiotechActive ? parms.pawn.ageTracker.CurLifeStage.bodyWidth ?? 1.5f : 1.5f) :
                                 Vector2.one * 1.5f);


            return new Vector3(scale.x, 1f, scale.y);
        }
    }
}
