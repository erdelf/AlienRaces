using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace AlienRace
{
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
            });
            this.alienRace.generalSettings.alienPartGenerator.alienProps = this;
            foreach (AlienPartGenerator.BodyAddon bodyAddon in this.alienRace.generalSettings.alienPartGenerator.bodyAddons)
                if (bodyAddon.offsets.west == null)
                    bodyAddon.offsets.west = bodyAddon.offsets.east;
        }

        public class AlienSettings
        {
            public GeneralSettings generalSettings = new GeneralSettings();
            public List<GraphicPaths> graphicPaths = new List<GraphicPaths>();
            public HairSettings hairSettings = new HairSettings();
            public ThoughtSettings thoughtSettings = new ThoughtSettings();
            public RelationSettings relationSettings = new RelationSettings();
            public RaceRestrictionSettings raceRestriction = new RaceRestrictionSettings();
        }
    }

    public class GeneralSettings
    {
        public float maleGenderProbability = 0.5f;
        public bool immuneToAge = false;
        public bool canLayDown = true;

        public List<string> validBeds;
        public List<ChemicalSettings> chemicalSettings;
        public List<AlienTraitEntry> forcedRaceTraitEntries;
        public List<string> disallowedTraits;
        public AlienPartGenerator alienPartGenerator = new AlienPartGenerator();

        public bool useOnlyPawnkindBackstories = false;

        public List<FactionRelationSettings> factionRelations;
        public int maxDamageForSocialfight = int.MaxValue;
        public bool allowHumanBios = false;
        public bool immuneToXenophobia = false;
        public List<string> notXenophobistTowards = new List<string>();
        public bool humanRecipeImport = false;
    }

    public class FactionRelationSettings
    {
        public List<string> factions;
        public IntRange goodwill;
    }

    public class ChemicalSettings
    {
        public string chemical;
        public bool ingestible = true;
        public List<IngestionOutcomeDoer> reactions;
    }

    public class AlienTraitEntry
    {
        public string defName;
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

        public const string VANILLA_HEAD_PATH = "Things/Pawn/Humanlike/Heads/";
        public const string VANILLA_SKELETON_PATH = "Things/Pawn/Humanlike/HumanoidDessicated";

        public string body = "Things/Pawn/Humanlike/Bodies/";
        public string head = "Things/Pawn/Humanlike/Heads/";
        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public string stump = "Things/Pawn/Humanlike/Heads/None_Average_Stump";
    }

    public class HairSettings
    {
        public bool hasHair = true;
        public List<string> hairTags;
        public int getsGreyAt = 40;
    }

    public class ThoughtSettings
    {
        public List<string> cannotReceiveThoughts;
        public bool cannotReceiveThoughtsAtAll = false;
        public List<string> canStillReceiveThoughts;

        public ButcherThought butcherThoughtGeneral = new ButcherThought();
        public List<ButcherThought> butcherThoughtSpecific = new List<ButcherThought>();

        public AteThought ateThoughtGeneral = new AteThought();
        public List<AteThought> ateThoughtSpecific = new List<AteThought>();

        public List<ThoughtReplacer> replacerList;
    }

    public class ButcherThought
    {
        public List<string> raceList;
        public string thought = "ButcheredHumanlikeCorpse";
        public string knowThought = "KnowButcheredHumanlikeCorpse";
    }

    public class AteThought
    {
        public List<string> raceList;
        public string thought = "AteHumanlikeMeatDirect";
        public string ingredientThought = "AteHumanlikeMeatAsIngredient";
    }

    public class ThoughtReplacer
    {
        public string original;
        public string replacer;
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
        public string relation;
        public string label;
        public string femaleLabel;
    }

    public class RaceRestrictionSettings
    {
        public bool onlyUseRaceRestrictedApparel = false;
        public List<string> apparelList;
        public List<string> whiteApparelList;

        public List<ResearchProjectRestrictions> researchList;

        public bool onlyUseRaceRestrictedWeapons = false;
        public List<string> weaponList;
        public List<string> whiteWeaponList;

        public bool onlyBuildRaceRestrictedBuildings = false;
        public List<string> buildingList;
        public List<string> whiteBuildingList;

        public bool onlyDoRaceRestrictedRecipes = false;
        public List<string> recipeList;
        public List<string> whiteRecipeList;

        public bool onlyDoRaceRastrictedPlants = false;
        public List<string> plantList;
        public List<string> whitePlantList;

        public bool onlyGetRaceRestrictedTraits = false;
        public List<string> traitList;
        public List<string> whiteTraitList;

        public bool onlyEatRaceRestrictedFood = false;
        public List<string> foodList;
        public List<string> whiteFoodList;

        public bool onlyTameRaceRestrictedPets = false;
        public List<string> petList;
        public List<string> whitePetList;

        public List<string> conceptList;

        public List<string> workGiverList;
    }

    public class ResearchProjectRestrictions
    {
        public List<string> projects;
        public List<string> apparelList;
    }
    
    static class GraphicPathsExtension
    {
        public static GraphicPaths GetCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef) => list.FirstOrDefault(predicate: gp => gp.lifeStageDefs?.Contains(item: lifeStageDef) ?? false) ?? list.First();
    }

    public class Info : DefModExtension
    {
        public bool useOnlyPawnkindBackstories = false;
        public bool allowHumanBios = true;
        public float maleGenderProbability = 0.5f;
    }

    public class LifeStageAgeAlien : LifeStageAge
    {
        public BodyDef body;
    }
}