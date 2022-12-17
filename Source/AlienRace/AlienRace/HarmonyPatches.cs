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
    using System.Runtime.InteropServices;
    using HarmonyLib;
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
            Harmony harmony = new Harmony(id: "rimworld.erdelf.alien_race.main");

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
            
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_Researcher), nameof(WorkGiver_Researcher.ShouldSkip)), 
                postfix: new HarmonyMethod(patchType, nameof(ShouldSkipResearchPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), name: "ViewSize"), transpiler:
                new HarmonyMethod(patchType, nameof(ResearchScreenTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), name: "DrawRightRect"), transpiler:
                new HarmonyMethod(patchType, nameof(ResearchScreenTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.CanConstruct), new[]{typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool)}), 
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
            harmony.Patch(AccessTools.Method(typeof(AgeInjuryUtility), nameof(AgeInjuryUtility.GenerateRandomOldAgeInjuries)),
                          new HarmonyMethod(patchType, nameof(GenerateRandomOldAgeInjuriesPrefix)));
            harmony.Patch(
                AccessTools.Method(typeof(AgeInjuryUtility), name: "RandomHediffsToGainOnBirthday", new[] { typeof(ThingDef), typeof(float), typeof(float) }),
                postfix: new HarmonyMethod(patchType, nameof(RandomHediffsToGainOnBirthdayPostfix)));
            
            //            harmony.Patch(original: AccessTools.Property(type: typeof(JobDriver), name: nameof(JobDriver.Posture)).GetGetMethod(nonPublic: false), postfix:
            //                postfix: new HarmonyMethod(type: patchType, name: nameof(PosturePostfix)));
            //            harmony.Patch(original: AccessTools.Property(type: typeof(JobDriver_Skygaze), name: nameof(JobDriver_Skygaze.Posture)).GetGetMethod(nonPublic: false), postfix:
            //                postfix: new HarmonyMethod(type: patchType, name: nameof(PosturePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateRandomAge"), new HarmonyMethod(patchType,             nameof(GenerateRandomAgePrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateTraits"),    new HarmonyMethod(patchType,             nameof(GenerateTraitsPrefix)), postfix: new HarmonyMethod(patchType, nameof(GenerateTraitsPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateTraitsFor"), transpiler: new HarmonyMethod(patchType, nameof(GenerateTraitsForTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_SatisfyChemicalNeed), name: "DrugValidator"), 
                          postfix:          new HarmonyMethod(patchType, nameof(DrugValidatorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(CompDrug), nameof(CompDrug.PostIngested)), 
                postfix: new HarmonyMethod(patchType, nameof(PostIngestedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(AddictionUtility), nameof(AddictionUtility.CanBingeOnNow)), 
                postfix: new HarmonyMethod(patchType, nameof(CanBingeNowPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateBodyType"), 
                          postfix: new HarmonyMethod(patchType, nameof(GenerateBodyTypePostfix)));
            harmony.Patch(AccessTools.Property(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.SkinColor)).GetGetMethod(), 
                postfix: new HarmonyMethod(patchType, nameof(SkinColorPostfix)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), name: "BirthdayBiological"), new HarmonyMethod(patchType, nameof(BirthdayBiologicalPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) }),
                new HarmonyMethod(patchType, nameof(GeneratePawnPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                new HarmonyMethod(patchType, nameof(ResolveAllGraphicsPrefix)));
            
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), name: "RenderPawnInternal",
                                             new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags)}), 
                          new HarmonyMethod(patchType, nameof(RenderPawnInternalPrefix)),
                          transpiler: new HarmonyMethod(patchType, nameof(RenderPawnInternalTranspiler)));

            harmony.Patch(AccessTools.PropertyGetter(typeof(StartingPawnUtility), "DefaultStartingPawnRequest"),
                          transpiler: new HarmonyMethod(patchType, nameof(DefaultStartingPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo)), 
                postfix: new HarmonyMethod(patchType, nameof(GiveAppropriateBioAndNameToPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName)),
                new HarmonyMethod(patchType, nameof(GeneratePawnNamePrefix)));
            harmony.Patch(AccessTools.Method(typeof(Page_ConfigureStartingPawns), name: "CanDoNext"), 
                postfix: new HarmonyMethod(patchType, nameof(CanDoNextStartPawnPostfix)));
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
                postfix: new HarmonyMethod(patchType, nameof(RecalculateLifeStageIndexPostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.FactionTick)), transpiler:
                          new HarmonyMethod(patchType, nameof(FactionTickTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Designator), nameof(Designator.CanDesignateThing)), 
                postfix: new HarmonyMethod(patchType, nameof(CanDesignateThingTamePostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_InteractAnimal), name: "CanInteractWithAnimal", new []{typeof(Pawn), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool), typeof(bool), typeof(bool), typeof(bool)}), 
                postfix: new HarmonyMethod(patchType, nameof(CanInteractWithAnimalPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt)), 
                          postfix: new HarmonyMethod(patchType, nameof(BaseHeadOffsetAtPostfix)),
                transpiler: new HarmonyMethod(patchType, nameof(BaseHeadOffsetAtTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), name: "CheckForStateChange"), 
                postfix: new HarmonyMethod(patchType, nameof(CheckForStateChangePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateGearFor"), 
                postfix:          new HarmonyMethod(patchType, nameof(GenerateGearForPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.ChangeKind)), new HarmonyMethod(patchType, nameof(ChangeKindPrefix)));

            harmony.Patch(AccessTools.Method(typeof(EditWindow_TweakValues), nameof(EditWindow_TweakValues.DoWindowContents)), transpiler: new HarmonyMethod(patchType, nameof(TweakValuesTranspiler)));
            
            HarmonyMethod misandryMisogonyTranspiler = new HarmonyMethod(patchType, nameof(MisandryMisogynyTranspiler));
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Woman), name: "CurrentSocialStateInternal"), transpiler: misandryMisogonyTranspiler);
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Man), name: "CurrentSocialStateInternal"), transpiler: misandryMisogonyTranspiler);

            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new []{typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool)}), postfix: new HarmonyMethod(patchType, nameof(CanEquipPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "GiveShuffledBioTo"), transpiler: 
                          new HarmonyMethod(patchType, nameof(MinAgeForAdulthood)));
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), name: "TryGiveSolidBioTo"), transpiler:
                          new HarmonyMethod(patchType, nameof(MinAgeForAdulthood)));

            harmony.Patch(AccessTools.Method(typeof(CompRottable), "StageChanged"), new HarmonyMethod(patchType, nameof(RottableCompStageChangedPostfix)));

            harmony.Patch(AccessTools.GetDeclaredMethods(typeof(PawnWoundDrawer)).First(mi => mi.Name.Contains("FindAnchors")), postfix: new HarmonyMethod(patchType, nameof(FindAnchorsPostfix)));

            harmony.Patch(AccessTools.GetDeclaredMethods(typeof(PawnWoundDrawer)).First(mi => mi.Name.Contains("CalcAnchorData")), postfix: new HarmonyMethod(patchType,    nameof(CalcAnchorDataPostfix)));
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
            
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn)),  transpiler: new HarmonyMethod(patchType, nameof(GetHumanlikeBodySetForPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn)),  transpiler: new HarmonyMethod(patchType, nameof(GetHumanlikeHeadSetForPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn)),  transpiler: new HarmonyMethod(patchType, nameof(GetHumanlikeHairSetForPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBeardSetForPawn)), transpiler: new HarmonyMethod(patchType, nameof(GetHumanlikeHairSetForPawnTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer),             nameof(PawnRenderer.GetBodyOverlayMeshSet)),                   postfix: new HarmonyMethod(patchType, nameof(GetBodyOverlayMeshSetPostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateSkills"), new HarmonyMethod(patchType, nameof(GenerateSkillsPrefix)), postfix: new HarmonyMethod(patchType, nameof(GenerateSkillsPostfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "TryGenerateNewPawnInternal"), transpiler: new HarmonyMethod(patchType, nameof(TryGenerateNewPawnInternalTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), "Notify_GenesChanged"), transpiler: new HarmonyMethod(patchType, nameof(NotifyGenesChangedTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(GrowthUtility),   nameof(GrowthUtility.IsGrowthBirthday)),       transpiler: new HarmonyMethod(patchType, nameof(IsGrowthBirthdayTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.TryChildGrowthMoment)), new HarmonyMethod(patchType,             nameof(TryChildGrowthMomentPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Gizmo_GrowthTier), "GrowthTierTooltip"), new HarmonyMethod(patchType,             nameof(GrowthTierTooltipPrefix)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_StyleTracker),             nameof(Pawn_StyleTracker.FinalizeHairColor)),  postfix: new HarmonyMethod(patchType,    nameof(FinalizeHairColorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Toils_StyleChange),             nameof(Toils_StyleChange.FinalizeLookChange)), postfix: new HarmonyMethod(patchType,    nameof(FinalizeLookChangePostfix)));
            harmony.Patch(AccessTools.Method(typeof(StatPart_FertilityByGenderAge), "AgeFactor"),                                  transpiler: new HarmonyMethod(patchType, nameof(FertilityAgeFactorTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.AddGene), new[] { typeof(Gene), typeof(bool) }), new HarmonyMethod(patchType, nameof(AddGenePrefix)));

            harmony.Patch(AccessTools.Method(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel)), transpiler: new HarmonyMethod(patchType, nameof(TryGetGraphicApparelTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(PregnancyUtility),   nameof(PregnancyUtility.PregnancyChanceForPartners)), prefix: new HarmonyMethod(patchType,     nameof(PregnancyChanceForPartnersPrefix)));
            harmony.Patch(AccessTools.Method(typeof(PregnancyUtility),   nameof(PregnancyUtility.CanEverProduceChild)),        transpiler: new HarmonyMethod(patchType, nameof(CanEverProduceChildTranspiler)));
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
            harmony.Patch(AccessTools.Method(typeof(PregnancyUtility),                   nameof(PregnancyUtility.ApplyBirthOutcome)), transpiler: new HarmonyMethod(patchType, nameof(ApplyBirthOutcomeTranspiler)));

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
                    if (!RaceRestrictionSettings.apparelRestricted.Contains(thingDef))
                        RaceRestrictionSettings.apparelRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteApparelList.Add(thingDef);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.weaponList)
                {
                    if (!RaceRestrictionSettings.weaponRestricted.Contains(thingDef))
                        RaceRestrictionSettings.weaponRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteWeaponList.Add(thingDef);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.buildingList)
                {
                    if (!RaceRestrictionSettings.buildingRestricted.Contains(thingDef))
                        RaceRestrictionSettings.buildingRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteBuildingList.Add(thingDef);
                }

                foreach (RecipeDef recipeDef in ar.alienRace.raceRestriction.recipeList)
                {
                    if (!RaceRestrictionSettings.recipeRestricted.Contains(recipeDef))
                        RaceRestrictionSettings.recipeRestricted.Add(recipeDef);
                    ar.alienRace.raceRestriction.whiteRecipeList.Add(recipeDef);
                }
                
                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.plantList)
                {
                    if (!RaceRestrictionSettings.plantRestricted.Contains(thingDef))
                        RaceRestrictionSettings.plantRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whitePlantList.Add(thingDef);
                }
                
                foreach (TraitDef traitDef in ar.alienRace.raceRestriction.traitList)
                {
                    if (!RaceRestrictionSettings.traitRestricted.Contains(traitDef))
                        RaceRestrictionSettings.traitRestricted.Add(traitDef);
                    ar.alienRace.raceRestriction.whiteTraitList.Add(traitDef);
                }
                
                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.foodList)
                {
                    if (!RaceRestrictionSettings.foodRestricted.Contains(thingDef))
                        RaceRestrictionSettings.foodRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteFoodList.Add(thingDef);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.petList)
                {
                    if (!RaceRestrictionSettings.petRestricted.Contains(thingDef))
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
                    if (!RaceRestrictionSettings.geneRestricted.Contains(geneDef))
                        RaceRestrictionSettings.geneRestricted.Add(geneDef);
                    ar.alienRace.raceRestriction.whiteGeneList.Add(geneDef);
                }

                foreach (ThingDef thingDef in ar.alienRace.raceRestriction.reproductionList)
                {
                    if (!RaceRestrictionSettings.reproductionRestricted.Contains(thingDef))
                        RaceRestrictionSettings.reproductionRestricted.Add(thingDef);
                    ar.alienRace.raceRestriction.whiteReproductionList.Add(thingDef);
                }

                if (ar.alienRace.generalSettings.corpseCategory != ThingCategoryDefOf.CorpsesHumanlike)
                {
                    ThingCategoryDefOf.CorpsesHumanlike.childThingDefs.Remove(ar.race.corpseDef);
                    if (ar.alienRace.generalSettings.corpseCategory != null)
                    {
                        ar.race.corpseDef.thingCategories = new List<ThingCategoryDef> { ar.alienRace.generalSettings.corpseCategory };
                        ar.alienRace.generalSettings.corpseCategory.childThingDefs.Add(ar.race.corpseDef);
                        ar.alienRace.generalSettings.corpseCategory.ResolveReferences();
                    }
                    ThingCategoryDefOf.CorpsesHumanlike.ResolveReferences();
                }

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

            Log.Message($"Alien race successfully completed {harmony.GetPatchedMethods().Select(Harmony.GetPatchInfo).SelectMany(selector: p => p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers)).Count(predicate: p => p.owner == harmony.Id)} patches with harmony.");
            HairDefOf.Bald.styleTags.Add(item: "alienNoStyle");
            BeardDefOf.NoBeard.styleTags.Add(item: "alienNoStyle");
            TattooDefOf.NoTattoo_Body.styleTags.Add(item: "alienNoStyle");
            TattooDefOf.NoTattoo_Face.styleTags.Add(item: "alienNoStyle");

            AlienRaceMod.settings.UpdateSettings();
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

            bool                  done            = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!done && instruction.LoadsField(genderInfo))
                {
                    done = true;
                    yield return instructionList[i + 1];
                    yield return CodeInstruction.Call(typeof(RaceRestrictionSettings), nameof(RaceRestrictionSettings.CanReproduce), new[] { typeof(Pawn), typeof(Pawn) });
                    yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i + 3].operand);
                    i += 3;
                } else
                {
                    yield return instruction;
                }
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

        public static bool AddGenePrefix(Gene gene, Pawn ___pawn, ref Gene __result)
        {
            if (!RaceRestrictionSettings.CanHaveGene(gene.def, ___pawn.def))
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

        public static void GetBodyOverlayMeshSetPostfix(PawnRenderer __instance, Pawn ___pawn, ref GraphicMeshSet __result)
        {
            if (!___pawn.RaceProps.Humanlike)
                return;

            Vector2 drawSize = (portraitRender.First.Target as Pawn == ___pawn && portraitRender.Second ?
                                    ___pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitHeadDrawSize :
                                    ___pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customHeadDrawSize) ?? Vector2.one;
            float   lifeStageFactor = ModsConfig.BiotechActive ? ___pawn.ageTracker.CurLifeStage.bodyWidth ?? 1f : 1f;
            Vector3 vector3         = __result.MeshAt(Rot4.North).vertices[2] * 2 * lifeStageFactor;
            __result = MeshPool.GetMeshSetForWidth(drawSize.x * vector3.x, drawSize.y * vector3.z);
        }

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
                    yield return CodeInstruction.Call(patchType, nameof(GrowthMomentHelper));
                else
                    yield return instruction;
        }

        public static int[] GrowthMomentHelper() =>
            (growthMomentPawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.growthAges?.ToArray() ?? GrowthUtility.GrowthMomentAges;

        public static IEnumerable<CodeInstruction> NotifyGenesChangedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo allDefsInfo = AccessTools.PropertyGetter(typeof(DefDatabase<HeadTypeDef>), nameof(DefDatabase<HeadTypeDef>.AllDefs));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.Calls(allDefsInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.pawn)));
                    yield return CodeInstruction.Call(patchType, nameof(HeadTypeFilter));
                }
            }
        }

        public static IEnumerable<CodeInstruction> TryGenerateNewPawnInternalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool done = false;

            MethodInfo allDefsInfo = AccessTools.PropertyGetter(typeof(DefDatabase<HeadTypeDef>), nameof(DefDatabase<HeadTypeDef>.AllDefs));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (!done && instruction.Calls(allDefsInfo))
                {
                    done = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
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
                        pawn.skills.GetSkill(passion.skill).passion = (Passion)passion.xp;
                }
        }

        public static IEnumerable<CodeInstruction> GetHumanlikeHairSetForPawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo helperInfo        = AccessTools.Method(patchType, nameof(GetHumanlikeHairSetForPawnHelper));


            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (instruction.IsLdloc() && instructionList[i-1].IsStloc())
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
            Vector2 drawSize = (portraitRender.First.Target as Pawn == pawn && portraitRender.Second ? 
                                   pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitHeadDrawSize : 
                                   pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customHeadDrawSize) ?? Vector2.one;
            return drawSize * headFactor;
        }

        public static IEnumerable<CodeInstruction> GetHumanlikeHeadSetForPawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getMeshInfo       = AccessTools.Method(typeof(MeshPool), nameof(MeshPool.GetMeshSetForWidth), new[] { typeof(float) });
            FieldInfo  humanlikeBodyInfo = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeHeadSet));
            MethodInfo helperInfo        = AccessTools.Method(patchType, nameof(GetHumanlikeHeadSetForPawnHelper));


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
            Vector2 drawSize = (portraitRender.First.Target as Pawn == pawn && portraitRender.Second ?
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitHeadDrawSize :
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customHeadDrawSize) ?? Vector2.one;

            Vector2 scaleFactor = lifestageFactor is Vector2 lifestageFactorV2 ?
                                      lifestageFactorV2 :
                                      lifestageFactor is float lifestageFactorF ?
                                          new Vector2(lifestageFactorF, lifestageFactorF) :
                                          Vector2.one;

            return MeshPool.GetMeshSetForWidth(drawSize.x * scaleFactor.x, drawSize.y * scaleFactor.y);
        }

        public static IEnumerable<CodeInstruction> GetHumanlikeBodySetForPawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getMeshInfo       = AccessTools.Method(typeof(MeshPool), nameof(MeshPool.GetMeshSetForWidth), new []{typeof(float)});
            FieldInfo  humanlikeBodyInfo = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeBodySet));
            MethodInfo helperInfo        = AccessTools.Method(patchType, nameof(GetHumanlikeBodySetForPawnHelper));


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
            Vector2 drawSize = (portraitRender.First.Target as Pawn == pawn && portraitRender.Second ?
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customPortraitDrawSize :
                                    pawn!.GetComp<AlienPartGenerator.AlienComp>()?.customDrawSize) ?? Vector2.one;

            Vector2 scaleFactor = lifestageFactor is Vector2 lifestageFactorV2 ?
                                      lifestageFactorV2 :
                                      lifestageFactor is float lifestageFactorF ?
                                          new Vector2(lifestageFactorF, lifestageFactorF) :
                                          Vector2.one;

            return MeshPool.GetMeshSetForWidth(drawSize.x * scaleFactor.x, drawSize.y * scaleFactor.y);
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
            MethodInfo defaultAnchorInfo = AccessTools.GetDeclaredMethods(typeof(PawnWoundDrawer)).First(mi => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.Name.Contains("GetDefaultAnchor"));

            List<CodeInstruction> instructionList = instructions.ToList();

            Label label = ilg.DefineLabel();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldarg_0 && instructionList[i + 4].Calls(defaultAnchorInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2).MoveLabelsFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Brfalse, label);
                    instructionList[i + 5].WithLabels(label);
                }

                yield return instruction;
            }
        }

        public static void TotalStyleItemLikelihoodPostfix(ref float __result)
        {
            __result += float.Epsilon;
        }

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

            if (thought == ThoughtDefOf.KnowButcheredHumanlikeCorpse)
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

            if (thought == ThoughtDefOf.ButcheredHumanlikeCorpse)
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

        public static void CalcAnchorDataPostfix(Pawn ___pawn, BodyTypeDef.WoundAnchor anchor, ref Vector3 anchorOffset)
        {
            if (___pawn.def is ThingDef_AlienRace alienRace)
            {
                List<AlienPartGenerator.WoundAnchorReplacement> anchorReplacements = alienRace.alienRace.generalSettings.alienPartGenerator.anchorReplacements;

                foreach (AlienPartGenerator.WoundAnchorReplacement anchorReplacement in anchorReplacements)
                {
                    if (anchor == anchorReplacement.replacement && anchorReplacement.offsets != null)
                    {
                        anchorOffset = anchorReplacement.offsets.GetOffset(anchor.rotation!.Value).GetOffset(false, ___pawn.story.bodyType, ___pawn.story.headType);
                        return;
                    }
                }
            }
        }

        public static List<BodyTypeDef.WoundAnchor> FindAnchorsPostfix(List<BodyTypeDef.WoundAnchor> __result, Pawn ___pawn)
        {
            if (___pawn.def is ThingDef_AlienRace alienRace)
            {
                List<AlienPartGenerator.WoundAnchorReplacement> anchorReplacements = alienRace.alienRace.generalSettings.alienPartGenerator.anchorReplacements;

                List<BodyTypeDef.WoundAnchor> result = new List<BodyTypeDef.WoundAnchor>();

                foreach (BodyTypeDef.WoundAnchor anchor in __result)
                {
                    AlienPartGenerator.WoundAnchorReplacement replacement = anchorReplacements.FirstOrDefault(war => war.ValidReplacement(anchor));
                    result.Add(replacement != null ? replacement.replacement : anchor);
                }

                return result;
            }

            return __result;
        }

        public static void RottableCompStageChangedPostfix(ThingWithComps ___parent)
        {
            Pawn pawn = ___parent as Pawn ?? (___parent as Corpse)?.InnerPawn;
            if(pawn != null)
                pawn.Drawer.renderer.graphics.nakedGraphic = null;

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
                    yield return new CodeInstruction(OpCodes.Ldloca, 1) { labels = instruction.labels.ListFullCopy() };
                    instruction.labels.Clear();
                    yield return new CodeInstruction(OpCodes.Ldloca, 2);
                    yield return new CodeInstruction(OpCodes.Ldloca, 3);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, operand: 4);
                    yield return new CodeInstruction(OpCodes.Call, 
                        AccessTools.Method(patchType, nameof(TweakValuesInstanceBased)));
                }

                yield return instruction;

                if (instruction.OperandIs(countInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, DefDatabase<ThingDef_AlienRace>.AllDefs.SelectMany(selector: ar =>
                            ar.alienRace.generalSettings.alienPartGenerator.bodyAddons).Sum(selector: ba =>
                                                                                                          new List<AlienPartGenerator.RotationOffset>
                                                                                                          {
                                                                                                              ba.offsets.east,
                                                                                                              ba.offsets.west,
                                                                                                              ba.offsets.north,
                                                                                                              ba.offsets.south,
                                                                                                          }.Sum(selector: ro => (ro.bodyTypes?.Count ?? 0) * 2 + (ro.headTypes?.Count ?? 0) * 2 + 3/* + (ro.portraitBodyTypes?.Count ?? 0) * 2 + 
                                                   (ro.crownTypes?.Count ?? 0) * 2 + (ro.portraitCrownTypes?.Count ?? 0) * 2*/) + 2) + 1);
                    yield return new CodeInstruction(OpCodes.Add);
                }
            }
        }

        private static Dictionary<string, float> tweakValuesSaved = new Dictionary<string, float>();

        public static void TweakValuesInstanceBased(ref Rect refRect2, ref Rect refRect3, ref Rect refRect4, ref Rect refRect5)
        {
            Rect rect2 = refRect2;
            Rect rect3 = refRect3;
            Rect rect4 = refRect4;
            Rect rect5 = refRect5;

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
                        string addonLabel     = label3Addons + label;
                        string raceAddonLabel = label2       + "." + addonLabel;
                        if(!tweakValuesSaved.ContainsKey(raceAddonLabel))
                            tweakValuesSaved.Add(raceAddonLabel, value);

                        Widgets.Label(rect2, label2);
                        Widgets.Label(rect3, addonLabel);

                        float num = value;
                        Widgets.HorizontalSlider(rect5, ref value, new FloatRange(-1, 1));

                        Rect valueFieldRect = rect4;

                        GUI.color = Color.red;
                        string savedS = tweakValuesSaved[raceAddonLabel].ToString(CultureInfo.InvariantCulture) + " -> ";
                        bool   changed      = Mathf.Abs(tweakValuesSaved[raceAddonLabel] - value) > float.Epsilon;
                        float  width  = changed ? Text.CalcSize(savedS).x : 0f;

                        Rect savedRect = rect4.LeftPartPixels(width);
                        Widgets.Label(savedRect, savedS);
                        GUI.color      = Color.white;
                        if(changed)
                            valueFieldRect = rect4.RightPartPixels(rect4.width - width);

                        if (Widgets.ButtonInvisible(savedRect))
                        {
                            GlobalTextureAtlasManager.FreeAllRuntimeAtlases();
                            return num = tweakValuesSaved[raceAddonLabel];
                        }

                        string valueS = value.ToString(CultureInfo.InvariantCulture);

                        string num2 = Widgets.TextField(valueFieldRect.ContractedBy(margin: 2).LeftPartPixels(Text.CalcSize(valueS).x + 6), valueS);

                        if (Mathf.Abs(num-value)<float.Epsilon)
                            if (float.TryParse(num2, out float num3))
                                num = num3;

                        if(Mathf.Abs(num - value) > float.Epsilon)
                            GlobalTextureAtlasManager.FreeAllRuntimeAtlases();

                        //Widgets.Label(rect: rect4, label: value.ToString(provider: CultureInfo.InvariantCulture));
                        return Mathf.Clamp(num, min: -1, max: 1);
                    }

                    {
                        Widgets.Label(rect2, label2);
                        string offsetLabel   = label3Addons + "DefaultOffsets";
                        string offsetDictKey = label2       + "." + offsetLabel;
                        Widgets.Label(rect3, offsetLabel);

                        if (!tweakValuesSaved.ContainsKey(offsetDictKey))
                            tweakValuesSaved.Add(offsetDictKey, ar.alienRace.generalSettings.alienPartGenerator.offsetDefaults.FirstIndexOf(on => on.name == ba.defaultOffset));


                        float offsetNew = ar.alienRace.generalSettings.alienPartGenerator.offsetDefaults.FirstIndexOf(on => on.name == ba.defaultOffset);
                        Widgets.HorizontalSlider(rect5, ref offsetNew, new FloatRange(0, ar.alienRace.generalSettings.alienPartGenerator.offsetDefaults.Count - 1), roundTo: 1);
                        offsetNew = Mathf.RoundToInt(offsetNew);

                        Rect valueFieldRect = rect4;

                        GUI.color = Color.red;
                        string savedS  = ar.alienRace.generalSettings.alienPartGenerator.offsetDefaults[(int)tweakValuesSaved[offsetDictKey]].name + " -> ";
                        bool   changed = Math.Abs(offsetNew - tweakValuesSaved[offsetDictKey]) > float.Epsilon;
                        float  width   = changed ? Text.CalcSize(savedS).x : 0f;

                        Rect savedRect = rect4.LeftPartPixels(width);
                        Widgets.Label(savedRect, savedS);
                        GUI.color = Color.white;
                        if (changed)
                        {
                            valueFieldRect = rect4.RightPartPixels(rect4.width - width);
                            GlobalTextureAtlasManager.FreeAllRuntimeAtlases();
                        }

                        AlienPartGenerator.OffsetNamed newOffsets = ar.alienRace.generalSettings.alienPartGenerator.offsetDefaults[(int) offsetNew];
                        Widgets.Label(valueFieldRect, newOffsets.name);
                        ba.defaultOffset  = newOffsets.name;
                        ba.defaultOffsets = newOffsets.offsets;

                        NextLine();
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
                        string                            label3Rotation;
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

                        ro.layerOffset = WriteLine(ro.layerOffset, label3Rotation + "layerOffset");
                        NextLine();
                        ro.offset.x = WriteLine(ro.offset.x, label3Rotation + "offset.x");
                        NextLine();
                        ro.offset.y = WriteLine(ro.offset.y, label3Rotation + "offset.y");
                        NextLine();

                        if (!ro.bodyTypes.NullOrEmpty())
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

                        if(!ro.headTypes.NullOrEmpty())
                            foreach (AlienPartGenerator.HeadTypeOffsets headTypeOffsets in ro.headTypes)
                            {
                                string  label3Type = headTypeOffsets.headType + ".";
                                Vector2 offset     = headTypeOffsets.offset;
                                float   offsetX    = offset.x;
                                float   offsetY    = offset.y;

                                float WriteAddonLine(float value, bool x) =>
                                    WriteLine(value, label3Rotation + label3Type + (x ? "x" : "y"));


                                headTypeOffsets.offset.x = WriteAddonLine(offsetX, x: true);
                                NextLine();
                                headTypeOffsets.offset.y = WriteAddonLine(offsetY, x: false);
                                NextLine();
                            }
                    }
                    NextLine();
                }
            }

            refRect2 = rect2;
            refRect3 = rect3;
            refRect4 = rect4;
            refRect5 = rect5;
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
            MethodInfo raceprops = AccessTools.Property(typeof(Pawn), nameof(Pawn.RaceProps)).GetGetMethod();
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

                    if (i < instructionList.Count - 1 && instructionList[i + 1].OperandIs(bodyInfo) && instruction.OperandIs(raceprops))
                    {

                        instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(ReplacedBody)));
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

        public static void CheckForStateChangePostfix(Pawn ___pawn)
        {
            if (Current.ProgramState == ProgramState.Playing && ___pawn.Spawned && ___pawn.def is ThingDef_AlienRace)
                ___pawn.Drawer.renderer.graphics.nakedGraphic = null;
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
            offset + ((pawn.ageTracker.CurLifeStageRace as LifeStageAgeAlien)?.headOffset ?? Vector2.zero);

        public static void BaseHeadOffsetAtPostfix(ref Vector3 __result, Rot4 rotation, Pawn ___pawn)
        {
            Vector2 offset = (___pawn.ageTracker.CurLifeStageRace as LifeStageAgeAlien)?.headOffsetDirectional?.GetOffset(rotation) ?? Vector2.zero;
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
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo isPlayerInfo = AccessTools.PropertyGetter(typeof(Faction), nameof(Faction.IsPlayer));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                yield return instruction;

                if (instruction.opcode == OpCodes.Brfalse && instructionList[i - 1].Calls(isPlayerInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call,    AccessTools.Method(patchType, nameof(FactionTickFactionRelationCheck)));
                    yield return new CodeInstruction(OpCodes.Brfalse, instruction.operand);
                }
            }
        }

        private static bool FactionTickFactionRelationCheck(Faction f)
        {
            FactionDef player = Faction.OfPlayerSilentFail?.def ?? Find.GameInitData?.playerFaction?.def;

            if (player == null)
                return false;

            return !DefDatabase<ThingDef_AlienRace>.AllDefs.Any(predicate: ar =>
                       f.def?.basicMemberKind?.race == ar &&
                       (ar?.alienRace.generalSettings?.factionRelations?.Any(predicate: frs => frs.factions?.Contains(player) ?? false) ?? false) ||
                       player.basicMemberKind?.race == ar &&
                       (ar?.alienRace.generalSettings?.factionRelations?.Any(predicate: frs => frs.factions?.Contains(f.def) ?? false) ?? false));
        }
        
        public static void RecalculateLifeStageIndexPostfix(Pawn ___pawn)
        {
            if (Current.ProgramState == ProgramState.Playing && (___pawn).def is ThingDef_AlienRace &&
                ___pawn.Drawer.renderer.graphics.AllResolved)
                ___pawn.Drawer.renderer.graphics.nakedGraphic = null;
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
            List<CodeInstruction> instructionList = instructions.ToList();
            MethodInfo            defListInfo     = AccessTools.Property(typeof(DefDatabase<TraitDef>), nameof(DefDatabase<TraitDef>.AllDefsListForReading)).GetGetMethod();

            foreach (CodeInstruction instruction in instructionList)
            {
                yield return instruction;
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
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(AdditionalInitialTraits)));
                }

                yield return instruction;
            }
        }

        public static int AdditionalInitialTraits(int count, Pawn pawn)
        {
            if (pawn.def is not ThingDef_AlienRace alienProps) return count;

            IntRange traitCount = alienProps.alienRace.generalSettings.traitCount;

            if (traitCount.min != 1 || traitCount.max != 3)
                count = traitCount.RandomInRange;
            return count;
        }

        public static IEnumerable<TraitDef> GenerateTraitsValidator(List<TraitDef> traits, Pawn p)
        {
            traits.RemoveAll(tr => !RaceRestrictionSettings.CanGetTrait(tr, p.def));
            return traits;
        }

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

        public static bool GainTraitPrefix(Trait trait, Pawn ___pawn)
        {
            if (___pawn.def is not ThingDef_AlienRace alienProps) 
                return true;

            if (!RaceRestrictionSettings.CanGetTrait(trait.def, alienProps, trait.Degree))
                return false;

            AlienTraitEntry ate = alienProps.alienRace.generalSettings.forcedRaceTraitEntries?.FirstOrDefault(predicate: at => at.defName == trait.def);
            if (ate == null) 
                return true;

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
                    relation.baseGoodwill = offset;
                    relation.kind = kind;

                    relation              = __instance.RelationWith(other);
                    relation.baseGoodwill = offset;
                    relation.kind         = kind;
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
                relation.baseGoodwill = offset;
                relation.kind     = kind;

                relation          = __instance.RelationWith(other);
                relation.baseGoodwill = offset;
                relation.kind     = kind;
            });

        }

        public static bool TryCreateThoughtPrefix(ref ThoughtDef def, SituationalThoughtHandler __instance, ref HashSet<ThoughtDef> ___tmpCachedThoughts)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is ThingDef_AlienRace race)
                def = race.alienRace.thoughtSettings.ReplaceIfApplicable(def);

            return !___tmpCachedThoughts.Contains(def);
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
                __result = -50f;
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

            if (!(pawn.def is ThingDef_AlienRace race)) return true;

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

        private static HashSet<ThingDef> colonistRaces = new HashSet<ThingDef>();
        private static int               colonistRacesTick;
        private const  int               COLONIST_RACES_TICK_TIMER = GenDate.TicksPerHour * 2;

        public static void UpdateColonistRaces()
        {
            if (Find.TickManager.TicksAbs > colonistRacesTick + COLONIST_RACES_TICK_TIMER || Find.TickManager.TicksAbs < colonistRacesTick)
            {
                List<Pawn> pawns = PawnsFinder.AllMaps_FreeColonistsSpawned;

                if (pawns.Count > 0)
                {
                    HashSet<ThingDef> newColonistRaces = new HashSet<ThingDef>(pawns.Select(selector: p => p.def));
                    colonistRacesTick = Find.TickManager.TicksAbs;
                    //Log.Message(string.Join(" | ", colonistRaces.Select(td => td.defName)));

                    if (newColonistRaces.Count != colonistRaces.Count || newColonistRaces.Any(td => !colonistRaces.Contains(td)))
                    {
                        RaceRestrictionSettings.buildingsRestrictedWithCurrentColony.Clear();
                        HashSet<ThingDef> buildingsRestrictedTemp = new HashSet<ThingDef>(RaceRestrictionSettings.buildingRestricted);
                        buildingsRestrictedTemp.AddRange(newColonistRaces.SelectMany(ar => (ar as ThingDef_AlienRace)?.alienRace.raceRestriction.blackBuildingList));
                        foreach (ThingDef td in buildingsRestrictedTemp)
                        {
                            bool canBuild = false;
                            foreach (ThingDef race in newColonistRaces)
                            {
                                if (RaceRestrictionSettings.CanBuild(td, race))
                                    canBuild = true;
                            }
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

        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<FoodUtility.ThoughtFromIngesting> __result)
        {
            try
            {
                if (ingester.def is not ThingDef_AlienRace alienProps) return;

                if (ingester.story.traits.HasTrait(AlienDefOf.Xenophobia) && ingester.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1)
                    if (__result.Any(tfi => tfi.thought       == ThoughtDefOf.AteHumanlikeMeatDirect) && foodDef.ingestible?.sourceDef != ingester.def)
                        __result.RemoveAll(tfi => tfi.thought == ThoughtDefOf.AteHumanlikeMeatDirect);
                    else if (__result.Any(tfi => tfi.thought == ThoughtDefOf.AteHumanlikeMeatAsIngredient) &&
                             (foodSource?.TryGetComp<CompIngredients>()?.ingredients
                                      ?.Any(predicate: td => FoodUtility.GetMeatSourceCategory(td) == MeatSourceCategory.Humanlike && td.ingestible?.sourceDef != ingester.def) ?? false))
                        __result.RemoveAll(tfi => tfi.thought == ThoughtDefOf.AteHumanlikeMeatAsIngredient);

                bool cannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);

                List<FoodUtility.ThoughtFromIngesting> resultingThoughts = new List<FoodUtility.ThoughtFromIngesting>();
                for (int i = 0; i < __result.Count; i++)
                {
                    ThoughtDef      thoughtDef = __result[i].thought;
                    ThoughtSettings settings   = alienProps.alienRace.thoughtSettings;

                    thoughtDef = settings.ReplaceIfApplicable(thoughtDef);

                    if (thoughtDef == ThoughtDefOf.AteHumanlikeMeatDirect || thoughtDef == ThoughtDefOf.AteHumanlikeMeatDirectCannibal)
                        thoughtDef = settings.GetAteThought(foodDef.ingestible?.sourceDef, cannibal, ingredient: false);

                    if (thoughtDef == ThoughtDefOf.AteHumanlikeMeatAsIngredient || thoughtDef == ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal)
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

        public static void CanDoNextStartPawnPostfix(ref bool __result)
        {
            if (__result) 
                return;

            bool result = true;
            Find.GameInitData.startingAndOptionalPawns.ForEach(action: current =>
            {
                if (!current.Name.IsValid && current.def.race.GetNameGenerator(current.gender) == null) result = false;
            });
            __result = result;
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

        public static void GiveAppropriateBioAndNameToPostfix(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace)
            {
                //Log.Message(pawn.LabelCap);

                AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                pawn.story.HairColor = alienComp.GetChannel(channel: "hair").first;
            }
        }

        public static IEnumerable<CodeInstruction> DefaultStartingPawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo basicMemberInfo = AccessTools.Field(typeof(FactionDef), nameof(FactionDef.basicMemberKind));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (!instruction.LoadsField(basicMemberInfo)) continue;

                yield return CodeInstruction.Call(patchType, nameof(NewGeneratedStartingPawnHelper));
            }
        }

        public static PawnKindDef NewGeneratedStartingPawnHelper(PawnKindDef basicMember) =>
            DefDatabase<RaceSettings>.AllDefsListForReading.Where(predicate: tdar => !tdar.pawnKindSettings.startingColonists.NullOrEmpty())
                                  .SelectMany(selector: tdar => tdar.pawnKindSettings.startingColonists).Where(predicate: sce => sce.factionDefs.Contains(Faction.OfPlayer.def))
                                  .SelectMany(selector: sce => sce.pawnKindEntries).AddItem(new PawnKindEntry {chance = 100f, kindDefs = new List<PawnKindDef> {basicMember}})
                                  .TryRandomElementByWeight(pke => pke.chance, out PawnKindEntry pk)
                ? pk.kindDefs.RandomElement()
                : basicMember;

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

        public static List<BackstoryDef> FilterBackstories(IEnumerable<BackstoryDef> backstories, Pawn pawn, BackstorySlot slot) =>
            backstories.Where(predicate: bs =>
                                         {
                                             AlienBackstoryDef abs = bs as AlienBackstoryDef;
                                             return (abs?.Approved(pawn) ?? true) && (slot != BackstorySlot.Adulthood || ((abs?.linkedBackstory == null) || pawn.story.Childhood == abs.linkedBackstory));
                                         }).ToList();

        private static PawnBioDef bioReference;

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

                if (alienComp != null)
                {
                    AlienPartGenerator apg = alienProps.alienRace.generalSettings.alienPartGenerator;

                    if (alienComp.fixGenderPostSpawn)
                    {
                        float? maleGenderProbability = alien.kindDef.GetModExtension<Info>()?.maleGenderProbability ?? alienProps.alienRace.generalSettings.maleGenderProbability;
                        __instance.pawn.gender = Rand.Value >= maleGenderProbability ? Gender.Female : Gender.Male;
                        __instance.pawn.Name   = PawnBioAndNameGenerator.GeneratePawnName(__instance.pawn);

                        alienComp.fixGenderPostSpawn = false;
                    }

                    GraphicPaths      graphicPaths = alienProps.alienRace.graphicPaths;
                    LifeStageAgeAlien lsaa         = (alien.ageTracker.CurLifeStageRace as LifeStageAgeAlien)!;



                    alienComp.customDrawSize             = lsaa.customDrawSize;
                    alienComp.customHeadDrawSize         = lsaa.customHeadDrawSize;
                    alienComp.customPortraitDrawSize     = lsaa.customPortraitDrawSize;
                    alienComp.customPortraitHeadDrawSize = lsaa.customPortraitHeadDrawSize;

                    alienComp.GetChannel("hair").first = alien.story.HairColor;

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
                                                                                                0, new List<ShaderParameter> { graphicPaths.SkinColoringParameter }, bodyMask)) :
                                                  null;
                    
                    __instance.rottingGraphic = !bodyPath.NullOrEmpty() ?
                                                GraphicDatabase.Get<Graphic_Multi>(bodyPath, ContentFinder<Texture2D>.Get(bodyPath + "_northm", reportFailure: false) == null ?
                                                                                                 skinShader : ShaderDatabase.CutoutComplex, 
                                                                                   Vector2.one, PawnGraphicSet.RottingColorDefault, PawnGraphicSet.RottingColorDefault, null, bodyMask) :
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
                                                           GraphicDatabase.Get<Graphic_Multi>(headPath, ShaderDatabase.Cutout, Vector2.one, PawnGraphicSet.RottingColorDefault) : 
                                                           null;

                    string skullPath = graphicPaths.skull.GetPath(alien, ref sharedIndex, alienComp.headVariant);
                    __instance.skullGraphic = alien.health.hediffSet.HasHead && !skullPath.NullOrEmpty()
                                                  ? GraphicDatabase.Get<Graphic_Multi>(skullPath, ShaderDatabase.Cutout, Vector2.one, Color.white)
                                                  : null;

                    if (!(__instance.pawn.story.hairDef?.noGraphic ?? true) && alienProps.alienRace.styleSettings[typeof(HairDef)].hasStyle)
                        __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.hairDef.texPath, ContentFinder<Texture2D>.Get(__instance.pawn.story.hairDef.texPath + "_northm", reportFailure: false) == null ? 
                                                                                                                               (alienProps.alienRace.styleSettings[typeof(HairDef)].shader?.Shader ?? ShaderDatabase.Transparent) : 
                                                                                                                               ShaderDatabase.CutoutComplex, Vector2.one, alien.story.HairColor,
                                                                                    alienComp.GetChannel(channel: "hair").second);

                    string stumpPath = graphicPaths.stump.GetPath(alien, ref sharedIndex, alienComp.headVariant);
                    __instance.headStumpGraphic = !stumpPath.NullOrEmpty() ? 
                                                      GraphicDatabase.Get<Graphic_Multi>(stumpPath, alien.story.SkinColor == apg.SkinColor(alien, first: false) ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, 
                                                                                                                Vector2.one, alien.story.SkinColor, apg.SkinColor(alien, first: false))
                                                      : null;
                    __instance.desiccatedHeadStumpGraphic = !stumpPath.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(stumpPath, ShaderDatabase.Cutout, Vector2.one, PawnGraphicSet.RottingColorDefault) : null;
                    
                    if (ModLister.BiotechInstalled)
                    {
                        __instance.furCoveredGraphic = alien.story.furDef != null ? GraphicDatabase.Get<Graphic_Multi>(alien.story.furDef.GetFurBodyGraphicPath(alien), ShaderDatabase.CutoutSkinOverlay, Vector2.one, alien.story.HairColor) : null;
                    }
                    if (ModsConfig.BiotechActive)
                    {
                        __instance.swaddledBabyGraphic = GraphicDatabase.Get<Graphic_Multi>(graphicPaths.swaddle.GetPath(alien, ref sharedIndex, alien.HashOffset()), ShaderDatabase.Cutout, Vector2.one, CachedData.swaddleColor(__instance));
                    }
                    
                    if (alien.style != null && ModsConfig.IdeologyActive && (!ModLister.BiotechInstalled || alien.genes == null || !alien.genes.GenesListForReading.Any(x => x.def.graphicData is { tattoosVisible: false } && x.Active)))
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
                    //alienComp.OverwriteColorChannel("hair", alien.story.HairColor);
                    if (alien.Corpse?.GetRotStage() == RotStage.Rotting)
                        alienComp.OverwriteColorChannel("skin", PawnGraphicSet.RottingColorDefault);

                    alienComp.RegenerateColorChannelLinks();
                    alienComp.addonGraphics = new List<Graphic>();

                    alienComp.addonVariants ??= new List<int>();

                    sharedIndex = 0;

                    using (IEnumerator<AlienPartGenerator.BodyAddon> bodyAddons = apg.bodyAddons.Concat(Utilities.UniversalBodyAddons).GetEnumerator())
                    {
                        int addonIndex = 0;
                        while (bodyAddons.MoveNext())
                        {
                            Graphic g = bodyAddons.Current.GetGraphic(alien, ref sharedIndex, alienComp.addonVariants.Count > addonIndex ? alienComp.addonVariants[addonIndex] : null);
                            alienComp.addonGraphics.Add(g);
                            if (alienComp.addonVariants.Count <= addonIndex)
                                alienComp.addonVariants.Add(sharedIndex);

                            addonIndex++;
                        }
                    }

                    __instance.ResolveApparelGraphics();
                    __instance.ResolveGeneGraphics();

                    PortraitsCache.SetDirty(alien);
                    GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(alien);

                    return false;
                }
            }

            return true;
        }

        public static void GenerateTraitsPostfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (!request.AllowedDevelopmentalStages.Newborn() && request.CanGeneratePawnRelations)
                CachedData.generatePawnsRelations(pawn, ref request);

            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                if (!alienProps.alienRace.generalSettings.forcedRaceTraitEntries.NullOrEmpty())
                    alienProps.alienRace.generalSettings.forcedRaceTraitEntries.ForEach(action: ate =>
                                                                                                {
                                                                                                    if ((pawn.gender != Gender.Male ||
                                                                                                         !(Math.Abs(ate.commonalityMale - -1f) < 0.001f) && !(Rand.Range(min: 0, max: 100) < ate.commonalityMale))                                  &&
                                                                                                        (pawn.gender != Gender.Female || Math.Abs(ate.commonalityFemale - -1f) > 0.001f && !(Rand.Range(min: 0, max: 100) < ate.commonalityFemale)) &&
                                                                                                        pawn.gender != Gender.None) return;
                                                                                                    if (!pawn.story.traits.allTraits.Any(predicate: tr => tr.def == ate.defName))
                                                                                                        pawn.story.traits.GainTrait(new Trait(ate.defName, ate.degree, forced: true));
                                                                                                });

                int         traits            = alienProps.alienRace.generalSettings.additionalTraits.RandomInRange;
                if (traits > 0)
                {
                    List<Trait> traitList = PawnGenerator.GenerateTraitsFor(pawn, traits, request);
                    foreach (Trait trait in traitList)
                        pawn.story.traits.GainTrait(trait);
                }
            }
        }

        public static void SkinColorPostfix(Pawn ___pawn, ref Color __result)
        {
            if (___pawn.def is ThingDef_AlienRace alienProps) 
                __result = alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(___pawn);
        }

        public static void GenerateBodyTypePostfix(ref Pawn pawn)
        {

            if (AlienBackstoryDef.checkBodyType.Contains(pawn.story.GetBackstory(BackstorySlot.Adulthood)))
                pawn.story.bodyType = DefDatabase<BodyTypeDef>.GetRandom();

            if (pawn.def is ThingDef_AlienRace alienProps && 
                !alienProps.alienRace.generalSettings.alienPartGenerator.bodyTypes.NullOrEmpty())
            {
                List<BodyTypeDef> bodyTypeDefs = alienProps.alienRace.generalSettings.alienPartGenerator.bodyTypes.ListFullCopy();
                
                if((pawn.ageTracker.CurLifeStage.developmentalStage.Baby() || pawn.ageTracker.CurLifeStage.developmentalStage.Newborn()) && bodyTypeDefs.Contains(BodyTypeDefOf.Baby))
                {
                    pawn.story.bodyType = BodyTypeDefOf.Baby;
                }
                else if (pawn.ageTracker.CurLifeStage.developmentalStage.Juvenile() && bodyTypeDefs.Contains(BodyTypeDefOf.Child))
                {
                    pawn.story.bodyType = BodyTypeDefOf.Child;
                }
                else
                {
                    bodyTypeDefs.Remove(BodyTypeDefOf.Baby);
                    bodyTypeDefs.Remove(BodyTypeDefOf.Child);

                    if (pawn.gender == Gender.Male && bodyTypeDefs.Contains(BodyTypeDefOf.Female) && bodyTypeDefs.Count > 1)
                        bodyTypeDefs.Remove(BodyTypeDefOf.Female);

                    if (pawn.gender == Gender.Female && bodyTypeDefs.Contains(BodyTypeDefOf.Male) && bodyTypeDefs.Count > 1)
                        bodyTypeDefs.Remove(BodyTypeDefOf.Male);

                    if (!bodyTypeDefs.Contains(pawn.story.bodyType))
                        pawn.story.bodyType = bodyTypeDefs.RandomElement();
                }
            }
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

        public static Pair<WeakReference, bool> portraitRender;

        public static void RenderPawnInternalPrefix(Pawn ___pawn, PawnRenderFlags flags)
        {
            portraitRender = new Pair<WeakReference, bool>(new WeakReference(___pawn), (flags & PawnRenderFlags.Portrait) != 0);
        }

        public static IEnumerable<CodeInstruction> RenderPawnInternalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo drawHeadHairInfo      = AccessTools.Method(typeof(PawnRenderer), "DrawHeadHair");
            MethodInfo flagSetInfo       = AccessTools.Method(typeof(PawnRenderFlagsExtension), nameof(PawnRenderFlagsExtension.FlagSet));

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (i > 6 && instructionList[i - 2].OperandIs(drawHeadHairInfo) && instructionList[i+1].OperandIs(flagSetInfo))
                {
                    yield return new CodeInstruction(OpCodes.Dup); // renderFlags
                    yield return new CodeInstruction(OpCodes.Ldloc_1); //vector
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 8); //b (aka headoffset)
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);             // quat
                    yield return new CodeInstruction(OpCodes.Ldarg_S, operand: 4); // bodyfacing
                    yield return new CodeInstruction(OpCodes.Call,    AccessTools.Method(patchType, nameof(DrawAddons)));
                }

                yield return instruction;
            }
        }

        public static void DrawAddons(PawnRenderFlags renderFlags, Vector3 vector, Vector3 headOffset, Pawn pawn, Quaternion quat, Rot4 rotation)
        {
            if (pawn.def is not ThingDef_AlienRace alienProps) return;

            AlienPartGenerator.AlienComp       alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();

            if (alienComp != null)
            {
                bool isPortrait  = renderFlags.FlagSet(PawnRenderFlags.Portrait);
                bool isInvisible = renderFlags.FlagSet(PawnRenderFlags.Invisible);

                using (IEnumerator<AlienPartGenerator.BodyAddon> bodyAddons = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons.Concat(Utilities.UniversalBodyAddons).GetEnumerator())
                {
                    int addonIndex = 0;
                    while (bodyAddons.MoveNext())
                    {
                        AlienPartGenerator.BodyAddon ba = bodyAddons.Current;

                        if (ba != null && ba.CanDrawAddon(pawn))
                        {
                            Vector3 offsetVector = (ba.defaultOffsets.GetOffset(rotation)?.GetOffset(isPortrait, pawn.story.bodyType, pawn.story.headType) ?? Vector3.zero) +
                                                   (ba.offsets.GetOffset(rotation)?.GetOffset(isPortrait, pawn.story.bodyType, pawn.story.headType)        ?? Vector3.zero);

                            //Defaults for tails 
                            //south 0.42f, -0.3f, -0.22f
                            //north     0f,  0.3f, -0.55f
                            //east -0.42f, -0.3f, -0.22f   

                            offsetVector.y = ba.inFrontOfBody ? 0.3f + offsetVector.y : -0.3f - offsetVector.y;

                            float num = ba.angle;

                            if (rotation == Rot4.North)
                            {
                                if (ba.layerInvert)
                                    offsetVector.y = -offsetVector.y;
                                num = 0;
                            }

                            if (rotation == Rot4.East)
                            {
                                num            = -num; //Angle
                                offsetVector.x = -offsetVector.x;
                            }

                            Graphic addonGraphic = alienComp.addonGraphics[addonIndex];

                            addonGraphic.drawSize = (isPortrait && ba.drawSizePortrait != Vector2.zero ? ba.drawSizePortrait : ba.drawSize) *
                                                    (ba.scaleWithPawnDrawsize ?
                                                         (ba.alignWithHead ?
                                                              (isPortrait ?
                                                                   alienComp.customPortraitHeadDrawSize :
                                                                   alienComp.customHeadDrawSize) :
                                                              (isPortrait ?
                                                                   alienComp.customPortraitDrawSize :
                                                                   alienComp.customDrawSize)
                                                         ) * (ModsConfig.BiotechActive ? pawn.ageTracker.CurLifeStage.bodyWidth ?? 1.5f : 1.5f) :
                                                         Vector2.one * 1.5f);

                            Material mat = addonGraphic.MatAt(rotation);
                            if (!isPortrait && isInvisible)
                                mat = InvisibilityMatPool.GetInvisibleMat(mat);

                            DrawAddonsFinalHook(pawn, ba, rotation, ref addonGraphic, ref offsetVector, ref num, ref mat);

                            //                                                                                   Angle calculation to not pick the shortest, taken from Quaternion.Angle and modified
                            GenDraw.DrawMeshNowOrLater(
                                                       addonGraphic.MeshAt(rotation),
                                                       vector + (ba.alignWithHead ? headOffset : Vector3.zero) + offsetVector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quat)) * 2f * 57.29578f),
                                                       Quaternion.AngleAxis(num, Vector3.up) * quat, mat, renderFlags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                        addonIndex++;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DrawAddonsFinalHook(Pawn pawn, AlienPartGenerator.BodyAddon addon, Rot4 rot, ref Graphic graphic, ref Vector3 offsetVector, ref float angle, ref Material mat)
        {

        }

        public static IEnumerable<CodeInstruction> TryGetGraphicApparelTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo originalMethod = AccessTools.Method(typeof(GraphicDatabase), "Get",
                                                           new[] { typeof(string), typeof(Shader), typeof(Vector2), typeof(Color) }, new[] { typeof(Graphic_Multi) }
            );

            MethodInfo newMethod = AccessTools.Method(typeof(ApparelGraphics.ApparelGraphicUtility), nameof(ApparelGraphics.ApparelGraphicUtility.GetGraphic));

            foreach(CodeInstruction instruction in codeInstructions)
            {
                if (instruction.Calls(originalMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // apparel
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