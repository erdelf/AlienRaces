namespace AlienRace.BodyAddonSupport;

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
    public static bool logAddons;
    private bool noPath = false;

    public DefaultGraphicsLoader() : this(new GraphicFinder2D())
    {
    }

    public DefaultGraphicsLoader(IGraphicFinder<Texture2D> graphicFinder2D) => this.graphicFinder2D = graphicFinder2D;

    private void LogFor(StringBuilder logBuilder, string logLine, bool shouldLog = false)
    {
        if (shouldLog) logBuilder.AppendLine(logLine);
    }

    private void LoadAll2DVariantsForGraphic(IBodyAddonGraphic graphic,
                                             StringBuilder     logBuilder,
                                             string            source,
                                             bool              shouldLog = false)
    {
        
        LogFor(logBuilder, $"Loading variants for {graphic.GetPath()}");

        // Load all variant paths until we find one that doesn't exist
        while (this.graphicFinder2D.GetByPath(graphic.GetPath(), graphic.GetVariantCount(), "north", false) != null)
        {
            graphic.IncrementVariantCount();
        }

        LogFor(logBuilder, $"Variants found for {graphic.GetPath()}: {graphic.GetVariantCount()}", shouldLog);

        // If we didn't find any, warn about it
        if (graphic.GetVariantCount() == 0)
        {
            if (logAddons)
            {
                Log.Warning($"No graphics found at {graphic.GetPath()} for {graphic.GetType()} in {source}.");
            }
            else
            {
                noPath = true;
            }
        }
    }

    /**
     * This method loads all the graphics in the given set of addons in a strictly non-idempotent manner
     */
    public void LoadAllGraphics(string                                    source,
                                List<AlienPartGenerator.OffsetNamed>      offsetDefaults,
                                IEnumerable<AlienPartGenerator.BodyAddon> bodyAddons)
    {
        Stack<IEnumerator<IBodyAddonGraphic>> bodyAddonGraphics = new();
        StringBuilder                         logBuilder        = new();

        // Initialise the stack with the set of top level Graphics enumerators from all the bodyaddons.
        foreach (AlienPartGenerator.BodyAddon bodyAddon in bodyAddons)
        {
            // Initialise the offsets of each addon with the generic default offsets
            bodyAddon.defaultOffsets = offsetDefaults.Find(on => on.name == bodyAddon.defaultOffset).offsets;

            // This process is not idempotent; each loaded variant increases the variant count.
            // So if the variantCount isn't 0, this addon has already been initialised so we can skip it.
            // It seems likely this could be hoisted above the offset config above as that would also have been done.
            // I have not reordered these lines in case there is an alternate code-path which could result in non-zero variants without defaultOffsets being set. 
            if (bodyAddon.GetVariantCount() != 0) continue;

            // Load path variants for current addon
            LoadAll2DVariantsForGraphic(bodyAddon, logBuilder, source, bodyAddon.debug);

            // Add to stack of Graphics Enumerators to evaluate
            bodyAddonGraphics.Push(bodyAddon.GetSubGraphics());

            while (bodyAddonGraphics.Count > 0)
            {
                // Take the next unprocessed graphics set off the stack
                IEnumerator<IBodyAddonGraphic> subGraphicSet = bodyAddonGraphics.Pop();

                // For each graphic in the set being looked at, load it and add any sub-graphics to the stack
                while (subGraphicSet.MoveNext())
                {
                    IBodyAddonGraphic currentGraphic = subGraphicSet.Current;
                    if (currentGraphic == null) break;
                    LoadAll2DVariantsForGraphic(currentGraphic, logBuilder, source, bodyAddon.debug);

                    // Add the enumerator for any sub graphics to the stack
                    bodyAddonGraphics.Push(currentGraphic.GetSubGraphics());
                }
            }
        }
        if (noPath) Log.Message($"Body addon textures were not found for one or more body addons, enable detailed addon logging in settings for more information");
        if (logBuilder.Length > 0) Log.Message($"Loaded body addon variants for {source}\n{logBuilder}");
    }
}