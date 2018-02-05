using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace AlienRace
{
    /// <summary>
    /// "More useful than the Harmony wiki" ~ Mehni
    /// </summary>

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.erdelf.alien_race.main");

            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Child), nameof(PawnRelationWorker_Child.GenerationChance)), null, new HarmonyMethod(patchType, nameof(GenerationChanceChildPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExLover), nameof(PawnRelationWorker_ExLover.GenerationChance)), null, new HarmonyMethod(patchType, nameof(GenerationChanceExLoverPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExSpouse), nameof(PawnRelationWorker_ExSpouse.GenerationChance)), null, new HarmonyMethod(patchType, nameof(GenerationChanceExSpousePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Fiance), nameof(PawnRelationWorker_Spouse.GenerationChance)), null, new HarmonyMethod(patchType, nameof(GenerationChanceFiancePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Lover.GenerationChance)), null, new HarmonyMethod(patchType, nameof(GenerationChanceLoverPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Parent), nameof(PawnRelationWorker_Parent.GenerationChance)), null, new HarmonyMethod(patchType, nameof(GenerationChanceParentPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Sibling), nameof(PawnRelationWorker_Sibling.GenerationChance)), null, new HarmonyMethod(patchType, nameof(GenerationChanceSiblingPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Spouse), nameof(PawnRelationWorker_Spouse.GenerationChance)), null, new HarmonyMethod(patchType, nameof(GenerationChanceSpousePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GeneratePawnRelations"), new HarmonyMethod(patchType, nameof(GeneratePawnRelationsPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnRelationDef), nameof(PawnRelationDef.GetGenderSpecificLabel)), new HarmonyMethod(patchType, nameof(GetGenderSpecificLabelPrefix)), null);

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "TryGetRandomUnusedSolidBioFor"), null, new HarmonyMethod(patchType, nameof(TryGetRandomUnusedSolidBioForPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "FillBackstorySlotShuffled"), new HarmonyMethod(patchType, nameof(FillBackstoryInSlotShuffledPrefix)), null);



            harmony.Patch(AccessTools.Method(typeof(WorkGiver_Researcher), nameof(WorkGiver_Researcher.ShouldSkip)), null, new HarmonyMethod(patchType, nameof(ShouldSkipResearchPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), "ViewSize"), null, null, new HarmonyMethod(patchType, nameof(ResearchScreenTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), "DrawRightRect"), null, null, new HarmonyMethod(patchType, nameof(ResearchScreenTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.CanConstruct)), null, new HarmonyMethod(patchType, nameof(CanConstructPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GameRules), nameof(GameRules.DesignatorAllowed)), null, new HarmonyMethod(patchType, nameof(DesignatorAllowedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew)), null, new HarmonyMethod(patchType, nameof(PawnAllowedToStartAnewPostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GrowerHarvest), nameof(WorkGiver_GrowerHarvest.HasJobOnCell)), null, new HarmonyMethod(patchType, nameof(HasJobOnCellHarvestPostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GrowerSow), "ExtraRequirements"), null, new HarmonyMethod(patchType, nameof(ExtraRequirementsGrowerSowPostfix)));
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null, new HarmonyMethod(patchType, nameof(AddHumanlikeOrdersPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.SetFaction)), null, new HarmonyMethod(patchType, nameof(SetFactionPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Pawn.SetFactionDirect)), null, new HarmonyMethod(patchType, nameof(SetFactionDirectPostfix)));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreGain)), null, new HarmonyMethod(patchType, nameof(ApparelScoreGainPostFix)));

            DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.ForEach(ar =>
            {
                ThingCategoryDefOf.CorpsesHumanlike.childThingDefs.Remove(ar.race.corpseDef);
                ar.race.corpseDef.thingCategories = new List<ThingCategoryDef>() { AlienDefOf.alienCorpseCategory };
                AlienDefOf.alienCorpseCategory.childThingDefs.Add(ar.race.corpseDef);
                ar.alienRace.generalSettings.alienPartGenerator.GenerateMeshsAndMeshPools();

                if (ar.alienRace.generalSettings.humanRecipeImport)
                {
                    (ar.recipes ?? (ar.recipes = new List<RecipeDef>())).AddRange(ThingDefOf.Human.recipes.Where(rd => !rd.targetsBodyPart || (rd.appliedOnFixedBodyParts?.Any(bpd => ar.race.body.AllParts.Any(bpr => bpr.def == bpd)) ?? false)));

                    DefDatabase<RecipeDef>.AllDefsListForReading.ForEach(rd =>
                    {
                        if (rd.recipeUsers?.Contains(ThingDefOf.Human) ?? false)
                            rd.recipeUsers.Add(ar);
                    });
                    ar.recipes.RemoveDuplicates();
                }

                ar.alienRace.raceRestriction?.workGiverList?.ForEach(wgd =>
                {
                    WorkGiverDef wg = DefDatabase<WorkGiverDef>.GetNamedSilentFail(wgd);
                    if (wg != null)
                    {
                        harmony.Patch(AccessTools.Method(wg.giverClass, "JobOnThing"), null, new HarmonyMethod(patchType, nameof(GenericJobOnThingPostfix)));
                        MethodInfo hasJobOnThingInfo = AccessTools.Method(wg.giverClass, "HasJobOnThing");
                        if (hasJobOnThingInfo != null)
                            harmony.Patch(hasJobOnThingInfo, null, new HarmonyMethod(patchType, nameof(GenericHasJobOnThingPostfix)));
                    }
                });
            });
            harmony.Patch(AccessTools.Method(typeof(ThoughtUtility), nameof(ThoughtUtility.CanGetThought)), null, new HarmonyMethod(patchType, nameof(CanGetThoughtPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Corpse), nameof(Corpse.ButcherProducts)), new HarmonyMethod(patchType, nameof(ButcherProductsPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting)), null, new HarmonyMethod(patchType, nameof(ThoughtsFromIngestingPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(Thought_Memory), typeof(Pawn) }), new HarmonyMethod(patchType, nameof(TryGainMemoryThoughtPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(SituationalThoughtHandler), "TryCreateThought"), new HarmonyMethod(patchType, nameof(TryCreateSituationalThoughtPrefix)), null);

            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("AgeInjuryUtility"), "GenerateRandomOldAgeInjuries"), new HarmonyMethod(patchType, nameof(GenerateRandomOldAgeInjuriesPrefix)), null);
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("AgeInjuryUtility"), "RandomHediffsToGainOnBirthday", new Type[] { typeof(ThingDef), typeof(int) }), null, new HarmonyMethod(patchType, nameof(RandomHediffsToGainOnBirthdayPostfix)));
            harmony.Patch(AccessTools.Property(typeof(JobDriver), nameof(JobDriver.Posture)).GetGetMethod(false), null, new HarmonyMethod(patchType, nameof(PosturePostfix)));
            harmony.Patch(AccessTools.Property(typeof(JobDriver_Skygaze), nameof(JobDriver_Skygaze.Posture)).GetGetMethod(false), null, new HarmonyMethod(patchType, nameof(PosturePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateRandomAge"), new HarmonyMethod(patchType, nameof(GenerateRandomAgePrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateTraits"), new HarmonyMethod(patchType, nameof(GenerateTraitsPrefix)), null, new HarmonyMethod(patchType, nameof(GenerateTraitsTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_SatisfyChemicalNeed), "DrugValidator"), null, new HarmonyMethod(patchType, nameof(DrugValidatorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(CompDrug), nameof(CompDrug.PostIngested)), null, new HarmonyMethod(patchType, nameof(PostIngestedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(AddictionUtility), nameof(AddictionUtility.CanBingeOnNow)), null, new HarmonyMethod(patchType, nameof(CanBingeNowPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateBodyType"), null, new HarmonyMethod(patchType, nameof(GenerateBodyTypePostfix)));
            harmony.Patch(AccessTools.Property(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.SkinColor)).GetGetMethod(), null, new HarmonyMethod(patchType, nameof(SkinColorPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnHairChooser), nameof(PawnHairChooser.RandomHairDefFor)), new HarmonyMethod(patchType, nameof(RandomHairDefForPrefix)), null);
            //harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), "BirthdayBiological"), new HarmonyMethod(patchClass, nameof(BirthdayBiologicalPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) }), new HarmonyMethod(patchType, nameof(GeneratePawnPrefix)), null);

            harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)), new HarmonyMethod(patchType, nameof(ResolveAllGraphicsPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) }), null, null, new HarmonyMethod(patchType, nameof(RenderPawnInternalTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(StartingPawnUtility), nameof(StartingPawnUtility.NewGeneratedStartingPawn)), new HarmonyMethod(patchType, nameof(NewGeneratedStartingPawnPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo)), null, new HarmonyMethod(patchType, nameof(GiveAppropriateBioAndNameToPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName)), new HarmonyMethod(patchType, nameof(GeneratePawnNamePrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(Page_ConfigureStartingPawns), "CanDoNext"), null, new HarmonyMethod(patchType, nameof(CanDoNextStartPawnPostfix)));

            harmony.Patch(AccessTools.Method(typeof(GameInitData), nameof(GameInitData.PrepForMapGen)), new HarmonyMethod(patchType, nameof(PrepForMapGenPrefix)), null);


            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor)), null, null, new HarmonyMethod(patchType, nameof(SecondaryLovinChanceFactorTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.CompatibilityWith)), null, new HarmonyMethod(patchType, nameof(CompatibilityWith)));
            harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.TryMakeInitialRelationsWith)), null, new HarmonyMethod(patchType, nameof(TryMakeInitialRelationsWithPostfix)));
            harmony.Patch(AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait)), new HarmonyMethod(patchType, nameof(GainTraitPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(TraderCaravanUtility), nameof(TraderCaravanUtility.GetTraderCaravanRole)), null, null, new HarmonyMethod(patchType, nameof(GetTraderCaravanRoleTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(ITab_Pawn_Gear), "TryDrawAverageArmor"), null, null, new HarmonyMethod(patchType, nameof(TryDrawAverageArmorTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(RestUtility), nameof(RestUtility.CanUseBedEver)), null, new HarmonyMethod(patchType, nameof(CanUseBedEverPostfix)));
            harmony.Patch(AccessTools.Property(typeof(Building_Bed), nameof(Building_Bed.AssigningCandidates)).GetGetMethod(), null, new HarmonyMethod(patchType, nameof(AssigningCandidatesPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GenText), nameof(GenText.AdjustedFor)), null, new HarmonyMethod(patchType, nameof(GenTextAdjustedForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(RaceProperties), nameof(RaceProperties.CanEverEat), new Type[] { typeof(ThingDef) }), null, new HarmonyMethod(patchType, nameof(CanEverEat)));
            harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "DamageInfosToApply"), null, new HarmonyMethod(patchType, nameof(DamageInfosToApplyPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnWeaponGenerator), nameof(PawnWeaponGenerator.TryGenerateWeaponFor)), new HarmonyMethod(patchType, nameof(TryGenerateWeaponForPrefix)), new HarmonyMethod(patchType, nameof(TryGenerateWeaponForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.GenerateStartingApparelFor)), new HarmonyMethod(patchType, nameof(GenerateStartingApparelForPrefix)), new HarmonyMethod(patchType, nameof(GenerateStartingApparelForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateInitialHediffs"), null, new HarmonyMethod(patchType, nameof(GenerateInitialHediffsPostfix)));
            harmony.Patch(typeof(HediffSet).GetMethods(AccessTools.all).Where(mi => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.ReturnType == typeof(bool) && mi.GetParameters().First().ParameterType == typeof(BodyPartRecord)).First(), null, new HarmonyMethod(patchType, nameof(HasHeadPostfix)));
            harmony.Patch(AccessTools.Property(typeof(HediffSet), nameof(HediffSet.HasHead)).GetGetMethod(), new HarmonyMethod(patchType, nameof(HasHeadPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), "RecalculateLifeStageIndex"), null, new HarmonyMethod(patchType, nameof(RecalculateLifeStageIndexPostfix)));
            harmony.Patch(typeof(FactionGenerator).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic).MaxBy(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Count()).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray()?.Count() ?? -1), null, new HarmonyMethod(patchType, nameof(EnsureRequiredEnemiesPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.FactionTick)), null, null, new HarmonyMethod(patchType, nameof(FactionTickTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Designator_Build), nameof(Designator_Build.CanDesignateThing)), null, new HarmonyMethod(patchType, nameof(CanDesignateThingTamePostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_InteractAnimal), "CanInteractWithAnimal"), null, new HarmonyMethod(patchType, nameof(CanInteractWithAnimalPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt)), null, new HarmonyMethod(patchType, nameof(BaseHeadOffsetAtPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), "CheckForStateChange"), null, new HarmonyMethod(patchType, nameof(CheckForStateChangePostfix)));
            harmony.Patch(AccessTools.Method(typeof(ApparelProperties), nameof(ApparelProperties.GetInterferingBodyPartGroups)), null, null, new HarmonyMethod(patchType, nameof(GetInterferingBodyPartGroupsTranspiler)));
            //Log.Message("Alien race successfully completed " + harmony.GetPatchedMethods().Count() + " patches with harmony.");
            DefDatabase<HairDef>.GetNamed("Shaved").hairTags.Add("alienNoHair"); // needed because..... the original idea doesn't work and I spend enough time finding a good solution
        }

        //Zorba.....
        public static IEnumerable<CodeInstruction> GetInterferingBodyPartGroupsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool done = false;

            foreach (CodeInstruction instruction in instructions)
                if (!done && instruction.opcode == OpCodes.Call)
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(DefDatabase<BodyDef>), nameof(DefDatabase<BodyDef>.DefCount)).GetGetMethod());
                    done = true;
                }
                else
                    yield return instruction;
        }

        public static void CheckForStateChangePostfix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (Current.ProgramState == ProgramState.Playing && pawn.Spawned && pawn.def is ThingDef_AlienRace)
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public static void BaseHeadOffsetAtPostfix(PawnRenderer __instance, ref Vector3 __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            Vector2 offset = (pawn.def as ThingDef_AlienRace)?.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).headOffset ?? Vector2.zero;
            __result.x += offset.x;
            __result.z += offset.y;
        }

        public static void CanInteractWithAnimalPostfix(ref bool __result, Pawn pawn, Pawn animal) =>
            __result = __result ? (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.petList?.Contains(animal.def.defName) ?? false ? true : (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whitePetList?.Contains(animal.def.defName) ?? false ? true :
                    (((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyTameRaceRestrictedPets ?? false) ? false :
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.petList?.Contains(animal.def.defName) ?? false))) : false;

        public static void CanDesignateThingTamePostfix(ref bool __result, Thing t) =>
            __result = __result ? colonistRaces.OfType<ThingDef_AlienRace>().Any(td =>
                    td.alienRace.raceRestriction.petList.Contains(t.def.defName) ||
                    td.alienRace.raceRestriction.whitePetList.Contains(t.def.defName) ||
                    !td.alienRace.raceRestriction.onlyTameRaceRestrictedPets) ||
                !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(ar => ar.alienRace.raceRestriction.petList.Contains(t.def.defName)) : false;

        public static IEnumerable<CodeInstruction> FactionTickTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Beq)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(FactionTickFactionRelationCheck)));
                    yield return new CodeInstruction(OpCodes.Brfalse, instruction.operand);
                }
            }
        }

        private static bool FactionTickFactionRelationCheck(Faction f)
        {
            FactionDef player = Faction.OfPlayerSilentFail?.def ?? Find.GameInitData.playerFaction.def;
            return !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(ar =>
                    (f.def?.basicMemberKind?.race == ar &&
                        (ar.alienRace.generalSettings?.factionRelations?.Any(frs => frs.factions?.Contains(player.defName) ?? false) ?? false)) ||
                    (player.basicMemberKind?.race == ar &&
                        (ar.alienRace.generalSettings?.factionRelations?.Any(frs => frs.factions?.Contains(f.def.defName) ?? false) ?? false)));
        }

        public static void EnsureRequiredEnemiesPostfix(ref bool __result, Faction f) => __result = __result ||
                !FactionTickFactionRelationCheck(f);

        public static void RecalculateLifeStageIndexPostfix(Pawn_AgeTracker __instance)
        {
            Pawn pawn;
            if (Current.ProgramState == ProgramState.Playing && (pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>()).def is ThingDef_AlienRace && pawn.Drawer.renderer.graphics.AllResolved)
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public static void HasHeadPrefix(HediffSet __instance) =>
            headPawnDef = (__instance.pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.headBodyPartDef;

        static BodyPartDef headPawnDef;

        public static void HasHeadPostfix(BodyPartRecord x, bool __result) =>
            __result = headPawnDef != null ? x.def == headPawnDef : __result;

        public static void GenerateInitialHediffsPostfix(Pawn pawn) =>
            pawn.story?.AllBackstories?.Select(bs => DefDatabase<BackstoryDef>.GetNamedSilentFail(bs.identifier)).OfType<BackstoryDef>().SelectMany(bd => bd.forcedHediffs).Concat(bioReference?.forcedHediffs ?? new List<string>(0)).Select(s =>
                DefDatabase<HediffDef>.GetNamedSilentFail(s)).ToList().ForEach(hd =>
                {
                    BodyPartRecord bodyPartRecord = null;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(rd => rd.addsHediff == hd)?.appliedOnFixedBodyParts.SelectMany(bpd => pawn.health.hediffSet.GetNotMissingParts().Where(bpr => bpr.def == bpd && !pawn.health.hediffSet.hediffs.Any(h => h.def == hd && h.Part == bpr))).TryRandomElement(out bodyPartRecord);
                    pawn.health.AddHediff(hd, bodyPartRecord);
                });

        public static void GenerateStartingApparelForPostfix() => Traverse.Create(typeof(PawnApparelGenerator)).Field("allApparelPairs").GetValue<List<ThingStuffPair>>().AddRange(apparelList);

        static HashSet<ThingStuffPair> apparelList;

        public static void GenerateStartingApparelForPrefix(Pawn pawn)
        {
            ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;

            Traverse apparelInfo = Traverse.Create(typeof(PawnApparelGenerator)).Field("allApparelPairs");

            apparelList = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in apparelInfo.GetValue<List<ThingStuffPair>>().ListFullCopy())
            {
                ThingDef equipment = pair.thing;
                if (!((alienProps?.alienRace.raceRestriction.apparelList?.Contains(equipment.defName) ?? false) ||
                        (alienProps?.alienRace.raceRestriction.whiteApparelList?.Contains(equipment.defName) ?? false) ||
                        (((!alienProps?.alienRace.raceRestriction.onlyUseRaceRestrictedApparel) ?? true) && !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d =>
                        pawn.def != d && (d.alienRace.raceRestriction.apparelList?.Contains(equipment.defName) ?? false)))))
                    apparelList.Add(pair);
            }
            apparelInfo.GetValue<List<ThingStuffPair>>().RemoveAll(tsp => apparelList.Contains(tsp));
        }

        public static void TryGenerateWeaponForPostfix() =>
            Traverse.Create(typeof(PawnWeaponGenerator)).Field("allWeaponPairs").GetValue<List<ThingStuffPair>>().AddRange(weaponList);

        static HashSet<ThingStuffPair> weaponList;

        public static void TryGenerateWeaponForPrefix(Pawn pawn)
        {
            ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;

            Traverse weaponInfo = Traverse.Create(typeof(PawnWeaponGenerator)).Field("allWeaponPairs");
            weaponList = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in weaponInfo.GetValue<List<ThingStuffPair>>().ListFullCopy())
            {
                ThingDef equipment = pair.thing;
                if (!((alienProps?.alienRace.raceRestriction.weaponList?.Contains(equipment.defName) ?? false) ||
                        (alienProps?.alienRace.raceRestriction.whiteWeaponList?.Contains(equipment.defName) ?? false) ||
                        (((!alienProps?.alienRace.raceRestriction.onlyUseRaceRestrictedWeapons) ?? true) && !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d =>
                        pawn.def != d && (d.alienRace.raceRestriction.weaponList?.Contains(equipment.defName) ?? false)))))
                    weaponList.Add(pair);
            }
            weaponInfo.GetValue<List<ThingStuffPair>>().RemoveAll(tsp => weaponList.Contains(tsp));
        }

        public static void DamageInfosToApplyPostfix(Verb __instance, ref IEnumerable<DamageInfo> __result)
        {
            if (__instance.CasterIsPawn && __instance.CasterPawn.def is ThingDef_AlienRace alienProps && __instance.CasterPawn.CurJob.def == JobDefOf.SocialFight)
                __result = __result.Select(di =>
                    new DamageInfo(di.Def, Math.Min(di.Amount, alienProps.alienRace.generalSettings.maxDamageForSocialfight), di.Angle, di.Instigator, di.HitPart, di.Weapon, di.Category));
        }

        public static void CanEverEat(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (__instance.Humanlike)
            {

                ThingDef eater = new List<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading).Concat(
                    new List<ThingDef_AlienRace>(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading).Cast<ThingDef>()).First(td => td.race == __instance);

                __result = (eater as ThingDef_AlienRace)?.alienRace.raceRestriction.foodList?.Contains(t.defName) ?? false ? true : (eater as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteFoodList?.Contains(t.defName) ?? false ?
                        true : ((eater as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyEatRaceRestrictedFood ?? false) ? false :
                        DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => eater != d && (d.alienRace.raceRestriction.foodList?.Contains(t.defName) ?? false)) ? false : __result;
            }
        }

        public static void GenTextAdjustedForPostfix(ref string __result, Pawn p) => __result.Replace("ALIENRACE", p.def.LabelCap);

        public static IEnumerable<CodeInstruction> GenerateTraitsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            MethodInfo defListInfo = AccessTools.Property(typeof(DefDatabase<TraitDef>), nameof(DefDatabase<TraitDef>.AllDefsListForReading)).GetGetMethod();
            MethodInfo validatorInfo = AccessTools.Method(patchType, nameof(GenerateTraitsValidator));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Call && instruction.operand == defListInfo)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    instruction.operand = validatorInfo;

                    for (int x = 0; x < 4; x++)
                        instructionList.RemoveAt(i + 1);
                }

                yield return instruction;
            }
        }

        public static TraitDef GenerateTraitsValidator(Pawn p)
        {
            IEnumerable<TraitDef> defs = DefDatabase<TraitDef>.AllDefs;
            ThingDef_AlienRace alienProps = p.def as ThingDef_AlienRace;
            defs = defs.Where(tr => (p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.traitList?.Contains(tr.defName) ?? false ? true : (p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteTraitList?.Contains(tr.defName) ?? false ? true :
                    ((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyGetRaceRestrictedTraits ?? false ? false :
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => p.def != d && (d.alienRace.raceRestriction.traitList?.Contains(tr.defName) ?? false))));

            return defs.RandomElementByWeight(tr => tr.GetGenderSpecificCommonality(p));
        }

        public static void AssigningCandidatesPostfix(ref IEnumerable<Pawn> __result, Building_Bed __instance) =>
            __result = __result.Where(p => RestUtility.CanUseBedEver(p, __instance.def));

        public static void CanUseBedEverPostfix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            if (__result)
            {
                __result = (p.def is ThingDef_AlienRace alienProps && (alienProps.alienRace.generalSettings.validBeds?.Contains(bedDef.defName) ?? false)) ||
                    !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(td => td.alienRace.generalSettings.validBeds?.Contains(bedDef.defName) ?? false);
            }
        }

        public static IEnumerable<CodeInstruction> TryDrawAverageArmorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo bodyInfo = AccessTools.Field(typeof(RaceProperties), nameof(RaceProperties.body));
            MethodInfo humanlikeInfo = AccessTools.Property(typeof(RaceProperties), nameof(RaceProperties.Humanlike)).GetGetMethod();

            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.operand == bodyInfo)
                {
                    instructionList.RemoveAt(i + 1);
                    object lab = instructionList[i + 1].operand;
                    instructionList.RemoveAt(i + 1);

                    yield return new CodeInstruction(OpCodes.Callvirt, humanlikeInfo);
                    instruction.opcode = OpCodes.Brtrue_S;
                    instruction.operand = lab;
                }

                yield return instruction;
            }
        }

        public static void CanWearTogetherPostfix(ThingDef A, ThingDef B, bool __result)
        {
            /*
            if (__result)
            {
                Log.Message(A.defName + " - " + B.defName);

                bool flag = false;
                for (int i = 0; i < A.apparel.layers.Count; i++)
                {
                    for (int j = 0; j < B.apparel.layers.Count; j++)
                    {
                        if (A.apparel.layers[i] == B.apparel.layers[j])
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                }
                if (!flag)
                    Log.Message("You are out");
                else
                {
                    for (int k = 0; k < A.apparel.bodyPartGroups.Count; k++)
                    {
                        for (int l = 0; l < B.apparel.bodyPartGroups.Count; l++)
                        {
                            BodyPartGroupDef item = A.apparel.bodyPartGroups[k];
                            BodyPartGroupDef item2 = B.apparel.bodyPartGroups[l];
                            for (int m = 0; m < BodyDefOf.Human.AllParts.Count; m++)
                            {
                                BodyPartRecord bodyPartRecord = BodyDefOf.Human.AllParts[m];
                                if (bodyPartRecord.groups.Contains(item) && bodyPartRecord.groups.Contains(item2))
                                {
                                    Log.Message("you are in");
                                }
                            }
                        }
                    }
                }
            }*/
        }

        public static IEnumerable<CodeInstruction> GetTraderCaravanRoleTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            MethodInfo traderRoleInfo = AccessTools.Method(patchType, nameof(GetTraderCaravanRoleInfix));

            List<CodeInstruction> instructionList = instructions.ToList();
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_3)
                {
                    Label jumpToEnd = il.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = instruction.labels.ListFullCopy() };
                    instruction.labels.Clear();
                    yield return new CodeInstruction(OpCodes.Call, traderRoleInfo);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, jumpToEnd);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_4);
                    yield return new CodeInstruction(OpCodes.Ret);
                    yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label>() { jumpToEnd } };
                }
                yield return instruction;
            }
        }

        private static bool GetTraderCaravanRoleInfix(Pawn p)
        {
            //Log.Message(p.Name?.ToStringFull ?? p.ToString());
            if (p.def is ThingDef_AlienRace)
            {
                if ((p.def as ThingDef_AlienRace).alienRace.pawnKindSettings.alienslavekinds?.Any(pke => pke.kindDefs?.Contains(p.kindDef.defName) ?? false) ?? false)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GetGenderSpecificLabelPrefix(Pawn pawn, ref string __result, PawnRelationDef __instance)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                RelationRenamer ren = alienProps.alienRace.relationSettings.renamer?.FirstOrDefault(rn => rn.relation.EqualsIgnoreCase(__instance.defName));
                if (ren != null)
                {
                    __result = pawn.gender == Gender.Female ? ren.femaleLabel : ren.label;
                    if (__result.CanTranslate())
                        __result = __result.Translate();
                    return false;
                }
            }
            return true;
        }

        public static bool GeneratePawnRelationsPrefix(Pawn pawn, ref PawnGenerationRequest request)
        {
            PawnGenerationRequest localReq = request;
            ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;

            if (!pawn.RaceProps.Humanlike || pawn.RaceProps.hasGenders || alienProps == null)
            {
                return true;
            }

            List<KeyValuePair<Pawn, PawnRelationDef>> list = new List<KeyValuePair<Pawn, PawnRelationDef>>();
            List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
            List<Pawn> enumerable = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                                     where x.def == pawn.def
                                     select x).ToList();

            RelationSettings relations = alienProps.alienRace.relationSettings;

            enumerable.ForEach(current =>
            {
                if (current.Discarded)
                {
                    Log.Warning(string.Concat(new object[]
                    {
                        "Warning during generating pawn relations for ",
                        pawn,
                        ": Pawn ",
                        current,
                        " is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything."
                            }));
                }
                else
                {
                    allDefsListForReading.ForEach(relationDef =>
                    {
                        if (relationDef.generationChanceFactor > 0f)
                        {
                            list.Add(new KeyValuePair<Pawn, PawnRelationDef>(current, relationDef));
                        }
                    });
                }
            });

            KeyValuePair<Pawn, PawnRelationDef> keyValuePair = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<Pawn, PawnRelationDef> x)
            {
                if (!x.Value.familyByBloodRelation)
                {
                    return 0f;
                }
                return GenerationChanceGenderless(x.Value, pawn, x.Key, localReq);
            }, 82f);

            Pawn other = keyValuePair.Key;
            if (other != null)
            {
                CreateRelationGenderless(keyValuePair.Value, pawn, other);
            }
            KeyValuePair<Pawn, PawnRelationDef> keyValuePair2 = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<Pawn, PawnRelationDef> x)
            {
                if (x.Value.familyByBloodRelation)
                {
                    return 0f;
                }
                return GenerationChanceGenderless(x.Value, pawn, x.Key, localReq);
            }, 82f);
            other = keyValuePair2.Key;
            if (other != null)
            {
                CreateRelationGenderless(keyValuePair2.Value, pawn, other);
            }
            return false;
        }

        private static float GenerationChanceGenderless(PawnRelationDef relationDef, Pawn pawn, Pawn current, PawnGenerationRequest request)
        {
            float generationChance = relationDef.generationChanceFactor;
            float lifeExpectancy = pawn.RaceProps.lifeExpectancy;

            if (relationDef == PawnRelationDefOf.Child)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(current, pawn, current.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, p => p != pawn));
                HarmonyPatches.GenerationChanceChildPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.ExLover)
            {
                generationChance = 0.5f;
                HarmonyPatches.GenerationChanceExLoverPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.ExSpouse)
            {
                generationChance = 0.5f;
                HarmonyPatches.GenerationChanceExSpousePostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Fiance)
            {
                generationChance =
                Mathf.Clamp(GenMath.LerpDouble(lifeExpectancy / 1.6f, lifeExpectancy, 1f, 0.01f, pawn.ageTracker.AgeBiologicalYearsFloat), 0.01f, 1f) *
                Mathf.Clamp(GenMath.LerpDouble(lifeExpectancy / 1.6f, lifeExpectancy, 1f, 0.01f, current.ageTracker.AgeBiologicalYearsFloat), 0.01f, 1f);
                HarmonyPatches.GenerationChanceFiancePostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Lover)
            {
                generationChance = 0.5f;
                HarmonyPatches.GenerationChanceLoverPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Parent)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(current, pawn, current.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, p => p != pawn));
                HarmonyPatches.GenerationChanceParentPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Sibling)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(current, pawn, current.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, p => p != pawn));
                generationChance *= 0.65f;
                HarmonyPatches.GenerationChanceSiblingPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Spouse)
            {
                generationChance = 0.5f;
                HarmonyPatches.GenerationChanceSpousePostfix(ref generationChance, pawn, current);
            }

            return generationChance *= relationDef.Worker.BaseGenerationChanceFactor(pawn, current, request);
        }

        private static void CreateRelationGenderless(PawnRelationDef relationDef, Pawn pawn, Pawn other)
        {
            if (relationDef == PawnRelationDefOf.Child)
            {
                Pawn parent = other.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent);
                if (parent != null)
                {
                    pawn.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || Rand.Value > 0.8f ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Spouse, parent);
                }

                other.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
            }

            if (relationDef == PawnRelationDefOf.ExLover)
            {
                if (!pawn.GetRelations(other).Contains(PawnRelationDefOf.ExLover))
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, other);
                }

                other.relations.Children.ToList().ForEach(p =>
                {
                    if (p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && Rand.Value < 0.35)
                    {
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                    }
                });
            }

            if (relationDef == PawnRelationDefOf.ExSpouse)
            {
                pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, other);

                other.relations.Children.ToList().ForEach(p =>
                {
                    if (p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && Rand.Value < 1)
                    {
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                    }
                });
            }

            if (relationDef == PawnRelationDefOf.Fiance)
            {
                pawn.relations.AddDirectRelation(PawnRelationDefOf.Fiance, other);

                other.relations.Children.ToList().ForEach(p =>
                {
                    if (p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && Rand.Value < 0.7)
                    {
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                    }
                });
            }

            if (relationDef == PawnRelationDefOf.Lover)
            {
                pawn.relations.AddDirectRelation(PawnRelationDefOf.Lover, other);

                other.relations.Children.ToList().ForEach(p =>
                {
                    if (p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && Rand.Value < 0.35f)
                    {
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                    }
                });
            }

            if (relationDef == PawnRelationDefOf.Parent)
            {
                Pawn parent = other.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent);
                if (parent != null && pawn != parent && !pawn.GetRelations(parent).Contains(PawnRelationDefOf.ExLover))
                {
                    pawn.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || Rand.Value > 0.8f ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Spouse, parent);
                }

                pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, other);
            }

            if (relationDef == PawnRelationDefOf.Sibling)
            {
                Pawn parent = other.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, null);
                List<DirectPawnRelation> dprs = other.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent && dpr.otherPawn != parent).ToList();
                Pawn parent2 = dprs.NullOrEmpty() ? null : dprs.First().otherPawn;

                if (parent == null)
                {
                    parent = PawnGenerator.GeneratePawn(other.kindDef, Find.FactionManager.FirstFactionOfDef(other.kindDef.defaultFactionType) ?? Find.FactionManager.AllFactions.RandomElement());
                    if (!other.GetRelations(parent).Contains(PawnRelationDefOf.Parent))
                    {
                        other.relations.AddDirectRelation(PawnRelationDefOf.Parent, parent);
                    }
                }

                if (parent2 == null)
                {
                    parent2 = PawnGenerator.GeneratePawn(other.kindDef, Find.FactionManager.FirstFactionOfDef(other.kindDef.defaultFactionType) ?? Find.FactionManager.AllFactions.RandomElement());
                    if (!other.GetRelations(parent2).Contains(PawnRelationDefOf.Parent))
                    {
                        other.relations.AddDirectRelation(PawnRelationDefOf.Parent, parent2);
                    }
                }

                if (!parent.GetRelations(parent2).Any(prd => prd == PawnRelationDefOf.ExLover || prd == PawnRelationDefOf.Lover))
                {
                    parent.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || Rand.Value > 0.8 ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Lover, parent2);
                }

                if (!pawn.GetRelations(parent).Contains(PawnRelationDefOf.Parent) && pawn != parent)
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, parent);
                }

                if (!pawn.GetRelations(parent2).Contains(PawnRelationDefOf.Parent) && pawn != parent2)
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, parent2);
                }
            }

            if (relationDef == PawnRelationDefOf.Spouse)
            {
                if (!pawn.GetRelations(other).Contains(PawnRelationDefOf.Spouse))
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Spouse, other);
                }

                other.relations.Children.ToList().ForEach(p =>
                {
                    if (pawn != p && p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, x => x == pawn) == null && Rand.Value < 0.7)
                    {
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                    }
                });
            }

        }

        private static float ChanceOfBecomingGenderlessChildOf(Pawn child, Pawn parent1, Pawn parent2)
        {
            if (child == null || parent1 == null || !(parent2 == null || child.relations.DirectRelations.Count(dpr => dpr.def == PawnRelationDefOf.Parent) > 1))
                return 0f;
            if (parent1 != null && parent2 != null && !LovePartnerRelationUtility.LovePartnerRelationExists(parent1, parent2) && !LovePartnerRelationUtility.ExLovePartnerRelationExists(parent1, parent2))
                return 0f;

            float num = 1f;
            float num2 = 1f;
            float num3 = 1f;
            Traverse childRelation = Traverse.Create(typeof(ChildRelationUtility));

            if (parent1 != null)
            {
                num = childRelation.Method("GetParentAgeFactor", parent1, child, parent1.RaceProps.lifeExpectancy / 5f, parent1.RaceProps.lifeExpectancy / 2.5f, parent1.RaceProps.lifeExpectancy / 1.6f).GetValue<float>();
                if (num == 0f)
                {
                    return 0f;
                }
            }
            if (parent2 != null)
            {
                num2 = childRelation.Method("GetParentAgeFactor", parent2, child, parent1.RaceProps.lifeExpectancy / 5f, parent1.RaceProps.lifeExpectancy / 2.5f, parent1.RaceProps.lifeExpectancy / 1.6f).GetValue<float>();
                if (num2 == 0f)
                {
                    return 0f;
                }
                num3 = 1f;
            }
            float num6 = 1f;
            if (parent2 != null)
            {
                Pawn firstDirectRelationPawn = parent2.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
                if (firstDirectRelationPawn != null && firstDirectRelationPawn != parent2)
                {
                    num6 *= 0.15f;
                }
            }
            if (parent2 != null)
            {
                Pawn firstDirectRelationPawn2 = parent2.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
                if (firstDirectRelationPawn2 != null && firstDirectRelationPawn2 != parent2)
                {
                    num6 *= 0.15f;
                }
            }
            return num * num2 * num3 * num6;

        }

        public static bool GainTraitPrefix(Trait trait, TraitSet __instance)
        {
            if (Traverse.Create(__instance).Field("pawn").GetValue<Pawn>().def is ThingDef_AlienRace alienProps)
            {
                if (alienProps.alienRace.generalSettings.disallowedTraits?.Contains(trait.def.defName) ?? false)
                    return false;

                AlienTraitEntry ate = alienProps.alienRace.generalSettings.forcedRaceTraitEntries?.FirstOrDefault(at => at.defName.EqualsIgnoreCase(trait.def.defName));
                if (ate == null)
                {
                    return true;
                }

                return Rand.Range(0, 100) < ate.chance;
            }
            return true;
        }

        public static void TryMakeInitialRelationsWithPostfix(Faction __instance, Faction other)
        {
            if (other.def.basicMemberKind?.race is ThingDef_AlienRace alienProps)
            {
                alienProps.alienRace.generalSettings.factionRelations?.ForEach(frs =>
                {
                    if (frs.factions.Contains(__instance.def.defName))
                    {
                        FactionRelation fr;
                        float offset = frs.goodwill.RandomInRange;
                        (fr = other.RelationWith(__instance, false)).goodwill = frs.goodwill.RandomInRange;
                        fr.hostile = fr.goodwill < 0;
                        (fr = __instance.RelationWith(other, false)).goodwill = frs.goodwill.RandomInRange;
                        fr.hostile = fr.goodwill < 0;
                    }
                });
            }

            alienProps = __instance.def.basicMemberKind?.race as ThingDef_AlienRace;
            if (alienProps != null)
            {
                alienProps.alienRace.generalSettings.factionRelations?.ForEach(frs =>
                {
                    if (frs.factions.Contains(other.def.defName))
                    {
                        FactionRelation fr;
                        float offset = frs.goodwill.RandomInRange;
                        (fr = other.RelationWith(__instance, false)).goodwill = frs.goodwill.RandomInRange;
                        fr.hostile = fr.goodwill < 0;
                        (fr = __instance.RelationWith(other, false)).goodwill = frs.goodwill.RandomInRange;
                        fr.hostile = fr.goodwill < 0;
                    }
                });
            }
        }

        public static bool TryCreateSituationalThoughtPrefix(ref ThoughtDef def, SituationalThoughtHandler __instance)
        {
            Pawn pawn = __instance.pawn;
            if (DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(ar => !ar.alienRace.thoughtSettings.replacerList.NullOrEmpty()).SelectMany(ar => ar.alienRace.thoughtSettings.replacerList.Select(tr => tr.replacer)).Contains(def.defName))
            {
                return false;
            }

            if (pawn.def is ThingDef_AlienRace)
            {
                string name = def.defName;
                ThoughtReplacer replacer = (pawn.def as ThingDef_AlienRace)?.alienRace.thoughtSettings.replacerList?.FirstOrDefault(tr => name.EqualsIgnoreCase(tr.original));
                if (replacer != null)
                {
                    ThoughtDef replacerThoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(replacer.replacer);
                    if (replacerThoughtDef != null)
                    {
                        def = replacerThoughtDef;
                    }
                }
            }
            return !Traverse.Create(__instance).Field("tmpCachedThoughts").GetValue<HashSet<ThoughtDef>>().Contains(def);
        }

        public static void CanBingeNowPostfix(Pawn pawn, ChemicalDef chemical, ref bool __result)
        {
            if (__result)
            {
                if (pawn.def is ThingDef_AlienRace alienProps)
                {
                    bool result = __result;
                    alienProps.alienRace.generalSettings.chemicalSettings?.ForEach(cs =>
                    {
                        if (cs.chemical?.EqualsIgnoreCase(chemical.defName) ?? false && !cs.ingestible)
                        {
                            result = false;
                        }
                    }
                    );
                    __result = result;
                }
            }
        }

        public static void PostIngestedPostfix(Pawn ingester, CompDrug __instance)
        {
            if (ingester.def is ThingDef_AlienRace alienProps)
            {
                alienProps.alienRace.generalSettings.chemicalSettings?.ForEach(cs =>
                {
                    if (cs.chemical?.EqualsIgnoreCase(__instance.Props?.chemical?.defName) ?? false)
                    {
                        cs.reactions?.ForEach(iod => iod.DoIngestionOutcome(ingester, __instance.parent));
                    }
                });
            }
        }

        public static void DrugValidatorPostfix(ref bool __result, Pawn pawn, Thing drug) =>
            CanBingeNowPostfix(pawn, drug?.TryGetComp<CompDrug>()?.Props?.chemical, ref __result);

        public static void CompatibilityWith(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result)
        {
            Traverse traverse = Traverse.Create(__instance);
            Pawn pawn = traverse.Field("pawn").GetValue<Pawn>();

            if (pawn.RaceProps.Humanlike != otherPawn.RaceProps.Humanlike || pawn == otherPawn)
            {
                __result = 0f;
                return;
            }
            float x = Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
            float num = GenMath.LerpDouble(0f, 20f, 0.45f, -0.45f, x);
            num = Mathf.Clamp(num, -0.45f, 0.45f);
            float num2 = __instance.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);
            __result = num + num2;
        }

        public static IEnumerable<CodeInstruction> SecondaryLovinChanceFactorTranspiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo defField = AccessTools.Field(typeof(Pawn), nameof(Pawn.def));
            MethodInfo racePropsProperty = AccessTools.Property(typeof(Pawn), nameof(Pawn.RaceProps)).GetGetMethod();
            MethodInfo humanlikeProperty = AccessTools.Property(typeof(RaceProperties), nameof(RaceProperties.Humanlike)).GetGetMethod();
            int counter = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                counter++;
                if (counter < 10)
                {
                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == defField)
                    {
                        yield return new CodeInstruction(OpCodes.Callvirt, racePropsProperty);

                        instruction.opcode = OpCodes.Callvirt;
                        instruction.operand = humanlikeProperty;
                    }
                }
                yield return instruction;
            }
        }

        public static void GenericHasJobOnThingPostfix(WorkGiver __instance, Pawn pawn, ref bool __result)
        {
            if (__result)
            {
                __result = (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.workGiverList?.Any(wgd => DefDatabase<WorkGiverDef>.GetNamedSilentFail(wgd)?.giverClass == __instance.GetType()) ?? false ||
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.workGiverList?.Any(wgd => DefDatabase<WorkGiverDef>.GetNamedSilentFail(wgd)?.giverClass == __instance.GetType()) ?? false));
            }

        }

        public static void GenericJobOnThingPostfix(WorkGiver __instance, Pawn pawn, ref Job __result)
        {
            if (__result != null)
            {
                if (!((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.workGiverList?.Any(wgd => DefDatabase<WorkGiverDef>.GetNamedSilentFail(wgd)?.giverClass == __instance.GetType()) ?? false ||
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.workGiverList?.Any(wgd => DefDatabase<WorkGiverDef>.GetNamedSilentFail(wgd)?.giverClass == __instance.GetType()) ?? false))))
                {
                    __result = null;
                }
            }
        }

        public static void SetFactionDirectPostfix(Thing __instance, Faction newFaction)
        {
            if (__instance.def is ThingDef_AlienRace alienProps && newFaction == Faction.OfPlayerSilentFail)
            {
                alienProps.alienRace.raceRestriction.conceptList?.ForEach(cdd =>
                {
                    ConceptDef cd = DefDatabase<ConceptDef>.GetNamedSilentFail(cdd);
                    if (cdd != null)
                    {
                        Find.Tutor.learningReadout.TryActivateConcept(cd);
                        PlayerKnowledgeDatabase.SetKnowledge(cd, 0);
                    }
                });
            }
        }

        public static void SetFactionPostfix(Pawn __instance, Faction newFaction)
        {
            if (__instance.def is ThingDef_AlienRace alienProps && newFaction == Faction.OfPlayerSilentFail && Current.ProgramState == ProgramState.Playing)
            {
                alienProps.alienRace.raceRestriction.conceptList?.ForEach(cdd =>
                {
                    ConceptDef cd = DefDatabase<ConceptDef>.GetNamedSilentFail(cdd);
                    if (cdd != null)
                    {
                        Find.Tutor.learningReadout.TryActivateConcept(cd);
                        PlayerKnowledgeDatabase.SetKnowledge(cd, 0);
                    }
                });
            }
        }

        public static void ApparelScoreGainPostFix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (__result >= 0f)
            {
                if (!((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.apparelList?.Contains(ap.def.defName) ?? false ? true : (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteApparelList?.Contains(ap.def.defName) ?? false ? true :
                    (((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyUseRaceRestrictedApparel ?? false) ? false :
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.apparelList?.Contains(ap.def.defName) ?? false)))))
                {
                    __result = -50f;
                }
            }
        }

        public static void PosturePostfix(JobDriver __instance, ref PawnPosture __result)
        {
            if (__result != PawnPosture.Standing)
            {
                if (__instance.pawn.def is ThingDef_AlienRace alienProps)
                {
                    if (!alienProps.alienRace.generalSettings.canLayDown && !(__instance.pawn.CurrentBed()?.def.defName.EqualsIgnoreCase("ET_Bed") ?? false))
                    {
                        __result = PawnPosture.Standing;
                    }
                }
            }
        }

        public static void PrepForMapGenPrefix(GameInitData __instance) => Find.Scenario.AllParts.OfType<ScenPart_StartingHumanlikes>().Select(sp => sp.GetPawns()).ToList().ForEach(sp =>
        {
            __instance.startingPawns.InsertRange(__instance.startingPawnCount, sp);
            __instance.startingPawnCount += sp.Count();
        });

        public static bool TryGainMemoryThoughtPrefix(ref Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            string thoughtName = newThought.def.defName;
            Pawn pawn = __instance.pawn;
            if (DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(ar => !ar.alienRace.thoughtSettings.replacerList.NullOrEmpty()).SelectMany(ar => ar.alienRace.thoughtSettings.replacerList.Select(tr => tr.replacer)).Contains(thoughtName))
            {
                return false;
            }

            if (pawn.def is ThingDef_AlienRace)
            {
                ThoughtReplacer replacer = (pawn.def as ThingDef_AlienRace)?.alienRace.thoughtSettings.replacerList?.FirstOrDefault(tr => thoughtName.EqualsIgnoreCase(tr.original));
                if (replacer != null)
                {
                    ThoughtDef replacerThoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(replacer.replacer);
                    if (replacerThoughtDef != null)
                    {
                        Thought_Memory replaceThought = (Thought_Memory) ThoughtMaker.MakeThought(replacerThoughtDef);
                        /*
                        foreach (string infoName in AccessTools.GetFieldNames(newThought.GetType()))
                        {
                            Traverse.Create(replaceThought).Field(infoName)?.SetValue(Traverse.Create(newThought).Field(infoName).GetValue());
                        }
                        */
                        newThought = replaceThought;
                    }
                }
            }
            return true;
        }

        public static void ExtraRequirementsGrowerSowPostfix(Pawn pawn, IPlantToGrowSettable settable, WorkGiver_GrowerSow __instance, ref bool __result)
        {
            if (__result)
            {
                ThingDef plant = WorkGiver_Grower.CalculateWantedPlantDef((settable as Zone_Growing)?.Cells[0] ?? (settable as Thing).Position, pawn.Map);

                __result = (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.plantList?.Contains(plant.defName) ?? false ? true : (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whitePlantList?.Contains(plant.defName) ?? false ? true :
                    (((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyDoRaceRastrictedPlants ?? false) ? false :
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.plantList?.Contains(plant.defName) ?? false)));
            }
        }

        public static void HasJobOnCellHarvestPostfix(Pawn pawn, IntVec3 c, ref bool __result)
        {
            if (__result)
            {
                ThingDef plant = c.GetPlant(pawn.Map).def;

                __result = (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.plantList?.Contains(plant.defName) ?? false ? true : (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whitePlantList?.Contains(plant.defName) ?? false ? true :
                    ((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyDoRaceRastrictedPlants ?? false ? false :
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.plantList?.Contains(plant.defName) ?? false)));
            }
        }

        public static void PawnAllowedToStartAnewPostfix(Pawn p, Bill __instance, ref bool __result)
        {
            RecipeDef recipe = __instance.recipe;

            if (__result)
            {
                __result = (p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.recipeList?.Contains(recipe.defName) ?? false ? true : (p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteRecipeList?.Contains(recipe.defName) ?? false ? true :
                    (((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyDoRaceRestrictedRecipes ?? false) ? false :
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => p.def != d && (d.alienRace.raceRestriction.recipeList?.Contains(recipe.defName) ?? false)));
            }
        }

        static HashSet<ThingDef> colonistRaces;
        static int colonistRacesTick;
        const int colonistRacesTickTimer = GenDate.TicksPerHour * 2;

        public static void DesignatorAllowedPostfix(Designator d, ref bool __result)
        {
            if (__result && d is Designator_Build)
            {
                if (Find.TickManager.TicksAbs > colonistRacesTick + colonistRacesTickTimer || Find.TickManager.TicksAbs < colonistRacesTick)
                    if ((colonistRaces = new HashSet<ThingDef>(PawnsFinder.AllMaps_FreeColonistsSpawned.Select(p => p.def))).Count > 0)
                        colonistRacesTick = Find.TickManager.TicksAbs;

                Def toBuild = (d as Designator_Build).PlacingDef;
                IEnumerable<ThingDef_AlienRace> races = DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(ar =>
                    (ar.alienRace.raceRestriction.buildingList?.Contains(toBuild.defName) ?? false) ||
                    (ar.alienRace.raceRestriction.whiteBuildingList?.Contains(toBuild.defName) ?? false));
                if (races.Count() > 0)
                    __result = races.Any(ar => colonistRaces.Contains(ar));

                if (__result)
                    if (colonistRaces.ToList().TrueForAll(p =>
                        ((p as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyBuildRaceRestrictedBuildings ?? false) &&
                        !((p as ThingDef_AlienRace)?.alienRace.raceRestriction.buildingList?.Contains(toBuild.defName) ?? false) &&
                        !((p as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteBuildingList?.Contains(toBuild.defName) ?? false)))
                        __result = false;
            }
        }

        public static void CanConstructPostfix(Thing t, Pawn p, ref bool __result)
        {
            if (__result)
            {
                __result = ((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.buildingList?.Contains(t.def.defName) ?? false) ? true : ((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteBuildingList?.Contains(t.def.defName) ?? false) ? true :
                    (((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyBuildRaceRestrictedBuildings ?? false) ? false :
                    !(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => p.def != d && (d.alienRace.raceRestriction.buildingList?.Contains(t.def.entityDefToBuild.defName) ?? false))));
            }
        }

        public static IEnumerable<CodeInstruction> ResearchScreenTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo defListInfo = AccessTools.Method(
                typeof(DefDatabase<ResearchProjectDef>), nameof(DefDatabase<ResearchProjectDef>.AllDefsListForReading));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand == defListInfo)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(ResearchFixed)));
                }
                else
                    yield return instruction;
            }
        }

        private static List<ResearchProjectDef> ResearchFixed(List<ResearchProjectDef> researchList) =>
            researchList.Where(prj => !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(ar => !colonistRaces.Contains(ar) &&
                (ar.alienRace.raceRestriction?.researchList?.Any(rpr => rpr.projects.Contains(prj.defName)) ?? false))).ToList();

        public static void ShouldSkipResearchPostfix(Pawn pawn, ref bool __result)
        {
            if (!__result)
            {
                ResearchProjectDef project = Find.ResearchManager.currentProj;

                ResearchProjectRestrictions rprest = (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.researchList?.FirstOrDefault(rpr => rpr.projects.Contains(project.defName));
                if (rprest != null)
                {
                    IEnumerable<string> apparel = pawn.apparel.WornApparel.Select(twc => twc.def.defName);
                    if (!rprest.apparelList?.TrueForAll(ap => apparel.Contains(ap)) ?? false)
                        __result = true;
                }
                else
                    __result = DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.researchList?.Any(rpr => rpr.projects.Contains(project.defName)) ?? false));
            }
        }

        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ref List<ThoughtDef> __result)
        {
            if (ingester.story.traits.HasTrait(AlienDefOf.Xenophobia) && ingester.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1)
                if (__result.Contains(ThoughtDefOf.AteHumanlikeMeatDirect) && foodSource.def.ingestible.sourceDef != ingester.def)
                    __result.Remove(ThoughtDefOf.AteHumanlikeMeatDirect);
                else if (__result.Contains(ThoughtDefOf.AteHumanlikeMeatAsIngredient) && (foodSource.TryGetComp<CompIngredients>()?.ingredients.Any(td => FoodUtility.IsHumanlikeMeat(td) && td.ingestible.sourceDef != ingester.def) ?? false))
                    __result.Remove(ThoughtDefOf.AteHumanlikeMeatAsIngredient);
            if (ingester.def is ThingDef_AlienRace alienProps)
            {
                if (__result.Contains(ThoughtDefOf.AteHumanlikeMeatDirect) || __result.Contains(ThoughtDefOf.AteHumanlikeMeatDirectCannibal))
                {
                    int index = __result.IndexOf(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect);
                    ThoughtDef thought = DefDatabase<ThoughtDef>.GetNamedSilentFail(alienProps.alienRace.thoughtSettings.ateThoughtSpecific?.FirstOrDefault(at => at.raceList?.Contains(foodSource.def.ingestible.sourceDef.defName) ?? false)?.thought ?? alienProps.alienRace.thoughtSettings.ateThoughtGeneral.thought);
                    if (thought != null)
                    {
                        __result.RemoveAt(index);
                        __result.Insert(index, thought);
                    }
                }
                if (__result.Contains(ThoughtDefOf.AteHumanlikeMeatAsIngredient) || __result.Contains(ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal))
                {
                    CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
                    if (compIngredients != null)
                    {
                        foreach (ThingDef ingredient in compIngredients.ingredients)
                        {
                            if (FoodUtility.IsHumanlikeMeat(ingredient))
                            {
                                int index = __result.IndexOf(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient);
                                ThoughtDef thought = DefDatabase<ThoughtDef>.GetNamedSilentFail(alienProps.alienRace.thoughtSettings.ateThoughtSpecific?.FirstOrDefault(at => at.raceList?.Contains(ingredient.ingestible.sourceDef.defName) ?? false)?.ingredientThought ?? alienProps.alienRace.thoughtSettings.ateThoughtGeneral.ingredientThought);
                                if (thought != null)
                                {
                                    __result.RemoveAt(index);
                                    __result.Insert(index, thought);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void GenerationChanceSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierSpouse;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierSpouse;
            }

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierSpouse ?? 1;

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceSiblingPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierSibling;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierSibling;
            }

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierSibling ?? 1;

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceParentPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierParent;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierParent;
            }

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierParent ?? 1;

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierLover;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierLover;
            }

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierLover ?? 1;

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceFiancePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierFiance;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierFiance;
            }

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierFiance ?? 1;

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceExSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierExSpouse;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierExSpouse;
            }


            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierExSpouse ?? 1;

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceExLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierExLover;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierExLover;
            }

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierExLover ?? 1;

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceChildPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace alienProps)
            {
                __result *= alienProps.alienRace.relationSettings.relationChanceModifierChild;
            }

            if (other.def is ThingDef_AlienRace alienProps2)
            {
                __result *= alienProps2.alienRace.relationSettings.relationChanceModifierChild;
            }
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings.relationChanceModifierChild ?? 1;

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void BirthdayBiologicalPrefix(Pawn_AgeTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                string path = alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStageRace.def).head;
                if (path != null)
                {
                    Traverse.Create(pawn.story).Field("headGraphicPath").SetValue((pawn.def as ThingDef_AlienRace).alienRace.generalSettings.alienPartGenerator.RandomAlienHead(path, pawn));
                }
            }
        }

        public static bool ButcherProductsPrefix(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {
            __result = new Func<IEnumerable<Thing>>(() =>
            {
                ThingDef_AlienRace alienPropsButcher = butcher.def as ThingDef_AlienRace;

                Pawn corpse = __instance.InnerPawn;
                IEnumerable<Thing> things = corpse.ButcherProducts(butcher, efficiency);
                if (corpse.RaceProps.BloodDef != null)
                {
                    FilthMaker.MakeFilth(butcher.Position, butcher.Map, corpse.RaceProps.BloodDef, corpse.LabelIndefinite(), 1);
                }
                if (corpse.RaceProps.Humanlike)
                {
                    ThoughtDef thought = alienPropsButcher == null ? ThoughtDefOf.ButcheredHumanlikeCorpse : DefDatabase<ThoughtDef>.GetNamedSilentFail(
                        alienPropsButcher.alienRace.thoughtSettings.butcherThoughtSpecific?.FirstOrDefault(bt => bt.raceList?.Contains(corpse.def.defName) ?? false)?.thought ??
                        alienPropsButcher.alienRace.thoughtSettings.butcherThoughtGeneral.thought);

                    butcher.needs.mood.thoughts.memories.TryGainMemory(thought ?? ThoughtDefOf.ButcheredHumanlikeCorpse, null);

                    butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction).ForEach(p =>
                    {
                        if (p != butcher && p.needs != null && p.needs.mood != null && p.needs.mood.thoughts != null)
                        {
                            ThingDef_AlienRace alienPropsPawn = p.def as ThingDef_AlienRace;
                            thought = alienPropsPawn == null ? ThoughtDefOf.KnowButcheredHumanlikeCorpse : DefDatabase<ThoughtDef>.GetNamedSilentFail(alienPropsPawn.alienRace.thoughtSettings.butcherThoughtSpecific?.FirstOrDefault(bt => bt.raceList?.Contains(corpse.def.defName) ?? false)?.knowThought ?? alienPropsPawn.alienRace.thoughtSettings.butcherThoughtGeneral.knowThought);

                            p.needs.mood.thoughts.memories.TryGainMemory(thought ?? ThoughtDefOf.KnowButcheredHumanlikeCorpse, null);
                        }
                    });
                    TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, new object[] { butcher });
                }
                return things;
            })();
            return false;
        }

        public static void AddHumanlikeOrdersPostfix(ref List<FloatMenuOption> opts, Pawn pawn, Vector3 clickPos)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;
            {
                Thing drugs = c.GetThingList(pawn.Map).FirstOrDefault(t => t?.TryGetComp<CompDrug>() != null);
                if (drugs != null && (alienProps?.alienRace.generalSettings.chemicalSettings?.Any(cs => cs.chemical.EqualsIgnoreCase(drugs.TryGetComp<CompDrug>()?.Props.chemical?.defName) && !cs.ingestible) ?? false))
                {
                    List<FloatMenuOption> options = opts.Where(fmo => !fmo.Disabled && fmo.Label.Contains(string.Format(drugs.def.ingestible.ingestCommandString, drugs.LabelShort))).ToList();
                    foreach (FloatMenuOption fmo in options)
                    {
                        int index = opts.IndexOf(fmo);
                        opts.Remove(fmo);

                        opts.Insert(index, new FloatMenuOption("CannotEquip".Translate(new object[]
                            {
                                    drugs.LabelShort
                            }) + " (" + pawn.def.LabelCap + " can't consume this)", null));
                    }
                }
            }
            if (pawn.equipment != null)
            {
                ThingWithComps equipment = (ThingWithComps) c.GetThingList(pawn.Map).FirstOrDefault(t => t.TryGetComp<CompEquippable>() != null && t.def.IsWeapon);
                if (equipment != null)
                {
                    List<FloatMenuOption> options = opts.Where(fmo => !fmo.Disabled && fmo.Label.Contains("Equip".Translate(new object[] { equipment.LabelShort }))).ToList();

                    bool restrictionsOff = (alienProps?.alienRace.raceRestriction.weaponList?.Contains(equipment.def.defName) ?? false) ? true :
                    (alienProps?.alienRace.raceRestriction.whiteWeaponList?.Contains(equipment.def.defName) ?? true);

                    if (!options.NullOrEmpty() && (!restrictionsOff || DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d =>
                    pawn.def != d && (d.alienRace.raceRestriction.weaponList?.Contains(equipment.def.defName) ?? false))))
                    {
                        foreach (FloatMenuOption fmo in options)
                        {
                            int index = opts.IndexOf(fmo);
                            opts.Remove(fmo);

                            opts.Insert(index, new FloatMenuOption("CannotEquip".Translate(new object[]
                                {
                                    equipment.LabelShort
                                }) + " (" + pawn.def.LabelCap + " can't equip this" + ")", null));
                        }
                    }

                    if (alienProps != null && alienProps.alienRace.raceRestriction.onlyUseRaceRestrictedWeapons)
                    {
                        options = opts.Where(fmo => !fmo.Disabled && fmo.Label.Contains("Equip".Translate(new object[] { equipment.LabelShort }))).ToList();

                        if (!options.NullOrEmpty() && !((alienProps.alienRace.raceRestriction.weaponList?.Contains(equipment.def.defName) ?? false) ||
                        (alienProps.alienRace.raceRestriction.whiteWeaponList?.Contains(equipment.def.defName) ?? false)))
                        {
                            foreach (FloatMenuOption fmo in options)
                            {
                                int index = opts.IndexOf(fmo);
                                opts.Remove(fmo);

                                opts.Insert(index, new FloatMenuOption("CannotEquip".Translate(new object[]
                                    {
                                        equipment.LabelShort
                                    }) + " (" + pawn.def.LabelCap + " can't use other races' weapons" + ")", null));
                            }
                        }
                    }
                }
            }

            if (pawn.apparel != null)
            {
                Apparel apparel = pawn.Map.thingGrid.ThingAt<Apparel>(c);
                if (apparel != null)
                {
                    List<FloatMenuOption> options = opts.Where(fmo => !fmo.Disabled && fmo.Label.Contains("ForceWear".Translate(new object[] { apparel.LabelShort }))).ToList();

                    bool restrictionsOff = (alienProps?.alienRace.raceRestriction.apparelList?.Contains(apparel.def.defName) ?? false) ? true :
                    (alienProps?.alienRace.raceRestriction.whiteApparelList?.Contains(apparel.def.defName) ?? true) ? true : false;

                    if (!options.NullOrEmpty() && (!restrictionsOff && DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d =>
                    pawn.def != d && (d.alienRace.raceRestriction.apparelList?.Contains(apparel.def.defName) ?? false))))
                    {
                        foreach (FloatMenuOption fmo in options)
                        {
                            int index = opts.IndexOf(fmo);
                            opts.Remove(fmo);

                            opts.Insert(index, new FloatMenuOption("CannotWear".Translate(new object[]
                                {
                                    apparel.LabelShort
                                }) + " (" + pawn.def.LabelCap + " can't wear this" + ")", null));
                        }
                    }

                    if (alienProps != null && alienProps.alienRace.raceRestriction.onlyUseRaceRestrictedApparel)
                    {
                        options = opts.Where(fmo => !fmo.Disabled && fmo.Label.Contains("ForceWear".Translate(new object[] { apparel.LabelShort }))).ToList();
                        if (!options.NullOrEmpty() && !((alienProps.alienRace.raceRestriction.apparelList?.Contains(apparel.def.defName) ?? false) ||
                        (alienProps.alienRace.raceRestriction.whiteApparelList?.Contains(apparel.def.defName) ?? false)))
                        {
                            foreach (FloatMenuOption fmo in options)
                            {
                                int index = opts.IndexOf(fmo);
                                opts.Remove(fmo);

                                opts.Insert(index, new FloatMenuOption("CannotWear".Translate(new object[]
                                    {
                                        apparel.LabelShort
                                    }) + " (" + pawn.def.LabelCap + " can't use other races' apparel" + ")", null));
                            }
                        }
                    }
                }
            }
        }

        public static void CanGetThoughtPostfix(ref bool __result, ThoughtDef def, Pawn pawn)
        {
            ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;
            if (__result && alienProps != null)
            {
                if (alienProps.alienRace.thoughtSettings.cannotReceiveThoughtsAtAll && (alienProps.alienRace.thoughtSettings.canStillReceiveThoughts?.Contains(def.defName) ?? false))
                {
                    __result = false;
                }
                else if (!alienProps.alienRace.thoughtSettings.cannotReceiveThoughts.NullOrEmpty() && alienProps.alienRace.thoughtSettings.cannotReceiveThoughts.Contains(def.defName))
                {
                    __result = false;
                }
            }
        }

        public static void CanDoNextStartPawnPostfix(ref bool __result)
        {
            if (__result)
            {
                return;
            }

            bool result = true;
            Find.GameInitData.startingPawns.ForEach(current =>
            {
                if (!current.Name.IsValid && current.def.race.GetNameGenerator(current.gender) == null)
                {
                    result = false;
                }
            });
            __result = result;
        }

        public static bool GeneratePawnNamePrefix(ref Name __result, Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
        {
            ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;
            if (alienProps == null || alienProps.race.GetNameGenerator(pawn.gender) == null || style != NameStyle.Full)
            {
                return true;
            }

            NameTriple nameTriple = NameTriple.FromString(NameGenerator.GenerateName(alienProps.race.GetNameGenerator(pawn.gender)));

            string first = nameTriple.First, nick = nameTriple.Nick, last = nameTriple.Last;

            if (nick == null)
            {
                nick = nameTriple.First;
            }

            if (last != null && forcedLastName != null)
            {
                last = forcedLastName;
            }

            __result = new NameTriple(first, nick, last);

            return false;
        }

        public static void GenerateRandomAgePrefix(Pawn pawn, PawnGenerationRequest request)
        {
            if (!request.FixedGender.HasValue)
            {
                float maleGenderProbability = (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.maleGenderProbability ?? pawn.kindDef.GetModExtension<Info>()?.maleGenderProbability ?? 0.5f;
                if (maleGenderProbability != 0.5f)
                {
                    pawn.gender = Rand.Value >= maleGenderProbability ? Gender.Female : Gender.Male;
                    AlienPartGenerator.AlienComp alienComp = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
                    if (alienComp != null && maleGenderProbability == 0f || maleGenderProbability == 100f)
                    {
                        alienComp.fixGenderPostSpawn = true;
                    }
                }
            }
        }

        public static void GiveAppropriateBioAndNameToPostfix(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                //Log.Message(pawn.LabelCap);
                if (alienProps.alienRace.generalSettings.alienPartGenerator.alienhaircolorgen != null)
                    pawn.story.hairColor = alienProps.alienRace.generalSettings.alienPartGenerator.alienhaircolorgen.NewRandomizedColor();
                else if (alienProps.alienRace.generalSettings.alienPartGenerator.useSkincolorForHair && alienProps.alienRace.generalSettings.alienPartGenerator.alienskincolorgen != null)
                    pawn.story.hairColor = pawn.story.SkinColor;

                pawn.GetComp<AlienPartGenerator.AlienComp>().hairColorSecond = alienProps.alienRace.generalSettings.alienPartGenerator.alienhairsecondcolorgen?.NewRandomizedColor() ?? pawn.story.hairColor;

                if (alienProps.alienRace.hairSettings.getsGreyAt <= pawn.ageTracker.AgeBiologicalYears)
                {
                    float grey = Rand.Range(0.65f, 0.85f);
                    pawn.story.hairColor = new Color(grey, grey, grey);
                }
                Traverse.Create(pawn.story).Field("headGraphicPath").SetValue(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).head.NullOrEmpty() ? "" :
                    alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).head, pawn));
            }
        }

        public static bool NewGeneratedStartingPawnPrefix(ref Pawn __result)
        {
            PawnKindDef kindDef = Faction.OfPlayer.def.basicMemberKind;

            DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(tdar => !tdar.alienRace.pawnKindSettings.startingColonists.NullOrEmpty()).
                SelectMany(tdar => tdar.alienRace.pawnKindSettings.startingColonists).Where(sce => sce.factionDefs.Contains(Faction.OfPlayer.def.defName)).SelectMany(sce => sce.pawnKindEntries).InRandomOrder().ToList().ForEach(pke =>
                {
                    if (Rand.Range(0f, 100f) < pke.chance)
                    {
                        PawnKindDef pk = DefDatabase<PawnKindDef>.GetNamedSilentFail(pke.kindDefs.RandomElement());
                        if (pk != null)
                        {
                            kindDef = pk;
                        }
                    }
                });

            if (kindDef == Faction.OfPlayer.def.basicMemberKind)
            {
                return true;
            }

            PawnGenerationRequest request = new PawnGenerationRequest(kindDef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, forceGenerateNewPawn: true, colonistRelationChanceFactor: 26f);
            Pawn pawn = null;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            __result = pawn;

            return false;
        }

        public static void RandomHediffsToGainOnBirthdayPostfix(ref IEnumerable<HediffGiver_Birthday> __result, ThingDef raceDef)
        {
            if ((raceDef as ThingDef_AlienRace)?.alienRace.generalSettings.immuneToAge ?? false)
                __result = new List<HediffGiver_Birthday>();
        }

        public static bool GenerateRandomOldAgeInjuriesPrefix(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps && alienProps.alienRace.generalSettings.immuneToAge)
            {
                return false;
            }
            return true;
        }

        public static bool FillBackstoryInSlotShuffledPrefix(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
        {
            bioReference = null;
            if (slot == BackstorySlot.Adulthood && DefDatabase<BackstoryDef>.GetNamedSilentFail(pawn.story.childhood.identifier)?.linkedBackstory is string id && BackstoryDatabase.TryGetWithIdentifier(id, out backstory))
                return false;

            if (((pawn.def is ThingDef_AlienRace alienProps && alienProps.alienRace.generalSettings.pawnsSpecificBackstories) || (pawn.kindDef.GetModExtension<Info>()?.usePawnKindBackstories ?? false)) && !pawn.kindDef.backstoryCategory.NullOrEmpty())
            {
                /*
                Log.Message(pawn.def.defName);
re
                Log.Message(string.Join("\n", BackstoryDatabase.allBackstories.Where(kvp => kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(pawn.kindDef.backstoryCategory) &&
                    kvp.Value.slot == slot && (slot == BackstorySlot.Childhood ||
                    !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood?.workDisables ?? WorkTags.None)) &&
                    (DefDatabase<BackstoryDef>.GetNamedSilentFail(kvp.Value.identifier)?.commonalityApproved(pawn.gender) ?? true)).Select(kvp => kvp.Value.identifier).ToArray()));
                    */
                if (BackstoryDatabase.allBackstories.Where(kvp => kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(pawn.kindDef.backstoryCategory) &&
                    kvp.Value.slot == slot && (slot == BackstorySlot.Childhood ||
                    !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood?.workDisables ?? WorkTags.None)) &&
                    (!(DefDatabase<BackstoryDef>.GetNamedSilentFail(kvp.Value.identifier) is BackstoryDef bs) || (bs.Approved(pawn) && (slot == BackstorySlot.Childhood || bs.linkedBackstory.NullOrEmpty())))).TryRandomElement(out KeyValuePair<string, Backstory> backstoryPair))
                {
                    backstory = backstoryPair.Value;
                    return false;
                }
                Log.Message("FAILED: " + pawn.def.defName);
            }
            return true;
        }

        static PawnBioDef bioReference;

        public static void TryGetRandomUnusedSolidBioForPostfix(string backstoryCategory, ref PawnBio __result, PawnKindDef kind, Gender gender, string requiredLastName)
        {
            if (SolidBioDatabase.allBios.Where(pb => ((((kind.race as ThingDef_AlienRace)?.alienRace.generalSettings.allowHumanBios ?? true) && (kind.GetModExtension<Info>()?.allowHumanBios ?? true)) ||
                (DefDatabase<PawnBioDef>.AllDefs.FirstOrDefault(pbd => pb.name.ConfusinglySimilarTo(pbd.name))?.validRaces.Contains(kind.race) ?? false)) &&
                (pb.gender == GenderPossibility.Either || (pb.gender == GenderPossibility.Male && gender == Gender.Male)) &&
                (requiredLastName.NullOrEmpty() || !(pb.name.Last != requiredLastName)) && (!kind.factionLeader || pb.pirateKing) &&
                pb.adulthood.spawnCategories.Contains(backstoryCategory) && !pb.name.UsedThisGame).TryRandomElement(out PawnBio bio))
            {
                __result = bio;
                bioReference = DefDatabase<PawnBioDef>.AllDefs.FirstOrDefault(pbd => bio.name.ConfusinglySimilarTo(pbd.name));
            }
            else
            {
                __result = null;
            }
        }

        public static bool ResolveAllGraphicsPrefix(PawnGraphicSet __instance)
        {
            Pawn alien = __instance.pawn;
            if (alien.def is ThingDef_AlienRace alienProps)
            {
                AlienPartGenerator.AlienComp alienComp = __instance.pawn.GetComp<AlienPartGenerator.AlienComp>();
                if (alienComp.fixGenderPostSpawn)
                {
                    if (alienProps.alienRace.generalSettings.maleGenderProbability != 0.5f)
                    {
                        __instance.pawn.gender = Rand.Value >= alienProps.alienRace.generalSettings.maleGenderProbability ? Gender.Female : Gender.Male;
                        __instance.pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(__instance.pawn, NameStyle.Full);
                    }


                    Traverse.Create(__instance.pawn.story).Field("headGraphicPath").SetValue(
                        alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).head.NullOrEmpty() ? "" :
                        alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).head, __instance.pawn));

                    alienComp.fixGenderPostSpawn = false;
                }
                GraphicPaths graphicPaths = alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage);

                alienComp.customDrawSize = graphicPaths.customDrawSize;
                alienComp.customPortraitDrawSize = graphicPaths.customPortraitDrawSize;

                alienComp.AssignProperMeshs();

                Traverse.Create(alien.story).Field("headGraphicPath").SetValue(alienComp.crownType.NullOrEmpty() ? alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(graphicPaths.head, alien) : AlienPartGenerator.GetAlienHead(graphicPaths.head, (alienProps.alienRace.generalSettings.alienPartGenerator.useGenderedHeads ? alien.gender.ToString() : ""), alienComp.crownType));

                __instance.nakedGraphic = !graphicPaths.body.NullOrEmpty() ? alienProps.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(alien.story.bodyType, ContentFinder<Texture2D>.Get(AlienPartGenerator.GetNakedPath(alien.story.bodyType, graphicPaths.body, alienProps.alienRace.generalSettings.alienPartGenerator.useGenderedBodies ? alien.gender.ToString() : "") + "_backm", false) == null ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, __instance.pawn.story.SkinColor, alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(alien, false), graphicPaths.body, alien.gender.ToString()) : null;
                __instance.rottingGraphic = !graphicPaths.body.NullOrEmpty() ? alienProps.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(alien.story.bodyType, ShaderDatabase.Cutout, PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor, graphicPaths.body, alien.gender.ToString()) : null;
                __instance.dessicatedGraphic = !graphicPaths.skeleton.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(graphicPaths.skeleton, ShaderDatabase.Cutout) : null;
                __instance.headGraphic = alien.health.hediffSet.HasHead && !alien.story.HeadGraphicPath.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(alien.story.HeadGraphicPath, ContentFinder<Texture2D>.Get(alien.story.HeadGraphicPath + "_backm", false) == null ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, Vector2.one, alien.story.SkinColor, alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(alien, false)) : null;
                __instance.desiccatedHeadGraphic = alien.health.hediffSet.HasHead && !alien.story.HeadGraphicPath.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(alien.story.HeadGraphicPath, ShaderDatabase.Cutout, Vector2.one, PawnGraphicSet.RottingColor) : null;
                __instance.skullGraphic = alien.health.hediffSet.HasHead && !graphicPaths.skull.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(graphicPaths.skull, ShaderDatabase.Cutout, Vector2.one, Color.white) : null;
                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.hairDef.texPath, ContentFinder<Texture2D>.Get(__instance.pawn.story.hairDef.texPath + "_backm", false) == null ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, Vector2.one, alien.story.hairColor, alienComp.hairColorSecond);
                __instance.headStumpGraphic = !graphicPaths.stump.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(graphicPaths.stump, alienComp.skinColor == alienComp.skinColorSecond ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, Vector2.one, alien.story.SkinColor, alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(alien, false)) : null;

                AlienPartGenerator apg = alienProps.alienRace.generalSettings.alienPartGenerator;
                alienComp.addonGraphics = new List<Graphic>();
                if(alienComp.addonVariants == null)
                    alienComp.addonVariants = new List<int>();
                int sharedIndex = 0;
                for (int i = 0; i < apg.bodyAddons.Count; i++)
                {
                    alienComp.addonGraphics.Add(apg.bodyAddons[i].GetPath(alien, ref sharedIndex, alienComp.addonVariants.Count > i ? (int?) alienComp.addonVariants[i] : null));
                    if (alienComp.addonVariants.Count <= i)
                        alienComp.addonVariants.Add(sharedIndex);
                }

                __instance.ResolveApparelGraphics();

                return false;
            }
            return true;
        }

        public static void GenerateTraitsPrefix(Pawn pawn, PawnGenerationRequest request)
        {

            if (!request.Newborn && request.CanGeneratePawnRelations && pawn.story.AllBackstories.Any(bs => DefDatabase<BackstoryDef>.GetNamedSilentFail(bs.identifier)?.relationSettings != null))
            {
                pawn.relations.ClearAllRelations();
                AccessTools.Method(typeof(PawnGenerator), "GeneratePawnRelations").Invoke(null, new object[] { pawn, request });
            }



            if (pawn.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.generalSettings.forcedRaceTraitEntries.NullOrEmpty())
            {
                alienProps.alienRace.generalSettings.forcedRaceTraitEntries.ForEach(ate =>
                {
                    if (pawn.story.traits.allTraits.Count < 4 && (pawn.gender == Gender.Male && (ate.commonalityMale == -1f || Rand.Range(0, 100) < ate.commonalityMale)) || (pawn.gender == Gender.Female && (ate.commonalityFemale == -1f || Rand.Range(0, 100) < ate.commonalityFemale)) || pawn.gender == Gender.None)
                    {
                        if (!pawn.story.traits.allTraits.Any(tr => tr.def.defName.EqualsIgnoreCase(ate.defName)))
                        {
                            pawn.story.traits.GainTrait(new Trait(TraitDef.Named(ate.defName), ate.degree, true));
                        }
                    }
                });
            }

        }

        public static void SkinColorPostfix(Pawn_StoryTracker __instance, ref Color __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                __result = alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(pawn);
            }
        }

        public static void GenerateBodyTypePostfix(ref Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.NullOrEmpty() && !alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.Contains(pawn.story.bodyType))
            {
                pawn.story.bodyType = alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.RandomElement();
            }
        }

        static FactionDef noHairFaction = new FactionDef() { hairTags = new List<string>() { "alienNoHair" } };
        static FactionDef hairFaction = new FactionDef();

        public static void RandomHairDefForPrefix(Pawn pawn, ref FactionDef factionType)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                if (!alienProps.alienRace.hairSettings.hasHair)
                {
                    factionType = noHairFaction;
                }
                else if (!alienProps.alienRace.hairSettings.hairTags.NullOrEmpty())
                {
                    hairFaction.hairTags = alienProps.alienRace.hairSettings.hairTags;
                    factionType = hairFaction;
                }
            }
        }

        public static void GeneratePawnPrefix(ref PawnGenerationRequest request)
        {
            PawnKindDef kindDef = request.KindDef;
            if (Faction.OfPlayerSilentFail != null && kindDef == PawnKindDefOf.Villager && request.Faction.IsPlayer && kindDef.race != Faction.OfPlayer?.def.basicMemberKind.race)
            {
                kindDef = Faction.OfPlayer.def.basicMemberKind;
            }

            if (Rand.Value <= 0.4f)
            {
                IEnumerable<ThingDef_AlienRace> comps = DefDatabase<ThingDef_AlienRace>.AllDefsListForReading;
                PawnKindEntry pk;
                if (request.KindDef == PawnKindDefOf.SpaceRefugee)
                {
                    if (comps.Where(r => !r.alienRace.pawnKindSettings.alienrefugeekinds.NullOrEmpty()).Select(r => r.alienRace.pawnKindSettings.alienrefugeekinds.RandomElement()).TryRandomElementByWeight((PawnKindEntry pke) => pke.chance, out pk))
                    {
                        PawnKindDef pkd = DefDatabase<PawnKindDef>.GetNamedSilentFail(pk.kindDefs.RandomElement());
                        if (pkd != null)
                        {
                            kindDef = pkd;
                        }
                    }
                }
                else if (request.KindDef == PawnKindDefOf.Slave)
                {
                    if (comps.Where(r => !r.alienRace.pawnKindSettings.alienslavekinds.NullOrEmpty()).Select(r => r.alienRace.pawnKindSettings.alienslavekinds.RandomElement()).TryRandomElementByWeight((PawnKindEntry pke) => pke.chance, out pk))
                    {
                        PawnKindDef pkd = DefDatabase<PawnKindDef>.GetNamedSilentFail(pk.kindDefs.RandomElement());
                        if (pkd != null)
                        {
                            kindDef = pkd;
                        }
                    }
                }
                else if (request.KindDef == PawnKindDefOf.Villager)
                {
                    DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(tdar => !tdar.alienRace.pawnKindSettings.alienwandererkinds.NullOrEmpty()).
                        SelectMany(tdar => tdar.alienRace.pawnKindSettings.alienwandererkinds).Where(sce => sce.factionDefs.Contains(Faction.OfPlayer.def.defName)).SelectMany(sce => sce.pawnKindEntries).InRandomOrder().ToList().ForEach(pke =>
                        {
                            if (Rand.Range(0f, 100f) < pke.chance)
                            {
                                PawnKindDef fpk = DefDatabase<PawnKindDef>.GetNamedSilentFail(pke.kindDefs.RandomElement());
                                if (fpk != null)
                                {
                                    kindDef = fpk;
                                }
                            }
                        });
                }
            }

            request = new PawnGenerationRequest(kindDef, request.Faction, request.Context, request.Tile, request.ForceGenerateNewPawn, request.Newborn,
            request.AllowDead, request.AllowDead, request.CanGeneratePawnRelations, request.MustBeCapableOfViolence, request.ColonistRelationChanceFactor,
            request.ForceAddFreeWarmLayerIfNeeded, request.AllowGay, request.AllowFood, request.Inhabitant, request.CertainlyBeenInCryptosleep,
            request.ForceRedressWorldPawnIfFormerColonist, request.WorldPawnFactionDoesntMatter, request.Validator, request.MinChanceToRedressWorldPawn, request.FixedBiologicalAge,
            request.FixedChronologicalAge, request.FixedGender, request.FixedMelanin, request.FixedLastName);
        }

        public static IEnumerable<CodeInstruction> RenderPawnInternalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo humanlikeBodyInfo = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeBodySet));
            FieldInfo humanlikeHeadInfo = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeHeadSet));
            MethodInfo hairInfo = AccessTools.Property(typeof(PawnGraphicSet), nameof(PawnGraphicSet.HairMeshSet)).GetGetMethod();
            MethodInfo isAnimalInfo = AccessTools.Property(typeof(RaceProperties), nameof(RaceProperties.Animal)).GetGetMethod();

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (instruction.operand == humanlikeBodyInfo)
                {
                    instructionList.RemoveRange(i, 2);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7); // portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4); // bodyfacing
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(GetPawnMesh)));
                }
                else if (instruction.operand == humanlikeHeadInfo)
                {
                    instructionList.RemoveRange(i, 2);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7); // portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5); //headfacing
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(GetPawnMesh)));
                }
                else if (i + 4 < instructionList.Count && instructionList[i + 2].operand == hairInfo)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7) { labels = instruction.labels }; // portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);  //headfacing
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.graphics)));
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(GetPawnHairMesh)));
                    instructionList.RemoveRange(i, 4);
                }
                else if (i + 5 < instructionList.Count && instructionList[i + 5].operand == isAnimalInfo)
                {
                    yield return instruction; // portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6); //vector
                    yield return new CodeInstruction(OpCodes.Ldarg_2); // quat
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4); // bodyfacing
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(DrawAddons)));

                    instruction = new CodeInstruction(OpCodes.Ldarg_S, 7);
                }
                yield return instruction;
            }
        }

        public static Mesh GetPawnMesh(bool portrait, Pawn pawn, Rot4 facing, bool wantsBody) =>
            pawn.GetComp<AlienPartGenerator.AlienComp>() is AlienPartGenerator.AlienComp alienComp ?
                portrait ?
                    wantsBody ?
                        alienComp.alienPortraitGraphics.bodySet.MeshAt(facing) :
                        alienComp.alienPortraitGraphics.headSet.MeshAt(facing) :
                    wantsBody ?
                        alienComp.alienGraphics.bodySet.MeshAt(facing) :
                        alienComp.alienGraphics.headSet.MeshAt(facing) :
                wantsBody ?
                    MeshPool.humanlikeBodySet.MeshAt(facing) :
                    MeshPool.humanlikeHeadSet.MeshAt(facing);

        public static Mesh GetPawnHairMesh(bool portrait, Pawn pawn, Rot4 headFacing, PawnGraphicSet graphics) =>
            pawn.GetComp<AlienPartGenerator.AlienComp>() is AlienPartGenerator.AlienComp alienComp ?
                    (pawn.story.crownType == CrownType.Narrow ?
                        (portrait ?
                            alienComp.alienPortraitGraphics.hairSetNarrow :
                            alienComp.alienGraphics.hairSetNarrow) :
                        (portrait ?
                            alienComp.alienPortraitGraphics.hairSetAverage :
                            alienComp.alienGraphics.hairSetAverage)).MeshAt(headFacing) :
                    graphics.HairMeshSet.MeshAt(headFacing);
        
        public static void DrawAddons(bool portrait, Pawn pawn, Vector3 vector, Quaternion quat, Rot4 rotation)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                List<AlienPartGenerator.BodyAddon> addons = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons;
                AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                for (int i = 0; i < addons.Count; i++)
                {
                    AlienPartGenerator.BodyAddon ba = addons[i];


                    if (ba.CanDrawAddon(pawn))
                    {
                        AlienPartGenerator.RotationOffset offset = rotation == Rot4.South ? ba.offsets.front : rotation == Rot4.North ? ba.offsets.back : ba.offsets.side;
                        
                        Vector2 bodyOffset = (portrait ? offset?.portraitBodyTypes ?? offset.bodyTypes : offset?.bodyTypes)?.FirstOrDefault(to => to.bodyType == pawn.story.bodyType)?.offset ?? Vector2.zero;
                        Vector2 crownOffset = (portrait ? offset?.portraitCrownTypes ?? offset.crownTypes : offset?.crownTypes)?.FirstOrDefault(to => to.crownType == alienComp.crownType)?.offset ?? Vector2.zero;

                        //Defaults for tails 
                        //front 0.42f, -0.3f, -0.22f
                        //back     0f,  0.3f, -0.55f
                        //side -0.42f, -0.3f, -0.22f   

                        float MoffsetX = 0.42f;
                        float MoffsetZ = -0.22f;
                        float MoffsetY = ba.inFrontOfBody ? 0.3f + ba.layerOffset : -0.3f - ba.layerOffset;
                        float num = ba.angle;

                        Mesh mesh = portrait ? alienComp.alienPortraitGraphics.addonMeshFlipped : alienComp.alienGraphics.addonMesh;

                        if (rotation == Rot4.North)
                        {
                            MoffsetX = 0f;
                            MoffsetY = !ba.inFrontOfBody ? -0.3f - ba.layerOffset : 0.3f + ba.layerOffset;
                            MoffsetZ = -0.55f;
                            num = 0;
                        }

                        MoffsetX += bodyOffset.x + crownOffset.x;
                        MoffsetZ += bodyOffset.y + crownOffset.y;

                        if (rotation == Rot4.East)
                        {
                            MoffsetX = -MoffsetX;
                            num = -num; //Angle
                            mesh = alienComp.alienGraphics.addonMeshFlipped;
                        }
                        Vector3 offsetVector = new Vector3(MoffsetX, MoffsetY, MoffsetZ);
                        //                                                                                Angle calculation to not pick the shortest, taken from Quaternion.Angle
                        GenDraw.DrawMeshNowOrLater(mesh, vector + Vector3Utility.RotatedBy(offsetVector, (Mathf.Acos(Quaternion.Dot(Quaternion.identity, quat)) * 2f * 57.29578f)), Quaternion.AngleAxis(num, Vector3.up) * quat, alienComp.addonGraphics[i].MatAt(rotation), portrait);
                    }
                }
            }
        }
    }
}