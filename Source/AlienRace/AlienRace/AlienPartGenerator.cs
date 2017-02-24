using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string>() { "Average_Normal" };

        public List<BodyType> alienbodytypes = new List<BodyType>();

        public bool UseGenderedHeads = true;

        public ColorGenerator alienskincolorgen;
        public ColorGenerator alienhaircolorgen;

        public Vector2 CustomDrawSize = Vector2.one;
        public Vector2 CustomPortraitDrawSize = Vector2.one;

        static Dictionary<Vector2, GraphicMeshSet[]> meshPools = new Dictionary<Vector2, GraphicMeshSet[]>();

        public GraphicMeshSet bodySet;
        public GraphicMeshSet headSet;
        public GraphicMeshSet hairSetAverage;
        public GraphicMeshSet hairSetNarrow;

        public GraphicMeshSet bodyPortraitSet;
        public GraphicMeshSet headPortraitSet;
        public GraphicMeshSet hairPortraitSetAverage;
        public GraphicMeshSet hairPortraitSetNarrow;

        public string RandomAlienHead(string userpath, Gender gender)
        {
            return userpath + (UseGenderedHeads ? gender.ToString() + "_" : "") + aliencrowntypes[Rand.Range(0, aliencrowntypes.Count)];
        }

        public static Graphic GetNakedGraphic(BodyType bodyType, Shader shader, Color skinColor, string userpath)
        {
            return GraphicDatabase.Get<Graphic_Multi>(userpath + "Naked_" + bodyType.ToString(), shader, Vector2.one, skinColor);
        }

        public Color SkinColor(Pawn alien)
        {
            AlienComp alienComp = alien.TryGetComp<AlienComp>();
            if (alienComp.skinColor == Color.clear)
                alienComp.skinColor = (alienskincolorgen != null ? alienskincolorgen.NewRandomizedColor() : PawnSkinColors.GetSkinColor(alien.story.melanin));
            return alienComp.skinColor;
        }

        public AlienPartGenerator()
        {
            LongEventHandler.QueueLongEvent(() =>
                { 
                    {
                        if (!meshPools.Keys.Any(v => v.Equals(CustomDrawSize)))
                            meshPools.Add(CustomDrawSize, new GraphicMeshSet[]
                                {
                                new GraphicMeshSet(1.5f * CustomDrawSize.x, 1.5f * CustomDrawSize.y), // bodySet
                                new GraphicMeshSet(1.5f * CustomDrawSize.x, 1.5f * CustomDrawSize.y), // headSet
                                new GraphicMeshSet(1.5f * CustomDrawSize.x, 1.5f * CustomDrawSize.y), // hairSetAverage
                                new GraphicMeshSet(1.3f * CustomDrawSize.x, 1.5f * CustomDrawSize.y), // hairSetNarrow
                                });

                        GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(CustomDrawSize))];

                        bodySet = meshSet[0];
                        headSet = meshSet[1];
                        hairSetAverage = meshSet[2];
                        hairSetNarrow = meshSet[3];
                    }
                    {
                        if (!meshPools.Keys.Any(v => v.Equals(CustomPortraitDrawSize)))
                            meshPools.Add(CustomPortraitDrawSize, new GraphicMeshSet[]
                                {
                                new GraphicMeshSet(1.5f * CustomPortraitDrawSize.x, 1.5f * CustomPortraitDrawSize.y), // bodySet
                                new GraphicMeshSet(1.5f * CustomPortraitDrawSize.x, 1.5f * CustomPortraitDrawSize.y), // headSet
                                new GraphicMeshSet(1.5f * CustomPortraitDrawSize.x, 1.5f * CustomPortraitDrawSize.y), // hairSetAverage
                                new GraphicMeshSet(1.3f * CustomPortraitDrawSize.x, 1.5f * CustomPortraitDrawSize.y), // hairSetNarrow
                                });

                        GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(CustomPortraitDrawSize))];

                        bodyPortraitSet = meshSet[0];
                        headPortraitSet = meshSet[1];
                        hairPortraitSetAverage = meshSet[2];
                        hairPortraitSetNarrow = meshSet[3];
                    }
                }, "meshSetAlien", false, null);
        }

        public class AlienComp : ThingComp
        {
            public bool fixGenderPostSpawn;
            public Color skinColor;

            public override void PostExposeData()
            {
                base.PostExposeData();
                Scribe_Values.LookValue<bool>(ref fixGenderPostSpawn, "fixAlienGenderPostSpawn", false);
                Scribe_Values.LookValue<Color>(ref skinColor, "skinColorAlien");
            }
        }
    }
}