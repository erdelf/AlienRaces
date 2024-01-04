namespace AlienRace
{
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    [StaticConstructorOnStartup]
    public class RaceSettings : Def
    {
        public PawnKindSettings                   pawnKindSettings    = new();
        public List<AlienPartGenerator.BodyAddon> universalBodyAddons = new();
    }

    public class PawnKindSettings
    {
        public List<PawnKindEntry>        alienslavekinds    = new();
        public List<PawnKindEntry>        alienrefugeekinds  = new();
        public List<FactionPawnKindEntry> startingColonists  = new();
        public List<FactionPawnKindEntry> alienwandererkinds = new();
    }

    public class PawnKindEntry
    {
        public List<PawnKindDef> kindDefs = new();
        public float             chance;
    }

    public class FactionPawnKindEntry
    {
        public List<PawnKindEntry> pawnKindEntries = new();
        public List<FactionDef>    factionDefs     = new();
    }
}
