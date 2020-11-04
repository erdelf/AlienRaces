using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
namespace AvaliMod
{
	// Token: 0x0200046D RID: 1133
	internal static class AvaliMaterialAllocator
	{
		// Token: 0x06001CBD RID: 7357 RVA: 0x000F1A98 File Offset: 0x000EFC98
		public static Material Create(Material material)
		{
			Material material2 = new Material(material);
			AvaliMaterialAllocator.references[material2] = new AvaliMaterialAllocator.MaterialInfo
			{
				stackTrace = (Prefs.DevMode ? Environment.StackTrace : "(unavailable)")
			};
			AvaliMaterialAllocator.TryReport();
			return material2;
		}

		// Token: 0x06001CBE RID: 7358 RVA: 0x000F1AE0 File Offset: 0x000EFCE0
		public static Material Create(Shader shader)
		{
			Material material = new Material(shader);
			AvaliMaterialAllocator.references[material] = new AvaliMaterialAllocator.MaterialInfo
			{
				stackTrace = (Prefs.DevMode ? Environment.StackTrace : "(unavailable)")
			};
			AvaliMaterialAllocator.TryReport();
			return material;
		}

		// Token: 0x06001CBF RID: 7359 RVA: 0x00019F8E File Offset: 0x0001818E
		public static void Destroy(Material material)
		{
			if (!AvaliMaterialAllocator.references.ContainsKey(material))
			{
				Log.Error(string.Format("Destroying material {0}, but that material was not created through the MaterialTracker", material), false);
			}
			AvaliMaterialAllocator.references.Remove(material);
            UnityEngine.Object.Destroy(material);
		}

		// Token: 0x06001CC0 RID: 7360 RVA: 0x000F1B28 File Offset: 0x000EFD28
		public static void TryReport()
		{
			if (AvaliMaterialAllocator.MaterialWarningThreshold() > AvaliMaterialAllocator.nextWarningThreshold)
			{
				AvaliMaterialAllocator.nextWarningThreshold = AvaliMaterialAllocator.MaterialWarningThreshold();
			}
			if (AvaliMaterialAllocator.references.Count > AvaliMaterialAllocator.nextWarningThreshold)
			{
				Log.Error(string.Format("Material allocator has allocated {0} materials; this may be a sign of a material leak", AvaliMaterialAllocator.references.Count), false);
				if (Prefs.DevMode)
				{
					AvaliMaterialAllocator.MaterialReport();
				}
				AvaliMaterialAllocator.nextWarningThreshold *= 2;
			}
		}

		// Token: 0x06001CC1 RID: 7361 RVA: 0x00019FC0 File Offset: 0x000181C0
		public static int MaterialWarningThreshold()
		{
			return int.MaxValue;
		}

		// Token: 0x06001CC2 RID: 7362 RVA: 0x000F1B94 File Offset: 0x000EFD94
		[DebugOutput("System", false)]
		public static void MaterialReport()
		{
			foreach (string text in Enumerable.Take<string>(Enumerable.Select<IGrouping<string, KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>>, string>(Enumerable.OrderByDescending<IGrouping<string, KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>>, int>(Enumerable.GroupBy<KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>, string>(AvaliMaterialAllocator.references, (KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo> kvp) => kvp.Value.stackTrace), (IGrouping<string, KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>> g) => Enumerable.Count<KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>>(g)), (IGrouping<string, KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>> g) => string.Format("{0}: {1}", Enumerable.Count<KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>>(g), Enumerable.FirstOrDefault<KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>>(g).Value.stackTrace)), 20))
			{
				Log.Error(text, false);
			}
		}

		// Token: 0x06001CC3 RID: 7363 RVA: 0x000F1C54 File Offset: 0x000EFE54
		[DebugOutput("System", false)]
		public static void MaterialSnapshot()
		{
			AvaliMaterialAllocator.snapshot = new Dictionary<string, int>();
			foreach (IGrouping<string, KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>> grouping in Enumerable.GroupBy<KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>, string>(AvaliMaterialAllocator.references, (KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo> kvp) => kvp.Value.stackTrace))
			{
				AvaliMaterialAllocator.snapshot[grouping.Key] = Enumerable.Count<KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>>(grouping);
			}
		}

		// Token: 0x06001CC4 RID: 7364 RVA: 0x000F1CE0 File Offset: 0x000EFEE0
		[DebugOutput("System", false)]
		public static void MaterialDelta()
		{
			IEnumerable<string> enumerable = Enumerable.Distinct<string>(Enumerable.Concat<string>(Enumerable.Select<AvaliMaterialAllocator.MaterialInfo, string>(AvaliMaterialAllocator.references.Values, (AvaliMaterialAllocator.MaterialInfo v) => v.stackTrace), AvaliMaterialAllocator.snapshot.Keys));
			Dictionary<string, int> currentSnapshot = new Dictionary<string, int>();
			foreach (IGrouping<string, KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>> grouping in Enumerable.GroupBy<KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>, string>(AvaliMaterialAllocator.references, (KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo> kvp) => kvp.Value.stackTrace))
			{
				currentSnapshot[grouping.Key] = Enumerable.Count<KeyValuePair<Material, AvaliMaterialAllocator.MaterialInfo>>(grouping);
			}
			foreach (string text in Enumerable.Take<string>(Enumerable.Select<KeyValuePair<string, int>, string>(Enumerable.OrderByDescending<KeyValuePair<string, int>, int>(Enumerable.Select<string, KeyValuePair<string, int>>(enumerable, (string k) => new KeyValuePair<string, int>(k, currentSnapshot.TryGetValue(k, 0) - AvaliMaterialAllocator.snapshot.TryGetValue(k, 0))), (KeyValuePair<string, int> kvp) => kvp.Value), (KeyValuePair<string, int> g) => string.Format("{0}: {1}", g.Value, g.Key)), 20))
			{
				Log.Error(text, false);
			}
		}

		// Token: 0x04001489 RID: 5257
		private static Dictionary<Material, AvaliMaterialAllocator.MaterialInfo> references = new Dictionary<Material, AvaliMaterialAllocator.MaterialInfo>();

		// Token: 0x0400148A RID: 5258
		public static int nextWarningThreshold;

		// Token: 0x0400148B RID: 5259
		private static Dictionary<string, int> snapshot = new Dictionary<string, int>();

		// Token: 0x0200046E RID: 1134
		private struct MaterialInfo
		{
			// Token: 0x0400148C RID: 5260
			public string stackTrace;
		}
	}
}
