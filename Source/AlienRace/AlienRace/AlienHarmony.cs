using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienRace
{
    using System.Reflection;
    using HarmonyLib;
    using Verse;

    public class AlienHarmony
    {
        public Harmony harmony;

        public AlienHarmony(string id) => 
            this.harmony = new Harmony(id);

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
                    Log.Error($"Error during patching {original} with: pre {prefix?.method} | post: {postfix?.method} | trans: {transpiler?.method}\n{e}");
                }

            return null;
        }
    }
}
