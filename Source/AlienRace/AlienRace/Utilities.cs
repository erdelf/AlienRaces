﻿namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;
    using AlienRace.ExtendedGraphics;
    using HarmonyLib;
    using JetBrains.Annotations;
    using MonoMod.Utils;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using static AlienRace.AlienPartGenerator;

    [DefOf]
    public static class AlienDefOf
    {
        // ReSharper disable InconsistentNaming
        [Obsolete("Prefixing")]
        public static TraitDef Xenophobia;

        [Obsolete("Prefixing")]
        public static ThoughtDef XenophobiaVsAlien;

        [Obsolete("Capitalizing and prefixing")]
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
        public static bool IsGenderApplicable(this GenderPossibility possibility, Gender gender) =>
            possibility switch
            {
                GenderPossibility.Either => true,
                GenderPossibility.Male => gender   == Gender.Male,
                GenderPossibility.Female => gender == Gender.Female,
                _ => false
            };

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
                    universalBodyAddons = [.. DefDatabase<RaceSettings>.AllDefsListForReading.SelectMany(rs => rs.universalBodyAddons)];
                    universalBodyAddons.GeneBodyAddonPatcher();

                    foreach (BodyAddon bodyAddon in universalBodyAddons) 
                        bodyAddon.offsets.west ??= bodyAddon.offsets.east;

                    new DefaultGraphicsLoader().LoadAllGraphics("Universal Addons", universalBodyAddons.Cast<ExtendedGraphicTop>().ToArray());
                }

                return universalBodyAddons;
            }
        }

        public static void SetFieldFromXmlNode(Traverse field, XmlNode xmlNode)
        {
            if (!field.FieldExists())
                return;
            field.SetValue(field.GetValueType().IsGenericType || !ParseHelper.HandlesType(field.GetValueType()) ?
                               DirectXmlToObject.GetObjectFromXmlMethod(field.GetValueType())(xmlNode, false) :
                               ParseHelper.FromString(xmlNode.InnerXml.Trim(), field.GetValueType()));
        }


        public static void GeneBodyAddonPatcher(this List<AlienPartGenerator.BodyAddon> universal)
        {
            List<AlienPartGenerator.BodyAddon> geneAddons  = new();
            AlienPartGenerator                 partHandler = new();
            partHandler.GenericOffsets();

            foreach (GeneDef gene in (DefDatabase<GeneDef>.AllDefsListForReading))
            {
                if (!gene.HasModExtension<BodyAddonGene>())
                    continue;
                BodyAddonGene har = gene.GetModExtension<BodyAddonGene>();
                har.addon.geneRequirement = gene;
                har.addon.defaultOffsets  = partHandler.offsetDefaults.Find(on => on.name == har.addon.defaultOffset).offsets;
                geneAddons.Add(har.addon);

                if (!har.addons.NullOrEmpty())
                {
                    foreach (AlienPartGenerator.BodyAddon addons in har.addons)
                    {
                        addons.defaultOffsets  = partHandler.offsetDefaults.Find(on => on.name == addons.defaultOffset).offsets;
                        addons.geneRequirement = gene;
                    }

                    geneAddons.AddRange(har.addons);
                }
            }
            
            universal.AddRange(geneAddons);
        }

        public class BodyAddonGene : DefModExtension
        {
            //public bool useAutogeneratedAddon = true;
            public AlienPartGenerator.BodyAddon       addon;
            public List<AlienPartGenerator.BodyAddon> addons;
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

        public LoadDefFromField(string defName) =>
            this.defName = defName;

        public Def GetDef(Type defType) =>
            GenDefDatabase.GetDef(defType, this.defName);
    }

    public class Graphic_Multi_RotationFromData : Graphic_Multi
    {
        public override bool ShouldDrawRotated =>
            this.data?.drawRotated ?? false;
    }

    [StaticConstructorOnStartup]
    public static class CachedData
    {
        [StaticConstructorOnStartup]
        public static class Textures
        {
            public static readonly Texture2D AlienIconInactive = ContentFinder<Texture2D>.Get("AlienRace/UI/AlienIconInactive");
            public static readonly Texture2D AlienIconActive   = ContentFinder<Texture2D>.Get("AlienRace/UI/AlienIconActive");
        }

        private static readonly Dictionary<RaceProperties, ThingDef> racePropsToRaceDict = new();

        public static ThingDef GetRaceFromRaceProps(RaceProperties props)
        {
            if (!racePropsToRaceDict.ContainsKey(props))
                racePropsToRaceDict.Add(props,
                                        new List<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading).Concat(new List<ThingDef_AlienRace>(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading))
                                                                                                       .First(td => td.race == props));

            return racePropsToRaceDict[props];
        }

        private static readonly Dictionary<ApparelProperties, ThingDef> apparelPropsToApparelDict = new();

        public static ThingDef GetApparelFromApparelProps(ApparelProperties props)
        {
            if (!apparelPropsToApparelDict.ContainsKey(props))
                apparelPropsToApparelDict.Add(props, DefDatabase<ThingDef>.AllDefsListForReading.First(td => td.apparel == props));

            return apparelPropsToApparelDict[props];
        }


        public delegate bool CanBeChild(PawnKindDef kindDef);

        public static readonly CanBeChild canBeChild =
            AccessTools.MethodDelegate<CanBeChild>(AccessTools.Method(typeof(ScenarioUtility), "CanBeChild"));

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
            AccessTools.MethodDelegate<GetGraphic>(AccessTools.Method(typeof(GraphicDatabase), "GetInner", new[] { typeof(GraphicRequest) }, new[] { typeof(Graphic_Multi) }));

        public delegate void PawnMethod(Pawn pawn);

        public static readonly PawnMethod generateStartingPossessions =
            AccessTools.MethodDelegate<PawnMethod>(AccessTools.Method(typeof(StartingPawnUtility), "GeneratePossessions"));

        public static readonly AccessTools.FieldRef<Pawn_StoryTracker, Color?> skinColorBase =
            AccessTools.FieldRefAccess<Pawn_StoryTracker, Color?>(AccessTools.Field(typeof(Pawn_StoryTracker), "skinColorBase"));

        public static readonly Action<Dialog_StylingStation, Rect> drawTabs = AccessTools.Method(typeof(Dialog_StylingStation), "DrawTabs").CreateDelegate<Action<Dialog_StylingStation, Rect>>();

        public static readonly AccessTools.FieldRef<Dialog_StylingStation, Pawn> stationPawn = AccessTools.FieldRefAccess<Dialog_StylingStation, Pawn>("pawn");

        public static readonly AccessTools.FieldRef<Dialog_StylingStation, Color> stationDesiredHairColor = AccessTools.FieldRefAccess<Dialog_StylingStation, Color>("desiredHairColor");

        public static readonly AccessTools.FieldRef<Dialog_StylingStation, Dialog_StylingStation.StylingTab> stationCurTab = AccessTools.FieldRefAccess<Dialog_StylingStation, Dialog_StylingStation.StylingTab>("curTab");

        public static readonly AccessTools.FieldRef<object, bool>        statPartAgeUseBiologicalYearsField = AccessTools.FieldRefAccess<bool>(typeof(StatPart_Age), "useBiologicalYears");
        public static readonly AccessTools.FieldRef<object, SimpleCurve> statPartAgeCurveField              = AccessTools.FieldRefAccess<SimpleCurve>(typeof(StatPart_Age), "curve");
    }
}