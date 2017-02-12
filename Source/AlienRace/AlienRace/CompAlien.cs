using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AlienRace
{

    public class CompAlien : ThingComp
    {
        CompProperties_Alien alienProps;
        Pawn alien;

        static Dictionary<Vector2, GraphicMeshSet[]> meshPools = new Dictionary<Vector2, GraphicMeshSet[]>();

        public GraphicMeshSet bodySet;
        public GraphicMeshSet headSet;
        public GraphicMeshSet hairSetAverage;
        public GraphicMeshSet hairSetNarrow;

        public Color skinColor;
        public Color hairColor;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            alienProps = props as CompProperties_Alien;
            alien = parent as Pawn;

            skinColor = alienProps.skincolorgen != null ? alienProps.skincolorgen.NewRandomizedColor() : alien.story.SkinColor;
            hairColor = alienProps.haircolorgen != null ? alienProps.skincolorgen.NewRandomizedColor() : Color.clear;




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


        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            if (hairColor == Color.clear)
                hairColor = alien.story.hairColor; 


            float grey = Rand.Range(0.65f, 0.85f);
            alien.story.hairColor = alienProps.GetsGreyAt <= alien.ageTracker.AgeBiologicalYears ? new Color(grey,grey,grey) : hairColor;

            if (!alienProps.NakedHeadGraphicLocation.NullOrEmpty())
                typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(alien.story,
                    alienProps.partgenerator.RandomAlienHead(alienProps.NakedHeadGraphicLocation, alien.gender));
        }
    }

    public class CompProperties_Alien : CompProperties
    {
        public bool HasHair = true;
        public string NakedBodyGraphicLocation = "Things/Pawn/Humanlike/Bodies/";
        public string NakedHeadGraphicLocation = "Things/Pawn/Humanlike/Heads/";
        public string DesiccatedGraphicLocation = "Things/Pawn/Humanlike/HumanoidDessicated";
        public string SkullGraphicLocation = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        public int GetsGreyAt = 40;
        public float MaleGenderProbability = 0.5f;
        public bool PawnsSpecificBackstories = false;
        public bool RandomlyGenerated = true;
        public AlienPartGenerator partgenerator;
        public ColorGenerator skincolorgen;
        public ColorGenerator haircolorgen;
        public List<AlienTraitEntry> ForcedRaceTraitEntries;
        public Vector2 CustomDrawSize = Vector2.one;

        public CompProperties_Alien()
        {
            compClass = typeof(CompAlien);
        }
    }
}