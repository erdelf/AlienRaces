﻿namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using Verse.Grammar;
    using OpCodes = System.Reflection.Emit.OpCodes;

    /// <summary>
    /// "More useful than the Harmony wiki" ~ Mehni
    /// </summary>

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.erdelf.alien_race.main");
            //Harmony.DEBUG = true;
            
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Child), nameof(PawnRelationWorker_Child.GenerationChance)), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerationChanceChildPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExLover), nameof(PawnRelationWorker_ExLover.GenerationChance)), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerationChanceExLoverPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExSpouse), nameof(PawnRelationWorker_ExSpouse.GenerationChance)), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerationChanceExSpousePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Fiance), nameof(PawnRelationWorker_Spouse.GenerationChance)), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerationChanceFiancePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Lover.GenerationChance)), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerationChanceLoverPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Parent), nameof(PawnRelationWorker_Parent.GenerationChance)), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerationChanceParentPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Sibling), nameof(PawnRelationWorker_Sibling.GenerationChance)), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerationChanceSiblingPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Spouse), nameof(PawnRelationWorker_Spouse.GenerationChance)), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerationChanceSpousePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GeneratePawnRelations"),
                new HarmonyMethod(patchType, nameof(GeneratePawnRelationsPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationDef), nameof(PawnRelationDef.GetGenderSpecificLabel)),
                new HarmonyMethod(patchType, nameof(GetGenderSpecificLabelPrefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "TryGetRandomUnusedSolidBioFor"), prefix: null,
                new HarmonyMethod(patchType, nameof(TryGetRandomUnusedSolidBioForPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "FillBackstorySlotShuffled"),
                new HarmonyMethod(patchType, nameof(FillBackstoryInSlotShuffledPrefix)), transpiler: new HarmonyMethod(patchType, nameof(FillBackstoryInSlotShuffledTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(WorkGiver_Researcher), nameof(WorkGiver_Researcher.ShouldSkip)), prefix: null,
                new HarmonyMethod(patchType, nameof(ShouldSkipResearchPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), name: "ViewSize"), prefix: null, postfix: null,
                new HarmonyMethod(patchType, nameof(ResearchScreenTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), name: "DrawRightRect"), prefix: null, postfix: null,
                new HarmonyMethod(patchType, nameof(ResearchScreenTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.CanConstruct)), prefix: null,
                new HarmonyMethod(patchType, nameof(CanConstructPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GameRules), nameof(GameRules.DesignatorAllowed)), prefix: null,
                new HarmonyMethod(patchType, nameof(DesignatorAllowedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew)), prefix: null,
                new HarmonyMethod(patchType, nameof(PawnAllowedToStartAnewPostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GrowerHarvest), nameof(WorkGiver_GrowerHarvest.HasJobOnCell)), prefix: null,
                new HarmonyMethod(patchType, nameof(HasJobOnCellHarvestPostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GrowerSow), name: "ExtraRequirements"), prefix: null,
                new HarmonyMethod(patchType, nameof(ExtraRequirementsGrowerSowPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.SetFaction)), prefix: null, new HarmonyMethod(patchType, nameof(SetFactionPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Pawn.SetFactionDirect)), prefix: null,
                new HarmonyMethod(patchType, nameof(SetFactionDirectPostfix)));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreGain_NewTmp)), prefix: null,
                new HarmonyMethod(patchType, nameof(ApparelScoreGainPostFix)));
            harmony.Patch(AccessTools.Method(typeof(ThoughtUtility), nameof(ThoughtUtility.CanGetThought_NewTemp)), prefix: null,
                new HarmonyMethod(patchType, nameof(CanGetThoughtPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Corpse), nameof(Corpse.ButcherProducts)), new HarmonyMethod(patchType, nameof(ButcherProductsPrefix)));
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting)), prefix: null,
                new HarmonyMethod(patchType, nameof(ThoughtsFromIngestingPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new[] { typeof(Thought_Memory), typeof(Pawn) }),
                new HarmonyMethod(patchType, nameof(TryGainMemoryPrefix)));
            harmony.Patch(AccessTools.Method(typeof(SituationalThoughtHandler), name: "TryCreateThought"),
                new HarmonyMethod(patchType, nameof(TryCreateThoughtPrefix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.RemoveMemoriesOfDef)),
                          new HarmonyMethod(patchType, nameof(ThoughtReplacementPrefix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.RemoveMemoriesOfDefIf)),
                          new HarmonyMethod(patchType, nameof(ThoughtReplacementPrefix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.RemoveMemoriesOfDefWhereOtherPawnIs)),
                          new HarmonyMethod(patchType, nameof(ThoughtReplacementPrefix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.OldestMemoryOfDef)),
                          new HarmonyMethod(patchType, nameof(ThoughtReplacementPrefix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.NumMemoriesOfDef)),
                          new HarmonyMethod(patchType, nameof(ThoughtReplacementPrefix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.GetFirstMemoryOfDef)),
                          new HarmonyMethod(patchType, nameof(ThoughtReplacementPrefix)));

            harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly(typeName: "AgeInjuryUtility"), name: "GenerateRandomOldAgeInjuries"),
                new HarmonyMethod(patchType, nameof(GenerateRandomOldAgeInjuriesPrefix)));
            harmony.Patch(
                AccessTools.Method(GenTypes.GetTypeInAnyAssembly(typeName: "AgeInjuryUtility"), name: "RandomHediffsToGainOnBirthday", new[] { typeof(ThingDef), typeof(int) }),
                prefix: null, new HarmonyMethod(patchType, nameof(RandomHediffsToGainOnBirthdayPostfix)));
            //            harmony.Patch(original: AccessTools.Property(type: typeof(JobDriver), name: nameof(JobDriver.Posture)).GetGetMethod(nonPublic: false), prefix: null,
            //                postfix: new HarmonyMethod(type: patchType, name: nameof(PosturePostfix)));
            //            harmony.Patch(original: AccessTools.Property(type: typeof(JobDriver_Skygaze), name: nameof(JobDriver_Skygaze.Posture)).GetGetMethod(nonPublic: false), prefix: null,
            //                postfix: new HarmonyMethod(type: patchType, name: nameof(PosturePostfix)));
                            
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateRandomAge"), new HarmonyMethod(patchType, nameof(GenerateRandomAgePrefix)));
            
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateTraits"), postfix: new HarmonyMethod(patchType, nameof(GenerateTraitsPostfix)),
                          transpiler: new HarmonyMethod(patchType, nameof(GenerateTraitsTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_SatisfyChemicalNeed), name: "DrugValidator"), prefix: null,
                          new HarmonyMethod(patchType, nameof(DrugValidatorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(CompDrug), nameof(CompDrug.PostIngested)), prefix: null,
                new HarmonyMethod(patchType, nameof(PostIngestedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(AddictionUtility), nameof(AddictionUtility.CanBingeOnNow)), prefix: null,
                new HarmonyMethod(patchType, nameof(CanBingeNowPostfix)));
                
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateBodyType_NewTemp"), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerateBodyTypePostfix)));
            harmony.Patch(AccessTools.Property(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.SkinColor)).GetGetMethod(), prefix: null,
                new HarmonyMethod(patchType, nameof(SkinColorPostfix)));
                
            harmony.Patch(AccessTools.Method(typeof(PawnHairChooser), nameof(PawnHairChooser.RandomHairDefFor)),
                new HarmonyMethod(patchType, nameof(RandomHairDefForPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), name: "BirthdayBiological"), new HarmonyMethod(patchType, nameof(BirthdayBiologicalPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) }),
                new HarmonyMethod(patchType, nameof(GeneratePawnPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                new HarmonyMethod(patchType, nameof(ResolveAllGraphicsPrefix)));
            //HarmonyInstance.DEBUG = true;
                            
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), name: "RenderPawnInternal",
                                             new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) }), prefix: null, postfix: null,
                new HarmonyMethod(patchType, nameof(RenderPawnInternalTranspiler)));
            //HarmonyInstance.DEBUG = false;
            harmony.Patch(AccessTools.Method(typeof(StartingPawnUtility), nameof(StartingPawnUtility.NewGeneratedStartingPawn)),
                new HarmonyMethod(patchType, nameof(NewGeneratedStartingPawnPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo)), prefix: null,
                new HarmonyMethod(patchType, nameof(GiveAppropriateBioAndNameToPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName)),
                new HarmonyMethod(patchType, nameof(GeneratePawnNamePrefix)));
            harmony.Patch(AccessTools.Method(typeof(Page_ConfigureStartingPawns), name: "CanDoNext"), prefix: null,
                new HarmonyMethod(patchType, nameof(CanDoNextStartPawnPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GameInitData), nameof(GameInitData.PrepForMapGen)),
                new HarmonyMethod(patchType, nameof(PrepForMapGenPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor)), prefix: null, postfix: null,
                new HarmonyMethod(patchType, nameof(SecondaryLovinChanceFactorTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.CompatibilityWith)), prefix: null,
                new HarmonyMethod(patchType, nameof(CompatibilityWithPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.TryMakeInitialRelationsWith)), prefix: null,
                new HarmonyMethod(patchType, nameof(TryMakeInitialRelationsWithPostfix)));
            harmony.Patch(AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait)), new HarmonyMethod(patchType, nameof(GainTraitPrefix)));
            harmony.Patch(AccessTools.Method(typeof(TraderCaravanUtility), nameof(TraderCaravanUtility.GetTraderCaravanRole)), prefix: null, postfix: null,
                new HarmonyMethod(patchType, nameof(GetTraderCaravanRoleTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(RestUtility), nameof(RestUtility.CanUseBedEver)), prefix: null,
                new HarmonyMethod(patchType, nameof(CanUseBedEverPostfix)));
            harmony.Patch(AccessTools.Property(typeof(CompAssignableToPawn), nameof(CompAssignableToPawn.AssigningCandidates)).GetGetMethod(), prefix: null,
                new HarmonyMethod(patchType, nameof(AssigningCandidatesPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GrammarUtility), nameof(GrammarUtility.RulesForPawn), new[] { typeof(string), typeof(Pawn), typeof(Dictionary<string, string>), typeof(bool), typeof(bool) }), prefix: null,
                new HarmonyMethod(patchType, nameof(RulesForPawnPostfix)));
            harmony.Patch(AccessTools.Method(typeof(RaceProperties), nameof(RaceProperties.CanEverEat), new[] { typeof(ThingDef) }), prefix: null,
                new HarmonyMethod(patchType, nameof(CanEverEat)));
            harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttackDamage), name: "DamageInfosToApply"), prefix: null,
                new HarmonyMethod(patchType, nameof(DamageInfosToApplyPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnWeaponGenerator), nameof(PawnWeaponGenerator.TryGenerateWeaponFor)),
                new HarmonyMethod(patchType, nameof(TryGenerateWeaponForPrefix)), new HarmonyMethod(patchType, nameof(TryGenerateWeaponForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.GenerateStartingApparelFor)),
                new HarmonyMethod(patchType, nameof(GenerateStartingApparelForPrefix)),
                new HarmonyMethod(patchType, nameof(GenerateStartingApparelForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateInitialHediffs"), prefix: null,
                new HarmonyMethod(patchType, nameof(GenerateInitialHediffsPostfix)));
            harmony.Patch(
                typeof(HediffSet).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(predicate: mi => mi.ReturnType == typeof(bool) && mi.GetParameters().First().ParameterType == typeof(BodyPartRecord)), prefix: null,
                new HarmonyMethod(patchType, nameof(HasHeadPostfix)));
            harmony.Patch(AccessTools.Property(typeof(HediffSet), nameof(HediffSet.HasHead)).GetGetMethod(),
                new HarmonyMethod(patchType, nameof(HasHeadPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), name: "RecalculateLifeStageIndex"), prefix: null,
                new HarmonyMethod(patchType, nameof(RecalculateLifeStageIndexPostfix)));
            harmony.Patch(
                AccessTools.GetDeclaredMethods(typeof(FactionGenerator).GetNestedTypes(AccessTools.all)
                                                                                    .OrderBy(keySelector: t => t.GetMethods(AccessTools.all).Length).Skip(count: 1).First()).Where(predicate: mi => mi.GetParameters()[0].ParameterType == typeof(Faction))
                                               .MaxBy(selector: mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1), prefix: null, new HarmonyMethod(patchType, nameof(EnsureRequiredEnemiesPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.FactionTick)), prefix: null, postfix: null,
                new HarmonyMethod(patchType, nameof(FactionTickTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Designator), nameof(Designator.CanDesignateThing)), prefix: null,
                new HarmonyMethod(patchType, nameof(CanDesignateThingTamePostfix)));

            harmony.Patch(AccessTools.Method(typeof(WorkGiver_InteractAnimal), name: "CanInteractWithAnimal"), prefix: null,
                new HarmonyMethod(patchType, nameof(CanInteractWithAnimalPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt)), 
                          postfix: new HarmonyMethod(patchType, nameof(BaseHeadOffsetAtPostfix)),
                transpiler: new HarmonyMethod(patchType, nameof(BaseHeadOffsetAtTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), name: "CheckForStateChange"), prefix: null,
                new HarmonyMethod(patchType, nameof(CheckForStateChangePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateGearFor"), prefix: null,
                          new HarmonyMethod(patchType, nameof(GenerateGearForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.ChangeKind)), new HarmonyMethod(patchType, nameof(ChangeKindPrefix)));

            harmony.Patch(AccessTools.Method(typeof(EditWindow_TweakValues), nameof(EditWindow_TweakValues.DoWindowContents)), transpiler: new HarmonyMethod(patchType, nameof(TweakValuesTranspiler)));

            HarmonyMethod misandryMisogonyTranspiler = new HarmonyMethod(patchType, nameof(MisandryMisogynyTranspiler));
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Woman), name: "CurrentSocialStateInternal"), transpiler: misandryMisogonyTranspiler);
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Man), name: "CurrentSocialStateInternal"), transpiler: misandryMisogonyTranspiler);

            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip_NewTmp), new []{typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool)}), postfix: new HarmonyMethod(patchType, nameof(CanEquipPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "GiveShuffledBioTo"), transpiler: 
                          new HarmonyMethod(patchType, nameof(MinAgeForAdulthood)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "TryGiveSolidBioTo"), transpiler:
                          new HarmonyMethod(patchType, nameof(MinAgeForAdulthood)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "TryGenerateNewPawnInternal"), transpiler: new HarmonyMethod(patchType, nameof(TryGenerateNewPawnTranspiler)));

            foreach (ThingDef_AlienRace ar in DefDatabase<ThingDef_AlienRace>.AllDefsListForReading)
            {
                foreach (ThoughtDef thoughtDef in ar.alienRace.thoughtSettings.restrictedThoughts)
                {
                    if (!ThoughtSettings.thoughtRestrictionDict.ContainsKey(thoughtDef))
                        ThoughtSettings.thoughtRestrictionDict.Add(thoughtDef, new List<ThingDef_AlienRace>());
                    ThoughtSettings.thoughtRestrictionDict[thoughtDef].Add(ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.apparelList)
                {
                    if (!RaceRestrictionSettings.apparelRestrictionDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.apparelRestrictionDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.apparelRestrictionDict[thingDef].Add(ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whiteApparelList)
                {
                    if (!RaceRestrictionSettings.apparelWhiteDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.apparelWhiteDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.apparelWhiteDict[thingDef].Add(ar);
                }


                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.weaponList)
                {
                    if (!RaceRestrictionSettings.weaponRestrictionDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.weaponRestrictionDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.weaponRestrictionDict[thingDef].Add(ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whiteWeaponList)
                {
                    if (!RaceRestrictionSettings.weaponWhiteDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.weaponWhiteDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.weaponWhiteDict[thingDef].Add(ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.buildingList)
                {
                    if (!RaceRestrictionSettings.buildingRestrictionDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.buildingRestrictionDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.buildingRestrictionDict[thingDef].Add(ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whiteBuildingList)
                {
                    if (!RaceRestrictionSettings.buildingWhiteDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.buildingWhiteDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.buildingWhiteDict[thingDef].Add(ar);
                }

                foreach (RecipeDef recipeDef in ar.alienRace.raceRestriction.recipeList)
                {
                    if (!RaceRestrictionSettings.recipeRestrictionDict.ContainsKey(recipeDef))
                        RaceRestrictionSettings.recipeRestrictionDict.Add(recipeDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.recipeRestrictionDict[recipeDef].Add(ar);
                }

                foreach (RecipeDef recipeDef in ar.alienRace.raceRestriction.whiteRecipeList)
                {
                    if (!RaceRestrictionSettings.recipeWhiteDict.ContainsKey(recipeDef))
                        RaceRestrictionSettings.recipeWhiteDict.Add(recipeDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.recipeWhiteDict[recipeDef].Add(ar);
                }


                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.plantList)
                {
                    if (!RaceRestrictionSettings.plantRestrictionDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.plantRestrictionDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.plantRestrictionDict[thingDef].Add(ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whitePlantList)
                {
                    if (!RaceRestrictionSettings.plantWhiteDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.plantWhiteDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.plantWhiteDict[thingDef].Add(ar);
                }


                foreach (TraitDef traitDef in ar.alienRace.raceRestriction.traitList)
                {
                    if (!RaceRestrictionSettings.traitRestrictionDict.ContainsKey(traitDef))
                        RaceRestrictionSettings.traitRestrictionDict.Add(traitDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.traitRestrictionDict[traitDef].Add(ar);
                }

                foreach (TraitDef traitDef in ar.alienRace.raceRestriction.whiteTraitList)
                {
                    if (!RaceRestrictionSettings.traitWhiteDict.ContainsKey(traitDef))
                        RaceRestrictionSettings.traitWhiteDict.Add(traitDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.traitWhiteDict[traitDef].Add(ar);
                }


                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.foodList)
                {
                    if (!RaceRestrictionSettings.foodRestrictionDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.foodRestrictionDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.foodRestrictionDict[thingDef].Add(ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whiteFoodList)
                {
                    if (!RaceRestrictionSettings.foodWhiteDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.foodWhiteDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.foodWhiteDict[thingDef].Add(ar);
                }


                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.petList)
                {
                    if (!RaceRestrictionSettings.tameRestrictionDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.tameRestrictionDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.tameRestrictionDict[thingDef].Add(ar);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.whitePetList)
                {
                    if (!RaceRestrictionSettings.tameWhiteDict.ContainsKey(thingDef))
                        RaceRestrictionSettings.tameWhiteDict.Add(thingDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.tameWhiteDict[thingDef].Add(ar);
                }

                foreach (ResearchProjectDef projectDef in ar.alienRace.raceRestriction.researchList.SelectMany(selector: rl => rl?.projects))
                {
                    if (!RaceRestrictionSettings.researchRestrictionDict.ContainsKey(projectDef))
                        RaceRestrictionSettings.researchRestrictionDict.Add(projectDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.researchRestrictionDict[projectDef].Add(ar);
                }


                ThingCategoryDefOf.CorpsesHumanlike.childThingDefs.Remove(ar.race.corpseDef);
                ar.race.corpseDef.thingCategories = new List<ThingCategoryDef> {AlienDefOf.alienCorpseCategory};
                AlienDefOf.alienCorpseCategory.childThingDefs.Add(ar.race.corpseDef);
                ar.alienRace.generalSettings.alienPartGenerator.GenerateMeshsAndMeshPools();

                if (ar.alienRace.generalSettings.humanRecipeImport && ar != ThingDefOf.Human)
                {
                    (ar.recipes ?? (ar.recipes = new List<RecipeDef>())).AddRange(ThingDefOf.Human.recipes.Where(predicate: rd => !rd.targetsBodyPart ||
                                                        rd.appliedOnFixedBodyParts.NullOrEmpty() ||
                                                        rd.appliedOnFixedBodyParts.Any(predicate: bpd => ar.race.body.AllParts.Any(predicate: bpr => bpr.def == bpd))));

                    DefDatabase<RecipeDef>.AllDefsListForReading.ForEach(action: rd =>
                                                                                 {
                                                                                     if (rd.recipeUsers?.Contains(ThingDefOf.Human) ?? false)
                                                                                         rd.recipeUsers.Add(ar);
                                                                                     if (!rd.defaultIngredientFilter?.Allows(ThingDefOf.Meat_Human) ?? false)
                                                                                         rd.defaultIngredientFilter.SetAllow(ar.race.meatDef, allow: false);
                                                                                 });
                    ar.recipes.RemoveDuplicates();
                }

                ar.alienRace.raceRestriction?.workGiverList?.ForEach(action: wgd =>
                                                                             {
                                                                                 if (wgd == null)
                                                                                     return;
                                                                                 harmony.Patch(AccessTools.Method(wgd.giverClass, name: "JobOnThing"),
                                                                                               postfix: new HarmonyMethod(patchType, nameof(GenericJobOnThingPostfix)));
                                                                                 MethodInfo hasJobOnThingInfo = AccessTools.Method(wgd.giverClass, name: "HasJobOnThing");
                                                                                 if (hasJobOnThingInfo?.IsDeclaredMember() ?? false)
                                                                                     harmony.Patch(hasJobOnThingInfo, postfix: new HarmonyMethod(patchType, nameof(GenericHasJobOnThingPostfix)));
                                                                             });
            }

            {
                FieldInfo bodyInfo = AccessTools.Field(typeof(RaceProperties), nameof(RaceProperties.body));
                MethodInfo bodyCheck = AccessTools.Method(patchType, nameof(ReplacedBody));
                HarmonyMethod bodyTranspiler = new HarmonyMethod(patchType, nameof(BodyReferenceTranspiler));


                //Full assemblies scan
                foreach (MethodInfo mi in typeof(LogEntry).Assembly.GetTypes().
                    //SelectMany(t => t.GetNestedTypes(AccessTools.all).Concat(t)).
                    Where(predicate: t => (!t.IsAbstract || t.IsSealed) && !typeof(Delegate).IsAssignableFrom(t) && !t.IsGenericType && !t.HasAttribute<CompilerGeneratedAttribute>()).SelectMany(selector: t =>
                       t.GetMethods(AccessTools.all).Concat(t.GetProperties(AccessTools.all).SelectMany(selector: pi => pi.GetAccessors(nonPublic: true)))
                        .Where(predicate: mi => mi != null && !mi.IsAbstract && mi.DeclaringType == t && !mi.IsGenericMethod && !mi.HasAttribute<System.Runtime.InteropServices.DllImportAttribute>()))// && mi.GetMethodBody()?.GetILAsByteArray()?.Length > 1))
                ) //.Select(mi => mi.IsGenericMethod ? mi.MakeGenericMethod(mi.GetGenericArguments()) : mi))
                {
                    IEnumerable<KeyValuePair<OpCode, object>> instructions = PatchProcessor.ReadMethodBody(mi);
                    if (mi != bodyCheck && instructions.Any(predicate: il => il.Value?.Equals(bodyInfo) ?? false))
                        harmony.Patch(mi, prefix: null, postfix: null, bodyTranspiler);
                }

                //PawnRenderer Posture scan
                MethodInfo postureInfo = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetPosture));

                foreach (MethodInfo mi in typeof(PawnRenderer).GetMethods(AccessTools.all).Concat(typeof(PawnRenderer).GetProperties(AccessTools.all)
                                                                                                                                        .SelectMany(selector: pi =>
                                                                                                                                                                  new List<MethodInfo> { pi.GetGetMethod(nonPublic: true), pi.GetGetMethod(nonPublic: false), pi.GetSetMethod(nonPublic: true), pi.GetSetMethod(nonPublic: false) }))
                   .Where(predicate: mi => mi != null && mi.DeclaringType == typeof(PawnRenderer) && !mi.IsGenericMethod))
                {
                    IEnumerable<KeyValuePair<OpCode, object>> instructions = PatchProcessor.ReadMethodBody(mi);
                    if (instructions.Any(predicate: il => il.Value?.Equals(postureInfo) ?? false))
                        harmony.Patch(mi, prefix: null, postfix: null, new HarmonyMethod(patchType, nameof(PostureTranspiler)));
                }
            }

            Log.Message($"Alien race successfully completed {harmony.GetPatchedMethods().Select(Harmony.GetPatchInfo).SelectMany(selector: p => p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers)).Count(predicate: p => p.owner == harmony.Id)} patches with harmony.");
            DefDatabase<HairDef>.GetNamed(defName: "Shaved").hairTags.Add(item: "alienNoHair");

            foreach (BackstoryDef bd in DefDatabase<BackstoryDef>.AllDefs)
                BackstoryDef.UpdateTranslateableFields(bd);

            AlienRaceMod.settings.UpdateSettings();
        }

        public static IEnumerable<CodeInstruction> TryGenerateNewPawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            MethodInfo newbornInfo = AccessTools.PropertyGetter(typeof(PawnGenerationRequest), nameof(PawnGenerationRequest.Newborn));

            bool done = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (!done && instructionList[i + 1].OperandIs(newbornInfo))
                {
                    done = true;
                    i += 8;
                } else 
                    yield return instruction;
            }
        }

        public static void ThoughtReplacementPrefix(MemoryThoughtHandler __instance, ref ThoughtDef def)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is ThingDef_AlienRace race)
                def = race.alienRace.thoughtSettings.ReplaceIfApplicable(def);
        }

        public static IEnumerable<CodeInstruction> MinAgeForAdulthood(IEnumerable<CodeInstruction> instructions)
        {
            float value = (float) AccessTools.Field(typeof(PawnBioAndNameGenerator), name: "MinAgeForAdulthood").GetValue(obj: null);

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && instruction.OperandIs(value))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(GetMinAgeForAdulthood)));
                }
                else
                    yield return instruction;
            }
        }

        public static float GetMinAgeForAdulthood(Pawn pawn, float value) => 
            pawn.def is ThingDef_AlienRace alienProps ? alienProps.alienRace.generalSettings.minAgeForAdulthood : value;

        public static IEnumerable<CodeInstruction> MisandryMisogynyTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            FieldInfo defInfo = AccessTools.Field(typeof(Thing), nameof(Thing.def));

            bool yield = true;
            
            foreach (CodeInstruction instruction in instructionList)
            {
                if (yield && instruction.OperandIs(defInfo))
                    yield = false;

                if (yield)
                    yield return instruction;

                if (!yield && instruction.opcode == OpCodes.Ldarg_2)
                    yield = true;
            }
        }

        public static IEnumerable<CodeInstruction> TweakValuesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo endScrollInfo = AccessTools.Method(typeof(Widgets), nameof(Widgets.EndScrollView));

            MethodInfo countInfo = AccessTools.Property(
                AccessTools.Field(typeof(EditWindow_TweakValues), name: "tweakValueFields").FieldType, 
                nameof(List<Graphic_Random>.Count)).GetGetMethod();

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.OperandIs(endScrollInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1) { labels = instruction.labels.ListFullCopy() };
                    instruction.labels.Clear();
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, operand: 4);
                    yield return new CodeInstruction(OpCodes.Call, 
                        AccessTools.Method(patchType, nameof(TweakValuesInstanceBased)));
                }

                yield return instruction;

                if (instruction.OperandIs(countInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, DefDatabase<ThingDef_AlienRace>.AllDefs.SelectMany(selector: ar =>
                            ar.alienRace.generalSettings.alienPartGenerator.bodyAddons).Sum(selector: ba =>
                                                                                                          new List<AlienPartGenerator.RotationOffset>()
                                                                                                          {
                                                                                                              ba.offsets.east,
                                                                                                              ba.offsets.west,
                                                                                                              ba.offsets.north,
                                                                                                              ba.offsets.south,
                                                                                                          }.Sum(selector: ro => (ro.bodyTypes?.Count ?? 0) * 2 + (ro.crownTypes?.Count ?? 0) * 2/* + (ro.portraitBodyTypes?.Count ?? 0) * 2 + 
                                                   (ro.crownTypes?.Count ?? 0) * 2 + (ro.portraitCrownTypes?.Count ?? 0) * 2*/) + 1) + 1);
                    yield return new CodeInstruction(OpCodes.Add);
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

            rect5.y += rect2.height / 3f;

            foreach (ThingDef_AlienRace ar in DefDatabase<ThingDef_AlienRace>.AllDefs)
            {
                string label2 = ar.LabelCap;
                foreach (AlienPartGenerator.BodyAddon ba in ar.alienRace.generalSettings.alienPartGenerator.bodyAddons)
                {
                    string label3Addons = $"{Path.GetFileName(ba.path)}.";

                    float WriteLine(float value, string label)
                    {
                        Widgets.Label(rect2, label2);
                        Widgets.Label(rect3, label3Addons + label);


                        float num = Widgets.HorizontalSlider(rect5, value, leftValue: -1, rightValue: 1);

                        string valueS = value.ToString(CultureInfo.InvariantCulture);
                        string num2 = Widgets.TextField(rect4.ContractedBy(margin: 2).LeftPartPixels(Text.CalcSize(valueS).x + 6*3), valueS);
                        
                        if (Mathf.Abs(num-value)<float.Epsilon)
                            if (float.TryParse(num2, out float num3))
                                num = num3;

                        //Widgets.Label(rect: rect4, label: value.ToString(provider: CultureInfo.InvariantCulture));
                        return Mathf.Clamp(num, min: -1, max: 1);
                    }


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
                        AlienPartGenerator.RotationOffset ro = rotationOffsets[i];
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
                        if(!ro.bodyTypes.NullOrEmpty())
                            foreach (AlienPartGenerator.BodyTypeOffset bodyTypeOffset in ro.bodyTypes)
                            {
                                string label3Type = bodyTypeOffset.bodyType.defName + ".";
                                Vector2 offset = bodyTypeOffset.offset;
                                float offsetX = offset.x;
                                float offsetY = offset.y;

                                float WriteAddonLine(float value, bool x) => 
                                    WriteLine(value, label3Rotation + label3Type + (x ? "x" : "y"));


                                bodyTypeOffset.offset.x = WriteAddonLine(offsetX, x: true);
                                NextLine();
                                bodyTypeOffset.offset.y = WriteAddonLine(offsetY, x: false);
                                NextLine();
                            }

                        if(!ro.crownTypes.NullOrEmpty())
                            foreach (AlienPartGenerator.CrownTypeOffset crownTypeOffsets in ro.crownTypes)
                            {
                                string  label3Type = crownTypeOffsets.crownType + ".";
                                Vector2 offset     = crownTypeOffsets.offset;
                                float   offsetX    = offset.x;
                                float   offsetY    = offset.y;

                                float WriteAddonLine(float value, bool x) =>
                                    WriteLine(value, label3Rotation + label3Type + (x ? "x" : "y"));


                                crownTypeOffsets.offset.x = WriteAddonLine(offsetX, x: true);
                                NextLine();
                                crownTypeOffsets.offset.y = WriteAddonLine(offsetY, x: false);
                                NextLine();
                            }
                    }

                    ba.layerOffset = WriteLine(ba.layerOffset, label: "layerOffset");
                    NextLine();
                }
            }
        }

        public static IEnumerable<CodeInstruction> PostureTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo postureInfo = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetPosture));

            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            foreach (CodeInstruction instruction in codeInstructions)
                if (instruction.opcode == OpCodes.Call && instruction.OperandIs(postureInfo))
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(PostureTweak)));
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

        public static IEnumerable<CodeInstruction> BodyReferenceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo bodyInfo  = AccessTools.Field(typeof(RaceProperties), nameof(RaceProperties.body));
            FieldInfo propsInfo = AccessTools.Field(typeof(ThingDef),       nameof(ThingDef.race));
            FieldInfo defInfo = AccessTools.Field(typeof(Thing), nameof(Thing.def));
            {
                List<CodeInstruction> instructionList = instructions.ToList();
                for (int i = 0; i < instructionList.Count; i++)
                {
                    CodeInstruction instruction = instructionList[i];

                    if (i < instructionList.Count - 2 && instructionList[i + 2].OperandIs(bodyInfo) && instructionList[i + 1].OperandIs(propsInfo) && instruction.OperandIs(defInfo))
                    {
                        instruction =  new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(ReplacedBody)));
                        i           += 2;
                    }

                    if (i < instructionList.Count - 1 && instructionList[i + 1].OperandIs(bodyInfo) && instruction.OperandIs(defInfo))
                    {
                        instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(ReplacedBody)));
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
            pawn.story?.AllBackstories?.Select(selector: bs => DefDatabase<BackstoryDef>.GetNamedSilentFail(bs.identifier)).Where(predicate: bs => bs != null)
               .SelectMany(selector: bd => bd.forcedItems).Concat(bioReference?.forcedItems ?? new List<ThingDefCountRangeClass>(capacity: 0)).Do(action: tdcrc =>
               {
                   int count = tdcrc.countRange.RandomInRange;

                   while(count > 0)
                   {
                       Thing thing = ThingMaker.MakeThing(tdcrc.thingDef, GenStuff.RandomStuffFor(tdcrc.thingDef));
                       thing.stackCount = Mathf.Min(count, tdcrc.thingDef.stackLimit);
                       count -= thing.stackCount;
                       pawn.inventory?.TryAddItemNotForSale(thing);
                   }
               });

        public static void CheckForStateChangePostfix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field(name: "pawn").GetValue<Pawn>();
            if (Current.ProgramState == ProgramState.Playing && pawn.Spawned && pawn.def is ThingDef_AlienRace)
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public static IEnumerable<CodeInstruction> BaseHeadOffsetAtTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo offsetInfo = AccessTools.Field(typeof(BodyTypeDef), nameof(BodyTypeDef.headOffset));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (!instruction.OperandIs(offsetInfo)) continue;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), name: "pawn"));
                yield return new CodeInstruction(OpCodes.Call,  AccessTools.Method(patchType, nameof(BaseHeadOffsetAtHelper)));
            }
        }

        public static Vector2 BaseHeadOffsetAtHelper(Vector2 offset, Pawn pawn) => 
            offset + ((pawn.def as ThingDef_AlienRace)?.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).headOffset ?? Vector2.zero);

        public static void BaseHeadOffsetAtPostfix(ref Vector3 __result, Rot4 rotation, PawnRenderer __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field(name: "pawn").GetValue<Pawn>();
            Vector2 offset = (pawn.def as ThingDef_AlienRace)?.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).headOffsetDirectional?.GetOffset(rotation) ?? Vector2.zero;
            __result += new Vector3(offset.x, y: 0, offset.y);
        }

        public static void CanInteractWithAnimalPostfix(ref bool __result, Pawn pawn, Pawn animal) =>
            __result = __result && RaceRestrictionSettings.CanTame(animal.def, pawn.def);

        public static void CanDesignateThingTamePostfix(Designator __instance, ref AcceptanceReport __result, Thing t)
        {
            if (!__result.Accepted || !(__instance is Designator_Build)) return;

            __result = colonistRaces.Any(predicate: td => RaceRestrictionSettings.CanTame(t.def, td));
        }

        public static IEnumerable<CodeInstruction> FactionTickTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode != OpCodes.Beq) continue;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call,    AccessTools.Method(patchType, nameof(FactionTickFactionRelationCheck)));
                yield return new CodeInstruction(OpCodes.Brfalse, instruction.operand);
            }
        }

        private static bool FactionTickFactionRelationCheck(Faction f)
        {
            FactionDef player = Faction.OfPlayerSilentFail?.def ?? Find.GameInitData.playerFaction.def;
            return !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(predicate: ar =>
                       f.def?.basicMemberKind?.race == ar &&
                       (ar?.alienRace.generalSettings?.factionRelations?.Any(predicate: frs => frs.factions?.Contains(player) ?? false) ?? false) ||
                       player.basicMemberKind?.race == ar &&
                       (ar?.alienRace.generalSettings?.factionRelations?.Any(predicate: frs => frs.factions?.Contains(f.def) ?? false) ?? false));
        }

        public static void EnsureRequiredEnemiesPostfix(ref bool __result, Faction f) => __result = __result ||
                                                                                                    !FactionTickFactionRelationCheck(f);

        public static void RecalculateLifeStageIndexPostfix(Pawn_AgeTracker __instance)
        {
            Pawn pawn;
            if (Current.ProgramState == ProgramState.Playing && (pawn = Traverse.Create(__instance).Field(name: "pawn").GetValue<Pawn>()).def is ThingDef_AlienRace &&
                pawn.Drawer.renderer.graphics.AllResolved)
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public static void HasHeadPrefix(HediffSet __instance) =>
            headPawnDef = (__instance.pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.headBodyPartDef;

        private static BodyPartDef headPawnDef;

        public static void HasHeadPostfix(BodyPartRecord x, ref bool __result) =>
            __result = headPawnDef != null ? x.def == headPawnDef : __result;

        public static void GenerateInitialHediffsPostfix(Pawn pawn) =>
            pawn.story?.AllBackstories?.Select(selector: bs => DefDatabase<BackstoryDef>.GetNamedSilentFail(bs.identifier)).Where(predicate: bd => bd != null)
               .SelectMany(selector: bd => bd.forcedHediffs).Concat(bioReference?.forcedHediffs ?? new List<string>(capacity: 0)).Select(DefDatabase<HediffDef>.GetNamedSilentFail)
               .ToList().ForEach(action: hd =>
                {
                    BodyPartRecord bodyPartRecord = null;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(predicate: rd => rd.addsHediff == hd)?.appliedOnFixedBodyParts.SelectMany(selector: bpd =>
                            pawn.health.hediffSet.GetNotMissingParts().Where(predicate: bpr => bpr.def == bpd && !pawn.health.hediffSet.hediffs.Any(predicate: h => h.def == hd && h.Part == bpr)))
                       .TryRandomElement(out bodyPartRecord);
                    pawn.health.AddHediff(hd, bodyPartRecord);
                });

        public static void GenerateStartingApparelForPostfix() =>
            Traverse.Create(typeof(PawnApparelGenerator)).Field(name: "allApparelPairs").GetValue<List<ThingStuffPair>>().AddRange(apparelList);

        private static HashSet<ThingStuffPair> apparelList;

        public static void GenerateStartingApparelForPrefix(Pawn pawn)
        {
            Traverse apparelInfo = Traverse.Create(typeof(PawnApparelGenerator)).Field(name: "allApparelPairs");

            apparelList = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in apparelInfo.GetValue<List<ThingStuffPair>>().ListFullCopy())
            {
                ThingDef equipment = pair.thing;
                if (!RaceRestrictionSettings.CanWear(equipment, pawn.def))
                    apparelList.Add(pair);
            }

            apparelInfo.GetValue<List<ThingStuffPair>>().RemoveAll(match: tsp => apparelList.Contains(tsp));
        }

        public static void TryGenerateWeaponForPostfix() =>
            Traverse.Create(typeof(PawnWeaponGenerator)).Field(name: "allWeaponPairs").GetValue<List<ThingStuffPair>>().AddRange(weaponList);

        private static HashSet<ThingStuffPair> weaponList;

        public static void TryGenerateWeaponForPrefix(Pawn pawn)
        {
            Traverse weaponInfo = Traverse.Create(typeof(PawnWeaponGenerator)).Field(name: "allWeaponPairs");
            weaponList = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in weaponInfo.GetValue<List<ThingStuffPair>>().ListFullCopy())
            {
                ThingDef equipment = pair.thing;
                if (!RaceRestrictionSettings.CanEquip(equipment, pawn.def))
                    weaponList.Add(pair);
            }

            weaponInfo.GetValue<List<ThingStuffPair>>().RemoveAll(match: tsp => weaponList.Contains(tsp));
        }

        public static void DamageInfosToApplyPostfix(Verb __instance, ref IEnumerable<DamageInfo> __result)
        {
            if (__instance.CasterIsPawn && __instance.CasterPawn.def is ThingDef_AlienRace alienProps && __instance.CasterPawn.CurJob.def == JobDefOf.SocialFight)
                __result = __result.Select(selector: di =>
                    new DamageInfo(di.Def, Math.Min(di.Amount, alienProps.alienRace.generalSettings.maxDamageForSocialfight), angle: di.Angle, instigator: di.Instigator,
                        hitPart: di.HitPart, weapon: di.Weapon, category: di.Category));
        }

        public static void CanEverEat(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (!__instance.Humanlike) return;
            ThingDef eater = new List<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading).Concat(
                new List<ThingDef_AlienRace>(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading)).First(predicate: td => td.race == __instance);

            __result = __result && RaceRestrictionSettings.CanEat(t, eater);
        }
        
        public static IEnumerable<Rule> RulesForPawnPostfix(IEnumerable<Rule> __result, Pawn pawn, string pawnSymbol) =>
            __result.AddItem(new Rule_String(pawnSymbol + "_alienRace", pawn.def.LabelCap));

        public static IEnumerable<CodeInstruction> GenerateTraitsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            MethodInfo            defListInfo     = AccessTools.Property(typeof(DefDatabase<TraitDef>), nameof(DefDatabase<TraitDef>.AllDefsListForReading)).GetGetMethod();
            MethodInfo            validatorInfo   = AccessTools.Method(patchType, nameof(GenerateTraitsValidator));

            foreach (CodeInstruction instruction in instructionList)
            {
                if (instruction.opcode == OpCodes.Call && instruction.OperandIs(defListInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    instruction.operand = validatorInfo;
                }

                if (instruction.opcode == OpCodes.Stloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(AdditionalInitialTraits)));
                }

                yield return instruction;
            }
        }

        public static int AdditionalInitialTraits(int count, Pawn pawn)
        {
            if (!(pawn.def is ThingDef_AlienRace alienProps)) return count;

            return count + alienProps.alienRace.generalSettings.additionalTraits.RandomInRange;
        }

        public static IEnumerable<TraitDef> GenerateTraitsValidator(Pawn p) => DefDatabase<TraitDef>.AllDefs.Where(predicate: tr => 
            RaceRestrictionSettings.CanGetTrait(tr, p.def));

        public static void AssigningCandidatesPostfix(ref IEnumerable<Pawn> __result, CompAssignableToPawn __instance) =>
            __result = __result.Where(predicate: p => !(__instance is CompAssignableToPawn_Bed) || RestUtility.CanUseBedEver(p, __instance.parent.def));

        public static void CanUseBedEverPostfix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            if (__result)
                __result = p.def is ThingDef_AlienRace alienProps && (alienProps.alienRace.generalSettings.validBeds?.Contains(bedDef) ?? false) ||
                           !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(predicate: td => td.alienRace.generalSettings.validBeds?.Contains(bedDef) ?? false);
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
            MethodInfo traderRoleInfo = AccessTools.Method(patchType, nameof(GetTraderCaravanRoleInfix));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_3)
                {
                    Label jumpToEnd = il.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Ldarg_0) {labels = instruction.labels.ListFullCopy()};
                    instruction.labels.Clear();
                    yield return new CodeInstruction(OpCodes.Call,      traderRoleInfo);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, jumpToEnd);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_4);
                    yield return new CodeInstruction(OpCodes.Ret);
                    yield return new CodeInstruction(OpCodes.Nop) {labels = new List<Label> {jumpToEnd}};
                }

                yield return instruction;
            }
        }

        private static bool GetTraderCaravanRoleInfix(Pawn p) => 
            p.def is ThingDef_AlienRace && 
                DefDatabase<RaceSettings>.AllDefs.Any(predicate: rs => rs.pawnKindSettings.alienslavekinds.Any(predicate: pke => pke.kindDefs.Contains(p.kindDef)));

        public static bool GetGenderSpecificLabelPrefix(Pawn pawn, ref string __result, PawnRelationDef __instance)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                RelationRenamer ren = alienProps.alienRace.relationSettings.renamer?.FirstOrDefault(predicate: rn => rn.relation == __instance);
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
                    Log.Warning(string.Concat(new object[]
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
                        if (relationDef.generationChanceFactor > 0f) list.Add(new KeyValuePair<Pawn, PawnRelationDef>(current, relationDef));
                    });
            });

            KeyValuePair<Pawn, PawnRelationDef> keyValuePair = list.RandomElementByWeightWithDefault(weightSelector: x =>
                !x.Value.familyByBloodRelation ? 0f : GenerationChanceGenderless(x.Value, pawn, x.Key, localReq), defaultValueWeight: 82f);

            Pawn other = keyValuePair.Key;
            if (other != null) CreateRelationGenderless(keyValuePair.Value, pawn, other);
            KeyValuePair<Pawn, PawnRelationDef> keyValuePair2 = list.RandomElementByWeightWithDefault(weightSelector: x =>
                x.Value.familyByBloodRelation ? 0f : GenerationChanceGenderless(x.Value, pawn, x.Key, localReq), defaultValueWeight: 82f);
            other = keyValuePair2.Key;
            if (other != null) CreateRelationGenderless(keyValuePair2.Value, pawn, other);
            return false;
        }

        private static float GenerationChanceGenderless(PawnRelationDef relationDef, Pawn pawn, Pawn current, PawnGenerationRequest request)
        {
            float generationChance = relationDef.generationChanceFactor;
            float lifeExpectancy   = pawn.RaceProps.lifeExpectancy;

            if (relationDef == PawnRelationDefOf.Child)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(current, pawn,
                    current.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, predicate: p => p != pawn));
                GenerationChanceChildPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.ExLover)
            {
                generationChance = 0.5f;
                GenerationChanceExLoverPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.ExSpouse)
            {
                generationChance = 0.5f;
                GenerationChanceExSpousePostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Fiance)
            {
                generationChance =
                    Mathf.Clamp(GenMath.LerpDouble(lifeExpectancy / 1.6f, lifeExpectancy, outFrom: 1f, outTo: 0.01f, pawn.ageTracker.AgeBiologicalYearsFloat), min: 0.01f,
                        max: 1f) *
                    Mathf.Clamp(GenMath.LerpDouble(lifeExpectancy / 1.6f, lifeExpectancy, outFrom: 1f, outTo: 0.01f, current.ageTracker.AgeBiologicalYearsFloat), min: 0.01f,
                        max: 1f);
                GenerationChanceFiancePostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Lover)
            {
                generationChance = 0.5f;
                GenerationChanceLoverPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Parent)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(current, pawn,
                    current.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, predicate: p => p != pawn));
                GenerationChanceParentPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Sibling)
            {
                generationChance = ChanceOfBecomingGenderlessChildOf(current, pawn,
                    current.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, predicate: p => p != pawn));
                generationChance *= 0.65f;
                GenerationChanceSiblingPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Spouse)
            {
                generationChance = 0.5f;
                GenerationChanceSpousePostfix(ref generationChance, pawn, current);
            }

            return generationChance * relationDef.Worker.BaseGenerationChanceFactor(pawn, current, request);
        }

        private static void CreateRelationGenderless(PawnRelationDef relationDef, Pawn pawn, Pawn other)
        {
            if (relationDef == PawnRelationDefOf.Child)
            {
                Pawn parent = other.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent);
                if (parent != null)
                    pawn.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || Rand.Value > 0.8f ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Spouse,
                        parent);

                other.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
            }

            if (relationDef == PawnRelationDefOf.ExLover)
            {
                if (!pawn.GetRelations(other).Contains(PawnRelationDefOf.ExLover)) pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (p.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) < 2 && Rand.Value < 0.35)
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                });
            }

            if (relationDef == PawnRelationDefOf.ExSpouse)
            {
                pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (p.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) < 2 && Rand.Value < 1)
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                });
            }

            if (relationDef == PawnRelationDefOf.Fiance)
            {
                pawn.relations.AddDirectRelation(PawnRelationDefOf.Fiance, other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (p.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) < 2 && Rand.Value < 0.7)
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                });
            }

            if (relationDef == PawnRelationDefOf.Lover)
            {
                pawn.relations.AddDirectRelation(PawnRelationDefOf.Lover, other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (p.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) < 2 && Rand.Value < 0.35f)
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                });
            }

            if (relationDef == PawnRelationDefOf.Parent)
            {
                Pawn parent = other.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent);
                if (parent != null && pawn != parent && !pawn.GetRelations(parent).Contains(PawnRelationDefOf.ExLover))
                    pawn.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || Rand.Value > 0.8f ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Spouse,
                        parent);

                pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, other);
            }

            if (relationDef == PawnRelationDefOf.Sibling)
            {
                Pawn                     parent  = other.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent);
                List<DirectPawnRelation> dprs    = other.relations.DirectRelations.Where(predicate: dpr => dpr.def == PawnRelationDefOf.Parent && dpr.otherPawn != parent).ToList();
                Pawn                     parent2 = dprs.NullOrEmpty() ? null : dprs.First().otherPawn;

                if (parent == null)
                {
                    parent = PawnGenerator.GeneratePawn(other.kindDef,
                        Find.FactionManager.FirstFactionOfDef(other.kindDef.defaultFactionType) ?? Find.FactionManager.AllFactions.RandomElement());
                    if (!other.GetRelations(parent).Contains(PawnRelationDefOf.Parent)) other.relations.AddDirectRelation(PawnRelationDefOf.Parent, parent);
                }

                if (parent2 == null)
                {
                    parent2 = PawnGenerator.GeneratePawn(other.kindDef,
                        Find.FactionManager.FirstFactionOfDef(other.kindDef.defaultFactionType) ?? Find.FactionManager.AllFactions.RandomElement());
                    if (!other.GetRelations(parent2).Contains(PawnRelationDefOf.Parent)) other.relations.AddDirectRelation(PawnRelationDefOf.Parent, parent2);
                }

                if (!parent.GetRelations(parent2).Any(predicate: prd => prd == PawnRelationDefOf.ExLover || prd == PawnRelationDefOf.Lover))
                    parent.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || Rand.Value > 0.8 ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Lover,
                        parent2);

                if (!pawn.GetRelations(parent).Contains(PawnRelationDefOf.Parent) && pawn != parent) pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, parent);

                if (!pawn.GetRelations(parent2).Contains(PawnRelationDefOf.Parent) && pawn != parent2)
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, parent2);
            }

            if (relationDef != PawnRelationDefOf.Spouse) return;
            {
                if (!pawn.GetRelations(other).Contains(PawnRelationDefOf.Spouse)) pawn.relations.AddDirectRelation(PawnRelationDefOf.Spouse, other);

                other.relations.Children.ToList().ForEach(action: p =>
                {
                    if (pawn != p && p.relations.DirectRelations.Count(predicate: dpr => dpr.def                == PawnRelationDefOf.Parent) < 2     &&
                        p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, predicate: x => x == pawn)                     == null &&
                        Rand.Value                                                                                                           < 0.7)
                        p.relations.AddDirectRelation(PawnRelationDefOf.Parent, pawn);
                });
            }

        }

        private static float ChanceOfBecomingGenderlessChildOf(Pawn child, Pawn parent1, Pawn parent2)
        {
            if (child == null || parent1 == null || !(parent2 == null || child.relations.DirectRelations.Count(predicate: dpr => dpr.def == PawnRelationDefOf.Parent) > 1))
                return 0f;
            if (parent2 != null && !LovePartnerRelationUtility.LovePartnerRelationExists(parent1, parent2) &&
                !LovePartnerRelationUtility.ExLovePartnerRelationExists(parent1, parent2))
                return 0f;

            float    num2          = 1f;
            float    num3          = 1f;
            Traverse childRelation = Traverse.Create(typeof(ChildRelationUtility));

            float num = childRelation.Method(name: "GetParentAgeFactor", parent1, child, parent1.RaceProps.lifeExpectancy / 5f, parent1.RaceProps.lifeExpectancy / 2.5f,
                parent1.RaceProps.lifeExpectancy                                                                    / 1.6f).GetValue<float>();
            if (Math.Abs(num) < 0.001f) return 0f;
            if (parent2 != null)
            {
                num2 = childRelation.Method(name: "GetParentAgeFactor", parent2, child, parent1.RaceProps.lifeExpectancy / 5f, parent1.RaceProps.lifeExpectancy / 2.5f,
                    parent1.RaceProps.lifeExpectancy                                                               / 1.6f).GetValue<float>();
                if (Math.Abs(num2) < 0.001f) return 0f;
                num3 = 1f;
            }

            float num6                                                                      = 1f;
            Pawn  firstDirectRelationPawn                                                   = parent2?.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
            if (firstDirectRelationPawn != null && firstDirectRelationPawn != parent2) num6 *= 0.15f;
            if (parent2 == null) return num * num2 * num3 * num6;
            Pawn firstDirectRelationPawn2                                                     = parent2.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
            if (firstDirectRelationPawn2 != null && firstDirectRelationPawn2 != parent2) num6 *= 0.15f;
            return num * num2 * num3 * num6;

        }

        public static bool GainTraitPrefix(Trait trait, TraitSet __instance)
        {
            if (!(Traverse.Create(__instance).Field(name: "pawn").GetValue<Pawn>().def is ThingDef_AlienRace alienProps)) return true;

            if(!alienProps.alienRace.generalSettings.disallowedTraits.NullOrEmpty())
                foreach (AlienTraitEntry traitEntry in alienProps.alienRace.generalSettings.disallowedTraits)
                {
                    if (traitEntry.defName == trait.def)
                    {
                        if (trait.Degree == traitEntry.degree || traitEntry.degree == 0)
                        {
                            if (Rand.Range(min: 0, max: 100) < traitEntry.chance)
                                return false;
                        }
                    }
                }

            AlienTraitEntry ate = alienProps.alienRace.generalSettings.forcedRaceTraitEntries?.FirstOrDefault(predicate: at => at.defName == trait.def);
            if (ate == null) return true;

            return Rand.Range(min: 0, max: 100) < ate.chance;
        }

        public static void TryMakeInitialRelationsWithPostfix(Faction __instance, Faction other)
        {
            ThingDef_AlienRace GetRaceOfFaction(FactionDef fac) =>
                (fac.basicMemberKind?.race ?? fac.pawnGroupMakers?.SelectMany(selector: pgm => pgm.options).GroupBy(keySelector: pgm => pgm.kind.race).OrderByDescending(keySelector: g => g.Count()).First().Key) as ThingDef_AlienRace;

            ThingDef_AlienRace alienRace = GetRaceOfFaction(other.def);
            alienRace?.alienRace.generalSettings.factionRelations?.ForEach(action: frs =>
                {
                    if (!frs.factions.Contains(__instance.def)) return;

                    int           offset = frs.goodwill.RandomInRange;

                    FactionRelationKind kind = offset > 75 ?
                                                   FactionRelationKind.Ally :
                                                   offset <= -10 ?
                                                       FactionRelationKind.Hostile :
                                                       FactionRelationKind.Neutral;

                    FactionRelation relation = other.RelationWith(__instance);
                    relation.goodwill = offset;
                    relation.kind = kind;

                    relation = __instance.RelationWith(other);
                    relation.goodwill = offset;
                    relation.goodwill = offset;
                    relation.kind = kind;
                });

            alienRace = GetRaceOfFaction(__instance.def);

            alienRace?.alienRace.generalSettings.factionRelations?.ForEach(action: frs =>
            {
                if (!frs.factions.Contains(other.def)) return;
                int           offset = frs.goodwill.RandomInRange;

                FactionRelationKind kind = offset > 75 ?
                                               FactionRelationKind.Ally :
                                               offset <= -10 ?
                                                   FactionRelationKind.Hostile :
                                                   FactionRelationKind.Neutral;

                FactionRelation relation = other.RelationWith(__instance);
                relation.goodwill = offset;
                relation.kind     = kind;

                relation          = __instance.RelationWith(other);
                relation.goodwill = offset;
                relation.kind     = kind;
            });

        }

        public static bool TryCreateThoughtPrefix(ref ThoughtDef def, SituationalThoughtHandler __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is ThingDef_AlienRace race)
                def = race.alienRace.thoughtSettings.ReplaceIfApplicable(def);

            return !Traverse.Create(__instance).Field(name: "tmpCachedThoughts").GetValue<HashSet<ThoughtDef>>().Contains(def);
        }

        public static void CanBingeNowPostfix(Pawn pawn, ChemicalDef chemical, ref bool __result)
        {
            if (!__result) return;
            if (!(pawn.def is ThingDef_AlienRace alienProps)) return;
            bool result = true;
            alienProps.alienRace.generalSettings.chemicalSettings?.ForEach(action: cs =>
                                                                                   {
                                                                                       if (cs.chemical == chemical && !cs.ingestible)
                                                                                           result = false;
                                                                                   }
                                                                          );
            __result = result;
        }

        public static void PostIngestedPostfix(Pawn ingester, CompDrug __instance)
        {
            if (ingester.def is ThingDef_AlienRace alienProps)
                alienProps.alienRace.generalSettings.chemicalSettings?.ForEach(action: cs =>
                {
                    if (cs.chemical == __instance.Props?.chemical)
                        cs.reactions?.ForEach(action: iod => iod.DoIngestionOutcome(ingester, __instance.parent));
                });
        }

        public static void DrugValidatorPostfix(ref bool __result, Pawn pawn, Thing drug) =>
            CanBingeNowPostfix(pawn, drug?.TryGetComp<CompDrug>()?.Props?.chemical, ref __result);

        // ReSharper disable once RedundantAssignment
        public static void CompatibilityWithPostfix(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result)
        {
            Traverse traverse = Traverse.Create(__instance);
            Pawn     pawn     = traverse.Field(name: "pawn").GetValue<Pawn>();

            if (pawn.RaceProps.Humanlike != otherPawn.RaceProps.Humanlike || pawn == otherPawn)
            {
                __result = 0f;
                return;
            }

            float x   = Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
            float num = GenMath.LerpDouble(inFrom: 0f, inTo: 20f, outFrom: 0.45f, outTo: -0.45f, x);
            num = Mathf.Clamp(num, min: -0.45f, max: 0.45f);
            float num2 = __instance.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);
            __result = num + num2;
        }

        public static IEnumerable<CodeInstruction> SecondaryLovinChanceFactorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo  defField          = AccessTools.Field(typeof(Thing), nameof(Pawn.def));
            MethodInfo racePropsProperty = AccessTools.Property(typeof(Pawn), nameof(Pawn.RaceProps)).GetGetMethod();
            MethodInfo humanlikeProperty = AccessTools.Property(typeof(RaceProperties), nameof(RaceProperties.Humanlike)).GetGetMethod();
            
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(defField))
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, racePropsProperty);

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
            __result = ((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd => wgd.giverClass == __instance.GetType()) ?? false) ||
                       !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(predicate: d => pawn.def != d && (d.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd =>
                                                 wgd.giverClass == __instance.GetType()) ?? false));
        }

        public static void GenericJobOnThingPostfix(WorkGiver __instance, Pawn pawn, ref Job __result)
        {
            if (__result == null) return;
            // ReSharper disable once ImplicitlyCapturedClosure
            if (!(((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd => wgd.giverClass == __instance.GetType()) ?? false) ||
                  !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(predicate: d => pawn.def != d && 
                                                                            (d.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd => wgd.giverClass == __instance.GetType()) ?? false))))
                __result = null;
        }

        public static void SetFactionDirectPostfix(Thing __instance, Faction newFaction)
        {
            if (__instance.def is ThingDef_AlienRace alienProps && newFaction == Faction.OfPlayerSilentFail)
                alienProps.alienRace.raceRestriction.conceptList?.ForEach(action: cdd =>
                {
                    if (cdd == null) return;
                    Find.Tutor.learningReadout.TryActivateConcept(cdd);
                    PlayerKnowledgeDatabase.SetKnowledge(cdd, value: 0);
                });
        }

        public static void SetFactionPostfix(Pawn __instance, Faction newFaction)
        {
            if (__instance.def is ThingDef_AlienRace alienProps && newFaction == Faction.OfPlayerSilentFail && Current.ProgramState == ProgramState.Playing)
                alienProps.alienRace.raceRestriction.conceptList?.ForEach(action: cdd =>
                {
                    if (cdd == null) return;
                    Find.Tutor.learningReadout.TryActivateConcept(cdd);
                    PlayerKnowledgeDatabase.SetKnowledge(cdd, value: 0);
                });
        }

        public static void ApparelScoreGainPostFix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (!(__result >= 0f)) return;
            if (!RaceRestrictionSettings.CanWear(ap.def, pawn.def))
                __result = -50f;
        }

        public static void PrepForMapGenPrefix(GameInitData __instance) => Find.Scenario.AllParts.OfType<ScenPart_StartingHumanlikes>().Select(selector: sp => sp.GetPawns()).ToList().ForEach(
            action: sp =>
            {
                IEnumerable<Pawn> spa = sp as Pawn[] ?? sp.ToArray();
                __instance.startingAndOptionalPawns.InsertRange(__instance.startingPawnCount, spa);
                __instance.startingPawnCount += spa.Count();
            });

        public static bool TryGainMemoryPrefix(ref Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            Pawn   pawn        = __instance.pawn;

            if (!(pawn.def is ThingDef_AlienRace race)) return true;

            ThoughtDef newThoughtDef = race.alienRace.thoughtSettings.ReplaceIfApplicable(newThought.def);

            if (newThoughtDef == newThought.def) return true;

            Thought_Memory replacedThought = (Thought_Memory)ThoughtMaker.MakeThought(newThoughtDef, newThought.CurStageIndex);
            //foreach (FieldInfo field in newThought.GetType().GetFields(AccessTools.all))
            //field.SetValue(replacedThought, field.GetValue(newThought));
            newThought = replacedThought;
            return true;
        }

        public static void ExtraRequirementsGrowerSowPostfix(Pawn pawn, IPlantToGrowSettable settable, ref bool __result)
        {
            if (!__result) return;
            ThingDef plant = WorkGiver_Grower.CalculateWantedPlantDef((settable as Zone_Growing)?.Cells[index: 0] ?? ((Thing) settable).Position, pawn.Map);

            __result = RaceRestrictionSettings.CanPlant(plant, pawn.def);
        }

        public static void HasJobOnCellHarvestPostfix(Pawn pawn, IntVec3 c, ref bool __result)
        {
            if (!__result) return;
            ThingDef plant = c.GetPlant(pawn.Map).def;

            __result = RaceRestrictionSettings.CanPlant(plant, pawn.def);
        }

        public static void PawnAllowedToStartAnewPostfix(Pawn p, Bill __instance, ref bool __result)
        {
            RecipeDef recipe = __instance.recipe;

            if (__result)
                __result = RaceRestrictionSettings.CanDoRecipe(recipe, p.def);
        }

        private static HashSet<ThingDef> colonistRaces = new HashSet<ThingDef>();
        private static int               colonistRacesTick;
        private const  int               COLONIST_RACES_TICK_TIMER = GenDate.TicksPerHour * 2;

        public static void UpdateColonistRaces()
        {
            if (Find.TickManager.TicksAbs > colonistRacesTick + COLONIST_RACES_TICK_TIMER || Find.TickManager.TicksAbs < colonistRacesTick)
                if ((colonistRaces = new HashSet<ThingDef>(PawnsFinder.AllMaps_FreeColonistsSpawned.Select(selector: p => p.def))).Count > 0)
                {
                    colonistRacesTick = Find.TickManager.TicksAbs;
                    //Log.Message(string.Join(" | ", colonistRaces.Select(td => td.defName)));
                }
        }

        public static void DesignatorAllowedPostfix(Designator d, ref bool __result)
        {
            if (!__result || !(d is Designator_Build build)) 
                return;
            UpdateColonistRaces();
            __result = colonistRaces.Any(predicate: ar => RaceRestrictionSettings.CanBuild(build.PlacingDef, ar));
        }

        public static void CanConstructPostfix(Thing t, Pawn p, ref bool __result)
        {
            if (!__result) return;
            // t.def.defName.Replace(ThingDefGenerator_Buildings.BlueprintDefNameSuffix, string.Empty).Replace(ThingDefGenerator_Buildings.BuildingFrameDefNameSuffix, string.Empty).Replace(ThingDefGenerator_Buildings.InstallBlueprintDefNameSuffix, string.Empty);
            __result = RaceRestrictionSettings.CanBuild(t.def.entityDefToBuild ?? t.def, p.def);
        }

        public static IEnumerable<CodeInstruction> ResearchScreenTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo defListInfo = AccessTools.PropertyGetter(
                typeof(DefDatabase<ResearchProjectDef>), nameof(DefDatabase<ResearchProjectDef>.AllDefsListForReading));

            foreach (CodeInstruction instruction in instructions)
                if (instruction.opcode == OpCodes.Call && instruction.OperandIs(defListInfo))
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(ResearchFixed)));
                }
                else
                {
                    yield return instruction;
                }
        }

        private static List<ResearchProjectDef> ResearchFixed(List<ResearchProjectDef> researchList)
        {
            UpdateColonistRaces();
            return researchList.Where(predicate: rpd => RaceRestrictionSettings.CanResearch(colonistRaces, rpd)).ToList();
        }


        public static void ShouldSkipResearchPostfix(Pawn pawn, ref bool __result)
        {
            if (__result) return;
            ResearchProjectDef project = Find.ResearchManager.currentProj;

            ResearchProjectRestrictions rprest =
                (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.researchList?.FirstOrDefault(predicate: rpr => rpr.projects.Contains(project));
            if (rprest != null)
            {
                IEnumerable<ThingDef> apparel = pawn.apparel.WornApparel.Select(selector: twc => twc.def);
                if (!rprest.apparelList?.TrueForAll(match: ap => apparel.Contains(ap)) ?? false)
                    __result = true;
            }
            else
            {
                __result = DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(predicate: d =>
                    pawn.def != d && (d.alienRace.raceRestriction.researchList?.Any(predicate: rpr => rpr.projects.Contains(project)) ?? false));
            }
        }

        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<ThoughtDef> __result)
        {
            if (ingester.story.traits.HasTrait(AlienDefOf.Xenophobia) && ingester.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1)
                if (__result.Contains(ThoughtDefOf.AteHumanlikeMeatDirect) && foodDef.ingestible.sourceDef != ingester.def)
                    __result.Remove(ThoughtDefOf.AteHumanlikeMeatDirect);
                else if (__result.Contains(ThoughtDefOf.AteHumanlikeMeatAsIngredient) &&
                         (foodSource?.TryGetComp<CompIngredients>()?.ingredients.Any(predicate: td => FoodUtility.IsHumanlikeMeat(td) && td.ingestible.sourceDef != ingester.def) ?? false))
                    __result.Remove(ThoughtDefOf.AteHumanlikeMeatAsIngredient);

            if (!(ingester.def is ThingDef_AlienRace alienProps)) return;

            bool cannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);

            for (int i = 0; i < __result.Count; i++)
            {
                ThoughtDef thoughtDef = __result[i];
                ThoughtSettings settings = alienProps.alienRace.thoughtSettings;

                thoughtDef = settings.ReplaceIfApplicable(thoughtDef);

                if(thoughtDef == ThoughtDefOf.AteHumanlikeMeatDirect || thoughtDef == ThoughtDefOf.AteHumanlikeMeatDirectCannibal)
                    thoughtDef = settings.GetAteThought(foodDef.ingestible.sourceDef, cannibal, ingredient: false);

                if (thoughtDef == ThoughtDefOf.AteHumanlikeMeatAsIngredient || thoughtDef == ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal)
                {
                    ThingDef race = foodSource?.TryGetComp<CompIngredients>()?.ingredients.FirstOrDefault(predicate: td => td.ingestible?.sourceDef?.race?.Humanlike ?? false);
                    if(race != null)
                        thoughtDef = settings.GetAteThought(race, cannibal, ingredient: true);
                }

                __result[i] = thoughtDef;
            }
        }

        public static void GenerationChanceSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierSpouse;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierSpouse;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSpouse ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceSiblingPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierSibling;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierSibling;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSibling ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierSibling ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceParentPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierParent;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierParent;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierParent ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierParent ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierLover;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierLover;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierLover ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceFiancePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierFiance;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierFiance;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierFiance ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierFiance ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceExSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierExSpouse;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierExSpouse;


            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExSpouse ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExSpouse ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceExLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) __result *= race.alienRace.relationSettings.relationChanceModifierExLover;

            if (other.def is ThingDef_AlienRace alienRace) __result *= alienRace.alienRace.relationSettings.relationChanceModifierExLover;

            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExLover ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierExLover ?? 1;

            if (generated == other) __result = 0;
        }

        public static void GenerationChanceChildPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace alienProps) __result *= alienProps.alienRace.relationSettings.relationChanceModifierChild;

            if (other.def is ThingDef_AlienRace alienProps2) __result *= alienProps2.alienRace.relationSettings.relationChanceModifierChild;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(generated.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Childhood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierChild ?? 1;
            __result *= DefDatabase<BackstoryDef>.GetNamedSilentFail(other.story.GetBackstory(BackstorySlot.Adulthood)?.identifier ?? "nothingHere")?.relationSettings
                           .relationChanceModifierChild ?? 1;

            if (generated == other) __result = 0;
        }

        public static void BirthdayBiologicalPrefix(Pawn_AgeTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field(name: "pawn").GetValue<Pawn>();
            if (!(pawn.def is ThingDef_AlienRace alienProps)) return;

            if (!pawn.def.race.lifeStageAges.Skip(count: 1).Any() || pawn.ageTracker.CurLifeStageIndex == 0) return;
            LifeStageAge lsac = pawn.ageTracker.CurLifeStageRace;
            LifeStageAge lsap = pawn.def.race.lifeStageAges[pawn.ageTracker.CurLifeStageIndex - 1];

            if (lsac is LifeStageAgeAlien lsaac && lsaac.body != null && ((lsap as LifeStageAgeAlien)?.body ?? pawn.RaceProps.body) != lsaac.body ||
                lsap is LifeStageAgeAlien lsaap && lsaap.body != null && ((lsac as LifeStageAgeAlien)?.body ?? pawn.RaceProps.body) != lsaap.body)
            {
                pawn.health.hediffSet = new HediffSet(pawn);
                string path = alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStageRace.def).head;
                Traverse.Create(pawn.story).Field(name: "headGraphicPath").SetValue(alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(path, pawn));
            }
        }

        // ReSharper disable once RedundantAssignment
        public static bool ButcherProductsPrefix(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {
            Pawn               corpse = __instance.InnerPawn;
            IEnumerable<Thing> things = corpse.ButcherProducts(butcher, efficiency);
            if (corpse.RaceProps.BloodDef != null) FilthMaker.TryMakeFilth(butcher.Position, butcher.Map, corpse.RaceProps.BloodDef, corpse.LabelIndefinite());
            if (!corpse.RaceProps.Humanlike)
            {
                __result = things;
                return false;
            }

            ThoughtDef thought = !(butcher.def is ThingDef_AlienRace alienPropsButcher) ?
                                     ThoughtDefOf.ButcheredHumanlikeCorpse :
                                     alienPropsButcher.alienRace.thoughtSettings.butcherThoughtSpecific
                                                    ?.FirstOrDefault(predicate: bt => bt.raceList?.Contains(corpse.def) ?? false)?.thought ??
                                                  alienPropsButcher.alienRace.thoughtSettings.butcherThoughtGeneral.thought;

            butcher.needs?.mood?.thoughts?.memories?.TryGainMemory(thought ?? ThoughtDefOf.ButcheredHumanlikeCorpse);

            butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction).ForEach(action: p =>
            {
                if (p == butcher || p.needs?.mood?.thoughts == null) return;
                thought = !(p.def is ThingDef_AlienRace alienPropsPawn) ?
                              ThoughtDefOf.KnowButcheredHumanlikeCorpse :
                              alienPropsPawn.alienRace.thoughtSettings.butcherThoughtSpecific
                                             ?.FirstOrDefault(predicate: bt => bt.raceList?.Contains(corpse.def) ?? false)?.knowThought ??
                                           alienPropsPawn.alienRace.thoughtSettings.butcherThoughtGeneral.knowThought;

                p.needs.mood.thoughts.memories.TryGainMemory(thought ?? ThoughtDefOf.KnowButcheredHumanlikeCorpse);
            });
            TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, new object[] {butcher});
            __result = things;
            return false;
        }

        public static void CanEquipPostfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (__result)
            {
                if (thing.def.IsApparel && !RaceRestrictionSettings.CanWear(thing.def, pawn.def))
                {
                    __result = false;
                    cantReason = $"{pawn.def.LabelCap} can't wear this";
                    return;
                }

                if (thing.def.IsWeapon && !RaceRestrictionSettings.CanEquip(thing.def, pawn.def))
                {
                    __result   = false;
                    cantReason = $"{pawn.def.LabelCap} can't equip this";
                    return;
                }
            }
        }

        public static void CanGetThoughtPostfix(ref bool __result, ThoughtDef def, Pawn pawn)
        {
            if (!__result) return;

            __result = !(ThoughtSettings.thoughtRestrictionDict.TryGetValue(def, out List<ThingDef_AlienRace> races));

            if(!(pawn.def is ThingDef_AlienRace alienProps)) return;

            __result = races?.Contains(alienProps) ?? true;

            def = alienProps.alienRace.thoughtSettings.ReplaceIfApplicable(def);

            if ((alienProps.alienRace.thoughtSettings.cannotReceiveThoughtsAtAll && !(alienProps.alienRace.thoughtSettings.canStillReceiveThoughts?.Contains(def) ?? false)) ||
                (alienProps.alienRace.thoughtSettings.cannotReceiveThoughts?.Contains(def) ?? false))
                __result = false;
        }

        public static void CanDoNextStartPawnPostfix(ref bool __result)
        {
            if (__result) return;

            bool result = true;
            Find.GameInitData.startingAndOptionalPawns.ForEach(action: current =>
            {
                if (!current.Name.IsValid && current.def.race.GetNameGenerator(current.gender) == null) result = false;
            });
            __result = result;
        }

        public static bool GeneratePawnNamePrefix(ref Name __result, Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
        {
            if (!(pawn.def is ThingDef_AlienRace alienProps) || alienProps.race.GetNameGenerator(pawn.gender) == null || style != NameStyle.Full) return true;

            NameTriple nameTriple = NameTriple.FromString(NameGenerator.GenerateName(alienProps.race.GetNameGenerator(pawn.gender)));

            string first = nameTriple.First, nick = nameTriple.Nick, last = nameTriple.Last;
            
            if (nick == null) nick = nameTriple.First;

            if (last != null && forcedLastName != null) last = forcedLastName;

            __result = new NameTriple(first ?? string.Empty, nick ?? string.Empty, last ?? string.Empty);

            return false;
        }

        public static void GenerateRandomAgePrefix(Pawn pawn, PawnGenerationRequest request)
        {
            if (request.FixedGender.HasValue || !pawn.RaceProps.hasGenders) return;
            float? maleGenderProbability = pawn.kindDef.GetModExtension<Info>()?.maleGenderProbability ?? (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.maleGenderProbability;

            if (!maleGenderProbability.HasValue) return;

            pawn.gender = Rand.Value >= maleGenderProbability ? Gender.Female : Gender.Male;

            AlienPartGenerator.AlienComp alienComp = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
            if ((alienComp == null || !(Math.Abs(maleGenderProbability.Value) < 0.001f)) && !(Math.Abs(maleGenderProbability.Value - 1f) < 0.001f)) return;
            if (alienComp != null)
                alienComp.fixGenderPostSpawn = true;
        }

        public static void GiveAppropriateBioAndNameToPostfix(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                //Log.Message(pawn.LabelCap);

                pawn.story.hairColor = pawn.GetComp<AlienPartGenerator.AlienComp>().GetChannel(channel: "hair").first;

                string headPath = alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).head;

                Traverse.Create(pawn.story).Field(name: "headGraphicPath").SetValue(
                    alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).head.NullOrEmpty() ?
                               "" :
                               alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(
                                   headPath, pawn));
                pawn.story.crownType = CrownType.Average;
            }
        }

        public static bool NewGeneratedStartingPawnPrefix(ref Pawn __result)
        {
            PawnKindDef kindDef = Faction.OfPlayer.def.basicMemberKind;

            if (DefDatabase<RaceSettings>.AllDefsListForReading.Where(predicate: tdar => !tdar.pawnKindSettings.startingColonists.NullOrEmpty())
                                      .SelectMany(selector: tdar => tdar.pawnKindSettings.startingColonists).Where(predicate: sce => sce.factionDefs.Contains(Faction.OfPlayer.def))
                                      .SelectMany(selector: sce => sce.pawnKindEntries).TryRandomElementByWeight(pke => pke.chance, out PawnKindEntry pk))
                kindDef = pk.kindDefs.RandomElement();

            if (kindDef == Faction.OfPlayer.def.basicMemberKind) return true;

            PawnGenerationRequest request = new PawnGenerationRequest(kindDef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, forceGenerateNewPawn: true,
                colonistRelationChanceFactor: 26f);
            Pawn pawn;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error($"There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: {arg}");
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
            if (pawn.def is ThingDef_AlienRace alienProps && alienProps.alienRace.generalSettings.immuneToAge) return false;
            return true;
        }

        
        public static bool FillBackstoryInSlotShuffledPrefix(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
        {
            bioReference = null;
            if (slot == BackstorySlot.Adulthood && DefDatabase<BackstoryDef>.GetNamedSilentFail(pawn.story.childhood.identifier)?.linkedBackstory is string id &&
                BackstoryDatabase.TryGetWithIdentifier(id, out backstory))
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

        public static IEnumerable<CodeInstruction> FillBackstoryInSlotShuffledTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo shuffleableInfo = AccessTools.Method(typeof(BackstoryDatabase), nameof(BackstoryDatabase.ShuffleableBackstoryList));

            foreach (CodeInstruction codeInstruction in instructions)
            {
                yield return codeInstruction;

                if (codeInstruction.opcode == OpCodes.Call && codeInstruction.OperandIs(shuffleableInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(FilterBackstories)));
                }
            }
        }

        public static List<Backstory> FilterBackstories(List<Backstory> backstories, Pawn pawn, BackstorySlot slot) =>
            backstories.Where(predicate: bs =>
                                         {
                                             BackstoryDef def = DefDatabase<BackstoryDef>.GetNamedSilentFail(bs.identifier);
                                             return (def?.Approved(pawn) ?? true) && (slot != BackstorySlot.Adulthood || (def?.linkedBackstory.NullOrEmpty() ?? true));
                                         }).ToList();

        private static PawnBioDef bioReference;

        // ReSharper disable once RedundantAssignment
        public static void TryGetRandomUnusedSolidBioForPostfix(List<string> backstoryCategories, ref bool __result, ref PawnBio result, PawnKindDef kind, Gender gender, string requiredLastName)
        {
            if (SolidBioDatabase.allBios.Where(predicate: pb =>
                (((kind.race as ThingDef_AlienRace)?.alienRace.generalSettings.allowHumanBios ?? true) && (kind.GetModExtension<Info>()?.allowHumanBios ?? true) ||
                 (DefDatabase<PawnBioDef>.AllDefs.FirstOrDefault(predicate: pbd => pb.name.ConfusinglySimilarTo(pbd.name))?.validRaces.Contains(kind.race) ?? false)) &&
                (pb.gender == GenderPossibility.Either || pb.gender == GenderPossibility.Male && gender == Gender.Male)                                                            &&
                (requiredLastName.NullOrEmpty()        || pb.name.Last == requiredLastName)                                                                                        &&
                (!kind.factionLeader                   || pb.pirateKing)                                                                                                           &&
                pb.adulthood.spawnCategories.Any(backstoryCategories.Contains)                                                                                                     &&
                !pb.name.UsedThisGame).TryRandomElement(out PawnBio bio))
            {
                result     = bio;
                bioReference = DefDatabase<PawnBioDef>.AllDefs.FirstOrDefault(predicate: pbd => bio.name.ConfusinglySimilarTo(pbd.name));
                __result = true;
            }
            else
            {
                result = null;
                __result = false;
            }
        }

        public static bool ResolveAllGraphicsPrefix(PawnGraphicSet __instance)
        {
            Pawn alien = __instance.pawn;
            if (alien.def is ThingDef_AlienRace alienProps)
            {
                AlienPartGenerator.AlienComp alienComp = __instance.pawn.GetComp<AlienPartGenerator.AlienComp>();

                AlienPartGenerator apg = alienProps.alienRace.generalSettings.alienPartGenerator;

                if (alienComp.fixGenderPostSpawn)
                {
                    float? maleGenderProbability = alien.kindDef.GetModExtension<Info>()?.maleGenderProbability ?? alienProps.alienRace.generalSettings.maleGenderProbability;
                    __instance.pawn.gender = Rand.Value >= maleGenderProbability ? Gender.Female : Gender.Male;
                    __instance.pawn.Name   = PawnBioAndNameGenerator.GeneratePawnName(__instance.pawn);


                    Traverse.Create(__instance.pawn.story).Field(name: "headGraphicPath").SetValue(
                        alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).head.NullOrEmpty() ?
                                   "" :
                                   apg.RandomAlienHead(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).head, __instance.pawn));

                    alienComp.fixGenderPostSpawn = false;
                }

                GraphicPaths graphicPaths = alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage);

                alienComp.customDrawSize             = graphicPaths.customDrawSize;
                alienComp.customHeadDrawSize         = graphicPaths.customHeadDrawSize;
                alienComp.customPortraitDrawSize     = graphicPaths.customPortraitDrawSize;
                alienComp.customPortraitHeadDrawSize = graphicPaths.customPortraitHeadDrawSize;

                alienComp.AssignProperMeshs();

                Traverse.Create(alien.story).Field(name: "headGraphicPath").SetValue(alienComp.crownType.NullOrEmpty() ?
                                                                                                      apg.RandomAlienHead(
                                                                                                                                                        graphicPaths.head, alien) :
                                                                                                      AlienPartGenerator.GetAlienHead(graphicPaths.head,
                                                                                                          apg.useGenderedHeads ?
                                                                                                                      alien.gender.ToString() :
                                                                                                                      "", alienComp.crownType));

                __instance.nakedGraphic = !graphicPaths.body.NullOrEmpty() ?
                apg.GetNakedGraphic(alien.story.bodyType,
                ContentFinder<Texture2D>.Get(
                AlienPartGenerator.GetNakedPath(alien.story.bodyType, graphicPaths.body,
                apg.useGenderedBodies ? alien.gender.ToString() : "") +
                "_northm", reportFailure: false) == null ?
                graphicPaths.skinShader?.Shader ?? TriColorShaderDatabase.Tricolor :
                TriColorShaderDatabase.Tricolor, __instance.pawn.story.SkinColor,
                apg.SkinColor(alien, 2), apg.SkinColor(alien, 3),graphicPaths.body,
                alien.gender.ToString()) : null;

                __instance.rottingGraphic = !graphicPaths.body.NullOrEmpty() ?
                apg.GetNakedGraphic(alien.story.bodyType, graphicPaths.skinShader?.Shader ?? TriColorShaderDatabase.Tricolor,
                PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor, graphicPaths.body,
                alien.gender.ToString()) : null;

                __instance.dessicatedGraphic = !graphicPaths.skeleton.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>((graphicPaths.skeleton == GraphicPaths.VANILLA_SKELETON_PATH ? alien.story.bodyType.bodyDessicatedGraphicPath : graphicPaths.skeleton), ShaderDatabase.Cutout) : null;
                
                __instance.headGraphic = alien.health.hediffSet.HasHead && !alien.story.HeadGraphicPath.NullOrEmpty() ?
                GraphicDatabase.Get<Graphic_Multi>(alien.story.HeadGraphicPath,
                ContentFinder<Texture2D>.Get(alien.story.HeadGraphicPath + "_northm", reportFailure: false) == null ?
                graphicPaths.skinShader?.Shader ?? ShaderDatabase.Cutout :
                ShaderDatabase.CutoutComplex, Vector2.one, alien.story.SkinColor,
                apg.SkinColor(alien, 2)) : null;

                __instance.desiccatedHeadGraphic = alien.health.hediffSet.HasHead && !alien.story.HeadGraphicPath.NullOrEmpty() ?
                                                       GraphicDatabase.Get<Graphic_Multi>(alien.story.HeadGraphicPath, ShaderDatabase.Cutout, Vector2.one,
                                                           PawnGraphicSet.RottingColor) :
                                                       null;
                __instance.skullGraphic = alien.health.hediffSet.HasHead && !graphicPaths.skull.NullOrEmpty() ?
                                              GraphicDatabase.Get<Graphic_Multi>(graphicPaths.skull, ShaderDatabase.Cutout, Vector2.one, Color.white) :
                                              null;
                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.hairDef.texPath,
                    ContentFinder<Texture2D>.Get(__instance.pawn.story.hairDef.texPath + "_northm", reportFailure: false) == null ?
                                (alienProps.alienRace.hairSettings.shader?.Shader ?? ShaderDatabase.Cutout) :
                                ShaderDatabase.CutoutComplex, Vector2.one, alien.story.hairColor, alienComp.GetChannel(channel: "hair").second);
               
                __instance.headStumpGraphic = !graphicPaths.stump.NullOrEmpty() ?
                GraphicDatabase.Get<Graphic_Multi>(graphicPaths.stump,
                alien.story.SkinColor == apg.SkinColor(alien, 2) ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, Vector2.one,
                alien.story.SkinColor, apg.SkinColor(alien, 2)) : null;
                
                __instance.desiccatedHeadStumpGraphic = !graphicPaths.stump.NullOrEmpty() ?
                                                            GraphicDatabase.Get<Graphic_Multi>(graphicPaths.stump,
                                                                ShaderDatabase.Cutout, Vector2.one,
                                                                PawnGraphicSet.RottingColor) :
                                                            null;

                alienComp.ColorChannels[key: "hair"].first = alien.story.hairColor;

                alienComp.addonGraphics = new List<Graphic>();
                if (alienComp.addonVariants == null)
                    alienComp.addonVariants = new List<int>();
                int sharedIndex = 0;
                for (int i = 0; i < apg.bodyAddons.Count; i++)
                {
                    Graphic g = apg.bodyAddons[i].GetPath(alien, ref sharedIndex,
                        alienComp.addonVariants.Count > i ? (int?) alienComp.addonVariants[i] : null);
                    alienComp.addonGraphics.Add(g);
                    if (alienComp.addonVariants.Count <= i)
                        alienComp.addonVariants.Add(sharedIndex);
                }

                __instance.ResolveApparelGraphics();

                PortraitsCache.SetDirty(alien);

                return false;
            }

            return true;
        }

        public static void GenerateTraitsPostfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (!request.Newborn && request.CanGeneratePawnRelations)
                AccessTools.Method(typeof(PawnGenerator), name: "GeneratePawnRelations").Invoke(obj: null, new object[] {pawn, request});

            if (pawn.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.generalSettings.forcedRaceTraitEntries.NullOrEmpty())
                alienProps.alienRace.generalSettings.forcedRaceTraitEntries.ForEach(action: ate =>
                {
                    if ((pawn.gender != Gender.Male ||
                         !(Math.Abs(ate.commonalityMale - -1f) < 0.001f) && !(Rand.Range(min: 0, max: 100) < ate.commonalityMale))                           &&
                        (pawn.gender != Gender.Female || Math.Abs(ate.commonalityFemale - -1f) > 0.001f && !(Rand.Range(min: 0, max: 100) < ate.commonalityFemale)) &&
                        pawn.gender != Gender.None) return;
                    if (!pawn.story.traits.allTraits.Any(predicate: tr => tr.def == ate.defName))
                        pawn.story.traits.GainTrait(new Trait(ate.defName, ate.degree, forced: true));
                });
        }

        public static void SkinColorPostfix(Pawn_StoryTracker __instance, ref Color __result)
        {
            Pawn pawn                                               = Traverse.Create(__instance).Field(name: "pawn").GetValue<Pawn>();
            if (pawn.def is ThingDef_AlienRace alienProps) __result = alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(pawn);
        }

        public static void GenerateBodyTypePostfix(ref Pawn pawn)
        {

            if (BackstoryDef.checkBodyType.Contains(pawn.story.GetBackstory(BackstorySlot.Adulthood)))
                pawn.story.bodyType = DefDatabase<BodyTypeDef>.GetRandom();

            if (pawn.def is ThingDef_AlienRace alienProps && 
                !alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.NullOrEmpty() &&
                    !alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.Contains(pawn.story.bodyType))
                    pawn.story.bodyType = alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.RandomElement();
        }

        // ReSharper disable InconsistentNaming
        private static readonly FactionDef noHairFaction = new FactionDef {hairTags = new List<string> {"alienNoHair"}};
        private static readonly FactionDef hairFaction   = new FactionDef();
        // ReSharper restore InconsistentNaming

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



            IEnumerable<RaceSettings> settings = DefDatabase<RaceSettings>.AllDefsListForReading;
            PawnKindEntry             pk;
            if (request.KindDef == PawnKindDefOf.SpaceRefugee || request.KindDef == PawnKindDefOf.Refugee)
            {
                if (settings.Where(predicate: r => !r.pawnKindSettings.alienrefugeekinds.NullOrEmpty()).SelectMany(selector: r => r.pawnKindSettings.alienrefugeekinds)
                         .TryRandomElementByWeight(weightSelector: pke => pke.chance, out pk)) 
                    kindDef = pk.kindDefs.RandomElement();
            }
            else if (request.KindDef == PawnKindDefOf.Slave)
            {
                if (settings.Where(predicate: r => !r.pawnKindSettings.alienslavekinds.NullOrEmpty()).SelectMany(selector: r => r.pawnKindSettings.alienslavekinds)
                         .TryRandomElementByWeight(weightSelector: pke => pke.chance, out pk))
                    kindDef = pk.kindDefs.RandomElement();
            }
            else if (request.KindDef == PawnKindDefOf.Villager)
            {
                if (DefDatabase<RaceSettings>.AllDefsListForReading.Where(predicate: tdar => !tdar.pawnKindSettings.alienwandererkinds.NullOrEmpty())
                                          .SelectMany(selector: rs => rs.pawnKindSettings.alienwandererkinds).Where(predicate: fpke => fpke.factionDefs.Contains(Faction.OfPlayer.def))
                                          .SelectMany(selector: fpke => fpke.pawnKindEntries).TryRandomElementByWeight(pke => pke.chance, out pk))
                    kindDef = pk.kindDefs.RandomElement();
            }

            request = new PawnGenerationRequest(kindDef, request.Faction, request.Context, request.Tile, request.ForceGenerateNewPawn,
                                                request.Newborn,
                                                request.AllowDead, request.AllowDead, request.CanGeneratePawnRelations, request.MustBeCapableOfViolence,
                                                request.ColonistRelationChanceFactor,
                                                request.ForceAddFreeWarmLayerIfNeeded, request.AllowGay, request.AllowFood, inhabitant: request.Inhabitant,
                                                certainlyBeenInCryptosleep: request.CertainlyBeenInCryptosleep,
                                                forceRedressWorldPawnIfFormerColonist: request.ForceRedressWorldPawnIfFormerColonist,
                                                worldPawnFactionDoesntMatter: request.WorldPawnFactionDoesntMatter,
                                                validatorPreGear: request.ValidatorPreGear,
                                                validatorPostGear: request.ValidatorPostGear, minChanceToRedressWorldPawn: request.MinChanceToRedressWorldPawn,
                                                fixedBiologicalAge: request.FixedBiologicalAge,
                                                fixedChronologicalAge: request.FixedChronologicalAge, fixedGender: request.FixedGender, fixedMelanin: request.FixedMelanin,
                                                fixedLastName: request.FixedLastName);
        }

        public static IEnumerable<CodeInstruction> RenderPawnInternalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo  humanlikeBodyInfo = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeBodySet));
            FieldInfo  humanlikeHeadInfo = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeHeadSet));
            MethodInfo hairInfo          = AccessTools.Property(typeof(PawnGraphicSet), nameof(PawnGraphicSet.HairMeshSet)).GetGetMethod();

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (instruction.OperandIs(humanlikeBodyInfo))
                {
                    instructionList.RemoveRange(i, count: 2);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 7); // portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld,   AccessTools.Field(typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 4); // bodyfacing
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(GetPawnMesh)));
                }
                else if (instruction.OperandIs(humanlikeHeadInfo))
                {
                    instructionList.RemoveRange(i, count: 2);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 7); // portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld,   AccessTools.Field(typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 5); //headfacing
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(GetPawnMesh)));
                }
                else if (i + 4 < instructionList.Count && instructionList[i + 2].OperandIs(hairInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 7) {labels = instruction.labels}; // portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld,   AccessTools.Field(typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 5); //headfacing
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.graphics)));
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(GetPawnHairMesh)));
                    instructionList.RemoveRange(i, count: 4);
                }
                else if (i > 1 && instructionList[i -1].OperandIs(AccessTools.Method(typeof(Graphics), nameof(Graphics.DrawMesh), new []{typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(Int32)})))
                {
                    yield return instruction; // portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);             // quat
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 4); // bodyfacing
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 9); //invisible
                    yield return new CodeInstruction(OpCodes.Call,    AccessTools.Method(patchType, nameof(DrawAddons)));

                    instruction = new CodeInstruction(OpCodes.Ldarg_S, operand: 7);
                }

                yield return instruction;
            }
        }

        public static Mesh GetPawnMesh(bool portrait, Pawn pawn, Rot4 facing, bool wantsBody) =>
            pawn.GetComp<AlienPartGenerator.AlienComp>() is AlienPartGenerator.AlienComp alienComp ?
                portrait ?
                    wantsBody ?
                        alienComp.alienPortraitGraphics.bodySet.MeshAt(facing) :
                        alienComp.alienPortraitHeadGraphics.headSet.MeshAt(facing) :
                    wantsBody ?
                        alienComp.alienGraphics.bodySet.MeshAt(facing) :
                        alienComp.alienHeadGraphics.headSet.MeshAt(facing) :
                wantsBody ?
                    MeshPool.humanlikeBodySet.MeshAt(facing) :
                    MeshPool.humanlikeHeadSet.MeshAt(facing);

        public static Mesh GetPawnHairMesh(bool portrait, Pawn pawn, Rot4 headFacing, PawnGraphicSet graphics) =>
            pawn.GetComp<AlienPartGenerator.AlienComp>() is AlienPartGenerator.AlienComp alienComp ?
                     (portrait ?
                          alienComp.alienPortraitHeadGraphics.hairSetAverage :
                          alienComp.alienHeadGraphics.hairSetAverage).MeshAt(headFacing) :
                graphics.HairMeshSet.MeshAt(headFacing);

        public static void DrawAddons(bool portrait, Vector3 vector, Pawn pawn, Quaternion quat, Rot4 rotation, bool invisible)
        {
            if (!(pawn.def is ThingDef_AlienRace alienProps) || invisible) return;

            List<AlienPartGenerator.BodyAddon> addons    = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons;
            AlienPartGenerator.AlienComp       alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();

            for (int i = 0; i < addons.Count; i++)
            {
                AlienPartGenerator.BodyAddon ba = addons[i];
                if (!ba.CanDrawAddon(pawn)) continue;

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
                    if(ba.layerInvert)
                        moffsetY = -moffsetY;
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
                
                Vector3 offsetVector = new Vector3(moffsetX, moffsetY, moffsetZ);

                Graphic addonGraphic = alienComp.addonGraphics[i];
                addonGraphic.drawSize = (portrait && ba.drawSizePortrait != Vector2.zero ? ba.drawSizePortrait : ba.drawSize) * 1.5f;
                //                                                                                        Angle calculation to not pick the shortest, taken from Quaternion.Angle and modified
                GenDraw.DrawMeshNowOrLater(addonGraphic.MeshAt(rotation), vector + offsetVector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quat)) * 2f * 57.29578f),
                                           Quaternion.AngleAxis(num, Vector3.up) * quat, addonGraphic.MatAt(rotation), portrait);
            }
        }
    }
}