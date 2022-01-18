namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using JetBrains.Annotations;
    using RimWorld;
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

        public static readonly AccessTools.FieldRef<Pawn_StoryTracker, string> headGraphicPath = AccessTools.FieldRefAccess<Pawn_StoryTracker, string>("headGraphicPath");

        public static readonly AccessTools.FieldRef<List<ThingStuffPair>> allApparelPairs =
            AccessTools.StaticFieldRefAccess<List<ThingStuffPair>>(AccessTools.Field(typeof(PawnApparelGenerator), "allApparelPairs"));

        public static readonly AccessTools.FieldRef<List<ThingStuffPair>> allWeaponPairs =
            AccessTools.StaticFieldRefAccess<List<ThingStuffPair>>(AccessTools.Field(typeof(PawnWeaponGenerator), "allWeaponPairs"));

        public delegate void PawnGeneratorPawnRelations(Pawn pawn, ref PawnGenerationRequest request);

        public static readonly PawnGeneratorPawnRelations generatePawnsRelations =
            AccessTools.MethodDelegate<PawnGeneratorPawnRelations>(AccessTools.Method(typeof(PawnGenerator), "GeneratePawnRelations"));

        public delegate void FoodUtilityAddThoughtsFromIdeo(HistoryEventDef eventDef, Pawn ingester, ThingDef foodDef, MeatSourceCategory meatSourceCategory);

        public static readonly FoodUtilityAddThoughtsFromIdeo foodUtilityAddThoughtsFromIdeo =
            AccessTools.MethodDelegate<FoodUtilityAddThoughtsFromIdeo>(AccessTools.Method(typeof(FoodUtility), "AddThoughtsFromIdeo"));

        public static readonly AccessTools.FieldRef<PawnTextureAtlas, Dictionary<Pawn, PawnTextureAtlasFrameSet>> PawnTextureAtlasFrameAssignments =
            AccessTools.FieldRefAccess<PawnTextureAtlas, Dictionary<Pawn, PawnTextureAtlasFrameSet>>("frameAssignments");

        public static readonly AccessTools.FieldRef<List<FoodUtility.ThoughtFromIngesting>> ingestThoughts =
            AccessTools.StaticFieldRefAccess<List<FoodUtility.ThoughtFromIngesting>>(AccessTools.Field(typeof(FoodUtility), "ingestThoughts"));
    }
}