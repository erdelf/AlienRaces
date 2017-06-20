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
        public Backstory childhoodDef;
        public Backstory adulthoodDef;
        public GenderPossibility gender;
        public NameTriple name;
        public List<ThingDef> validRaces;
        public bool factionLeader;

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            if (this.adulthoodDef.slot != BackstorySlot.Adulthood || this.childhoodDef.slot != BackstorySlot.Childhood)
                return;

            PawnBio bio = new PawnBio()
            {
                gender = this.gender,
                name = this.name,
                childhood = this.childhoodDef,
                adulthood = this.adulthoodDef,
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