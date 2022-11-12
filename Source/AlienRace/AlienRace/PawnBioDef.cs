namespace AlienRace
{
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    public class PawnBioDef : Def
    {
        public BackstoryDef childhood;
        public BackstoryDef adulthood;
        public GenderPossibility gender;
        public NameTriple name;
        public List<ThingDef> validRaces;
        public bool factionLeader;
        public List<HediffDef> forcedHediffs = new List<HediffDef>();
        public List<ThingDefCountRangeClass> forcedItems = new List<ThingDefCountRangeClass>();

        public override IEnumerable<string> ConfigErrors()
        {
            if(this.childhood == null)
                yield return "Error in " + this.defName + ": Childhood backstory not found";
            if (this.adulthood == null)
                yield return "Error in " + this.defName + ": Childhood backstory not found";

            foreach (string configError in base.ConfigErrors())
                yield return configError;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            
            PawnBio bio = new PawnBio
                          {
                              gender     = this.gender,
                              name       = this.name,
                              childhood  = this.childhood,
                              adulthood  = this.adulthood,
                              pirateKing = this.factionLeader
                          };

            if (this.adulthood.spawnCategories.Count == 1 && this.adulthood.spawnCategories[index: 0] == "Trader")
                this.adulthood.spawnCategories.Add(item: "Civil");

            SolidBioDatabase.allBios.Add(bio);
        }
    }
}