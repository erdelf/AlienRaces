using System;

namespace AlienRace
{
    using System.Collections.Generic;
    using HarmonyLib;
    using System.Linq;
    using System.Reflection;
    using Verse;

    public class AlienHarmony(string id)
    {
        public readonly Harmony harmony = new(id);
        public          string  PatchReport
        {
            get
            {
                List<Patches> patchInfos = this.harmony.GetPatchedMethods().Select(Harmony.GetPatchInfo).ToList();

                int prefixCount     = patchInfos.SelectMany(p => p.Prefixes).Count(predicate: p => p.owner == this.harmony.Id);
                int postfixCount    = patchInfos.SelectMany(p => p.Postfixes).Count(predicate: p => p.owner == this.harmony.Id);
                int transpilerCount = patchInfos.SelectMany(p => p.Transpilers).Count(predicate: p => p.owner == this.harmony.Id);

                return $"{prefixCount + postfixCount + transpilerCount} patches ({prefixCount} pre, {postfixCount} post, {transpilerCount} trans)";
            }
        }

        public MethodInfo Patch(
            MethodBase    original,
            HarmonyMethod prefix     = null,
            HarmonyMethod postfix    = null,
            HarmonyMethod transpiler = null,
            HarmonyMethod finalizer  = null)
        {
            if (original == null) 
                Log.Error($"{nameof(original)} is null for: pre {prefix?.method} | post: {postfix?.method} | trans: {transpiler?.method}");
            else if(prefix?.method == null && postfix?.method == null && transpiler?.method == null)
                Log.Error($"Patches are null for {original}");
            else
                try
                {
                    MethodInfo methodInfo = this.harmony.Patch(original, prefix, postfix, transpiler, finalizer);
                    return methodInfo;
                }
                catch (Exception e)
                {
                    Log.Error($"Error during patching {original.DeclaringType?.FullName} :: {original} with: pre {prefix?.method} | post: {postfix?.method} | trans: {transpiler?.method}\n{e}");
                }

            return null;
        }
    }
}
