using RimWorld;
using System.Collections.Generic;
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
#pragma warning disable CS0649
        public bool HasHair = true;
        public string NakedBodyGraphicLocation = "Things/Pawn/Humanlike/Bodies/";
        public string NakedHeadGraphicLocation = "Things/Pawn/Humanlike/Heads/";
        public string DesiccatedGraphicLocation = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string SkullGraphicLocation = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public int GetsGreyAt = 40;
        public float MaleGenderProbability = 0.5f;
        public bool PawnsSpecificBackstories = false;
        public List<PawnKindEntry> alienslavekinds;
        public List<PawnKindEntry> alienrefugeekinds;
        public List<StartingColonistEntry> startingColonists;

        public AlienPartGenerator alienPartGenerator = new AlienPartGenerator();
        public List<AlienTraitEntry> forcedRaceTraitEntries;
        public bool ImmuneToAge = false;
        public List<ThoughtDef> cannotReceiveThoughts;
        public bool onlyUseRacerestrictedApparel = false;
        public List<ThingDef> raceRestrictedApparel;
        public List<string> hairTags;

        public ThoughtDef butcherThoughtSame = ThoughtDefOf.ButcheredHumanlikeCorpse;
        public ThoughtDef butcherKnowThoughtSame = ThoughtDefOf.KnowButcheredHumanlikeCorpse;
        public ThoughtDef butcherThoughtDifferent = ThoughtDefOf.ButcheredHumanlikeCorpse;
        public ThoughtDef butcherKnowThoughtDifferent = ThoughtDefOf.KnowButcheredHumanlikeCorpse;
#pragma warning restore CS0649
    }

    public class AlienTraitEntry
    {
        public string defname;
        public int degree = 0;
        public float chance = 100;
        public float commonalityMale=-1f;
        public float commonalityFemale=-1f;
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
}
