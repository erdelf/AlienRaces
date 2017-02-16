using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public class ThingDef_AlienRace : ThingDef
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

        public AlienPartGenerator alienpartgenerator;
        public ColorGenerator alienskincolorgen;
        public ColorGenerator alienhaircolorgen;
        public List<AlienTraitEntry> forcedRaceTraitEntries;
        public Vector2 CustomDrawSize = Vector2.one;
        public bool ImmuneToAge = false;
        public List<ThoughtDef> cannotReceiveThoughts;
        public bool onlyUseRacerestrictedApparel = false;
        public List<ThingDef> raceRestrictedApparel;

#pragma warning restore CS0649

        public override void ResolveReferences()
        {
            comps.Add(new CompProperties(typeof(CompAlien)));
            base.ResolveReferences();
        }
    }

    public class AlienTraitEntry
    {
        public string defname;
        public int degree = 0;
        public float chance = 100;
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
