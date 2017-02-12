using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AlienRace
{
    public class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string> { };

        public List<BodyType> alienbodytypes = new List<BodyType>();

        public string AlienHeadTypeLoc;

        public bool UseGenderedHeads = true;

        public string RandomAlienHead(string userpath, Gender gender)
        {
            return AlienHeadTypeLoc = userpath + (UseGenderedHeads ? gender.ToString() + "_" : "") + aliencrowntypes[Rand.Range(0, aliencrowntypes.Count)];
        }
    }
}