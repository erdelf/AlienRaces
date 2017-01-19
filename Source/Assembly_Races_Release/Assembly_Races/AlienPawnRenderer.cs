using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    internal static class AlienPawnRendererDetour
    {

        static FieldInfo pawnInfo = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo internalPawnRender = typeof(PawnRenderer).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where((MethodInfo m) => m.Name.EqualsIgnoreCase("renderPawnInternal")).ToList().First((MethodInfo m) => m.GetParameters().Count((ParameterInfo p) => p.ParameterType == typeof(Boolean)) == 1);
        static MethodInfo internalPawnRender2 = typeof(PawnRenderer).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where((MethodInfo m) => m.Name.EqualsIgnoreCase("renderPawnInternal")).ToList().First((MethodInfo m) => m.GetParameters().Count((ParameterInfo p) => p.ParameterType == typeof(Boolean)) == 1);
        static FieldInfo shadowGraphicInfo = typeof(PawnRenderer).GetField("shadowGraphic", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo layingFacingInfo = typeof(PawnRenderer).GetMethod("LayingFacing", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo debugDrawInfo = typeof(PawnRenderer).GetMethod("DrawDebug", BindingFlags.NonPublic | BindingFlags.Instance);
        
    }
}