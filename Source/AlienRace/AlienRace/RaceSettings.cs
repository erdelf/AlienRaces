namespace AlienRace
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using Verse;

    [StaticConstructorOnStartup]
    public class RaceSettings : Def
    {
        public PawnKindSettings pawnKindSettings;
        public List<BackstoryTagItem> backstoryTagInsertion;

        static RaceSettings()
        {
            foreach (BackstoryTagItem bt in DefDatabase<RaceSettings>.AllDefs.SelectMany(selector: rs => rs.backstoryTagInsertion))
            {
                foreach (string backstory in bt.backstories)
                    if (BackstoryDatabase.TryGetWithIdentifier(identifier: backstory, bs: out Backstory bs))
                        bs.spawnCategories.AddRange(collection: bt.spawnCategories);
            }
        }
    }

    public class BackstoryTagItem
    {
        public List<string> backstories     = new List<string>();
        public List<string> spawnCategories = new List<string>();
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
