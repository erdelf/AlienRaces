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
    public partial class AlienPartGenerator
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

        public Vector2 headOffset = Vector2.zero;

        public Vector2 customDrawSize = Vector2.one;
        public Vector2 customPortraitDrawSize = Vector2.one;
        public Vector2 customHeadDrawSize = Vector2.zero;
        public Vector2 customPortraitHeadDrawSize = Vector2.zero;

        public BodyPartDef headBodyPartDef;

        static Dictionary<Vector2, AlienGraphicMeshSet> meshPools = new Dictionary<Vector2, AlienGraphicMeshSet>();

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
            if (this.customHeadDrawSize == Vector2.zero)
                this.customHeadDrawSize = this.customDrawSize;
            if (this.customPortraitHeadDrawSize == Vector2.zero)
                this.customPortraitHeadDrawSize = this.customPortraitDrawSize;

            void AddMeshSet(Vector2 drawSize, Vector2 headDrawSize)
            {
                if (!meshPools.Keys.Any(v => v.Equals(drawSize)))
                {
                    meshPools.Add(drawSize, new AlienGraphicMeshSet()
                    {
                        bodySet = new GraphicMeshSet(1.5f * drawSize.x, 1.5f * drawSize.y), // bodySet
                        headSet = new GraphicMeshSet(1.5f * headDrawSize.x, 1.5f * headDrawSize.y), // headSet
                        hairSetAverage = new GraphicMeshSet(1.5f * headDrawSize.x, 1.5f * headDrawSize.y), // hairSetAverage
                        hairSetNarrow = new GraphicMeshSet(1.3f * headDrawSize.x, 1.5f * headDrawSize.y), // hairSetNarrow
                        addonMesh = (Mesh) meshInfo.Invoke(null, new object[] { drawSize * 1.5f, false, false, false }),
                        addonMeshFlipped = (Mesh) meshInfo.Invoke(null, new object[] { drawSize * 1.5f, true, false, false })
                    });
                }
            }

            foreach (GraphicPaths graphicsPath in this.alienProps.alienRace.graphicPaths.Concat(new GraphicPaths() { customDrawSize = this.customDrawSize, customHeadDrawSize = this.customHeadDrawSize, customPortraitDrawSize = this.customPortraitDrawSize, customPortraitHeadDrawSize = this.customPortraitHeadDrawSize }))
                /*.Select(gp => gp.customDrawSize).
                Concat(this.alienProps.alienRace.graphicPaths.Select(gp => gp.customPortraitDrawSize)).Add(this.customDrawSize).Add(this.customPortraitDrawSize))*/
            {
                AddMeshSet(graphicsPath.customDrawSize, graphicsPath.customHeadDrawSize);
                AddMeshSet(graphicsPath.customPortraitDrawSize, graphicsPath.customPortraitHeadDrawSize);
            }

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


        public class AlienComp : ThingComp
        {
            public bool fixGenderPostSpawn;
            public Color skinColor;
            public Color skinColorSecond;
            public Color hairColorSecond;
            public string crownType;
            public Vector2 customDrawSize = Vector2.one;
            public Vector2 customPortraitDrawSize = Vector2.one;
            public AlienGraphicMeshSet alienGraphics;
            public AlienGraphicMeshSet alienPortraitGraphics;
            public List<Graphic> addonGraphics;
            public List<int> addonVariants;

            public override void PostSpawnSetup(bool respawningAfterLoad)
            {
                base.PostSpawnSetup(respawningAfterLoad);
                AlienPartGenerator apg = (this.parent.def as ThingDef_AlienRace).alienRace.generalSettings.alienPartGenerator;
                this.customDrawSize = apg.customDrawSize;
                this.customPortraitDrawSize = apg.customPortraitDrawSize;
            }

            public override void PostExposeData()
            {
                base.PostExposeData();
                Scribe_Values.Look(ref this.fixGenderPostSpawn, "fixAlienGenderPostSpawn", false);
                Scribe_Values.Look(ref this.skinColor, "skinColorAlien");
                Scribe_Values.Look(ref this.skinColorSecond, "skinColorSecondAlien");
                Scribe_Values.Look(ref this.hairColorSecond, "hairColorSecondAlien");
                Scribe_Values.Look(ref this.crownType, "crownType");
                Scribe_Collections.Look(ref this.addonVariants, "addonVariants");
            }

            internal void AssignProperMeshs()
            {
                this.alienGraphics = AlienPartGenerator.meshPools[this.customDrawSize];
                this.alienPortraitGraphics = AlienPartGenerator.meshPools[this.customPortraitDrawSize];
            }
        }

        public class AlienGraphicMeshSet
        {
            public GraphicMeshSet bodySet;
            public GraphicMeshSet headSet;
            public GraphicMeshSet hairSetAverage;
            public GraphicMeshSet hairSetNarrow;
            public Mesh addonMesh;
            public Mesh addonMeshFlipped;
        }
    }
}