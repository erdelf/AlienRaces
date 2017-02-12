using RimWorld;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public static class AlienRaceUtilties
    {
        public static Graphic GetNakedGraphic(BodyType bodyType, Shader shader, Color skinColor, string userpath)
        {
            return GraphicDatabase.Get<Graphic_Multi>(userpath+"Naked_"+bodyType.ToString(), shader, Vector2.one, skinColor);
        }
    }
}
