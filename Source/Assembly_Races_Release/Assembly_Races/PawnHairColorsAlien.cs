using RimWorld;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public static class PawnHairColorsAlien

    {
        public static Color RandomHairColor(Color skinColor, int ageYears, int getsGrayAt)
        {
            if (Rand.Value < 0.02f)
            {
                return new Color(Rand.Value, Rand.Value, Rand.Value);
            }
            if (ageYears > (getsGrayAt + Rand.Range(-0.3f*getsGrayAt, 0.1f*getsGrayAt)))
            {
                float num = GenMath.SmootherStep(40f, 75f, (float)ageYears);
                if (Rand.Value < num)
                {
                    float num2 = Rand.Range(0.65f, 0.85f);
                    return new Color(num2, num2, num2);
                }
            }
            if (PawnSkinColors.IsDarkSkin(skinColor) || Rand.Value < 0.5f)
            {
                float value = Rand.Value;
                if (value < 0.25f)
                {
                    return new Color(0.2f, 0.2f, 0.2f);
                }
                if (value < 0.5f)
                {
                    return new Color(0.31f, 0.28f, 0.26f);
                }
                if (value < 0.75f)
                {
                    return new Color(0.25f, 0.2f, 0.15f);
                }
                return new Color(0.3f, 0.2f, 0.1f);
            }
            else
            {
                float value2 = Rand.Value;
                if (value2 < 0.25f)
                {
                    return new Color(0.3529412f, 0.227450982f, 0.1254902f);
                }
                if (value2 < 0.5f)
                {
                    return new Color(0.5176471f, 0.3254902f, 0.184313729f);
                }
                if (value2 < 0.75f)
                {
                    return new Color(0.75686276f, 0.572549045f, 0.333333343f);
                }
                return new Color(0.929411769f, 0.7921569f, 0.6117647f);
            }
        }
    }
}
