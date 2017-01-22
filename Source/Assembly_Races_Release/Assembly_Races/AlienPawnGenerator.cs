using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public static class AlienPawnGenerator
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct PawnGenerationStatus
        {

            public Pawn Pawn
            {
                get;
                private set;
            }

            public List<Pawn> PawnsGeneratedInTheMeantime
            {
                get;
                private set;
            }

            public PawnGenerationStatus(Pawn pawn, List<Pawn> pawnsGeneratedInTheMeantime)
            {
                this.Pawn = pawn;
                this.PawnsGeneratedInTheMeantime = pawnsGeneratedInTheMeantime;
            }
        }

        public const int MaxStartMentalStateThreshold = 40;

        private static List<AlienPawnGenerator.PawnGenerationStatus> pawnsBeingGenerated = new List<AlienPawnGenerator.PawnGenerationStatus>();

        private static SimpleCurve DefaultAgeGenerationCurve = new SimpleCurve
        {
            new CurvePoint(0.05f, 0f),
            new CurvePoint(0.1f, 100f),
            new CurvePoint(0.675f, 100f),
            new CurvePoint(0.75f, 30f),
            new CurvePoint(0.875f, 18f),
            new CurvePoint(1f, 10f),
            new CurvePoint(1.125f, 3f),
            new CurvePoint(1.25f, 0f)
        };

        public static Pawn GeneratePawn(PawnKindDef kindDef, Faction faction = null)
        {
            return GeneratePawn(new PawnGenerationRequest(kindDef, faction, PawnGenerationContext.NonPlayer, null, false, false, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null));
        }

        public static Pawn GeneratePawn(PawnGenerationRequest request)
        {
            //Log.Message("0");
            if (request.KindDef != null && request.KindDef == PawnKindDefOf.SpaceRefugee && Rand.Value > 0.6f)
                request.KindDef.race = DefDatabase<Thingdef_AlienRace>.AllDefs.Where((Thingdef_AlienRace x) => !x.defName.Contains("Base")).RandomElement();

            //typeof(PawnGenerationRequest).GetProperty("KindDef", System.Reflection.BindingFlags.Instance).GetSetMethod(false).Invoke(request, new object[] { PawnKindDef.Named("OrassanVillager") });
            //Log.Message("1");
            request.EnsureNonNullFaction();
            Pawn pawn = null;
            if (!request.Newborn && !request.ForceGenerateNewPawn && Rand.Value < AlienPawnGenerator.ChanceToRedressAnyWorldPawn())
            {
                IEnumerable<Pawn> enumerable = Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.Free);
                if (request.KindDef.factionLeader)
                {
                    enumerable = enumerable.Concat(Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.FactionLeader));
                }
                enumerable = from x in enumerable
                             where AlienPawnGenerator.IsValidCandidateToRedress(x, request)
                             select x;
                if (enumerable.TryRandomElementByWeight((Pawn x) => AlienPawnGenerator.WorldPawnSelectionWeight(x), out pawn))
                {
                    Verse.PawnGenerator.RedressPawn(pawn, request);
                    Find.WorldPawns.RemovePawn(pawn);
                }
            }
            //Log.Message("2");
            if (pawn == null)
            {
                //Log.Message("2.1");
                pawn = AlienPawnGenerator.GenerateNewNakedPawn(ref request);
                if (pawn == null)
                {
                    return null;
                }



                //Log.Message("2.2");
                if (!request.Newborn)
                {
                    if (pawn.story != null && pawn.story.bodyType == BodyType.Undefined)
                    {
                        
                        if (pawn.story.adulthood != null)
                            pawn.story.bodyType = pawn.story.adulthood.BodyTypeFor(pawn.gender);
                        
                        if (pawn.story.bodyType == BodyType.Undefined && pawn.story.adulthood != null)
                            pawn.story.bodyType = pawn.story.childhood.BodyTypeFor(pawn.gender);
                        

                        if(pawn.story.bodyType == BodyType.Undefined)
                            pawn.story.bodyType = pawn.gender == Gender.Male ? BodyType.Male : BodyType.Female;

                        if (pawn.story.bodyType == BodyType.Undefined)
                            pawn.story.bodyType = BodyType.Thin;

                        Thingdef_AlienRace aliendef = pawn.def as Thingdef_AlienRace;
                        /*
                        Log.Message("START DEBUG");
                        Log.Message((aliendef != null).ToString());
                        Log.Message((aliendef.alienpartgenerator != null).ToString());
                        Log.Message((aliendef.alienpartgenerator.alienbodytypes.NullOrEmpty().ToString()));
                        Log.Message((aliendef.alienpartgenerator.alienbodytypes.Count.ToString()));
                        aliendef.alienpartgenerator.alienbodytypes.ForEach((BodyType t) => Log.Message(t.ToString()));

                        Log.Message(aliendef.alienpartgenerator.alienbodytypes.Contains(pawn.story.bodyType).ToString());
                        */
                        if (aliendef != null && !aliendef.alienpartgenerator.alienbodytypes.NullOrEmpty() && !aliendef.alienpartgenerator.alienbodytypes.Contains(pawn.story.bodyType))
                            pawn.story.bodyType = aliendef.alienpartgenerator.alienbodytypes.RandomElement();
                    }
                    
                    AlienPawnGenerator.GenerateGearFor(pawn, request);
                    
                }
            }
            //Log.Message("3");
            if (Find.Scenario != null)
            {
                Find.Scenario.Notify_PawnGenerated(pawn, request.Context);
            }
            //Log.Message("4");

            if (pawn.kindDef.race.ToString().Contains("Alien_") && !(pawn is AlienPawn))
                pawn = AlienPawn.GeneratePawn(pawn);

            //Log.Message(((pawn is AlienPawn) ? "Alien " : "") + "Pawn " + (pawn.NameStringShort ?? "") + " generated.");
            return (pawn as AlienPawn) ?? pawn;
        }


        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return false;
                }
                toCheck = toCheck.BaseType;
            }
            return true;
        }



        public static void RedressPawn(Pawn pawn, PawnGenerationRequest request)
        {
            pawn.kindDef = request.KindDef;
            AlienPawnGenerator.GenerateGearFor(pawn, request);
        }

        public static bool IsBeingGenerated(Pawn pawn)
        {
            return AlienPawnGenerator.pawnsBeingGenerated.Any((AlienPawnGenerator.PawnGenerationStatus x) => x.Pawn == pawn);
        }

        private static bool IsValidCandidateToRedress(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn.def != request.KindDef.race)
            {
                return false;
            }
            if (pawn.Faction != request.Faction)
            {
                return false;
            }
            if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
            {
                return false;
            }
            if (!request.AllowDowned && pawn.Downed)
            {
                return false;
            }
            if (pawn.health.hediffSet.BleedRateTotal > 0.001f)
            {
                return false;
            }
            if (!request.CanGeneratePawnRelations && pawn.RaceProps.IsFlesh && pawn.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
            {
                return false;
            }
            if (!request.AllowGay && pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
            {
                return false;
            }
            if (request.Validator != null && !request.Validator(pawn))
            {
                return false;
            }
            if (request.FixedBiologicalAge.HasValue && pawn.ageTracker.AgeBiologicalYearsFloat != request.FixedBiologicalAge)
            {
                return false;
            }
            if (request.FixedChronologicalAge.HasValue && (float)pawn.ageTracker.AgeChronologicalYears != request.FixedChronologicalAge)
            {
                return false;
            }
            if (request.FixedGender.HasValue && pawn.gender != request.FixedGender)
            {
                return false;
            }
            if (request.FixedLastName != null && ((NameTriple)pawn.Name).Last != request.FixedLastName)
            {
                return false;
            }
            if (request.FixedMelanin.HasValue && pawn.story != null && pawn.story.melanin != request.FixedMelanin)
            {
                return false;
            }
            if (request.MustBeCapableOfViolence)
            {
                if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    return false;
                }
                if (pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    return false;
                }
            }
            return true;
        }

        private static Pawn GenerateNewNakedPawn(ref PawnGenerationRequest request)
        {
            Pawn pawn = null;
            string text = null;
            bool ignoreScenarioRequirements = false;
            for (int i = 0; i < 100; i++)
            {
                if (i == 70)
                {
                    Log.Error(string.Concat(new object[]
                    {
                        "Could not generate a pawn after ",
                        70,
                        " tries. Last error: ",
                        text,
                        " Ignoring scenario requirements."
                    }));
                    ignoreScenarioRequirements = true;
                }
                PawnGenerationRequest pawnGenerationRequest = request;
                pawn = AlienPawnGenerator.DoGenerateNewNakedPawn(ref pawnGenerationRequest, out text, ignoreScenarioRequirements);
                if (pawn != null)
                {
                    request = pawnGenerationRequest;
                    break;
                }
            }
            if (pawn == null)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Pawn generation error: ",
                    text,
                    " Too many tries (",
                    100,
                    "), returning null. Generation request: ",
                    request
                }));
                return null;
            }
            return pawn;
        }

        private static Pawn DoGenerateNewNakedPawn(ref PawnGenerationRequest request, out string error, bool ignoreScenarioRequirements)
        {
            error = null;
            Pawn pawn = (Pawn)ThingMaker.MakeThing(request.KindDef.race, null);
            AlienPawnGenerator.pawnsBeingGenerated.Add(new PawnGenerationStatus(pawn, null));
            Pawn result;
            try
            {
                pawn.kindDef = request.KindDef;
                pawn.SetFactionDirect(request.Faction);
                PawnComponentsUtility.CreateInitialComponents(pawn);
                if (request.FixedGender.HasValue)
                {
                    pawn.gender = request.FixedGender.Value;
                }
                else if (pawn.RaceProps.hasGenders)
                {
                    if (Rand.Value < 0.5f)
                    {
                        pawn.gender = Gender.Male;
                    }
                    else
                    {
                        pawn.gender = Gender.Female;
                    }
                }
                else
                {
                    pawn.gender = Gender.None;
                }
                AlienPawnGenerator.GenerateRandomAge(pawn, request);
                pawn.needs.SetInitialLevels();
                if (!request.Newborn && request.CanGeneratePawnRelations)
                {
                    AlienPawnGenerator.GeneratePawnRelations(pawn, ref request);
                }
                if (pawn.RaceProps.Humanlike)
                {
                    pawn.story.melanin = ((!request.FixedMelanin.HasValue) ? PawnSkinColors.RandomMelanin() : request.FixedMelanin.Value);
                    pawn.story.crownType = ((Rand.Value >= 0.5f) ? CrownType.Narrow : CrownType.Average);
                    pawn.story.hairColor = PawnHairColors.RandomHairColor(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
                    PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo(pawn, request.FixedLastName);
                    pawn.story.hairDef = PawnHairChooser.RandomHairDefFor(pawn, request.Faction.def);
                    //Log.Message("hey");
                    typeof(PawnGenerator).GetMethod("GenerateTraits", BindingFlags.NonPublic|BindingFlags.Static).Invoke(null, new object[] { pawn, request.AllowGay });
                    //Log.Message("hey");
                    if (pawn.kindDef.race.ToString().Contains("Alien_"))
                    {
                        pawn = AlienPawn.GeneratePawn(pawn);
                        pawn.Name = (NameTriple) typeof(PawnBioAndNameGenerator).GetMethod("GeneratePawnName_Shuffled", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { pawn, request.FixedLastName });
                    }
                    GenerateSkills(pawn);
                }
                typeof(PawnGenerator).GetMethod("GenerateInitialHediffs", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { pawn, request});
                if (pawn.workSettings != null && request.Faction.IsPlayer)
                {
                    pawn.workSettings.EnableAndInitialize();
                }
                if (request.Faction != null && pawn.RaceProps.Animal)
                {
                    pawn.GenerateNecessaryName();
                }
                if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
                {
                    AlienPawnGenerator.DiscardGeneratedPawn(pawn);
                    error = "Generated dead pawn.";
                    result = null;
                }
                else if (!request.AllowDowned && pawn.Downed)
                {
                    AlienPawnGenerator.DiscardGeneratedPawn(pawn);
                    error = "Generated downed pawn.";
                    result = null;
                }
                else if (request.MustBeCapableOfViolence && pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    AlienPawnGenerator.DiscardGeneratedPawn(pawn);
                    error = "Generated pawn incapable of violence.";
                    result = null;
                }
                else if (!ignoreScenarioRequirements && request.Context == PawnGenerationContext.PlayerStarter && !Find.Scenario.AllowPlayerStartingPawn(pawn))
                {
                    AlienPawnGenerator.DiscardGeneratedPawn(pawn);
                    error = "Generated pawn doesn't meet scenario requirements.";
                    result = null;
                }
                else if (request.Validator != null && !request.Validator(pawn))
                {
                    AlienPawnGenerator.DiscardGeneratedPawn(pawn);
                    error = "Generated pawn didn't pass validator check.";
                    result = null;
                }
                else
                {
                    for (int i = 0; i < AlienPawnGenerator.pawnsBeingGenerated.Count - 1; i++)
                    {
                        if (AlienPawnGenerator.pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime == null)
                        {
                            AlienPawnGenerator.pawnsBeingGenerated[i] = new AlienPawnGenerator.PawnGenerationStatus(AlienPawnGenerator.pawnsBeingGenerated[i].Pawn, new List<Pawn>());
                        }
                        AlienPawnGenerator.pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime.Add(pawn);
                    }
                    result = pawn;
                }
            }
            finally
            {
                AlienPawnGenerator.pawnsBeingGenerated.RemoveLast<AlienPawnGenerator.PawnGenerationStatus>();
            }
            //Log.Message("RELATION: " + (result.relations != null).ToString());
            return result;
        }

        private static void GenerateSkills(Pawn pawn)
        {
            //Log.Message("Backstory Count: " + pawn.story.AllBackstories.Count().ToString());
            //Log.Message(pawn.story.GetBackstory(BackstorySlot.Childhood).Title);
            //Log.Message(pawn.story.GetBackstory(BackstorySlot.Adulthood)?.Title);
            


            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                SkillDef skillDef = allDefsListForReading[i];
                int num = FinalLevelOfSkill(pawn, skillDef);
                SkillRecord skill = pawn.skills.GetSkill(skillDef);
                skill.Level = num;
                if (!skill.TotallyDisabled)
                {
                    float num2 = (float)num * 0.11f;
                    float value = Rand.Value;
                    if (value < num2)
                    {
                        if (value < num2 * 0.2f)
                        {
                            skill.passion = Passion.Major;
                        }
                        else
                        {
                            skill.passion = Passion.Minor;
                        }
                    }
                    skill.xpSinceLastLevel = Rand.Range(skill.XpRequiredForLevelUp * 0.1f, skill.XpRequiredForLevelUp * 0.9f);
                }
            }
        }

        private static void DiscardGeneratedPawn(Pawn pawn)
        {
            if (Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            List<Pawn> pawnsGeneratedInTheMeantime = AlienPawnGenerator.pawnsBeingGenerated.Last<AlienPawnGenerator.PawnGenerationStatus>().PawnsGeneratedInTheMeantime;
            if (pawnsGeneratedInTheMeantime != null)
            {
                for (int i = 0; i < pawnsGeneratedInTheMeantime.Count; i++)
                {
                    Pawn pawn2 = pawnsGeneratedInTheMeantime[i];
                    if (Find.WorldPawns.Contains(pawn2))
                    {
                        Find.WorldPawns.RemovePawn(pawn2);
                    }
                    Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
                    for (int j = 0; j < AlienPawnGenerator.pawnsBeingGenerated.Count; j++)
                    {
                        AlienPawnGenerator.pawnsBeingGenerated[j].PawnsGeneratedInTheMeantime.Remove(pawn2);
                    }
                }
            }
        }

        private static float ChanceToRedressAnyWorldPawn()
        {
            int pawnsBySituationCount = Find.WorldPawns.GetPawnsBySituationCount(WorldPawnSituation.Free);
            return Mathf.Min(0.02f + 0.01f * ((float)pawnsBySituationCount / 25f), 0.8f);
        }

        private static float WorldPawnSelectionWeight(Pawn p)
        {
            if (p.RaceProps.IsFlesh && !p.relations.everSeenByPlayer && p.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
            {
                return 0.1f;
            }
            return 1f;
        }

        private static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
        {
            ProfilerThreadCheck.BeginSample("GenerateGearFor");
            ProfilerThreadCheck.BeginSample("GenerateStartingApparelFor");
            PawnApparelGenerator.GenerateStartingApparelFor(pawn, request);
            ProfilerThreadCheck.EndSample();
            ProfilerThreadCheck.BeginSample("TryGenerateWeaponFor");
            PawnWeaponGenerator.TryGenerateWeaponFor(pawn);
            ProfilerThreadCheck.EndSample();
            ProfilerThreadCheck.BeginSample("GenerateInventoryFor");
            PawnInventoryGenerator.GenerateInventoryFor(pawn, request);
            ProfilerThreadCheck.EndSample();
            ProfilerThreadCheck.EndSample();
        }

        private static void GenerateRandomAge(Pawn pawn, PawnGenerationRequest request)
        {
            if (request.FixedBiologicalAge.HasValue && request.FixedChronologicalAge.HasValue)
            {
                float? fixedBiologicalAge = request.FixedBiologicalAge;
                bool arg_67_0;
                if (fixedBiologicalAge.HasValue)
                {
                    float? fixedChronologicalAge = request.FixedChronologicalAge;
                    if (fixedChronologicalAge.HasValue)
                    {
                        arg_67_0 = (fixedBiologicalAge.Value > fixedChronologicalAge.Value);
                        goto IL_67;
                    }
                }
                arg_67_0 = false;
            IL_67:
                if (arg_67_0)
                {
                    Log.Warning(string.Concat(new object[]
                    {
                        "Tried to generate age for pawn ",
                        pawn,
                        ", but pawn generation request demands biological age (",
                        request.FixedBiologicalAge,
                        ") to be greater than chronological age (",
                        request.FixedChronologicalAge,
                        ")."
                    }));
                }
            }
            if (request.Newborn)
            {
                pawn.ageTracker.AgeBiologicalTicks = 0L;
            }
            else if (request.FixedBiologicalAge.HasValue)
            {
                pawn.ageTracker.AgeBiologicalTicks = (long)(request.FixedBiologicalAge.Value * 3600000f);
            }
            else
            {
                int num = 0;
                float num2;
                while (true)
                {
                    if (pawn.RaceProps.ageGenerationCurve != null)
                    {
                        num2 = (float)Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve, 200));
                    }
                    else if (pawn.RaceProps.IsMechanoid)
                    {
                        num2 = (float)Rand.Range(0, 2500);
                    }
                    else
                    {
                        num2 = Rand.ByCurve(AlienPawnGenerator.DefaultAgeGenerationCurve, 200) * pawn.RaceProps.lifeExpectancy;
                    }
                    num++;
                    if (num > 300)
                    {
                        break;
                    }
                    if (num2 <= (float)pawn.kindDef.maxGenerationAge && num2 >= (float)pawn.kindDef.minGenerationAge)
                    {
                        goto IL_1D7;
                    }
                }
                Log.Error("Tried 300 times to generate age for " + pawn);
            IL_1D7:
                pawn.ageTracker.AgeBiologicalTicks = (long)(num2 * 3600000f) + (long)Rand.Range(0, 3600000);
            }
            if (request.Newborn)
            {
                pawn.ageTracker.AgeChronologicalTicks = 0L;
            }
            else if (request.FixedChronologicalAge.HasValue)
            {
                pawn.ageTracker.AgeChronologicalTicks = (long)(request.FixedChronologicalAge.Value * 3600000f);
            }
            else
            {
                int num3;
                if (Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
                {
                    float value = Rand.Value;
                    if (value < 0.7f)
                    {
                        num3 = Rand.Range(0, 100);
                    }
                    else if (value < 0.95f)
                    {
                        num3 = Rand.Range(100, 1000);
                    }
                    else
                    {
                        int max = GenDate.DefaultStartingYear + GenDate.YearsPassed - 2026 - pawn.ageTracker.AgeBiologicalYears;
                        num3 = Rand.Range(1000, max);
                    }
                }
                else
                {
                    num3 = 0;
                }
                int ticksAbs = GenTicks.TicksAbs;
                long num4 = (long)ticksAbs - pawn.ageTracker.AgeBiologicalTicks;
                num4 -= (long)num3 * 3600000L;
                pawn.ageTracker.BirthAbsTicks = num4;
            }
            if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
            {
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
            }
        }

        /*
        private static void GiveRandomTraits(Pawn pawn, bool allowGay)
        {
            if (pawn.story == null)
            {
                return;
            }
            if (pawn.story.childhood.forcedTraits != null)
            {
                List<TraitEntry> forcedTraits = pawn.story.childhood.forcedTraits;
                for (int i = 0; i < forcedTraits.Count; i++)
                {
                    TraitEntry traitEntry = forcedTraits[i];
                    if (traitEntry.def == null)
                    {
                        Log.Error("Null forced trait def on " + pawn.story.childhood);
                    }
                    else if (!pawn.story.traits.HasTrait(traitEntry.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree));
                    }
                }
            }

            if (pawn.story.adulthood?.forcedTraits != null)
            {
                List<TraitEntry> forcedTraits2 = pawn.story.adulthood.forcedTraits;
                for (int j = 0; j < forcedTraits2.Count; j++)
                {
                    TraitEntry traitEntry2 = forcedTraits2[j];
                    if (traitEntry2.def == null)
                    {
                        Log.Error("Null forced trait def on " + pawn.story.adulthood);
                    }
                    else if (!pawn.story.traits.HasTrait(traitEntry2.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(traitEntry2.def, traitEntry2.degree));
                    }
                }
            }
            int num = Rand.RangeInclusive(2, 3);
            if (allowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
            {
                Trait trait = new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay));
                pawn.story.traits.GainTrait(trait);
            }
            while (pawn.story.traits.allTraits.Count < num)
            {
                TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn));
                if (!pawn.story.traits.HasTrait(newTraitDef))
                {
                    if (newTraitDef == TraitDefOf.Gay)
                    {
                        if (!allowGay)
                        {
                            continue;
                        }
                        if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                        {
                            continue;
                        }
                    }
                    if (!pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) && (newTraitDef.conflictingTraits == null || !newTraitDef.conflictingTraits.Any((TraitDef tr) => pawn.story.traits.HasTrait(tr))))
                    {
                        if (newTraitDef.requiredWorkTypes == null || !pawn.story.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes))
                        {
                            if (!pawn.story.WorkTagIsDisabled(newTraitDef.requiredWorkTags))
                            {
                                int degree = PawnGenerator.RandomTraitDegree(newTraitDef);
                                if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && (pawn.story.adulthood==null || !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree)))
                                {
                                    Trait trait2 = new Trait(newTraitDef, degree);
                                    if (pawn.mindState == null || pawn.mindState.mentalBreaker == null || pawn.mindState.mentalBreaker.BreakThresholdExtreme + trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold) <= 40f)
                                    {
                                        pawn.story.traits.GainTrait(trait2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }*/

        public static void PostProcessGeneratedGear(Thing gear, Pawn pawn)
        {
            CompQuality compQuality = gear.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(QualityUtility.RandomGeneratedGearQuality(pawn.kindDef), ArtGenerationContext.Outsider);
            }
            if (gear.def.useHitPoints)
            {
                float randomInRange = pawn.kindDef.gearHealthRange.RandomInRange;
                if (randomInRange < 1f)
                {
                    int num = Mathf.RoundToInt(randomInRange * (float)gear.MaxHitPoints);
                    num = Mathf.Max(1, num);
                    gear.HitPoints = num;
                }
            }
        }

        private static void GeneratePawnRelations(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                return;
            }
            List<KeyValuePair<Pawn, PawnRelationDef>> list = new List<KeyValuePair<Pawn, PawnRelationDef>>();
            List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
            IEnumerable<Pawn> enumerable = from x in Find.WorldPawns.AllPawnsAliveOrDead
                                           where x.def == pawn.def
                                           select x;
            foreach (Pawn current in enumerable)
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
                    for (int i = 0; i < allDefsListForReading.Count; i++)
                    {
                        if (allDefsListForReading[i].generationChanceFactor > 0f)
                        {
                            list.Add(new KeyValuePair<Pawn, PawnRelationDef>(current, allDefsListForReading[i]));
                        }
                    }
                }
            }
            PawnGenerationRequest localReq = request;
            KeyValuePair<Pawn, PawnRelationDef> keyValuePair = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<Pawn, PawnRelationDef> x)
            {
                if (!x.Value.familyByBloodRelation)
                {
                    return 0f;
                }
                return x.Value.generationChanceFactor * x.Value.Worker.GenerationChance(pawn, x.Key, localReq);
            }, 82f);
            if (keyValuePair.Key != null)
            {
                keyValuePair.Value.Worker.CreateRelation(pawn, keyValuePair.Key, ref request);
            }
            KeyValuePair<Pawn, PawnRelationDef> keyValuePair2 = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<Pawn, PawnRelationDef> x)
            {
                if (x.Value.familyByBloodRelation)
                {
                    return 0f;
                }
                return x.Value.generationChanceFactor * x.Value.Worker.GenerationChance(pawn, x.Key, localReq);
            }, 82f);
            if (keyValuePair2.Key != null)
            {
                keyValuePair2.Value.Worker.CreateRelation(pawn, keyValuePair2.Key, ref request);
            }
        }

        private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.5f, 150f),
            new CurvePoint(4f, 150f),
            new CurvePoint(5f, 25f),
            new CurvePoint(10f, 5f),
            new CurvePoint(15f, 0f)
        };

        private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(10f, 10f),
            new CurvePoint(20f, 16f),
            new CurvePoint(27f, 20f)
        };

        private static readonly SimpleCurve AgeSkillMaxFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(10f, 0.7f),
            new CurvePoint(35f, 1f),
            new CurvePoint(60f, 1.6f)
        };


        private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float num;
            if (sk.usuallyDefinedInBackstories)
            {
                num = (float)Rand.RangeInclusive(0, 4);
            }
            else
            {
                num = Rand.ByCurve(LevelRandomCurve, 100);
            }

            foreach (Backstory current in from bs in pawn.story.AllBackstories
                                          where bs != null
                                          select bs)
            {
                foreach (KeyValuePair<SkillDef, int> current2 in current.skillGainsResolved)
                {
                    if (current2.Key == sk)
                    {
                        num += (float)current2.Value * Rand.Range(1f, 1.4f);
                    }
                }
            }
            for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
            {
                int num2 = 0;
                if (pawn.story.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out num2))
                {
                    num += (float)num2;
                }
            }
            float num3 = Rand.Range(1f, AgeSkillMaxFactorCurve.Evaluate((float)pawn.ageTracker.AgeBiologicalYears));
            num *= num3;
            num = LevelFinalAdjustmentCurve.Evaluate(num);
            return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
        }
    }
}