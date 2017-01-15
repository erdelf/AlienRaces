using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AlienRace
{
    public class AlienPartGenerator
    {
        
        public List<string> aliencrowntypes = new List<string> {};

        public List<BodyType> alienbodytypes = new List<BodyType>();

        public string AlienHeadTypeLoc;

        public bool UseGenderedHeads = true;

        public string RandomAlienHead(string userpath, Gender gender)
        {
            System.Random r = new System.Random();
            int index = r.Next(aliencrowntypes.Count);
            string genstring = "";
            if (UseGenderedHeads)
            {
                genstring = "Male_";
                if (gender == Gender.Female)
                {
                    genstring = "Female_";
                }
            }
            return AlienHeadTypeLoc = userpath + genstring + aliencrowntypes[index];            
        }
    }
}
