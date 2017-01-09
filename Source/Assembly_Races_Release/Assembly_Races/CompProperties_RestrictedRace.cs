using Verse;

namespace AlienRace
{
    public class CompProperties_RestrictedRace : CompProperties
    {
        public string RestrictedToRace = "Human";

        public CompProperties_RestrictedRace()
        {
            this.compClass = typeof(CompRestritctedRace);
        }
    }
}
