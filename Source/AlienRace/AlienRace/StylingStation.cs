namespace AlienRace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

[StaticConstructorOnStartup]
public static class StylingStation
{
    private static readonly Texture2D ChainTex = ContentFinder<Texture2D>.Get("AlienRace/UI/LinkChain");

    private static readonly List<TabRecord> mainTabs = new();
    private static readonly List<TabRecord> raceTabs = new();
    private static          MainTab         curMainTab;
    private static          RaceTab         curRaceTab = RaceTab.BODY_ADDONS;

    private static Dialog_StylingStation        instance;
    private static Pawn                         pawn;
    private static AlienPartGenerator.AlienComp alienComp;
    private static ThingDef_AlienRace           alienRaceDef;

    private static List<int>                                                                addonVariants;
    private static Dictionary<string, AlienPartGenerator.ExposableValueTuple<Color, Color>> colorChannels;


    public static void ConstructorPostfix(Pawn pawn)
    {
        StylingStation.pawn         = pawn;
        StylingStation.alienComp    = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
        StylingStation.alienRaceDef = pawn.def as ThingDef_AlienRace;
        addonVariants               = [.. alienComp.addonVariants];
        colorChannels               = new Dictionary<string, AlienPartGenerator.ExposableValueTuple<Color, Color>>(alienComp.ColorChannels);
        List<string> list = [.. colorChannels.Keys];

        foreach (string key in list) 
            colorChannels[key] = (AlienPartGenerator.ExposableValueTuple<Color, Color>) colorChannels[key].Clone();
    }

