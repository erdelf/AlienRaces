using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public sealed class ThingDef_AlienRace : ThingDef
    {
        public AlienSettings alienRace;

        public override void ResolveReferences()
        {
            this.comps.Add(new CompProperties(typeof(AlienPartGenerator.AlienComp)));
            base.ResolveReferences();
            if (this.alienRace.graphicPaths.NullOrEmpty())
                this.alienRace.graphicPaths.Add(new GraphicPaths());
            this.alienRace.generalSettings.alienPartGenerator.alienProps = this;
        }

        public sealed class AlienSettings
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

    public sealed class GeneralSettings
    {
        public float MaleGenderProbability = 0.5f;
        public bool PawnsSpecificBackstories = false;
        public bool ImmuneToAge = false;
        public bool CanLayDown = true;

        public List<string> validBeds;
        public List<ChemicalSettings> chemicalSettings;
        public List<AlienTraitEntry> forcedRaceTraitEntries;
        public List<string> disallowedTraits;
        public AlienPartGenerator alienPartGenerator = new AlienPartGenerator();

        public List<FactionRelationSettings> factionRelations;
        public int maxDamageForSocialfight = int.MaxValue;
        public bool allowHumanBios = false;
        public bool ImmuneToXenophobia = false;
        public List<string> notXenophobistTowards = new List<string>();
    }

    public sealed class FactionRelationSettings
    {
        public List<string> factions;
        public FloatRange goodwill;
    }

    public sealed class ChemicalSettings
    {
        public string chemical;
        public bool ingestible = true;
        public List<IngestionOutcomeDoer> reactions;
    }

    public sealed class AlienTraitEntry
    {
        public string defname;
        public int degree = 0;
        public float chance = 100;

        public float commonalityMale = -1f;
        public float commonalityFemale = -1f;
    }

    public sealed class GraphicPaths
    {
        public List<LifeStageDef> lifeStageDefs;

        public const string vanillaHeadPath = "Things/Pawn/Humanlike/Heads/";

        public string body = "Things/Pawn/Humanlike/Bodies/";
        public string head = "Things/Pawn/Humanlike/Heads/";
        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public string stump = "Things/Pawn/Humanlike/Heads/None_Average_Stump";
    }

    public sealed class HairSettings
    {
        public bool HasHair = true;
        public List<string> hairTags;
        public int GetsGreyAt = 40;
    }

    public sealed class PawnKindSettings
    {
        public List<PawnKindEntry> alienslavekinds;
        public List<PawnKindEntry> alienrefugeekinds;
        public List<FactionPawnKindEntry> startingColonists;
        public List<FactionPawnKindEntry> alienwandererkinds;
    }

    public sealed class PawnKindEntry
    {
        public List<string> kindDefs;
        public float chance;
    }

    public sealed class FactionPawnKindEntry
    {
        public List<PawnKindEntry> pawnKindEntries;
        public List<string> factionDefs;
    }

    public sealed class ThoughtSettings
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

    public sealed class ButcherThought
    {
        public List<string> raceList;
        public string thought = "ButcheredHumanlikeCorpse";
        public string knowThought = "KnowButcheredHumanlikeCorpse";
    }

    public sealed class AteThought
    {
        public List<string> raceList;
        public string thought = "AteHumanlikeMeatDirect";
        public string ingredientThought = "AteHumanlikeMeatAsIngredient";
    }

    public sealed class ThoughtReplacer
    {
        public string original;
        public string replacer;
    }

    public sealed class RelationSettings
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

    public sealed class RelationRenamer
    {
        public string relation;
        public string label;
        public string femaleLabel;
    }

    public sealed class RaceRestrictionSettings
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

        public List<string> conceptList;

        public List<string> workGiverList;
    }

    public sealed class ResearchProjectRestrictions
    {
        public List<string> projects;
        public List<string> apparelList;
    }
    
    static class GraphicPathsExtension
    {
        public static GraphicPaths GetCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef) => list.FirstOrDefault(gp => gp.lifeStageDefs?.Contains(lifeStageDef) ?? false) ?? list.First();
    }

    public class Info : DefModExtension
    {
        public bool usePawnKindBackstories = false;
    }
}