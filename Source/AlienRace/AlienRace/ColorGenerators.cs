namespace AlienRace
{
    using System.Collections.Generic;
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

        public void GetInfo(out string channel, out bool first)
        {
            string[] split = this.colorChannel.Split('_');

            channel = split[0];
            first   = split[1] == "1";
        }
    }

    public abstract class ColorGenerator_PawnBased : ColorGenerator
    {
        public override Color NewRandomizedColor() =>
            Color.clear;

        public abstract Color NewRandomizedColor(Pawn pawn);
    }

    public class ColorGenerator_GenderBased : ColorGenerator_PawnBased
    {
        public Dictionary<Gender, ColorGenerator> colors = new();

        public override Color NewRandomizedColor(Pawn pawn)
        {
            Gender gender = pawn.gender;

            if (!this.colors.ContainsKey(pawn.gender)) 
                gender = this.colors.Keys.RandomElement();

            return this.colors[gender].NewRandomizedColor();
        }
    }
}