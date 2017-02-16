using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlienRace
{

    public class CompAlien : ThingComp
    {
        ThingDef_AlienRace alienProps;
        Pawn alien;

        static Dictionary<Vector2, GraphicMeshSet[]> meshPools = new Dictionary<Vector2, GraphicMeshSet[]>();

        public GraphicMeshSet bodySet;
        public GraphicMeshSet headSet;
        public GraphicMeshSet hairSetAverage;
        public GraphicMeshSet hairSetNarrow;

        public Color skinColor;
        public Color hairColor;

        public bool fixGenderPostSpawn;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            alien = parent as Pawn;

            alienProps = alien.def as ThingDef_AlienRace;
            
            hairColor = alienProps.alienhaircolorgen != null ? alienProps.alienskincolorgen.NewRandomizedColor() : Color.clear;
            
            {
                if (!meshPools.Keys.Any(v => v.Equals(alienProps.CustomDrawSize)))
                    meshPools.Add(alienProps.CustomDrawSize, new GraphicMeshSet[]
                        {
                            new GraphicMeshSet(1.5f * alienProps.CustomDrawSize.x, 1.5f * alienProps.CustomDrawSize.y), // bodySet
                            new GraphicMeshSet(1.5f * alienProps.CustomDrawSize.x, 1.5f * alienProps.CustomDrawSize.y), // headSet
                            new GraphicMeshSet(1.5f * alienProps.CustomDrawSize.x, 1.5f * alienProps.CustomDrawSize.y), // hairSetAverage
                            new GraphicMeshSet(1.3f * alienProps.CustomDrawSize.x, 1.5f * alienProps.CustomDrawSize.y), // hairSetNarrow
                        });

                GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(alienProps.CustomDrawSize))];

                bodySet = meshSet[0];
                headSet = meshSet[1];
                hairSetAverage = meshSet[2];
                hairSetNarrow = meshSet[3];
            }
        }

        


        public Color SkinColor
        {
            get
            {
                if (skinColor == Color.clear)
                    skinColor = alienProps.alienskincolorgen != null ? alienProps.alienskincolorgen.NewRandomizedColor() : PawnSkinColors.GetSkinColor(alien.story.melanin);

                //Log.Message(alien.NameStringShort + "\n" + skinColor.ToString() + "\n" + StackTraceUtility.ExtractStackTrace());
                return skinColor;
            }
        }
    }
}