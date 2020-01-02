using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienRace
{
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [UsedImplicitly]
    public class ColorGenerator_SkinColorMelanin : ColorGenerator
    {
        public float minMelanin = 0f;
        public float maxMelanin = 1f;

        public override Color NewRandomizedColor() => 
            PawnSkinColors.GetSkinColor(melanin: Rand.Range(min: this.minMelanin, max: this.maxMelanin));
    }
}
