namespace AlienRace;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

public static class StylingStation
{
    public const int BODYADDON_TAB_INDEX = 0xab7fedb; //have fun figuring that one out


    public static Dialog_StylingStation instance;

    public static IEnumerable<CodeInstruction> DrawTabsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        MethodInfo drawMenuSectionInfo = AccessTools.Method(typeof(Widgets), nameof(Widgets.DrawMenuSection));
        FieldInfo  curTabInfo          = AccessTools.Field(typeof(Dialog_StylingStation), "curTab");


        List<CodeInstruction> instructionList = instructions.ToList();

        CodeMatcher matcher = new CodeMatcher(instructionList).MatchEndForward(new CodeMatch(OpCodes.Ldstr, "ApparelColor"));

        LocalBuilder tag         = ilg.DeclareLocal(typeof(TaggedString));
        Label        drawLabel = ilg.DefineLabel();

        for (int i = 0; i < instructionList.Count; i++)
        {
            CodeInstruction instruction = instructionList[i];

            if (instruction.Calls(drawMenuSectionInfo))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(typeof(StylingStation), nameof(instance)));
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Dialog_StylingStation), "tabs"));
                yield return new CodeInstruction(OpCodes.Ldstr, "BodyAddons");

                yield return new CodeInstruction(matcher.Advance(1).Instruction); //yield return CodeInstruction.Call(typeof(Translator), nameof(Translator.Translate), new[] { typeof(string) });
                yield return new CodeInstruction(OpCodes.Stloc, tag.LocalIndex);
                yield return new CodeInstruction(OpCodes.Ldloca, tag.LocalIndex);
                yield return new CodeInstruction(matcher.Advance(3).Instruction); //yield return CodeInstruction.Call(typeof(TaggedString), nameof(TaggedString.CapitalizeFirst));
                yield return new CodeInstruction(matcher.Advance(1).Instruction); // yield return CodeInstruction.Call(typeof(TaggedString), "op_Implicit", new[] { typeof(TaggedString) });
                yield return new CodeInstruction(OpCodes.Ldnull);
                yield return new CodeInstruction(OpCodes.Ldftn, AccessTools.Method(typeof(StylingStation), nameof(SetTab)));
                yield return new CodeInstruction(matcher.SearchForward(ci => ci.opcode == OpCodes.Newobj).Instruction);
                yield return new CodeInstruction(matcher.Advance(1).Instruction); //ldarg.0
                yield return new CodeInstruction(matcher.Advance(1).Instruction); //yield return new CodeInstruction(OpCodes.Ldfld, curTabInfo);
                yield return new CodeInstruction(OpCodes.Ldc_I4, BODYADDON_TAB_INDEX);
                yield return new CodeInstruction(matcher.Advance(2).Instruction); //ceq
                yield return new CodeInstruction(matcher.Advance(1).Instruction); //newobj tabrecord
                yield return new CodeInstruction(matcher.Advance(1).Instruction); //add
            }

            yield return instruction;

            if (instruction.opcode == OpCodes.Switch)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld,  AccessTools.Field(typeof(Dialog_StylingStation), "curTab"));
                yield return new CodeInstruction(OpCodes.Ldc_I4, BODYADDON_TAB_INDEX);
                yield return new CodeInstruction(OpCodes.Ceq);
                yield return new CodeInstruction(OpCodes.Brfalse, drawLabel);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return CodeInstruction.Call(typeof(StylingStation), nameof(DrawBodyAddonTab));
                instructionList[i + 1].labels.Add(drawLabel);
            }
        }
    }

    public static void SetTab() => 
        CachedData.curTab(instance) = (Dialog_StylingStation.StylingTab) BODYADDON_TAB_INDEX;

    public static void DrawBodyAddonTab(Rect rect)
    {
        Widgets.Label(rect, "BODY ADDONS HERE");
    }
}