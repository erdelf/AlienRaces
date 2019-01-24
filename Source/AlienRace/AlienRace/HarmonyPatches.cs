namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using Harmony;
    using Harmony.ILCopying;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using Verse.Grammar;

    /// <summary>
    /// "More useful than the Harmony wiki" ~ Mehni
    /// </summary>

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create(id: "rimworld.erdelf.alien_race.main");
            
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationWorker_Child), name: nameof(PawnRelationWorker_Child.GenerationChance)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerationChanceChildPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationWorker_ExLover), name: nameof(PawnRelationWorker_ExLover.GenerationChance)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerationChanceExLoverPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationWorker_ExSpouse), name: nameof(PawnRelationWorker_ExSpouse.GenerationChance)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerationChanceExSpousePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationWorker_Fiance), name: nameof(PawnRelationWorker_Spouse.GenerationChance)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerationChanceFiancePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationWorker_Lover), name: nameof(PawnRelationWorker_Lover.GenerationChance)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerationChanceLoverPostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationWorker_Parent), name: nameof(PawnRelationWorker_Parent.GenerationChance)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerationChanceParentPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationWorker_Sibling), name: nameof(PawnRelationWorker_Sibling.GenerationChance)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerationChanceSiblingPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationWorker_Spouse), name: nameof(PawnRelationWorker_Spouse.GenerationChance)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerationChanceSpousePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGenerator), name: "GeneratePawnRelations"),
                prefix: new HarmonyMethod(type: patchType, name: nameof(GeneratePawnRelationsPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRelationDef), name: nameof(PawnRelationDef.GetGenderSpecificLabel)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(GetGenderSpecificLabelPrefix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(PawnBioAndNameGenerator), name: "TryGetRandomUnusedSolidBioFor"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(TryGetRandomUnusedSolidBioForPostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(PawnBioAndNameGenerator), name: "FillBackstorySlotShuffled"),
                prefix: new HarmonyMethod(type: patchType, name: nameof(FillBackstoryInSlotShuffledPrefix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(WorkGiver_Researcher), name: nameof(WorkGiver_Researcher.ShouldSkip)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(ShouldSkipResearchPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(MainTabWindow_Research), name: "ViewSize"), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(type: patchType, name: nameof(ResearchScreenTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(MainTabWindow_Research), name: "DrawRightRect"), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(type: patchType, name: nameof(ResearchScreenTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(GenConstruct), name: nameof(GenConstruct.CanConstruct)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CanConstructPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(GameRules), name: nameof(GameRules.DesignatorAllowed)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(DesignatorAllowedPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Bill), name: nameof(Bill.PawnAllowedToStartAnew)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(PawnAllowedToStartAnewPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(WorkGiver_GrowerHarvest), name: nameof(WorkGiver_GrowerHarvest.HasJobOnCell)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(HasJobOnCellHarvestPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(WorkGiver_GrowerSow), name: "ExtraRequirements"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(ExtraRequirementsGrowerSowPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(FloatMenuMakerMap), name: "AddHumanlikeOrders"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(AddHumanlikeOrdersPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn), name: nameof(Pawn.SetFaction)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(SetFactionPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Thing), name: nameof(Pawn.SetFactionDirect)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(SetFactionDirectPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(JobGiver_OptimizeApparel), name: nameof(JobGiver_OptimizeApparel.ApparelScoreGain)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(ApparelScoreGainPostFix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(ThoughtUtility), name: nameof(ThoughtUtility.CanGetThought)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CanGetThoughtPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Corpse), name: nameof(Corpse.ButcherProducts)), prefix: new HarmonyMethod(type: patchType, name: nameof(ButcherProductsPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(FoodUtility), name: nameof(FoodUtility.ThoughtsFromIngesting)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(ThoughtsFromIngestingPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(MemoryThoughtHandler), name: nameof(MemoryThoughtHandler.TryGainMemory), parameters: new[] {typeof(Thought_Memory), typeof(Pawn)}),
                prefix: new HarmonyMethod(type: patchType, name: nameof(TryGainMemoryThoughtPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(SituationalThoughtHandler), name: "TryCreateThought"),
                prefix: new HarmonyMethod(type: patchType, name: nameof(TryCreateSituationalThoughtPrefix)));

            harmony.Patch(original: AccessTools.Method(type: AccessTools.TypeByName(name: "AgeInjuryUtility"), name: "GenerateRandomOldAgeInjuries"),
                prefix: new HarmonyMethod(type: patchType, name: nameof(GenerateRandomOldAgeInjuriesPrefix)));
            harmony.Patch(
                original: AccessTools.Method(type: AccessTools.TypeByName(name: "AgeInjuryUtility"), name: "RandomHediffsToGainOnBirthday", parameters: new[] {typeof(ThingDef), typeof(int)}),
                prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(RandomHediffsToGainOnBirthdayPostfix)));
            //            harmony.Patch(original: AccessTools.Property(type: typeof(JobDriver), name: nameof(JobDriver.Posture)).GetGetMethod(nonPublic: false), prefix: null,
            //                postfix: new HarmonyMethod(type: patchType, name: nameof(PosturePostfix)));
            //            harmony.Patch(original: AccessTools.Property(type: typeof(JobDriver_Skygaze), name: nameof(JobDriver_Skygaze.Posture)).GetGetMethod(nonPublic: false), prefix: null,
            //                postfix: new HarmonyMethod(type: patchType, name: nameof(PosturePostfix)));
            
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGenerator), name: "GenerateRandomAge"), prefix: new HarmonyMethod(type: patchType, name: nameof(GenerateRandomAgePrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGenerator), name: "GenerateTraits"), prefix: new HarmonyMethod(type: patchType, name: nameof(GenerateTraitsPrefix)),
                postfix: null, transpiler: new HarmonyMethod(type: patchType,                                                                           name: nameof(GenerateTraitsTranspiler)));
            
            harmony.Patch(original: AccessTools.Method(type: typeof(JobGiver_SatisfyChemicalNeed), name: "DrugValidator"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(DrugValidatorPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(CompDrug), name: nameof(CompDrug.PostIngested)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(PostIngestedPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(AddictionUtility), name: nameof(AddictionUtility.CanBingeOnNow)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CanBingeNowPostfix)));
            
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGenerator), name: "GenerateBodyType"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerateBodyTypePostfix)));
            harmony.Patch(original: AccessTools.Property(type: typeof(Pawn_StoryTracker), name: nameof(Pawn_StoryTracker.SkinColor)).GetGetMethod(), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(SkinColorPostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(PawnHairChooser), name: nameof(PawnHairChooser.RandomHairDefFor)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(RandomHairDefForPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn_AgeTracker), name: "BirthdayBiological"), prefix: new HarmonyMethod(type: patchType, name: nameof(BirthdayBiologicalPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGenerator), name: nameof(PawnGenerator.GeneratePawn), parameters: new[] {typeof(PawnGenerationRequest)}),
                prefix: new HarmonyMethod(type: patchType, name: nameof(GeneratePawnPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGraphicSet), name: nameof(PawnGraphicSet.ResolveAllGraphics)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(ResolveAllGraphicsPrefix)));
            //HarmonyInstance.DEBUG = true;
            harmony.Patch(
                original: AccessTools.Method(type: typeof(PawnRenderer), name: "RenderPawnInternal",
                    parameters: new[] {typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool)}), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(type: patchType, name: nameof(RenderPawnInternalTranspiler)));
            //HarmonyInstance.DEBUG = false;
            harmony.Patch(original: AccessTools.Method(type: typeof(StartingPawnUtility), name: nameof(StartingPawnUtility.NewGeneratedStartingPawn)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(NewGeneratedStartingPawnPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnBioAndNameGenerator), name: nameof(PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GiveAppropriateBioAndNameToPostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(PawnBioAndNameGenerator), name: nameof(PawnBioAndNameGenerator.GeneratePawnName)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(GeneratePawnNamePrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Page_ConfigureStartingPawns), name: "CanDoNext"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CanDoNextStartPawnPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(GameInitData), name: nameof(GameInitData.PrepForMapGen)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(PrepForMapGenPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn_RelationsTracker), name: nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor)), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(type: patchType, name: nameof(SecondaryLovinChanceFactorTranspiler)));
            
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn_RelationsTracker), name: nameof(Pawn_RelationsTracker.CompatibilityWith)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CompatibilityWithPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Faction), name: nameof(Faction.TryMakeInitialRelationsWith)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(TryMakeInitialRelationsWithPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(TraitSet), name: nameof(TraitSet.GainTrait)), prefix: new HarmonyMethod(type: patchType, name: nameof(GainTraitPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(TraderCaravanUtility), name: nameof(TraderCaravanUtility.GetTraderCaravanRole)), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(type: patchType, name: nameof(GetTraderCaravanRoleTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(RestUtility), name: nameof(RestUtility.CanUseBedEver)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CanUseBedEverPostfix)));
            harmony.Patch(original: AccessTools.Property(type: typeof(Building_Bed), name: nameof(Building_Bed.AssigningCandidates)).GetGetMethod(), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(AssigningCandidatesPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(GrammarUtility), name: nameof(GrammarUtility.RulesForPawn), parameters: new []{typeof(string), typeof(Pawn), typeof(Dictionary<string,string>)}), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(RulesForPawnPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(RaceProperties), name: nameof(RaceProperties.CanEverEat), parameters: new[] {typeof(ThingDef)}), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CanEverEat)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Verb_MeleeAttackDamage), name: "DamageInfosToApply"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(DamageInfosToApplyPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnWeaponGenerator), name: nameof(PawnWeaponGenerator.TryGenerateWeaponFor)),
                prefix: new HarmonyMethod(type: patchType, name: nameof(TryGenerateWeaponForPrefix)), postfix: new HarmonyMethod(type: patchType, name: nameof(TryGenerateWeaponForPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnApparelGenerator), name: nameof(PawnApparelGenerator.GenerateStartingApparelFor)),
                prefix: new HarmonyMethod(type: patchType,  name: nameof(GenerateStartingApparelForPrefix)),
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerateStartingApparelForPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGenerator), name: "GenerateInitialHediffs"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerateInitialHediffsPostfix)));
            harmony.Patch(
                original: typeof(HediffSet).GetMethods(bindingAttr: AccessTools.all).First(predicate: mi =>
                    mi.HasAttribute<CompilerGeneratedAttribute>() && mi.ReturnType == typeof(bool) && mi.GetParameters().First().ParameterType == typeof(BodyPartRecord)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(HasHeadPostfix)));
            harmony.Patch(original: AccessTools.Property(type: typeof(HediffSet), name: nameof(HediffSet.HasHead)).GetGetMethod(),
                prefix: new HarmonyMethod(type: patchType, name: nameof(HasHeadPrefix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn_AgeTracker), name: "RecalculateLifeStageIndex"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(RecalculateLifeStageIndexPostfix)));
            harmony.Patch(
                original: typeof(FactionGenerator).GetNestedTypes(bindingAttr: BindingFlags.Instance                                                                      | BindingFlags.NonPublic)
                   .MaxBy(selector: t => t.GetMethods(bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance).Length).GetMethods(bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance)
                   .MaxBy(selector: mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(EnsureRequiredEnemiesPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Faction), name: nameof(Faction.FactionTick)), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(type: patchType, name: nameof(FactionTickTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Designator), name: nameof(Designator.CanDesignateThing)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CanDesignateThingTamePostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(WorkGiver_InteractAnimal), name: "CanInteractWithAnimal"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CanInteractWithAnimalPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnRenderer), name: nameof(PawnRenderer.BaseHeadOffsetAt)), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(BaseHeadOffsetAtPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn_HealthTracker), name: "CheckForStateChange"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(CheckForStateChangePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(ApparelProperties), name: nameof(ApparelProperties.GetInterferingBodyPartGroups)), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(type: patchType, name: nameof(GetInterferingBodyPartGroupsTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGenerator), name: "GenerateGearFor"), prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(GenerateGearForPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn), name: nameof(Pawn.ChangeKind)), prefix: new HarmonyMethod(type: patchType, name: nameof(ChangeKindPrefix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(EditWindow_TweakValues), name: nameof(EditWindow_TweakValues.DoWindowContents)), transpiler: new HarmonyMethod(type: patchType, name: nameof(TweakValuesTranspiler)));
            
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnBioAndNameGenerator), name: "GetBackstoryCategoriesFor"), prefix: null, postfix: null, transpiler: new HarmonyMethod(type: patchType, name: nameof(GetBackstoryCategoriesForTranspiler)));

            DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.ForEach(action: ar =>
            {
                
                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.apparelList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if(!RaceRestrictionSettings.apparelRestrictionDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.apparelRestrictionDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.apparelRestrictionDict[key: thingDef].Add(item: ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whiteApparelList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.apparelWhiteDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.apparelWhiteDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.apparelWhiteDict[key: thingDef].Add(item: ar);
                }


                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.weaponList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.weaponRestrictionDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.weaponRestrictionDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.weaponRestrictionDict[key: thingDef].Add(item: ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whiteWeaponList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.weaponWhiteDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.weaponWhiteDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.weaponWhiteDict[key: thingDef].Add(item: ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.buildingList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.buildingRestrictionDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.buildingRestrictionDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.buildingRestrictionDict[key: thingDef].Add(item: ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whiteBuildingList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.buildingWhiteDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.buildingWhiteDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.buildingWhiteDict[key: thingDef].Add(item: ar);
                }

                foreach (RecipeDef recipeDef in ar.alienRace.raceRestriction.recipeList.Select(selector: DefDatabase<RecipeDef>.GetNamedSilentFail).Where(predicate: rp => rp != null))
                {
                    if (!RaceRestrictionSettings.recipeRestrictionDict.ContainsKey(key: recipeDef))
                        RaceRestrictionSettings.recipeRestrictionDict.Add(key: recipeDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.recipeRestrictionDict[key: recipeDef].Add(item: ar);
                }

                foreach (RecipeDef recipeDef in ar.alienRace.raceRestriction.whiteRecipeList.Select(selector: DefDatabase<RecipeDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.recipeWhiteDict.ContainsKey(key: recipeDef))
                        RaceRestrictionSettings.recipeWhiteDict.Add(key: recipeDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.recipeWhiteDict[key: recipeDef].Add(item: ar);
                }


                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.plantList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.plantRestrictionDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.plantRestrictionDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.plantRestrictionDict[key: thingDef].Add(item: ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whitePlantList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.plantWhiteDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.plantWhiteDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.plantWhiteDict[key: thingDef].Add(item: ar);
                }


                foreach (TraitDef traitDef in ar.alienRace.raceRestriction.traitList.Select(selector: DefDatabase<TraitDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.traitRestrictionDict.ContainsKey(key: traitDef))
                        RaceRestrictionSettings.traitRestrictionDict.Add(key: traitDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.traitRestrictionDict[key: traitDef].Add(item: ar);
                }

                foreach (TraitDef traitDef in ar.alienRace.raceRestriction.whiteTraitList.Select(selector: DefDatabase<TraitDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.traitWhiteDict.ContainsKey(key: traitDef))
                        RaceRestrictionSettings.traitWhiteDict.Add(key: traitDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.traitWhiteDict[key: traitDef].Add(item: ar);
                }


                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.foodList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.foodRestrictionDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.foodRestrictionDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.foodRestrictionDict[key: thingDef].Add(item: ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whiteFoodList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.foodWhiteDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.foodWhiteDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.foodWhiteDict[key: thingDef].Add(item: ar);
                }


                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.petList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.tameRestrictionDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.tameRestrictionDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.tameRestrictionDict[key: thingDef].Add(item: ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whitePetList.Select(selector: DefDatabase<ThingDef>.GetNamedSilentFail).Where(predicate: td => td != null))
                {
                    if (!RaceRestrictionSettings.tameWhiteDict.ContainsKey(key: thingDef))
                        RaceRestrictionSettings.tameWhiteDict.Add(key: thingDef, value: new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.tameWhiteDict[key: thingDef].Add(item: ar);
                }


                ThingCategoryDefOf.CorpsesHumanlike.childThingDefs.Remove(item: ar.race.corpseDef);
                ar.race.corpseDef.thingCategories = new List<ThingCategoryDef> { AlienDefOf.alienCorpseCategory };
                AlienDefOf.alienCorpseCategory.childThingDefs.Add(item: ar.race.corpseDef);
                ar.alienRace.generalSettings.alienPartGenerator.GenerateMeshsAndMeshPools();

                if (ar.alienRace.generalSettings.humanRecipeImport)
                {
                    (ar.recipes ?? (ar.recipes = new List<RecipeDef>())).AddRange(collection: ThingDefOf.Human.recipes.Where(predicate: rd => 
                        !rd.targetsBodyPart || rd.appliedOnFixedBodyParts.NullOrEmpty() || 
                        rd.appliedOnFixedBodyParts.Any(predicate: bpd => ar.race.body.AllParts.Any(predicate: bpr => bpr.def == bpd))));

                    DefDatabase<RecipeDef>.AllDefsListForReading.ForEach(action: rd =>
                    {
                        if (rd.recipeUsers?.Contains(item: ThingDefOf.Human) ?? false)
                            rd.recipeUsers.Add(item: ar);
                        if (!rd.defaultIngredientFilter?.Allows(def: ThingDefOf.Meat_Human) ?? false)
                            rd.defaultIngredientFilter.SetAllow(thingDef: ar.race.meatDef, allow: false);
                    });
                    ar.recipes.RemoveDuplicates();
                }

                ar.alienRace.raceRestriction?.workGiverList?.ForEach(action: wgd =>
                {
                    WorkGiverDef wg = DefDatabase<WorkGiverDef>.GetNamedSilentFail(defName: wgd);
                    if (wg == null)
                        return;
                    harmony.Patch(original: AccessTools.Method(type: wg.giverClass, name: "JobOnThing"), prefix: null,
                        postfix: new HarmonyMethod(type: patchType, name: nameof(GenericJobOnThingPostfix)));
                    MethodInfo hasJobOnThingInfo = AccessTools.Method(type: wg.giverClass, name: "HasJobOnThing");
                    if (hasJobOnThingInfo != null)
                        harmony.Patch(original: hasJobOnThingInfo, prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(GenericHasJobOnThingPostfix)));
                });
            });


            {
                harmony.Patch(original: AccessTools.Method(type: typeof(ILInstruction), name: nameof(ILInstruction.GetSize)), prefix: null, postfix: null,
                    transpiler: new HarmonyMethod(type: patchType, name: nameof(HarmonySizeBugFix)));


                FieldInfo bodyInfo = AccessTools.Field(type: typeof(RaceProperties), name: nameof(RaceProperties.body));


                ILGenerator ilg      = new DynamicMethod(name: "ScanMethod", returnType: typeof(int), parameterTypes: Type.EmptyTypes).GetILGenerator();

                //Full assemblies scan
                foreach (MethodInfo mi in LoadedModManager.RunningMods.Where(predicate: mcp => mcp.LoadedAnyAssembly)
                   .SelectMany(selector: mcp => mcp.assemblies.loadedAssemblies.Where(predicate: ase => ase.GetType(name: "HarmonyInstance", throwOnError: false) != null))
                   .Concat(rhs: typeof(LogEntry).Assembly).SelectMany(selector: ase => ase.GetTypes()).
                    //SelectMany(t => t.GetNestedTypes(AccessTools.all).Concat(t)).
                    Where(predicate: t => (!t.IsAbstract || t.IsSealed) && !typeof(Delegate).IsAssignableFrom(c: t) && !t.IsGenericType).SelectMany(selector: t =>
                        t.GetMethods(bindingAttr: AccessTools.all)
                           .Concat(second: t.GetProperties(bindingAttr: AccessTools.all).SelectMany(selector: pi =>
                                new List<MethodInfo> {pi.GetGetMethod(nonPublic: true), pi.GetGetMethod(nonPublic: false), pi.GetSetMethod(nonPublic: true), pi.GetSetMethod(nonPublic: false)}))
                           .Where(predicate: mi => mi != null && !mi.IsAbstract && mi.DeclaringType == t && !mi.IsGenericMethod))
                ) //.Select(mi => mi.IsGenericMethod ? mi.MakeGenericMethod(mi.GetGenericArguments()) : mi))
                {
                    List<ILInstruction> instructions = PatchFunctions.GetInstructions(generator: ilg, method: mi);
                    if (instructions.Any(predicate: il => il.operand == bodyInfo))
                        harmony.Patch(original: mi, prefix: null, postfix: null, transpiler: new HarmonyMethod(type: patchType, name: nameof(BodyReferenceTranspiler)));
                }


                //PawnRenderer Posture scan
                MethodInfo postureInfo = AccessTools.Method(type: typeof(PawnUtility), name: nameof(PawnUtility.GetPosture));

                foreach (MethodInfo mi in typeof(PawnRenderer).GetMethods(bindingAttr: AccessTools.all).Concat(second: typeof(PawnRenderer).GetProperties(bindingAttr: AccessTools.all)
                       .SelectMany(selector: pi =>
                            new List<MethodInfo> {pi.GetGetMethod(nonPublic: true), pi.GetGetMethod(nonPublic: false), pi.GetSetMethod(nonPublic: true), pi.GetSetMethod(nonPublic: false)}))
                   .Where(predicate: mi => mi != null && mi.DeclaringType == typeof(PawnRenderer) && !mi.IsGenericMethod))
                {
                    List<ILInstruction> instructions = PatchFunctions.GetInstructions(generator: ilg, method: mi);

                    if (instructions.Any(predicate: il => il.operand == postureInfo))
                        harmony.Patch(original: mi, prefix: null, postfix: null, transpiler: new HarmonyMethod(type: patchType, name: nameof(PostureTranspiler)));
                }
            }
            
            Log.Message(text:
                $"Alien race successfully completed {harmony.GetPatchedMethods().Select(selector: mb => harmony.GetPatchInfo(method: mb)).SelectMany(selector: p => p.Prefixes.Concat(second: p.Postfixes).Concat(second: p.Transpilers)).Count(predicate: p => p.owner == harmony.Id)} patches with harmony.");
            DefDatabase<HairDef>.GetNamed(defName: "Shaved").hairTags.Add(item: "alienNoHair"); // needed because..... the original idea doesn't work and I spend enough time finding a good solution

            foreach (BackstoryDef bd in DefDatabase<BackstoryDef>.AllDefs) BackstoryDef.UpdateTranslateableFields(bs: bd);
        }

        public static IEnumerable<CodeInstruction> GetBackstoryCategoriesForTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int index = 0; index < instructionList.Count; index++)
            {
                CodeInstruction instruction = instructionList[index: index];

                if (instruction.opcode == OpCodes.Ldc_I4_0)
                {
                    Label label = ilg.DefineLabel();

                    object breakLabel = instructionList[index: index - 1].operand;

                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(Pawn), name: nameof(Pawn.def)));
                    yield return new CodeInstruction(opcode: OpCodes.Isinst, operand: typeof(ThingDef_AlienRace));
                    yield return new CodeInstruction(opcode: OpCodes.Brfalse, operand: label);
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(Pawn), name: nameof(Pawn.def)));
                    yield return new CodeInstruction(opcode: OpCodes.Castclass, operand: typeof(ThingDef_AlienRace));
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(ThingDef_AlienRace), name: nameof(ThingDef_AlienRace.alienRace)));
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(ThingDef_AlienRace.AlienSettings), name: nameof(ThingDef_AlienRace.AlienSettings.generalSettings)));
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(GeneralSettings), name: nameof(GeneralSettings.useOnlyPawnkindBackstories)));
                    yield return new CodeInstruction(opcode: OpCodes.Brtrue, operand: breakLabel);
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(Pawn), name: nameof(Pawn.kindDef)));
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: typeof(PawnKindDef), name: nameof(PawnKindDef.GetModExtension)).MakeGenericMethod(typeof(Info)));
                    yield return new CodeInstruction(opcode: OpCodes.Brfalse, operand: label);
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(Pawn), name: nameof(Pawn.kindDef)));
                    yield return new CodeInstruction(opcode: OpCodes.Call,  operand: AccessTools.Method(type: typeof(PawnKindDef), name: nameof(PawnKindDef.GetModExtension)).MakeGenericMethod(typeof(Info)));
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(Info), name: nameof(Info.useOnlyPawnkindBackstories)));
                    yield return new CodeInstruction(opcode: OpCodes.Brtrue, operand: breakLabel);

                    instruction.labels.Add(item: label);
                }

                yield return instruction;
            }
        }

        public static IEnumerable<CodeInstruction> TweakValuesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo endScrollInfo = AccessTools.Method(type: typeof(Widgets), name: nameof(Widgets.EndScrollView));

            MethodInfo countInfo = AccessTools.Property(
                type: AccessTools.Field(type: typeof(EditWindow_TweakValues), name: "tweakValueFields").FieldType, 
                name: nameof(List<Graphic_Random>.Count)).GetGetMethod();

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand == endScrollInfo)
                {
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_2) { labels = instruction.labels.ListFullCopy() };
                    instruction.labels.Clear();
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_3);
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_S, operand: 4);
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_S, operand: 5);
                    yield return new CodeInstruction(opcode: OpCodes.Call, 
                        operand: AccessTools.Method(type: patchType, name: nameof(TweakValuesInstanceBased)));
                }

                yield return instruction;

                if (instruction.operand == countInfo)
                {
                    yield return new CodeInstruction(opcode: OpCodes.Ldc_I4, operand:
                        DefDatabase<ThingDef_AlienRace>.AllDefs.SelectMany(selector: ar =>
                            ar.alienRace.generalSettings.alienPartGenerator.bodyAddons).SelectMany(selector: ba =>
                            new List<AlienPartGenerator.RotationOffset>
                            {
                                ba.offsets.east,
                                ba.offsets.west,
                                ba.offsets.north,
                                ba.offsets.south
                            }).Sum(selector: ro => (ro.bodyTypes?.Count ?? 0) * 2 + (ro.portraitBodyTypes?.Count ?? 0) * 2 + 
                                                   (ro.crownTypes?.Count ?? 0) * 2 + (ro.portraitCrownTypes?.Count ?? 0) * 2) + 1);
                    yield return new CodeInstruction(opcode: OpCodes.Add);
                }
            }
        }
        
        public static void TweakValuesInstanceBased(Rect rect2, Rect rect3, Rect rect4, Rect rect5)
        {
            void NextLine()
            {
                rect2.y += rect2.height;
                rect3.y += rect2.height;
                rect4.y += rect2.height;
                rect5.y += rect2.height;
            }
            NextLine();

            foreach (ThingDef_AlienRace ar in DefDatabase<ThingDef_AlienRace>.AllDefs)
            {
                string label2 = ar.LabelCap;
                foreach (AlienPartGenerator.BodyAddon ba in ar.alienRace.generalSettings.alienPartGenerator.bodyAddons)
                {
                    string label3Addons = $"{Path.GetFileName(path: ba.path)}.";

                    List<AlienPartGenerator.RotationOffset> rotationOffsets = new List<AlienPartGenerator.RotationOffset>
                    {
                        ba.offsets.north,
                        ba.offsets.south,
                        ba.offsets.west,
                        ba.offsets.east                        
                    };
                    for (int i = 0; i < rotationOffsets.Count; i++)
                    {
                        string label3Rotation;
                        AlienPartGenerator.RotationOffset ro = rotationOffsets[index: i];
                        switch (i)
                        {
                            case 0:
                                label3Rotation = "north.";
                                break;
                            case 1:
                                label3Rotation = "south.";
                                break;
                            case 2:
                                label3Rotation = "west.";
                                break;
                            default:
                                label3Rotation = "east.";
                                break;
                        }

                        foreach (AlienPartGenerator.BodyTypeOffset bodyTypeOffset in ro.bodyTypes)
                        {
                            string label3Type = bodyTypeOffset.bodyType.defName + ".";
                            Vector2 offset = bodyTypeOffset.offset;
                            float offsetX = offset.x;
                            float offsetY = offset.y;


                            float WriteLine(float value, bool x)
                            {
                                Widgets.Label(rect: rect2, label: label2);
                                Widgets.Label(rect: rect3, label: label3Addons + label3Rotation + label3Type + (x ? "x" : "y"));


                                float num = Widgets.HorizontalSlider(rect: rect5, value: value, leftValue: -1, rightValue: 1);

                                if (Math.Abs(value: num - value) > 0.0001)
                                {
                                    GUI.color = Color.red;
                                    Widgets.Label(rect: rect4, label: $"{value} -> {num}");
                                    GUI.color = Color.white;
                                    if (Widgets.ButtonInvisible(butRect: rect5))
                                        bodyTypeOffset.offset.x = num;
                                }
                                else
                                {
                                    Widgets.Label(rect: rect4, label: value.ToString(provider: CultureInfo.InvariantCulture));
                                }
                                return num;
                            }

                            bodyTypeOffset.offset.x = WriteLine(value: offsetX, x: true);
                            NextLine();
                            bodyTypeOffset.offset.y = WriteLine(value: offsetY, x: false);
                            NextLine();
                        }
                    }
                }
            }
        }

        public static IEnumerable<CodeInstruction> HarmonySizeBugFix(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Castclass)
                    instruction.operand = typeof(IEnumerable<object>);
                yield return instruction;
            }
        }

        public static IEnumerable<CodeInstruction> PostureTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo postureInfo = AccessTools.Method(type: typeof(PawnUtility), name: nameof(PawnUtility.GetPosture));

            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            foreach (CodeInstruction instruction in codeInstructions)
                if (instruction.opcode == OpCodes.Call && instruction.operand == postureInfo)
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: patchType, name: nameof(PostureTweak)));
                else
                    yield return instruction;
        }

        public static PawnPosture PostureTweak(Pawn pawn)
        {
            PawnPosture posture = pawn.GetPosture();

            if (posture != PawnPosture.Standing && pawn.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.generalSettings.canLayDown &&
                !(pawn.CurrentBed()?.def.defName.EqualsIgnoreCase(B: "ET_Bed") ?? false))
                return PawnPosture.Standing;
            return posture;
        }

        public static IEnumerable<CodeInstruction> BodyReferenceTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase mb)
        {
            FieldInfo bodyInfo  = AccessTools.Field(type: typeof(RaceProperties), name: nameof(RaceProperties.body));
            FieldInfo propsInfo = AccessTools.Field(type: typeof(ThingDef),       name: nameof(ThingDef.race));
            FieldInfo defInfo = AccessTools.Field(type: typeof(Thing), name: nameof(Thing.def));
            {
                List<CodeInstruction> instructionList = instructions.ToList();
                for (int i = 0; i < instructionList.Count; i++)
                {
                    CodeInstruction instruction = instructionList[index: i];

                    if (i < instructionList.Count - 2 && instructionList[index: i + 2].operand == bodyInfo && instructionList[index: i + 1].operand == propsInfo && instruction.operand == defInfo)
                    {
                        instruction =  new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: patchType, name: nameof(ReplacedBody)));
                        i           += 2;
                    }

                    if (i < instructionList.Count - 1 && instructionList[index: i + 1].operand == bodyInfo && instruction.operand == defInfo)
                    {
                        instruction = new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: patchType, name: nameof(ReplacedBody)));
                        i++;
                    }

                    yield return instruction;
                }
            }
        }

        public static BodyDef ReplacedBody(Pawn pawn) =>
            pawn.def is ThingDef_AlienRace ? (pawn.ageTracker.CurLifeStageRace as LifeStageAgeAlien)?.body ?? pawn.RaceProps.body : pawn.RaceProps.body;

        public static bool ChangeKindPrefix(Pawn __instance, PawnKindDef newKindDef) =>
            __instance.kindDef == PawnKindDefOf.WildMan || newKindDef == PawnKindDefOf.WildMan;

        public static void GenerateGearForPostfix(Pawn pawn) =>
            pawn.story?.AllBackstories?.Select(selector: bs => DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: bs.identifier)).Where(predicate: bs => bs != null)
               .SelectMany(selector: bd => bd.forcedItems).Concat(second: bioReference?.forcedItems ?? new List<ThingDefCountRangeClass>(capacity: 0)).Do(action: tdcrc =>
               {
                    for(int i = 0; i < tdcrc.countRange.RandomInRange; i++)
                        pawn.inventory?.TryAddItemNotForSale(item: ThingMaker.MakeThing(def: tdcrc.thingDef, stuff: GenStuff.RandomStuffFor(td: tdcrc.thingDef)));
               });
               

        //Zorba.....
        public static IEnumerable<CodeInstruction> GetInterferingBodyPartGroupsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool done = false;

            foreach (CodeInstruction instruction in instructions)
                if (!done && instruction.opcode == OpCodes.Call)
                {
                    yield return new CodeInstruction(opcode: OpCodes.Call,
                        operand: AccessTools.Property(type: typeof(DefDatabase<BodyDef>), name: nameof(DefDatabase<BodyDef>.DefCount)).GetGetMethod());
                    done = true;
                }
                else
                {
                    yield return instruction;
                }
        }

        public static void CheckForStateChangePostfix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>();
            if (Current.ProgramState == ProgramState.Playing && pawn.Spawned && pawn.def is ThingDef_AlienRace)
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public static void BaseHeadOffsetAtPostfix(PawnRenderer __instance, ref Vector3 __result)
        {
            Pawn    pawn   = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>();
            Rot4 rotation = pawn.Rotation;
            Vector2 offset = (pawn.def as ThingDef_AlienRace)?.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: pawn.ageTracker.CurLifeStage).headOffset ?? Vector2.zero;
            switch (rotation.AsInt)
			{
			case 0:
                __result.z += offset.y;
                break;
			case 1:
                __result.x += offset.x;
                __result.z += offset.y;
                break;
			case 2:
                __result.z += offset.y;
                break;
			case 3:
                __result.x += -offset.x;
                __result.z += offset.y;
                break;
			default:
				Log.Error("BaseHeadOffsetAtPostfix error in " + pawn, false);
                break;
			}
        }

        public static void CanInteractWithAnimalPostfix(ref bool __result, Pawn pawn, Pawn animal) =>
            __result = __result && RaceRestrictionSettings.CanTame(pet: animal.def, race: pawn.def);

        public static void CanDesignateThingTamePostfix(Designator __instance, ref bool __result, Thing t)
        {
            if (!__result || !(__instance is Designator_Build)) return;

            __result = colonistRaces.Any(predicate: td => RaceRestrictionSettings.CanTame(pet: t.def, race: td));
        }

        public static IEnumerable<CodeInstruction> FactionTickTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode != OpCodes.Beq) continue;
                yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                yield return new CodeInstruction(opcode: OpCodes.Call,    operand: AccessTools.Method(type: patchType, name: nameof(FactionTickFactionRelationCheck)));
                yield return new CodeInstruction(opcode: OpCodes.Brfalse, operand: instruction.operand);
            }
        }

        private static bool FactionTickFactionRelationCheck(Faction f)
        {
            FactionDef player = Faction.OfPlayerSilentFail?.def ?? Find.GameInitData.playerFaction.def;
            return !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(predicate: ar =>
                       f.def?.basicMemberKind?.race == ar &&
                       (ar?.alienRace.generalSettings?.factionRelations?.Any(predicate: frs => frs.factions?.Contains(item: player.defName) ?? false) ?? false) ||
                       player.basicMemberKind?.race == ar &&
                       (ar?.alienRace.generalSettings?.factionRelations?.Any(predicate: frs => frs.factions?.Contains(item: f.def.defName) ?? false) ?? false));
        }

        public static void EnsureRequiredEnemiesPostfix(ref bool __result, Faction f) => __result = __result ||
                                                                                                    !FactionTickFactionRelationCheck(f: f);

        public static void RecalculateLifeStageIndexPostfix(Pawn_AgeTracker __instance)
        {
            Pawn pawn;
            if (Current.ProgramState == ProgramState.Playing && (pawn = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>()).def is ThingDef_AlienRace &&
                pawn.Drawer.renderer.graphics.AllResolved)
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public static void HasHeadPrefix(HediffSet __instance) =>
            headPawnDef = (__instance.pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.headBodyPartDef;

        private static BodyPartDef headPawnDef;

        public static void HasHeadPostfix(BodyPartRecord x, ref bool __result) =>
            __result = headPawnDef != null ? x.def == headPawnDef : __result;

        public static void GenerateInitialHediffsPostfix(Pawn pawn) =>
            pawn.story?.AllBackstories?.Select(selector: bs => DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: bs.identifier)).Where(predicate: bd => bd != null)
               .SelectMany(selector: bd => bd.forcedHediffs).Concat(second: bioReference?.forcedHediffs ?? new List<string>(capacity: 0)).Select(selector: DefDatabase<HediffDef>.GetNamedSilentFail)
               .ToList().ForEach(action: hd =>
                {
                    BodyPartRecord bodyPartRecord = null;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(predicate: rd => rd.addsHediff == hd)?.appliedOnFixedBodyParts.SelectMany(selector: bpd =>
                            pawn.health.hediffSet.GetNotMissingParts().Where(predicate: bpr => bpr.def == bpd && !pawn.health.hediffSet.hediffs.Any(predicate: h => h.def == hd && h.Part == bpr)))
                       .TryRandomElement(result: out bodyPartRecord);
                    pawn.health.AddHediff(def: hd, part: bodyPartRecord);
                });

        public static void GenerateStartingApparelForPostfix() =>
            Traverse.Create(type: typeof(PawnApparelGenerator)).Field(name: "allApparelPairs").GetValue<List<ThingStuffPair>>().AddRange(collection: apparelList);

        private static HashSet<ThingStuffPair> apparelList;

        public static void GenerateStartingApparelForPrefix(Pawn pawn)
        {
            Traverse apparelInfo = Traverse.Create(type: typeof(PawnApparelGenerator)).Field(name: "allApparelPairs");

            apparelList = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in apparelInfo.GetValue<List<ThingStuffPair>>().ListFullCopy())
            {
                ThingDef equipment = pair.thing;
                if (!RaceRestrictionSettings.CanWear(apparel: equipment, race: pawn.def))
                    apparelList.Add(item: pair);
            }

            apparelInfo.GetValue<List<ThingStuffPair>>().RemoveAll(match: tsp => apparelList.Contains(item: tsp));
        }

        public static void TryGenerateWeaponForPostfix() =>
            Traverse.Create(type: typeof(PawnWeaponGenerator)).Field(name: "allWeaponPairs").GetValue<List<ThingStuffPair>>().AddRange(collection: weaponList);

        private static HashSet<ThingStuffPair> weaponList;

        public static void TryGenerateWeaponForPrefix(Pawn pawn)
        {
            Traverse weaponInfo = Traverse.Create(type: typeof(PawnWeaponGenerator)).Field(name: "allWeaponPairs");
            weaponList = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in weaponInfo.GetValue<List<ThingStuffPair>>().ListFullCopy())
            {
                ThingDef equipment = pair.thing;
                if (!RaceRestrictionSettings.CanEquip(weapon: equipment, race: pawn.def))
                    weaponList.Add(item: pair);
            }

            weaponInfo.GetValue<List<ThingStuffPair>>().RemoveAll(match: tsp => weaponList.Contains(item: tsp));
        }

        public static void DamageInfosToApplyPostfix(Verb __instance, ref IEnumerable<DamageInfo> __result)
        {
            if (__instance.CasterIsPawn && __instance.CasterPawn.def is ThingDef_AlienRace alienProps && __instance.CasterPawn.CurJob.def == JobDefOf.SocialFight)
                __result = __result.Select(selector: di =>
                    new DamageInfo(def: di.Def, amount: Math.Min(val1: di.Amount, val2: alienProps.alienRace.generalSettings.maxDamageForSocialfight), angle: di.Angle, instigator: di.Instigator,
                        hitPart: di.HitPart, weapon: di.Weapon, category: di.Category));
        }

        public static void CanEverEat(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (!__instance.Humanlike) return;
            ThingDef eater = new List<ThingDef>(collection: DefDatabase<ThingDef>.AllDefsListForReading).Concat(
                second: new List<ThingDef_AlienRace>(collection: DefDatabase<ThingDef_AlienRace>.AllDefsListForReading).Cast<ThingDef>()).First(predicate: td => td.race == __instance);

            __result = RaceRestrictionSettings.CanEat(food: t, race: eater) && __result;
        }
        
        public static IEnumerable<Rule> RulesForPawnPostfix(IEnumerable<Rule> __result, Pawn pawn, string pawnSymbol) =>
            __result.Add(item: new Rule_String(keyword: pawnSymbol + "_alienRace", output: pawn.def.LabelCap));

        public static IEnumerable<CodeInstruction> GenerateTraitsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            MethodInfo            defListInfo     = AccessTools.Property(type: typeof(DefDatabase<TraitDef>), name: nameof(DefDatabase<TraitDef>.AllDefsListForReading)).GetGetMethod();
            MethodInfo            validatorInfo   = AccessTools.Method(type: patchType, name: nameof(GenerateTraitsValidator));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[index: i];

                if (instruction.opcode == OpCodes.Call && instruction.operand == defListInfo)
                {
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    instruction.operand = validatorInfo;

                    for (int x = 0; x < 4; x++)
                        instructionList.RemoveAt(index: i + 1);
                }

                yield return instruction;
            }
        }

        public static TraitDef GenerateTraitsValidator(Pawn p)
        {
            IEnumerable<TraitDef> defs       = DefDatabase<TraitDef>.AllDefs;
            defs = defs.Where(predicate: tr => RaceRestrictionSettings.CanGetTrait(trait: tr, race: p.def));
            return defs.RandomElementByWeight(weightSelector: tr => tr.GetGenderSpecificCommonality(gender: p.gender));
        }

        public static void AssigningCandidatesPostfix(ref IEnumerable<Pawn> __result, Building_Bed __instance) =>
            __result = __result.Where(predicate: p => RestUtility.CanUseBedEver(p: p, bedDef: __instance.def));

        public static void CanUseBedEverPostfix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            if (__result)
                __result = p.def is ThingDef_AlienRace alienProps && (alienProps.alienRace.generalSettings.validBeds?.Contains(item: bedDef.defName) ?? false) ||
                           !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(predicate: td => td.alienRace.generalSettings.validBeds?.Contains(item: bedDef.defName) ?? false);
        }

