using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    class ScenPart_StartingHumanlikes : ScenPart
    {
        /*
        static ScenPart_StartingHumanlikes()
        {
            ScenPartDef scenPart = new ScenPartDef()
            {
                defName = "StartingHumanlikes",
                label = "Start with humanlikes",
                scenPartClass = typeof(ScenPart_StartingHumanlikes),
                category = ScenPartCategory.StartingImportant,
                selectionWeight = 1.0f,
                summaryPriority = 10
            };
            scenPart.ResolveReferences();
            scenPart.PostLoad();
            DefDatabase<ScenPartDef>.Add(scenPart);
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f);
            if (Widgets.ButtonText(scenPartRect.TopPart(0.333f), this.trait.DataAtDegree(this.degree).label.CapitalizeFirst(), true, false, true))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (TraitDef current in from td in DefDatabase<TraitDef>.AllDefs
                                             orderby td.label
                                             select td)
                {
                    foreach (TraitDegreeData current2 in current.degreeDatas)
                    {
                        TraitDef localDef = current;
                        TraitDegreeData localDeg = current2;
                        list.Add(new FloatMenuOption(localDeg.label.CapitalizeFirst(), delegate
                        {
                            this.trait = localDef;
                            this.degree = localDeg.degree;
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            base.DoPawnModifierEditInterface(scenPartRect.BottomPart(0.666f));
        }*/
    }
}