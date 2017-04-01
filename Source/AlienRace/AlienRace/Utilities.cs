using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AlienRace
{
    public class ThinkNode_ConditionalIsMemberOfRace : ThinkNode_Conditional
    {
        public List<string> races;

        protected override bool Satisfied(Pawn pawn) => races.Contains(pawn.def.defName);
    }
}
