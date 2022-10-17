namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;

public interface IExtendedGraphic
{
    public string GetPath();
    public int    GetVariantCount();
    public int    IncrementVariantCount();

    /*
     * Get all sub-graphics with no restrictions
     */
    public IEnumerator<IExtendedGraphic> GetSubGraphics(); 
    
    /**
     * Get sub-graphics relevant to pawn and part
     */
    public IEnumerator<IExtendedGraphic> GetSubGraphics(ExtendedGraphicsPawnWrapper pawn, string part);
    
    /**
     * Check if this graphic is relevant to the pawn and part.
     */
    public bool                           IsApplicable(ExtendedGraphicsPawnWrapper   pawn, string part);

    public IEnumerable<IExtendedGraphic> GetSubGraphicsOfPriority(AlienPartGenerator.ExtendedGraphicsPrioritization priority);
}
