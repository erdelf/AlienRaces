namespace AlienRace.BodyAddonSupport;

using UnityEngine;
using Verse;

class GraphicFinder2D : IGraphicFinder<Texture2D>
{
    public Texture2D GetByPath(string basePath, int variant, string direction, bool reportFailure) =>
        ContentFinder<Texture2D>.Get($"{basePath}{(variant == 0 ? "" : variant.ToString())}_{direction}",
                                     reportFailure);
}
