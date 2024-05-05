namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using HarmonyLib;
    using LudeonTK;
    using RimWorld;
    using RimWorld.QuestGen;
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
        // ReSharper disable once InconsistentNaming
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            AlienHarmony harmony = new(id: "rimworld.erdelf.alien_race.main");

            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Child), nameof(PawnRelationWorker_Child.GenerationChance)), 
                          postfix: new HarmonyMethod(patchType, nameof(GenerationChanceChildPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExLover), nameof(PawnRelationWorker_ExLover.GenerationChance)), 
                          postfix: new HarmonyMethod(patchType, nameof(GenerationChanceExLoverPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExSpouse), nameof(PawnRelationWorker_ExSpouse.GenerationChance)), 
                postfix: new HarmonyMethod(patchType, nameof(GenerationChanceExSpousePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Fiance), nameof(PawnRelationWorker_Spouse.GenerationChance)), 
                postfix: new HarmonyMethod(patchType, nameof(GenerationChanceFiancePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Lover.GenerationChance)), 
                postfix: new HarmonyMethod(patchType, nameof(GenerationChanceLoverPostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Parent), nameof(PawnRelationWorker_Parent.GenerationChance)), 
                          postfix: new HarmonyMethod(patchType, nameof(GenerationChanceParentPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Sibling), nameof(PawnRelationWorker_Sibling.GenerationChance)), 
                postfix: new HarmonyMethod(patchType, nameof(GenerationChanceSiblingPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Spouse), nameof(PawnRelationWorker_Spouse.GenerationChance)), 
                postfix: new HarmonyMethod(patchType, nameof(GenerationChanceSpousePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GeneratePawnRelations"),
                new HarmonyMethod(patchType, nameof(GeneratePawnRelationsPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationDef), nameof(PawnRelationDef.GetGenderSpecificLabel)),
                new HarmonyMethod(patchType, nameof(GetGenderSpecificLabelPrefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "TryGetRandomUnusedSolidBioFor"), 
                postfix: new HarmonyMethod(patchType, nameof(TryGetRandomUnusedSolidBioForPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.FillBackstorySlotShuffled)),
                new HarmonyMethod(patchType, nameof(FillBackstoryInSlotShuffledPrefix)), transpiler: new HarmonyMethod(patchType, nameof(FillBackstorySlotShuffledTranspiler)));
            //-------------------------------------------
            
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_Researcher), nameof(WorkGiver_Researcher.ShouldSkip)), 
                postfix: new HarmonyMethod(patchType, nameof(ShouldSkipResearchPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), name: "ViewSize"),      transpiler: new HarmonyMethod(patchType, nameof(ResearchScreenTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), name: "DrawRightRect"), transpiler: new HarmonyMethod(patchType, nameof(ResearchScreenTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.CanConstruct), [typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool), typeof(JobDef)]), 
                postfix: new HarmonyMethod(patchType, nameof(CanConstructPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GameRules), nameof(GameRules.DesignatorAllowed)), 
                transpiler: new HarmonyMethod(patchType, nameof(DesignatorAllowedTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew)), 
                postfix: new HarmonyMethod(patchType, nameof(PawnAllowedToStartAnewPostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GrowerHarvest), nameof(WorkGiver_GrowerHarvest.HasJobOnCell)), 
                postfix: new HarmonyMethod(patchType, nameof(HasJobOnCellHarvestPostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GrowerSow), name: "ExtraRequirements"), 
                postfix: new HarmonyMethod(patchType, nameof(ExtraRequirementsGrowerSowPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.SetFaction)), postfix: new HarmonyMethod(patchType, nameof(SetFactionPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Pawn.SetFactionDirect)), 
                postfix: new HarmonyMethod(patchType, nameof(SetFactionDirectPostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreGain)), 
                postfix: new HarmonyMethod(patchType, nameof(ApparelScoreGainPostFix)));
            
            harmony.Patch(AccessTools.Method(typeof(ThoughtUtility), nameof(ThoughtUtility.CanGetThought)),
                postfix: new HarmonyMethod(patchType, nameof(CanGetThoughtPostfix)));
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting)), 
                          postfix: new HarmonyMethod(patchType, nameof(ThoughtsFromIngestingPostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), [typeof(Thought_Memory), typeof(Pawn)]),
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
            
            harmony.Patch(AccessTools.Method(typeof(AgeInjuryUtility), nameof(AgeInjuryUtility.GenerateRandomOldAgeInjuries)),
                          new HarmonyMethod(patchType, nameof(GenerateRandomOldAgeInjuriesPrefix)));
            harmony.Patch(
                AccessTools.Method(typeof(AgeInjuryUtility), name: "RandomHediffsToGainOnBirthday", [typeof(ThingDef), typeof(float), typeof(float)]),
                postfix: new HarmonyMethod(patchType, nameof(RandomHediffsToGainOnBirthdayPostfix)));
            
            //            harmony.Patch(original: AccessTools.Property(type: typeof(JobDriver), name: nameof(JobDriver.Posture)).GetGetMethod(nonPublic: false), postfix:
            //                postfix: new HarmonyMethod(type: patchType, name: nameof(PosturePostfix)));
            //            harmony.Patch(original: AccessTools.Property(type: typeof(JobDriver_Skygaze), name: nameof(JobDriver_Skygaze.Posture)).GetGetMethod(nonPublic: false), postfix:
            //                postfix: new HarmonyMethod(type: patchType, name: nameof(PosturePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateRandomAge"), new HarmonyMethod(patchType,             nameof(GenerateRandomAgePrefix)));
            
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateTraits"),    new HarmonyMethod(patchType,             nameof(GenerateTraitsPrefix)), postfix: new HarmonyMethod(patchType, nameof(GenerateTraitsPostfix)));
            //--
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateTraitsFor"), transpiler: new HarmonyMethod(patchType, nameof(GenerateTraitsForTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_SatisfyChemicalNeed), name: "DrugValidator"), 
                          postfix:          new HarmonyMethod(patchType, nameof(DrugValidatorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(CompDrug), nameof(CompDrug.PostIngested)), postfix: new HarmonyMethod(patchType,    nameof(DrugPostIngestedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Thing),    nameof(Thing.Ingested)),        transpiler: new HarmonyMethod(patchType, nameof(IngestedTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(AddictionUtility), nameof(AddictionUtility.CanBingeOnNow)), 
                postfix: new HarmonyMethod(patchType, nameof(CanBingeNowPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateBodyType"), postfix: new HarmonyMethod(patchType, nameof(GenerateBodyTypePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: nameof(PawnGenerator.GetBodyTypeFor)), postfix: new HarmonyMethod(patchType, nameof(GetBodyTypeForPostfix)));
            harmony.Patch(AccessTools.Property(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.SkinColor)).GetGetMethod(), 
                transpiler: new HarmonyMethod(patchType, nameof(SkinColorTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), name: "BirthdayBiological"), new HarmonyMethod(patchType, nameof(BirthdayBiologicalPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) }),
                new HarmonyMethod(patchType, nameof(GeneratePawnPrefix)),
                postfix: new HarmonyMethod(patchType, nameof(GeneratePawnPostfix)));

            harmony.Patch(AccessTools.PropertyGetter(typeof(StartingPawnUtility), "DefaultStartingPawnRequest"),
                          transpiler: new HarmonyMethod(patchType, nameof(DefaultStartingPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateGenes"), prefix: new HarmonyMethod(patchType, nameof(GenerateGenesPrefix)),
                postfix: new HarmonyMethod(patchType, nameof(GenerateGenesPostfix)), transpiler: new HarmonyMethod(patchType, nameof(GenerateGenesTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnHairColors), nameof(PawnHairColors.RandomHairColor)), transpiler: new HarmonyMethod(patchType, nameof(GenerateGenesTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName)),
                new HarmonyMethod(patchType, nameof(GeneratePawnNamePrefix)));
            harmony.Patch(AccessTools.Method(typeof(Page_ConfigureStartingPawns), name: "CanDoNext"), 
                transpiler: new HarmonyMethod(patchType, nameof(CanDoNextStartPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard)),
                transpiler: new HarmonyMethod(patchType, nameof(DrawCharacterCardTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(GameInitData), nameof(GameInitData.PrepForMapGen)),
                new HarmonyMethod(patchType, nameof(PrepForMapGenPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor)), postfix: new HarmonyMethod(patchType, nameof(SecondaryLovinChanceFactorPostfix)),
                          transpiler: new HarmonyMethod(patchType, nameof(SecondaryLovinChanceFactorTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.CompatibilityWith)), 
                postfix: new HarmonyMethod(patchType, nameof(CompatibilityWithPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.TryMakeInitialRelationsWith)), 
                postfix: new HarmonyMethod(patchType, nameof(TryMakeInitialRelationsWithPostfix)));
            harmony.Patch(AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait)), new HarmonyMethod(patchType, nameof(GainTraitPrefix)));
            harmony.Patch(AccessTools.Method(typeof(TraderCaravanUtility), nameof(TraderCaravanUtility.GetTraderCaravanRole)), transpiler:
                new HarmonyMethod(patchType, nameof(GetTraderCaravanRoleTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(RestUtility), nameof(RestUtility.CanUseBedEver)), 
                postfix: new HarmonyMethod(patchType, nameof(CanUseBedEverPostfix)));
            harmony.Patch(AccessTools.Property(typeof(CompAssignableToPawn_Bed), nameof(CompAssignableToPawn.AssigningCandidates)).GetGetMethod(), 
                postfix: new HarmonyMethod(patchType, nameof(AssigningCandidatesPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GrammarUtility), nameof(GrammarUtility.RulesForPawn), new[] { typeof(string), typeof(Pawn), typeof(Dictionary<string, string>), typeof(bool), typeof(bool) }), 
                postfix: new HarmonyMethod(patchType, nameof(RulesForPawnPostfix)));
            harmony.Patch(AccessTools.Method(typeof(RaceProperties), nameof(RaceProperties.CanEverEat), new[] { typeof(ThingDef) }), 
                postfix: new HarmonyMethod(patchType, nameof(CanEverEatPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttackDamage), name: "DamageInfosToApply"), 
                postfix: new HarmonyMethod(patchType, nameof(DamageInfosToApplyPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnWeaponGenerator), nameof(PawnWeaponGenerator.TryGenerateWeaponFor)),
                new HarmonyMethod(patchType, nameof(TryGenerateWeaponForPrefix)), new HarmonyMethod(patchType, nameof(TryGenerateWeaponForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.GenerateStartingApparelFor)),
                new HarmonyMethod(patchType, nameof(GenerateStartingApparelForPrefix)),
                new HarmonyMethod(patchType, nameof(GenerateStartingApparelForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateInitialHediffs"), 
                postfix: new HarmonyMethod(patchType, nameof(GenerateInitialHediffsPostfix)));

            harmony.Patch(
                typeof(HediffSet).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(predicate: mi => mi.ReturnType == typeof(bool) && mi.GetParameters().First().ParameterType == typeof(BodyPartRecord)), 
                postfix: new HarmonyMethod(patchType, nameof(HasHeadPostfix)));
            harmony.Patch(AccessTools.Property(typeof(HediffSet), nameof(HediffSet.HasHead)).GetGetMethod(),
                new HarmonyMethod(patchType, nameof(HasHeadPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), name: "RecalculateLifeStageIndex"), 
                transpiler: new HarmonyMethod(patchType, nameof(RecalculateLifeStageIndexTranspiler)));
            
            harmony.Patch(AccessTools.Method(typeof(Designator), nameof(Designator.CanDesignateThing)), 
                postfix: new HarmonyMethod(patchType, nameof(CanDesignateThingTamePostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_InteractAnimal), name: "CanInteractWithAnimal", new []{typeof(Pawn), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool), typeof(bool), typeof(bool), typeof(bool)}), 
                postfix: new HarmonyMethod(patchType, nameof(CanInteractWithAnimalPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt)), 
                          postfix: new HarmonyMethod(patchType, nameof(BaseHeadOffsetAtPostfix)), transpiler: new HarmonyMethod(patchType, nameof(BaseHeadOffsetAtTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new []{typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)}), postfix: new HarmonyMethod(patchType, nameof(AddHediffPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff)), postfix: new HarmonyMethod(patchType, nameof(RemoveHediffPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Notify_HediffChanged)), postfix: new HarmonyMethod(patchType, nameof(HediffChangedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateGearFor"), 
                postfix:          new HarmonyMethod(patchType, nameof(GenerateGearForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.ChangeKind)), new HarmonyMethod(patchType, nameof(ChangeKindPrefix)));

            harmony.Patch(AccessTools.Method(typeof(EditWindow_TweakValues), nameof(EditWindow_TweakValues.DoWindowContents)), transpiler: new HarmonyMethod(typeof(TweakValues), nameof(TweakValues.TweakValuesTranspiler)));
            //....
            HarmonyMethod misandryMisogonyTranspiler = new(patchType, nameof(MisandryMisogynyTranspiler));
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Woman), name: "CurrentSocialStateInternal"), transpiler: misandryMisogonyTranspiler);
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Man), name: "CurrentSocialStateInternal"), transpiler: misandryMisogonyTranspiler);

            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new []{typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool)}), postfix: new HarmonyMethod(patchType, nameof(CanEquipPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "GiveShuffledBioTo"), transpiler: 
                          new HarmonyMethod(patchType, nameof(MinAgeForAdulthood)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "TryGiveSolidBioTo"), transpiler:
                          new HarmonyMethod(patchType, nameof(MinAgeForAdulthood)));

            harmony.Patch(AccessTools.Method(typeof(PawnDrawUtility), nameof(PawnDrawUtility.FindAnchors)), postfix: new HarmonyMethod(patchType, nameof(FindAnchorsPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnDrawUtility), nameof(PawnDrawUtility.CalcAnchorData)), postfix: new HarmonyMethod(patchType, nameof(CalcAnchorDataPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnWoundDrawer), "WriteCache"), transpiler: new HarmonyMethod(patchType, nameof(WoundWriteCacheTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(PawnCacheRenderer), nameof(PawnCacheRenderer.RenderPawn)), new HarmonyMethod(patchType, nameof(CacheRenderPawnPrefix)));
            
            harmony.Patch(AccessTools.Constructor(typeof(PawnTextureAtlas)), transpiler: new HarmonyMethod(patchType, nameof(PawnTextureAtlasConstructorTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(PawnTextureAtlas), nameof(PawnTextureAtlas.TryGetFrameSet)), 
                          transpiler: new HarmonyMethod(patchType, nameof(PawnTextureAtlasGetFrameSetTranspiler)));
            
            harmony.Patch(typeof(PawnTextureAtlas).GetNestedTypes(AccessTools.all)[0].GetMethods(AccessTools.all).First(mi => mi.GetParameters().Any()), 
                          transpiler: new HarmonyMethod(patchType, nameof(PawnTextureAtlasConstructorFuncTranspiler)));
            
            harmony.Patch(AccessTools.Method(typeof(GlobalTextureAtlasManager), nameof(GlobalTextureAtlasManager.TryGetPawnFrameSet)), new HarmonyMethod(patchType, nameof(GlobalTextureAtlasGetFrameSetPrefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnStyleItemChooser), nameof(PawnStyleItemChooser.WantsToUseStyle)), prefix: new HarmonyMethod(patchType, nameof(WantsToUseStylePrefix)), postfix: new HarmonyMethod(patchType, nameof(WantsToUseStylePostfix)));

            harmony.Patch(AccessTools.Method(typeof(PreceptComp_SelfTookMemoryThought), nameof(PreceptComp_SelfTookMemoryThought.Notify_MemberTookAction)),
                          transpiler: new HarmonyMethod(patchType, nameof(SelfTookMemoryThoughtTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(PreceptComp_KnowsMemoryThought), nameof(PreceptComp_KnowsMemoryThought.Notify_MemberWitnessedAction)),
                          transpiler: new HarmonyMethod(patchType, nameof(KnowsMemoryThoughtTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(PawnStyleItemChooser), nameof(PawnStyleItemChooser.TotalStyleItemLikelihood)),
                          postfix: new HarmonyMethod(patchType, nameof(TotalStyleItemLikelihoodPostfix)));

            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Thing.Ingested)), new HarmonyMethod(patchType, nameof(IngestedPrefix)));

            harmony.Patch(AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker.Interacted)),
                          transpiler: new HarmonyMethod(patchType, nameof(RomanceAttemptInteractTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance)),
                          postfix: new HarmonyMethod(patchType, nameof(RomanceAttemptSuccessChancePostfix)));

            harmony.Patch(AccessTools.Method(typeof(BedUtility), nameof(BedUtility.WillingToShareBed)), postfix: new HarmonyMethod(patchType, nameof(WillingToShareBedPostfix)));

            harmony.Patch(AccessTools.Method(typeof(Tradeable_Pawn), nameof(Tradeable_Pawn.ResolveTrade)), transpiler: new HarmonyMethod(patchType, nameof(TradeablePawnResolveTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(TradeUI), nameof(TradeUI.DrawTradeableRow)), transpiler: new HarmonyMethod(patchType, nameof(DrawTradeableRowTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_MindState), nameof(Pawn_MindState.SetupLastHumanMeatTick)), new HarmonyMethod(patchType, nameof(SetupLastHumanMeatTickPrefix)));

            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "AddThoughtsFromIdeo"), new HarmonyMethod(patchType, nameof(FoodUtilityAddThoughtsFromIdeoPrefix)));

            harmony.Patch(AccessTools.Method(typeof(PreceptComp_UnwillingToDo_Gendered), nameof(PreceptComp.MemberWillingToDo)), transpiler: new HarmonyMethod(patchType, nameof(UnwillingWillingToDoGenderedTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(JobDriver_Lovin), "GenerateRandomMinTicksToNextLovin"), transpiler: new HarmonyMethod(patchType, nameof(GenerateRandomMinTicksToNextLovinTranspiler)));
            
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateSkills"), new HarmonyMethod(patchType, nameof(GenerateSkillsPrefix)), postfix: new HarmonyMethod(patchType, nameof(GenerateSkillsPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "TryGenerateNewPawnInternal"), prefix: new HarmonyMethod(patchType, nameof(TryGenerateNewPawnInternalPrefix)), transpiler: new HarmonyMethod(patchType, nameof(TryGenerateNewPawnInternalTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), "Notify_GenesChanged"), transpiler: new HarmonyMethod(patchType, nameof(NotifyGenesChangedTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(GrowthUtility),    nameof(GrowthUtility.IsGrowthBirthday)),       transpiler: new HarmonyMethod(patchType, nameof(IsGrowthBirthdayTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker),  nameof(Pawn_AgeTracker.TryChildGrowthMoment)), new HarmonyMethod(patchType,             nameof(TryChildGrowthMomentPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Gizmo_GrowthTier), "GrowthTierTooltip"),                          new HarmonyMethod(patchType,             nameof(GrowthTierTooltipPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.TrySimulateGrowthPoints)), transpiler: new HarmonyMethod(patchType, nameof(TrySimulateGrowthPointsTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(ChoiceLetter_GrowthMoment), "CacheLetterText"), transpiler: new HarmonyMethod(patchType, nameof(GrowthMomentCacheLetterTextTranspiler)));


            harmony.Patch(AccessTools.Method(typeof(Pawn_StyleTracker),             nameof(Pawn_StyleTracker.FinalizeHairColor)),  postfix: new HarmonyMethod(patchType,    nameof(FinalizeHairColorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Toils_StyleChange),             nameof(Toils_StyleChange.FinalizeLookChange)), postfix: new HarmonyMethod(patchType,    nameof(FinalizeLookChangePostfix)));
            harmony.Patch(AccessTools.Method(typeof(StatPart_FertilityByGenderAge), "AgeFactor"),                                  transpiler: new HarmonyMethod(patchType, nameof(FertilityAgeFactorTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.AddGene), [typeof(Gene), typeof(bool)]), new HarmonyMethod(patchType, nameof(AddGenePrefix)));

            harmony.Patch(AccessTools.Method(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel)), transpiler: new HarmonyMethod(patchType, nameof(TryGetGraphicApparelTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(PregnancyUtility),   nameof(PregnancyUtility.PregnancyChanceForPartners)), prefix: new HarmonyMethod(patchType,     nameof(PregnancyChanceForPartnersPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PregnancyUtility),   nameof(PregnancyUtility.CanEverProduceChild)), postfix: new HarmonyMethod(patchType, nameof(CanEverProduceChildPostfix)), transpiler: new HarmonyMethod(patchType, nameof(CanEverProduceChildTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Recipe_ExtractOvum), nameof(Recipe_ExtractOvum.AvailableReport)),          postfix: new HarmonyMethod(patchType,    nameof(ExtractOvumAvailableReportPostfix)));
            harmony.Patch(AccessTools.Method(typeof(HumanOvum),          "CanFertilizeReport"),                                postfix: new HarmonyMethod(patchType,    nameof(HumanOvumCanFertilizeReportPostfix)));
            harmony.Patch(AccessTools.Method(typeof(HumanEmbryo),        "ImplantPawnValid"),                                  prefix: new HarmonyMethod(patchType,    nameof(EmbryoImplantPawnPrefix)));
            harmony.Patch(AccessTools.Method(typeof(HumanEmbryo),        "CanImplantReport"),                                  postfix: new HarmonyMethod(patchType,    nameof(EmbryoImplantReportPostfix)));

            harmony.Patch(AccessTools.Method(typeof(LifeStageWorker_HumanlikeChild), nameof(LifeStageWorker_HumanlikeChild.Notify_LifeStageStarted)), 
                          postfix: new HarmonyMethod(patchType, nameof(ChildLifeStageStartedPostfix)), transpiler: new HarmonyMethod(patchType, nameof(ChildLifeStageStartedTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(LifeStageWorker_HumanlikeAdult), nameof(LifeStageWorker_HumanlikeAdult.Notify_LifeStageStarted)),
                          postfix: new HarmonyMethod(patchType, nameof(AdultLifeStageStartedPostfix)), transpiler: new HarmonyMethod(patchType, nameof(AdultLifeStageStartedTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "GetBackstoryCategoryFiltersFor"), postfix: new HarmonyMethod(patchType, nameof(GetBackstoryCategoryFiltersForPostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(QuestNode_Root_WandererJoin_WalkIn), nameof(QuestNode_Root_WandererJoin_WalkIn.GeneratePawn)), transpiler: new HarmonyMethod(patchType, nameof(WandererJoinTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PregnancyUtility),                   nameof(PregnancyUtility.ApplyBirthOutcome)),              transpiler: new HarmonyMethod(patchType, nameof(ApplyBirthOutcomeTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator),                      nameof(PawnGenerator.XenotypesAvailableFor)) ,            postfix: new HarmonyMethod(patchType,    nameof(XenotypesAvailableForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator),                      nameof(PawnGenerator.GetXenotypeForGeneratedPawn)),       transpiler: new HarmonyMethod(patchType, nameof(GetXenotypeForGeneratedPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker),                   nameof(Pawn_GeneTracker.SetXenotype)), new HarmonyMethod(patchType, nameof(SetXenotypePrefix)));
            harmony.Patch(AccessTools.Method(typeof(CharacterCardUtility), "LifestageAndXenotypeOptions"),                        transpiler: new HarmonyMethod(patchType, nameof(LifestageAndXenotypeOptionsTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.AdjustXenotypeForFactionlessPawn)), transpiler: new HarmonyMethod(patchType, nameof(LifestageAndXenotypeOptionsTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(StartingPawnUtility),  nameof(StartingPawnUtility.NewGeneratedStartingPawn)), transpiler: new HarmonyMethod(patchType, nameof(NewGeneratedStartingPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnHairColors),       nameof(PawnHairColors.HasGreyHair)),                   transpiler: new HarmonyMethod(patchType, nameof(HasGreyHairTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(Dialog_StylingStation), "DoWindowContents"), transpiler: new HarmonyMethod(typeof(StylingStation), nameof(StylingStation.DoWindowContentsTranspiler)));
            harmony.Patch(AccessTools.Constructor(typeof(Dialog_StylingStation), [typeof(Pawn), typeof(Thing)]), postfix: new HarmonyMethod(typeof(StylingStation), nameof(StylingStation.ConstructorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Dialog_StylingStation), "Reset"), postfix: new HarmonyMethod(typeof(StylingStation), nameof(StylingStation.ResetPostfix)));

            harmony.Patch(AccessTools.Method(typeof(ApparelProperties), nameof(ApparelProperties.PawnCanWear), [typeof(Pawn), typeof(bool)]), postfix: new HarmonyMethod(patchType, nameof(PawnCanWearPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Scenario), nameof(Scenario.PostIdeoChosen)), prefix: new HarmonyMethod(patchType, nameof(ScenarioPostIdeoChosenPrefix)));
            harmony.Patch(AccessTools.Method(typeof(StartingPawnUtility), "RegenerateStartingPawnInPlace"), prefix: new HarmonyMethod(patchType, nameof(RegenerateStartingPawnInPlacePrefix)));
            
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), "GrowthPointsFactor"), postfix: new HarmonyMethod(patchType, nameof(GrowthPointsFactorPostfix)));

            harmony.Patch(AccessTools.Method(typeof(StatPart_Age), "AgeMultiplier"), new HarmonyMethod(patchType, nameof(StatPartAgeMultiplierPrefix)));

            AlienRenderTreePatches.HarmonyInit(harmony);


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
                    RaceRestrictionSettings.apparelRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteApparelList.Add(thingDef);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.weaponList)
                {
                    RaceRestrictionSettings.weaponRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteWeaponList.Add(thingDef);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.buildingList)
                {
                    RaceRestrictionSettings.buildingRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteBuildingList.Add(thingDef);
                }

                foreach (RecipeDef recipeDef in ar.alienRace.raceRestriction.recipeList)
                {
                    RaceRestrictionSettings.recipeRestricted.Add(recipeDef);
                    ar.alienRace.raceRestriction.whiteRecipeList.Add(recipeDef);
                }
                
                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.plantList)
                {
                    RaceRestrictionSettings.plantRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whitePlantList.Add(thingDef);
                }
                
                foreach (TraitDef traitDef in ar.alienRace.raceRestriction.traitList)
                {
                    RaceRestrictionSettings.traitRestricted.Add(traitDef);
                    ar.alienRace.raceRestriction.whiteTraitList.Add(traitDef);
                }
                
                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.foodList)
                {
                    RaceRestrictionSettings.foodRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteFoodList.Add(thingDef);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.petList)
                {
                    RaceRestrictionSettings.petRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whitePetList.Add(thingDef);
                }

                foreach (ResearchProjectDef projectDef in ar.alienRace.raceRestriction.researchList.SelectMany(selector: rl => rl?.projects))
                {
                    if (!RaceRestrictionSettings.researchRestrictionDict.ContainsKey(projectDef))
                        RaceRestrictionSettings.researchRestrictionDict.Add(projectDef, new List<ThingDef_AlienRace>());
                    RaceRestrictionSettings.researchRestrictionDict[projectDef].Add(ar);
                }

                foreach (GeneDef geneDef in ar.alienRace.raceRestriction.geneList)
                {
                    RaceRestrictionSettings.geneRestricted.Add(geneDef);
                    ar.alienRace.raceRestriction.whiteGeneList.Add(geneDef);
                }

                foreach (GeneDef geneDef in ar.alienRace.raceRestriction.geneListEndo)
                {
                    RaceRestrictionSettings.geneRestrictedEndo.Add(geneDef);
                    ar.alienRace.raceRestriction.whiteGeneListEndo.Add(geneDef);
                }

                foreach (GeneDef geneDef in ar.alienRace.raceRestriction.geneListXeno)
                {
                    RaceRestrictionSettings.geneRestrictedXeno.Add(geneDef);
                    ar.alienRace.raceRestriction.whiteGeneListXeno.Add(geneDef);
                }

                foreach (XenotypeDef xenotypeDef in ar.alienRace.raceRestriction.xenotypeList)
                {
                    RaceRestrictionSettings.xenotypeRestricted.Add(xenotypeDef);
                    ar.alienRace.raceRestriction.whiteXenotypeList.Add(xenotypeDef);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.reproductionList)
                {
                    RaceRestrictionSettings.reproductionRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteReproductionList.Add(thingDef);
                }

                if (ar.race.hasCorpse && ar.alienRace.generalSettings.corpseCategory != ThingCategoryDefOf.CorpsesHumanlike)
                {
                    ThingCategoryDefOf.CorpsesHumanlike.childThingDefs.Remove(ar.race.corpseDef);
                    if (ar.alienRace.generalSettings.corpseCategory != null)
                    {
                        ar.race.corpseDef.thingCategories = [ar.alienRace.generalSettings.corpseCategory];
                        ar.alienRace.generalSettings.corpseCategory.childThingDefs.Add(ar.race.corpseDef);
                        ar.alienRace.generalSettings.corpseCategory.ResolveReferences();
                    }
                    ThingCategoryDefOf.CorpsesHumanlike.ResolveReferences();
                }

                ar.alienRace.generalSettings.alienPartGenerator.GenerateMeshsAndMeshPools();

                if (ar.alienRace.generalSettings.humanRecipeImport && ar != ThingDefOf.Human)
                {
                    (ar.recipes ??= new List<RecipeDef>()).AddRange(ThingDefOf.Human.recipes.Where(predicate: rd => !rd.targetsBodyPart                  ||
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

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                AnimalBodyAddons extension = def.GetModExtension<AnimalBodyAddons>();
                if (extension != null)
                {
                    extension.GenerateAddonData(def);
                    def.comps.Add(new CompProperties(typeof(AnimalComp)));
                }
            }

            {
                FieldInfo bodyInfo = AccessTools.Field(typeof(RaceProperties), nameof(RaceProperties.body));
                MethodInfo bodyCheck = AccessTools.Method(patchType, nameof(ReplacedBody));
                HarmonyMethod bodyTranspiler = new(patchType, nameof(BodyReferenceTranspiler));

                //Full assemblies scan
                foreach (MethodInfo mi in typeof(LogEntry).Assembly.GetTypes().
                    SelectMany(t => t.GetNestedTypes(AccessTools.all).Concat(t)).
                    Where(predicate: t => (!t.IsAbstract || t.IsSealed) && !typeof(Delegate).IsAssignableFrom(t) && !t.IsGenericType).SelectMany(selector: t =>
                        t.GetMethods(AccessTools.all).Concat(t.GetProperties(AccessTools.all).SelectMany(selector: pi => pi.GetAccessors(nonPublic: true)))
                      .Where(predicate: mi => mi != null && !mi.IsAbstract && mi.DeclaringType == t && !mi.IsGenericMethod && !mi.HasAttribute<DllImportAttribute>())).Distinct()// && mi.GetMethodBody()?.GetILAsByteArray()?.Length > 1))
                ) //.Select(mi => mi.IsGenericMethod ? mi.MakeGenericMethod(mi.GetGenericArguments()) : mi))
                {
                    IEnumerable<KeyValuePair<OpCode, object>> instructions = PatchProcessor.ReadMethodBody(mi);
                    if (mi != bodyCheck && instructions.Any(predicate: il => il.Value?.Equals(bodyInfo) ?? false))
                        harmony.Patch(mi, transpiler: bodyTranspiler);
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
                        harmony.Patch(mi, transpiler: new HarmonyMethod(patchType, nameof(PostureTranspiler)));
                }
            }

            Log.Message($"Alien race successfully completed {harmony.harmony.GetPatchedMethods().Select(Harmony.GetPatchInfo).SelectMany(selector: p => p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers)).Count(predicate: p => p.owner == harmony.harmony.Id)} patches with harmony.");
            HairDefOf.Bald.styleTags.Add(item: "alienNoStyle");
            BeardDefOf.NoBeard.styleTags.Add(item: "alienNoStyle");
            TattooDefOf.NoTattoo_Body.styleTags.Add(item: "alienNoStyle");
            TattooDefOf.NoTattoo_Face.styleTags.Add(item: "alienNoStyle");

            AlienRaceMod.settings.UpdateSettings();
        }

        public static bool StatPartAgeMultiplierPrefix(ref float __result, StatPart_Age __instance, Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace race && race.alienRace.generalSettings.ageStatOverride.TryGetValue(__instance.parentStat, out StatPart_Age overridePart))
            {
                ref bool        useBiologicalYears = ref CachedData.statPartAgeUseBiologicalYearsField.Invoke(overridePart);
                ref SimpleCurve curve              = ref CachedData.statPartAgeCurveField.Invoke(overridePart);

                __result = useBiologicalYears ?
                               curve.Evaluate(pawn.ageTracker.AgeBiologicalYears) :
                               curve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat / pawn.RaceProps.lifeExpectancy);

                return false;
            }
            return true;
        }

        public static void GrowthPointsFactorPostfix(Pawn_AgeTracker __instance, ref float __result, Pawn ___pawn)
        {
            if (___pawn.def is ThingDef_AlienRace alienProps)
                if (alienProps.alienRace.generalSettings.growthFactorByAge != null)
                    __result = alienProps.alienRace.generalSettings.growthFactorByAge.Evaluate(__instance.AgeBiologicalYears);
        }


        public static void RegenerateStartingPawnInPlacePrefix() => 
            firstStartingRequest = false;

        public static void ScenarioPostIdeoChosenPrefix() => 
            firstStartingRequest = true;

        public static void PawnCanWearPostfix(ApparelProperties __instance, Pawn pawn, ref bool __result) => 
            __result &= RaceRestrictionSettings.CanWear(CachedData.GetApparelFromApparelProps(__instance), pawn.def);

        public static IEnumerable<CodeInstruction> HasGreyHairTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldc_I4_S)
                    instruction.operand = -1;

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(patchType, nameof(HasOldHairHelper));
                }
                yield return instruction;
            }
        }

        public static float HasOldHairHelper(float originalChance, Pawn pawn) =>
            pawn.def is ThingDef_AlienRace alienProps ? 
                alienProps.alienRace.generalSettings.alienPartGenerator.oldHairAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat) : 
                originalChance;

        public static PawnGenerationRequest currentStartingRequest;
        public static bool                  firstStartingRequest;
        
        public static IEnumerable<CodeInstruction> NewGeneratedStartingPawnTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg, MethodBase originalMethod)
        {
            MethodInfo getRequestInfo = AccessTools.Method(typeof(StartingPawnUtility), nameof(StartingPawnUtility.GetGenerationRequest));
            
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (instruction.Calls(getRequestInfo))
                {
                    yield return CodeInstruction.LoadField(typeof(AlienRaceMod),      nameof(AlienRaceMod.settings));
                    yield return CodeInstruction.LoadField(typeof(AlienRaceSettings), nameof(AlienRaceSettings.randomizeStartingPawnsOnReroll));
                    yield return new CodeInstruction(OpCodes.Brfalse, instructionList[i + 1].operand);
                    yield return CodeInstruction.LoadField(typeof(HarmonyPatches), nameof(firstStartingRequest));
                    yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i + 1].operand);
                    yield return new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(patchType, nameof(currentStartingRequest)));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, instructionList[i + 2].operand);
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return CodeInstruction.Call(typeof(StartingPawnUtility), nameof(StartingPawnUtility.SetGenerationRequest));
                    yield return new CodeInstruction(OpCodes.Ldsflda,  AccessTools.Field(patchType, nameof(currentStartingRequest)));
                    yield return new CodeInstruction(OpCodes.Initobj, typeof(PawnGenerationRequest));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                }
            }
        }
        
        public static IEnumerable<CodeInstruction> LifestageAndXenotypeOptionsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo dbXenoInfo = AccessTools.PropertyGetter(typeof(DefDatabase<XenotypeDef>), nameof(DefDatabase<XenotypeDef>.AllDefs));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.Calls(dbXenoInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.def)));
                    yield return new CodeInstruction(OpCodes.Call,  AccessTools.Method(patchType, nameof(FilterXenotypeHelper)));
                }
            }
        }

        public static bool SetXenotypePrefix(XenotypeDef xenotype, Pawn ___pawn) => 
            RaceRestrictionSettings.CanUseXenotype(xenotype, ___pawn.def);

        public static IEnumerable<CodeInstruction> GetXenotypeForGeneratedPawnTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            MethodInfo pawnGenAllowedXenotypeInfo = AccessTools.PropertyGetter(typeof(PawnGenerationRequest), nameof(PawnGenerationRequest.AllowedXenotypes));

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                yield return instruction;

                if (instruction.Calls(pawnGenAllowedXenotypeInfo) && instructionList[i+1].opcode == OpCodes.Ldloca_S)
                {
                    yield return new CodeInstruction(instructionList[i-1]);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(PawnGenerationRequest), nameof(PawnGenerationRequest.KindDef)));
                    yield return CodeInstruction.LoadField(typeof(PawnKindDef), nameof(PawnKindDef.race));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(FilterXenotypeHelper)));
                }
            }
        }

        public static List<XenotypeDef> FilterXenotypeHelper(List<XenotypeDef> xenotypes, ThingDef race) => 
            RaceRestrictionSettings.FilterXenotypes(xenotypes, race, out _).ToList();

        public static void XenotypesAvailableForPostfix(PawnKindDef kind, ref Dictionary<XenotypeDef, float> __result)
        {
            RaceRestrictionSettings.FilterXenotypes(__result.Keys, kind.race, out HashSet<XenotypeDef> forbidden);

            foreach (XenotypeDef xenotypeDef in forbidden)
                __result.Remove(xenotypeDef);
        }

        public static int currentBirthCount = int.MinValue;

        public static IEnumerable<CodeInstruction> ApplyBirthOutcomeTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg, MethodBase originalMethod)
        {
            FieldInfo pawnKindInfo = AccessTools.Field(typeof(Pawn), nameof(Pawn.kindDef));
            FieldInfo countInfo    = AccessTools.Field(patchType,    nameof(currentBirthCount));

            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (instruction.opcode == OpCodes.Ldarg_S && instructionList[i+1].LoadsField(pawnKindInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg, 6);
                    yield return CodeInstruction.Call(patchType, nameof(BirthOutcomeHelper));
                    i ++;
                }

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    Label loop    = ilg.DefineLabel();
                    Label loopEnd = ilg.DefineLabel();
                    Label loopSkip = ilg.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Ldsfld, countInfo);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, int.MinValue);
                    yield return new CodeInstruction(OpCodes.Bne_Un, loopSkip);
                    yield return new CodeInstruction(OpCodes.Ldarg,  4);
                    yield return CodeInstruction.Call(patchType, nameof(BirthOutcomeMultiplier));
                    yield return new CodeInstruction(OpCodes.Stsfld, countInfo);
                    yield return new CodeInstruction(OpCodes.Ldsfld, countInfo) {labels = new List<Label>{loop}};
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Sub);
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stsfld, countInfo);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Blt_S, loopEnd);
                    for (int j = 0; j < originalMethod.GetParameters().Length; j++)
                        yield return new CodeInstruction(OpCodes.Ldarg, j);
                    yield return new CodeInstruction(OpCodes.Call,   originalMethod);
                    yield return new CodeInstruction(OpCodes.Br, loop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, int.MinValue) { labels = new List<Label> { loopEnd } };
                    yield return new CodeInstruction(OpCodes.Stsfld, countInfo);
                    yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { loopSkip } };
                }
            }
        }

        public static int BirthOutcomeMultiplier(Pawn mother) => 
            mother != null ? Mathf.RoundToInt(Rand.ByCurve(mother.RaceProps.litterSizeCurve)) - 1 : 0;

        public static PawnKindDef BirthOutcomeHelper(Pawn mother, Pawn partner)
        {
            if (mother?.def is not ThingDef_AlienRace alienProps)
                return mother?.kindDef;

            PawnKindDef kindDef = alienProps.alienRace.generalSettings.reproduction.childKindDef;

            if (partner != null)
            {
                List<HybridSpecificSettings> hybrids = alienProps.alienRace.generalSettings.reproduction.hybridSpecific.Where(hss => hss.partnerRace == partner.def).ToList();
                if (hybrids.Any() && hybrids.TryRandomElementByWeight(hss => hss.probability, out HybridSpecificSettings res))
                    kindDef = res.childKindDef;
            }

            return kindDef ?? mother.kindDef;
        }

        public static void AdultLifeStageStartedPostfix(Pawn pawn) =>
            LongEventHandler.ExecuteWhenFinished(() =>
                                                 {
                                                     //Log.Message("adult life started");
                                                     List<BackstoryTrait> forcedTraits = pawn.story?.Adulthood?.forcedTraits;
                                                     if (!forcedTraits.NullOrEmpty())
                                                         foreach (BackstoryTrait te2 in forcedTraits!)
                                                             if (te2.def == null)
                                                                 Log.Error("Null forced trait def on " + pawn.story.Adulthood);
                                                             else if (!pawn.story.traits.HasTrait(te2.def))
                                                                 pawn.story.traits.GainTrait(new Trait(te2.def, te2.degree));
                                                 });

        public static void ChildLifeStageStartedPostfix(Pawn pawn) =>
            LongEventHandler.ExecuteWhenFinished(() =>
                                                 {
                                                     List<BackstoryTrait> forcedTraits = pawn.story?.Childhood?.forcedTraits;
                                                     if (!forcedTraits.NullOrEmpty())
                                                         foreach (BackstoryTrait te2 in forcedTraits!)
                                                             if (te2.def == null)
                                                                 Log.Error("Null forced trait def on " + pawn.story.Childhood);
                                                             else if (!pawn.story.traits.HasTrait(te2.def))
                                                                 pawn.story.traits.GainTrait(new Trait(te2.def, te2.degree));
                                                 });

        public static IEnumerable<CodeInstruction> WandererJoinTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if(instruction.opcode == OpCodes.Ldloc_1)
                    yield return CodeInstruction.Call(patchType, nameof(WandererJoinHelper));
            }
        }

        public static PawnGenerationRequest WandererJoinHelper(PawnGenerationRequest request)
        {
            PawnKindDef kindDef = request.KindDef;

            if (kindDef.race != Faction.OfPlayerSilentFail?.def.basicMemberKind.race)
                kindDef = Faction.OfPlayerSilentFail?.def.basicMemberKind ?? kindDef;
            
            if (DefDatabase<RaceSettings>.AllDefsListForReading.Where(predicate: tdar => !tdar.pawnKindSettings.alienwandererkinds.NullOrEmpty())
                                      .SelectMany(selector: rs => rs.pawnKindSettings.alienwandererkinds).Where(predicate: fpke => fpke.factionDefs.Contains(Faction.OfPlayer.def))
                                      .SelectMany(selector: fpke => fpke.pawnKindEntries).TryRandomElementByWeight(pke => pke.chance, out PawnKindEntry pk))
                kindDef = pk.kindDefs.RandomElement();

            request.KindDef = kindDef;
            return request;
        }

        public static void EmbryoImplantReportPostfix(HumanEmbryo __instance, Pawn pawn, ref AcceptanceReport __result)
        {
            Pawn second = __instance.TryGetComp<CompHasPawnSources>().pawnSources?.FirstOrDefault();

            if(second != null && pawn != null && second.def != pawn.def)
                __result = false;
        }

        public static void EmbryoImplantPawnPrefix(HumanEmbryo __instance, ref bool cancel)
        {
            if (__instance.implantTarget is Pawn pawn)
            {
                Pawn second = __instance.TryGetComp<CompHasPawnSources>().pawnSources?.FirstOrDefault();
                cancel = second != null && second.def != pawn.def;
            }
        }

        public static IEnumerable<CodeInstruction> AdultLifeStageStartedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo backstoryFilters = AccessTools.Field(typeof(LifeStageWorker_HumanlikeAdult), "VatgrowBackstoryFilter");
            MethodInfo isPlayerColonyChildBackstory = AccessTools.PropertyGetter(typeof(BackstoryDef), nameof(BackstoryDef.IsPlayerColonyChildBackstory));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1) { labels = instruction.ExtractLabels()};
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return CodeInstruction.Call(patchType, nameof(LifeStageStartedHelper));
                }

                if (instruction.Calls(isPlayerColonyChildBackstory))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1) { labels = instruction.ExtractLabels() };
                    yield return CodeInstruction.Call(patchType, nameof(IsPlayerColonyChildBackstoryHelper));
                } else
                {
                    yield return instruction;
                }


                if (instruction.LoadsField(backstoryFilters))
                {
                    
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                    yield return CodeInstruction.Call(patchType, nameof(LifeStageStartedHelper));
                }
            }
        }

        public static IEnumerable<CodeInstruction> ChildLifeStageStartedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo backstoryFilters = AccessTools.Field(typeof(LifeStageWorker_HumanlikeChild), "ChildBackstoryFilters");

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(backstoryFilters))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return CodeInstruction.Call(patchType, nameof(LifeStageStartedHelper));
                }
            }
        }

        public static List<BackstoryCategoryFilter> LifeStageStartedHelper(List<BackstoryCategoryFilter> filters, Pawn pawn, int backstoryKind)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                List<BackstoryCategoryFilter> filtersNew = backstoryKind switch
                {
                    0 => alienProps.alienRace.generalSettings.childBackstoryFilter,
                    1 => alienProps.alienRace.generalSettings.adultBackstoryFilter,
                    2 => alienProps.alienRace.generalSettings.adultVatBackstoryFilter,
                    3 => alienProps.alienRace.generalSettings.newbornBackstoryFilter,
                    _ => null
                };

                return filtersNew.NullOrEmpty() ? 
                           filters : 
                           filtersNew;
            }

            return filters;
        }

        public static bool IsPlayerColonyChildBackstoryHelper(BackstoryDef backstory, Pawn pawn) =>
            ((pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.childBackstoryFilter?.Any(bcf => bcf.Matches(backstory)) ?? false) || 
            backstory.IsPlayerColonyChildBackstory;

        public static void GetBackstoryCategoryFiltersForPostfix(Pawn pawn, ref List<BackstoryCategoryFilter> __result)
        {
            if (pawn.def is ThingDef_AlienRace && pawn.DevelopmentalStage.Juvenile())
            {
                int index = 0;
                if (pawn.DevelopmentalStage.Baby())
                    index = 3;
                __result = LifeStageStartedHelper(__result, pawn, index);
            }
        }

        public static void HumanOvumCanFertilizeReportPostfix(Pawn pawn, ref AcceptanceReport __result)
        {
            if (__result.Accepted)
            {
                Pawn second = pawn.TryGetComp<CompHasPawnSources>()?.pawnSources?.FirstOrDefault();

                if(second != null ? 
                       !RaceRestrictionSettings.CanReproduce(second, pawn) : 
                       !(pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.canReproduce ?? true)
                    __result = false;
                
            }
        }

        public static void ExtractOvumAvailableReportPostfix(Thing thing, ref AcceptanceReport __result)
        {
            if (__result.Accepted && thing.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.raceRestriction.canReproduce)
                __result = false;
        }

        public static IEnumerable<CodeInstruction> CanEverProduceChildTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            FieldInfo genderInfo = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));

            int step = 0;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.LoadsField(genderInfo))
                {
                    switch (step)
                    {
                        case 0:
                            step++;
                            yield return instructionList[i + 1];
                            yield return CodeInstruction.Call(typeof(ReproductionSettings), nameof(ReproductionSettings.GenderReproductionCheck));
                            yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i + 3].operand);
                            i += 3;
                            break;
                        case 1:
                            step++;
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            yield return CodeInstruction.Call(typeof(ReproductionSettings), nameof(ReproductionSettings.ApplicableGender), new[] { typeof(Pawn), typeof(bool) });
                            yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i + 2].operand);
                            i += 2;
                            break;
                        case 2:
                            step++;
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            yield return CodeInstruction.Call(typeof(ReproductionSettings), nameof(ReproductionSettings.ApplicableGender), new[] { typeof(Pawn), typeof(bool) });
                            yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i + 2].operand);
                            i += 2;
                            break;
                    }
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static void CanEverProduceChildPostfix(Pawn first, Pawn second, ref AcceptanceReport __result)
        {
            if (__result.Accepted)
            {
                if (!RaceRestrictionSettings.CanReproduce(first, second))
                    __result = "HAR.ReproductionNotAllowed".Translate(new NamedArgument(first.gender.GetLabel(),  "genderOne"), new NamedArgument(first.def.LabelCap,  "raceOne"),
                                                                      new NamedArgument(second.gender.GetLabel(), "genderTwo"), new NamedArgument(second.def.LabelCap, "raceTwo"));
            }
        }

        public static bool PregnancyChanceForPartnersPrefix(Pawn woman, Pawn man, ref float __result)
        {
            if (!RaceRestrictionSettings.CanReproduce(woman, man))
            {
                __result = 0f;
                return false;
            }
            return true;
        }

        public static bool AddGenePrefix(Gene gene, Pawn ___pawn, ref Gene __result, bool addAsXenogene)
        {
            if (!RaceRestrictionSettings.CanHaveGene(gene.def, ___pawn.def, addAsXenogene))
            {
                __result = null;
                return false;
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> FertilityAgeFactorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo maleInfo = AccessTools.Field(typeof(StatPart_FertilityByGenderAge), "maleFertilityAgeFactor");
            FieldInfo femaleInfo = AccessTools.Field(typeof(StatPart_FertilityByGenderAge), "femaleFertilityAgeFactor");

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.LoadsField(maleInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return CodeInstruction.Call(patchType, nameof(FertilityCurveHelper));
                } else if (instruction.LoadsField(femaleInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                    yield return CodeInstruction.Call(patchType, nameof(FertilityCurveHelper));
                }
            }
        }

        public static SimpleCurve FertilityCurveHelper(SimpleCurve original, Pawn pawn, Gender gender) =>
            pawn.def is ThingDef_AlienRace alienProps ?
                gender == Gender.Female ?
                    alienProps.alienRace.generalSettings.reproduction.femaleFertilityAgeFactor :
                    alienProps.alienRace.generalSettings.reproduction.maleFertilityAgeFactor :
                original;

        public static void FinalizeLookChangePostfix(ref Toil __result)
        {
            Action initAction = __result.initAction;
            Toil   toil       = __result;
            __result.initAction = () =>
                              {
                                  initAction();
                                  toil.actor.GetComp<AlienPartGenerator.AlienComp>()?.OverwriteColorChannel("hair", toil.actor.style.nextHairColor);
                              };
        }

        public static void FinalizeHairColorPostfix(Pawn_StyleTracker __instance) =>
            __instance.pawn.GetComp<AlienPartGenerator.AlienComp>()?.OverwriteColorChannel("hair", __instance.pawn.style.nextHairColor);

        public static IEnumerable<CodeInstruction> GrowthMomentCacheLetterTextTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList      = instructions.ToList();
            FieldInfo             growthMomentAgesInfo = AccessTools.Field(typeof(GrowthUtility), nameof(GrowthUtility.GrowthMomentAges));


            foreach (CodeInstruction instruction in instructionList)
            {
                if (instruction.LoadsField(growthMomentAgesInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_AgeTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn),            nameof(Pawn.def)));
                    yield return CodeInstruction.Call(patchType, nameof(GrowthMomentHelper), new[] { typeof(ThingDef) });
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static IEnumerable<CodeInstruction> TrySimulateGrowthPointsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList      = instructions.ToList();
            FieldInfo             growthMomentAgesInfo = AccessTools.Field(typeof(GrowthUtility), nameof(GrowthUtility.GrowthMomentAges));
            FieldInfo             growthMomentAgesCacheInfo = AccessTools.Field(typeof(Pawn_AgeTracker), "growthMomentAges");


            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.LoadsField(growthMomentAgesCacheInfo) && instructionList[i+1].opcode == OpCodes.Brtrue_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0).MoveLabelsFrom(instruction);
                } 
                else if (instruction.LoadsField(growthMomentAgesInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_AgeTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn),            nameof(Pawn.def)));
                    yield return CodeInstruction.Call(patchType, nameof(GrowthMomentHelper), [typeof(ThingDef)]);
                }
                else if (instruction.opcode == OpCodes.Ldc_I4_3)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_AgeTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn),            nameof(Pawn.def)));
                    yield return CodeInstruction.Call(patchType, nameof(GetBabyToChildAge));
                }
                else if (instruction.Is(OpCodes.Ldc_I4_S, 13))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_AgeTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn),            nameof(Pawn.def)));
                    yield return CodeInstruction.Call(patchType, nameof(GetChildToAdultAge));
                }
                else
                {
                    if (instruction.Is(OpCodes.Ldc_R4, 7))
                    {
                        yield return new CodeInstruction(OpCodes.Dup);
                    } 
                    else if (instruction.opcode == OpCodes.Mul && instructionList[i+1].opcode == OpCodes.Stloc_2)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_AgeTracker), "pawn"));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn),            nameof(Pawn.def)));
                        yield return CodeInstruction.Call(patchType, nameof(EvalGrowthPointsHelper));
                    }

                    yield return instruction;
                }
            }
        }

        public static float EvalGrowthPointsHelper(float age, float original, ThingDef pawnDef)
        {
            if (pawnDef is ThingDef_AlienRace alienProps)
                if (alienProps.alienRace.generalSettings.growthFactorByAge != null)
                    return alienProps.alienRace.generalSettings.growthFactorByAge.Evaluate(age);
            return original;
        }

        public static int GetBabyToChildAge(ThingDef pawnDef) => 
            Mathf.FloorToInt(pawnDef.race.lifeStageAges.First(lsa => lsa.def.developmentalStage.HasAny(DevelopmentalStage.Child | DevelopmentalStage.Adult)).minAge);

        public static int GetChildToAdultAge(ThingDef pawnDef) =>
            Mathf.FloorToInt(pawnDef.race.lifeStageAges.First(lsa => lsa.def.developmentalStage.HasAny(DevelopmentalStage.Adult)).minAge);

        public static void GrowthTierTooltipPrefix(Pawn ___child) =>
            growthMomentPawn = ___child;

        public static void TryChildGrowthMomentPrefix(Pawn ___pawn) =>
            growthMomentPawn = ___pawn;

        public static void GenerateSkillsPrefix(Pawn pawn) =>
            growthMomentPawn = pawn;

        public static void GenerateTraitsPrefix(Pawn pawn) =>
            growthMomentPawn = pawn;

        public static Pawn growthMomentPawn;

        public static IEnumerable<CodeInstruction> IsGrowthBirthdayTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
                if (instruction.opcode == OpCodes.Ldsfld)
                    yield return CodeInstruction.Call(patchType, nameof(GrowthMomentHelper), Type.EmptyTypes);
                else
                    yield return instruction;
        }

        public static int[] GrowthMomentHelper() =>
            GrowthMomentHelper(growthMomentPawn.def);

        public static int[] GrowthMomentHelper(ThingDef pawnDef) =>
            (pawnDef as ThingDef_AlienRace)?.alienRace.generalSettings.GrowthAges ?? GrowthUtility.GrowthMomentAges;

        public static IEnumerable<CodeInstruction> NotifyGenesChangedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo allDefsInfo = AccessTools.PropertyGetter(typeof(DefDatabase<HeadTypeDef>), nameof(DefDatabase<HeadTypeDef>.AllDefs));

            bool dirtyFlagSet = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                }

                yield return instruction;
                if (instruction.Calls(allDefsInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.pawn)));
                    yield return CodeInstruction.Call(patchType, nameof(HeadTypeFilter));
                }
            }
        }

        public static void TryGenerateNewPawnInternalPrefix(ref PawnGenerationRequest request)
        {
            if (!request.KindDef.race.race.IsFlesh)
                request.AllowGay = false;
        }

        public static IEnumerable<CodeInstruction> TryGenerateNewPawnInternalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool done = false;

            MethodInfo allDefsInfo = AccessTools.PropertyGetter(typeof(DefDatabase<HeadTypeDef>), nameof(DefDatabase<HeadTypeDef>.AllDefs));

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (!done && instruction.Calls(allDefsInfo))
                {
                    done = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(instructionList[i-2]);
                    yield return CodeInstruction.Call(patchType, nameof(HeadTypeFilter));
                }
            }
        }

        public static IEnumerable<HeadTypeDef> HeadTypeFilter(IEnumerable<HeadTypeDef> headTypes, Pawn pawn) => 
            pawn.def is ThingDef_AlienRace alienProps ? 
                headTypes.Intersect(alienProps.alienRace.generalSettings.alienPartGenerator.HeadTypes) :
                headTypes;

        public static void GenerateSkillsPostfix(Pawn pawn)
        {
            foreach (BackstoryDef backstory in pawn.story.AllBackstories)
                if (backstory is AlienBackstoryDef alienBackstory)
                {
                    IEnumerable<SkillGain> passions = alienBackstory.passions;

                    if (pawn.def is ThingDef_AlienRace alienProps)
                        passions = passions.Concat(alienProps.alienRace.generalSettings.passions);

                    foreach (SkillGain passion in passions)
                        pawn.skills.GetSkill(passion.skill).passion = (Passion)passion.amount;
                }
        }

        public static IEnumerable<CodeInstruction> GenerateRandomMinTicksToNextLovinTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo ageCurveInfo = AccessTools.Field(typeof(JobDriver_Lovin), "LovinIntervalHoursFromAgeCurve");

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(ageCurveInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.Call(patchType, nameof(LovinInterval));
                }
            }
        }

        public static SimpleCurve LovinInterval(SimpleCurve humanDefault, Pawn pawn) => 
            pawn.def is ThingDef_AlienRace alienProps ? alienProps.alienRace.generalSettings.lovinIntervalHoursFromAge ?? humanDefault : humanDefault;

        public static IEnumerable<CodeInstruction> UnwillingWillingToDoGenderedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;

                if (instruction.opcode == OpCodes.Ldloc_0)
                {
                    yield return CodeInstruction.LoadField(typeof(Pawn),           nameof(Pawn.def));
                    yield return CodeInstruction.LoadField(typeof(ThingDef),       nameof(ThingDef.race));
                    yield return CodeInstruction.LoadField(typeof(RaceProperties), nameof(RaceProperties.hasGenders));
                    yield return new CodeInstruction(OpCodes.Brfalse, instructionList[i + 4].operand);
                    yield return new CodeInstruction(instruction);
                }
            }
        }

        public static void FoodUtilityAddThoughtsFromIdeoPrefix(ref HistoryEventDef eventDef, Pawn ingester, ThingDef foodDef, MeatSourceCategory meatSourceCategory)
        {
            if (meatSourceCategory == MeatSourceCategory.Humanlike)
            {
                bool alienMeat = (foodDef.IsCorpse || foodDef.IsMeat) && Utilities.DifferentRace(ingester.def, foodDef.ingestible.sourceDef);
                if(alienMeat)
                    eventDef = AlienDefOf.HAR_AteAlienMeat;
            }
        }

        public static void SetupLastHumanMeatTickPrefix(Pawn ___pawn)
        {
            AlienPartGenerator.AlienComp alienComp = ___pawn.GetComp<AlienPartGenerator.AlienComp>();
            if (alienComp != null)
            {
                alienComp.lastAlienMeatIngestedTick = Find.TickManager.TicksGame;
                alienComp.lastAlienMeatIngestedTick -= new IntRange(0, 60000).RandomInRange;
            }
        }

        public static IEnumerable<CodeInstruction> DrawTradeableRowTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo doerWillingInfo = AccessTools.Method(typeof(IdeoUtility), nameof(IdeoUtility.DoerWillingToDo), new []{typeof(HistoryEvent)});

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                yield return instruction;

                if (i > 5 && instructionList[i - 1].Calls(doerWillingInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(TradeSession), nameof(TradeSession.playerNegotiator)));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.Call(patchType, nameof(DrawTransferableRowIsWilling));
                    yield return new CodeInstruction(instruction);
                }
            }
        }

        public static bool DrawTransferableRowIsWilling(Pawn doer, Tradeable trad) => 
            trad is Tradeable_Pawn {AnyThing: Pawn _} && IdeoUtility.DoerWillingToDo(AlienDefOf.HAR_Alien_SoldSlave, doer);

        public static IEnumerable<CodeInstruction> TradeablePawnResolveTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            MethodInfo sellingSlaveryInfo = AccessTools.Method(typeof(GuestUtility), nameof(GuestUtility.IsSellingToSlavery));
            MethodInfo buyingSlaveryInfo = AccessTools.Method(typeof(ITrader), nameof(ITrader.GiveSoldThingToPlayer));

            Label orbitalTradeLabel = ilg.DefineLabel();

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(sellingSlaveryInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(TradeSession), nameof(TradeSession.playerNegotiator)));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return CodeInstruction.Call(typeof(List<>).MakeGenericType(typeof(Pawn)), "get_Item");
                    yield return CodeInstruction.Call(patchType, nameof(SoldSlave));
                }

                if (instruction.Calls(buyingSlaveryInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,    AccessTools.Field(typeof(TradeSession), nameof(TradeSession.trader)));
                    yield return new CodeInstruction(OpCodes.Isinst,    typeof(Pawn));
                    yield return new CodeInstruction(OpCodes.Brfalse,   orbitalTradeLabel);
                    yield return new CodeInstruction(OpCodes.Ldsfld,    AccessTools.Field(typeof(TradeSession), nameof(TradeSession.trader)));
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(Pawn));
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return CodeInstruction.Call(typeof(List<>).MakeGenericType(typeof(Pawn)), "get_Item");
                    yield return CodeInstruction.Call(patchType, nameof(SoldSlave));
                    yield return new CodeInstruction(OpCodes.Nop).WithLabels(orbitalTradeLabel);
                }
            }
        }

        public static void SoldSlave(Pawn pawn, Pawn slave)
        {
            if(Utilities.DifferentRace(pawn.def, slave.def) && ModsConfig.IdeologyActive)
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(AlienDefOf.HAR_Alien_SoldSlave, pawn.Named(HistoryEventArgsNames.Doer), slave.Named(HistoryEventArgsNames.Victim)));
        }

        public static void WillingToShareBedPostfix(Pawn pawn1, Pawn pawn2, ref bool __result)
        {
            if (Utilities.DifferentRace(pawn1.def, pawn2.def))
                if (!IdeoUtility.DoerWillingToDo(AlienDefOf.HAR_AlienDating_SharedBed, pawn1) || !IdeoUtility.DoerWillingToDo(AlienDefOf.HAR_AlienDating_SharedBed, pawn2))
                    __result = false;
        }

        public static void RomanceAttemptSuccessChancePostfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (Utilities.DifferentRace(initiator.def, recipient.def))
                if (!IdeoUtility.DoerWillingToDo(AlienDefOf.HAR_AlienDating_BeginRomance, initiator) || !IdeoUtility.DoerWillingToDo(AlienDefOf.HAR_AlienDating_BeginRomance, recipient))
                    __result = -1f;
        }

        public static IEnumerable<CodeInstruction> RomanceAttemptInteractTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo becameLoverInfo = AccessTools.Field(typeof(TaleDefOf), nameof(TaleDefOf.BecameLover));


            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsField(becameLoverInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return CodeInstruction.Call(patchType, nameof(NewLoverHelper));
                }

                yield return instruction;
            }
        }

        public static void NewLoverHelper(Pawn initiator, Pawn recipient)
        {
            if (Utilities.DifferentRace(initiator.def, recipient.def) && ModsConfig.IdeologyActive)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(AlienDefOf.HAR_AlienDating_Dating, initiator.Named(HistoryEventArgsNames.Doer), recipient.Named(HistoryEventArgsNames.Victim)));
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(AlienDefOf.HAR_AlienDating_Dating, recipient.Named(HistoryEventArgsNames.Doer), initiator.Named(HistoryEventArgsNames.Victim)));
            }
        }


        public static void IngestedPrefix(Pawn ingester, Thing __instance)
        {
            if (__instance.Destroyed || !__instance.IngestibleNow)
                return;

            if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(__instance))
            {
                bool alienMeat = (__instance.def.IsCorpse     && Utilities.DifferentRace(ingester.def, (__instance as Corpse)!.InnerPawn.def)) ||
                                 (__instance.def.IsIngestible && __instance.def.IsMeat && Utilities.DifferentRace(ingester.def, __instance.def.ingestible.sourceDef));

                CompIngredients compIngredients = __instance.TryGetComp<CompIngredients>();
                if (compIngredients != null)
                    foreach (ThingDef ingredient in compIngredients.ingredients)
                        if (ingredient.IsMeat && Utilities.DifferentRace(ingester.def, ingredient.ingestible.sourceDef))
                            alienMeat = true;
                if (ModsConfig.IdeologyActive)
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(alienMeat ? AlienDefOf.HAR_AteAlienMeat : AlienDefOf.HAR_AteNonAlienFood, ingester.Named(HistoryEventArgsNames.Doer)));
                if (alienMeat)
                {
                    AlienPartGenerator.AlienComp alienComp = ingester.GetComp<AlienPartGenerator.AlienComp>();
                    if (alienComp != null)
                        alienComp.lastAlienMeatIngestedTick = Find.TickManager.TicksGame;
                }
            }
        }

        public static IEnumerable<CodeInstruction> WoundWriteCacheTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            MethodInfo defaultAnchorInfo = AccessTools.Method(typeof(PawnWoundDrawer), "GetDefaultAnchor");

            List<CodeInstruction> instructionList = instructions.ToList();

            Label label = ilg.DefineLabel();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldarg_0 && instructionList[i + 4].Calls(defaultAnchorInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_3).MoveLabelsFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Brfalse, label);
                    instructionList[i + 5].WithLabels(label);
                }

                yield return instruction;
            }
        }

        public static void TotalStyleItemLikelihoodPostfix(ref float __result) => 
            __result += float.Epsilon;

        public static IEnumerable<CodeInstruction> KnowsMemoryThoughtTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            FieldInfo    thoughtInfo  = AccessTools.Field(typeof(PreceptComp_Thought), nameof(PreceptComp_Thought.thought));
            LocalBuilder thoughtLocal = ilg.DeclareLocal(typeof(ThoughtDef));
            
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, thoughtInfo);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return CodeInstruction.Call(patchType, nameof(KnowsGetHistoryEventThoughtDefReplacer));
                    yield return new CodeInstruction(OpCodes.Stloc, thoughtLocal.LocalIndex);
                }

                if (instruction.LoadsField(thoughtInfo))
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldloc, thoughtLocal.LocalIndex);
                }
            }
        }

        public static ThoughtDef KnowsGetHistoryEventThoughtDefReplacer(ThoughtDef thought, PreceptComp_KnowsMemoryThought comp, HistoryEvent ev, Precept precept)
        {
            ThoughtDef result = thought;

            ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn doer);
            ev.args.TryGetArg(HistoryEventArgsNames.Victim, out Pawn victim);

            if (thought == AlienDefOf.KnowButcheredHumanlikeCorpse)
            {
                if(Utilities.DifferentRace(doer.def, victim.def) && ModsConfig.IdeologyActive)
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(AlienDefOf.HAR_ButcheredAlien, doer.Named(HistoryEventArgsNames.Doer), victim.Named(HistoryEventArgsNames.Victim)));

                if (doer.def is ThingDef_AlienRace alienPropsPawn)
                    result = alienPropsPawn.alienRace.thoughtSettings.butcherThoughtSpecific
                                        ?.FirstOrDefault(predicate: bt => bt.raceList?.Contains(victim.def) ?? false)?.knowThought ??
                             alienPropsPawn.alienRace.thoughtSettings.butcherThoughtGeneral.knowThought;
            }

            return result;
        }

        public static IEnumerable<CodeInstruction> SelfTookMemoryThoughtTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            FieldInfo    thoughtInfo  = AccessTools.Field(typeof(PreceptComp_Thought), nameof(PreceptComp_Thought.thought));
            LocalBuilder thoughtLocal = ilg.DeclareLocal(typeof(ThoughtDef));

            bool first = true;

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(thoughtInfo))
                {
                    if (first)
                    {
                        first = false;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return CodeInstruction.Call(patchType, nameof(SelfTookGetHistoryEventThoughtDefReplacer));
                        yield return new CodeInstruction(OpCodes.Stloc, thoughtLocal.LocalIndex);
                    }
                    else
                        yield return new CodeInstruction(OpCodes.Pop);

                    yield return new CodeInstruction(OpCodes.Ldloc, thoughtLocal.LocalIndex);
                }
            }
        }

        public static ThoughtDef SelfTookGetHistoryEventThoughtDefReplacer(ThoughtDef thought, PreceptComp_SelfTookMemoryThought comp, HistoryEvent ev, Precept precept)
        {
            ThoughtDef result = thought;

            Pawn doer = ev.args.GetArg<Pawn>(HistoryEventArgsNames.Doer);
            ev.args.TryGetArg(HistoryEventArgsNames.Victim, out Pawn victim);

            if (thought == AlienDefOf.ButcheredHumanlikeCorpse)
            {
                if(doer.def is ThingDef_AlienRace alienPropsButcher)
                    result = alienPropsButcher.alienRace.thoughtSettings.butcherThoughtSpecific
                                           ?.FirstOrDefault(predicate: bt => bt.raceList?.Contains(victim.def) ?? false)?.thought ??
                             alienPropsButcher.alienRace.thoughtSettings.butcherThoughtGeneral.thought;
            }


            return result;
        }

        public static bool WantsToUseStylePrefix(Pawn pawn, StyleItemDef styleItemDef, ref bool __result)
        {
            if (pawn.def is not ThingDef_AlienRace alienProps || styleItemDef == null)
                return true;
            if (alienProps.alienRace.styleSettings[styleItemDef.GetType()].hasStyle)
            {
                if (!alienProps.alienRace.styleSettings[styleItemDef.GetType()].styleTagsOverride.NullOrEmpty())
                {
                    __result = alienProps.alienRace.styleSettings[styleItemDef.GetType()].IsValidStyle(styleItemDef, pawn, true);
                    return false;
                }

                return true;
            }

            __result = true;
            return false;
        }

        public static void WantsToUseStylePostfix(Pawn pawn, StyleItemDef styleItemDef, ref bool __result)
        {
            if (__result && pawn.def is ThingDef_AlienRace alienProps && styleItemDef != null)
                __result = alienProps.alienRace.styleSettings[styleItemDef.GetType()].IsValidStyle(styleItemDef, pawn);
        }

        public static void CacheRenderPawnPrefix(Pawn pawn, ref float cameraZoom, bool portrait)
        {
            if(!portrait)
                cameraZoom = 1f / ((pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.borderScale ?? 1f);
        }

        public static Pawn createPawnAtlasPawn;

        public static void GlobalTextureAtlasGetFrameSetPrefix(Pawn pawn) => 
            createPawnAtlasPawn = pawn;

        public static IEnumerable<CodeInstruction> PawnTextureAtlasGetFrameSetTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            bool done = false;

            Label jumpLabel = ilg.DefineLabel();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!done && i > 2 && instructionList[i-1].opcode == OpCodes.Ret)
                {
                    done = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0) {labels = instruction.ExtractLabels()};
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.LoadField(typeof(PawnTextureAtlas), "freeFrameSets");
                    yield return CodeInstruction.Call(patchType, nameof(TextureAtlasSameRace));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, jumpLabel);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ret);
                    instruction = instruction.WithLabels(jumpLabel);
                }

                yield return instruction;
            }
        }

        public static bool TextureAtlasSameRace(PawnTextureAtlas atlas, Pawn pawn, List<PawnTextureAtlasFrameSet> frameSets)
        {
            Dictionary<Pawn, PawnTextureAtlasFrameSet>.KeyCollection keys = CachedData.pawnTextureAtlasFrameAssignments(atlas).Keys;

            int atlasScale = (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.atlasScale ?? 1;
            float borderScale = (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.borderScale ?? 1;

            if (keys.Count == 0)
            {
                if (atlas.RawTexture.width == 2048 * atlasScale && Math.Abs(frameSets.First().meshes.First().vertices.First().x + borderScale) < 0.01f)
                    return true;
            }
            else if (keys.Any(p => p.def == pawn.def || (((p.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.atlasScale ?? 1)     == atlasScale) && 
                                   (Math.Abs(((p.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.borderScale ?? 1) - borderScale) < 0.01)))
            {
                return true;
            }

            return false;
        }
        
        public static float GetBorderSizeForPawn() =>
            (createPawnAtlasPawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.borderScale ?? 1f;

        public static IEnumerable<CodeInstruction> PawnTextureAtlasConstructorFuncTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4)
                {
                    yield return CodeInstruction.Call(patchType, nameof(GetBorderSizeForPawn));
                }
                else
                    yield return instruction;
            }
        }

        public static int GetAtlasSizeForPawn() => 
            (createPawnAtlasPawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.atlasScale ?? 1;

        public static IEnumerable<CodeInstruction> PawnTextureAtlasConstructorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.OperandIs(2048) || instruction.OperandIs(2048f))
                {
                    if (instruction.opcode == OpCodes.Ldc_I4)
                    {
                        yield return CodeInstruction.Call(patchType, nameof(GetAtlasSizeForPawn));
                        yield return new CodeInstruction(OpCodes.Mul);
                    }
                    else if (instruction.opcode == OpCodes.Ldc_R4)
                    {
                        yield return CodeInstruction.Call(patchType, nameof(GetAtlasSizeForPawn));
                        yield return new CodeInstruction(OpCodes.Conv_R4);
                        yield return new CodeInstruction(OpCodes.Mul);
                    }
                } else if (instruction.OperandIs(128))
                {
                    yield return CodeInstruction.Call(patchType, nameof(GetAtlasSizeForPawn));
                    yield return new CodeInstruction(OpCodes.Mul);
                }
            }
        }

        public static void CalcAnchorDataPostfix(Pawn pawn, BodyTypeDef.WoundAnchor anchor, ref Vector3 anchorOffset)
        {
            if (pawn.def is ThingDef_AlienRace alienRace)
            {
                List<AlienPartGenerator.WoundAnchorReplacement> anchorReplacements = alienRace.alienRace.generalSettings.alienPartGenerator.anchorReplacements;

                foreach (AlienPartGenerator.WoundAnchorReplacement anchorReplacement in anchorReplacements)
                {
                    if (anchor == anchorReplacement.replacement && anchorReplacement.offsets != null)
                    {
                        anchorOffset = anchorReplacement.offsets.GetOffset(anchor.rotation!.Value).GetOffset(false, pawn.story.bodyType, pawn.story.headType);
                        return;
                    }
                }
            }
        }

        public static IEnumerable<BodyTypeDef.WoundAnchor> FindAnchorsPostfix(IEnumerable<BodyTypeDef.WoundAnchor> __result, Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienRace)
            {
                List<AlienPartGenerator.WoundAnchorReplacement> anchorReplacements = alienRace.alienRace.generalSettings.alienPartGenerator.anchorReplacements;
                List<BodyTypeDef.WoundAnchor> result = [];

                if (!__result.Any()) 
                    return [];

                foreach (BodyTypeDef.WoundAnchor anchor in __result)
                {
                    AlienPartGenerator.WoundAnchorReplacement replacement = anchorReplacements.FirstOrDefault(war => war.ValidReplacement(anchor));
                    result.Add(replacement != null ? replacement.replacement : anchor);
                }

                return result;
            }

            return __result;
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
                else if (instruction.opcode == OpCodes.Ldarg_2)
                    yield = true;
            }
        }

        public static IEnumerable<CodeInstruction> PostureTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo postureInfo = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetPosture));

            List<CodeInstruction> instructionList = instructions.ToList();
            foreach (CodeInstruction instruction in instructionList)
            {
                bool found = instruction.Calls(postureInfo);

                if (found) 
                    yield return new CodeInstruction(OpCodes.Dup);

                yield return instruction;

                if (found) 
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(PostureTweak)));
            }
        }

        public static PawnPosture PostureTweak(Pawn pawn, PawnPosture posture)
        {
            if (posture != PawnPosture.Standing && pawn.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.generalSettings.canLayDown && 
                !(pawn.CurrentBed()?.def.defName.EqualsIgnoreCase(B: "ET_Bed") ?? false)) //todo: how damn specific is this.. a race that can't lay down.. but isn't laying in their own specific bed..........
                return PawnPosture.Standing;
            return posture;
        }

        public static IEnumerable<CodeInstruction> BodyReferenceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo bodyInfo  = AccessTools.Field(typeof(RaceProperties), nameof(RaceProperties.body));
            FieldInfo propsInfo = AccessTools.Field(typeof(ThingDef),       nameof(ThingDef.race));
            FieldInfo defInfo = AccessTools.Field(typeof(Thing), nameof(Thing.def));
            MethodInfo raceprops = AccessTools.Property(typeof(Pawn), nameof(Pawn.RaceProps)).GetGetMethod();
            {
                List<CodeInstruction> instructionList = instructions.ToList();
                for (int i = 0; i < instructionList.Count; i++)
                {
                    CodeInstruction instruction = instructionList[i];

                    if (i < instructionList.Count - 2 && instructionList[i + 2].OperandIs(bodyInfo) && instructionList[i + 1].OperandIs(propsInfo) && instruction.OperandIs(defInfo))
                    {
                        instruction.opcode  =  OpCodes.Call;
                        instruction.operand =  AccessTools.Method(patchType, nameof(ReplacedBody));
                        i                   += 2;
                    }

                    if (i < instructionList.Count - 1 && instructionList[i + 1].OperandIs(bodyInfo) && instruction.OperandIs(defInfo))
                    {
                        instruction.opcode  = OpCodes.Call;
                        instruction.operand = AccessTools.Method(patchType, nameof(ReplacedBody));
                        i++;
                    }

                    if (i < instructionList.Count - 1 && instructionList[i + 1].OperandIs(bodyInfo) && instruction.OperandIs(raceprops))
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = AccessTools.Method(patchType, nameof(ReplacedBody));
                        i++;
                    }

                    yield return instruction;
                }
            }
        }

        public static BodyDef ReplacedBody(Pawn pawn) => 
            pawn.def is ThingDef_AlienRace ? (pawn.ageTracker?.CurLifeStageRace as LifeStageAgeAlien)?.body ?? pawn.RaceProps.body : pawn.RaceProps.body;

        public static bool ChangeKindPrefix(Pawn __instance, PawnKindDef newKindDef) => 
            !__instance.RaceProps.Humanlike || !newKindDef.RaceProps.Humanlike || 
            __instance.kindDef == PawnKindDefOf.WildMan || newKindDef == PawnKindDefOf.WildMan;

        public static void GenerateGearForPostfix(Pawn pawn) =>
            pawn.story?.AllBackstories?.OfType<AlienBackstoryDef>()
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

        public static void AddHediffPostfix(Pawn ___pawn, Hediff hediff)
        {
            if(!hediff.def.hairColorOverride.HasValue && !hediff.def.HasDefinedGraphicProperties)
                ___pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public static void RemoveHediffPostfix(Pawn ___pawn, Hediff hediff)
        {
            if (!hediff.def.HasDefinedGraphicProperties && !hediff.def.forceRenderTreeRecache)
                ___pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public static void HediffChangedPostfix(Pawn ___pawn, HediffSet ___hediffSet)
        {
            if (Current.ProgramState == ProgramState.Playing && ___pawn.Spawned && ___pawn.def is ThingDef_AlienRace && (AlienPartGenerator.racesWithSeverity?.Contains(___pawn.def) ?? true) && (!___hediffSet.HasRegeneration || ___pawn.IsHashIntervalTick(300)))
                ___pawn.Drawer.renderer.SetAllGraphicsDirty();
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

        public static Vector2 BaseHeadOffsetAtHelper(Vector2 offset, Pawn pawn)
        {
            LifeStageAgeAlien ageAlien = (pawn.ageTracker.CurLifeStageRace as LifeStageAgeAlien);

            Vector2 alienHeadOffset = (ageAlien == null ? Vector2.zero : pawn.gender switch
                                                   {
                                                       Gender.Female => ageAlien.headFemaleOffset,
                                                       _ => ageAlien.headOffset
                                                   });

            //Log.Message($"{ageAlien?.def.defName}: {alienHeadOffset.ToStringTwoDigits()}");

            return offset + alienHeadOffset;
        }

        public static void BaseHeadOffsetAtPostfix(ref Vector3 __result, Rot4 rotation, Pawn ___pawn)
        {
            LifeStageAgeAlien stageAgeAlien = (___pawn.ageTracker.CurLifeStageRace as LifeStageAgeAlien);
            Vector3 offsetSpecific = (___pawn.gender == Gender.Female ? 
                                          stageAgeAlien?.headFemaleOffsetDirectional : 
                                          stageAgeAlien?.headOffsetDirectional)?.GetOffset(rotation)?.GetOffset(false, ___pawn.story.bodyType, ___pawn.story.headType) ?? Vector3.zero;
            __result += offsetSpecific;
        }

        public static void CanInteractWithAnimalPostfix(ref bool __result, Pawn pawn, Pawn animal) =>
            __result = __result && RaceRestrictionSettings.CanTame(animal.def, pawn.def);

        public static void CanDesignateThingTamePostfix(Designator __instance, ref AcceptanceReport __result, Thing t)
        {
            if (!__result.Accepted || __instance is not Designator_Build) 
                return;

            __result = colonistRaces.Any(predicate: td => RaceRestrictionSettings.CanTame(t.def, td));
        }

        public static IEnumerable<CodeInstruction> RecalculateLifeStageIndexTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo biotechInfo = AccessTools.PropertyGetter(typeof(ModsConfig), nameof(ModsConfig.BiotechActive));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(biotechInfo))
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                else
                    yield return instruction;
            }
        }

        public static void HasHeadPrefix(HediffSet __instance) =>
            headPawnDef = (__instance.pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.headBodyPartDef;

        private static BodyPartDef headPawnDef;

        public static void HasHeadPostfix(BodyPartRecord x, ref bool __result) =>
            __result = headPawnDef != null ? x.def == headPawnDef : __result;

        public static void GenerateInitialHediffsPostfix(Pawn pawn)
        {
            foreach (HediffDef hd in pawn.story?.AllBackstories?.OfType<AlienBackstoryDef>().SelectMany(selector: bd => bd.forcedHediffs).Concat(bioReference?.forcedHediffs ?? new List<HediffDef>(capacity: 0)) ?? Array.Empty<HediffDef>())
            {
                BodyPartRecord bodyPartRecord = null;
                DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(predicate: rd => rd.addsHediff == hd)?.
                                       appliedOnFixedBodyParts.SelectMany(bpd => pawn.health.hediffSet.GetNotMissingParts().Where(bpr =>
                                        bpr.def == bpd && !pawn.health.hediffSet.hediffs.Any(predicate: h => h.def == hd && h.Part == bpr))).
                                            TryRandomElement(out bodyPartRecord);
                pawn.health.AddHediff(hd, bodyPartRecord);
            }
        }

        public static void GenerateStartingApparelForPostfix() =>
            CachedData.allApparelPairs().AddRange(apparelList);

        private static HashSet<ThingStuffPair> apparelList;

        public static void GenerateStartingApparelForPrefix(Pawn pawn)
        {
            apparelList = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in CachedData.allApparelPairs().ListFullCopy())
            {
                ThingDef equipment = pair.thing;
                if (!RaceRestrictionSettings.CanWear(equipment, pawn.def))
                    apparelList.Add(pair);
            }

            CachedData.allApparelPairs().RemoveAll(match: tsp => apparelList.Contains(tsp));
        }

        public static void TryGenerateWeaponForPostfix() =>
            CachedData.allWeaponPairs().AddRange(weaponList);

        private static HashSet<ThingStuffPair> weaponList;

        public static void TryGenerateWeaponForPrefix(Pawn pawn)
        {
            weaponList = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in CachedData.allWeaponPairs().ListFullCopy())
            {
                ThingDef equipment = pair.thing;
                if (!RaceRestrictionSettings.CanEquip(equipment, pawn.def))
                    weaponList.Add(pair);
            }
            CachedData.allWeaponPairs().RemoveAll(match: tsp => float.IsNaN(tsp.commonalityMultiplier) ? weaponList.Any(pair => pair.thing == tsp.thing && pair.stuff == tsp.stuff && float.IsNaN(pair.commonalityMultiplier)) : weaponList.Contains(tsp));
        }

        public static void DamageInfosToApplyPostfix(Verb __instance, ref IEnumerable<DamageInfo> __result)
        {
            if (__instance.CasterIsPawn && __instance.CasterPawn.def is ThingDef_AlienRace alienProps && __instance.CasterPawn.CurJob.def == JobDefOf.SocialFight)
                __result = __result.Select(selector: di =>
                    new DamageInfo(di.Def, Math.Min(di.Amount, alienProps.alienRace.generalSettings.maxDamageForSocialfight), angle: di.Angle, instigator: di.Instigator,
                        hitPart: di.HitPart, weapon: di.Weapon, category: di.Category));
        }

        public static void CanEverEatPostfix(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (!__instance.Humanlike || !__result) return;
            __result = RaceRestrictionSettings.CanEat(t, CachedData.GetRaceFromRaceProps(__instance));
        }
        
        public static IEnumerable<Rule> RulesForPawnPostfix(IEnumerable<Rule> __result, Pawn pawn, string pawnSymbol) =>
            __result.AddItem(new Rule_String(pawnSymbol + "_alienRace", pawn.def.LabelCap));

        public static IEnumerable<CodeInstruction> GenerateTraitsForTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList      = instructions.ToList();
            MethodInfo            defListInfo          = AccessTools.Property(typeof(DefDatabase<TraitDef>), nameof(DefDatabase<TraitDef>.AllDefsListForReading)).GetGetMethod();
            FieldInfo             growthMomentAgesInfo = AccessTools.Field(typeof(GrowthUtility), nameof(GrowthUtility.GrowthMomentAges));


            foreach (CodeInstruction instruction in instructionList)
            {
                if (instruction.LoadsField(growthMomentAgesInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.def)));
                    yield return CodeInstruction.Call(patchType, nameof(GrowthMomentHelper), new []{typeof(ThingDef)});
                } else
                {
                    yield return instruction;
                }

                if (instruction.opcode == OpCodes.Call && instruction.OperandIs(defListInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(patchType, nameof(GenerateTraitsValidator));
                }
            }
        }

        public static IEnumerable<CodeInstruction> GenerateTraitsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            foreach (CodeInstruction instruction in instructionList)
            {
                
                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    //yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(AdditionalInitialTraits)));
                }

                yield return instruction;
            }
        }

        public static IEnumerable<TraitDef> GenerateTraitsValidator(List<TraitDef> traits, Pawn p) => 
            traits.Where(tr => RaceRestrictionSettings.CanGetTrait(tr, p));

        public static void AssigningCandidatesPostfix(ref IEnumerable<Pawn> __result, CompAssignableToPawn __instance) =>
            __result = __instance.parent.def.building.bed_humanlike ? __result.Where(predicate: p => RestUtility.CanUseBedEver(p, __instance.parent.def)) : __result;

        public static void CanUseBedEverPostfix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            if (__result)
            {
                __result = p.def is not ThingDef_AlienRace alienProps ||
                           ((alienProps.alienRace.generalSettings.validBeds?.Contains(bedDef) ?? false) ||
                            (alienProps.alienRace.generalSettings.validBeds.NullOrEmpty() &&
                             !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(predicate: td => td.alienRace.generalSettings.validBeds?.Contains(bedDef) ?? false)));
            }
        }

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
            p.def is ThingDef_AlienRace && DefDatabase<RaceSettings>.AllDefs.Any(rs => rs.pawnKindSettings.alienslavekinds.Any(predicate: pke => pke.kindDefs.Contains(p.kindDef)));

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

            if (!pawn.RaceProps.Humanlike || pawn.RaceProps.hasGenders || pawn.def is not ThingDef_AlienRace) return true;

            List<KeyValuePair<Pawn, PawnRelationDef>> list                  = new();
            List<PawnRelationDef>                     allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
            List<Pawn> enumerable = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                                     where x.def == pawn.def
                                     select x).ToList();

            enumerable.ForEach(action: current =>
            {
                if (current.Discarded)
                    Log.Warning(string.Concat("Warning during generating pawn relations for ", pawn, ": Pawn ", current, " is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything."));
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
                generationChance = 0.2f;
                GenerationChanceExLoverPostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.ExSpouse)
            {
                generationChance = 0.2f;
                GenerationChanceExSpousePostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Fiance)
            {
                generationChance =
                    Mathf.Clamp(GenMath.LerpDouble(lifeExpectancy / 1.6f, lifeExpectancy, outFrom: 1f, outTo: 0.01f, pawn.ageTracker.AgeBiologicalYearsFloat), min: 0.01f,
                        max: 1f) *
                    Mathf.Clamp(GenMath.LerpDouble(lifeExpectancy / 1.6f, lifeExpectancy, outFrom: 1f, outTo: 0.01f, current.ageTracker.AgeBiologicalYearsFloat), min: 0.01f,
                        max: 1f);

                if (LovePartnerRelationUtility.HasAnyLovePartner(pawn) || LovePartnerRelationUtility.HasAnyLovePartner(current))
                    generationChance = 0;
                GenerationChanceFiancePostfix(ref generationChance, pawn, current);
            }
            else if (relationDef == PawnRelationDefOf.Lover)
            {
                generationChance = 0.5f;

                if (LovePartnerRelationUtility.HasAnyLovePartner(pawn) || LovePartnerRelationUtility.HasAnyLovePartner(current))
                    generationChance = 0;
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

                if (LovePartnerRelationUtility.HasAnyLovePartner(pawn) || LovePartnerRelationUtility.HasAnyLovePartner(current))
                    generationChance = 0;
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

            float num = childRelation.Method(name: "GetParentAgeFactor", parent1, child, parent1.RaceProps.lifeExpectancy / 5f, parent1.RaceProps.lifeExpectancy / 2.5f, parent1.RaceProps.lifeExpectancy / 1.6f).GetValue<float>();
            if (Math.Abs(num) < 0.001f) 
                return 0f;
            if (parent2 != null)
            {
                num2 = childRelation.Method(name: "GetParentAgeFactor", parent2, child, parent1.RaceProps.lifeExpectancy / 5f, parent1.RaceProps.lifeExpectancy / 2.5f, parent1.RaceProps.lifeExpectancy / 1.6f).GetValue<float>();
                if (Math.Abs(num2) < 0.001f) 
                    return 0f;
                num3 = 1f;
            }

            float num6                                                                      = 1f;
            Pawn  firstDirectRelationPawn                                                   = parent2?.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
            if (firstDirectRelationPawn != null && firstDirectRelationPawn != parent2) 
                num6 *= 0.15f;
            if (parent2 == null) 
                return num * num2 * num3 * num6;
            Pawn firstDirectRelationPawn2                                                     = parent2.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
            if (firstDirectRelationPawn2 != null && firstDirectRelationPawn2 != parent2) 
                num6 *= 0.15f;
            return num * num2 * num3 * num6;

        }

        public static bool GainTraitPrefix(Trait trait, Pawn ___pawn) => 
            RaceRestrictionSettings.CanGetTrait(trait.def, ___pawn, trait.Degree);

        public static void TryMakeInitialRelationsWithPostfix(Faction __instance, Faction other)
        {
            static ThingDef_AlienRace GetRaceOfFaction(FactionDef fac) =>
                (fac.basicMemberKind?.race ?? fac.pawnGroupMakers?.SelectMany(pgm => pgm.options).GroupBy(pgm => pgm.kind.race).OrderByDescending(g => g.Count()).First().Key) as ThingDef_AlienRace;

            ThingDef_AlienRace alienRace = GetRaceOfFaction(other.def);
            if(alienRace != null)
                foreach (FactionRelationSettings frs in alienRace.alienRace.generalSettings.factionRelations)
                {
                    if (!frs.factions.Contains(__instance.def)) continue;

                    int offset = frs.goodwill.RandomInRange;

                    FactionRelationKind kind = offset > 75 ?
                                                   FactionRelationKind.Ally :
                                                   offset <= -10 ?
                                                       FactionRelationKind.Hostile :
                                                       FactionRelationKind.Neutral;

                    FactionRelation relation = other.RelationWith(__instance);
                    relation.baseGoodwill = offset;
                    relation.kind         = kind;

                    relation              = __instance.RelationWith(other);
                    relation.baseGoodwill = offset;
                    relation.kind         = kind;
                }

            alienRace = GetRaceOfFaction(__instance.def);
            if(alienRace != null)
                foreach (FactionRelationSettings frs in alienRace.alienRace.generalSettings.factionRelations)
                {
                    if (!frs.factions.Contains(other.def)) continue;
                    int offset = frs.goodwill.RandomInRange;

                    FactionRelationKind kind = offset > 75 ?
                                                   FactionRelationKind.Ally :
                                                   offset <= -10 ?
                                                       FactionRelationKind.Hostile :
                                                       FactionRelationKind.Neutral;

                    FactionRelation relation = other.RelationWith(__instance);
                    relation.baseGoodwill = offset;
                    relation.kind         = kind;

                    relation              = __instance.RelationWith(other);
                    relation.baseGoodwill = offset;
                    relation.kind         = kind;
                }
        }

        public static bool TryCreateThoughtPrefix(ref ThoughtDef def, SituationalThoughtHandler __instance, ref List<Thought_Situational> ___cachedThoughts)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is ThingDef_AlienRace race)
                def = race.alienRace.thoughtSettings.ReplaceIfApplicable(def);
            ThoughtDef  thoughtDef = def;
            return !___cachedThoughts.Any(th => th.def == thoughtDef);
        }

        public static void CanBingeNowPostfix(Pawn pawn, ChemicalDef chemical, ref bool __result)
        {
            if (!__result) 
                return;
            if (pawn.def is not ThingDef_AlienRace alienProps) 
                return;

            bool result = true;
            alienProps.alienRace.generalSettings.chemicalSettings?.ForEach(action: cs =>
                                                                                   {
                                                                                       if (cs.chemical == chemical && !cs.ingestible)
                                                                                           result = false;
                                                                                   }
                                                                          );
            __result = result;
        }

        public static IEnumerable<CodeInstruction> IngestedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo postIngestedInfo = AccessTools.Method(typeof(Thing), "PostIngested");

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(postIngestedInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(patchType, nameof(ingestedCount)));
                }

                yield return instruction;
            }
        }

        public static int ingestedCount;

        public static void DrugPostIngestedPostfix(Pawn ingester, CompDrug __instance)
        {
            if (ingester.def is ThingDef_AlienRace alienProps)
                alienProps.alienRace.generalSettings.chemicalSettings?.ForEach(action: cs =>
                {
                    if (cs.chemical == __instance.Props?.chemical)
                        cs.reactions?.ForEach(action: iod => iod.DoIngestionOutcome(ingester, __instance.parent, ingestedCount));
                });
        }

        public static void DrugValidatorPostfix(ref bool __result, Pawn pawn, Thing drug) =>
            CanBingeNowPostfix(pawn, drug?.TryGetComp<CompDrug>()?.Props?.chemical, ref __result);

        // ReSharper disable once RedundantAssignment
        public static void CompatibilityWithPostfix(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result, Pawn ___pawn)
        {
            if (___pawn.RaceProps.Humanlike != otherPawn.RaceProps.Humanlike || ___pawn == otherPawn)
            {
                __result = 0f;
                return;
            }

            float x   = Mathf.Abs(___pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
            float num = GenMath.LerpDouble(inFrom: 0f, inTo: 20f, outFrom: 0.45f, outTo: -0.45f, x);
            num = Mathf.Clamp(num, min: -0.45f, max: 0.45f);
            float num2 = __instance.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);
            __result = num + num2;
        }

        public static void SecondaryLovinChanceFactorPostfix(Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            
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
                                                 wgd == __instance.def) ?? false));
        }

        public static void GenericJobOnThingPostfix(WorkGiver __instance, Pawn pawn, ref Job __result)
        {
            if (__result == null) return;
            // ReSharper disable once ImplicitlyCapturedClosure
            if (!(((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd => wgd.giverClass == __instance.GetType()) ?? false) ||
                  !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(predicate: d => pawn.def != d && 
                                                                            (d.alienRace.raceRestriction.workGiverList?.Any(predicate: wgd => wgd == __instance.def) ?? false))))
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
                __result = -1001f;
        }

        public static void PrepForMapGenPrefix(GameInitData __instance)
        {
            foreach (ScenPart part in Find.Scenario.AllParts)
                if (part is ScenPart_StartingHumanlikes sp1)
                {
                    IEnumerable<Pawn> sp  = sp1.GetPawns();
                    Pawn[] spa = sp as Pawn[] ?? sp.ToArray();
                    __instance.startingAndOptionalPawns.InsertRange(__instance.startingPawnCount, spa);
                    __instance.startingPawnCount += spa.Length;

                    foreach (Pawn pawn in spa)
                        CachedData.generateStartingPossessions(pawn);
                }
        }

        public static bool TryGainMemoryPrefix(ref Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            Pawn   pawn        = __instance.pawn;

            if (pawn.def is not ThingDef_AlienRace race) return true;

            ThoughtDef newThoughtDef = race.alienRace.thoughtSettings.ReplaceIfApplicable(newThought.def);

            if (newThoughtDef == newThought.def) return true;

            Thought_Memory replacedThought = ThoughtMaker.MakeThought(newThoughtDef, newThought.CurStageIndex);
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

        private static HashSet<ThingDef> colonistRaces = new();
        private static int               colonistRacesTick;
        private const  int               COLONIST_RACES_TICK_TIMER = GenDate.TicksPerHour * 2;

        public static void UpdateColonistRaces()
        {
            if (Find.TickManager.TicksAbs > colonistRacesTick + COLONIST_RACES_TICK_TIMER || Find.TickManager.TicksAbs < colonistRacesTick)
            {
                List<Pawn> pawns = PawnsFinder.AllMaps_FreeColonistsSpawned;

                if (pawns.Count > 0)
                {
                    HashSet<ThingDef> newColonistRaces = new(pawns.Select(selector: p => p.def));
                    colonistRacesTick = Find.TickManager.TicksAbs;
                    //Log.Message(string.Join(" | ", colonistRaces.Select(td => td.defName)));

                    if (newColonistRaces.Count != colonistRaces.Count || newColonistRaces.Any(td => !colonistRaces.Contains(td)))
                    {
                        RaceRestrictionSettings.buildingsRestrictedWithCurrentColony.Clear();
                        HashSet<ThingDef> buildingsRestrictedTemp = new(RaceRestrictionSettings.buildingRestricted);
                        buildingsRestrictedTemp.AddRange(newColonistRaces.Where(td => td is ThingDef_AlienRace).SelectMany(td => (td as ThingDef_AlienRace)!.alienRace.raceRestriction.blackBuildingList));
                        foreach (ThingDef td in buildingsRestrictedTemp)
                        {
                            bool canBuild = false;
                            foreach (ThingDef race in newColonistRaces)
                                if (RaceRestrictionSettings.CanBuild(td, race))
                                    canBuild = true;
                            if(!canBuild)
                                RaceRestrictionSettings.buildingsRestrictedWithCurrentColony.Add(td);
                        }
                        colonistRaces = newColonistRaces;
                    }
                }
                else
                {
                    colonistRaces.Clear();
                    colonistRacesTick = Find.TickManager.TicksAbs - COLONIST_RACES_TICK_TIMER + GenTicks.TicksPerRealSecond;
                    RaceRestrictionSettings.buildingsRestrictedWithCurrentColony.Clear();
                    RaceRestrictionSettings.buildingsRestrictedWithCurrentColony.AddRange(RaceRestrictionSettings.buildingRestricted);
                }
            }
        }

        public static IEnumerable<CodeInstruction> DesignatorAllowedTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            Label gotoReturn = ilg.DefineLabel();

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Dup).MoveLabelsFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Brfalse, gotoReturn);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Isinst,  typeof(Designator_Build));
                    yield return new CodeInstruction(OpCodes.Brfalse, gotoReturn);
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(Designator_Build));
                    yield return CodeInstruction.Call(patchType, nameof(DesignatorAllowedHelper));
                    instruction.labels.Add(gotoReturn);
                }

                yield return instruction;
            }
        }

        public static bool DesignatorAllowedHelper(Designator_Build d)
        {
            UpdateColonistRaces();
            return RaceRestrictionSettings.CanColonyBuild(d.PlacingDef);
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
            ResearchProjectDef project = Find.ResearchManager.GetProject();

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

        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<FoodUtility.ThoughtFromIngesting> __result)
        {
            try
            {
                if (ingester.def is not ThingDef_AlienRace alienProps) return;

                if (ingester.story.traits.HasTrait(AlienDefOf.HAR_Xenophobia) && ingester.story.traits.DegreeOfTrait(AlienDefOf.HAR_Xenophobia) == 1)
                    if (__result.Any(tfi => tfi.thought       == AlienDefOf.AteHumanlikeMeatDirect) && foodDef.ingestible?.sourceDef != ingester.def)
                        __result.RemoveAll(tfi => tfi.thought == AlienDefOf.AteHumanlikeMeatDirect);
                    else if (__result.Any(tfi => tfi.thought == AlienDefOf.AteHumanlikeMeatAsIngredient) &&
                             (foodSource?.TryGetComp<CompIngredients>()?.ingredients
                                      ?.Any(predicate: td => FoodUtility.GetMeatSourceCategory(td) == MeatSourceCategory.Humanlike && td.ingestible?.sourceDef != ingester.def) ?? false))
                        __result.RemoveAll(tfi => tfi.thought == AlienDefOf.AteHumanlikeMeatAsIngredient);

                bool cannibal = ingester.story.traits.HasTrait(AlienDefOf.Cannibal);

                List<FoodUtility.ThoughtFromIngesting> resultingThoughts = [];
                for (int i = 0; i < __result.Count; i++)
                {
                    ThoughtDef      thoughtDef = __result[i].thought;
                    ThoughtSettings settings   = alienProps.alienRace.thoughtSettings;

                    thoughtDef = settings.ReplaceIfApplicable(thoughtDef);

                    if (thoughtDef == AlienDefOf.AteHumanlikeMeatDirect || thoughtDef == AlienDefOf.AteHumanlikeMeatDirectCannibal)
                        thoughtDef = settings.GetAteThought(foodDef.ingestible?.sourceDef, cannibal, ingredient: false);

                    if (thoughtDef == AlienDefOf.AteHumanlikeMeatAsIngredient || thoughtDef == AlienDefOf.AteHumanlikeMeatAsIngredientCannibal)
                    {
                        ThingDef race = foodSource?.TryGetComp<CompIngredients>()?.ingredients?.FirstOrDefault(predicate: td => td.ingestible?.sourceDef?.race?.Humanlike ?? false)?.ingestible
                                                ?.sourceDef;
                        if (race != null)
                            thoughtDef = settings.GetAteThought(race, cannibal, ingredient: true);
                    }

                    resultingThoughts.Add(new FoodUtility.ThoughtFromIngesting { fromPrecept = __result[i].fromPrecept, thought = thoughtDef });
                }

                __result = resultingThoughts;

                if (foodSource != null && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(foodSource))
                {
                    bool alienMeat = false;

                    CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
                    if (compIngredients?.ingredients != null)
                        foreach (ThingDef ingredient in compIngredients.ingredients)
                            if (ingredient.IsMeat && ingredient.ingestible.sourceDef != ingester.def)
                                alienMeat = true;


                    CachedData.ingestThoughts().Clear();
                    CachedData.foodUtilityAddThoughtsFromIdeo(alienMeat ? AlienDefOf.HAR_AteAlienMeat : AlienDefOf.HAR_AteNonAlienFood, ingester, foodDef,
                                                              alienMeat ? MeatSourceCategory.Humanlike : MeatSourceCategory.NotMeat);
                    resultingThoughts.AddRange(CachedData.ingestThoughts());
                }

                __result = resultingThoughts;

                
            }
            catch (Exception ex)
            {
                Log.Error($"AlienRace encountered an error processing food\nPawn: {ingester?.Name} | {ingester?.def.defName}\nFood: {foodDef?.defName} | {foodSource?.def.defName} | {foodDef?.modContentPack?.Name}\n{ex}");
            }
        }

        public static void GenerationChanceSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race) 
                __result *= race.alienRace.relationSettings.relationChanceModifierSpouse;

            if (other.def is ThingDef_AlienRace alienRace) 
                __result *= alienRace.alienRace.relationSettings.relationChanceModifierSpouse;

            __result *= (generated.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierSpouse ?? 1;
            __result *= (generated.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierSpouse ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierSpouse     ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierSpouse     ?? 1;

            if (generated == other)
                __result = 0;
        }

        public static void GenerationChanceSiblingPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race)
                __result *= race.alienRace.relationSettings.relationChanceModifierSibling;

            if (other.def is ThingDef_AlienRace alienRace)
                __result *= alienRace.alienRace.relationSettings.relationChanceModifierSibling;

            __result *= (generated.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierSibling ?? 1;
            __result *= (generated.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierSibling ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierSibling     ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierSibling     ?? 1;

            if (generated == other)
                __result = 0;
        }

        public static void GenerationChanceParentPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race)
                __result *= race.alienRace.relationSettings.relationChanceModifierParent;

            if (other.def is ThingDef_AlienRace alienRace)
                __result *= alienRace.alienRace.relationSettings.relationChanceModifierParent;

            __result *= (generated.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierParent ?? 1;
            __result *= (generated.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierParent ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierParent     ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierParent     ?? 1;

            if (generated == other)
                __result = 0;
        }

        public static void GenerationChanceLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race)
                __result *= race.alienRace.relationSettings.relationChanceModifierLover;

            if (other.def is ThingDef_AlienRace alienRace)
                __result *= alienRace.alienRace.relationSettings.relationChanceModifierLover;

            __result *= (generated.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierLover ?? 1;
            __result *= (generated.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierLover ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierLover     ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierLover     ?? 1;

            if (generated == other)
                __result = 0;
        }

        public static void GenerationChanceFiancePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race)
                __result *= race.alienRace.relationSettings.relationChanceModifierFiance;

            if (other.def is ThingDef_AlienRace alienRace)
                __result *= alienRace.alienRace.relationSettings.relationChanceModifierFiance;

            __result *= (generated.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierFiance ?? 1;
            __result *= (generated.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierFiance ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierFiance     ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierFiance     ?? 1;

            if (generated == other)
                __result = 0;
        }

        public static void GenerationChanceExSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race)
                __result *= race.alienRace.relationSettings.relationChanceModifierExSpouse;

            if (other.def is ThingDef_AlienRace alienRace)
                __result *= alienRace.alienRace.relationSettings.relationChanceModifierExSpouse;

            __result *= (generated.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierExSpouse ?? 1;
            __result *= (generated.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierExSpouse ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierExSpouse     ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierExSpouse     ?? 1;

            if (generated == other)
                __result = 0;
        }

        public static void GenerationChanceExLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race)
                __result *= race.alienRace.relationSettings.relationChanceModifierExLover;

            if (other.def is ThingDef_AlienRace alienRace)
                __result *= alienRace.alienRace.relationSettings.relationChanceModifierExLover;

            __result *= (generated.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierExLover ?? 1;
            __result *= (generated.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierExLover ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierExLover     ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierExLover     ?? 1;

            if (generated == other)
                __result = 0;
        }

        public static void GenerationChanceChildPostfix(ref float __result, Pawn generated, Pawn other)
        {
            if (generated.def is ThingDef_AlienRace race)
                __result *= race.alienRace.relationSettings.relationChanceModifierChild;

            if (other.def is ThingDef_AlienRace alienRace)
                __result *= alienRace.alienRace.relationSettings.relationChanceModifierChild;

            __result *= (generated.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierChild ?? 1;
            __result *= (generated.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierChild ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Childhood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierChild     ?? 1;
            __result *= (other.story.GetBackstory(BackstorySlot.Adulthood) as AlienBackstoryDef)?.relationSettings.relationChanceModifierChild     ?? 1;

            if (generated == other)
                __result = 0;
        }

        public static void BirthdayBiologicalPrefix(Pawn ___pawn)
        {
            if (___pawn.def is not ThingDef_AlienRace) 
                return;

            if (!___pawn.def.race.lifeStageAges.Skip(count: 1).Any() || ___pawn.ageTracker.CurLifeStageIndex == 0) 
                return;
            LifeStageAge lsac = ___pawn.ageTracker.CurLifeStageRace;
            LifeStageAge lsap = ___pawn.def.race.lifeStageAges[___pawn.ageTracker.CurLifeStageIndex - 1];

            if (lsac is LifeStageAgeAlien lsaac && lsaac.body != null && ((lsap as LifeStageAgeAlien)?.body ?? ___pawn.RaceProps.body) != lsaac.body ||
                lsap is LifeStageAgeAlien lsaap && lsaap.body != null && ((lsac as LifeStageAgeAlien)?.body ?? ___pawn.RaceProps.body) != lsaap.body)
            {
                ___pawn.health.hediffSet = new HediffSet(___pawn);
            }
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
                }
            }
        }

        public static void CanGetThoughtPostfix(ref bool __result, ThoughtDef def, Pawn pawn)
        {
            if (!__result) 
                return;

            __result = ThoughtSettings.CanGetThought(def, pawn);
        }

        public static IEnumerable<CodeInstruction> CanDoNextStartPawnTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            MethodInfo getNameInfo = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Name));

            LocalBuilder pawnLocal = ilg.DeclareLocal(typeof(Pawn));

            List<CodeInstruction> instructionList = instructions.ToList();
            for (int index = 0; index < instructionList.Count; index++)
            {
                CodeInstruction instruction = instructionList[index];
                if (instruction.Calls(getNameInfo))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stloc, pawnLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn),     nameof(Pawn.def)));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef), nameof(ThingDef.race)));
                    yield return new CodeInstruction(OpCodes.Ldloc, pawnLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.gender)));
                    yield return CodeInstruction.Call(typeof(RaceProperties), nameof(RaceProperties.GetNameGenerator));
                    yield return new CodeInstruction(instructionList[index + 2]);
                    yield return new CodeInstruction(OpCodes.Ldloc, pawnLocal.LocalIndex);
                }

                yield return instruction;
            }
        }

        public static IEnumerable<CodeInstruction> DrawCharacterCardTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo originalMethod = AccessTools.Method(typeof(Widgets), nameof(Widgets.ButtonText), [typeof(Rect), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(TextAnchor?)]);
            MethodInfo myPassthroughMethod = AccessTools.Method(patchType, nameof(PawnKindRandomizeButtonPassthrough));

            bool found = false;
            foreach(CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(originalMethod))
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Call, myPassthroughMethod);
                }
                else
                {
                    yield return instruction;
                }

            }

            if (!found) Log.Error("(Humanoid Alien Races) Unable to find injection point for character card randomization transpiler, the target may have been changed or transpiled by another mod.");
        }

        private static PawnKindDef startingPawnKindRestriction;
        private static string      startingPawnKindLabel;

        private static bool PawnKindRandomizeButtonPassthrough(Rect rect, string label, bool drawBackground = true, bool doMouseoverSound = true, bool active = true, TextAnchor? overrideTextAnchor = null)
        {
            Rect rightRect = rect;
            rightRect.x += 154f;
            rightRect.width = 46f;

            if (Mouse.IsOver(rightRect))
                if (Find.WindowStack.FloatMenu == null)
                {
                    TaggedString tipString = "HAR.StartingRace".Translate(startingPawnKindLabel ?? ("None".Translate())).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "HAR.StartingRaceDescription".Translate();
                    TooltipHandler.TipRegion(rightRect, tipString.Resolve());
                }

            if (Widgets.ButtonImageWithBG(rightRect, startingPawnKindRestriction == null ? CachedData.Textures.AlienIconInactive : CachedData.Textures.AlienIconActive, new Vector2(22f, 22f))) 
                DoStartingPawnKindDropdown();

            rect.width = 150f;

            return Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, active, overrideTextAnchor);
        }

        private static void DoStartingPawnKindDropdown()
        {
            List<PawnKindDef> startingPawnKinds                 = [];
            HashSet<string>   startingPawnKindLabelSet          = [];
            HashSet<string>   startingPawnKindDuplicateLabelSet = [];

            PawnKindDef basicMemberKind = Find.GameInitData.startingPawnKind ?? Faction.OfPlayer.def.basicMemberKind;

            List<FloatMenuOption> options =
            [
                new FloatMenuOption("NoneBrackets".Translate(), delegate
                                                                {
                                                                    startingPawnKindRestriction = null;
                                                                    startingPawnKindLabel       = "None".Translate();
                                                                })
            ];

            foreach (PawnKindEntry entry in NewGeneratedStartingPawnKinds(basicMemberKind))
            {
                foreach (PawnKindDef kind in entry.kindDefs)
                {
                    if (startingPawnKinds.Contains(kind))
                        continue;

                    startingPawnKinds.Add(kind);
                    if (!startingPawnKindLabelSet.Add(kind.label))
                        startingPawnKindDuplicateLabelSet.Add(kind.label);
                }
            }

            foreach (PawnKindDef kind in startingPawnKinds)
            {
                string label;
                if (startingPawnKindDuplicateLabelSet.Contains(kind.label))
                    label = $"{kind.race.LabelCap} ({kind.defName})";
                else if (kind.label == kind.race.label)
                    label = kind.race.LabelCap;
                else
                    label = $"{kind.race.LabelCap} ({kind.label})";


                options.Add(new FloatMenuOption(label, delegate
                                                       {
                                                           startingPawnKindRestriction = kind;
                                                           startingPawnKindLabel       = label;
                                                       }
                                               ));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        public static bool GeneratePawnNamePrefix(ref Name __result, Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
        {
            if (pawn.def is not ThingDef_AlienRace alienProps || alienProps.race.GetNameGenerator(pawn.gender) == null || style != NameStyle.Full || pawn.kindDef.GetNameMaker(pawn.gender) != null) 
                return true;

            NameTriple nameTriple = NameTriple.FromString(NameGenerator.GenerateName(alienProps.race.GetNameGenerator(pawn.gender)));

            string first = nameTriple.First, nick = nameTriple.Nick, last = nameTriple.Last;
            
            nick ??= nameTriple.First;

            if (last != null && forcedLastName != null) 
                last = forcedLastName;

            __result = new NameTriple(first ?? string.Empty, nick ?? string.Empty, last ?? string.Empty);

            return false;
        }

        public static void GenerateRandomAgePrefix(Pawn pawn, PawnGenerationRequest request)
        {
            AlienPartGenerator.AlienComp alienComp = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
            if (!request.FixedGender.HasValue && !pawn.kindDef.fixedGender.HasValue && pawn.RaceProps.hasGenders)
            {
                float? maleGenderProbability = pawn.kindDef.GetModExtension<Info>()?.maleGenderProbability ?? (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.maleGenderProbability;

                if (!maleGenderProbability.HasValue) return;

                pawn.gender = Rand.Value >= maleGenderProbability ? Gender.Female : Gender.Male;

                if ((alienComp == null || !(Math.Abs(maleGenderProbability.Value) < 0.001f)) && !(Math.Abs(maleGenderProbability.Value - 1f) < 0.001f)) return;
                if (alienComp != null)
                    alienComp.fixGenderPostSpawn = true;
            }
            if(alienComp != null && pawn.kindDef.forcedHairColor.HasValue)
                alienComp.OverwriteColorChannel("hair", pawn.kindDef.forcedHairColor.Value);
            if (alienComp != null && pawn.kindDef.skinColorOverride.HasValue)
                alienComp.OverwriteColorChannel("skin", pawn.kindDef.skinColorOverride.Value);

        }

        public static IEnumerable<CodeInstruction> GenerateGenesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo randomGreyColorInfo = AccessTools.Method(typeof(PawnHairColors), nameof(PawnHairColors.RandomGreyHairColor));


            bool foundForcedGeneEntry = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!foundForcedGeneEntry && instruction.opcode == OpCodes.Ldarg_1)
                {
                    foundForcedGeneEntry = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(patchType, nameof(GenerateGenesForcedHelper));
                }


                yield return instruction;

                if (instructionList[i].Calls(randomGreyColorInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(patchType, nameof(OldHairColorHelper));
                }
            }
        }

        public static Color OldHairColorHelper(Color originalColor, Pawn pawn) =>
            pawn.def is ThingDef_AlienRace alienProps ?
                pawn.GetComp<AlienPartGenerator.AlienComp>().GenerateColor(alienProps.alienRace.generalSettings.alienPartGenerator.oldHairColorGen) :
                originalColor;

        public static void GenerateGenesForcedHelper(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
                foreach (AlienChanceEntry<GeneDef> gene in alienProps.alienRace.generalSettings.raceGenes)
                    foreach(GeneDef option in gene.Select(pawn))
                        pawn.genes.AddGene(option, false);
        }

        public static void GenerateGenesPrefix(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (pawn.def is ThingDef_AlienRace)
            {
                if (pawn.story.HairColor == Color.white)
                    pawn.story.HairColor = Color.clear;

                AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                pawn.story.SkinColorBase = alienComp.GetChannel(channel: "skin").first;
            }
        }

        public static void GenerateGenesPostfix(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace)
            {
                AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                //Genes did some nonsense, so actual assignment here

                if(pawn.story.HairColor == Color.clear)
                    pawn.story.HairColor = alienComp.GetChannel(channel: "hair").first;
                else if (alienComp.GetChannel(channel: "hair").first == Color.clear)
                    alienComp.OverwriteColorChannel("hair", pawn.story.HairColor);
            }
        }

        public static IEnumerable<CodeInstruction> DefaultStartingPawnTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            FieldInfo basicMemberInfo = AccessTools.Field(typeof(FactionDef),    nameof(FactionDef.basicMemberKind));
            FieldInfo baseLinerInfo   = AccessTools.Field(typeof(XenotypeDefOf), nameof(XenotypeDefOf.Baseliner));

            //LocalBuilder pawnKindDefLocal = ilg.DeclareLocal(typeof(PawnKindDef));

            LocalBuilder xenotypeDefLocal        = ilg.DeclareLocal(typeof(XenotypeDef));
            LocalBuilder xenotypeCustomLocal     = ilg.DeclareLocal(typeof(CustomXenotype));
            LocalBuilder developmentalStageLocal = ilg.DeclareLocal(typeof(DevelopmentalStage));
            LocalBuilder allowDownedLocal        = ilg.DeclareLocal(typeof(bool));

            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;

                if (instruction.LoadsField(basicMemberInfo))
                {
                    yield return CodeInstruction.Call(patchType, nameof(NewGeneratedStartingPawnHelper)).WithLabels(instructionList[i+1].ExtractLabels());
                    yield return new CodeInstruction(OpCodes.Dup) { labels = instructionList[i + 1].ExtractLabels() };
                    //yield return new CodeInstruction(OpCodes.Stloc, pawnKindDefLocal.LocalIndex) { labels = instructionList[i +1].ExtractLabels()};
                    //yield return new CodeInstruction(OpCodes.Ldloc, pawnKindDefLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloca, xenotypeDefLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloca, xenotypeCustomLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloca, developmentalStageLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloca, allowDownedLocal.LocalIndex);
                    yield return CodeInstruction.Call(patchType, nameof(PickStartingPawnConfig));
                } else if (instruction.opcode == OpCodes.Ldc_I4_0 && instructionList[i - 1].opcode == OpCodes.Ldc_I4_0 && instructionList[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldloc, allowDownedLocal.LocalIndex);
                } else if (instruction.LoadsField(baseLinerInfo))
                {
                    //yield return new CodeInstruction(OpCodes.Ldloc,  pawnKindDefLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldloc, xenotypeDefLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloc, xenotypeCustomLocal.LocalIndex).WithLabels(instructionList[i+1].labels);
                    yield return instructionList[i + 2];
                    yield return instructionList[i + 3];
                    yield return new CodeInstruction(OpCodes.Ldloc, developmentalStageLocal.LocalIndex);

                    i +=4;
                }
            }
        }

        public static void PickStartingPawnConfig(PawnKindDef kindDef, out XenotypeDef xenotypeDef, out CustomXenotype xenotypeCustom, out DevelopmentalStage devStage, out bool allowDowned)
        {
            xenotypeDef = currentStartingRequest.ForcedXenotype ?? XenotypeDefOf.Baseliner;
            xenotypeDef = RaceRestrictionSettings.CanUseXenotype(xenotypeDef, kindDef.race) ?
                              xenotypeDef :
                              RaceRestrictionSettings.FilterXenotypes(DefDatabase<XenotypeDef>.AllDefsListForReading, kindDef.race, out _).TryRandomElement(out XenotypeDef def) ?
                                  def :
                                  xenotypeDef;

            xenotypeCustom = currentStartingRequest.ForcedCustomXenotype;

            devStage = currentStartingRequest.AllowedDevelopmentalStages.Equals(DevelopmentalStage.None) ? 
                           DevelopmentalStage.Adult : 
                           currentStartingRequest.AllowedDevelopmentalStages;

            allowDowned = currentStartingRequest.AllowDowned;

            if (!CachedData.canBeChild(kindDef))
            {
                devStage    = DevelopmentalStage.Adult;
                allowDowned = false;
            }
        }

        public static IEnumerable<PawnKindEntry> NewGeneratedStartingPawnKinds(PawnKindDef basicMember) =>
            DefDatabase<RaceSettings>.AllDefsListForReading.Where(tdar => !tdar.pawnKindSettings.startingColonists.NullOrEmpty())
                                  .SelectMany(tdar => tdar.pawnKindSettings.startingColonists).Where(sce => sce.factionDefs.Contains(Faction.OfPlayer.def))
                                  .SelectMany(sce => sce.pawnKindEntries).AddItem(new PawnKindEntry { chance = 100f, kindDefs = [basicMember] });

        public static PawnKindDef NewGeneratedStartingPawnHelper(PawnKindDef basicMember)
        {
            IEnumerable<PawnKindEntry> usableEntries = NewGeneratedStartingPawnKinds(basicMember);

            // Only use the override if it's specifying a valid pawnkind for the current scenario (safety check)
            if (startingPawnKindRestriction != null && usableEntries.Any(pke => pke.kindDefs.Contains(startingPawnKindRestriction))) 
                return startingPawnKindRestriction;
            
            return usableEntries.TryRandomElementByWeight(pke => pke.chance, out PawnKindEntry pk) ? 
                       pk.kindDefs.RandomElement() : 
                       basicMember;
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

        
        public static bool FillBackstoryInSlotShuffledPrefix(Pawn pawn, BackstorySlot slot)
        {
            bioReference = null;
            if (slot == BackstorySlot.Adulthood && pawn.story.Childhood is AlienBackstoryDef absd && absd.linkedBackstory != null)
            {
                pawn.story.Adulthood = absd.linkedBackstory;
                return false;
            }
            
            return true;
        }

        public static IEnumerable<CodeInstruction> FillBackstorySlotShuffledTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo backstoryDatabaseInfo = AccessTools.PropertyGetter(typeof(DefDatabase<BackstoryDef>), nameof(DefDatabase<BackstoryDef>.AllDefs));

            bool done = false;

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                yield return codeInstruction;

                if (!done && i > 1 && codeInstruction.Calls(backstoryDatabaseInfo))
                {
                    done = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(FilterBackstories)));
                }
            }
        }

        public static IEnumerable<BackstoryDef> FilterBackstories(IEnumerable<BackstoryDef> backstories, Pawn pawn, BackstorySlot slot) =>
            backstories.Where(predicate: bs => bs is not AlienBackstoryDef abs || 
                                               abs.Approved(pawn) && (slot != BackstorySlot.Adulthood || abs.linkedBackstory == null || pawn.story.Childhood == abs.linkedBackstory));

        private static PawnBioDef                bioReference;

        public static void TryGetRandomUnusedSolidBioForPostfix(List<BackstoryCategoryFilter> backstoryCategories, ref bool __result, ref PawnBio result, PawnKindDef kind, Gender gender, string requiredLastName)
        {
            List<BackstoryCategoryFilter> categories = backstoryCategories.ListFullCopy();
            while (!categories.NullOrEmpty())
            {
                if(!categories.TryRandomElementByWeight(b => b.commonality, out BackstoryCategoryFilter bcf))
                    bcf = categories.RandomElement();
                categories.Remove(bcf);

                if (SolidBioDatabase.allBios.Where(predicate: pb =>
                      (((kind.race as ThingDef_AlienRace)?.alienRace.generalSettings.allowHumanBios ?? true) && (kind.GetModExtension<Info>()?.allowHumanBios ?? true) ||
                       (DefDatabase<PawnBioDef>.AllDefs.FirstOrDefault(predicate: pbd => pb.name.ConfusinglySimilarTo(pbd.name))?.validRaces.Contains(kind.race) ?? false)) &&
                      pb.gender.IsGenderApplicable(gender)                                                                                                                  &&
                      (requiredLastName.NullOrEmpty() || pb.name.Last == requiredLastName)                                                                                  &&
                      (!kind.factionLeader            || pb.pirateKing)                                                                                                     &&
                      bcf.Matches(pb.adulthood)                                                                                                                             &&
                      !pb.name.UsedThisGame).TryRandomElement(out PawnBio bio))
                {
                    result       = bio;
                    bioReference = DefDatabase<PawnBioDef>.AllDefs.FirstOrDefault(predicate: pbd => bio.name.ConfusinglySimilarTo(pbd.name));
                    __result     = true;
                    return;
                }
            }

            result   = null;
            __result = false;
        }

        public static void GenerateTraitsPostfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (!request.AllowedDevelopmentalStages.Newborn() && request.CanGeneratePawnRelations)
                CachedData.generatePawnsRelations(pawn, ref request);

            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                List<AlienChanceEntry<TraitWithDegree>> alienTraits = [];
                
                if(!alienProps.alienRace.generalSettings.forcedRaceTraitEntries.NullOrEmpty())
                    alienTraits.AddRange(alienProps.alienRace.generalSettings.forcedRaceTraitEntries);

                foreach (BackstoryDef backstory in pawn.story.AllBackstories)
                    if (backstory is AlienBackstoryDef alienBackstory)
                        if(!alienBackstory.forcedTraitsChance.NullOrEmpty())
                            alienTraits.AddRange(alienBackstory.forcedTraitsChance);


                foreach (AlienChanceEntry<TraitWithDegree> ate in alienTraits)
                        foreach (TraitWithDegree trait in ate.Select(pawn))
                            if (!pawn.story.traits.HasTrait(trait.def)) 
                                pawn.story.traits.GainTrait(new Trait(trait.def, trait.degree, forced: true));

                int traits = alienProps.alienRace.generalSettings.additionalTraits.RandomInRange;
                if (traits > 0)
                {
                    List<Trait> traitList = PawnGenerator.GenerateTraitsFor(pawn, traits, request);
                    foreach (Trait trait in traitList)
                        pawn.story.traits.GainTrait(trait);
                }
            }
        }

        public static IEnumerable<CodeInstruction> SkinColorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo baseColorInfo = AccessTools.PropertyGetter(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.SkinColorBase));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(baseColorInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_StoryTracker), "pawn"));
                    yield return CodeInstruction.Call(patchType, nameof(SkinColorHelper));
                } else
                {
                    yield return instruction;
                }
            }
        }

        public static Color SkinColorHelper(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps) 
                return alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(pawn);
            return CachedData.skinColorBase(pawn.story)!.Value;
        }

        public static void GetBodyTypeForPostfix(Pawn pawn, ref BodyTypeDef __result) =>
            __result = CheckBodyType(pawn, __result);

        public static void GenerateBodyTypePostfix(Pawn pawn) =>
            pawn.story.bodyType = CheckBodyType(pawn, pawn.story.bodyType);

        public static BodyTypeDef CheckBodyType(Pawn pawn, BodyTypeDef bodyType)
        {
            if (AlienBackstoryDef.checkBodyType.Contains(pawn.story.GetBackstory(BackstorySlot.Adulthood)))
                bodyType = DefDatabase<BodyTypeDef>.GetRandom();

            if (pawn.def is ThingDef_AlienRace alienProps                            &&
                alienProps.alienRace.generalSettings.alienPartGenerator is { } parts &&
                !alienProps.alienRace.generalSettings.alienPartGenerator.bodyTypes.NullOrEmpty())
            {
                List<BodyTypeDef> bodyTypeDefs = parts.bodyTypes.ListFullCopy();

                if ((pawn.ageTracker.CurLifeStage.developmentalStage.Baby() || pawn.ageTracker.CurLifeStage.developmentalStage.Newborn()) && bodyTypeDefs.Contains(BodyTypeDefOf.Baby))
                {
                    bodyType = BodyTypeDefOf.Baby;
                }
                else if (pawn.ageTracker.CurLifeStage.developmentalStage.Juvenile() && bodyTypeDefs.Contains(BodyTypeDefOf.Child))
                {
                    bodyType = BodyTypeDefOf.Child;
                }
                else
                {
                    bodyTypeDefs.Remove(BodyTypeDefOf.Baby);
                    bodyTypeDefs.Remove(BodyTypeDefOf.Child);

                    if (pawn.gender == Gender.Male)
                    {
                        BodyTypeDef femaleBodyType = parts.defaultFemaleBodyType;
                        if (bodyTypeDefs.Contains(femaleBodyType) && bodyTypeDefs.Count > 1)
                            bodyTypeDefs.Remove(femaleBodyType);
                    }

                    if (pawn.gender == Gender.Female)
                    {
                        BodyTypeDef maleBodyType = parts.defaultMaleBodyType;
                        if (bodyTypeDefs.Contains(maleBodyType) && bodyTypeDefs.Count > 1)
                            bodyTypeDefs.Remove(maleBodyType);
                    }

                    if (!bodyTypeDefs.Contains(bodyType))
                        bodyType = bodyTypeDefs.RandomElement();
                }
            }

            return bodyType;
        }

        public static void GeneratePawnPrefix(ref PawnGenerationRequest request)
        {
            PawnKindDef kindDef = request.KindDef;

            if (request.AllowedDevelopmentalStages.Newborn())
                return;

            if (Faction.OfPlayerSilentFail != null && kindDef == PawnKindDefOf.Colonist && (request.Faction?.IsPlayer ?? false) && kindDef.race != Faction.OfPlayer?.def.basicMemberKind.race)
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

            request.KindDef = kindDef;
        }

        public static void GeneratePawnPostfix(Pawn __result)
        {
            if (__result?.def is ThingDef_AlienRace race)
                foreach(AlienChanceEntry<AbilityDef> entry in race.alienRace.generalSettings.abilities)
                    foreach (AbilityDef ability in entry.Select(__result)) 
                        __result.abilities?.GainAbility(ability);
        }


        public static IEnumerable<CodeInstruction> TryGetGraphicApparelTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo originalMethod = AccessTools.Method(typeof(GraphicDatabase), "Get", [typeof(string), typeof(Shader), typeof(Vector2), typeof(Color)], [typeof(Graphic_Multi)]);

            MethodInfo newMethod = AccessTools.Method(typeof(ApparelGraphics.ApparelGraphicUtility), nameof(ApparelGraphics.ApparelGraphicUtility.GetGraphic));

            foreach(CodeInstruction instruction in codeInstructions)
            {
                if (instruction.Calls(originalMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction); // apparel
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // bodyType
                    yield return new CodeInstruction(OpCodes.Call, newMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}