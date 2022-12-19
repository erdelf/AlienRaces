namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

/**
 * This class loads graphics in a reasonable way but makes no guarantees about idempotency
 */
public class DefaultGraphicsLoader : IGraphicsLoader

{
    private readonly IGraphicFinder<Texture2D> graphicFinder2D;
    private          bool                      noPath = false;

    public DefaultGraphicsLoader() : this(new GraphicFinder2D())
    {
    }

    public DefaultGraphicsLoader(IGraphicFinder<Texture2D> graphicFinder2D) => this.graphicFinder2D = graphicFinder2D;

    private static void LogFor(StringBuilder logBuilder, string logLine, bool shouldLog = false)
    {
        if (shouldLog) 
            logBuilder.AppendLine(logLine);
    }

    private void LoadAll2DVariantsForGraphic(IExtendedGraphic graphic,
                                             StringBuilder    logBuilder,
                                             string           source,
                                             bool             shouldLog = false)
    {
        graphic.Init();
        LogFor(logBuilder, $"Loading variants for {graphic.GetPath()}");

        // Load all variant paths until we find one that doesn't exist

        for (int i = 0; i < graphic.GetPathCount(); i++)
        {
            while (this.graphicFinder2D.GetByPath(graphic.GetPath(i), graphic.GetVariantCount(i), "south", false) != null)
                graphic.IncrementVariantCount(i);
            LogFor(logBuilder, $"Variants found for {graphic.GetPath(i)}: {graphic.GetVariantCount(i)}", shouldLog);
        }

        if(graphic.GetVariantCount() <= 0)
            if(graphic.UseFallback())
                for (int i = 0; i < graphic.GetPathCount(); i++)
                {
                    while (this.graphicFinder2D.GetByPath(graphic.GetPath(i), graphic.GetVariantCount(i), "south", false) != null)
                        graphic.IncrementVariantCount(i);
                    LogFor(logBuilder, $"Variants found for {graphic.GetPath(i)}: {graphic.GetVariantCount(i)}", shouldLog);
                }

        LogFor(logBuilder, $"Total variants found for {graphic.GetPath()}: {graphic.GetVariantCount()}", shouldLog);

        // If we didn't find any, warn about it
        if (graphic.GetVariantCount() == 0)
        {
            if (Prefs.DevMode)
            {
                LogFor(logBuilder, $"No graphics found at {graphic.GetPath()} for {graphic.GetType()} in {source}.", shouldLog);
            }
            else
            {
                this.noPath = true;
            }
        }
    }

    /**
     * This method loads all the graphics in the given set of graphics in a strictly non-idempotent manner
     */
    public void LoadAllGraphics(string source, params AlienPartGenerator.ExtendedGraphicTop[] graphicTops)
    {
        Stack<IEnumerator<IExtendedGraphic>> topGraphics = new();
        StringBuilder                        logBuilder        = new();

        // Initialise the stack with the set of top level Graphics enumerators from all the bodyaddons.
        foreach (AlienPartGenerator.ExtendedGraphicTop topGraphic in graphicTops)
        {
            // This process is not idempotent; each loaded variant increases the variant count.
            // So if the variantCount isn't 0, this addon has already been initialised so we can skip it.
            // It seems likely this could be hoisted above the offset config above as that would also have been done.
            // I have not reordered these lines in case there is an alternate code-path which could result in non-zero variants without defaultOffsets being set. 
            if (topGraphic.GetVariantCount() != 0) continue;

            // Load path variants for current addon
            this.LoadAll2DVariantsForGraphic(topGraphic, logBuilder, source, topGraphic.Debug);
            topGraphic.VariantCountMax = topGraphic.GetVariantCount();
            // Add to stack of Graphics Enumerators to evaluate
            topGraphics.Push(topGraphic.GetSubGraphics());

            while (topGraphics.Count > 0)
            {
                // Take the next unprocessed graphics set off the stack
                IEnumerator<IExtendedGraphic> subGraphicSet = topGraphics.Pop();

                // For each graphic in the set being looked at, load it and add any sub-graphics to the stack
                while (subGraphicSet.MoveNext())
                {
                    IExtendedGraphic currentGraphic = subGraphicSet.Current;
                    if (currentGraphic == null) break;
                    this.LoadAll2DVariantsForGraphic(currentGraphic, logBuilder, source, topGraphic.Debug);
                    topGraphic.VariantCountMax = currentGraphic.GetVariantCount();
                    // Add the enumerator for any sub graphics to the stack
                    topGraphics.Push(currentGraphic.GetSubGraphics());
                }
            }
        }

        if (this.noPath) 
            Log.Message($"Textures were not found for one or more extended graphics, enable devmode for more information");
        if (logBuilder.Length > 0) 
            Log.Message($"Loaded graphic variants for {source}\n{logBuilder}");
    }
}