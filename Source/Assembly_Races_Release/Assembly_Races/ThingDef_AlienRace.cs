using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace AlienRace
{
    public class Thingdef_AlienRace : ThingDef
        {
            public bool IsHumanoidAlien = true;
            public AlienHairTypes HasHair = AlienHairTypes.Vanilla;
            public bool CustomSkinColors=false;
            public string NakedBodyGraphicLocation;
            public string NakedHeadGraphicLocation;
            public string DesiccatedGraphicLocation;
            public string SkullGraphicLocation;
            public int GetsGreyAt;
            public bool CustomGenderDistribution = false;
            public float MaleGenderProbability = 0.5f;
            public bool PawnsSpecificBackstories = false;
            public AlienPartGenerator alienpartgenerator;
            public ColorGenerator alienskincolorgen;
            public ColorGenerator alienhaircolorgen;
            public List<AlienTraitEntry> ForcedRaceTraitEntries;
            public Vector2 CustomDrawSize;
            public bool Headless = false;              
    }
    
}
