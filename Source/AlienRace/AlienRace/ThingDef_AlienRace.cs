namespace AlienRace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class ThingDef_AlienRace : ThingDef
    {
        public AlienSettings alienRace;

        public override void ResolveReferences()
        {
            this.comps.Add(new CompProperties(typeof(AlienPartGenerator.AlienComp)));
            base.ResolveReferences();

            if (this.alienRace.graphicPaths.NullOrEmpty())
                this.alienRace.graphicPaths.Add(new GraphicPaths());

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

            if(!this.alienRace.styleSettings.ContainsKey(typeof(HairDef)))
                this.alienRace.styleSettings.Add(typeof(HairDef), new StyleSettings());
            if (!this.alienRace.styleSettings.ContainsKey(typeof(TattooDef)))
                this.alienRace.styleSettings.Add(typeof(TattooDef), new StyleSettings());
            if (!this.alienRace.styleSettings.ContainsKey(typeof(BeardDef)))
                this.alienRace.styleSettings.Add(typeof(BeardDef), new StyleSettings());

            foreach (AlienPartGenerator.BodyAddon bodyAddon in this.alienRace.generalSettings.alienPartGenerator.bodyAddons)
            {
                if (bodyAddon.offsets.west == null)
                    bodyAddon.offsets.west = bodyAddon.offsets.east;
            }

            if (this.alienRace.generalSettings.minAgeForAdulthood < 0)
                this.alienRace.generalSettings.minAgeForAdulthood = (float) AccessTools.Field(typeof(PawnBioAndNameGenerator), name: "MinAgeForAdulthood").GetValue(obj: null);

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
                            {
                                if (o.GetType().Assembly == typeof(ThingDef_AlienRace).Assembly)
                                    RecursiveAttributeCheck(o.GetType(), Traverse.Create(o));
                            }
                    }

                    if (field.FieldType.Assembly == typeof(ThingDef_AlienRace).Assembly) 
                        RecursiveAttributeCheck(field.FieldType, instanceNew);

                    LoadDefFromField attribute = field.GetCustomAttribute<LoadDefFromField>();
                    if (attribute != null)
                        if (instanceNew.GetValue() == null)
                            instanceNew.SetValue(attribute.GetDef(field.FieldType));
                }
            }
            RecursiveAttributeCheck(typeof(AlienSettings), Traverse.Create(this.alienRace));
        }

        public class AlienSettings
        {
            public GeneralSettings                 generalSettings  = new GeneralSettings();
            public List<GraphicPaths>              graphicPaths     = new List<GraphicPaths>();
            public Dictionary<Type, StyleSettings> styleSettings    = new Dictionary<Type, StyleSettings>();
            public ThoughtSettings                 thoughtSettings  = new ThoughtSettings();
            public RelationSettings                relationSettings = new RelationSettings();
            public RaceRestrictionSettings         raceRestriction  = new RaceRestrictionSettings();
            public CompatibilityInfo               compatibility    = new CompatibilityInfo();
        }
    }

    public class GeneralSettings
    {
        public float maleGenderProbability = 0.5f;
        public bool immuneToAge = false;
        public bool canLayDown = true;
        public float minAgeForAdulthood = -1f;

        public List<ThingDef>         validBeds;
        public List<ChemicalSettings> chemicalSettings;
        public List<AlienTraitEntry>  forcedRaceTraitEntries;
        public List<AlienTraitEntry>  disallowedTraits;
        public IntRange               traitCount         = new IntRange(2, 3);
        public IntRange               additionalTraits   = IntRange.zero;
        public AlienPartGenerator     alienPartGenerator = new AlienPartGenerator();

        public List<FactionRelationSettings> factionRelations;
        public int maxDamageForSocialfight = int.MaxValue;
        public bool allowHumanBios = false;
        public bool immuneToXenophobia = false;
        public List<ThingDef> notXenophobistTowards = new List<ThingDef>();
        public bool humanRecipeImport = false;

        public SimpleCurve lovinIntervalHoursFromAge;
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

        public  string body          = "Things/Pawn/Humanlike/Bodies/";
        public  string bodyMasks     = string.Empty;
        private int    bodyMaskCount = -1;
        public  string head          = "Things/Pawn/Humanlike/Heads/";
        public  string headMasks     = string.Empty;
        private int    headMaskCount = -1;

        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull    = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public string stump    = "Things/Pawn/Humanlike/Heads/None_Average_Stump";

        public ShaderTypeDef skinShader;

        public int HeadMaskCount
        {
            get
            {
                if (this.headMaskCount >= 0 || this.headMasks.NullOrEmpty())
                    return this.headMaskCount;

                this.headMaskCount = 0;
                while (ContentFinder<Texture2D>.Get($"{this.headMasks}{(this.headMaskCount == 0 ? string.Empty : this.headMaskCount.ToString())}_north", reportFailure: false) != null)
                    this.headMaskCount++;

                return this.headMaskCount;
            }
        }

        public int BodyMaskCount
        {
            get
            {
                if (this.bodyMaskCount >= 0 || this.bodyMasks.NullOrEmpty())
                    return this.bodyMaskCount;

                this.bodyMaskCount = 0;
                while (ContentFinder<Texture2D>.Get($"{this.bodyMasks}{(this.bodyMaskCount == 0 ? string.Empty : this.bodyMaskCount.ToString())}_north", reportFailure: false) != null)
                    this.bodyMaskCount++;

                return this.bodyMaskCount;
            }
        }
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

    public class StyleSettings
    {
        public bool          hasStyle = true;
        public List<string>  styleTags;
        public List<string>  styleTagsOverride;
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
            (this.replacerList == null || this.replacerList.Select(selector: tr => tr.replacer).Contains(def))
                ? def : this.replacerList.FirstOrDefault(predicate: tr => tr.original == def)?.replacer ?? def;

        public ButcherThought       butcherThoughtGeneral  = new ButcherThought();
        public List<ButcherThought> butcherThoughtSpecific = new List<ButcherThought>();

        public AteThought       ateThoughtGeneral  = new AteThought();
        public List<AteThought> ateThoughtSpecific = new List<AteThought>();

        public ThoughtDef GetAteThought(ThingDef race, bool cannibal, bool ingredient) =>
            (this.ateThoughtSpecific?.FirstOrDefault(predicate: at => at.raceList?.Contains(race) ?? false) ?? this.ateThoughtGeneral)?.GetThought(cannibal, ingredient);


        public List<ThoughtReplacer> replacerList;
    }

    public class ButcherThought
    {
        public List<ThingDef> raceList;

        [LoadDefFromField(nameof(ThoughtDefOf.ButcheredHumanlikeCorpse))]
        public ThoughtDef thought;// "ButcheredHumanlikeCorpse";

        [LoadDefFromField(nameof(ThoughtDefOf.KnowButcheredHumanlikeCorpse))]
        public ThoughtDef knowThought;// "KnowButcheredHumanlikeCorpse";
    }

    public class AteThought
    {
        public List<ThingDef> raceList;
        [LoadDefFromField(nameof(ThoughtDefOf.AteHumanlikeMeatDirect))]
        public ThoughtDef thought;// "AteHumanlikeMeatDirect";

        [LoadDefFromField(nameof(ThoughtDefOf.AteHumanlikeMeatDirectCannibal))]
        public ThoughtDef thoughtCannibal; // "AteHumanlikeMeatDirectCannibal";

        [LoadDefFromField(nameof(ThoughtDefOf.AteHumanlikeMeatAsIngredient))]
        public ThoughtDef ingredientThought; // "AteHumanlikeMeatAsIngredient";

        [LoadDefFromField(nameof(ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal))]
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
        public bool           onlyUseRaceRestrictedApparel = false;
        public List<ThingDef> apparelList                  = new List<ThingDef>();
        public List<ThingDef> whiteApparelList             = new List<ThingDef>();
        public List<ThingDef> blackApparelList             = new List<ThingDef>();

        public static HashSet<ThingDef> apparelRestricted = new HashSet<ThingDef>();

        public static bool CanWear(ThingDef apparel, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool result = true;

            if (apparelRestricted.Contains(apparel) || (raceRestriction?.onlyUseRaceRestrictedApparel ?? false)) 
                result = raceRestriction?.whiteApparelList.Contains(apparel) ?? false;

            return result && !(raceRestriction?.blackApparelList.Contains(apparel) ?? false);
        }


        public List<ResearchProjectRestrictions> researchList = new List<ResearchProjectRestrictions>();
        public static Dictionary<ResearchProjectDef, List<ThingDef_AlienRace>> researchRestrictionDict = new Dictionary<ResearchProjectDef, List<ThingDef_AlienRace>>();

        public static bool CanResearch(IEnumerable<ThingDef> races, ResearchProjectDef project) =>
            !researchRestrictionDict.ContainsKey(project) || races.Any(predicate: ar => researchRestrictionDict[project].Contains(ar));


        public bool           onlyUseRaceRestrictedWeapons = false;
        public List<ThingDef> weaponList                   = new List<ThingDef>();
        public List<ThingDef> whiteWeaponList              = new List<ThingDef>();
        public List<ThingDef> blackWeaponList             = new List<ThingDef>();

        public static HashSet<ThingDef>                              weaponRestricted     = new HashSet<ThingDef>();

        public static bool CanEquip(ThingDef weapon, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (weaponRestricted.Contains(weapon) || (raceRestriction?.onlyUseRaceRestrictedWeapons ?? false))
                result = raceRestriction?.whiteWeaponList.Contains(weapon)                          ?? false;

            return result && !(raceRestriction?.blackWeaponList.Contains(weapon) ?? false);
        }

        public bool           onlyBuildRaceRestrictedBuildings = false;
        public List<ThingDef> buildingList                     = new List<ThingDef>();
        public List<ThingDef> whiteBuildingList                = new List<ThingDef>();
        public List<ThingDef> blackBuildingList                = new List<ThingDef>();

        public static HashSet<ThingDef> buildingRestricted = new HashSet<ThingDef>();

        public static HashSet<ThingDef> buildingsRestrictedWithCurrentColony = new HashSet<ThingDef>();

        public static bool CanColonyBuild(BuildableDef building) => 
            !buildingsRestrictedWithCurrentColony.Contains(building);

        public static bool CanBuild(BuildableDef building, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (buildingRestricted.Contains(building) || (raceRestriction?.onlyBuildRaceRestrictedBuildings ?? false))
                result = raceRestriction?.whiteBuildingList.Contains(building)                          ?? false;

            return result && !(raceRestriction?.blackBuildingList.Contains(building) ?? false);
        }




        public bool            onlyDoRaceRestrictedRecipes = false;
        public List<RecipeDef> recipeList                  = new List<RecipeDef>();
        public List<RecipeDef> whiteRecipeList             = new List<RecipeDef>();
        public List<RecipeDef> blackRecipeList             = new List<RecipeDef>();

        public static HashSet<RecipeDef> recipeRestricted = new HashSet<RecipeDef>();

        public static bool CanDoRecipe(RecipeDef recipe, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (recipeRestricted.Contains(recipe) || (raceRestriction?.onlyDoRaceRestrictedRecipes ?? false))
                result = raceRestriction?.whiteRecipeList.Contains(recipe)                              ?? false;

            return result && !(raceRestriction?.blackRecipeList.Contains(recipe) ?? false);
        }

        public bool           onlyDoRaceRestrictedPlants = false;
        public List<ThingDef> plantList                  = new List<ThingDef>();
        public List<ThingDef> whitePlantList             = new List<ThingDef>();
        public List<ThingDef> blackPlantList             = new List<ThingDef>();

        public static HashSet<ThingDef> plantRestricted = new HashSet<ThingDef>();

        public static bool CanPlant(ThingDef plant, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (plantRestricted.Contains(plant) || (raceRestriction?.onlyDoRaceRestrictedPlants ?? false))
                result = raceRestriction?.whitePlantList.Contains(plant)                          ?? false;

            return result && !(raceRestriction?.blackPlantList.Contains(plant) ?? false);
        }

        public bool           onlyGetRaceRestrictedTraits = false;
        public List<TraitDef> traitList                   = new List<TraitDef>();
        public List<TraitDef> whiteTraitList              = new List<TraitDef>();
        public List<TraitDef> blackTraitList              = new List<TraitDef>();

        public static HashSet<TraitDef> traitRestricted = new HashSet<TraitDef>();

        public static bool CanGetTrait(TraitDef trait, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (traitRestricted.Contains(trait) || (raceRestriction?.onlyGetRaceRestrictedTraits ?? false))
                result = raceRestriction?.whiteTraitList.Contains(trait)                        ?? false;

            return result && !(raceRestriction?.blackTraitList.Contains(trait) ?? false);
        }
        
        public bool           onlyEatRaceRestrictedFood = false;
        public List<ThingDef> foodList                  = new List<ThingDef>();
        public List<ThingDef> whiteFoodList             = new List<ThingDef>();
        public List<ThingDef> blackFoodList             = new List<ThingDef>();

        public static HashSet<ThingDef> foodRestricted = new HashSet<ThingDef>();

        public static bool CanEat(ThingDef food, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (foodRestricted.Contains(food) || (raceRestriction?.onlyEatRaceRestrictedFood ?? false))
                result = raceRestriction?.whiteFoodList.Contains(food)                        ?? false;

            result &= !(raceRestriction?.blackFoodList.Contains(food) ?? false);

            ChemicalDef chemical = food.GetCompProperties<CompProperties_Drug>()?.chemical;
            return result && (chemical == null || ((race as ThingDef_AlienRace)?.alienRace.generalSettings.chemicalSettings?.TrueForAll(match: c => c.ingestible || c.chemical != chemical) ?? true));
        }

        public bool           onlyTameRaceRestrictedPets = false;
        public List<ThingDef> petList                    = new List<ThingDef>();
        public List<ThingDef> whitePetList               = new List<ThingDef>();
        public List<ThingDef> blackPetList               = new List<ThingDef>();

        public static HashSet<ThingDef> petRestricted = new HashSet<ThingDef>();

        public static bool CanTame(ThingDef pet, ThingDef race)
        {
            RaceRestrictionSettings raceRestriction = (race as ThingDef_AlienRace)?.alienRace.raceRestriction;
            bool                    result          = true;

            if (petRestricted.Contains(pet) || (raceRestriction?.onlyTameRaceRestrictedPets ?? false))
                result = raceRestriction?.whitePetList.Contains(pet)                       ?? false;

            return result && !(raceRestriction?.blackPetList.Contains(pet) ?? false);
        }

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
            list.FirstOrDefault(predicate: gp => gp.lifeStageDefs?.Contains(lifeStageDef) ?? false) ?? list.First();
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