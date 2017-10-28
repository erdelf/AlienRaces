using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public sealed partial class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string>() { "Average_Normal" };

        public List<BodyType> alienbodytypes = new List<BodyType>();

        public bool useGenderedHeads = true;
        public bool useGenderedBodies = false;

        public ColorGenerator alienskincolorgen;
        public ColorGenerator alienskinsecondcolorgen;
        public ColorGenerator alienhaircolorgen;
        public ColorGenerator alienhairsecondcolorgen;
        public bool useSkincolorForHair = false;

        public Vector2 customDrawSize = Vector2.one;
        public Vector2 customPortraitDrawSize = Vector2.one;

        public BodyPartDef headBodyPartDef;

        static Dictionary<Vector2, GraphicMeshSet[]> meshPools = new Dictionary<Vector2, GraphicMeshSet[]>();

        public GraphicMeshSet bodySet;
        public GraphicMeshSet headSet;
        public GraphicMeshSet hairSetAverage;
        public GraphicMeshSet hairSetNarrow;

        public GraphicMeshSet bodyPortraitSet;
        public GraphicMeshSet headPortraitSet;
        public GraphicMeshSet hairPortraitSetAverage;
        public GraphicMeshSet hairPortraitSetNarrow;

        public List<BodyAddon> bodyAddons = new List<BodyAddon>();

        public ThingDef_AlienRace alienProps;

        static MethodInfo meshInfo = AccessTools.Method(AccessTools.TypeByName("MeshMakerPlanes"), "NewPlaneMesh", new Type[] { typeof(Vector2), typeof(bool), typeof(bool), typeof(bool) });

        public string RandomAlienHead(string userpath, Pawn pawn) => GetAlienHead(userpath, (this.useGenderedHeads ? pawn.gender.ToString() : ""), pawn.GetComp<AlienComp>().crownType = this.aliencrowntypes[Rand.Range(0, this.aliencrowntypes.Count)]);

        public static string GetAlienHead(string userpath, string gender, string crowntype) => userpath.NullOrEmpty() ? "" : userpath + (userpath == GraphicPaths.vanillaHeadPath ? gender + "/" : "") + (!gender.NullOrEmpty() ? gender + "_" : "") + crowntype;

        public Graphic GetNakedGraphic(BodyType bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath, string gender) => GraphicDatabase.Get<Graphic_Multi>(GetNakedPath(bodyType, userpath, (this.useGenderedBodies ? gender : "")), shader, Vector2.one, skinColor, skinColorSecond);

        public static string GetNakedPath(BodyType bodyType, string userpath, string gender) => userpath + (!gender.NullOrEmpty() ? gender + "_" : "") + "Naked_" + bodyType.ToString();

        public Color SkinColor(Pawn alien, bool first = true)
        {
            AlienComp alienComp = alien.TryGetComp<AlienComp>();
            if (alienComp.skinColor == Color.clear)
            {
                alienComp.skinColor = (this.alienskincolorgen != null ? this.alienskincolorgen.NewRandomizedColor() : PawnSkinColors.GetSkinColor(alien.story.melanin));
                alienComp.skinColorSecond = (this.alienskinsecondcolorgen != null ? this.alienskinsecondcolorgen.NewRandomizedColor() : alienComp.skinColor);
            }
            return first ? alienComp.skinColor : alienComp.skinColorSecond;
        }

        public void GenerateMeshsAndMeshPools()
        {

            {
                if (!meshPools.Keys.Any(v => v.Equals(this.customDrawSize)))
                {
                    meshPools.Add(this.customDrawSize, new GraphicMeshSet[]
                        {
                                                            new GraphicMeshSet(1.5f * this.customDrawSize.x, 1.5f * this.customDrawSize.y), // bodySet
                                                            new GraphicMeshSet(1.5f * this.customDrawSize.x, 1.5f * this.customDrawSize.y), // headSet
                                                            new GraphicMeshSet(1.5f * this.customDrawSize.x, 1.5f * this.customDrawSize.y), // hairSetAverage
                                                            new GraphicMeshSet(1.3f * this.customDrawSize.x, 1.5f * this.customDrawSize.y), // hairSetNarrow
                        });
                }

                GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(this.customDrawSize))];

                this.bodySet = meshSet[0];
                this.headSet = meshSet[1];
                this.hairSetAverage = meshSet[2];
                this.hairSetNarrow = meshSet[3];
                this.bodyAddons.ForEach(ba =>
                {
                    ba.addonMesh = (Mesh) meshInfo.Invoke(null, new object[] { this.customDrawSize * 1.5f, false, false, false });
                    ba.addonMeshFlipped = (Mesh) meshInfo.Invoke(null, new object[] { this.customDrawSize * 1.5f, true, false, false });
                });
            }
            {
                if (!meshPools.Keys.Any(v => v.Equals(this.customPortraitDrawSize)))
                {
                    meshPools.Add(this.customPortraitDrawSize, new GraphicMeshSet[]
                        {
                                                            new GraphicMeshSet(1.5f * this.customPortraitDrawSize.x, 1.5f * this.customPortraitDrawSize.y), // bodySet
                                                            new GraphicMeshSet(1.5f * this.customPortraitDrawSize.x, 1.5f * this.customPortraitDrawSize.y), // headSet
                                                            new GraphicMeshSet(1.5f * this.customPortraitDrawSize.x, 1.5f * this.customPortraitDrawSize.y), // hairSetAverage
                                                            new GraphicMeshSet(1.3f * this.customPortraitDrawSize.x, 1.5f * this.customPortraitDrawSize.y), // hairSetNarrow
                        });
                }

                GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(this.customPortraitDrawSize))];

                this.bodyPortraitSet = meshSet[0];
                this.headPortraitSet = meshSet[1];
                this.hairPortraitSetAverage = meshSet[2];
                this.hairPortraitSetNarrow = meshSet[3];
                this.bodyAddons.ForEach(ba =>
                {
                    ba.addonPortraitMesh = (Mesh) meshInfo.Invoke(null, new object[] { this.customPortraitDrawSize * 1.5f, false, false, false });
                    ba.addonPortraitMeshFlipped = (Mesh) meshInfo.Invoke(null, new object[] { this.customPortraitDrawSize * 1.5f, true, false, false });
                });
            }
            {
                this.bodyAddons.Do(ba =>
                {
                    if (ba.variantCount == 0)
                    {
                        while (ContentFinder<Texture2D>.Get(ba.path + (ba.variantCount == 0 ? "" : ba.variantCount.ToString()) + "_back", false) != null)
                            ba.variantCount++;
                        Log.Message("Variants found for " + ba.path + ": " + ba.variantCount.ToString());
                        if (ba.hediffGraphics != null)
                            foreach (BodyAddonHediffGraphic bahg in ba.hediffGraphics)
                            {
                                if (bahg.variantCount == 0)
                                {
                                    while (ContentFinder<Texture2D>.Get(bahg.path + (bahg.variantCount == 0 ? "" : bahg.variantCount.ToString()) + "_back", false) != null)
                                        bahg.variantCount++;
                                    Log.Message("Variants found for " + bahg.path + ": " + bahg.variantCount.ToString());
                                }
                            }
                        if (ba.backstoryGraphics != null)
                            foreach (BodyAddonBackstoryGraphic babg in ba.backstoryGraphics)
                            {
                                if (babg.variantCount == 0)
                                {
                                    while (ContentFinder<Texture2D>.Get(babg.path + (babg.variantCount == 0 ? "" : babg.variantCount.ToString()) + "_back", false) != null)
                                        babg.variantCount++;
                                    Log.Message("Variants found for " + babg.path + ": " + babg.variantCount.ToString());
                                }
                            }


                    }
                });
            }
        }

        public class AlienComp : ThingComp
        {
            public bool fixGenderPostSpawn;
            public Color skinColor;
            public Color skinColorSecond;
            public Color hairColorSecond;
            public string crownType;

            public List<Graphic> addonGraphics;

            public override void PostExposeData()
            {
                base.PostExposeData();
                Scribe_Values.Look(ref this.fixGenderPostSpawn, "fixAlienGenderPostSpawn", false);
                Scribe_Values.Look(ref this.skinColor, "skinColorAlien");
                Scribe_Values.Look(ref this.skinColorSecond, "skinColorSecondAlien");
                Scribe_Values.Look(ref this.hairColorSecond, "hairColorSecondAlien");
                Scribe_Values.Look(ref this.crownType, "crownType");
            }
        }
    }
}