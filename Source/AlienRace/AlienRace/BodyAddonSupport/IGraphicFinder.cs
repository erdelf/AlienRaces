namespace AlienRace.BodyAddonSupport;

public interface IGraphicFinder<out T>
{
    public T GetByPath(string basePath, int variant, string direction, bool reportFailure);
}
