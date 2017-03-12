using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.erdelf.alien_race.main");


            #region RelationSettings
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Child), "GenerationChance"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerationChanceChildPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExLover), "GenerationChance"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerationChanceExLoverPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExSpouse), "GenerationChance"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerationChanceExSpousePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Fiance), "GenerationChance"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerationChanceFiancePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), "GenerationChance"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerationChanceLoverPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Parent), "GenerationChance"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerationChanceParentPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Sibling), "GenerationChance"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerationChanceSiblingPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Spouse), "GenerationChance"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerationChanceSpousePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GeneratePawnRelations"), new HarmonyMethod(typeof(HarmonyPatches), nameof(GeneratePawnRelationsPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnRelationDef), nameof(PawnRelationDef.GetGenderSpecificLabel)), new HarmonyMethod(typeof(HarmonyPatches), nameof(GetGenderSpecificLabelPrefix)), null);
            #endregion

            #region Backstory
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "TryGiveSolidBioTo"), new HarmonyMethod(typeof(HarmonyPatches), nameof(TryGiveSolidBioToPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "SetBackstoryInSlot"), new HarmonyMethod(typeof(HarmonyPatches), nameof(SetBackstoryInSlotPrefix)), null);
            #endregion

            #region RaceRestriction
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_Researcher), "ShouldSkip"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(ShouldSkipResearchPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), "PreOpen"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(ResearchPreOpenPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GenConstruct), "CanConstruct"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(CanConstructPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GameRules), "DesignatorAllowed"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(DesignatorAllowedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Bill), "PawnAllowedToStartAnew"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnAllowedToStartAnewPostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GrowerHarvest), "HasJobOnCell"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(HasJobOnCellHarvestPostfix)));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GrowerSow), "ExtraRequirements"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(ExtraRequirementsGrowerSowPostfix)));
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHumanlikeOrdersPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.SetFaction)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(SetFactionPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Pawn.SetFactionDirect)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(SetFactionDirectPostfix)));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreGain)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(ApparelScoreGainPostFix)));
            DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.ForEach(ar =>
            {
                ar.alienRace.raceRestriction.workGiverList?.ForEach(wgd =>
                {
                    WorkGiverDef wg = DefDatabase<WorkGiverDef>.GetNamedSilentFail(wgd);
                    if (wg != null)
                    {
                        harmony.Patch(AccessTools.Method(wg.giverClass, "JobOnThing"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenericJobOnThingPostfix)));
                        MethodInfo hasJobOnThingInfo = AccessTools.Method(wg.giverClass, "HasJobOnThing");
                        if (hasJobOnThingInfo != null)
                        {
                            harmony.Patch(hasJobOnThingInfo, null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenericHasJobOnThingPostfix)));
                        }
                    }
                });
            });
            #endregion

            #region ThoughtSettings
            harmony.Patch(AccessTools.Method(typeof(ThoughtHandler), "CanGetThought"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(CanGetThoughtPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Corpse), "ButcherProducts"), new HarmonyMethod(typeof(HarmonyPatches), nameof(ButcherProductsPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "ThoughtsFromIngesting"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(ThoughtsFromIngestingPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), "TryGainMemoryThought", new Type[] { typeof(Thought_Memory), typeof(Pawn) }), new HarmonyMethod(typeof(HarmonyPatches), nameof(TryGainMemoryThoughtPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(SituationalThoughtHandler), "TryCreateSituationalThought"), new HarmonyMethod(typeof(HarmonyPatches), nameof(TryCreateSituationalThoughtPrefix)), null);            
            #endregion

            #region GeneralSettings
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("AgeInjuryUtility"), "GenerateRandomOldAgeInjuries"), new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerateRandomOldAgeInjuriesPrefix)), null);
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("AgeInjuryUtility"), "RandomHediffsToGainOnBirthday", new Type[] { typeof(ThingDef), typeof(int) }), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(RandomHediffsToGainOnBirthdayPostfix)));
            harmony.Patch(AccessTools.Property(typeof(JobDriver), "Posture").GetGetMethod(false), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(PosturePostfix)));
            harmony.Patch(AccessTools.Property(typeof(JobDriver_Skygaze), "Posture").GetGetMethod(false), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(PosturePostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateRandomAge"), new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerateRandomAgePrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateTraits"), new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerateTraitsPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(JobGiver_SatisfyChemicalNeed), "DrugValidator"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(DrugValidatorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(CompDrug), nameof(CompDrug.PostIngested)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(PostIngestedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(AddictionUtility), nameof(AddictionUtility.CanBingeOnNow)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(CanBingeNowPostfix)));
            
            #region PartGenerator
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateBodyType"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerateBodyTypePostfix)));
            harmony.Patch(AccessTools.Property(typeof(Pawn_StoryTracker), "SkinColor").GetGetMethod(), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(SkinColorPostfix)));
            #endregion
            #endregion

            harmony.Patch(AccessTools.Method(typeof(PawnHairChooser), "RandomHairDefFor"), new HarmonyMethod(typeof(HarmonyPatches), nameof(RandomHairDefForPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), "BirthdayBiological"), new HarmonyMethod(typeof(HarmonyPatches), nameof(BirthdayBiologicalPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) }), new HarmonyMethod(typeof(HarmonyPatches), nameof(GeneratePawnPrefix)), null);
            
            harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), new HarmonyMethod(typeof(HarmonyPatches), nameof(ResolveAllGraphicsPrefix)), null);
            
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool) }), new HarmonyMethod(typeof(HarmonyPatches), nameof(RenderPawnInternalPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(StartingPawnUtility), "NewGeneratedStartingPawn"), new HarmonyMethod(typeof(HarmonyPatches), nameof(NewGeneratedStartingPawnPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "GiveAppropriateBioAndNameTo"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GiveAppropriateBioAndNameToPostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "GeneratePawnName"), new HarmonyMethod(typeof(HarmonyPatches), nameof(GeneratePawnNamePrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(Page_ConfigureStartingPawns), "CanDoNext"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(CanDoNextStartPawnPostfix)));
            
            harmony.Patch(AccessTools.Method(typeof(GameInitData), "PrepForMapGen"), new HarmonyMethod(typeof(HarmonyPatches), nameof(PrepForMapGenPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryRomanceChanceFactor)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(SecondaryRomanceChanceFactorPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.CompatibilityWith)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(CompatibilityWith)));
            harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.TryMakeInitialRelationsWith)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(TryMakeInitialRelationsWithPostfix)));
            harmony.Patch(AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait)), new HarmonyMethod(typeof(HarmonyPatches), nameof(GainTraitPrefix)), null);
            harmony.Patch(AccessTools.Method(typeof(TraderCaravanUtility), nameof(TraderCaravanUtility.GetTraderCaravanRole)), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GetTraderCaravanRolePostfix)));

            #region prepareCarefully
            {
                try
                {
                    ((Action)(() =>
                    {
                        if (AccessTools.Method(typeof(EdB.PrepareCarefully.PrepareCarefully), nameof(EdB.PrepareCarefully.PrepareCarefully.Initialize)) != null)
                        {
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.CustomPawn), "CopyPawn"), null, new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullyCopyPawn"));
                            harmony.Patch(AccessTools.Property(typeof(EdB.PrepareCarefully.CustomPawn), "MelaninLevel").GetSetMethod(false), new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullyMelaninLevel"), null);
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.Page_ConfigureStartingPawnsCarefully), "DrawAppearance"), null, new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullyDrawAppearance"));
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.CustomPawn), "GetSelectedApparel"), new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullySelectedApparel"), null);
                            harmony.Patch(AccessTools.Property(typeof(EdB.PrepareCarefully.Page_ConfigureStartingPawnsCarefully), "PawnLayerLabel").GetGetMethod(true), null, new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullyPawnLayerLabel"));

                            //harmony.Patch(AccessTools.Constructor(typeof(EdB.PrepareCarefully.Page_ConfigureStartingPawnsCarefully)), null, new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullyCtor"));
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.CustomPawn), "SetSelectedStuff"), new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullySetSelectedStuff"), null);
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.CustomPawn), "GetSelectedStuff"), new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullyGetSelectedStuff"), null);
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.CustomPawn), "SetSelectedApparelInternal"), new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullySetSelectedApparelInternal"), null);
                            //harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.PawnLayers), "Label"), null, new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullyLayerLabel"));
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.CustomPawn), "ConvertToPawn", new Type[] { typeof(bool) }), null, new HarmonyMethod(typeof(HarmonyPatches), "PrepareCarefullyConvertToPawn"));
                        }
                    }))();
                }
                catch (TypeLoadException) { }
            }
            #endregion

            DefDatabase<HairDef>.GetNamed("Shaved").hairTags.Add("alienNoHair"); // needed because..... the original idea doesn't work and I spend enough time finding a good solution
        }

        public static void GetTraderCaravanRolePostfix(this Pawn p, ref TraderCaravanRole __result)
        {
            if (__result == TraderCaravanRole.Guard)
                if (p.def is ThingDef_AlienRace alienProps && alienProps.alienRace.pawnKindSettings.alienslavekinds.Any(pke => pke.kindDefs.Contains(p.kindDef.defName)))
                    __result = TraderCaravanRole.Chattel;
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
            List<Pawn> enumerable = (from x in PawnsFinder.AllMapsAndWorld_AliveOrDead
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
                    pawn.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || UnityEngine.Random.value > 0.8f ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Spouse, parent);
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
                    if (p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && UnityEngine.Random.value < 0.35)
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
                    if (p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && UnityEngine.Random.value < 1)
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
                    if (p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && UnityEngine.Random.value < 0.7)
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
                    if (p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && UnityEngine.Random.value < 0.35f)
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
                    pawn.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || UnityEngine.Random.value > 0.8f ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Spouse, parent);
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
                    parent.relations.AddDirectRelation(LovePartnerRelationUtility.HasAnyLovePartner(parent) || UnityEngine.Random.value > 0.8 ? PawnRelationDefOf.ExLover : PawnRelationDefOf.Lover, parent2);
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
                    if (pawn != p && p.relations.DirectRelations.Where(dpr => dpr.def == PawnRelationDefOf.Parent).Count() < 2 && p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, x => x == pawn) == null && UnityEngine.Random.value < 0.7)
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
            if(parent1 != null && parent2 != null && !LovePartnerRelationUtility.LovePartnerRelationExists(parent1, parent2) && !LovePartnerRelationUtility.ExLovePartnerRelationExists(parent1, parent2))
                return 0f;

            float num = 1f;
            float num2 = 1f;
            float num3 = 1f;
            Traverse childRelation = Traverse.Create(typeof(ChildRelationUtility));

            if (parent1 != null)
            {
                num = childRelation.Method("GetParentAgeFactor", parent1, child, parent1.RaceProps.lifeExpectancy/5f, parent1.RaceProps.lifeExpectancy/2.5f, parent1.RaceProps.lifeExpectancy/1.6f).GetValue<float>();
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
                if (alienProps.alienRace.generalSettings.disallowedTraits.Contains(trait.def))
                    return false;

                AlienTraitEntry ate = alienProps.alienRace.generalSettings.forcedRaceTraitEntries?.FirstOrDefault(at => at.defname.EqualsIgnoreCase(trait.def.defName));
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
            if (!__instance.HasName || !other.HasName)
            {
                return;
            }

            if (other.def.basicMemberKind?.race is ThingDef_AlienRace alienProps)
            {
                alienProps.alienRace.generalSettings.factionRelations?.ForEach(frs =>
                {
                    if (frs.factions.Contains(__instance.def.defName))
                    {
                        if (other.RelationWith(__instance, true) != null)
                        {
                            other.RelationWith(__instance, true).goodwill = frs.goodwill.RandomInRange;
                        }
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
                        if (__instance.RelationWith(other) != null)
                        {
                            __instance.RelationWith(other).goodwill = frs.goodwill.RandomInRange;
                        }
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

        public static void SecondaryRomanceChanceFactorPostfix(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result)
        {
            Traverse traverse = Traverse.Create(__instance);
            Pawn pawn = traverse.Field("pawn").GetValue<Pawn>();

            if (pawn.RaceProps.Humanlike != otherPawn.RaceProps.Humanlike || pawn == otherPawn)
            {
                __result = 0f;
                return;
            }
            Rand.PushSeed();
            Rand.Seed = pawn.HashOffset();
            bool flag = UnityEngine.Random.value < 0.015f;
            Rand.PopSeed();
            float num = 1f;
            float num2 = 1f;
            float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
            float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            if (pawn.gender == Gender.Male)
            {
                if (!flag)
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender == Gender.Female)
                        {
                            __result = 0f;
                            return;
                        }
                    }
                    else if (otherPawn.gender == Gender.Male)
                    {
                        __result = 0f;
                        return;
                    }
                }
                num2 = GenMath.FlatHill(0f, 16f, 20f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 15f, 0.07f, ageBiologicalYearsFloat2);
            }
            else if (pawn.gender == Gender.Female)
            {
                if (!flag)
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender == Gender.Male)
                        {
                            __result = 0f;
                            return;
                        }
                    }
                    else if (otherPawn.gender == Gender.Female)
                    {
                        num = 0f;
                    }
                }
                if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f)
                {
                    __result = 0f;
                    return;
                }
                if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)
                {
                    num2 = Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.2f;
                }
                else
                {
                    num2 = GenMath.FlatHill(0.2f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 30f, 0.1f, ageBiologicalYearsFloat2);
                }
            }
            float num3 = 1f;
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking));
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Manipulation));
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Moving));
            float num4 = 1f;
            foreach (PawnRelationDef current in pawn.GetRelations(otherPawn))
            {
                num4 *= current.attractionFactor;
            }
            int num5 = 0;
            if (otherPawn.RaceProps.Humanlike)
            {
                num5 = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            }
            float num6 = 1f;
            if (num5 < 0)
            {
                num6 = 0.3f;
            }
            else if (num5 > 0)
            {
                num6 = 2.3f;
            }
            float num7 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
            float num8 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
            __result = num * num2 * num3 * num4 * num7 * num8 * num6;
            return;
        }

        public static void PrepareCarefullyConvertToPawn(ref Pawn __result, object __instance, bool resolveGraphics)
        {
            Traverse traverse = Traverse.Create(__instance);
            Pawn source = traverse.Field("pawn").GetValue<Pawn>();
            AlienPartGenerator.AlienComp sourceComp = source.TryGetComp<AlienPartGenerator.AlienComp>();
            if (sourceComp != null)
            {
                AlienPartGenerator.AlienComp resultComp = __result.TryGetComp<AlienPartGenerator.AlienComp>();

                resultComp.skinColor = sourceComp.skinColor;
                resultComp.skinColorSecond = sourceComp.skinColorSecond;
                resultComp.Tail = sourceComp.Tail;
                resultComp.fixGenderPostSpawn = sourceComp.fixGenderPostSpawn;

                //if (resolveGraphics)
                    __result.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }
 
        public static void PrepareCarefullyLayerLabel(int layer, ref string __result)
        {
            if (layer == 9)
            {
                __result = "Race";
            }
        }

        public static bool PrepareCarefullySetSelectedApparelInternal(int layer, ThingDef def, object __instance)
        {
            if (layer != 9)
            {
                return true;
            }

            Traverse traversePawn = Traverse.Create(__instance);

            Pawn pawn = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.AllDefsListForReading.Where(pkd => pkd.race == def).ToList().TryRandomElement(out PawnKindDef pk) ? pk : PawnKindDefOf.Villager, Faction.OfPlayer);

            traversePawn.Method("InitializeWithPawn", pawn).GetValue();

            return false;
        }

        public static bool PrepareCarefullyGetSelectedStuff(int layer, ref ThingDef __result, object __instance)
        {
            if (layer == 9)
            {
                Traverse traversePawn = Traverse.Create(__instance);
                Pawn pawn = traversePawn.Field("pawn").GetValue<Pawn>();
                __result = pawn.def;
                return false;
            }
            return true;

        }

        public static bool PrepareCarefullySetSelectedStuff(int layer, ThingDef stuffDef, object __instance)
        {
            if (layer != 9)
            {
                return true;
            }

            Traverse traversePawn = Traverse.Create(__instance);

            Pawn pawn = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.AllDefsListForReading.Where(pkd => pkd.race == stuffDef).ToList().TryRandomElement(out PawnKindDef pk) ? pk : PawnKindDefOf.Villager, Faction.OfPlayer);

            traversePawn.Method("InitializeWithPawn", pawn).GetValue();

            return false;
        }

        public static void PrepareCarefullyPawnLayerLabel(object __instance, ref string __result)
        {
            Traverse traverseInstance = Traverse.Create(__instance);

            if (traverseInstance.Field("selectedPawnLayer").GetValue<int>() == 9)
            {
                string name = Traverse.Create(traverseInstance.Property("CurrentPawn").GetValue()).Field("pawn").GetValue<Pawn>().def.LabelCap;
                __result = name;
                traverseInstance.Field("pawnLayerLabel").SetValue(__result);
            }
        }

        public static bool PrepareCarefullySelectedApparel(int layer, object __instance, ref ThingDef __result)
        {
            if (layer == 9)
            {
                Traverse traversePawn = Traverse.Create(__instance);
                Pawn pawn = traversePawn.Field("pawn").GetValue<Pawn>();

                __result = pawn.def;
                return false;
            }
            return true;
        }

        public static void PrepareCarefullyDrawAppearance(object __instance, object customPawn)
        {
            Traverse traversePawn = Traverse.Create(customPawn);
            Pawn pawn = traversePawn.Field("pawn").GetValue<Pawn>();

            Traverse traverseInstance = Traverse.Create(__instance);
            List<Action> pawnLayerActions = traverseInstance.Field("pawnLayerActions").GetValue<List<Action>>();

            if (pawnLayerActions.Count == 9)
            {
                pawnLayerActions.Add(() => traverseInstance.Method("ChangePawnLayer", 9).GetValue());
                traverseInstance.Field("pawnLayers").GetValue<List<int>>().Add(9);

                List<ThingDef> raceList = new List<ThingDef>();
                raceList.AddRange(DefDatabase<ThingDef_AlienRace>.AllDefs.Cast<ThingDef>());
                raceList.Add(ThingDefOf.Human);

                traverseInstance.Field("apparelLists").GetValue<List<List<ThingDef>>>().Add(raceList);
            }
        }

        public static void PrepareCarefullyMelaninLevel(object __instance, float value)
        {
            Traverse traverse = Traverse.Create(__instance);
            Pawn pawn = traverse.Field("pawn").GetValue<Pawn>();
            if (pawn.def is ThingDef_AlienRace)
            {
                pawn.GetComp<AlienPartGenerator.AlienComp>().skinColor = PawnSkinColors.GetSkinColor(value);
            }
        }

        public static void PrepareCarefullyCopyPawn(Pawn source, ref Pawn __result)
        {
            AlienPartGenerator.AlienComp sourceComp = source.TryGetComp<AlienPartGenerator.AlienComp>();
            if (sourceComp != null)
            {
                AlienPartGenerator.AlienComp resultComp = __result.TryGetComp<AlienPartGenerator.AlienComp>();

                resultComp.skinColor = sourceComp.skinColor;
                resultComp.skinColorSecond = sourceComp.skinColorSecond;
                resultComp.Tail = sourceComp.Tail;
                resultComp.fixGenderPostSpawn = sourceComp.fixGenderPostSpawn;
            }
        }
    
        public static void GenericHasJobOnThingPostfix(WorkGiver __instance, Pawn pawn, ref bool __result)
        {
            if(__result)
            {
                __result = (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.workGiverList?.Any(wgd => DefDatabase<WorkGiverDef>.GetNamedSilentFail(wgd)?.giverClass == __instance.GetType()) ?? false ||
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.workGiverList?.Any(wgd => DefDatabase<WorkGiverDef>.GetNamedSilentFail(wgd)?.giverClass == __instance.GetType()) ?? false)) ;
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
                if (!((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.apparelList?.Contains(ap.def.defName) ?? false ? true : (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteApparelList?.Contains(ap.def.defName) ?? false ? true:
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
                    if (!alienProps.alienRace.generalSettings.CanLayDown && !(__instance.pawn.CurrentBed()?.def.defName.EqualsIgnoreCase("ET_Bed") ?? false))
                    {
                        __result = PawnPosture.Standing;
                    }
                }
            }
        }

        public static void PrepForMapGenPrefix(GameInitData __instance) => Find.Scenario.AllParts.Where(sp => sp is ScenPart_StartingHumanlikes).Select(sp => sp as ScenPart_StartingHumanlikes).ToList().ForEach(sp => __instance.startingPawns.AddRange(sp.GetPawns()));

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
                        Thought_Memory replaceThought = (Thought_Memory)ThoughtMaker.MakeThought(replacerThoughtDef);
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
            if(__result)
            {
                ThingDef plant = WorkGiver_Grower.CalculateWantedPlantDef((settable as Zone_Growing)?.Cells[0] ?? (settable as Thing).Position, pawn.Map);

                __result = (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.plantList?.Contains(plant.defName) ?? false ? true : (pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whitePlantList?.Contains(plant.defName) ?? false ? true :
                    (((pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyDoRaceRastrictedPlants ?? false) ? false :
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.plantList?.Contains(plant.defName) ?? false)));
            }
        }

        public static void HasJobOnCellHarvestPostfix(Pawn pawn, IntVec3 c, ref bool __result)
        {
            if(__result)
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

            if(__result)
            {
                __result = (p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.recipeList?.Contains(recipe.defName) ?? false ? true : (p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteRecipeList?.Contains(recipe.defName) ?? false ? true :
                    (((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyDoRaceRestrictedRecipes ?? false) ? false :
                    !DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => p.def != d && (d.alienRace.raceRestriction.recipeList?.Contains(recipe.defName) ?? false)));
            }
        }

        public static void DesignatorAllowedPostfix(Designator d, ref bool __result)
        {
            if (__result && d is Designator_Build)
            {
                Def toBuild = (d as Designator_Build).PlacingDef;
                IEnumerable<ThingDef_AlienRace> races = DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(ar => (ar.alienRace.raceRestriction.buildingList?.Contains(toBuild.defName) ?? false) || (ar.alienRace.raceRestriction.whiteBuildingList?.Contains(toBuild.defName) ?? false));
                if (races.Count() > 0)
                {
                    __result = races.Any(ar => Find.ColonistBar.GetColonistsInOrder().Any(p => !p.Dead && p.def == ar));
                }

                if (__result)
                {
                    if (Find.ColonistBar.GetColonistsInOrder().Where(p => !p.Dead).ToList().TrueForAll(p => ((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyBuildRaceRestrictedBuildings ?? false) && !((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.buildingList?.Contains(toBuild.defName) ?? false) && !((p.def as ThingDef_AlienRace)?.alienRace.raceRestriction.whiteBuildingList?.Contains(toBuild.defName) ?? false)))
                    {
                        __result = false;
                    }
                }
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

        public static void ResearchPreOpenPostfix(MainTabWindow_Research __instance)
        {
            List<ResearchProjectDef> projects = Traverse.Create(__instance).Field("relevantProjects").GetValue<IEnumerable<ResearchProjectDef>>().ToList();
            for(int i=0; i<projects.Count;i++)
            {
                ResearchProjectDef project = projects[i];
                if (!project.IsFinished)
                {
                    List<ThingDef_AlienRace> alienRaces =
                        DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Where(ar => ar.alienRace.raceRestriction?.researchList?.Any(rpr => rpr.projects.Contains(project.defName)) ?? false).ToList();

                    if (!alienRaces.NullOrEmpty() && !Find.ColonistBar.GetColonistsInOrder().Any(p => !p.Dead && p.def is ThingDef_AlienRace && alienRaces.Contains(p.def as ThingDef_AlienRace) && (alienRaces.First(ar => ar == p.def).alienRace.raceRestriction.researchList.First(rp => (rp.projects.Contains(project.defName)))?.apparelList?.TrueForAll(ap => p.apparel.WornApparel.Select(apd => apd.def.defName).Contains(ap)) ?? true)))
                    {
                        projects.RemoveAt(i);
                        i--;
                    }
                }
            }
            bool changed = true;

            while (changed)
            {
                changed = false;
                for (int i = 0; i < projects.Count; i++)
                {
                    if (projects[i].prerequisites != null)
                    {
                        foreach (ResearchProjectDef project in projects[i].prerequisites)
                        {
                            if (!projects.Contains(project))
                            {
                                projects.RemoveAt(i);
                                i--;
                                changed = true;
                            }
                        }
                    }
                }
            }
            
            Traverse.Create(__instance).Field("relevantProjects").SetValue(projects);
        }

        public static void ShouldSkipResearchPostfix(Pawn pawn, ref bool __result)
        {
            if (!__result)
            {
                ResearchProjectDef project = Find.ResearchManager.currentProj;

                __result = (!(pawn.def as ThingDef_AlienRace)?.alienRace.raceRestriction.researchList?.Any(rpr => rpr.projects.Contains(project.defName) && 
                (rpr.apparelList?.TrueForAll(ap => pawn.apparel.WornApparel.Select(twc => twc.def.defName).Contains(ap)) ?? false))) ?? 
                    DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d => pawn.def != d && (d.alienRace.raceRestriction.researchList?.Any(rpr => rpr.projects.Contains(project.defName)) ?? false));
            }
        }

        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing t, ref List<ThoughtDef> __result)
        {
            if (ingester.def is ThingDef_AlienRace alienProps)
            {
                if (__result.Contains(ThoughtDefOf.AteHumanlikeMeatDirect) || __result.Contains(ThoughtDefOf.AteHumanlikeMeatDirectCannibal))
                {
                    int index = __result.IndexOf(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect);
                    ThoughtDef thought = DefDatabase<ThoughtDef>.GetNamedSilentFail(alienProps.alienRace.thoughtSettings.ateThoughtSpecific?.FirstOrDefault(at => at.raceList?.Contains(t.def.ingestible.sourceDef.defName) ?? false)?.thought ?? alienProps.alienRace.thoughtSettings.ateThoughtGeneral.thought);
                    if (thought != null)
                    {
                        __result.RemoveAt(index);
                        __result.Insert(index, thought);
                    }
                }
                if (__result.Contains(ThoughtDefOf.AteHumanlikeMeatAsIngredient) || __result.Contains(ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal))
                {
                    CompIngredients compIngredients = t.TryGetComp<CompIngredients>();
                    if (compIngredients != null)
                    {
                        foreach (ThingDef ingredient in compIngredients.ingredients)
                        {
                            if (FoodUtility.IsHumanlikeMeat(ingredient))
                            {
                                int index = __result.IndexOf(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient);
                                ThoughtDef thought = DefDatabase<ThoughtDef>.GetNamedSilentFail(alienProps.alienRace.thoughtSettings.ateThoughtSpecific?.FirstOrDefault(at => at.raceList?.Contains(ingredient.ingestible.sourceDef.defName) ?? false)?.thought ?? alienProps.alienRace.thoughtSettings.ateThoughtGeneral.thought);
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
            //if (__result == 0) __result++;
            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierSpouse;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierSpouse;
            }

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceSiblingPostfix(ref float __result, Pawn generated, Pawn other)
        {
            //if (__result == 0) __result++;

            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierSibling;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierSibling;
            }

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceParentPostfix(ref float __result, Pawn generated, Pawn other)
        {
            //if (__result == 0) __result++;

            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierParent;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierParent;
            }

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            //if (__result == 0) __result++;

            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierLover;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierLover;
            }

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceFiancePostfix(ref float __result, Pawn generated, Pawn other)
        {
            //if (__result == 0) __result++;

            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierFiance;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierFiance;
            }

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceExSpousePostfix(ref float __result, Pawn generated, Pawn other)
        {
            //if (__result == 0) __result++;

            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierExSpouse;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierExSpouse;
            }

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceExLoverPostfix(ref float __result, Pawn generated, Pawn other)
        {
            //if (__result == 0) __result++;

            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierExLover;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierExLover;
            }

            if (generated == other)
            {
                __result = 0;
            }
        }

        public static void GenerationChanceChildPostfix(ref float __result, Pawn generated, Pawn other)
        {
            //if (__result == 0) __result++;

            if (generated.def is ThingDef_AlienRace)
            {
                __result *= (generated.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierChild;
            }

            if (other.def is ThingDef_AlienRace)
            {
                __result *= (other.def as ThingDef_AlienRace).alienRace.relationSettings.relationChanceModifierChild;
            }

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
                    Traverse.Create(pawn.story).Field("headGraphicPath").SetValue((pawn.def as ThingDef_AlienRace).alienRace.generalSettings.alienPartGenerator.RandomAlienHead(path, pawn.gender));
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
                    
                        butcher.needs.mood.thoughts.memories.TryGainMemoryThought(thought ?? ThoughtDefOf.ButcheredHumanlikeCorpse, null);

                    butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction).ForEach(p =>
                    {
                        if (p != butcher && p.needs != null && p.needs.mood != null && p.needs.mood.thoughts != null)
                        {
                            ThingDef_AlienRace alienPropsPawn = p.def as ThingDef_AlienRace;
                            thought = alienPropsPawn == null ? ThoughtDefOf.KnowButcheredHumanlikeCorpse : DefDatabase<ThoughtDef>.GetNamedSilentFail(alienPropsPawn.alienRace.thoughtSettings.butcherThoughtSpecific?.FirstOrDefault(bt => bt.raceList?.Contains(corpse.def.defName) ?? false)?.knowThought ?? alienPropsPawn.alienRace.thoughtSettings.butcherThoughtGeneral.knowThought);

                            p.needs.mood.thoughts.memories.TryGainMemoryThought(thought ?? ThoughtDefOf.KnowButcheredHumanlikeCorpse, null);
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
                if(equipment != null)
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
                        
                        if (!options.NullOrEmpty() && !(alienProps.alienRace.raceRestriction.weaponList?.Contains(equipment.def.defName) ?? false ||
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
                    (alienProps?.alienRace.raceRestriction.whiteApparelList?.Contains(apparel.def.defName) ?? true);

                    if (!options.NullOrEmpty() && (!restrictionsOff || DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(d =>
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

                        if (!options.NullOrEmpty() && !(alienProps.alienRace.raceRestriction.apparelList?.Contains(apparel.def.defName) ?? false ||
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

        public static void CanGetThoughtPostfix(ThoughtHandler __instance, ref bool __result, ThoughtDef def)
        {
            ThingDef_AlienRace alienProps = __instance.pawn.def as ThingDef_AlienRace;
            if (__result && alienProps != null)
            {
                if (alienProps.alienRace.thoughtSettings.cannotReceiveThoughtsAtAll)
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
            if(alienProps == null || alienProps.race.GetNameGenerator(pawn.gender) == null || style != NameStyle.Full)
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
            if (pawn.def is ThingDef_AlienRace alienProps && alienProps.alienRace.generalSettings.MaleGenderProbability != 0.5f)
            {
                if (!request.FixedGender.HasValue)
                {
                    pawn.gender = Rand.RangeInclusive(0, 100) >= alienProps.alienRace.generalSettings.MaleGenderProbability ? Gender.Female : Gender.Male;
                }
                else
                {
                    pawn.GetComp<AlienPartGenerator.AlienComp>().fixGenderPostSpawn = true;
                }
            }
        }

        public static void GiveAppropriateBioAndNameToPostfix(Pawn pawn)
        {

            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                //Log.Message(pawn.LabelCap);
                if (alienProps.alienRace.generalSettings.alienPartGenerator.alienhaircolorgen != null)
                {
                    pawn.story.hairColor = alienProps.alienRace.generalSettings.alienPartGenerator.alienhaircolorgen.NewRandomizedColor();
                }

                if (alienProps.alienRace.hairSettings.GetsGreyAt <= pawn.ageTracker.AgeBiologicalYears)
                {
                    float grey = Rand.Range(0.65f, 0.85f);
                    pawn.story.hairColor = new Color(grey, grey, grey);
                }
                if (!alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).head.NullOrEmpty())
                {
                    Traverse.Create(pawn.story).Field("headGraphicPath").SetValue(alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).head, pawn.gender));
                }
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

            PawnGenerationRequest request = new PawnGenerationRequest(kindDef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, null, true, false, false, false, true, false, 26f, false, true, true, null, null, null, null, null, null);
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
            if ((raceDef as ThingDef_AlienRace)?.alienRace.generalSettings.ImmuneToAge ?? false)
                __result = new List<HediffGiver_Birthday>();
        }

        public static bool GenerateRandomOldAgeInjuriesPrefix(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps && alienProps.alienRace.generalSettings.ImmuneToAge)
            {
                return false;
            }

            return true;
        }
        
        public static bool SetBackstoryInSlotPrefix(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
        {


            if (pawn.def is ThingDef_AlienRace alienProps && alienProps.alienRace.generalSettings.PawnsSpecificBackstories && !pawn.kindDef.backstoryCategory.NullOrEmpty())
            {
                if (BackstoryDatabase.allBackstories.Where(kvp => kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(pawn.kindDef.backstoryCategory) &&
                 kvp.Value.slot == slot && (slot != BackstorySlot.Adulthood ||
                 !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables))).Select(kvp => kvp.Value).TryRandomElement(out backstory))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryGiveSolidBioToPrefix(Pawn pawn, ref bool __result)
        {
            if(pawn.def is ThingDef_AlienRace)
            {
                __result = false;
                return false;
            }
            return true;
        }

        public static bool ResolveAllGraphicsPrefix(PawnGraphicSet __instance)
        {
            Pawn alien = __instance.pawn;
            if (__instance.pawn.def is ThingDef_AlienRace alienProps)
            {
                LifeStageDef lifeStage = alien.ageTracker.CurLifeStageRace.def;
                if (__instance.pawn.GetComp<AlienPartGenerator.AlienComp>().fixGenderPostSpawn)
                {
                    if (alienProps.alienRace.generalSettings.MaleGenderProbability != 0.5f)
                    {
                        __instance.pawn.gender = Rand.RangeInclusive(0, 100) >= alienProps.alienRace.generalSettings.MaleGenderProbability ? Gender.Female : Gender.Male;
                        __instance.pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(__instance.pawn, NameStyle.Full);
                    }

                    if (!alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).head.NullOrEmpty())
                    {
                        Traverse.Create(__instance.pawn.story).Field("headGraphicPath").SetValue(alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).head, __instance.pawn.gender));
                    }

                    __instance.pawn.GetComp<AlienPartGenerator.AlienComp>().fixGenderPostSpawn = false;
                }

                __instance.nakedGraphic = !alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).body.NullOrEmpty() ? AlienPartGenerator.GetNakedGraphic(alien.story.bodyType, ShaderDatabase.Cutout, __instance.pawn.story.SkinColor, alienProps.alienRace.generalSettings.alienPartGenerator.SkinColor(alien, false), alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).body) : null;
                __instance.rottingGraphic = !alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).body.NullOrEmpty() ? AlienPartGenerator.GetNakedGraphic(alien.story.bodyType, ShaderDatabase.Cutout, PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor, alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).body) : null;
                __instance.dessicatedGraphic = !alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).skeleton.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).skeleton, ShaderDatabase.Cutout) : null;
                __instance.headGraphic = !alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).head.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(alien.story.HeadGraphicPath, ShaderDatabase.Cutout, Vector2.one, alien.story.SkinColor) : null;
                __instance.desiccatedHeadGraphic = !alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).head.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(alien.story.HeadGraphicPath, ShaderDatabase.Cutout, Vector2.one, PawnGraphicSet.RottingColor) : null;
                __instance.skullGraphic = !alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).skull.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).skull, ShaderDatabase.Cutout, Vector2.one, Color.white) : null;
                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, alien.story.hairColor);

                alien.GetComp<AlienPartGenerator.AlienComp>().Tail = !alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).tail.NullOrEmpty() ? GraphicDatabase.Get<Graphic_Multi>(alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(alien.ageTracker.CurLifeStage).tail, ShaderDatabase.Transparent, new Vector3(1, 0, 1), alienProps.alienRace.generalSettings.alienPartGenerator.UseSkinColorForTail ? alien.story.SkinColor : alien.story.hairColor, alienProps.alienRace.generalSettings.alienPartGenerator.UseSkinColorForTail ? alien.story.SkinColor : alien.story.hairColor) : null;

                __instance.ResolveApparelGraphics();
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void GenerateTraitsPrefix(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProps && !alienProps.alienRace.generalSettings.forcedRaceTraitEntries.NullOrEmpty())
            {
                alienProps.alienRace.generalSettings.forcedRaceTraitEntries.ForEach(ate =>
                {
                    if ((pawn.gender == Gender.Male && (ate.commonalityMale == -1f || Rand.Range(0, 100) < ate.commonalityMale)) || (pawn.gender == Gender.Female && (ate.commonalityFemale == -1f || Rand.Range(0, 100) < ate.commonalityFemale)) || pawn.gender == Gender.None)
                    {
                        if (pawn.story.traits.allTraits.Any(tr => tr.def.defName.EqualsIgnoreCase(ate.defname)))
                        {
                            pawn.story.traits.GainTrait(new Trait(TraitDef.Named(ate.defname), ate.degree, true));
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
                if (!alienProps.alienRace.hairSettings.HasHair)
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
            
            if (UnityEngine.Random.value <= 0.4f)
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
            
            request = new PawnGenerationRequest(kindDef, request.Faction, request.Context, request.Map, request.ForceGenerateNewPawn, request.Newborn,
            request.AllowDead, request.AllowDead, request.CanGeneratePawnRelations, request.MustBeCapableOfViolence, request.ColonistRelationChanceFactor,
            request.ForceAddFreeWarmLayerIfNeeded, request.AllowGay, request.AllowFood, request.Validator, request.FixedBiologicalAge,
            request.FixedChronologicalAge, request.FixedGender, request.FixedMelanin, request.FixedLastName);
        }

        public static bool RenderPawnInternalPrefix(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;


            if (alienProps == null)
            {
                return true;
            }

            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;

            if (renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += 0.005f;

                if (bodyDrawType == RotDrawMode.Rotting)
                {
                    __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
                }
                else
                {
                    mesh = (portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.bodyPortraitSet.MeshAt(bodyFacing) : alienProps.alienRace.generalSettings.alienPartGenerator.bodySet.MeshAt(bodyFacing));
                }

                List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                list.ForEach(m =>
                {
                    Material damagedMat = __instance.graphics.flasher.GetDamagedMat(m);
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                    loc.y += 0.005f;
                });
                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    Vector3 drawLoc = rootLoc;
                    drawLoc.y += 0.02f;
                    Traverse.Create(__instance).Field("woundOverlays").GetValue<PawnWoundDrawer>().RenderOverBody(drawLoc, mesh, quat, portrait);
                }
            }
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.03f;
                vector.y += 0.0249999985f;
            }
            else
            {
                a.y += 0.0249999985f;
                vector.y += 0.03f;
            }
            if (__instance.graphics.headGraphic != null)
            {
                Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                Mesh mesh2 = (portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.headPortraitSet.MeshAt(headFacing) : alienProps.alienRace.generalSettings.alienPartGenerator.headSet.MeshAt(headFacing));
                Material mat = __instance.graphics.HeadMatAt(headFacing, bodyDrawType);
                GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, mat, portrait);
                Vector3 loc2 = rootLoc + b;
                loc2.y += 0.035f;
                bool flag = false;
                Mesh mesh3 = (pawn.story.crownType == CrownType.Narrow ? (portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.hairPortraitSetNarrow : alienProps.alienRace.generalSettings.alienPartGenerator.hairSetNarrow) : (portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.hairPortraitSetAverage : alienProps.alienRace.generalSettings.alienPartGenerator.hairSetAverage)).MeshAt(headFacing);
                List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                apparelGraphics.Where(apr => apr.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead).ToList().ForEach(apr =>
                {
                    flag = true;
                    Material material = apr.graphic.MatAt(bodyFacing, null);
                    material = __instance.graphics.flasher.GetDamagedMat(material);
                    GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material, portrait);
                });
                if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                {
                    Mesh mesh4 = (pawn.story.crownType == CrownType.Narrow ? (portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.hairPortraitSetNarrow : alienProps.alienRace.generalSettings.alienPartGenerator.hairSetNarrow) : (portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.hairPortraitSetAverage : alienProps.alienRace.generalSettings.alienPartGenerator.hairSetAverage)).MeshAt(headFacing);
                    Material mat2 = __instance.graphics.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat2, portrait);
                }
            }
            if (renderBody)
            {
                __instance.graphics.apparelGraphics.Where(apr => apr.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell).ToList().ForEach(apr =>
                {
                        Material material2 = apr.graphic.MatAt(bodyFacing, null);
                        material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material2, portrait);
                });


                if (pawn.GetComp<AlienPartGenerator.AlienComp>().Tail != null && alienProps.alienRace.generalSettings.alienPartGenerator.CanDrawTail(pawn))
                {
                    //mesh = MeshPool.plane10;

                    mesh = portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.tailPortraitMesh : alienProps.alienRace.generalSettings.alienPartGenerator.tailMesh;

                    float MoffsetX = 0.42f;
                    float MoffsetZ = -0.22f;
                    float MoffsetY = -0.3f;
                    float num = -40;

                    if (pawn.Rotation == Rot4.North)
                    {
                        MoffsetX = 0f;
                        MoffsetY = 0.3f;
                        MoffsetZ = -0.55f;
                        mesh = portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.tailPortraitMesh : alienProps.alienRace.generalSettings.alienPartGenerator.tailMesh;
                        num = 0;
                    }
                    else if (pawn.Rotation == Rot4.East)
                    {
                        MoffsetX = -MoffsetX;
                        num = -num + 0; //TailAngle
                        mesh = portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.tailPortraitMeshFlipped : alienProps.alienRace.generalSettings.alienPartGenerator.tailMeshFlipped;
                    }


                    Vector3 scaleVector = new Vector3(MoffsetX, MoffsetY, MoffsetZ);
                    scaleVector.x *= 1f + (1f - (portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.CustomPortraitDrawSize : alienProps.alienRace.generalSettings.alienPartGenerator.CustomDrawSize).x);
                    scaleVector.y *= 1f + (1f - (portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.CustomPortraitDrawSize : alienProps.alienRace.generalSettings.alienPartGenerator.CustomDrawSize).y);

                    Graphics.DrawMesh(mesh, pawn.DrawPos + scaleVector, Quaternion.AngleAxis(num, Vector3.up), pawn.GetComp<AlienPartGenerator.AlienComp>().Tail.MatAt(pawn.Rotation), 0);
                }
            }

            Traverse.Create(__instance).Method("DrawEquipment", new object[] { rootLoc });
            if (pawn.apparel != null)
            {
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                wornApparel.ForEach(ap =>
                {
                    ap.DrawWornExtras();
                });
            }
            Vector3 bodyLoc = rootLoc;
            bodyLoc.y += 0.0449999981f;
            Traverse.Create(__instance).Field("statusOverlays").GetValue<PawnHeadOverlays>().RenderStatusOverlays(bodyLoc, quat, portrait ? alienProps.alienRace.generalSettings.alienPartGenerator.headPortraitSet.MeshAt(headFacing) : alienProps.alienRace.generalSettings.alienPartGenerator.headSet.MeshAt(headFacing));
            return false;
        }
    }
}