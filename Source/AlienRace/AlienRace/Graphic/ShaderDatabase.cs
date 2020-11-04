using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using AlienRace
namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public class AvaliShaderDatabase
    {
        static AvaliShaderDatabase()
        {
            string dir = AlienRace.AlienRaceMod.dir;

            Log.Message(dir);
            string path = dir + "/RimValiAssetBundles/shader";
            AssetBundle bundle = RimValiUtility.shaderLoader(path);
            Tricolor = (Shader)bundle.LoadAsset("assets/resources/materials/avalishader.shader");
            if (!(Tricolor == null))
            {
                Log.Message(Tricolor.name);
                Log.Message("Load worked!");
            }
        }

        public static Shader Tricolor;
        public static Dictionary<string, Shader> lookup;

        public static Shader DefaultShader
        {
            get
            {
                return AvaliShaderDatabase.Tricolor;
            }
        }
    }
}