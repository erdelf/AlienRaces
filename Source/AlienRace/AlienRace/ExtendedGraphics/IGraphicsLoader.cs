namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;

public interface IGraphicsLoader
{
    public void LoadAllGraphics(string source, params AlienPartGenerator.ExtendedGraphicTop[] graphicTops);
}
