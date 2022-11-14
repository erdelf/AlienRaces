using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace AlienRace.ApparelGraphics
{
    using System;

    public static class ApparelGraphicUtility
    {
        public static Dictionary<(ThingDef, ThingDef_AlienRace, BodyTypeDef), AlienPartGenerator.ExtendedGraphicTop> apparelCache = new();

        public static Graphic GetGraphic(string path, Shader shader, Vector2 drawSize, Color color, Apparel apparel, BodyTypeDef bodyType) => 
            GraphicDatabase.Get<Graphic_Multi>(GetPath(path, apparel, bodyType) ?? path, shader, drawSize, color);

        public static string GetPath(string path, Apparel apparel, BodyTypeDef bodyType)
        {
            Pawn wearer = apparel.Wearer;
            if (wearer?.def is ThingDef_AlienRace alienRace)
            {
                int index      = 0;
                int savedIndex = wearer.HashOffset();

                if (apparelCache.TryGetValue((apparel.def, alienRace, bodyType), out AlienPartGenerator.ExtendedGraphicTop cachedGraphic))
                    return cachedGraphic.GetPath(wearer, ref index, savedIndex);

                AlienPartGenerator.ExtendedGraphicTop overrideEGraphic;
                string                                overridePath;
                ApparelGraphicsOverrides              overrides    = alienRace.alienRace.graphicPaths.apparel;

                // Check first for specific Def overrides
                if ((overrideEGraphic = overrides.GetOverride(apparel)) != null) // default initialize bodytypes
                {
                    overridePath = overrideEGraphic.GetPath(wearer, ref index, savedIndex);
                    return overridePath;
                }

                overrideEGraphic = overrides.pathPrefix;

                if (overrideEGraphic != null)
                {
                    overridePath = overrideEGraphic.GetPath(wearer, ref index, savedIndex);

                    // Blanket prefixes take precedence over vanilla path
                    if (!overridePath.NullOrEmpty() && ValidTexturesExist(overridePath += path))
                        return overridePath;
                }

                // If no prefix exists or if the prefix failed to find textures, try the regular textures
                if (ValidTexturesExist(path))
                    return path;

                // If regular textures cannot be found, attempt to find a valid fallback
                if ((overrideEGraphic = overrides.GetFallbackPath(apparel)) != null)
                {
                    overridePath = overrideEGraphic.GetPath(wearer, ref index, savedIndex);
                    return overridePath;
                }

                // If no specific fallbacks exist, attempt to use a fallback body type
                if (bodyType != null && path.EndsWith(overridePath = "_" + bodyType.defName) && overrides.TryGetBodyTypeFallback(wearer, out BodyTypeDef overrideBodyType))
                    return ReplaceBodyType(path, overridePath, overrideBodyType);

                // If all else fails, fall back to vanilla behavior
            }

            // Non-HAR races can sometimes wear apparel, such as via Animal Gear
            return null;
        }

        public static bool ValidTexturesExist(string path)
        {
            Texture2D southTexture = ContentFinder<Texture2D>.Get(path + Graphic_Multi.SouthSuffix, reportFailure: false);
            if (southTexture == null)
                return ContentFinder<Texture2D>.Get(path, reportFailure: false) != null;
            return true;
        }

        public static string ReplaceBodyType(string path, string oldToken, BodyTypeDef newBodyType)
        {
            int index = path.LastIndexOf(oldToken, StringComparison.Ordinal);
            return path.Remove(index) + "_" + newBodyType.defName;
        }
    }
}
