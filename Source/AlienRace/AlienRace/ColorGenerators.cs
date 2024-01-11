namespace AlienRace
{
    using System.Collections.Generic;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public interface IAlienChannelColorGenerator
    {
        public List<Color> AvailableColors(Pawn pawn);

        public List<ColorGenerator> AvailableGenerators(Pawn pawn);
    }

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

    public abstract class ChannelColorGenerator_PawnBased : ColorGenerator, IAlienChannelColorGenerator
    {
        public override Color NewRandomizedColor() =>
            Color.clear;

        public abstract Color                NewRandomizedColor(Pawn  pawn);
        public virtual  List<Color>          AvailableColors(Pawn     pawn) => [];
        public virtual  List<ColorGenerator> AvailableGenerators(Pawn pawn) => [];
    }

    public class ChannelColorGenerator_GenderBased : ChannelColorGenerator_PawnBased
    {
        public Dictionary<Gender, ColorGenerator> colors = [];

        public override Color NewRandomizedColor(Pawn pawn) => 
            this.GetGenerator(pawn.gender).NewRandomizedColor();

        public override List<Color> AvailableColors(Pawn pawn) => [];

        public override List<ColorGenerator> AvailableGenerators(Pawn pawn) => [this.GetGenerator(pawn.gender)];

        public ColorGenerator GetGenerator(Gender gender)
        {
            if (!this.colors.ContainsKey(gender))
                gender = this.colors.Keys.RandomElement();

            return this.colors[gender];
        }
    }
}