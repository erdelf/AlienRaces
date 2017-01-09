using System.Collections.Generic;
using UnityEngine;

namespace AlienRace
{
    public class AlienKey
    {
        public AlienKey(Thingdef_AlienRace race, List<Color> hairColors, List<Color> skinColors)
        {
            this.Race = race;
            this.AlienSkinColors = skinColors;
            this.AlienHairColors = hairColors;
        }

        public Thingdef_AlienRace Race;

        public List<Color> AlienSkinColors = new List<Color>();

        public List<Color> AlienHairColors = new List<Color>();        

    }
}
