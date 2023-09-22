namespace AlienRace;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AlienRace.ExtendedGraphics;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

[HotSwappable]
public static class StylingStation
{
    private static List<TabRecord> mainTabs = new();
    private static List<TabRecord> raceTabs = new();
    private static MainTab         curMainTab;
    private static RaceTab         curRaceTab;

    private static int selectedIndex = -1;

    private static Dialog_StylingStation        instance;
    private static Pawn                         pawn;
    private static AlienPartGenerator.AlienComp alienComp;
    private static ThingDef_AlienRace           alienRaceDef;

    public static IEnumerable<CodeInstruction> DoWindowContentsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        return instructions.MethodReplacer(AccessTools.Method(typeof(Dialog_StylingStation), "DrawTabs"), AccessTools.Method(typeof(StylingStation), nameof(DoRaceAndCharacterTabs)));
    }

    public static void DoRaceAndCharacterTabs(Dialog_StylingStation gotInstance, Rect inRect)
    {
        instance = gotInstance;
        pawn     = CachedData.stationPawn(instance);

        if (pawn.def is not ThingDef_AlienRace alienRace || pawn.TryGetComp<AlienPartGenerator.AlienComp>() is not { } comp)
        {
            CachedData.drawTabs(instance, inRect);
            return;
        }

        alienRaceDef = alienRace;
        alienComp    = comp;

        mainTabs.Clear();
        mainTabs.Add(new TabRecord("HAR.CharacterFeatures".Translate(), () => curMainTab = MainTab.CHARACTER, curMainTab == MainTab.CHARACTER));
        mainTabs.Add(new TabRecord("HAR.RaceFeatures".Translate(),      () => curMainTab = MainTab.RACE,      curMainTab == MainTab.RACE));
        Widgets.DrawMenuSection(inRect);
        TabDrawer.DrawTabs(inRect, mainTabs);
        inRect.yMin += 40;
        switch (curMainTab)
        {
            case MainTab.CHARACTER:
                CachedData.drawTabs(instance, inRect);
                break;
            case MainTab.RACE:
                DoRaceTabs(inRect);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static List<Color> AvailableColors(AlienPartGenerator.BodyAddon ba, bool first = true) => DefDatabase<ColorDef>.AllDefs.Select(cd => cd.color).ToList();
    // new List<Color>();

    public static void DoRaceTabs(Rect inRect)
    {
        raceTabs.Clear();
        raceTabs.Add(new TabRecord("HAR.BodyAddons".Translate(), () => curRaceTab = RaceTab.BODY_ADDONS, curRaceTab == RaceTab.BODY_ADDONS));
        Widgets.DrawMenuSection(inRect);
        TabDrawer.DrawTabs(inRect, raceTabs);
        switch (curRaceTab)
        {
            case RaceTab.BODY_ADDONS:
                DrawBodyAddonTab(inRect);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void DrawBodyAddonTab(Rect inRect)
    {
        List<AlienPartGenerator.BodyAddon> bodyAddons = alienRaceDef.alienRace.generalSettings.alienPartGenerator.bodyAddons.Concat(Utilities.UniversalBodyAddons).ToList();
        DoAddonList(inRect.LeftPartPixels(260), bodyAddons);
        inRect.xMin += 260;
        if (selectedIndex != -1) DoAddonInfo(inRect, bodyAddons[selectedIndex], selectedIndex < bodyAddons.Count - 1 ? bodyAddons[selectedIndex + 1] : null);
    }

    private static Vector2 addonsScrollPos;

    private static void DoAddonList(Rect inRect, List<AlienPartGenerator.BodyAddon> addons)
    {
        if (selectedIndex > addons.Count) selectedIndex = -1;

        Widgets.DrawMenuSection(inRect);
        Rect viewRect = new Rect(0, 0, 246, addons.Count * 54 + 4);
        Widgets.BeginScrollView(inRect, ref addonsScrollPos, viewRect);
        for (int i = 0; i < addons.Count; i++)
        {
            Rect rect = new Rect(6, i * 54f + 4, 240f, 50f).ContractedBy(2);
            if (i == selectedIndex)
                Widgets.DrawOptionSelected(rect);
            else
            {
                GUI.color = Widgets.WindowBGFillColor;
                GUI.DrawTexture(rect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                if ((i < addons.Count - 1 && addons[i + 1].linkVariantIndexWithPrevious && selectedIndex == i + 1)
                || (i  > 0                && addons[i].linkVariantIndexWithPrevious     && selectedIndex == i - 1))
                {
                    GUI.color = new ColorInt(135, 135, 135).ToColor;
                    Widgets.DrawBox(rect, 1);
                    GUI.color = Color.white;
                }
            }

            Widgets.DrawHighlightIfMouseover(rect);
            if (Widgets.ButtonInvisible(rect))
            {
                selectedIndex = i;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            Rect      position = rect.LeftPartPixels(rect.height).ContractedBy(2);
            Texture2D image    = ContentFinder<Texture2D>.Get(addons[i].GetPath() + "_south"); // TODO: Better way to get the image for this
            GUI.color = Widgets.MenuSectionBGFillColor;
            GUI.DrawTexture(position, BaseContent.WhiteTex);
            GUI.color = Color.white;
            GUI.DrawTexture(position, image);
            rect.xMin += rect.height;
            Widgets.Label(rect.ContractedBy(4), addons[i].Name);

            if (addons[i].linkVariantIndexWithPrevious)
            {
                GUI.color = new ColorInt(135, 135, 135).ToColor;
                GUI.DrawTexture(new Rect(rect.x - rect.height - 6, rect.center.y,     6, 2),  BaseContent.WhiteTex);
                GUI.DrawTexture(new Rect(rect.x - rect.height - 6, (i - 1) * 54 + 27, 6, 2),  BaseContent.WhiteTex);
                GUI.DrawTexture(new Rect(rect.x - rect.height - 6, (i - 1) * 54 + 27, 2, 56), BaseContent.WhiteTex);
                GUI.color = Color.white;
            }

            AlienPartGenerator.ExposableValueTuple<Color, Color> channelColors = alienComp.GetChannel(addons[i].ColorChannel);
            (Color, Color)                                       colors        = (addons[i].colorOverrideOne ?? channelColors.first, addons[i].colorOverrideTwo ?? channelColors.second);

            Rect colorRect = new Rect(rect.xMax - 44, rect.yMax - 22, 18, 18);
            Widgets.DrawLightHighlight(colorRect);
            Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item1);

            colorRect = new Rect(rect.xMax - 22, rect.yMax - 22, 18, 18);
            Widgets.DrawLightHighlight(colorRect);
            Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item2);
        }

        Widgets.EndScrollView();
    }

    private static Vector2 variantsScrollPos;
    private static bool    editingFirstColor;

    private static void DoAddonInfo(Rect inRect, AlienPartGenerator.BodyAddon addon, AlienPartGenerator.BodyAddon nextAddon)
    {
        List<Color>    firstColors   = AvailableColors(addon, true);
        List<Color>    secondColors  = AvailableColors(addon, false);
        var            channelColors = alienComp.GetChannel(addon.ColorChannel);
        (Color, Color) colors        = (addon.colorOverrideOne ?? channelColors.first, addon.colorOverrideTwo ?? channelColors.second);
        if (firstColors.Any() || secondColors.Any())
        {
            Rect colorsRect = inRect.BottomPart(0.4f);
            inRect.yMax -= colorsRect.height;

            Widgets.DrawMenuSection(colorsRect);

            Rect headerRect = colorsRect.TopPartPixels(30).ContractedBy(4);

            Widgets.Label(headerRect, "HAR.Colors".Translate());

            Rect colorRect;
            if (firstColors.Any())
            {
                colorRect = new Rect(headerRect.xMax - 44, headerRect.y, 18, 18);
                Widgets.DrawLightHighlight(colorRect);
                Widgets.DrawHighlightIfMouseover(colorRect);
                Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item1);
                if (editingFirstColor) Widgets.DrawBox(colorRect);
                if (Widgets.ButtonInvisible(colorRect)) editingFirstColor = true;
            }
            else editingFirstColor = false;

            if (secondColors.Any())
            {
                colorRect = new Rect(headerRect.xMax - 22, headerRect.y, 18, 18);
                Widgets.DrawLightHighlight(colorRect);
                Widgets.DrawHighlightIfMouseover(colorRect);
                Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item2);
                if (!editingFirstColor) Widgets.DrawBox(colorRect);
                if (Widgets.ButtonInvisible(colorRect)) editingFirstColor = false;
            }
            else editingFirstColor = true;

            List<Color> availableColors = editingFirstColor ? firstColors : secondColors;

            var pos  = new Vector2(colorsRect.x + 6, colorsRect.y + 30);
            var size = new Vector2(14,               18);
            foreach (Color color in availableColors)
            {
                var rect = new Rect(pos, size).ContractedBy(1);
                Widgets.DrawLightHighlight(rect);
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.DrawBoxSolid(rect.ContractedBy(1), color);
                if (editingFirstColor)
                {
                    if (colors.Item1.IndistinguishableFrom(color)) Widgets.DrawBox(rect);
                    if (Widgets.ButtonInvisible(rect)) addon.colorOverrideOne = color;
                }
                else
                {
                    if (colors.Item2.IndistinguishableFrom(color)) Widgets.DrawBox(rect);
                    if (Widgets.ButtonInvisible(rect)) addon.colorOverrideTwo = color;
                }

                pos.x += size.x;
                if (pos.x + size.x >= colorsRect.xMax)
                {
                    pos.y += size.y;
                    pos.x =  colorsRect.x + 6;
                }
            }
        }

        int   variantCount = addon.GetVariantCount();
        int   countPerRow  = 4;
        float itemSize     = inRect.width / countPerRow;
        while (itemSize > 48)
        {
            countPerRow++;
            itemSize = inRect.width / countPerRow;
        }

        Rect viewRect = new Rect(0, 0, inRect.width - 20, Mathf.Ceil((float)variantCount / countPerRow) * itemSize);

        Widgets.DrawMenuSection(inRect);
        Widgets.BeginScrollView(inRect, ref variantsScrollPos, viewRect);
        for (int i = 0; i < variantCount; i++)
        {
            Rect   rect    = new Rect(i % countPerRow * itemSize, Mathf.Floor((float)i / countPerRow) * itemSize, itemSize, itemSize).ContractedBy(2);
            int    index   = i;
            string variant = addon.GetBestGraphic(new ExtendedGraphicsPawnWrapper(pawn), addon.bodyPart, addon.bodyPartLabel).GetPathFromVariant(ref index, out _);
            Log.Message($"Variant {i} has path {variant}");

            GUI.color = Widgets.WindowBGFillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = Color.white;
            Widgets.DrawHighlightIfMouseover(rect);

            if (alienComp.addonVariants[selectedIndex] == i) Widgets.DrawBox(rect);

            Texture2D image = ContentFinder<Texture2D>.Get(variant + "_south"); // TODO: Better way to get the image for this
            GUI.DrawTexture(rect, image);

            if (Widgets.ButtonInvisible(rect))
            {
                alienComp.addonVariants[selectedIndex] = i;
                if (addon.linkVariantIndexWithPrevious) alienComp.addonVariants[selectedIndex                  - 1] = i;
                if (nextAddon is { linkVariantIndexWithPrevious: true }) alienComp.addonVariants[selectedIndex + 1] = i;
            }
        }

        Widgets.EndScrollView();
    }

    private enum MainTab
    {
        CHARACTER, RACE
    }

    private enum RaceTab
    {
        BODY_ADDONS
    }
}

public class HotSwappableAttribute : Attribute { }
