using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace AlienRace.ApparelGraphics
{
    public class ApparelGraphicsOverrides
    {
        public string pathPrefix = null;
        public Dictionary<ThingDef, string> individualFallbackPaths;
        public List<ApparelFallbackOption> fallbacks;
        public BodyTypeDef bodyTypeFallback = null;
        public BodyTypeDef femaleBodyTypeFallback = null;

        public string GetOverridePath(Apparel apparel)
        {
            if (apparel == null || apparel.def == null) return null;

            if (!individualFallbackPaths.NullOrEmpty())
            {
                if (individualFallbackPaths.TryGetValue(apparel.def, out string overridePath))
                {
                    return overridePath;
                }
            }
            return null;
        }

        public string GetFallbackPath(Apparel apparel)
        {
            if (apparel == null || apparel.def == null) return null;
            if (!fallbacks.NullOrEmpty())
            {
                foreach(ApparelFallbackOption option in fallbacks)
                {
                    if (option.IsSuitableFallBackFor(apparel.def))
                    {
                        return option.wornGraphicPath;
                    }
                }
            }
            return null;
        }

        public bool TryGetBodyTypeFallback(Pawn pawn, out BodyTypeDef def)
        {
            def = null;
            if (pawn == null) return false;
            if (pawn.gender == Gender.Female && femaleBodyTypeFallback != null)
            {
                def = femaleBodyTypeFallback;
                return true;
            }
            def = bodyTypeFallback;
            return def != null;
        }
    }

    public class ApparelFallbackOption
    {
        public string wornGraphicPath = "";
        public List<string> wornGraphicPaths;

        public List<string> apparelTags;
        public List<BodyPartGroupDef> bodyPartGroups = new List<BodyPartGroupDef>();
        public List<ApparelLayerDef> layers = new List<ApparelLayerDef>();

        public string GetGraphicPath(ThingDef apparelDef)
        {
            if (!wornGraphicPaths.NullOrEmpty())
            {
                return wornGraphicPaths[apparelDef.GetHashCode() % wornGraphicPaths.Count];
            }
            return wornGraphicPath;
        }
        public bool IsSuitableFallBackFor(ThingDef apparelDef)
        {
            if (apparelDef == null || apparelDef.apparel == null) return false;
            ApparelProperties props = apparelDef.apparel;

            if (!apparelTags.NullOrEmpty() && (props.tags.NullOrEmpty() || !props.tags.Intersect(apparelTags).Any())) return false;
            if (!bodyPartGroups.NullOrEmpty() && (props.bodyPartGroups.NullOrEmpty() || !props.bodyPartGroups.Intersect(bodyPartGroups).Any())) return false;
            if (!layers.NullOrEmpty() && (props.layers.NullOrEmpty() || !props.layers.Intersect(layers).Any())) return false;

            return true;
        }
    }
}
