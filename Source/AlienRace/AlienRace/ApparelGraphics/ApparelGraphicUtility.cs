using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace AlienRace.ApparelGraphics
{
    public static class ApparelGraphicUtility
    {
        public static Dictionary<(ThingDef, ThingDef_AlienRace, BodyTypeDef), Graphic> apparelCache = new();

        public static Graphic GetGraphic(string path, Shader shader, Vector2 drawSize, Color color, Apparel apparel, BodyTypeDef bodyType)
        {
            Pawn wearer = apparel.Wearer;
            if (wearer != null && wearer.def is ThingDef_AlienRace alienRace)
            {
                if (apparelCache.TryGetValue((apparel.def, alienRace, bodyType), out Graphic cachedGraphic)) {
                    return cachedGraphic;
                }

                string overridePath;
                ApparelGraphicsOverrides overrides = alienRace.alienRace.graphicPaths.apparel;

                // Check first for specific Def overrides
                if ((overridePath = overrides.GetOverridePath(apparel)) != null)
                {
                    if (bodyType != null) overridePath += "_" + bodyType.defName;
                    return CacheNewGraphic(overridePath);
                }
                // Blanket prefixes take precedence over vanilla path
                if (!overrides.pathPrefix.NullOrEmpty() && ValidTexturesExist(overridePath = overrides.pathPrefix + path))
                {
                    return CacheNewGraphic(overridePath);
                }

                // If no prefix exists or if the prefix failed to find textures, try the regular textures
                if (ValidTexturesExist(path))
                {
                    return CacheNewGraphic(path);
                }

                // If regular textures cannot be found, attempt to find a valid fallback
                if ((overridePath = overrides.GetFallbackPath(apparel)) != null)
                {
                    if (bodyType != null) overridePath += "_" + bodyType.defName;
                    return CacheNewGraphic(overridePath);
                }
                // If no specific fallbacks exist, attempt to use a fallback body type
                if (bodyType != null && path.EndsWith(overridePath = "_" + bodyType.defName) && overrides.TryGetBodyTypeFallback(wearer, out BodyTypeDef overrideBodyType))
                {
                    overridePath = ReplaceBodyType(path, overridePath, overrideBodyType);
                    return CacheNewGraphic(overridePath);
                }

                // If all else fails, fall back to vanilla behavior
                return CacheNewGraphic(path);
            }

            // Non-HAR races can sometimes wear apparel, such as via Animal Gear
            return GraphicDatabase.Get<Graphic_Multi>(path, shader, drawSize, color);

            Graphic CacheNewGraphic(string path)
            {
                Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, drawSize, color);
                apparelCache.Add((apparel.def, alienRace, bodyType), graphic);
                return graphic;
            }
        }

        public static bool ValidTexturesExist(string path)
        {
            Texture2D southTexture = ContentFinder<Texture2D>.Get(path + Graphic_Multi.SouthSuffix, reportFailure: false);
            if (southTexture == null)
            {
                return ContentFinder<Texture2D>.Get(path, reportFailure: false) != null;
            }
            return true;
        }

        public static string ReplaceBodyType(string path, string oldToken, BodyTypeDef newBodyType)
        {
            int index = path.LastIndexOf(oldToken);
            return path.Remove(index) + "_" + newBodyType.defName;
        }
    }
}
