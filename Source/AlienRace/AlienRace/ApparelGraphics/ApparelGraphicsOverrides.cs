using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace AlienRace.ApparelGraphics
{
    public class ApparelGraphicsOverrides
    {
        public AlienPartGenerator.ExtendedGraphicTop                       pathPrefix              = new() { path = string.Empty };
        public Dictionary<ThingDef, AlienPartGenerator.ExtendedGraphicTop> individualPaths = new();
        public List<ApparelFallbackOption>                                 fallbacks               = new();
        public BodyTypeDef                                                 bodyTypeFallback        = null;
        public BodyTypeDef                                                 femaleBodyTypeFallback  = null;

        public AlienPartGenerator.ExtendedGraphicTop GetOverride(Apparel apparel)
        {
            if (apparel?.def == null)
                return null;

            return !this.individualPaths.NullOrEmpty() && this.individualPaths.TryGetValue(apparel.def, out AlienPartGenerator.ExtendedGraphicTop overridePath) ?
                       overridePath :
                       null;
        }

        public AlienPartGenerator.ExtendedGraphicTop GetFallbackPath(Apparel apparel)
        {
            if (apparel?.def == null)
                return null;

            return !this.fallbacks.NullOrEmpty() ?
                       this.fallbacks.FirstOrDefault(option => option.IsSuitableFallBackFor(apparel.def))?.GetGraphics(apparel.def) :
                       null;
        }

        public bool TryGetBodyTypeFallback(Pawn pawn, out BodyTypeDef def)
        {
            def = null;
            if (pawn == null)
                return false;

            def = pawn.gender == Gender.Female && this.femaleBodyTypeFallback != null ?
                      this.femaleBodyTypeFallback :
                      this.bodyTypeFallback;

            return def != null;
        }
    }

    public class ApparelFallbackOption
    {
        public AlienPartGenerator.ExtendedGraphicTop       wornGraphicPath = new() { path = string.Empty };
        public List<AlienPartGenerator.ExtendedGraphicTop> wornGraphicPaths = new();

        public List<string>           apparelTags;
        public List<BodyPartGroupDef> bodyPartGroups = new();
        public List<ApparelLayerDef>  layers         = new();

        public AlienPartGenerator.ExtendedGraphicTop GetGraphics(ThingDef apparelDef) =>
            !this.wornGraphicPaths.NullOrEmpty() ?
                this.wornGraphicPaths[apparelDef.GetHashCode() % this.wornGraphicPaths.Count] :
                this.wornGraphicPath;

        public bool IsSuitableFallBackFor(ThingDef apparelDef)
        {
            if (apparelDef?.apparel == null)
                return false;

            ApparelProperties props = apparelDef.apparel;

            return (this.apparelTags.NullOrEmpty()    || !props.tags.NullOrEmpty()           && props.tags.Intersect(this.apparelTags).Any())              &&
                   (this.bodyPartGroups.NullOrEmpty() || !props.bodyPartGroups.NullOrEmpty() && props.bodyPartGroups.Intersect(this.bodyPartGroups).Any()) &&
                   (this.layers.NullOrEmpty()         || !props.layers.NullOrEmpty()         && props.layers.Intersect(this.layers).Any());
        }
    }
}
