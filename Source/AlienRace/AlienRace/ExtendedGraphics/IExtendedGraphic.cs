namespace AlienRace.ExtendedGraphics;

using System.Collections.Generic;
using Verse;

public interface IExtendedGraphic
{
    public void Init();

    public string GetPath();
    public string GetPath(int index);
    public int    GetPathCount();
    public string GetPathFromVariant(ref int variantIndex, out bool zero);
    public int    GetVariantCount();
    public int    GetVariantCount(int index);
    public int    IncrementVariantCount();
    public int    IncrementVariantCount(int index);

    public bool UseFallback();

    /*
     * Get all sub-graphics with no restrictions
     */
    public IEnumerable<IExtendedGraphic> GetSubGraphics(); 
    
    /**
     * Get sub-graphics relevant to pawn and part
     */
    public IEnumerable<IExtendedGraphic> GetSubGraphics(ExtendedGraphicsPawnWrapper pawn, ResolveData data);
    
    /**
     * Check if this graphic is relevant to the pawn and part.
     */
    public bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data);
}

public struct ResolveData
{
    public BodyPartDef bodyPart;
    public string      bodyPartLabel;

    /**
     * For other modders
     */
    public Dictionary<string, object> genericStorage = [];

    public ResolveData()
    {

    }
}