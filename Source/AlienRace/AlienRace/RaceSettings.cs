namespace AlienRace
{
    using System.Collections.Generic;
    using Verse;

    [StaticConstructorOnStartup]
    public class RaceSettings : Def
    {
        public PawnKindSettings pawnKindSettings;

    }

    public class PawnKindSettings
    {
        public List<PawnKindEntry>        alienslavekinds = new List<PawnKindEntry>();
        public List<PawnKindEntry>        alienrefugeekinds = new List<PawnKindEntry>();
        public List<FactionPawnKindEntry> startingColonists = new List<FactionPawnKindEntry>();
        public List<FactionPawnKindEntry> alienwandererkinds = new List<FactionPawnKindEntry>();
    }

    public class PawnKindEntry
    {
        public List<string> kindDefs = new List<string>();
        public float        chance;
    }

    public class FactionPawnKindEntry
    {
        public List<PawnKindEntry> pawnKindEntries = new List<PawnKindEntry>();
        public List<string>        factionDefs = new List<string>();
    }
}
