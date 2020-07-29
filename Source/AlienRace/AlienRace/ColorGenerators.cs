namespace AlienRace
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class ColorGenerator_SkinColorMelanin : ColorGenerator
    {
        public float minMelanin = 0f;
        public float maxMelanin = 1f;

        public override Color NewRandomizedColor() => 
            PawnSkinColors.GetSkinColor(melanin: Rand.Range(min: this.minMelanin, max: this.maxMelanin));
    }

    public class ColorGenerator_CustomAlienChannel : ColorGenerator
    {
        public string colorChannel;

        public override Color NewRandomizedColor() => 
            Color.clear;
        /*
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.colorChannel = ParseHelper.FromString<string>(xmlRoot.FirstChild.Value);
        }*/
    }
}
