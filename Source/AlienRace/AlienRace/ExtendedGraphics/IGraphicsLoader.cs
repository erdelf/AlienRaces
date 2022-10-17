namespace AlienRace.ExtendedGraphics;

public interface IGraphicsLoader
{
    public void LoadAllGraphics(string source, params AlienPartGenerator.ExtendedGraphicTop[] graphicTops);
}
