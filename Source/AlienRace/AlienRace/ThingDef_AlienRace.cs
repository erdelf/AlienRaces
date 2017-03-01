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

            if (alienRace.thoughtSettings.butcherThoughtGeneral.thought == null)
                alienRace.thoughtSettings.butcherThoughtGeneral.thought = ThoughtDef.Named("ButcheredHumanlikeCorpse");
            if (alienRace.thoughtSettings.butcherThoughtGeneral.knowThought == null)
                alienRace.thoughtSettings.butcherThoughtGeneral.knowThought = ThoughtDef.Named("KnowButcheredHumanlikeCorpse");

            if (alienRace.thoughtSettings.butcherThoughtSpecific != null)
                foreach (ButcherThought bt in alienRace.thoughtSettings.butcherThoughtSpecific)
                {
                    if (bt.thought == null)
                        bt.thought = ThoughtDef.Named("ButcheredHumanlikeCorpse");
                    if (bt.knowThought == null)
                        bt.knowThought = ThoughtDef.Named("KnowButcheredHumanlikeCorpse");
                }

            if (alienRace.thoughtSettings.ateThoughtGeneral.thought == null)
                alienRace.thoughtSettings.ateThoughtGeneral.thought = ThoughtDef.Named("AteHumanlikeMeatDirect");
            if (alienRace.thoughtSettings.ateThoughtGeneral.ingredientThought == null)
                alienRace.thoughtSettings.ateThoughtGeneral.ingredientThought = ThoughtDef.Named("AteHumanlikeMeatAsIngredient");

            if (alienRace.thoughtSettings.ateThoughtSpecific != null)
                foreach (AteThought at in alienRace.thoughtSettings.ateThoughtSpecific)
                {
                    if (at.thought == null)
                        at.thought = ThoughtDef.Named("AteHumanlikeMeatDirect");
                    if (at.ingredientThought == null)
                        at.ingredientThought = ThoughtDef.Named("AteHumanlikeMeatAsIngredient");
                }
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

        public List<AlienTraitEntry> forcedRaceTraitEntries;
        public AlienPartGenerator alienPartGenerator = new AlienPartGenerator();
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
        public List<StartingColonistEntry> startingColonists;
    }

    public class PawnKindEntry
    {
        public List<PawnKindDef> kindDefs;
        public float chance;
    }

    public class StartingColonistEntry
    {
        public List<PawnKindEntry> pawnKindEntries;
        public List<FactionDef> factionDefs;
    }

    public class ThoughtSettings
    {
        public List<ThoughtDef> cannotReceiveThoughts;
        public bool cannotReceiveThoughtsAtAll = false;

        public ButcherThought butcherThoughtGeneral = new ButcherThought();
        public List<ButcherThought> butcherThoughtSpecific = new List<ButcherThought>();

        public AteThought ateThoughtGeneral = new AteThought();
        public List<AteThought> ateThoughtSpecific = new List<AteThought>();

        public List<ThoughtReplacer> replacerList;
    }

    public class ButcherThought
    {
        public List<ThingDef> raceList;
        public ThoughtDef thought;// = ThoughtDef.Named("ButcheredHumanlikeCorpse");
        public ThoughtDef knowThought;// = ThoughtDef.Named("KnowButcheredHumanlikeCorpse");
    }

    public class AteThought
    {
        public List<ThingDef> raceList;
        public ThoughtDef thought;// = ThoughtDef.Named("AteHumanlikeMeatDirect");
        public ThoughtDef ingredientThought;// = ThoughtDef.Named("AteHumanlikeMeatAsIngredient");
    }

    public class ThoughtReplacer
    {
        public ThoughtDef original;
        public ThoughtDef replacer;
    }

    public class RelationSettings
    {
        public float relationChanceModifierChild = 0.1f;
        public float relationChanceModifierExLover = 0.1f;
        public float relationChanceModifierExSpouse = 0.1f;
        public float relationChanceModifierFiance = 0.1f;
        public float relationChanceModifierLover = 0.1f;
        public float relationChanceModifierParent = 0.1f;
        public float relationChanceModifierSibling = 0.1f;
        public float relationChanceModifierSpouse = 0.1f;
    }

    public class RaceRestrictionSettings
    {
        public bool onlyUseRaceRestrictedApparel = false;
        public List<ThingDef> apparelList;

        public List<ResearchProjectRestrictions> researchList;

        public bool onlyUseRaceRestrictedWeapons = false;
        public List<ThingDef> weaponList;

        public bool onlyBuildRaceRestrictedBuildings = false;
        public List<string> buildingList;

        public bool onlyDoRaceRestrictedRecipes = false;
        public List<RecipeDef> recipeList;

        public bool onlyDoRaceRastrictedPlants = false;
        public List<ThingDef> plantList;

        public List<ConceptDef> conceptList;

        public List<WorkGiverDef> workGiverList;
    }

    public class ResearchProjectRestrictions
    {
        public List<ResearchProjectDef> projects;
        public List<ThingDef> apparelList;
    }
    
    static class GraphicPathsExtension
    {
        public static GraphicPaths GetCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef)
        {
            return list.FirstOrDefault(gp => gp.lifeStageDefs?.Contains(lifeStageDef) ?? false) ?? list.First();
        }
    }
}