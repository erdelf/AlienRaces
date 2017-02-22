using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public class ThingDef_AlienRace : ThingDef
    {
#pragma warning disable CS0649
        public AlienSettings alienRace;
#pragma warning restore CS0649

        public override void ResolveReferences()
        {
            comps.Add(new CompProperties(typeof(AlienPartGenerator.AlienComp)));
            base.ResolveReferences();
        }
    }

    public class AlienSettings
    {
        //#pragma warning disable CS0649
        //#pragma warning restore CS0649

        public GeneralSettings generalSettings = new GeneralSettings();
        public HairSettings hairSettings = new HairSettings();
        public List<GraphicPaths> graphicPaths = new List<GraphicPaths>();
        public PawnKindSettings pawnKindSettings = new PawnKindSettings();
        public ThoughtSettings thoughtSettings = new ThoughtSettings();
        public RelationSettings relationSettings = new RelationSettings();
        public RaceRestrictionSettings raceRestriction = new RaceRestrictionSettings();
    }

    public class GeneralSettings
    {
        public float MaleGenderProbability = 0.5f;
        public bool PawnsSpecificBackstories = false;
        public bool ImmuneToAge = false;

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

    public class HairSettings
    {
        public bool HasHair = true;
        public List<string> hairTags;
        public int GetsGreyAt = 40;
    }

    public class RaceRestrictionSettings
    {
        public bool onlyUseRacerestrictedApparel = false;
        public List<ThingDef> raceRestrictedApparel;
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

    static class GraphicPathsExtension
    {
        public static GraphicPaths getCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef)
        {
            return list.FirstOrDefault(gp => gp.lifeStageDefs.Contains(lifeStageDef)) ?? list.First();
        }
    }

    public class GraphicPaths
    {
        public List<LifeStageDef> lifeStageDefs;

        public string body = "Things/Pawn/Humanlike/Bodies/";
        public string head = "Things/Pawn/Humanlike/Heads/";
        public string skeleton = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string skull = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
    }

    public class ThoughtSettings
    {
        public List<ThoughtDef> cannotReceiveThoughts;

        public ButcherThought butcherThoughtGeneral = new ButcherThought();
        public List<ButcherThought> butcherThoughtSpecific = new List<ButcherThought>();

        public AteThought ateThoughtGeneral = new AteThought();
        public List<AteThought> ateThoughtSpecific = new List<AteThought>();
    }

    public class ButcherThought
    {
        public List<ThingDef> raceList;
        public ThoughtDef butcherThought = ThoughtDefOf.ButcheredHumanlikeCorpse;
        public ThoughtDef butcherKnowThought = ThoughtDefOf.KnowButcheredHumanlikeCorpse;
    }

    public class AteThought
    {
        public List<ThingDef> raceList;
        public ThoughtDef thought = ThoughtDefOf.AteHumanlikeMeatDirect;
        public ThoughtDef ingredientThought = ThoughtDefOf.AteHumanlikeMeatAsIngredient;
    }

    public class RelationSettings
    {
        public int relationChanceModifierChild = 100;
        public int relationChanceModifierExLover = 100;
        public int relationChanceModifierExSpouse = 100;
        public int relationChanceModifierFiance = 100;
        public int relationChanceModifierLover = 100;
        public int relationChanceModifierParent = 100;
        public int relationChanceModifierSibling = 100;
        public int relationChanceModifierSpouse = 100;
    }
}