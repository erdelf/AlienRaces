namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
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
        public static bool DifferentRace(ThingDef one, ThingDef two) =>
            one != two                                                                                                && one != null && two != null && one.race.Humanlike && two.race.Humanlike &&
            !(one is ThingDef_AlienRace oneAr && oneAr.alienRace.generalSettings.notXenophobistTowards.Contains(two)) &&
            !(two is ThingDef_AlienRace twoAr && twoAr.alienRace.generalSettings.immuneToXenophobia);

        private static List<AlienPartGenerator.BodyAddon> universalBodyAddons;

        public static List<AlienPartGenerator.BodyAddon> UniversalBodyAddons
        {
            get
            {
                if (universalBodyAddons == null)
                {
                    universalBodyAddons = new List<AlienPartGenerator.BodyAddon>();
                    universalBodyAddons.AddRange(DefDatabase<RaceSettings>.AllDefsListForReading.SelectMany(rs => rs.universalBodyAddons));
                }
                return universalBodyAddons;
            }
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
        public string defName;

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
                                        new List<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading).Concat(new List<ThingDef_AlienRace>(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading)).First(predicate: td => td.race == props));

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

        public static readonly AccessTools.FieldRef<Pawn_AgeTracker, Pawn> ageTrackerPawn =
            AccessTools.FieldRefAccess<Pawn_AgeTracker, Pawn>(AccessTools.Field(typeof(Pawn_AgeTracker), "pawn"));

        private static List<HeadTypeDef> defaultHeadTypeDefs;

        public static List<HeadTypeDef> DefaultHeadTypeDefs
        {
            get => defaultHeadTypeDefs.NullOrEmpty() ? 
                       DefaultHeadTypeDefs = DefDatabase<HeadTypeDef>.AllDefsListForReading.Where(hd => Regex.IsMatch(hd.defName, @"(?>Male|Female)_(?>Average|Narrow)(?>Normal|Wide|Pointy)")).ToList() : 
                       defaultHeadTypeDefs;
            set => defaultHeadTypeDefs = value;
        }

        public static readonly AccessTools.FieldRef<Dictionary<Type, MethodInfo>> customDataLoadMethodCacheInfo =
            AccessTools.StaticFieldRefAccess<Dictionary<Type, MethodInfo>>(AccessTools.Field(typeof(DirectXmlToObject), "customDataLoadMethodCache"));

        public delegate Graphic_Multi GetGraphic(GraphicRequest req);

        public static GetGraphic getInnerGraphic =
            AccessTools.MethodDelegate<GetGraphic>(AccessTools.Method(typeof(GraphicDatabase), "GetInner", new []{typeof(GraphicRequest)}, new []{typeof(Graphic_Multi)}));

        public delegate void PawnMethod(Pawn pawn);

        public static readonly PawnMethod generateStartingPossessions =
            AccessTools.MethodDelegate<PawnMethod>(AccessTools.Method(typeof(StartingPawnUtility), "GeneratePossessions"));

        public static readonly AccessTools.FieldRef<Pawn_StoryTracker, Color?> skinColorBase =
            AccessTools.FieldRefAccess<Pawn_StoryTracker, Color?>(AccessTools.Field(typeof(Pawn_StoryTracker), "skinColorBase"));
    }
}