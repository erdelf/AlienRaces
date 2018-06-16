using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlienRace
{
    public class PawnBioDef : Def
    {
        public string childhoodDef;
        public string adulthoodDef;
        public Backstory resolvedChildhood;
        public Backstory resolvedAdulthood;
        public GenderPossibility gender;
        public NameTriple name;
        public List<ThingDef> validRaces;
        public bool factionLeader;
        public List<string> forcedHediffs = new List<string>();
        public List<string> forcedItems = new List<string>();

        public override void ResolveReferences()
        {
            if (!BackstoryDatabase.TryGetWithIdentifier(identifier: this.childhoodDef, bs: out this.resolvedChildhood))
                Log.Error(text: "Error in " + this.defName + ": Childhood backstory not found");
            if (!BackstoryDatabase.TryGetWithIdentifier(identifier: this.adulthoodDef, bs: out this.resolvedAdulthood))
                Log.Error(text: "Error in " + this.defName + ": Adulthood backstory not found");

            base.ResolveReferences();

            if (this.resolvedAdulthood.slot != BackstorySlot.Adulthood || this.resolvedChildhood.slot != BackstorySlot.Childhood)
                return;

            PawnBio bio = new PawnBio()
            {
                gender = this.gender,
                name = this.name,
                childhood = this.resolvedChildhood,
                adulthood = this.resolvedAdulthood,
                pirateKing = this.factionLeader
            };

            if (this.resolvedAdulthood.spawnCategories.Count == 1 && this.resolvedAdulthood.spawnCategories[index: 0] == "Trader")
                this.resolvedAdulthood.spawnCategories.Add(item: "Civil");

            if(!bio.ConfigErrors().Any())
                SolidBioDatabase.allBios.Add(item: bio);
            else
                Log.Error(text: this.defName + " has errors");
        }
    }
}