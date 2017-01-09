using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Assembly_Races
{
    public static class GraphicGetter_NakedAlien
    {
        private const string NakedBodyTextureFolderPath = "Things/Pawn/Humanlike/Bodies/";
        private static BodyType bodyType;        

        public static Graphic GetNakedBodyGraphicAlien(Shader shader, Color skinColor, Gender gender, String userpath)
        {

            if (gender == Gender.Male)
            {
                bodyType = BodyType.Male;
            }
            else bodyType = BodyType.Female;

            string str = "Naked_" + bodyType.ToString();
            string path = userpath + str;
            return GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor);
        }
    }
}
