namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    [DefOf]
    public static class AlienDefOf
    {
        // ReSharper disable InconsistentNaming
        public static TraitDef Xenophobia;
        public static ThoughtDef XenophobiaVsAlien;
        public static ThingCategoryDef alienCorpseCategory;

        [MayRequireIdeology]
        public static HistoryEventDef HAR_AteAlienMeat;
        [MayRequireIdeology]
        public static HistoryEventDef HAR_AteNonAlienFood;
        [MayRequireIdeology]
        public static HistoryEventDef HAR_ButcheredAlien;

        [MayRequireIdeology]
        public static HistoryEventDef HAR_AlienDating_Dating;
        [MayRequireIdeology]
        public static HistoryEventDef HAR_AlienDating_BeginRomance;
        [MayRequireIdeology]
        public static HistoryEventDef HAR_AlienDating_SharedBed;

        [MayRequireIdeology]
        public static HistoryEventDef HAR_Alien_SoldSlave;
        // ReSharper restore InconsistentNaming
    }

    public static class Utilities
    {
        public static bool DifferentRace(ThingDef one, ThingDef two)
        {
            return one != two && one != null && two != null && one.race.Humanlike && two.race.Humanlike && 
                   !(one is ThingDef_AlienRace oneAr && oneAr.alienRace.generalSettings.notXenophobistTowards.Contains(two)) &&
                   !(two is ThingDef_AlienRace twoAr && twoAr.alienRace.generalSettings.immuneToXenophobia);
        }
    }

    [UsedImplicitly]
    public class ThinkNode_ConditionalIsMemberOfRace : ThinkNode_Conditional
    {
        public List<ThingDef> races;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalIsMemberOfRace obj = (ThinkNode_ConditionalIsMemberOfRace)base.DeepCopy(resolve);
            obj.races = new List<ThingDef>(this.races);
            return obj;
        }

        protected override bool Satisfied(Pawn pawn) => 
            this.races.Contains(pawn.def);
    }

    [UsedImplicitly]
    public class Thought_XenophobeVsXenophile : Thought_SituationalSocial
    {
        public override float OpinionOffset() =>
             this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -25f : -15f;
    }

    [UsedImplicitly]
    public class ThoughtWorker_XenophobeVsXenophile : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) => 
            p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && p.story.traits.HasTrait(AlienDefOf.Xenophobia) && otherPawn.story.traits.HasTrait(AlienDefOf.Xenophobia) && 
            p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) != otherPawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) && RelationsUtility.PawnsKnowEachOther(p, otherPawn);
    }

    [UsedImplicitly]
    public class Thought_XenophobiaVsAlien : Thought_SituationalSocial
    {
        public override float OpinionOffset() => 
            this.pawn.def != this.OtherPawn().def ? this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -30 : 
            this.OtherPawn().story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -15 : 0 : 0;
    }

    [UsedImplicitly]
    public class ThoughtWorker_XenophobiaVsAlien : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p, otherPawn) &&
            !(p.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(otherPawn.def)) &&
            !(otherPawn.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.immuneToXenophobia) ?
                p.story.traits.HasTrait(AlienDefOf.Xenophobia) ?
                    p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(stageIndex: 0) :
                        ThoughtState.ActiveAtStage(stageIndex: 1) :
                    false :
                false;
    }

    [UsedImplicitly]
    public class ThoughtWorker_AlienVsXenophobia : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p, otherPawn) &&
            !(otherPawn.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(p.def)) &&
            !(p.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.immuneToXenophobia) ?
                otherPawn.story.traits.HasTrait(AlienDefOf.Xenophobia) ?
                    otherPawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(stageIndex: 0) :
                        ThoughtState.ActiveAtStage(stageIndex: 1) :
                    false :
                false;
    }

    [UsedImplicitly]
    public class ThoughtWorker_Precept_AlienRaces : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (lord.ownedPawns.Any(c => c.def != p.def))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (car.PawnsListForReading.Any(c => c.def != p.def))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def))
                        return true;
                } else if (map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c)))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [UsedImplicitly]
    public class ThoughtWorker_Precept_AlienRaces_Social : ThoughtWorker_Precept_Social
    {
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn) => 
            p.def != otherPawn.def;
    }

    public class ThoughtWorker_Precept_SlavesInColony : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (lord.ownedPawns.Any(c => c.def != p.def && c.IsSlave))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (car.PawnsListForReading.Any(c => c.def != p.def && c.IsSlave))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def && c.IsSlave))
                        return true;
                }
                else if (map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c) && c.IsSlave))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ThoughtWorker_Precept_Slavery_NoSlavesInColony : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (!lord.ownedPawns.Any(c => c.def != p.def && c.IsSlave))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (!car.PawnsListForReading.Any(c => c.def != p.def && c.IsSlave))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (!map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def && c.IsSlave))
                        return true;
                }
                else if (!map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c) && c.IsSlave))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ThoughtWorker_Precept_NoRecentAlienMeat : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            int num = Mathf.Max(0, p.GetComp<AlienPartGenerator.AlienComp>()?.lastAlienMeatIngestedTick ?? GenTicks.TicksGame);
            return Find.TickManager.TicksGame - num > 8 * GenDate.TicksPerDay;
        }

        public IEnumerable<NamedArgument> GetDescriptionArgs()
        {
            yield return 8.Named("HUMANMEATREQUIREDINTERVAL");
        }
    }

    [UsedImplicitly]
    public class Thought_XenophobeVsXenophile : Thought_SituationalSocial
    {
        public override float OpinionOffset() =>
             this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -25f : -15f;
    }

    [UsedImplicitly]
    public class ThoughtWorker_XenophobeVsXenophile : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) => 
            p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && p.story.traits.HasTrait(AlienDefOf.Xenophobia) && otherPawn.story.traits.HasTrait(AlienDefOf.Xenophobia) && 
            p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) != otherPawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) && RelationsUtility.PawnsKnowEachOther(p, otherPawn);
    }

    [UsedImplicitly]
    public class Thought_XenophobiaVsAlien : Thought_SituationalSocial
    {
        public override float OpinionOffset() => 
            this.pawn.def != this.OtherPawn().def ? this.pawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -30 : 
            this.OtherPawn().story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == 1 ? -15 : 0 : 0;
    }

    [UsedImplicitly]
    public class ThoughtWorker_XenophobiaVsAlien : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p, otherPawn) &&
            !(p.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(otherPawn.def)) &&
            !(otherPawn.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.immuneToXenophobia) ?
                p.story.traits.HasTrait(AlienDefOf.Xenophobia) ?
                    p.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(stageIndex: 0) :
                        ThoughtState.ActiveAtStage(stageIndex: 1) :
                    false :
                false;
    }

    [UsedImplicitly]
    public class ThoughtWorker_AlienVsXenophobia : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn) =>
            p.def != otherPawn.def && p.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(p, otherPawn) &&
            !(otherPawn.def is ThingDef_AlienRace par && par.alienRace.generalSettings.notXenophobistTowards.Contains(p.def)) &&
            !(p.def is ThingDef_AlienRace oar && oar.alienRace.generalSettings.immuneToXenophobia) ?
                otherPawn.story.traits.HasTrait(AlienDefOf.Xenophobia) ?
                    otherPawn.story.traits.DegreeOfTrait(AlienDefOf.Xenophobia) == -1 ?
                        ThoughtState.ActiveAtStage(stageIndex: 0) :
                        ThoughtState.ActiveAtStage(stageIndex: 1) :
                    false :
                false;
    }

    [UsedImplicitly]
    public class ThoughtWorker_Precept_AlienRaces : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (lord.ownedPawns.Any(c => c.def != p.def))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (car.PawnsListForReading.Any(c => c.def != p.def))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def))
                        return true;
                } else if (map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c)))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [UsedImplicitly]
    public class ThoughtWorker_Precept_AlienRaces_Social : ThoughtWorker_Precept_Social
    {
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn) => 
            p.def != otherPawn.def;
    }

    public class ThoughtWorker_Precept_SlavesInColony : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (lord.ownedPawns.Any(c => c.def != p.def && c.IsSlave))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (car.PawnsListForReading.Any(c => c.def != p.def && c.IsSlave))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def && c.IsSlave))
                        return true;
                }
                else if (map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c) && c.IsSlave))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ThoughtWorker_Precept_Slavery_NoSlavesInColony : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (!lord.ownedPawns.Any(c => c.def != p.def && c.IsSlave))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (!car.PawnsListForReading.Any(c => c.def != p.def && c.IsSlave))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (!map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => c.def != p.def && c.IsSlave))
                        return true;
                }
                else if (!map.mapPawns.AllPawnsSpawned.Any(c => c.def != p.def && !p.HostileTo(c) && c.IsSlave))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ThoughtWorker_Precept_NoRecentAlienMeat : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            int num = Mathf.Max(0, p.GetComp<AlienPartGenerator.AlienComp>()?.lastAlienMeatIngestedTick ?? GenTicks.TicksGame);
            return Find.TickManager.TicksGame - num > 8 * GenDate.TicksPerDay;
        }

        public IEnumerable<NamedArgument> GetDescriptionArgs()
        {
            yield return 8.Named("HUMANMEATREQUIREDINTERVAL");
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class LoadDefFromField : Attribute
    {
        private string defName;

        public LoadDefFromField(string defName)
        {
            this.defName = defName;
        }

        public Def GetDef(Type defType) => 
            GenDefDatabase.GetDef(defType, this.defName);
    }

    public class Graphic_Multi_RotationFromData : Graphic_Multi
    {
        public override bool ShouldDrawRotated => 
            this.data?.drawRotated ?? false;
    }

    public static class CachedData
    {
        private static Dictionary<RaceProperties, ThingDef> racePropsToRaceDict = new Dictionary<RaceProperties, ThingDef>();

        public static ThingDef GetRaceFromRaceProps(RaceProperties props)
        {
            if (!racePropsToRaceDict.ContainsKey(props))
                racePropsToRaceDict.Add(props,
                                        new List<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading).Concat(new List<ThingDef_AlienRace>(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading))
                                                                                                    .First(predicate: td => td.race == props));

            return racePropsToRaceDict[props];
        }

        public static readonly AccessTools.FieldRef<List<ThingStuffPair>> allApparelPairs =
            AccessTools.StaticFieldRefAccess<List<ThingStuffPair>>(AccessTools.Field(typeof(PawnApparelGenerator), "allApparelPairs"));

        public static readonly AccessTools.FieldRef<List<ThingStuffPair>> allWeaponPairs =
            AccessTools.StaticFieldRefAccess<List<ThingStuffPair>>(AccessTools.Field(typeof(PawnWeaponGenerator), "allWeaponPairs"));

        public delegate Color SwaddleColor(PawnGraphicSet graphicSet);

        public static readonly SwaddleColor swaddleColor =
            AccessTools.MethodDelegate<SwaddleColor>(AccessTools.Method(typeof(PawnGraphicSet), "SwaddleColor"));

        public delegate void PawnGeneratorPawnRelations(Pawn pawn, ref PawnGenerationRequest request);

        public static readonly PawnGeneratorPawnRelations generatePawnsRelations =
            AccessTools.MethodDelegate<PawnGeneratorPawnRelations>(AccessTools.Method(typeof(PawnGenerator), "GeneratePawnRelations"));

        public delegate void FoodUtilityAddThoughtsFromIdeo(HistoryEventDef eventDef, Pawn ingester, ThingDef foodDef, MeatSourceCategory meatSourceCategory);

        public static readonly FoodUtilityAddThoughtsFromIdeo foodUtilityAddThoughtsFromIdeo =
            AccessTools.MethodDelegate<FoodUtilityAddThoughtsFromIdeo>(AccessTools.Method(typeof(FoodUtility), "AddThoughtsFromIdeo"));

        public static readonly AccessTools.FieldRef<PawnTextureAtlas, Dictionary<Pawn, PawnTextureAtlasFrameSet>> pawnTextureAtlasFrameAssignments =
            AccessTools.FieldRefAccess<PawnTextureAtlas, Dictionary<Pawn, PawnTextureAtlasFrameSet>>("frameAssignments");

        public static readonly AccessTools.FieldRef<List<FoodUtility.ThoughtFromIngesting>> ingestThoughts =
            AccessTools.StaticFieldRefAccess<List<FoodUtility.ThoughtFromIngesting>>(AccessTools.Field(typeof(FoodUtility), "ingestThoughts"));

        public static readonly AccessTools.FieldRef<Pawn_StoryTracker, Color> hairColor =
            AccessTools.FieldRefAccess<Pawn_StoryTracker, Color>(AccessTools.Field(typeof(Pawn_StoryTracker), "hairColor"));
    }
}