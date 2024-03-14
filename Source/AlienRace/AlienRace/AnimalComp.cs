using AlienRace.ExtendedGraphics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace AlienRace
{
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
            Scribe_Collections.Look(ref this.addonVariants, "addonVariants");
        }
    }
    public class AnimalBodyAddons : DefModExtension
    {
        public List<BodyAddon>   bodyAddons     = new();
        public List<OffsetNamed> offsetDefaults = new();
        public void GenerateAddonData(ThingDef def)
        {
            this.offsetDefaults.Add(new OffsetNamed
                                    {
                                        name = "Center",
                                        offsets = new DirectionalOffset()
                                    });
            this.offsetDefaults.Add(new OffsetNamed
                                    {
                                        name = "Tail",//rough estimate of tail location, lower on body, and to the rear
                                        offsets = new DirectionalOffset
                                                  {
                                                      south = new RotationOffset
                                                              {
                                                                  offset = new Vector2(0f, -0.15f)
                                                              },
                                                      north = new RotationOffset
                                                              {
                                                                  offset = new Vector2(0f, 0f)
                                                              },
                                                      east = new RotationOffset
                                                             {
                                                                 offset = new Vector2(0.42f, -0.15f)
                                                             },
                                                      west = new RotationOffset
                                                             {
                                                                 offset = new Vector2(0.42f, -0.15f)
                                                             }
                                                  }
                                    });
            this.offsetDefaults.Add(new OffsetNamed
                                    {
                                        name = "Head",//rough estimate of location, higher on body and to the front
                                        offsets = new DirectionalOffset
                                                  {
                                                      south = new RotationOffset
                                                              {
                                                                  offset = new Vector2(0f, 0.35f)
                                                              },
                                                      north = new RotationOffset
                                                              {
                                                                  offset = new Vector2(0f, 0.5f)
                                                              },
                                                      east = new RotationOffset
                                                             {
                                                                 offset = new Vector2(-0.47f, 0.35f)
                                                             },
                                                      west = new RotationOffset
                                                             {
                                                                 offset = new Vector2(-0.47f, 0.35f)
                                                             }
                                                  }
                                    });
            new DefaultGraphicsLoader().LoadAllGraphics(def.defName + " Animal Addons", this.bodyAddons.Cast<ExtendedGraphicTop>().ToArray());
        }
    }
}