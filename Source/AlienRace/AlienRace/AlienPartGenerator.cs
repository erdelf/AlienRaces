using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using System.Xml;

namespace AlienRace
{
    public sealed class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string>() { "Average_Normal" };

        public List<BodyType> alienbodytypes = new List<BodyType>();

        public bool UseGenderedHeads = true;

        public ColorGenerator alienskincolorgen;
        public ColorGenerator alienskinsecondcolorgen;
        public ColorGenerator alienhaircolorgen;
        public bool useSkincolorForHair = false;

        public Vector2 CustomDrawSize = Vector2.one;
        public Vector2 CustomPortraitDrawSize = Vector2.one;

        static Dictionary<Vector2, GraphicMeshSet[]> meshPools = new Dictionary<Vector2, GraphicMeshSet[]>();

        public GraphicMeshSet bodySet;
        public GraphicMeshSet headSet;
        public GraphicMeshSet hairSetAverage;
        public GraphicMeshSet hairSetNarrow;
        public Mesh tailMesh;
        public Mesh tailMeshFlipped;

        public GraphicMeshSet bodyPortraitSet;
        public GraphicMeshSet headPortraitSet;
        public GraphicMeshSet hairPortraitSetAverage;
        public GraphicMeshSet hairPortraitSetNarrow;
        public Mesh tailPortraitMesh;
        public Mesh tailPortraitMeshFlipped;

        public BodyPartDef tailBodyPart;
        public bool UseSkinColorForTail = true;
        public List<TailOffset> tailOffsets;

        static MethodInfo meshInfo = AccessTools.Method(AccessTools.TypeByName("MeshMakerPlanes"), "NewPlaneMesh", new Type[] { typeof(Vector2), typeof(bool), typeof(bool), typeof(bool) });

        public string RandomAlienHead(string userpath, Gender gender) => userpath + (userpath == GraphicPaths.vanillaHeadPath ? gender.ToString() + "/" : "") + (this.UseGenderedHeads ? gender.ToString() + "_" : "") + this.aliencrowntypes[Rand.Range(0, this.aliencrowntypes.Count)];

        public static Graphic GetNakedGraphic(BodyType bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath) => GraphicDatabase.Get<Graphic_Multi>(userpath + "Naked_" + bodyType.ToString(), shader, Vector2.one, skinColor, skinColorSecond);

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

        public AlienPartGenerator() => LongEventHandler.QueueLongEvent(() =>
                                         {

                                             {
                                                 if (!meshPools.Keys.Any(v => v.Equals(this.CustomDrawSize)))
                                                 {
                                                     meshPools.Add(this.CustomDrawSize, new GraphicMeshSet[]
                                                         {
                                                            new GraphicMeshSet(1.5f * this.CustomDrawSize.x, 1.5f * this.CustomDrawSize.y), // bodySet
                                                            new GraphicMeshSet(1.5f * this.CustomDrawSize.x, 1.5f * this.CustomDrawSize.y), // headSet
                                                            new GraphicMeshSet(1.5f * this.CustomDrawSize.x, 1.5f * this.CustomDrawSize.y), // hairSetAverage
                                                            new GraphicMeshSet(1.3f * this.CustomDrawSize.x, 1.5f * this.CustomDrawSize.y), // hairSetNarrow
                                                         });
                                                 }

                                                 GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(this.CustomDrawSize))];

                                                 this.bodySet = meshSet[0];
                                                 this.headSet = meshSet[1];
                                                 this.hairSetAverage = meshSet[2];
                                                 this.hairSetNarrow = meshSet[3];
                                                 this.tailMesh = (Mesh)meshInfo.Invoke(null, new object[] { this.CustomDrawSize, false, false, false });
                                                 this.tailMeshFlipped = (Mesh)meshInfo.Invoke(null, new object[] { this.CustomDrawSize, true, false, false });
                                             }
                                             {
                                                 if (!meshPools.Keys.Any(v => v.Equals(this.CustomPortraitDrawSize)))
                                                 {
                                                     meshPools.Add(this.CustomPortraitDrawSize, new GraphicMeshSet[]
                                                         {
                                                            new GraphicMeshSet(1.5f * this.CustomPortraitDrawSize.x, 1.5f * this.CustomPortraitDrawSize.y), // bodySet
                                                            new GraphicMeshSet(1.5f * this.CustomPortraitDrawSize.x, 1.5f * this.CustomPortraitDrawSize.y), // headSet
                                                            new GraphicMeshSet(1.5f * this.CustomPortraitDrawSize.x, 1.5f * this.CustomPortraitDrawSize.y), // hairSetAverage
                                                            new GraphicMeshSet(1.3f * this.CustomPortraitDrawSize.x, 1.5f * this.CustomPortraitDrawSize.y), // hairSetNarrow
                                                         });
                                                 }

                                                 GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(this.CustomPortraitDrawSize))];

                                                 this.bodyPortraitSet = meshSet[0];
                                                 this.headPortraitSet = meshSet[1];
                                                 this.hairPortraitSetAverage = meshSet[2];
                                                 this.hairPortraitSetNarrow = meshSet[3];
                                                 this.tailPortraitMesh = (Mesh)meshInfo.Invoke(null, new object[] { this.CustomPortraitDrawSize, false, false, false });
                                                 this.tailPortraitMeshFlipped = (Mesh)meshInfo.Invoke(null, new object[] { this.CustomPortraitDrawSize, true, false, false });
                                             }
                                         }, "meshSetAlien", false, null);

        public bool CanDrawTail(Pawn pawn) => RestUtility.CurrentBed(pawn) == null && !pawn.Downed && pawn.GetPosture() == PawnPosture.Standing && !pawn.Dead && (this.tailBodyPart == null ||
                pawn.health.hediffSet.GetNotMissingParts().Any(bpr => bpr.def == this.tailBodyPart));

        public class AlienComp : ThingComp
        {
            public bool fixGenderPostSpawn;
            public Color skinColor;
            public Color skinColorSecond;
            public Graphic Tail;

            public override void PostExposeData()
            {
                base.PostExposeData();
                Scribe_Values.Look(ref this.fixGenderPostSpawn, "fixAlienGenderPostSpawn", false);
                Scribe_Values.Look(ref this.skinColor, "skinColorAlien");
                Scribe_Values.Look(ref this.skinColorSecond, "skinColorSecondAlien");
            }
        }

        public class TailOffset
        {
            public BodyType bodyType;
            public Vector2 offset = new Vector2(0, 0);

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                this.bodyType = (BodyType) Enum.Parse(typeof(BodyType), xmlRoot.Name);
                //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(BodyType), xmlRoot.Name);
                this.offset = (Vector2)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
            }
        }
    }
}