namespace AlienRace.ExtendedGraphics;

public interface IGraphicFinder<out T>
{
    public T GetByPath(string basePath, int variant, string direction, bool reportFailure);
}