    public static IEnumerable<CodeInstruction> DoWindowContentsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg) =>
        instructions.MethodReplacer(AccessTools.Method(typeof(Dialog_StylingStation), "DrawTabs"), AccessTools.Method(typeof(StylingStation), nameof(DoRaceAndCharacterTabs)));

    public static void DoRaceAndCharacterTabs(Dialog_StylingStation gotInstance, Rect inRect)
    {
        instance = gotInstance;

        if (alienRaceDef == null)
        {
            CachedData.drawTabs(instance, inRect);
            return;
        }

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
    
    private static readonly Dictionary<AlienPartGenerator.ColorChannelGenerator, Dictionary<bool, List<Color>>> availableColorsCache = new();

    public static List<Color> AvailableColors(AlienPartGenerator.BodyAddon ba, bool first = true)
    {
        AlienPartGenerator.ColorChannelGenerator channelGenerator = alienRaceDef.alienRace.generalSettings?.alienPartGenerator.colorChannels?.Find(ccg => ccg.name == ba.ColorChannel);


        return channelGenerator != null ? 
                   AvailableColors(channelGenerator: channelGenerator, first: first) : 
                   [];
    }

    private static List<Color> AvailableColors(AlienPartGenerator.ColorChannelGenerator channelGenerator, bool first)
    {
        if (availableColorsCache.TryGetValue(channelGenerator, out Dictionary<bool, List<Color>> colorEntry))
            if (colorEntry.TryGetValue(first, out List<Color> colors))
                return colors;
        
        List<Color> availableColors = [];

        foreach (AlienPartGenerator.ColorChannelGeneratorCategory entry in channelGenerator.entries)
        {
            ColorGenerator cg = first ? entry.first : entry.second;

            availableColors.AddRange(AvailableColors(colorGenerator: cg));
        }

        if (!availableColorsCache.ContainsKey(channelGenerator))
            availableColorsCache.Add(channelGenerator, []);
        availableColorsCache[channelGenerator].Add(first, availableColors);

        return availableColors;
    }

    private static List<Color> AvailableColors(ColorGenerator colorGenerator)
    {
        List<Color> availableColors = [];
        switch (colorGenerator)
        {
            case ColorGenerator_CustomAlienChannel cgCustomAlien:
                cgCustomAlien.GetInfo(out string channel, out bool firstCustom);

                foreach (AlienPartGenerator.ColorChannelGeneratorCategory entriesCustom in
                         alienRaceDef.alienRace.generalSettings.alienPartGenerator.colorChannels.Find(ccg => ccg.name == channel).entries)
                    availableColors.AddRange(AvailableColors(firstCustom ? entriesCustom.first : entriesCustom.second));
                break;
            case ColorGenerator_SkinColorMelanin cgMelanin:
                if (cgMelanin.naturalMelanin)
                {
                    foreach (GeneDef geneDef in PawnSkinColors.SkinColorGenesInOrder)
                        if (geneDef.skinColorBase.HasValue)
                            availableColors.Add(geneDef.skinColorBase.Value);
                }
                else
                {
                    for (int i = 0; i < PawnSkinColors.SkinColorGenesInOrder.Count; i++)
                    {
                        float currentMelanin = Mathf.Lerp(cgMelanin.minMelanin, cgMelanin.maxMelanin, 1f / PawnSkinColors.SkinColorGenesInOrder.Count * i);

                        int     nextIndex = PawnSkinColors.SkinColorGenesInOrder.FirstIndexOf(gd => gd.minMelanin >= currentMelanin);
                        GeneDef nextGene  = PawnSkinColors.SkinColorGenesInOrder[nextIndex];


                        if (nextIndex == 0)
                        {
                            availableColors.Add(nextGene.skinColorBase!.Value);
                        }
                        else
                        {
                            GeneDef lastGene = PawnSkinColors.SkinColorGenesInOrder[nextIndex - 1];
                            availableColors.Add(Color.Lerp(lastGene.skinColorBase!.Value, nextGene.skinColorBase!.Value,
                                                           Mathf.InverseLerp(lastGene.minMelanin, nextGene.minMelanin, currentMelanin)));
                        }
                    }
                }
                break;
            case ColorGenerator_Options cgOptions:
                foreach (ColorOption co in cgOptions.options)
                    if (co.only.a >= 0f)
                    {
                        availableColors.Add(co.only);
                    }
                    else
                    {
                        List<Color> colorOptions = [];

                        Color diff = co.max - co.min;

                        //int steps = Math.Min(100, Mathf.RoundToInt((Mathf.Abs(diff.r) + Mathf.Abs(diff.g) + Mathf.Abs(diff.b) + Mathf.Abs(diff.a)) / 0.01f));

                        float redStep   = Mathf.Max(0.0001f, diff.r / 2);
                        float greenStep = Mathf.Max(0.0001f, diff.g / 2);
                        float blueStep  = Mathf.Max(0.0001f, diff.b / 2);
                        float alphaStep = Mathf.Max(0.0001f, diff.a / 2);

                        for (float r = co.min.r; r <= co.max.r; r += redStep)
                        {
                            for (float g = co.min.g; g <= co.max.g; g += greenStep)
                            {
                                for (float b = co.min.b; b <= co.max.b; b += blueStep)
                                {
                                    for (float a = co.min.a; a <= co.max.a; a += alphaStep)
                                        colorOptions.Add(new Color(r, g, b, a));
                                }
                            }
                        }

                        availableColors.AddRange(colorOptions.OrderBy(c =>
                                                                      {
                                                                          Color.RGBToHSV(c, out _, out float s, out float v);
                                                                          return s + v;
                                                                      }));
                        
                    }
                break;
            case ColorGenerator_Single:
            case ColorGenerator_White:
                availableColors.Add(colorGenerator.NewRandomizedColor());
                break;
            case IAlienChannelColorGenerator accg:
                foreach (ColorGenerator generator in accg.AvailableGenerators(pawn: pawn)) 
                    availableColors.AddRange(AvailableColors(generator));
                availableColors.AddRange(accg.AvailableColors(pawn));
                break;
            default:
                //availableColors.AddRange(DefDatabase<ColorDef>.AllDefs.Select(cd => cd.color));
                break;
        }
        return availableColors;
    }

    public static void DoRaceTabs(Rect inRect)
    {
        raceTabs.Clear();
        raceTabs.Add(new TabRecord("HAR.Colors".Translate(),     () => curRaceTab = RaceTab.COLORS,      curRaceTab == RaceTab.COLORS));
        raceTabs.Add(new TabRecord("HAR.BodyAddons".Translate(), () => curRaceTab = RaceTab.BODY_ADDONS, curRaceTab == RaceTab.BODY_ADDONS));
        Widgets.DrawMenuSection(inRect);
        TabDrawer.DrawTabs(inRect, raceTabs);
        switch (curRaceTab)
        {
            case RaceTab.BODY_ADDONS:
                DrawBodyAddonTab(inRect);
                break;
            case RaceTab.COLORS:
                DrawColorChannelTab(inRect);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #region Addons
    private static int selectedIndexAddons = -1;

    public static void DrawBodyAddonTab(Rect inRect)
    {
        List<AlienPartGenerator.BodyAddon> bodyAddons = alienRaceDef.alienRace.generalSettings.alienPartGenerator.bodyAddons.Concat(Utilities.UniversalBodyAddons).Where(ba => ba.userCustomizable).ToList();
        DoAddonList(inRect.LeftPartPixels(260), bodyAddons);
        inRect.xMin += 260;
        if (selectedIndexAddons != -1)
            DoAddonInfo(inRect, bodyAddons[selectedIndexAddons], bodyAddons);
    }

    private static Vector2 addonsScrollPos;

    private static void DoAddonList(Rect inRect, List<AlienPartGenerator.BodyAddon> addons)
    {
        if (selectedIndexAddons >= addons.Count)
            selectedIndexAddons = -1;

        Widgets.DrawMenuSection(inRect);

        Rect viewRect = new(0, 0, 250, addons.Count * 54 + 4);
        Widgets.BeginScrollView(inRect, ref addonsScrollPos, viewRect);
        for (int i = 0; i < addons.Count; i++)
        {
            Rect rect = new Rect(10, i * 54f + 4, 240f, 50f).ContractedBy(2);
            if (i == selectedIndexAddons)
            {
                Widgets.DrawOptionSelected(rect);
            }
            else
            {
                GUI.color = Widgets.WindowBGFillColor;
                GUI.DrawTexture(rect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                bool groupSelected = false;
                int  index         = i;

                while (index >= 0 && addons[index].linkVariantIndexWithPrevious)
                {
                    index--;
                    if (selectedIndexAddons == index)
                        groupSelected = true;
                }

                index = i + 1;

                while (index <= addons.Count - 1 && addons[index].linkVariantIndexWithPrevious)
                {
                    //Log.Message($"{index} is linked, selected is {selectedIndex}");
                    if (selectedIndexAddons == index)
                        groupSelected = true;
                    index++;
                }

                if (groupSelected)
                {
                    GUI.color = new ColorInt(135, 135, 135).ToColor;
                    Widgets.DrawBox(rect);
                    GUI.color = Color.white;
                }
            }

            Widgets.DrawHighlightIfMouseover(rect);

            if (Widgets.ButtonInvisible(rect))
            {
                selectedIndexAddons = i;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            Rect      position     = rect.LeftPartPixels(rect.height).ContractedBy(2);
            int       addonVariant = alienComp.addonVariants[i];
            Texture2D image        = ContentFinder<Texture2D>.Get(addons[i].GetPath(pawn, ref addonVariant, addonVariant) + "_south");
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

            Rect colorRect = new(rect.xMax - 44, rect.yMax - 22, 18, 18);
            Widgets.DrawLightHighlight(colorRect);
            Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item1);

            colorRect = new Rect(rect.xMax - 22, rect.yMax - 22, 18, 18);
            Widgets.DrawLightHighlight(colorRect);
            Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item2);
        }

        Widgets.EndScrollView();
    }

    private static Vector2 variantsScrollPos;
    private static bool    editingFirstColor = true;
    private static Vector2 colorsScrollPos;

    private static void DoAddonInfo(Rect inRect, AlienPartGenerator.BodyAddon addon, List<AlienPartGenerator.BodyAddon> addons)
    {
        List<Color>                                          firstColors   = AvailableColors(addon);
        List<Color>                                          secondColors  = AvailableColors(addon, false);
        AlienPartGenerator.ExposableValueTuple<Color, Color> channelColors = alienComp.GetChannel(addon.ColorChannel);
        (Color, Color)                                       colors        = (addon.colorOverrideOne ?? channelColors.first, addon.colorOverrideTwo ?? channelColors.second);
        Rect                                                 viewRect;

        const bool drawColors = false;

        if (drawColors && (firstColors.Any() || secondColors.Any()))
        {
            Rect colorsRect = inRect.BottomPart(0.4f);
            inRect.yMax -= colorsRect.height;

            Widgets.DrawMenuSection(colorsRect);


            List<Color> availableColors = editingFirstColor ? firstColors : secondColors;

            colorsRect = colorsRect.ContractedBy(6);

            Vector2 size = new(14, 18);
            viewRect = new Rect(0, 0, colorsRect.width - 16, (Mathf.Ceil(availableColors.Count / ((colorsRect.width - 14) / size.x)) + 1) * size.y + 35);

            Widgets.BeginScrollView(colorsRect, ref colorsScrollPos, viewRect);


            Rect headerRect = viewRect.TopPartPixels(30).ContractedBy(4);
            viewRect.yMin += 30;

            Widgets.Label(headerRect, "HAR.Colors".Translate());

            Rect colorRect;
            if (firstColors.Any())
            {
                colorRect = new Rect(headerRect.xMax - 44, headerRect.y, 18, 18);
                Widgets.DrawLightHighlight(colorRect);
                Widgets.DrawHighlightIfMouseover(colorRect);
                Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item1);

                if (editingFirstColor)
                    Widgets.DrawBox(colorRect);

                if (Widgets.ButtonInvisible(colorRect))
                    editingFirstColor = true;
            }
            else
            {
                editingFirstColor = false;
            }

            if (secondColors.Any())
            {
                colorRect = new Rect(headerRect.xMax - 22, headerRect.y, 18, 18);
                Widgets.DrawLightHighlight(colorRect);
                Widgets.DrawHighlightIfMouseover(colorRect);
                Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item2);

                if (!editingFirstColor)
                    Widgets.DrawBox(colorRect);

                if (Widgets.ButtonInvisible(colorRect))
                    editingFirstColor = false;
            }
            else
            {
                editingFirstColor = true;
            }

            Vector2 pos = new(0, 30);

            foreach (Color color in availableColors)
            {
                Rect rect = new Rect(pos, size).ContractedBy(1);
                Widgets.DrawLightHighlight(rect);
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.DrawBoxSolid(rect.ContractedBy(1), color);

                if (editingFirstColor)
                {
                    if (colors.Item1.IndistinguishableFrom(color))
                        Widgets.DrawBox(rect);
                    if (Widgets.ButtonInvisible(rect))
                        if (addon.ColorChannel == "hair")
                        {
                            pawn.story.HairColor = color;
                            pawn.style.Notify_StyleItemChanged();
                            pawn.style.ResetNextStyleChangeAttemptTick();
                            pawn.style.nextHairColor                     = null;
                            CachedData.stationDesiredHairColor(instance) = color;
                        }
                        else
                        {
                            alienComp.OverwriteColorChannel(addon.ColorChannel, color);
                        }
                    //addon.colorOverrideOne = color;
                }
                else
                {
                    if (colors.Item2.IndistinguishableFrom(color))
                        Widgets.DrawBox(rect);
                    if (Widgets.ButtonInvisible(rect))
                        alienComp.OverwriteColorChannel(addon.ColorChannel, second: color);
                    //addon.colorOverrideTwo = color;
                }

                pos.x += size.x;
                if (pos.x + size.x >= viewRect.xMax)
                {
                    pos.y += size.y;
                    pos.x =  0;
                }
            }

            Widgets.EndScrollView();
        }

        int   variantCount = addon.GetVariantCount();
        int   countPerRow  = 4;
        float width        = inRect.width - 20;
        float itemSize     = width / countPerRow;
        while (itemSize > 92)
        {
            countPerRow++;
            itemSize = width / countPerRow;
        }

        viewRect = new Rect(0, 0, width, Mathf.Ceil((float)variantCount / countPerRow) * itemSize);

        Widgets.DrawMenuSection(inRect);
        Widgets.BeginScrollView(inRect, ref variantsScrollPos, viewRect);

        for (int i = 0; i < variantCount; i++)
        {
            Rect rect  = new Rect(i % countPerRow * itemSize, Mathf.Floor((float)i / countPerRow) * itemSize, itemSize, itemSize).ContractedBy(2);
            int  index = i;

            GUI.color = Widgets.WindowBGFillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = Color.white;
            Widgets.DrawHighlightIfMouseover(rect);

            if (alienComp.addonVariants[selectedIndexAddons] == i)
                Widgets.DrawBox(rect);

            string    addonPath = addon.GetPath(pawn, ref index, i);
            Texture2D image     = ContentFinder<Texture2D>.Get(addonPath + "_south", false);

            if (image != null)
                GUI.DrawTexture(rect, image);

            if (Widgets.ButtonInvisible(rect))
            {
                alienComp.addonVariants[selectedIndexAddons] = i;

                index = selectedIndexAddons;

                while (index >= 0 && addons[index].linkVariantIndexWithPrevious)
                {
                    index--;
                    alienComp.addonVariants[index] = i;
                }

                index = selectedIndexAddons + 1;

                while (index <= addons.Count - 1 && addons[index].linkVariantIndexWithPrevious)
                {
                    alienComp.addonVariants[index] = i;
                    index++;
                }
            }
        }

        Widgets.EndScrollView();
    }
    #endregion

    #region ColorChannels

    private static int selectedIndexChannels = -1;

    public static void DrawColorChannelTab(Rect inRect)
    {
        List<AlienPartGenerator.ColorChannelGenerator> channels = alienRaceDef.alienRace.generalSettings.alienPartGenerator.colorChannels;


        DoChannelList(inRect.LeftPartPixels(260), channels);
        inRect.xMin += 260;
        if (selectedIndexChannels != -1)
            DoChannelInfo(inRect, channels[selectedIndexChannels], channels);
    }

    private static Vector2 channelsScrollPos;

    private static void DoChannelList(Rect inRect, List<AlienPartGenerator.ColorChannelGenerator> channels)
    {
        if (selectedIndexChannels >= channels.Count)
            selectedIndexChannels = -1;

        Widgets.DrawMenuSection(inRect);
        Rect viewRect = new(0, 0, 250, channels.Count * 54 + 4);
        Widgets.BeginScrollView(inRect, ref channelsScrollPos, viewRect);
        for (int i = 0; i < channels.Count; i++)
        {
            AlienPartGenerator.ColorChannelGenerator channel = channels[i];

            Rect rect = new Rect(10, i * 54f + 4, 240f, 50f).ContractedBy(2);
            if (i == selectedIndexChannels)
            {
                Widgets.DrawOptionSelected(rect);
            }
            else
            {
                GUI.color = Widgets.WindowBGFillColor;
                GUI.DrawTexture(rect, BaseContent.WhiteTex);
                GUI.color = Color.white;
            }

            Widgets.DrawHighlightIfMouseover(rect);

            if (Widgets.ButtonInvisible(rect))
            {
                selectedIndexChannels = i;
                if (channel.name == "hair")
                    editingFirstColor = false;

                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            AlienPartGenerator.ExposableValueTuple<Color, Color> channelColors         = alienComp.GetChannel(channel.name);
            (Color, Color)                                       colors                = (channelColors.first, channelColors.second);

            Rect position = rect.LeftPartPixels(rect.height).ContractedBy(2);

            Widgets.DrawLightHighlight(position);
            Widgets.DrawBoxSolid(position, colors.Item1);

            if (i == selectedIndexChannels && editingFirstColor)
                Widgets.DrawBox(position);

            Text.Anchor =  TextAnchor.MiddleCenter;
            Widgets.Label(rect, channel.name);
            Text.Anchor = TextAnchor.UpperLeft;

            rect.xMin += rect.height;

            position = rect.RightPartPixels(rect.height).ContractedBy(2);

            Widgets.DrawLightHighlight(position);
            Widgets.DrawBoxSolid(position, colors.Item2);

            if (i == selectedIndexChannels && !editingFirstColor)
                Widgets.DrawBox(position);
        }

        Widgets.EndScrollView();
    }
    
    private static void DoChannelInfo(Rect inRect, AlienPartGenerator.ColorChannelGenerator channel, List<AlienPartGenerator.ColorChannelGenerator> channels)
    {
        List<Color>                                          firstColors   = AvailableColors(channel, true);
        List<Color>                                          secondColors  = AvailableColors(channel, false);
        AlienPartGenerator.ExposableValueTuple<Color, Color> channelColors = alienComp.GetChannel(channel.name);
        (Color, Color)                                       colors        = (channelColors.first, channelColors.second);

        if (firstColors.Any() || secondColors.Any())
        {
            Rect colorsRect = inRect;//.BottomPart(0.4f);
            inRect.yMax -= colorsRect.height;

            Widgets.DrawMenuSection(colorsRect);


            List<Color> availableColors = editingFirstColor ? firstColors : secondColors;

            colorsRect = colorsRect.ContractedBy(6);

            Vector2 size     = new(18, 18);
            Rect    viewRect = new(0, 0, colorsRect.width - 16, (Mathf.Ceil(availableColors.Count / ((colorsRect.width - 14) / size.x)) + 1) * size.y + 35);

            Widgets.BeginScrollView(colorsRect, ref colorsScrollPos, viewRect);
            
            Rect headerRect = viewRect.TopPartPixels(30).ContractedBy(4);
            viewRect.yMin += 30;

            Widgets.Label(headerRect, "HAR.Colors".Translate());

            Rect colorRect;

            void LinkedIndicator(Rect linkedRect, bool first)
            {
                linkedRect.y += linkedRect.height * 0.5f;
                linkedRect.x += linkedRect.width * 0.5f;


                List<string> linkedTo = [];

                if (alienComp.ColorChannelLinks.Keys.Contains(channel.name))
                    foreach (AlienPartGenerator.ExposableValueTuple<AlienPartGenerator.ExposableValueTuple<string, int>, bool> link in alienComp.ColorChannelLinks[channel.name])
                        if (link.second == first)
                            linkedTo.Add("HAR.LinkText".Translate(link.first.first.CapitalizeFirst(), (link.first.second == 1 ? "HAR.FirstColor" : "HAR.SecondColor").Translate()));
            

                List<string> linkedFrom = [];

                foreach ((string baseChannel, HashSet<AlienPartGenerator.ExposableValueTuple<AlienPartGenerator.ExposableValueTuple<string, int>, bool>> hashSet) in alienComp.ColorChannelLinks)
                {
                    foreach (AlienPartGenerator.ExposableValueTuple<AlienPartGenerator.ExposableValueTuple<string, int>, bool> link in hashSet)
                        if (link.first.first == channel.name)
                            if (link.first.second == (first ? 1 : 2))
                                linkedFrom.Add("HAR.LinkText".Translate(baseChannel.CapitalizeFirst(), (link.second ? "HAR.FirstColor" : "HAR.SecondColor").Translate()));
                }

                if (linkedTo.Any() || linkedFrom.Any())
                {

                    Widgets.DrawTextureFitted(linkedRect, ChainTex, 1.5f);

                    //linkedRect.position += inRect.position;
                    if (Mouse.IsOver(linkedRect))
                    {
                        Log.Message($"{channel.name}_{first}");


                        StringBuilder sb = new();

                        if (linkedTo.Any())
                        {
                            sb.AppendLine("HAR.LinkedTo".Translate());
                            foreach (string s in linkedTo)
                                sb.AppendLine(s);
                        }

                        if (linkedFrom.Any())
                        {
                            sb.AppendLine("HAR.LinkedFrom".Translate());
                            foreach (string s in linkedFrom)
                                sb.AppendLine(s);
                        }
                        //Log.Message(sb.ToString());
                        TooltipHandler.TipRegion(linkedRect, new TipSignal(sb.ToString()));
                    }
                }
            }

            if (firstColors.Any())
            {
                colorRect = new Rect(headerRect.xMax - 44 - 7, headerRect.y, 18, 18);
                Widgets.DrawLightHighlight(colorRect);
                Widgets.DrawHighlightIfMouseover(colorRect);
                Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item1);

                if (editingFirstColor)
                    Widgets.DrawBox(colorRect);

                if (Widgets.ButtonInvisible(colorRect))
                    if (channel.name == "hair")
                    {
                        curMainTab                 = MainTab.CHARACTER;
                        CachedData.stationCurTab(instance) = Dialog_StylingStation.StylingTab.Hair;
                    }
                    else
                    {
                        editingFirstColor = true;
                    }

                LinkedIndicator(colorRect, true);
            }
            else
            {
                editingFirstColor = false;
            }

            if (secondColors.Any())
            {
                colorRect = new Rect(headerRect.xMax - 22, headerRect.y, 18, 18);
                Widgets.DrawLightHighlight(colorRect);
                Widgets.DrawHighlightIfMouseover(colorRect);
                Widgets.DrawBoxSolid(colorRect.ContractedBy(2), colors.Item2);

                if (!editingFirstColor)
                    Widgets.DrawBox(colorRect);

                if (Widgets.ButtonInvisible(colorRect))
                    editingFirstColor = false;

                LinkedIndicator(colorRect, false);
            }
            else
            {
                editingFirstColor = true;
            }

            Vector2 pos = new(0, 35);

            foreach (Color color in availableColors)
            {
                Rect rect = new Rect(pos, size).ContractedBy(1);
                Widgets.DrawLightHighlight(rect);
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.DrawBoxSolid(rect.ContractedBy(1), color);

                if (editingFirstColor)
                {
                    if (colors.Item1.IndistinguishableFrom(color))
                        Widgets.DrawBox(rect);
                    if (Widgets.ButtonInvisible(rect))
                        alienComp.OverwriteColorChannel(channel.name, color);
                    //addon.colorOverrideOne = color;
                }
                else
                {
                    if (colors.Item2.IndistinguishableFrom(color))
                        Widgets.DrawBox(rect);
                    if (Widgets.ButtonInvisible(rect))
                        alienComp.OverwriteColorChannel(channel.name, second: color);
                    //addon.colorOverrideTwo = color;
                }

                pos.x += size.x;
                if (pos.x + size.x >= viewRect.xMax)
                {
                    pos.y += size.y;
                    pos.x = 0;
                }
            }

            Widgets.EndScrollView();
        }
    }
    
    #endregion


    public static void ResetPostfix(bool resetColors)
    {
        if (resetColors)
        {
            alienComp.addonVariants = addonVariants;
            alienComp.ColorChannels = colorChannels;

            pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();

            ConstructorPostfix(pawn);
        }
    }

    private enum MainTab
    {
        CHARACTER,
        RACE
    }

    private enum RaceTab
    {
        COLORS,
        BODY_ADDONS
    }
}