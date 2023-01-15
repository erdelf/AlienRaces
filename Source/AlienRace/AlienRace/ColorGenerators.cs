namespace AlienRace
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class ColorGenerator_SkinColorMelanin : ColorGenerator
    {
        public float minMelanin     = 0f;
        public float maxMelanin     = 1f;
        public bool  naturalMelanin = false;

        public override Color NewRandomizedColor() => 
            PawnSkinColors.GetSkinColor(Rand.Range(this.minMelanin, this.maxMelanin));
    }

    public class ColorGenerator_CustomAlienChannel : ColorGenerator
    {
        public string colorChannel;

        public override Color NewRandomizedColor() => 
            Color.clear;
    }

    public abstract class ColorGenerator_PawnBased : ColorGenerator
    {
        public override Color NewRandomizedColor() => 
            Color.clear;

        public abstract Color NewRandomizedColor(Pawn pawn);
    }
}
