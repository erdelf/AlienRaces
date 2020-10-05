using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace AlienRace
{
    using System;
    using System.Collections;
    using System.Reflection;
    using HarmonyLib;

    public class ThingDef_AlienRace : ThingDef
    {
        public AlienSettings alienRace;

        public override void ResolveReferences()
        {
            this.comps.Add(item: new CompProperties(compClass: typeof(AlienPartGenerator.AlienComp)));
            base.ResolveReferences();

            if (this.alienRace.graphicPaths.NullOrEmpty())
                this.alienRace.graphicPaths.Add(item: new GraphicPaths());

            if (this.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize == Vector2.zero)
                this.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customDrawSize;
            if (this.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize == Vector2.zero)
                this.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize;

            this.alienRace.graphicPaths.ForEach(action: gp =>
            {
                if(gp.customDrawSize == Vector2.one)
                    gp.customDrawSize = this.alienRace.generalSettings.alienPartGenerator.customDrawSize;
                if (gp.customPortraitDrawSize == Vector2.one)
                    gp.customPortraitDrawSize = this.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize;
                if (gp.customHeadDrawSize == Vector2.zero)
                    gp.customHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize;
                if (gp.customPortraitHeadDrawSize == Vector2.zero)
                    gp.customPortraitHeadDrawSize = this.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize;
                if (gp.headOffset == Vector2.zero)
                    gp.headOffset = this.alienRace.generalSettings.alienPartGenerator.headOffset;
                if (gp.headOffsetDirectional == null)
                    gp.headOffsetDirectional = this.alienRace.generalSettings.alienPartGenerator.headOffsetDirectional;
            });
            this.alienRace.generalSettings.alienPartGenerator.alienProps = this;
            foreach (AlienPartGenerator.BodyAddon bodyAddon in this.alienRace.generalSettings.alienPartGenerator.bodyAddons)
                if (bodyAddon.offsets.west == null)
                    bodyAddon.offsets.west = bodyAddon.offsets.east;

            if (this.alienRace.generalSettings.minAgeForAdulthood < 0)
                this.alienRace.generalSettings.minAgeForAdulthood = (float) AccessTools.Field(typeof(PawnBioAndNameGenerator), "MinAgeForAdulthood").GetValue(null);

            void RecursiveAttributeCheck(Type type, Traverse instance)
            {
                if (type == typeof(ThingDef_AlienRace))
                    return;

                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    Traverse instanceNew = instance.Field(field.Name);

                    if (typeof(IList).IsAssignableFrom(field.FieldType))
                    {
                        object value = instanceNew.GetValue();
                        if (value != null)
                            foreach (object o in (IList) value)
                                RecursiveAttributeCheck(o.GetType(), Traverse.Create(o));
                    }

                    if (field.FieldType.Assembly == typeof(ThingDef_AlienRace).Assembly) 
                        RecursiveAttributeCheck(field.FieldType, instanceNew);

                    LoadDefFromField attribute = field.GetCustomAttribute<LoadDefFromField>();
                    if (attribute != null)
                        if (instanceNew.GetValue() == null)
                            instanceNew.SetValue(attribute.GetDef);
                }
            }
            RecursiveAttributeCheck(typeof(AlienSettings), Traverse.Create(this.alienRace));
            
        }

        public class AlienSettings
        {
            public GeneralSettings generalSettings = new GeneralSettings();
            public List<GraphicPaths> graphicPaths = new List<GraphicPaths>();
            public HairSettings hairSettings = new HairSettings();
            public ThoughtSettings thoughtSettings = new ThoughtSettings();
            public RelationSettings relationSettings = new RelationSettings();
            public RaceRestrictionSettings raceRestriction = new RaceRestrictionSettings();
            public CompatibilityInfo compatibility = new CompatibilityInfo();
        }
    }

    public class GeneralSettings
    {
        public float maleGenderProbability = 0.5f;
        public bool immuneToAge = false;
        public bool canLayDown = true;
        public float minAgeForAdulthood = -1f;

        public List<ThingDef> validBeds;
        public List<ChemicalSettings> chemicalSettings;
        public List<AlienTraitEntry> forcedRaceTraitEntries;
        public List<AlienTraitEntry> disallowedTraits;
        public IntRange additionalTraits = IntRange.zero;
        public AlienPartGenerator alienPartGenerator = new AlienPartGenerator();

        public List<FactionRelationSettings> factionRelations;
        public int maxDamageForSocialfight = int.MaxValue;
        public bool allowHumanBios = false;
        public bool immuneToXenophobia = false;
        public List<ThingDef> notXenophobistTowards = new List<ThingDef>();
        public bool humanRecipeImport = false;
    }

    public class FactionRelationSettings
    {
        public List<FactionDef> factions;
        public IntRange goodwill;
    }

    public class ChemicalSettings
    {
        public ChemicalDef chemical;
        public bool ingestible = true;
        public List<IngestionOutcomeDoer> reactions;
    }

    public class AlienTraitEntry
    {
        public TraitDef defName;
        public int degree = 0;
        public float chance = 100;

        public float commonalityMale = -1f;
        public float commonalityFemale = -1f;
    }

    public class GraphicPaths
    {
        public List<LifeStageDef> lifeStageDefs;

        public Vector2 customDrawSize = Vector2.one;
        public Vector2 customPortraitDrawSize = Vector2.one;
        public Vector2 customHeadDrawSize = Vector2.zero;
        public Vector2 customPortraitHeadDrawSize = Vector2.zero;

        public Vector2 headOffset = Vector2.zero;
        public DirectionOffset headOffsetDirectional;

        public const string VANILLA_HEAD_PATH = "Things/Pawn/Humanlike/Heads/";
        public const string VANILLA_SKELETON_PATH = "Things/Pawn/Humanlike/HumanoidDessicated";

        public string body = "Things/Pawn/Humanlike/Bodies/";
        public string head = "Things/Pawn/Humanlike/Heads/";
        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public string stump = "Things/Pawn/Humanlike/Heads/None_Average_Stump";

        public ShaderTypeDef skinShader;
    }

    public class DirectionOffset
    {
        public Vector2 north = Vector2.zero;
        public Vector2 west = Vector2.zero;
        public Vector2 east = Vector2.zero;
        public Vector2 south = Vector2.zero;

        public Vector2 GetOffset(Rot4 rot) => 
            rot == Rot4.North ? this.north : rot == Rot4.East ? this.east : rot == Rot4.West ? this.west : this.south;
    }

    public class HairSettings
    {
        public bool hasHair = true;
        public List<string> hairTags;
        public int getsGreyAt = 40;
        public ShaderTypeDef shader;
    }

    public class ThoughtSettings
    {
        public List<ThoughtDef> cannotReceiveThoughts;
        public bool         cannotReceiveThoughtsAtAll = false;
        public List<ThoughtDef> canStillReceiveThoughts;

        public static Dictionary<ThoughtDef, List<ThingDef_AlienRace>> thoughtRestrictionDict = new Dictionary<ThoughtDef, List<ThingDef_AlienRace>>();
        public        List<ThoughtDef>                                     restrictedThoughts     = new List<ThoughtDef>();

        public ThoughtDef ReplaceIfApplicable(ThoughtDef def) =>
            (this.replacerList == null || this.replacerList.Select(tr => tr.replacer).Contains(def))
                ? def : this.replacerList.FirstOrDefault(tr => tr.original == def)?.replacer ?? def;

        public ButcherThought       butcherThoughtGeneral  = new ButcherThought();
        public List<ButcherThought> butcherThoughtSpecific = new List<ButcherThought>();

        public AteThought       ateThoughtGeneral  = new AteThought();
        public List<AteThought> ateThoughtSpecific = new List<AteThought>();

        public ThoughtDef GetAteThought(ThingDef race, bool cannibal, bool ingredient) =>
            (this.ateThoughtSpecific?.FirstOrDefault(predicate: at => at.raceList?.Contains(item: race) ?? false) ?? this.ateThoughtGeneral)?.GetThought(cannibal: cannibal, ingredient: ingredient);


        public List<ThoughtReplacer> replacerList;
    }

    public class ButcherThought
    {
        public List<ThingDef> raceList;

        [LoadDefFromField(typeof(ThoughtDef), nameof(ThoughtDefOf.ButcheredHumanlikeCorpse))]
        public ThoughtDef thought;// "ButcheredHumanlikeCorpse";

        [LoadDefFromField(typeof(ThoughtDef), nameof(ThoughtDefOf.KnowButcheredHumanlikeCorpse))]
        public ThoughtDef knowThought;// "KnowButcheredHumanlikeCorpse";
    }

    public class AteThought
    {
        public List<ThingDef> raceList;
        [LoadDefFromField(typeof(ThoughtDef), nameof(ThoughtDefOf.AteHumanlikeMeatDirect))]
        public ThoughtDef thought;// "AteHumanlikeMeatDirect";

        [LoadDefFromField(typeof(ThoughtDef), nameof(ThoughtDefOf.AteHumanlikeMeatDirectCannibal))]
        public ThoughtDef thoughtCannibal; // "AteHumanlikeMeatDirectCannibal";

        [LoadDefFromField(typeof(ThoughtDef), nameof(ThoughtDefOf.AteHumanlikeMeatAsIngredient))]
        public ThoughtDef ingredientThought; // "AteHumanlikeMeatAsIngredient";

        [LoadDefFromField(typeof(ThoughtDef), nameof(ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal))]
        public ThoughtDef ingredientThoughtCannibal; // "AteHumanlikeMeatAsIngredientCannibal";

        public ThoughtDef GetThought(bool cannibal, bool ingredient) =>
            cannibal ? ingredient ? this.ingredientThoughtCannibal : this.thoughtCannibal : ingredient ? this.ingredientThought : this.thought;
    }

    public class ThoughtReplacer
    {
        public ThoughtDef original;
        public ThoughtDef replacer;
    }

    public class RelationSettings
    {
        public float relationChanceModifierChild = 1f;
        public float relationChanceModifierExLover = 1f;
        public float relationChanceModifierExSpouse = 1f;
        public float relationChanceModifierFiance = 1f;
        public float relationChanceModifierLover = 1f;
        public float relationChanceModifierParent = 1f;
        public float relationChanceModifierSibling = 1f;
        public float relationChanceModifierSpouse = 1f;

        public List<RelationRenamer> renamer;
    }

    public class RelationRenamer
    {
        public PawnRelationDef relation;
        public string label;
        public string femaleLabel;
    }

    public class RaceRestrictionSettings
    {
       
        public bool onlyUseRaceRestrictedApparel = false;
        public List<ThingDef> apparelList = new List<ThingDef>();
        public List<ThingDef> whiteApparelList = new List<ThingDef>();

        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> apparelRestrictionDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();
        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> apparelWhiteDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();


        public static bool CanWear(ThingDef apparel, ThingDef race) =>
            !apparelRestrictionDict.TryGetValue(key: apparel, value: out List<ThingDef_AlienRace> races) && 
            !((race as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyUseRaceRestrictedApparel ?? false) || 
            (races?.Contains(item: race as ThingDef_AlienRace) ?? false) ||
            apparelWhiteDict.TryGetValue(key: apparel, value: out races) && (races?.Contains(item: race as ThingDef_AlienRace) ?? false);

        public List<ResearchProjectRestrictions> researchList = new List<ResearchProjectRestrictions>();
        public static Dictionary<ResearchProjectDef, List<ThingDef_AlienRace>> researchRestrictionDict = new Dictionary<ResearchProjectDef, List<ThingDef_AlienRace>>();

        public static bool CanResearch(IEnumerable<ThingDef> races, ResearchProjectDef project) =>
            !researchRestrictionDict.ContainsKey(project) || races.Any(ar => researchRestrictionDict[project].Contains(ar));


        public bool onlyUseRaceRestrictedWeapons = false;
        public List<ThingDef> weaponList = new List<ThingDef>();
        public List<ThingDef> whiteWeaponList = new List<ThingDef>();

        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> weaponRestrictionDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();
        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> weaponWhiteDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();

        public static bool CanEquip(ThingDef weapon, ThingDef race) =>
            !weaponRestrictionDict.TryGetValue(key: weapon, value: out List<ThingDef_AlienRace> races) &&
            !((race as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyUseRaceRestrictedWeapons ?? false) ||
            (races?.Contains(item: race as ThingDef_AlienRace) ?? false) ||
            weaponWhiteDict.TryGetValue(key: weapon, value: out races) && (races?.Contains(item: race as ThingDef_AlienRace) ?? false);

        public bool onlyBuildRaceRestrictedBuildings = false;
        public List<ThingDef> buildingList = new List<ThingDef>();
        public List<ThingDef> whiteBuildingList = new List<ThingDef>();

        public static Dictionary<BuildableDef, List<ThingDef_AlienRace>> buildingRestrictionDict = new Dictionary<BuildableDef, List<ThingDef_AlienRace>>();
        public static Dictionary<BuildableDef, List<ThingDef_AlienRace>> buildingWhiteDict = new Dictionary<BuildableDef, List<ThingDef_AlienRace>>();

        public static bool CanBuild(BuildableDef building, ThingDef race) =>
            !buildingRestrictionDict.TryGetValue(key: building, value: out List<ThingDef_AlienRace> races) &&
            !((race as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyBuildRaceRestrictedBuildings ?? false) ||
            (races?.Contains(item: race as ThingDef_AlienRace) ?? false) ||
            buildingWhiteDict.TryGetValue(key: building, value: out races) && (races?.Contains(item: race as ThingDef_AlienRace) ?? false);

        public bool onlyDoRaceRestrictedRecipes = false;
        public List<RecipeDef> recipeList = new List<RecipeDef>();
        public List<RecipeDef> whiteRecipeList = new List<RecipeDef>();

        public static Dictionary<RecipeDef, List<ThingDef_AlienRace>> recipeRestrictionDict = new Dictionary<RecipeDef, List<ThingDef_AlienRace>>();
        public static Dictionary<RecipeDef, List<ThingDef_AlienRace>> recipeWhiteDict = new Dictionary<RecipeDef, List<ThingDef_AlienRace>>();

        public static bool CanDoRecipe(RecipeDef recipe, ThingDef race) =>
            !recipeRestrictionDict.TryGetValue(key: recipe, value: out List<ThingDef_AlienRace> races) &&
            !((race as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyDoRaceRestrictedRecipes ?? false) ||
            (races?.Contains(item: race as ThingDef_AlienRace) ?? false) ||
            recipeWhiteDict.TryGetValue(key: recipe, value: out races) && (races?.Contains(item: race as ThingDef_AlienRace) ?? false);


        public bool onlyDoRaceRestrictedPlants = false;
        public List<ThingDef> plantList = new List<ThingDef>();
        public List<ThingDef> whitePlantList = new List<ThingDef>();

        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> plantRestrictionDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();
        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> plantWhiteDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();

        public static bool CanPlant(ThingDef plant, ThingDef race) =>
            !plantRestrictionDict.TryGetValue(key: plant, value: out List<ThingDef_AlienRace> races) &&
            !((race as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyDoRaceRestrictedPlants ?? false) ||
            (races?.Contains(item: race as ThingDef_AlienRace) ?? false) ||
            plantWhiteDict.TryGetValue(key: plant, value: out races) && (races?.Contains(item: race as ThingDef_AlienRace) ?? false);

        public bool onlyGetRaceRestrictedTraits = false;
        public List<TraitDef> traitList = new List<TraitDef>();
        public List<TraitDef> whiteTraitList = new List<TraitDef>();

        public static Dictionary<TraitDef, List<ThingDef_AlienRace>> traitRestrictionDict = new Dictionary<TraitDef, List<ThingDef_AlienRace>>();
        public static Dictionary<TraitDef, List<ThingDef_AlienRace>> traitWhiteDict = new Dictionary<TraitDef, List<ThingDef_AlienRace>>();

        public static bool CanGetTrait(TraitDef trait, ThingDef race) =>
            !traitRestrictionDict.TryGetValue(key: trait, value: out List<ThingDef_AlienRace> races) &&
            !((race as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyGetRaceRestrictedTraits ?? false) ||
            (races?.Contains(item: race as ThingDef_AlienRace) ?? false) ||
            traitWhiteDict.TryGetValue(key: trait, value: out races) && (races?.Contains(item: race as ThingDef_AlienRace) ?? false);

        public bool onlyEatRaceRestrictedFood = false;
        public List<ThingDef> foodList = new List<ThingDef>();
        public List<ThingDef> whiteFoodList = new List<ThingDef>();

        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> foodRestrictionDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();
        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> foodWhiteDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();

        public static bool CanEat(ThingDef food, ThingDef race)
        {
            if (foodRestrictionDict.TryGetValue(key: food, value: out List<ThingDef_AlienRace> races) || 
                ((race as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyEatRaceRestrictedFood ?? false))
            {
                if (!(races?.Contains(race as ThingDef_AlienRace) ?? false) && (!foodWhiteDict.TryGetValue(key: food, value: out races) || !races.Contains(item: race as ThingDef_AlienRace)))
                    return false;
            }

            ChemicalDef chemical = food.GetCompProperties<CompProperties_Drug>()?.chemical;

            return chemical == null || ((race as ThingDef_AlienRace)?.alienRace.generalSettings.chemicalSettings?.TrueForAll(c => c.ingestible || c.chemical != chemical) ?? true);
        }

        public bool onlyTameRaceRestrictedPets = false;
        public List<ThingDef> petList = new List<ThingDef>();
        public List<ThingDef> whitePetList = new List<ThingDef>();

        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> tameRestrictionDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();
        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> tameWhiteDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();

        public static bool CanTame(ThingDef pet, ThingDef race) =>
            !tameRestrictionDict.TryGetValue(key: pet, value: out List<ThingDef_AlienRace> races) &&
            !((race as ThingDef_AlienRace)?.alienRace.raceRestriction.onlyTameRaceRestrictedPets ?? false) ||
            (races?.Contains(item: race as ThingDef_AlienRace) ?? false) ||
            tameWhiteDict.TryGetValue(key: pet, value: out races) && (races?.Contains(item: race as ThingDef_AlienRace) ?? false);

        public List<ConceptDef> conceptList = new List<ConceptDef>();

        public List<WorkGiverDef> workGiverList = new List<WorkGiverDef>();
    }

    public class ResearchProjectRestrictions
    {
        public List<ResearchProjectDef> projects = new List<ResearchProjectDef>();
        public List<ThingDef> apparelList;
    }
    
    public static class GraphicPathsExtension
    {
        public static GraphicPaths GetCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef) => 
            list.FirstOrDefault(predicate: gp => gp.lifeStageDefs?.Contains(item: lifeStageDef) ?? false) ?? list.First();
    }

    public class Info : DefModExtension
    {
        public bool allowHumanBios = true;
        public float maleGenderProbability = 0.5f;
    }

    public class LifeStageAgeAlien : LifeStageAge
    {
        public BodyDef body;
    }

    public class CompatibilityInfo
    {
        protected bool isFlesh = true;

        public virtual bool IsFlesh
        {
            get => this.isFlesh;
            set => this.isFlesh = value;
        }

        public virtual bool IsFleshPawn(Pawn pawn) => this.IsFlesh;

        protected bool isSentient = true;

        public virtual bool IsSentient
        {
            get => this.isSentient;
            set => this.isSentient = value;
        }

        public virtual bool IsSentientPawn(Pawn pawn) => this.IsSentient;

        protected bool hasBlood = true;

        public virtual bool HasBlood
        {
            get => this.hasBlood;
            set => this.hasBlood = value;
        }

        public virtual bool HasBloodPawn(Pawn pawn) => this.HasBlood;
    }
}