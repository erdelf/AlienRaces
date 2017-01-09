using Verse;

namespace AlienRace
{
    public class CompRestritctedRace : ThingComp
    {
        public CompProperties_RestrictedRace Props
        {
            get
            {
                return (CompProperties_RestrictedRace)this.props;
            }
        }
    }
}
