namespace AlienRace.BodyAddonSupport;

using System.Collections.Generic;

public interface IGraphicsLoader
{
    public void LoadAllGraphics(string                                    source,
                                List<AlienPartGenerator.OffsetNamed>      offsetDefaults,
                                IEnumerable<AlienPartGenerator.BodyAddon> bodyAddons);
}
