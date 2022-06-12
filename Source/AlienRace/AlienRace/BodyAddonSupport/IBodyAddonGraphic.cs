namespace AlienRace.BodyAddonSupport;

using System.Collections.Generic;

public interface IBodyAddonGraphic
{
    public string GetPath();
    public int    GetVariantCount();
    public int    IncrementVariantCount();

    /*
     * Get all sub-graphics with no restrictions
     */
    public IEnumerator<IBodyAddonGraphic> GetSubGraphics(); 
    
    /**
     * Get sub-graphics relevant to pawn and part
     */
    public IEnumerator<IBodyAddonGraphic> GetSubGraphics(BodyAddonPawnWrapper pawn, string part);
    
    /**
     * Check if this graphic is relevant to the pawn and part.
     */
    public bool                           IsApplicable(BodyAddonPawnWrapper   pawn, string part);
}
