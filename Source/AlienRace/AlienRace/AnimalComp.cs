using AlienRace.ExtendedGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    public static class HARHandler
    {
        static HARHandler()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                var extension = def.GetModExtension<AnimalBodyAddons>();
                if (extension != null)
                {
                    extension.GenerateMeshsAndMeshPools(def);
                    def.comps.Add(new CompProperties(typeof(AnimalComp)));
                }
            }
        }
    }

    public class AnimalComp : ThingComp
    {
        public List<Graphic> addonGraphics;
        public List<int> addonVariants;
        public Vector2 customDrawSize = Vector2.one;

        public Vector2 customHeadDrawSize = Vector2.one;

        public Vector2 customPortraitDrawSize = Vector2.one;

        public Vector2 customPortraitHeadDrawSize = Vector2.one;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref addonVariants, "addonVariants", LookMode.Undefined);
        }
    }
    public class AnimalBodyAddons : DefModExtension
    {
        public List<BodyAddon> bodyAddons = new List<BodyAddon>();
        public List<OffsetNamed> offsetDefaults = new List<OffsetNamed>();
        public void GenerateMeshsAndMeshPools(ThingDef def)
        {
            offsetDefaults.Add(new OffsetNamed
            {
                name = "Center",
                offsets = new BodyAddonOffsets()
            });
            offsetDefaults.Add(new OffsetNamed
            {
                name = "Tail",
                offsets = new BodyAddonOffsets
                {
                    south = new RotationOffset
                    {
                        offset = new Vector2(0.42f, -0.22f)
                    },
                    north = new RotationOffset
                    {
                        offset = new Vector2(0f, -0.55f)
                    },
                    east = new RotationOffset
                    {
                        offset = new Vector2(0.42f, -0.22f)
                    },
                    west = new RotationOffset
                    {
                        offset = new Vector2(0.42f, -0.22f)
                    }
                }
            });
            offsetDefaults.Add(new OffsetNamed
            {
                name = "Head",
                offsets = new BodyAddonOffsets
                {
                    south = new RotationOffset
                    {
                        offset = new Vector2(0f, 0.5f)
                    },
                    north = new RotationOffset
                    {
                        offset = new Vector2(0f, 0.35f)
                    },
                    east = new RotationOffset
                    {
                        offset = new Vector2(-0.07f, 0.5f)
                    },
                    west = new RotationOffset
                    {
                        offset = new Vector2(-0.07f, 0.5f)
                    }
                }
            });
            new DefaultGraphicsLoader().LoadAllGraphics(def.defName + " Animal Addons", bodyAddons.Cast<ExtendedGraphicTop>().ToArray());
        }
    }
}