using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AlienRace
{
    public sealed class PawnBioDef : Def
    {
        public string childhoodDef;
        public string adulthoodDef;
        public Backstory resolvedChildhood;
        public Backstory resolvedAdulthood;
        public GenderPossibility gender;
        public NameTriple name;
        public List<ThingDef> validRaces;
        public bool factionLeader;


        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string s in base.ConfigErrors())
                yield return s;
            if (BackstoryDatabase.TryGetWithIdentifier(this.childhoodDef, out this.resolvedChildhood))
                yield return "Childhood backstory not found";
            if (BackstoryDatabase.TryGetWithIdentifier(this.adulthoodDef, out this.resolvedAdulthood))
                yield return "Adulthood backstory not found";
        }

        public override void ResolveReferences()
        {
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
            bio.ResolveReferences();
            bio.PostLoad();

            if(!bio.ConfigErrors().Any())
                SolidBioDatabase.allBios.Add(bio);
            else
                Log.Error(this.defName + " has errors");
        }
    }
}