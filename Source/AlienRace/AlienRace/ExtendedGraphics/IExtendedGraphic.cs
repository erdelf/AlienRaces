namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;
using Verse;

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
    public IEnumerator<IExtendedGraphic> GetSubGraphics(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel);
    
    /**
     * Check if this graphic is relevant to the pawn and part.
     */
    public bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel);

    public IEnumerable<IExtendedGraphic> GetSubGraphicsOfPriority(AlienPartGenerator.ExtendedGraphicsPrioritization priority);
}
