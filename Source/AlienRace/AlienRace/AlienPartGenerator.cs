using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public partial class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string> { "Average_Normal" };

        public List<BodyTypeDef> alienbodytypes = new List<BodyTypeDef>();

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

        private static readonly Dictionary<Vector2, AlienGraphicMeshSet> meshPools = new Dictionary<Vector2, AlienGraphicMeshSet>();

        public List<BodyAddon> bodyAddons = new List<BodyAddon>();

        public ThingDef_AlienRace alienProps;

        public string RandomAlienHead(string userpath, Pawn pawn) => GetAlienHead(userpath: userpath, gender: (this.useGenderedHeads ? pawn.gender.ToString() : ""), crowntype: pawn.GetComp<AlienComp>().crownType = this.aliencrowntypes[index: Rand.Range(min: 0, max: this.aliencrowntypes.Count)]);

        public static string GetAlienHead(string userpath, string gender, string crowntype) => userpath.NullOrEmpty() ? "" : userpath + (userpath == GraphicPaths.VANILLA_HEAD_PATH ? gender + "/" : "") + (!gender.NullOrEmpty() ? gender + "_" : "") + crowntype;

        public Graphic GetNakedGraphic(BodyTypeDef bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath, string gender) => GraphicDatabase.Get<Graphic_Multi>(path: GetNakedPath(bodyType: bodyType, userpath: userpath, gender: this.useGenderedBodies ? gender : ""), shader: shader, drawSize: Vector2.one, color: skinColor, colorTwo: skinColorSecond);

        public static string GetNakedPath(BodyTypeDef bodyType, string userpath, string gender) => userpath + (!gender.NullOrEmpty() ? gender + "_" : "") + "Naked_" + bodyType;

        public Color SkinColor(Pawn alien, bool first = true)
        {
            AlienComp alienComp = alien.TryGetComp<AlienComp>();
            if (alienComp.skinColor != Color.clear)
                return first ? alienComp.skinColor : alienComp.skinColorSecond;

            alienComp.skinColor       = this.alienskincolorgen?.NewRandomizedColor() ?? PawnSkinColors.GetSkinColor(melanin: alien.story.melanin);
            alienComp.skinColorSecond = this.alienskinsecondcolorgen?.NewRandomizedColor() ?? alienComp.skinColor;
            return first ? alienComp.skinColor : alienComp.skinColorSecond;
        }

        public void GenerateMeshsAndMeshPools()
        {
            void AddMeshSet(Vector2 drawSize, Vector2 headDrawSize)
            {
                if (!meshPools.Keys.Any(predicate: v => v.Equals(other: drawSize)))
                {
                    meshPools.Add(key: drawSize, value: new AlienGraphicMeshSet()
                    {
                        bodySet = new GraphicMeshSet(width: 1.5f * drawSize.x, height: 1.5f * drawSize.y), // bodySet
                        headSet = new GraphicMeshSet(width: 1.5f * headDrawSize.x, height: 1.5f * headDrawSize.y), // headSet
                        hairSetAverage = new GraphicMeshSet(width: 1.5f * headDrawSize.x, height: 1.5f * headDrawSize.y), // hairSetAverage
                    });
                }
            }

            foreach (GraphicPaths graphicsPath in this.alienProps.alienRace.graphicPaths.Concat(
                    rhs: new GraphicPaths() { customDrawSize = this.customDrawSize, customHeadDrawSize = this.customHeadDrawSize, customPortraitDrawSize = this.customPortraitDrawSize, customPortraitHeadDrawSize = this.customPortraitHeadDrawSize }))
            {
                AddMeshSet(drawSize: graphicsPath.customDrawSize, headDrawSize: graphicsPath.customHeadDrawSize);
                AddMeshSet(drawSize: graphicsPath.customPortraitDrawSize, headDrawSize: graphicsPath.customPortraitHeadDrawSize);
            }

            this.bodyAddons.Do(action: ba =>
            {
                if (ba.variantCount == 0)
                {
                    while (ContentFinder<Texture2D>.Get(itemPath: ba.path + (ba.variantCount == 0 ? "" : ba.variantCount.ToString()) + "_north", reportFailure: false) != null)
                        ba.variantCount++;
                    Log.Message(text: "Variants found for " + ba.path + ": " + ba.variantCount.ToString());
                    if (ba.hediffGraphics != null)
                        foreach (BodyAddonHediffGraphic bahg in ba.hediffGraphics)
                        {
                            if (bahg.variantCount == 0)
                            {
                                while (ContentFinder<Texture2D>.Get(itemPath: bahg.path + (bahg.variantCount == 0 ? "" : bahg.variantCount.ToString()) + "_north", reportFailure: false) != null)
                                    bahg.variantCount++;
                                Log.Message(text: "Variants found for " + bahg.path + ": " + bahg.variantCount.ToString());
                            }
                        }
                    if (ba.backstoryGraphics != null)
                        foreach (BodyAddonBackstoryGraphic babg in ba.backstoryGraphics)
                        {
                            if (babg.variantCount == 0)
                            {
                                while (ContentFinder<Texture2D>.Get(itemPath: babg.path + (babg.variantCount == 0 ? "" : babg.variantCount.ToString()) + "_north", reportFailure: false) != null)
                                    babg.variantCount++;
                                Log.Message(text: "Variants found for " + babg.path + ": " + babg.variantCount.ToString());
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
                base.PostSpawnSetup(respawningAfterLoad: respawningAfterLoad);
                AlienPartGenerator apg = ((ThingDef_AlienRace) this.parent.def).alienRace.generalSettings.alienPartGenerator;
                this.customDrawSize = apg.customDrawSize;
                this.customPortraitDrawSize = apg.customPortraitDrawSize;
            }

            public override void PostExposeData()
            {
                base.PostExposeData();
                Scribe_Values.Look(value: ref this.fixGenderPostSpawn, label: "fixAlienGenderPostSpawn");
                Scribe_Values.Look(value: ref this.skinColor, label: "skinColorAlien");
                Scribe_Values.Look(value: ref this.skinColorSecond, label: "skinColorSecondAlien");
                Scribe_Values.Look(value: ref this.hairColorSecond, label: "hairColorSecondAlien");
                Scribe_Values.Look(value: ref this.crownType, label: "crownType");
                Scribe_Collections.Look(list: ref this.addonVariants, label: "addonVariants");
            }

            internal void AssignProperMeshs()
            {
                this.alienGraphics = meshPools[key: this.customDrawSize];
                this.alienPortraitGraphics = meshPools[key: this.customPortraitDrawSize];
            }
        }

        public class AlienGraphicMeshSet
        {
            public GraphicMeshSet bodySet;
            public GraphicMeshSet headSet;
            public GraphicMeshSet hairSetAverage;
        }
    }
}