//        public static void CanWearTogetherPostfix(ThingDef A, ThingDef b, bool __result)
//        {
//            /*
//            if (__result)
//            {
//                Log.Message(A.defName + " - " + B.defName);
//
//                bool flag = false;
//                for (int i = 0; i < A.apparel.layers.Count; i++)
//                {
//                    for (int j = 0; j < B.apparel.layers.Count; j++)
//                    {
//                        if (A.apparel.layers[i] == B.apparel.layers[j])
//                        {
//                            flag = true;
//                        }
//                        if (flag)
//                        {
//                            break;
//                        }
//                    }
//                    if (flag)
//                    {
//                        break;
//                    }
//                }
//                if (!flag)
//                    Log.Message("You are out");
//                else
//                {
//                    for (int k = 0; k < A.apparel.bodyPartGroups.Count; k++)
//                    {
//                        for (int l = 0; l < B.apparel.bodyPartGroups.Count; l++)
//                        {
//                            BodyPartGroupDef item = A.apparel.bodyPartGroups[k];
//                            BodyPartGroupDef item2 = B.apparel.bodyPartGroups[l];
//                            for (int m = 0; m < BodyDefOf.Human.AllParts.Count; m++)
//                            {
//                                BodyPartRecord bodyPartRecord = BodyDefOf.Human.AllParts[m];
//                                if (bodyPartRecord.groups.Contains(item) && bodyPartRecord.groups.Contains(item2))
//                                {
//                                    Log.Message("you are in");
//                                }
//                            }
//                        }
//                    }
//                }
//            }*/
//        }

        public static IEnumerable<CodeInstruction> GetTraderCaravanRoleTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            MethodInfo traderRoleInfo = AccessTools.Method(type: patchType, name: nameof(GetTraderCaravanRoleInfix));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_3)
                {
                    Label jumpToEnd = il.DefineLabel();
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0) {labels = instruction.labels.ListFullCopy()};
                    instruction.labels.Clear();
                    yield return new CodeInstruction(opcode: OpCodes.Call,      operand: traderRoleInfo);
                    yield return new CodeInstruction(opcode: OpCodes.Brfalse_S, operand: jumpToEnd);
                    yield return new CodeInstruction(opcode: OpCodes.Ldc_I4_4);
                    yield return new CodeInstruction(opcode: OpCodes.Ret);
                    yield return new CodeInstruction(opcode: OpCodes.Nop) {labels = new List<Label> {jumpToEnd}};
                }

                yield return instruction;
            }
        }

        private static bool GetTraderCaravanRoleInfix(Pawn p) => 
            p.def is ThingDef_AlienRace && 
                DefDatabase<RaceSettings>.AllDefs.Any(predicate: rs => rs.pawnKindSettings.alienslavekinds.Any(predicate: pke => pke.kindDefs.Contains(item: p.kindDef.defName)));

        public static bool GetGenderSpecificLabelPrefix(Pawn pawn, ref string __result, PawnRelationDef __instance)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                RelationRenamer ren = alienProps.alienRace.relationSettings.renamer?.FirstOrDefault(predicate: rn => rn.relation.EqualsIgnoreCase(B: __instance.defName));
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

            if (!pawn.RaceProps.Humanlike || pawn.RaceProps.hasGenders || !(pawn.def is ThingDef_AlienRace)) return true;

            List<KeyValuePair<Pawn, PawnRelationDef>> list                  = new List<KeyValuePair<Pawn, PawnRelationDef>>();
            List<PawnRelationDef>                     allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
            List<Pawn> enumerable = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                                     where x.def == pawn.def
                                     select x).ToList();

            enumerable.ForEach(action: current =>
            {
                if (current.Discarded)
                    Log.Warning(text: string.Concat(args: new object[]
                    {
                        "Warning during generating pawn relations for ",
                        pawn,
                        ": Pawn ",
                        current,
                        " is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything."
                    }));
                else
                    allDefsListForReading.ForEach(action: relationDef =>
                    {
                        if (relationDef.generationChanceFactor > 0f) list.Add(item: new KeyValuePair<Pawn, PawnRelationDef>(key: current, value: relationDef));
                    });
            });

            KeyValuePair<Pawn, PawnRelationDef> keyValuePair = list.RandomElementByWeightWithDefault(weightSelector: x =>
                !x.Value.familyByBloodRelation ? 0f : GenerationChanceGenderless(relationDef: x.Value, pawn: pawn, current: x.Key, request: localReq), defaultValueWeight: 82f);

            Pawn other = keyValuePair.Key;
            if (other != null) CreateRelationGenderless(relationDef: keyValuePair.Value, pawn: pawn, other: other);
            KeyValuePair<Pawn, PawnRelationDef> keyValuePair2 = list.RandomElementByWeightWithDefault(weightSelector: x =>
                x.Value.familyByBloodRelation ? 0f : GenerationChanceGenderless(relationDef: x.Value, pawn: pawn, current: x.Key, request: localReq), defaultValueWeight: 82f);
            other = keyValuePair2.Key;
            if (other != null) CreateRelationGenderless(relationDef: keyValuePair2.Value, pawn: pawn, other: other);
            return false;
        }

        private static float GenerationChanceGenderless(PawnRelationDef relationDef, Pawn pawn, Pawn current, PawnGenerationRequest request)
        {
            float generationChance = relationDef.generationChanceFactor;
            float lifeExpectancy   = pawn.RaceProps.lifeExpectancy;

            if (relationDef == PawnRelationDefOf.Child)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(child: current, parent1: pawn,
                    parent2: current.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Parent, predicate: p => p != pawn));
                GenerationChanceChildPostfix(__result: ref generationChance, generated: pawn, other: current);
            }
            else if (relationDef == PawnRelationDefOf.ExLover)
            {
                generationChance = 0.5f;
                GenerationChanceExLoverPostfix(__result: ref generationChance, generated: pawn, other: current);
            }
            else if (relationDef == PawnRelationDefOf.ExSpouse)
            {
                generationChance = 0.5f;
                GenerationChanceExSpousePostfix(__result: ref generationChance, generated: pawn, other: current);
            }
            else if (relationDef == PawnRelationDefOf.Fiance)
            {
                generationChance =
                    Mathf.Clamp(value: GenMath.LerpDouble(inFrom: lifeExpectancy / 1.6f, inTo: lifeExpectancy, outFrom: 1f, outTo: 0.01f, x: pawn.ageTracker.AgeBiologicalYearsFloat), min: 0.01f,
                        max: 1f) *
                    Mathf.Clamp(value: GenMath.LerpDouble(inFrom: lifeExpectancy / 1.6f, inTo: lifeExpectancy, outFrom: 1f, outTo: 0.01f, x: current.ageTracker.AgeBiologicalYearsFloat), min: 0.01f,
                        max: 1f);
                GenerationChanceFiancePostfix(__result: ref generationChance, generated: pawn, other: current);
            }
            else if (relationDef == PawnRelationDefOf.Lover)
            {
                generationChance = 0.5f;
                GenerationChanceLoverPostfix(__result: ref generationChance, generated: pawn, other: current);
            }
            else if (relationDef == PawnRelationDefOf.Parent)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(child: current, parent1: pawn,
                    parent2: current.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Parent, predicate: p => p != pawn));
                GenerationChanceParentPostfix(__result: ref generationChance, generated: pawn, other: current);
            }
            else if (relationDef == PawnRelationDefOf.Sibling)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(child: current, parent1: pawn,
                    parent2: current.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Parent, predicate: p => p != pawn));
                generationChance *= 0.65f;
                GenerationChanceSiblingPostfix(__result: ref generationChance, generated: pawn, other: current);
            }
            else if (relationDef == PawnRelationDefOf.Spouse)
            {
                generationChance = 0.5f;
                GenerationChanceSpousePostfix(__result: ref generationChance, generated: pawn, other: current);
            }

            return generationChance * relationDef.Worker.BaseGenerationChanceFactor(generated: pawn, other: current, request: request);
        }

        private static void CreateRelationGenderless(PawnRelationDef relationDef, Pawn pawn, Pawn other)
        {
            if (relationDef == PawnRelationDefOf.Child)
            {
                Pawn parent = other.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Parent);
                if (parent != null)
                    pawn.relations.AddDirectRelation(def: LovePartnerRelationUtility.HasAnyLovePartner(pawn: parent) || Rand.Value > 0.8f ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Spouse,
                        otherPawn: parent);

                other.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: pawn);
            }

            if (relationDef == PawnRelationDefOf.ExLover)
            {
                if (!pawn.GetRelations(other: other).Contains(value: PawnRelationDefOf.ExLover)) pawn.relations.AddDirectRelation(def: PawnRelationDefOf.ExLover, otherPawn: other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (p.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) < 2 && Rand.Value < 0.35)
                        p.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: pawn);
                });
            }

            if (relationDef == PawnRelationDefOf.ExSpouse)
            {
                pawn.relations.AddDirectRelation(def: PawnRelationDefOf.ExSpouse, otherPawn: other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (p.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) < 2 && Rand.Value < 1)
                        p.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: pawn);
                });
            }

            if (relationDef == PawnRelationDefOf.Fiance)
            {
                pawn.relations.AddDirectRelation(def: PawnRelationDefOf.Fiance, otherPawn: other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (p.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) < 2 && Rand.Value < 0.7)
                        p.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: pawn);
                });
            }

            if (relationDef == PawnRelationDefOf.Lover)
            {
                pawn.relations.AddDirectRelation(def: PawnRelationDefOf.Lover, otherPawn: other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (p.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) < 2 && Rand.Value < 0.35f)
                        p.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: pawn);
                });
            }

            if (relationDef == PawnRelationDefOf.Parent)
            {
                Pawn parent = other.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Parent);
                if (parent != null && pawn != parent && !pawn.GetRelations(other: parent).Contains(value: PawnRelationDefOf.ExLover))
                    pawn.relations.AddDirectRelation(def: LovePartnerRelationUtility.HasAnyLovePartner(pawn: parent) || Rand.Value > 0.8f ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Spouse,
                        otherPawn: parent);

                pawn.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: other);
            }

            if (relationDef == PawnRelationDefOf.Sibling)
            {
                Pawn                     parent  = other.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Parent);
                List<DirectPawnRelation> dprs    = other.relations.DirectRelations.Where(predicate: dpr => dpr.def == PawnRelationDefOf.Parent && dpr.otherPawn != parent).ToList();
                Pawn                     parent2 = dprs.NullOrEmpty() ? null : dprs.First().otherPawn;

                if (parent == null)
                {
                    parent = PawnGenerator.GeneratePawn(kindDef: other.kindDef,
                        faction: Find.FactionManager.FirstFactionOfDef(facDef: other.kindDef.defaultFactionType) ?? Find.FactionManager.AllFactions.RandomElement());
                    if (!other.GetRelations(other: parent).Contains(value: PawnRelationDefOf.Parent)) other.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: parent);
                }

                if (parent2 == null)
                {
                    parent2 = PawnGenerator.GeneratePawn(kindDef: other.kindDef,
                        faction: Find.FactionManager.FirstFactionOfDef(facDef: other.kindDef.defaultFactionType) ?? Find.FactionManager.AllFactions.RandomElement());
                    if (!other.GetRelations(other: parent2).Contains(value: PawnRelationDefOf.Parent)) other.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: parent2);
                }

                if (!parent.GetRelations(other: parent2).Any(predicate: prd => prd == PawnRelationDefOf.ExLover || prd == PawnRelationDefOf.Lover))
                    parent.relations.AddDirectRelation(def: LovePartnerRelationUtility.HasAnyLovePartner(pawn: parent) || Rand.Value > 0.8 ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Lover,
                        otherPawn: parent2);

                if (!pawn.GetRelations(other: parent).Contains(value: PawnRelationDefOf.Parent) && pawn != parent) pawn.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: parent);

                if (!pawn.GetRelations(other: parent2).Contains(value: PawnRelationDefOf.Parent) && pawn != parent2)
                    pawn.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: parent2);
            }

            if (relationDef != PawnRelationDefOf.Spouse) return;
            {
                if (!pawn.GetRelations(other: other).Contains(value: PawnRelationDefOf.Spouse)) pawn.relations.AddDirectRelation(def: PawnRelationDefOf.Spouse, otherPawn: other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (pawn != p && p.relations.DirectRelations.Count(predicate: dpr => dpr.def                == PawnRelationDefOf.Parent) < 2     &&
                        p.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Parent, predicate: x => x == pawn)                     == null &&
                        Rand.Value                                                                                                           < 0.7)
                        p.relations.AddDirectRelation(def: PawnRelationDefOf.Parent, otherPawn: pawn);
                });
            }

        }

        private static float ChanceOfBecomingGenderlessChildOf(Pawn child, Pawn parent1, Pawn parent2)
        {
            if (child == null || parent1 == null || !(parent2 == null || child.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) > 1))
                return 0f;
            if (parent2 != null && !LovePartnerRelationUtility.LovePartnerRelationExists(first: parent1, second: parent2) &&
                !LovePartnerRelationUtility.ExLovePartnerRelationExists(first: parent1, second: parent2))
                return 0f;

            float    num2          = 1f;
            float    num3          = 1f;
            Traverse childRelation = Traverse.Create(type: typeof(ChildRelationUtility));

            float num = childRelation.Method("GetParentAgeFactor", parent1, child, parent1.RaceProps.lifeExpectancy / 5f, parent1.RaceProps.lifeExpectancy / 2.5f,
                parent1.RaceProps.lifeExpectancy                                                                    / 1.6f).GetValue<float>();
            if (Math.Abs(value: num) < 0.001f) return 0f;
            if (parent2 != null)
            {
                num2 = childRelation.Method("GetParentAgeFactor", parent2, child, parent1.RaceProps.lifeExpectancy / 5f, parent1.RaceProps.lifeExpectancy / 2.5f,
                    parent1.RaceProps.lifeExpectancy                                                               / 1.6f).GetValue<float>();
                if (Math.Abs(value: num2) < 0.001f) return 0f;
                num3 = 1f;
            }

            float num6                                                                      = 1f;
            Pawn  firstDirectRelationPawn                                                   = parent2?.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Spouse);
            if (firstDirectRelationPawn != null && firstDirectRelationPawn != parent2) num6 *= 0.15f;
            if (parent2 == null) return num * num2 * num3 * num6;
            Pawn firstDirectRelationPawn2                                                     = parent2.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Spouse);
            if (firstDirectRelationPawn2 != null && firstDirectRelationPawn2 != parent2) num6 *= 0.15f;
            return num * num2 * num3 * num6;

        }

        public static bool GainTraitPrefix(Trait trait, TraitSet __instance)
        {
            if (!(Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>().def is ThingDef_AlienRace alienProps)) return true;
            if (alienProps.alienRace.generalSettings.disallowedTraits?.Contains(item: trait.def.defName) ?? false)
                return false;

            AlienTraitEntry ate = alienProps.alienRace.generalSettings.forcedRaceTraitEntries?.FirstOrDefault(predicate: at => at.defName.EqualsIgnoreCase(B: trait.def.defName));
            if (ate == null) return true;

            return Rand.Range(min: 0, max: 100) < ate.chance;
        }

        public static void TryMakeInitialRelationsWithPostfix(Faction __instance, Faction other)
        {
            if (other.def.basicMemberKind?.race is ThingDef_AlienRace alienProps)
                alienProps.alienRace.generalSettings.factionRelations?.ForEach(action: frs =>
                {
                    if (!frs.factions.Contains(item: __instance.def.defName)) return;
                    int           offset = frs.goodwill.RandomInRange;
                    FactionRelationKind kind = offset > 75 ?
                                                   FactionRelationKind.Ally :
                                                   offset <= -10 ?
                                                       FactionRelationKind.Hostile :
                                                       FactionRelationKind.Neutral;
                    FactionRelation relation = other.RelationWith(other: __instance);
                    relation.goodwill = offset;
                    relation.kind = kind;
                    relation = __instance.RelationWith(other: other);
                    relation.goodwill = offset;
                    relation.kind = kind;
                });

            alienProps = __instance.def.basicMemberKind?.race as ThingDef_AlienRace;
            alienProps?.alienRace.generalSettings.factionRelations?.ForEach(action: frs =>
            {
                if (!frs.factions.Contains(item: other.def.defName)) return;
                int           offset = frs.goodwill.RandomInRange;
                FactionRelationKind kind = offset > 75 ?
                                               FactionRelationKind.Ally :
                                               offset <= -10 ?
                                                   FactionRelationKind.Hostile :
                                                   FactionRelationKind.Neutral;
                FactionRelation relation = other.RelationWith(other: __instance);
                relation.goodwill = offset;
                relation.kind     = kind;
                relation          = __instance.RelationWith(other: other);
                relation.goodwill = offset;
                relation.kind     = kind;
            });
        }

        public static bool TryCreateSituationalThoughtPrefix(ref ThoughtDef def, SituationalThoughtHandler __instance)
        {
            Pawn pawn = __instance.pawn;
            if (DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(predicate: ar => !ar.alienRace.thoughtSettings.replacerList.NullOrEmpty())
               .SelectMany(selector: ar => ar.alienRace.thoughtSettings.replacerList.Select(selector: tr => tr.replacer)).Contains(value: def.defName)) return false;

            if (!(pawn.def is ThingDef_AlienRace race)) return !Traverse.Create(root: __instance).Field(name: "tmpCachedThoughts").GetValue<HashSet<ThoughtDef>>().Contains(item: def);
            {
                string          name     = def.defName;
                ThoughtReplacer replacer = race.alienRace.thoughtSettings.replacerList?.FirstOrDefault(predicate: tr => name.EqualsIgnoreCase(B: tr.original));
                if (replacer == null) return !Traverse.Create(root: __instance).Field(name: "tmpCachedThoughts").GetValue<HashSet<ThoughtDef>>().Contains(item: def);
                ThoughtDef replacerThoughtDef       = DefDatabase<ThoughtDef>.GetNamedSilentFail(defName: replacer.replacer);
                if (replacerThoughtDef != null) def = replacerThoughtDef;
            }
            return !Traverse.Create(root: __instance).Field(name: "tmpCachedThoughts").GetValue<HashSet<ThoughtDef>>().Contains(item: def);
        }

        public static void CanBingeNowPostfix(Pawn pawn, ChemicalDef chemical, ref bool __result)
        {
            if (!__result) return;
            if (!(pawn.def is ThingDef_AlienRace alienProps)) return;
            bool result = true;
            alienProps.alienRace.generalSettings.chemicalSettings?.ForEach(action: cs =>
                {
                    if ((cs.chemical?.EqualsIgnoreCase(B: chemical.defName) ?? false) && !cs.ingestible) result = false;
                }
            );
            __result = result;
        }

        public static void PostIngestedPostfix(Pawn ingester, CompDrug __instance)
        {
            if (ingester.def is ThingDef_AlienRace alienProps)
                alienProps.alienRace.generalSettings.chemicalSettings?.ForEach(action: cs =>
                {
                    if (cs.chemical?.EqualsIgnoreCase(B: __instance.Props?.chemical?.defName) ?? false)
                        cs.reactions?.ForEach(action: iod => iod.DoIngestionOutcome(pawn: ingester, ingested: __instance.parent));
                });
        }

        public static void DrugValidatorPostfix(ref bool __result, Pawn pawn, Thing drug) =>
            CanBingeNowPostfix(pawn: pawn, chemical: drug?.TryGetComp<CompDrug>()?.Props?.chemical, __result: ref __result);

        // ReSharper disable once RedundantAssignment
        public static void CompatibilityWithPostfix(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result)
        {
            Traverse traverse = Traverse.Create(root: __instance);
            Pawn     pawn     = traverse.Field(name: "pawn").GetValue<Pawn>();

            if (pawn.RaceProps.Humanlike != otherPawn.RaceProps.Humanlike || pawn == otherPawn)
            {
                __result = 0f;
                return;
            }

            float x   = Mathf.Abs(f: pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
            float num = GenMath.LerpDouble(inFrom: 0f, inTo: 20f, outFrom: 0.45f, outTo: -0.45f, x: x);
            num = Mathf.Clamp(value: num, min: -0.45f, max: 0.45f);
            float num2 = __instance.ConstantPerPawnsPairCompatibilityOffset(otherPawnID: otherPawn.thingIDNumber);
            __result = num + num2;
        }

        public static IEnumerable<CodeInstruction> SecondaryLovinChanceFactorTranspiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo  defField          = AccessTools.Field(type: typeof(Pawn), name: nameof(Pawn.def));
            MethodInfo racePropsProperty = AccessTools.Property(type: typeof(Pawn), name: nameof(Pawn.RaceProps)).GetGetMethod();
            MethodInfo humanlikeProperty = AccessTools.Property(type: typeof(RaceProperties), name: nameof(RaceProperties.Humanlike)).GetGetMethod();
            int        counter           = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                counter++;
                if (counter < 10)
                    if (instruction.opcode == OpCodes.Ldfld && instruction.operand == defField)
                    {
                        yield return new CodeInstruction(opcode: OpCodes.Callvirt, operand: racePropsProperty);

                        instruction.opcode  = OpCodes.Callvirt;
                        instruction.operand = humanlikeProperty;
                    }

                yield return instruction;
            }
        }

        public static void GenericHasJobOnThingPostfix(WorkGiver __instance, Pawn pawn, ref bool __result)
        {
            if (!__result) return;
            // ReSharper disable once ImplicitlyCapturedClosure
            __result = ((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd =>
                            DefDatabase<WorkGiverDef>.GetNamedSilentFail(defName: wgd)?.giverClass == __instance.GetType()) ?? false) ||
                       !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(predicate: d =>
                           pawn.def != d && (d.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd =>
                                                 DefDatabase<WorkGiverDef>.GetNamedSilentFail(defName: wgd)?.giverClass == __instance.GetType()) ?? false));
        }

        public static void GenericJobOnThingPostfix(WorkGiver __instance, Pawn pawn, ref Job __result)
        {
            if (__result == null) return;
            // ReSharper disable once ImplicitlyCapturedClosure
            if (!(((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd =>
                       DefDatabase<WorkGiverDef>.GetNamedSilentFail(defName: wgd)?.giverClass == __instance.GetType()) ?? false) ||
                  !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(predicate: d =>
                      pawn.def != d &&
                      (d.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd => DefDatabase<WorkGiverDef>.GetNamedSilentFail(defName: wgd)?.giverClass == __instance.GetType()) ?? false))))
                __result = null;
        }

        public static void SetFactionDirectPostfix(Thing __instance, Faction newFaction)
        {
            if (__instance.def is ThingDef_AlienRace alienProps && newFaction == Faction.OfPlayerSilentFail)
                alienProps.alienRace.raceRestriction.conceptList?.ForEach(action: cdd =>
                {
                    ConceptDef cd = DefDatabase<ConceptDef>.GetNamedSilentFail(defName: cdd);
                    if (cdd == null) return;
                    Find.Tutor.learningReadout.TryActivateConcept(conc: cd);
                    PlayerKnowledgeDatabase.SetKnowledge(def: cd, value: 0);
                });
        }

        public static void SetFactionPostfix(Pawn __instance, Faction newFaction)
        {
            if (__instance.def is ThingDef_AlienRace alienProps && newFaction == Faction.OfPlayerSilentFail && Current.ProgramState == ProgramState.Playing)
                alienProps.alienRace.raceRestriction.conceptList?.ForEach(action: cdd =>
                {
                    ConceptDef cd = DefDatabase<ConceptDef>.GetNamedSilentFail(defName: cdd);
                    if (cdd == null) return;
                    Find.Tutor.learningReadout.TryActivateConcept(conc: cd);
                    PlayerKnowledgeDatabase.SetKnowledge(def: cd, value: 0);
                });
        }

        public static void ApparelScoreGainPostFix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (!(__result >= 0f)) return;
            if (!RaceRestrictionSettings.CanWear(apparel: ap.def, race: pawn.def))
                __result = -50f;
        }

        public static void PrepForMapGenPrefix(GameInitData __instance) => Find.Scenario.AllParts.OfType<ScenPart_StartingHumanlikes>().Select(selector: sp => sp.GetPawns()).ToList().ForEach(
            action: sp =>
            {
                IEnumerable<Pawn> spa = sp as Pawn[] ?? sp.ToArray();
                __instance.startingAndOptionalPawns.InsertRange(index: __instance.startingPawnCount, collection: spa);
                __instance.startingPawnCount += spa.Count();
            });

        public static bool TryGainMemoryThoughtPrefix(ref Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            string thoughtName = newThought.def.defName;
            Pawn   pawn        = __instance.pawn;
            if (DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(predicate: ar => !ar.alienRace.thoughtSettings.replacerList.NullOrEmpty())
               .SelectMany(selector: ar => ar.alienRace.thoughtSettings.replacerList.Select(selector: tr => tr.replacer)).Contains(value: thoughtName)) return false;

            if (!(pawn.def is ThingDef_AlienRace race)) return true;
            {
                ThoughtReplacer replacer = race.alienRace.thoughtSettings.replacerList?.FirstOrDefault(predicate: tr => thoughtName.EqualsIgnoreCase(B: tr.original));
                if (replacer == null) return true;
                ThoughtDef replacerThoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(defName: replacer.replacer);
                if (replacerThoughtDef == null) return true;
                Thought_Memory replaceThought = (Thought_Memory) ThoughtMaker.MakeThought(def: replacerThoughtDef);
                newThought = replaceThought;
            }
            return true;
        }

        public static void ExtraRequirementsGrowerSowPostfix(Pawn pawn, IPlantToGrowSettable settable, WorkGiver_GrowerSow __instance, ref bool __result)
        {
            if (!__result) return;
            ThingDef plant = WorkGiver_Grower.CalculateWantedPlantDef(c: (settable as Zone_Growing)?.Cells[index: 0] ?? ((Thing) settable).Position, map: pawn.Map);

            __result = RaceRestrictionSettings.CanPlant(plant: plant, race: pawn.def);
        }

        public static void HasJobOnCellHarvestPostfix(Pawn pawn, IntVec3 c, ref bool __result)
        {
            if (!__result) return;
            ThingDef plant = c.GetPlant(map: pawn.Map).def;

            __result = RaceRestrictionSettings.CanPlant(plant: plant, race: pawn.def);
        }

        public static void PawnAllowedToStartAnewPostfix(Pawn p, Bill __instance, ref bool __result)
        {
            RecipeDef recipe = __instance.recipe;

            if (__result)
                __result = RaceRestrictionSettings.CanDoRecipe(recipe: recipe, race: p.def);
        }

        private static HashSet<ThingDef> colonistRaces;
        private static int               colonistRacesTick;
        private const  int               COLONIST_RACES_TICK_TIMER = GenDate.TicksPerHour * 2;

        public static void DesignatorAllowedPostfix(Designator d, ref bool __result)
        {
            if (!__result || !(d is Designator_Build build)) return;
            if (Find.TickManager.TicksAbs > colonistRacesTick + COLONIST_RACES_TICK_TIMER || Find.TickManager.TicksAbs < colonistRacesTick)
                if ((colonistRaces = new HashSet<ThingDef>(collection: PawnsFinder.AllMaps_FreeColonistsSpawned.Select(selector: p => p.def))).Count > 0)
                    colonistRacesTick = Find.TickManager.TicksAbs;

            __result = colonistRaces.Any(predicate: ar => RaceRestrictionSettings.CanBuild(building: build.PlacingDef, race: ar));
        }

        public static void CanConstructPostfix(Thing t, Pawn p, ref bool __result)
        {
            if (!__result) return;
            // t.def.defName.Replace(ThingDefGenerator_Buildings.BlueprintDefNameSuffix, string.Empty).Replace(ThingDefGenerator_Buildings.BuildingFrameDefNameSuffix, string.Empty).Replace(ThingDefGenerator_Buildings.InstallBlueprintDefNameSuffix, string.Empty);
            __result = RaceRestrictionSettings.CanBuild(building: t.def.entityDefToBuild ?? t.def, race: p.def);
        }

        public static IEnumerable<CodeInstruction> ResearchScreenTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo defListInfo = AccessTools.Method(
                type: typeof(DefDatabase<ResearchProjectDef>), name: nameof(DefDatabase<ResearchProjectDef>.AllDefsListForReading));

            foreach (CodeInstruction instruction in instructions)
                if (instruction.opcode == OpCodes.Call && instruction.operand == defListInfo)
                {
                    yield return instruction;
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: patchType, name: nameof(ResearchFixed)));
                }
                else
                {
                    yield return instruction;
                }
        }

        private static List<ResearchProjectDef> ResearchFixed(List<ResearchProjectDef> researchList) =>
            researchList.Where(predicate: prj => !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(predicate: ar => !colonistRaces.Contains(item: ar) &&
                                                                                                                             (ar.alienRace.raceRestriction?.researchList?.Any(predicate: rpr =>
                                                                                                                                  rpr.projects.Contains(item: prj.defName)) ?? false))).ToList();

        public static void ShouldSkipResearchPostfix(Pawn pawn, ref bool __result)
        {
            if (__result) return;
            ResearchProjectDef project = Find.ResearchManager.currentProj;

            ResearchProjectRestrictions rprest =
                (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.researchList?.FirstOrDefault(predicate: rpr => rpr.projects.Contains(item: project.defName));
            if (rprest != null)
            {
                IEnumerable<string> apparel = pawn.apparel.WornApparel.Select(selector: twc => twc.def.defName);
                if (!rprest.apparelList?.TrueForAll(match: ap => apparel.Contains(value: ap)) ?? false)
                    __result = true;
            }
            else
            {
                __result = DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(predicate: d =>
                    pawn.def != d && (d.alienRace.raceRestriction.researchList?.Any(predicate: rpr => rpr.projects.Contains(item: project.defName)) ?? false));
            }
        }

        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ref List<ThoughtDef> __result)
        {
            if (ingester.story.traits.HasTrait(tDef: AlienDefOf.Xenophobia) && ingester.story.traits.DegreeOfTrait(tDef: AlienDefOf.Xenophobia) == 1)
                if (__result.Contains(item: ThoughtDefOf.AteHumanlikeMeatDirect) && foodSource.def.ingestible.sourceDef != ingester.def)
                    __result.Remove(item: ThoughtDefOf.AteHumanlikeMeatDirect);
                else if (__result.Contains(item: ThoughtDefOf.AteHumanlikeMeatAsIngredient) &&
                         (foodSource.TryGetComp<CompIngredients>()?.ingredients.Any(predicate: td => FoodUtility.IsHumanlikeMeat(def: td) && td.ingestible.sourceDef != ingester.def) ?? false))
                    __result.Remove(item: ThoughtDefOf.AteHumanlikeMeatAsIngredient);
            if (!(ingester.def is ThingDef_AlienRace alienProps)) return;
            if (__result.Contains(item: ThoughtDefOf.AteHumanlikeMeatDirect) || __result.Contains(item: ThoughtDefOf.AteHumanlikeMeatDirectCannibal))
            {
                int index = __result.IndexOf(item: ingester.story.traits.HasTrait(tDef: TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect);
                ThoughtDef thought = DefDatabase<ThoughtDef>.GetNamedSilentFail(
                    defName: alienProps.alienRace.thoughtSettings.ateThoughtSpecific?.FirstOrDefault(predicate: at => at.raceList?.Contains(item: foodSource.def.ingestible.sourceDef.defName) ?? false)
                               ?.thought ?? alienProps.alienRace.thoughtSettings.ateThoughtGeneral.thought);
                if (thought != null)
                {
                    __result.RemoveAt(index: index);
                    __result.Insert(index: index, item: thought);
                }
            }

            if (!__result.Contains(item: ThoughtDefOf.AteHumanlikeMeatAsIngredient) && !__result.Contains(item: ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal)) return;
            {
                CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
                if (compIngredients == null) return;
                foreach (ThingDef ingredient in compIngredients.ingredients)
                    if (FoodUtility.IsHumanlikeMeat(def: ingredient))
                    {
                        int index = __result.IndexOf(item: ingester.story.traits.HasTrait(tDef: TraitDefOf.Cannibal) ?
                                                               ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal :
                                                               ThoughtDefOf.AteHumanlikeMeatAsIngredient);
                        ThoughtDef thought = DefDatabase<ThoughtDef>.GetNamedSilentFail(
                            defName: alienProps.alienRace.thoughtSettings.ateThoughtSpecific
                                       ?.FirstOrDefault(predicate: at => at.raceList?.Contains(item: ingredient.ingestible.sourceDef.defName) ?? false)?.ingredientThought ??
                                     alienProps.alienRace.thoughtSettings.ateThoughtGeneral.ingredientThought);
                        if (thought == null) continue;
                        __result.RemoveAt(index: index);
                        __result.Insert(index: index, item: thought);
                    }
            }
        }

        public static void GenerationChanceSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierSpouse;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierSpouse;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSpouse ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceSiblingPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierSibling;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierSibling;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSibling ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceParentPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierParent;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierParent;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierParent ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierLover;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierLover;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierLover ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceFiancePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierFiance;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierFiance;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierFiance ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceExSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierExSpouse;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierExSpouse;


            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExSpouse ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceExLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierExLover;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierExLover;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExLover ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceChildPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace alienProps) __result *= alienProps.alienRace.relationSettings.relationChanceModifierChild;

            if (other.def is ThingDef_AlienRace alienProps2) __result *= alienProps2.alienRace.relationSettings.relationChanceModifierChild;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: generated.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: other.story.GetBackstory(slot: BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierChild ?? 1;

            if (generated == other) __result = 0;
        }

        public static void BirthdayBiologicalPrefix(Pawn_AgeTracker __instance)
        {
            Pawn pawn = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>();
            if (!(pawn.def is ThingDef_AlienRace alienProps)) return;
            string path = alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: pawn.ageTracker.CurLifeStageRace.def).head;
            if (path != null)
                Traverse.Create(root: pawn.story).Field(name: "headGraphicPath").SetValue(value: alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(userpath: path, pawn: pawn));

            if (!pawn.def.race.lifeStageAges.Skip(count: 1).Any()) return;
            LifeStageAge lsac = pawn.ageTracker.CurLifeStageRace;
            LifeStageAge lsap = pawn.def.race.lifeStageAges[index: pawn.ageTracker.CurLifeStageIndex - 1];

            if (lsac is LifeStageAgeAlien lsaac && lsaac.body != null && ((lsap as LifeStageAgeAlien)?.body ?? pawn.RaceProps.body) != lsaac.body ||
                lsap is LifeStageAgeAlien lsaap && lsaap.body != null && ((lsac as LifeStageAgeAlien)?.body ?? pawn.RaceProps.body) != lsaap.body)
                pawn.health.hediffSet = new HediffSet(newPawn: pawn);
        }

        // ReSharper disable once RedundantAssignment
        public static bool ButcherProductsPrefix(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {
            Pawn               corpse = __instance.InnerPawn;
            IEnumerable<Thing> things = corpse.ButcherProducts(butcher: butcher, efficiency: efficiency);
            if (corpse.RaceProps.BloodDef != null) FilthMaker.MakeFilth(c: butcher.Position, map: butcher.Map, filthDef: corpse.RaceProps.BloodDef, source: corpse.LabelIndefinite());
            if (!corpse.RaceProps.Humanlike)
            {
                __result = things;
                return false;
            }

            ThoughtDef thought = !(butcher.def is ThingDef_AlienRace alienPropsButcher) ?
                                     ThoughtDefOf.ButcheredHumanlikeCorpse :
                                     DefDatabase<ThoughtDef>.GetNamedSilentFail(
                                         defName: alienPropsButcher.alienRace.thoughtSettings.butcherThoughtSpecific
                                                    ?.FirstOrDefault(predicate: bt => bt.raceList?.Contains(item: corpse.def.defName) ?? false)?.thought ??
                                                  alienPropsButcher.alienRace.thoughtSettings.butcherThoughtGeneral.thought);

            butcher.needs.mood.thoughts.memories.TryGainMemory(def: thought ?? ThoughtDefOf.ButcheredHumanlikeCorpse);

            butcher.Map.mapPawns.SpawnedPawnsInFaction(faction: butcher.Faction).ForEach(action: p =>
            {
                if (p == butcher || p.needs?.mood?.thoughts == null) return;
                thought = !(p.def is ThingDef_AlienRace alienPropsPawn) ?
                              ThoughtDefOf.KnowButcheredHumanlikeCorpse :
                              DefDatabase<ThoughtDef>.GetNamedSilentFail(
                                  defName: alienPropsPawn.alienRace.thoughtSettings.butcherThoughtSpecific
                                             ?.FirstOrDefault(predicate: bt => bt.raceList?.Contains(item: corpse.def.defName) ?? false)?.knowThought ??
                                           alienPropsPawn.alienRace.thoughtSettings.butcherThoughtGeneral.knowThought);

                p.needs.mood.thoughts.memories.TryGainMemory(def: thought ?? ThoughtDefOf.KnowButcheredHumanlikeCorpse);
            });
            TaleRecorder.RecordTale(def: TaleDefOf.ButcheredHumanlikeCorpse, args: new object[] {butcher});
            __result = things;
            return false;
        }

        public static void AddHumanlikeOrdersPostfix(ref List<FloatMenuOption> opts, Pawn pawn, Vector3 clickPos)
        {
            IntVec3            c          = IntVec3.FromVector3(v: clickPos);
            ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;
            {
                Thing drugs = c.GetThingList(map: pawn.Map).FirstOrDefault(predicate: t => t?.TryGetComp<CompDrug>() != null);
                if (drugs != null && (alienProps?.alienRace.generalSettings.chemicalSettings?.Any(predicate: cs =>
                                          cs.chemical.EqualsIgnoreCase(B: drugs.TryGetComp<CompDrug>()?.Props.chemical?.defName) && !cs.ingestible) ?? false))
                {
                    List<FloatMenuOption> options = opts.Where(predicate: fmo =>
                        !fmo.Disabled && fmo.Label.Contains(value: string.Format(format: drugs.def.ingestible.ingestCommandString, arg0: drugs.LabelShort))).ToList();
                    foreach (FloatMenuOption fmo in options)
                    {
                        int index = opts.IndexOf(item: fmo);
                        opts.Remove(item: fmo);

                        opts.Insert(index: index, item: new FloatMenuOption(label: $"{"CannotEquip".Translate(arg1: drugs.LabelShort)} {pawn.def.LabelCap} can't consume this)", action: null));
                    }
                }
            }
            if (pawn.equipment != null)
            {
                ThingWithComps equipment = (ThingWithComps) c.GetThingList(map: pawn.Map).FirstOrDefault(predicate: t => t.TryGetComp<CompEquippable>() != null && t.def.IsWeapon);
                if (equipment != null)
                {
                    List<FloatMenuOption> options = opts.Where(predicate: fmo => !fmo.Disabled && fmo.Label.Contains(value: "Equip".Translate(arg1: equipment.LabelShort))).ToList();


                    if (!options.NullOrEmpty() && !RaceRestrictionSettings.CanEquip(weapon: equipment.def, race: pawn.def))
                        foreach (FloatMenuOption fmo in options)
                        {
                            int index = opts.IndexOf(item: fmo);
                            opts.Remove(item: fmo);

                            opts.Insert(index: index,
                                item: new FloatMenuOption(label: $"{"CannotEquip".Translate(arg1: equipment.LabelShort)} ({pawn.def.LabelCap} can't equip this)", action: null));
                        } 
                }
            }

            if (pawn.apparel == null) return;
            {
                Apparel apparel = pawn.Map.thingGrid.ThingAt<Apparel>(c: c);
                if (apparel == null) return;
                List<FloatMenuOption> options = opts.Where(predicate: fmo => !fmo.Disabled && fmo.Label.Contains(value: "ForceWear".Translate(arg1: apparel.LabelShort))).ToList();

                if (options.NullOrEmpty() || RaceRestrictionSettings.CanWear(apparel: apparel.def, race: pawn.def)) return;
                {
                    foreach (FloatMenuOption fmo in options)
                    {
                        int index = opts.IndexOf(item: fmo);
                        opts.Remove(item: fmo);

                        opts.Insert(index: index, item: new FloatMenuOption(label: $"{"CannotWear".Translate(arg1: apparel.LabelShort)} ({pawn.def.LabelCap} can't wear this)", action: null));
                    }
                }
            }
        }

        public static void CanGetThoughtPostfix(ref bool __result, ThoughtDef def, Pawn pawn)
        {
            if (!__result || !(pawn.def is ThingDef_AlienRace alienProps)) return;
            if (alienProps.alienRace.thoughtSettings.cannotReceiveThoughtsAtAll && (alienProps.alienRace.thoughtSettings.canStillReceiveThoughts?.Contains(item: def.defName) ?? false))
                __result = false;
            else if (!alienProps.alienRace.thoughtSettings.cannotReceiveThoughts.NullOrEmpty() &&
                     alienProps.alienRace.thoughtSettings.cannotReceiveThoughts.Contains(item: def.defName)) __result = false;
        }

        public static void CanDoNextStartPawnPostfix(ref bool __result)
        {
            if (__result) return;

            bool result = true;
            Find.GameInitData.startingAndOptionalPawns.ForEach(action: current =>
            {
                if (!current.Name.IsValid && current.def.race.GetNameGenerator(gender: current.gender) == null) result = false;
            });
            __result = result;
        }

        public static bool GeneratePawnNamePrefix(ref Name __result, Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
        {
            if (!(pawn.def is ThingDef_AlienRace alienProps) || alienProps.race.GetNameGenerator(gender: pawn.gender) == null || style != NameStyle.Full) return true;

            NameTriple nameTriple = NameTriple.FromString(rawName: NameGenerator.GenerateName(rootPack: alienProps.race.GetNameGenerator(gender: pawn.gender)));

            string first = nameTriple.First, nick = nameTriple.Nick, last = nameTriple.Last;
            
            if (nick == null) nick = nameTriple.First;

            if (last != null && forcedLastName != null) last = forcedLastName;

            __result = new NameTriple(first: first ?? string.Empty, nick: nick ?? string.Empty, last: last ?? string.Empty);

            return false;
        }

        public static void GenerateRandomAgePrefix(Pawn pawn, PawnGenerationRequest request)
        {
            if (request.FixedGender.HasValue) return;
            float maleGenderProbability = (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.maleGenderProbability ?? pawn.kindDef.GetModExtension<Info>()?.maleGenderProbability ?? 0.5f;

            pawn.gender = Rand.Value >= maleGenderProbability ? Gender.Female : Gender.Male;
            AlienPartGenerator.AlienComp alienComp = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
            if ((alienComp == null || !(Math.Abs(value: maleGenderProbability) < 0.001f)) && !(Math.Abs(value: maleGenderProbability - 100f) < 0.001f)) return;
            if (alienComp != null)
                alienComp.fixGenderPostSpawn = true;
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

                pawn.GetComp<AlienPartGenerator.AlienComp>().hairColorSecond =
                    alienProps.alienRace.generalSettings.alienPartGenerator.alienhairsecondcolorgen?.NewRandomizedColor() ?? pawn.story.hairColor;

                if (alienProps.alienRace.hairSettings.getsGreyAt <= pawn.ageTracker.AgeBiologicalYears)
                {
                    float grey = Rand.Range(min: 0.65f, max: 0.85f);
                    pawn.story.hairColor = new Color(r: grey, g: grey, b: grey);
                }

                Traverse.Create(root: pawn.story).Field(name: "headGraphicPath").SetValue(
                    value: alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: pawn.ageTracker.CurLifeStage).head.NullOrEmpty() ?
                               "" :
                               alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(
                                   userpath: alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: pawn.ageTracker.CurLifeStage).head, pawn: pawn));
                pawn.story.crownType = CrownType.Average;
            }
        }

        public static bool NewGeneratedStartingPawnPrefix(ref Pawn __result)
        {
            PawnKindDef kindDef = Faction.OfPlayer.def.basicMemberKind;

            DefDatabase<RaceSettings>.AllDefsListForReading.Where(predicate: tdar => !tdar.pawnKindSettings.startingColonists.NullOrEmpty())
               .SelectMany(selector: tdar => tdar.pawnKindSettings.startingColonists).Where(predicate: sce => sce.factionDefs.Contains(item: Faction.OfPlayer.def.defName))
               .SelectMany(selector: sce => sce.pawnKindEntries).InRandomOrder().ToList().ForEach(action: pke =>
                {
                    if (!(Rand.Range(min: 0f, max: 100f) < pke.chance)) return;
                    PawnKindDef pk          = DefDatabase<PawnKindDef>.GetNamedSilentFail(defName: pke.kindDefs.RandomElement());
                    if (pk != null) kindDef = pk;
                });

            if (kindDef == Faction.OfPlayer.def.basicMemberKind) return true;

            PawnGenerationRequest request = new PawnGenerationRequest(kind: kindDef, faction: Faction.OfPlayer, context: PawnGenerationContext.PlayerStarter, forceGenerateNewPawn: true,
                colonistRelationChanceFactor: 26f);
            Pawn pawn;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request: request);
            }
            catch (Exception arg)
            {
                Log.Error(text: $"There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: {arg}");
                pawn = PawnGenerator.GeneratePawn(request: request);
            }

            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn: pawn);
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
            if (pawn.def is ThingDef_AlienRace alienProps && alienProps.alienRace.generalSettings.immuneToAge) return false;
            return true;
        }

        
        public static bool FillBackstoryInSlotShuffledPrefix(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
        {
            bioReference = null;
            if (slot == BackstorySlot.Adulthood && DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: pawn.story.childhood.identifier)?.linkedBackstory is string id &&
                BackstoryDatabase.TryGetWithIdentifier(identifier: id, bs: out backstory))
                return false;
            /*
            if ((pawn.def is ThingDef_AlienRace alienProps && alienProps.alienRace.generalSettings.pawnsSpecificBackstories ||
                 (pawn.kindDef.GetModExtension<Info>()?.usePawnKindBackstories ?? false)) && !pawn.kindDef.backstoryCategories.NullOrEmpty())
            {
                if (BackstoryDatabase.allBackstories.Where(predicate: kvp => kvp.Value.shuffleable  && kvp.Value.spawnCategories.Contains(item: pawn.kindDef.backstoryCategory) &&
                                                                             kvp.Value.slot == slot && (slot == BackstorySlot.Childhood ||
                                                                                                        !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(
                                                                                                            b: pawn.story.childhood?.workDisables ?? WorkTags.None)) &&
                                                                             (!(DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: kvp.Value.identifier) is BackstoryDef bs) ||
                                                                              bs.Approved(p: pawn) && (slot == BackstorySlot.Childhood || bs.linkedBackstory.NullOrEmpty())))
                   .TryRandomElement(result: out KeyValuePair<string, Backstory> backstoryPair))
                {
                    backstory = backstoryPair.Value;
                    return false;
                }

                Log.Message(
                    text:
                    $"FAILED: {pawn.def.defName} {pawn.kindDef.defName} {pawn.kindDef.backstoryCategories} {BackstoryDatabase.allBackstories.Values.Count(predicate: bs => bs.spawnCategories.Contains(item: pawn.kindDef.backstoryCategory))}");
            }

            */
            return true;
        }
        
        private static PawnBioDef bioReference;

        // ReSharper disable once RedundantAssignment
        public static void TryGetRandomUnusedSolidBioForPostfix(List<string> backstoryCategories, ref PawnBio __result, PawnKindDef kind, Gender gender, string requiredLastName)
        {
            if (SolidBioDatabase.allBios.Where(predicate: pb =>
                (((kind.race as ThingDef_AlienRace)?.alienRace.generalSettings.allowHumanBios ?? true) && (kind.GetModExtension<Info>()?.allowHumanBios ?? true) ||
                 (DefDatabase<PawnBioDef>.AllDefs.FirstOrDefault(predicate: pbd => pb.name.ConfusinglySimilarTo(other: pbd.name))?.validRaces.Contains(item: kind.race) ?? false)) &&
                (pb.gender == GenderPossibility.Either || pb.gender == GenderPossibility.Male && gender == Gender.Male)                                                            &&
                (requiredLastName.NullOrEmpty()        || pb.name.Last == requiredLastName)                                                                                        &&
                (!kind.factionLeader                   || pb.pirateKing)                                                                                                           &&
                pb.adulthood.spawnCategories.Any(predicate: backstoryCategories.Contains)                                                                                                     &&
                !pb.name.UsedThisGame).TryRandomElement(result: out PawnBio bio))
            {
                __result     = bio;
                bioReference = DefDatabase<PawnBioDef>.AllDefs.FirstOrDefault(predicate: pbd => bio.name.ConfusinglySimilarTo(other: pbd.name));
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
                    __instance.pawn.gender = Rand.Value >= alienProps.alienRace.generalSettings.maleGenderProbability ? Gender.Female : Gender.Male;
                    __instance.pawn.Name   = PawnBioAndNameGenerator.GeneratePawnName(pawn: __instance.pawn);


                    Traverse.Create(root: __instance.pawn.story).Field(name: "headGraphicPath").SetValue(
                        value: alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: alien.ageTracker.CurLifeStage).head.NullOrEmpty() ?
                                   "" :
                                   alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(
                                       userpath: alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: alien.ageTracker.CurLifeStage).head, pawn: __instance.pawn));

                    alienComp.fixGenderPostSpawn = false;
                }

                GraphicPaths graphicPaths = alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: alien.ageTracker.CurLifeStage);

                alienComp.customDrawSize         = graphicPaths.customDrawSize;
                alienComp.customPortraitDrawSize = graphicPaths.customPortraitDrawSize;

                alienComp.AssignProperMeshs();

                Traverse.Create(root: alien.story).Field(name: "headGraphicPath").SetValue(value: alienComp.crownType.NullOrEmpty() ?
                                                                                                      alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(
                                                                                                          userpath: graphicPaths.head, pawn: alien) :
                                                                                                      AlienPartGenerator.GetAlienHead(userpath: graphicPaths.head,
                                                                                                          gender: alienProps.alienRace.generalSettings.alienPartGenerator.useGenderedHeads ?
                                                                                                                      alien.gender.ToString() :
                                                                                                                      "", crowntype: alienComp.crownType));

                __instance.nakedGraphic = !graphicPaths.body.NullOrEmpty() ?
                                              alienProps.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(bodyType: alien.story.bodyType,
                                                  shader: ContentFinder<Texture2D>.Get(
                                                              itemPath: AlienPartGenerator.GetNakedPath(bodyType: alien.story.bodyType, userpath: graphicPaths.body,
                                                                            gender: alienProps.alienRace.generalSettings.alienPartGenerator.useGenderedBodies ? alien.gender.ToString() : "") +
                                                                        "_northm", reportFailure: false) == null ?
                                                              ShaderDatabase.Cutout :
                                                              ShaderDatabase.CutoutComplex, skinColor: __instance.pawn.story.SkinColor,
                                                  skinColorSecond: alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(alien: alien, first: false), userpath: graphicPaths.body,
                                                  gender: alien.gender.ToString()) :
                                              null;
                __instance.rottingGraphic = !graphicPaths.body.NullOrEmpty() ?
                                                alienProps.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(bodyType: alien.story.bodyType, shader: ShaderDatabase.Cutout,
                                                    skinColor: PawnGraphicSet.RottingColor, skinColorSecond: PawnGraphicSet.RottingColor, userpath: graphicPaths.body,
                                                    gender: alien.gender.ToString()) :
                                                null;
                __instance.dessicatedGraphic = !graphicPaths.skeleton.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(path:(graphicPaths.skeleton == GraphicPaths.VANILLA_SKELETON_PATH ? alien.story.bodyType.bodyDessicatedGraphicPath : graphicPaths.skeleton), shader: ShaderDatabase.Cutout) : null;
                __instance.headGraphic = alien.health.hediffSet.HasHead && !alien.story.HeadGraphicPath.NullOrEmpty() ?
                                             GraphicDatabase.Get<Graphic_Multi>(path: alien.story.HeadGraphicPath,
                                                 shader: ContentFinder<Texture2D>.Get(itemPath: alien.story.HeadGraphicPath + "_northm", reportFailure: false) == null ?
                                                             ShaderDatabase.Cutout :
                                                             ShaderDatabase.CutoutComplex, drawSize: Vector2.one, color: alien.story.SkinColor,
                                                 colorTwo: alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(alien: alien, first: false)) :
                                             null;
                __instance.desiccatedHeadGraphic = alien.health.hediffSet.HasHead && !alien.story.HeadGraphicPath.NullOrEmpty() ?
                                                       GraphicDatabase.Get<Graphic_Multi>(path: alien.story.HeadGraphicPath, shader: ShaderDatabase.Cutout, drawSize: Vector2.one,
                                                           color: PawnGraphicSet.RottingColor) :
                                                       null;
                __instance.skullGraphic = alien.health.hediffSet.HasHead && !graphicPaths.skull.NullOrEmpty() ?
                                              GraphicDatabase.Get<Graphic_Multi>(path: graphicPaths.skull, shader: ShaderDatabase.Cutout, drawSize: Vector2.one, color: Color.white) :
                                              null;
                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(path: __instance.pawn.story.hairDef.texPath,
                    shader: ContentFinder<Texture2D>.Get(itemPath: __instance.pawn.story.hairDef.texPath + "_northm", reportFailure: false) == null ?
                                ShaderDatabase.Cutout :
                                ShaderDatabase.CutoutComplex, drawSize: Vector2.one, color: alien.story.hairColor, colorTwo: alienComp.hairColorSecond);
                __instance.headStumpGraphic = !graphicPaths.stump.NullOrEmpty() ?
                                                  GraphicDatabase.Get<Graphic_Multi>(path: graphicPaths.stump,
                                                      shader: alienComp.skinColor == alienComp.skinColorSecond ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, drawSize: Vector2.one,
                                                      color: alien.story.SkinColor, colorTwo: alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(alien: alien, first: false)) :
                                                  null;
                __instance.desiccatedHeadStumpGraphic = !graphicPaths.stump.NullOrEmpty() ?
                                                            GraphicDatabase.Get<Graphic_Multi>(path: graphicPaths.stump,
                                                                shader: ShaderDatabase.Cutout, drawSize: Vector2.one,
                                                                color: PawnGraphicSet.RottingColor) :
                                                            null;

                AlienPartGenerator apg = alienProps.alienRace.generalSettings.alienPartGenerator;
                alienComp.addonGraphics = new List<Graphic>();
                if (alienComp.addonVariants == null)
                    alienComp.addonVariants = new List<int>();
                int sharedIndex = 0;
                for (int i = 0; i < apg.bodyAddons.Count; i++)
                {
                    Graphic g = apg.bodyAddons[index: i].GetPath(pawn: alien, sharedIndex: ref sharedIndex,
                        savedIndex: alienComp.addonVariants.Count > i ? (int?) alienComp.addonVariants[index: i] : null);
                    alienComp.addonGraphics.Add(item: g);
                    if (alienComp.addonVariants.Count <= i)
                        alienComp.addonVariants.Add(item: sharedIndex);
                }

                __instance.ResolveApparelGraphics();

                return false;
            }

            return true;
        }

        public static void GenerateTraitsPrefix(Pawn pawn, PawnGenerationRequest request)
        {

            if (!request.Newborn && request.CanGeneratePawnRelations &&
                pawn.story.AllBackstories.Any(predicate: bs => DefDatabase<BackstoryDef>.GetNamedSilentFail(defName: bs.identifier)?.relationSettings != null))
            {
                pawn.relations.ClearAllRelations();
                AccessTools.Method(type: typeof(PawnGenerator), name: "GeneratePawnRelations").Invoke(obj: null, parameters: new object[] {pawn, request});
            }



            if (pawn.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.generalSettings.forcedRaceTraitEntries.NullOrEmpty())
                alienProps.alienRace.generalSettings.forcedRaceTraitEntries.ForEach(action: ate =>
                {
                    if ((pawn.story.traits.allTraits.Count >= 4 || pawn.gender != Gender.Male ||
                         !(Math.Abs(value: ate.commonalityMale - -1f) < 0.001f) && !(Rand.Range(min: 0, max: 100) < ate.commonalityMale))                           &&
                        (pawn.gender != Gender.Female || Math.Abs(value: ate.commonalityFemale - -1f) > 0.001f && !(Rand.Range(min: 0, max: 100) < ate.commonalityFemale)) &&
                        pawn.gender != Gender.None) return;
                    if (!pawn.story.traits.allTraits.Any(predicate: tr => tr.def.defName.EqualsIgnoreCase(B: ate.defName)))
                        pawn.story.traits.GainTrait(trait: new Trait(def: TraitDef.Named(defName: ate.defName), degree: ate.degree, forced: true));
                });
        }

        public static void SkinColorPostfix(Pawn_StoryTracker __instance, ref Color __result)
        {
            Pawn pawn                                               = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>();
            if (pawn.def is ThingDef_AlienRace alienProps) __result = alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(alien: pawn);
        }

        public static void GenerateBodyTypePostfix(ref Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.NullOrEmpty() &&
                !alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.Contains(item: pawn.story.bodyType))
                pawn.story.bodyType = alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.RandomElement();
        }

        private static readonly FactionDef noHairFaction = new FactionDef {hairTags = new List<string> {"alienNoHair"}};
        private static readonly FactionDef hairFaction   = new FactionDef();

        public static void RandomHairDefForPrefix(Pawn pawn, ref FactionDef factionType)
        {
            if (!(pawn.def is ThingDef_AlienRace alienProps)) return;
            if (!alienProps.alienRace.hairSettings.hasHair)
            {
                factionType = noHairFaction;
            }
            else if (!alienProps.alienRace.hairSettings.hairTags.NullOrEmpty())
            {
                hairFaction.hairTags = alienProps.alienRace.hairSettings.hairTags;
                factionType          = hairFaction;
            }
        }

        public static void GeneratePawnPrefix(ref PawnGenerationRequest request)
        {
            PawnKindDef kindDef = request.KindDef;
            if (Faction.OfPlayerSilentFail != null && kindDef == PawnKindDefOf.Villager && request.Faction.IsPlayer && kindDef.race != Faction.OfPlayer?.def.basicMemberKind.race)
                kindDef = Faction.OfPlayer?.def.basicMemberKind;

            if (Rand.Value <= 0.4f)
            {
                IEnumerable<RaceSettings> settings = DefDatabase<RaceSettings>.AllDefsListForReading;
                PawnKindEntry                   pk;
                if (request.KindDef == PawnKindDefOf.SpaceRefugee)
                {
                    if (settings.Where(predicate: r => !r.pawnKindSettings.alienrefugeekinds.NullOrEmpty()).Select(selector: r => r.pawnKindSettings.alienrefugeekinds.RandomElement())
                       .TryRandomElementByWeight(weightSelector: pke => pke.chance, result: out pk))
                    {
                        PawnKindDef pkd          = DefDatabase<PawnKindDef>.GetNamedSilentFail(defName: pk.kindDefs.RandomElement());
                        if (pkd != null) kindDef = pkd;
                    }
                }
                else if (request.KindDef == PawnKindDefOf.Slave)
                {
                    if (settings.Where(predicate: r => !r.pawnKindSettings.alienslavekinds.NullOrEmpty()).Select(selector: r => r.pawnKindSettings.alienslavekinds.RandomElement())
                       .TryRandomElementByWeight(weightSelector: pke => pke.chance, result: out pk))
                    {
                        PawnKindDef pkd          = DefDatabase<PawnKindDef>.GetNamedSilentFail(defName: pk.kindDefs.RandomElement());
                        if (pkd != null) kindDef = pkd;
                    }
                }
                else if (request.KindDef == PawnKindDefOf.Villager)
                {
                    DefDatabase<RaceSettings>.AllDefsListForReading.Where(predicate: tdar => !tdar.pawnKindSettings.alienwandererkinds.NullOrEmpty())
                       .SelectMany(selector: tdar => tdar.pawnKindSettings.alienwandererkinds).Where(predicate: sce => sce.factionDefs.Contains(item: Faction.OfPlayer.def.defName))
                       .SelectMany(selector: sce => sce.pawnKindEntries).InRandomOrder().ToList().ForEach(action: pke =>
                        {
                            if (!(Rand.Range(min: 0f, max: 100f) < pke.chance)) return;
                            PawnKindDef fpk          = DefDatabase<PawnKindDef>.GetNamedSilentFail(defName: pke.kindDefs.RandomElement());
                            if (fpk != null) kindDef = fpk;
                        });
                }
            }

            request = new PawnGenerationRequest(kind: kindDef, faction: request.Faction, context: request.Context, tile: request.Tile, forceGenerateNewPawn: request.ForceGenerateNewPawn,
                newborn: request.Newborn,
                allowDead: request.AllowDead, allowDowned: request.AllowDead, canGeneratePawnRelations: request.CanGeneratePawnRelations, mustBeCapableOfViolence: request.MustBeCapableOfViolence,
                colonistRelationChanceFactor: request.ColonistRelationChanceFactor,
                forceAddFreeWarmLayerIfNeeded: request.ForceAddFreeWarmLayerIfNeeded, allowGay: request.AllowGay, allowFood: request.AllowFood, inhabitant: request.Inhabitant,
                certainlyBeenInCryptosleep: request.CertainlyBeenInCryptosleep,
                forceRedressWorldPawnIfFormerColonist: request.ForceRedressWorldPawnIfFormerColonist, worldPawnFactionDoesntMatter: request.WorldPawnFactionDoesntMatter,
                validatorPreGear: request.ValidatorPreGear,
                validatorPostGear: request.ValidatorPostGear, minChanceToRedressWorldPawn: request.MinChanceToRedressWorldPawn, fixedBiologicalAge: request.FixedBiologicalAge,
                fixedChronologicalAge: request.FixedChronologicalAge, fixedGender: request.FixedGender, fixedMelanin: request.FixedMelanin, fixedLastName: request.FixedLastName);
        }

        public static IEnumerable<CodeInstruction> RenderPawnInternalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo  humanlikeBodyInfo = AccessTools.Field(type: typeof(MeshPool), name: nameof(MeshPool.humanlikeBodySet));
            FieldInfo  humanlikeHeadInfo = AccessTools.Field(type: typeof(MeshPool), name: nameof(MeshPool.humanlikeHeadSet));
            MethodInfo hairInfo          = AccessTools.Property(type: typeof(PawnGraphicSet), name: nameof(PawnGraphicSet.HairMeshSet)).GetGetMethod();

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[index: i];
                if (instruction.operand == humanlikeBodyInfo)
                {
                    instructionList.RemoveRange(index: i, count: 2);
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 7); // portrait
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld,   operand: AccessTools.Field(type: typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 4); // bodyfacing
                    yield return new CodeInstruction(opcode: OpCodes.Ldc_I4_1);
                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: patchType, name: nameof(GetPawnMesh)));
                }
                else if (instruction.operand == humanlikeHeadInfo)
                {
                    instructionList.RemoveRange(index: i, count: 2);
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 7); // portrait
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld,   operand: AccessTools.Field(type: typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 5); //headfacing
                    yield return new CodeInstruction(opcode: OpCodes.Ldc_I4_0);
                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: patchType, name: nameof(GetPawnMesh)));
                }
                else if (i + 4 < instructionList.Count && instructionList[index: i + 2].operand == hairInfo)
                {
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 7) {labels = instruction.labels}; // portrait
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld,   operand: AccessTools.Field(type: typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 5); //headfacing
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(type: typeof(PawnRenderer), name: nameof(PawnRenderer.graphics)));
                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: patchType, name: nameof(GetPawnHairMesh)));
                    instructionList.RemoveRange(index: i, count: 4);
                }
                else if (i > 1 && instructionList[index: i -1].operand == AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new []{typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(Int32)}) && (i+1) < instructionList.Count && instructionList[index: i + 1].opcode == OpCodes.Brtrue)
                {
                    yield return instruction; // portrait
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld,   operand: AccessTools.Field(type: typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_S, operand: 6); //vector
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_0);             // quat
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 4); // bodyfacing
                    yield return new CodeInstruction(opcode: OpCodes.Call,    operand: AccessTools.Method(type: patchType, name: nameof(DrawAddons)));

                    instruction = new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 7);
                }

                yield return instruction;
            }
        }

        public static Mesh GetPawnMesh(bool portrait, Pawn pawn, Rot4 facing, bool wantsBody) =>
            pawn.GetComp<AlienPartGenerator.AlienComp>() is AlienPartGenerator.AlienComp alienComp ?
                portrait ?
                    wantsBody ?
                        alienComp.alienPortraitGraphics.bodySet.MeshAt(rot: facing) :
                        alienComp.alienPortraitGraphics.headSet.MeshAt(rot: facing) :
                    wantsBody ?
                        alienComp.alienGraphics.bodySet.MeshAt(rot: facing) :
                        alienComp.alienGraphics.headSet.MeshAt(rot: facing) :
                wantsBody ?
                    MeshPool.humanlikeBodySet.MeshAt(rot: facing) :
                    MeshPool.humanlikeHeadSet.MeshAt(rot: facing);

        public static Mesh GetPawnHairMesh(bool portrait, Pawn pawn, Rot4 headFacing, PawnGraphicSet graphics) =>
            pawn.GetComp<AlienPartGenerator.AlienComp>() is AlienPartGenerator.AlienComp alienComp ?
                     (portrait ?
                          alienComp.alienPortraitGraphics.hairSetAverage :
                          alienComp.alienGraphics.hairSetAverage).MeshAt(rot: headFacing) :
                graphics.HairMeshSet.MeshAt(rot: headFacing);

        public static void DrawAddons(bool portrait, Pawn pawn, Vector3 vector, Quaternion quat, Rot4 rotation)
        {
            if (!(pawn.def is ThingDef_AlienRace alienProps)) return;

            List<AlienPartGenerator.BodyAddon> addons    = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons;
            AlienPartGenerator.AlienComp       alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            for (int i = 0; i < addons.Count; i++)
            {
                AlienPartGenerator.BodyAddon ba = addons[index: i];


                if (!ba.CanDrawAddon(pawn: pawn)) continue;

                AlienPartGenerator.RotationOffset offset = rotation == Rot4.South ?
                                                               ba.offsets.south :
                                                               rotation == Rot4.North ?
                                                                   ba.offsets.north :
                                                                   rotation == Rot4.East ?
                                                                    ba.offsets.east : 
                                                                    ba.offsets.west;

                Vector2 bodyOffset = (portrait ? offset?.portraitBodyTypes ?? offset?.bodyTypes : offset?.bodyTypes)?.FirstOrDefault(predicate: to => to.bodyType == pawn.story.bodyType)
                                   ?.offset ?? Vector2.zero;
                Vector2 crownOffset = (portrait ? offset?.portraitCrownTypes ?? offset?.crownTypes : offset?.crownTypes)?.FirstOrDefault(predicate: to => to.crownType == alienComp.crownType)
                                    ?.offset ?? Vector2.zero;

                //Defaults for tails 
                //south 0.42f, -0.3f, -0.22f
                //north     0f,  0.3f, -0.55f
                //east -0.42f, -0.3f, -0.22f   

                float moffsetX = 0.42f;
                float moffsetZ = -0.22f;
                float moffsetY = ba.inFrontOfBody ? 0.3f + ba.layerOffset : -0.3f - ba.layerOffset;
                float num      = ba.angle;

                if (rotation == Rot4.North)
                {
                    moffsetX = 0f;
                    moffsetY = !ba.inFrontOfBody ? -0.3f - ba.layerOffset : 0.3f + ba.layerOffset;
                    moffsetZ = -0.55f;
                    num      = 0;
                }

                moffsetX += bodyOffset.x + crownOffset.x;
                moffsetZ += bodyOffset.y + crownOffset.y;

                if (rotation == Rot4.East)
                {
                    moffsetX = -moffsetX;
                    num      = -num; //Angle
                }
                
                Vector3 offsetVector = new Vector3(x: moffsetX, y: moffsetY, z: moffsetZ);
                /*
                Vector3 calcVec = vector + offsetVector.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: quat)) * 2f * 57.29578f);
                if (!portrait && pawn.GetPosture() != PawnPosture.Standing)
                    Log.ErrorOnce(text: $"{pawn.Name.ToStringShort}: {rotation} {pawn.GetPosture()} {pawn.CurrentBed()?.Rotation}\n{vector} + {offsetVector} = {calcVec}", key: pawn.GetHashCode() * rotation.AsInt * (int) pawn.GetPosture());
                GenDraw.DrawMeshNowOrLater(mesh: alienComp.addonGraphics[index: i].MeshAt(rot: rotation), loc: vector,
                    quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * quat, mat: alienComp.addonGraphics[index: i].GetColoredVersion(newShader: ShaderDatabase.Cutout, newColor: Color.red, newColorTwo: Color.black).MatAt(rot: rotation), drawNow: portrait);
                */
                //                                                                                        Angle calculation to not pick the shortest, taken from Quaternion.Angle and modified
                GenDraw.DrawMeshNowOrLater(mesh: alienComp.addonGraphics[index: i].MeshAt(rot: rotation), loc: vector + offsetVector.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: quat)) * 2f * 57.29578f),
                    quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * quat, mat: alienComp.addonGraphics[index: i].MatAt(rot: rotation), drawNow: portrait);
            }
        }
    }
}