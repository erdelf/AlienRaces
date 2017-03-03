using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public class ThingDef_AlienRace : ThingDef
    {
        public AlienSettings alienRace;

        public override void ResolveReferences()
        {
            comps.Add(new CompProperties(typeof(AlienPartGenerator.AlienComp)));
            base.ResolveReferences();
        }

        public class AlienSettings
        {
            public GeneralSettings generalSettings = new GeneralSettings();
            public List<GraphicPaths> graphicPaths = new List<GraphicPaths>();
            public HairSettings hairSettings = new HairSettings();
            public PawnKindSettings pawnKindSettings = new PawnKindSettings();
            public ThoughtSettings thoughtSettings = new ThoughtSettings();
            public RelationSettings relationSettings = new RelationSettings();
            public RaceRestrictionSettings raceRestriction = new RaceRestrictionSettings();
        }
    }

    public class GeneralSettings
    {
        public float MaleGenderProbability = 0.5f;
        public bool PawnsSpecificBackstories = false;
        public bool ImmuneToAge = false;
        public bool CanLayDown = true;

        public List<ChemicalSettings> chemicalSettings;
        public List<AlienTraitEntry> forcedRaceTraitEntries;
        public AlienPartGenerator alienPartGenerator = new AlienPartGenerator();
    }

    public class ChemicalSettings
    {
        public string chemical;
        public bool ingestible = true;
        public List<IngestionOutcomeDoer> reactions;
    }

    public class AlienTraitEntry
    {
        public string defname;
        public int degree = 0;
        public float chance = 100;

        public float commonalityMale = -1f;
        public float commonalityFemale = -1f;
    }

    public class GraphicPaths
    {
        public List<LifeStageDef> lifeStageDefs;

        public string body = "Things/Pawn/Humanlike/Bodies/";
        public string head = "Things/Pawn/Humanlike/Heads/";
        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public string tail = "";
    }

    public class HairSettings
    {
        public bool HasHair = true;
        public List<string> hairTags;
        public int GetsGreyAt = 40;
    }

    public class PawnKindSettings
    {
        public List<PawnKindEntry> alienslavekinds;
        public List<PawnKindEntry> alienrefugeekinds;
        public List<FactionPawnKindEntry> startingColonists;
        public List<FactionPawnKindEntry> alienwandererkinds;
    }

    public class PawnKindEntry
    {
        public List<string> kindDefs;
        public float chance;
    }

    public class FactionPawnKindEntry
    {
        public List<PawnKindEntry> pawnKindEntries;
        public List<string> factionDefs;
    }

    public class ThoughtSettings
    {
        public List<string> cannotReceiveThoughts;
        public bool cannotReceiveThoughtsAtAll = false;

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
        public static GraphicPaths GetCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef)
        {
            return list.FirstOrDefault(gp => gp.lifeStageDefs?.Contains(lifeStageDef) ?? false) ?? list.First();
        }
    }
}