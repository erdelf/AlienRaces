namespace AlienRace
{
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    [StaticConstructorOnStartup]
    public class RaceSettings : Def
    {
        public PawnKindSettings                   pawnKindSettings;
        public List<AlienPartGenerator.BodyAddon> universalBodyAddons = new List<AlienPartGenerator.BodyAddon>();
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
        public List<PawnKindDef> kindDefs = new List<PawnKindDef>();
        public float        chance;
    }

    public class FactionPawnKindEntry
    {
        public List<PawnKindEntry> pawnKindEntries = new List<PawnKindEntry>();
        public List<FactionDef>        factionDefs = new List<FactionDef>();
    }
}
