using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace AvaliMod
{
	// Token: 0x02000476 RID: 1142
	public static class AvaliMaterialPool
	{
		// Token: 0x06001CDD RID: 7389 RVA: 0x0001A0BF File Offset: 0x000182BF
		public static Material MatFrom(string texPath, bool reportFailure)
		{
			if (texPath == null || texPath == "null")
			{
				return null;
			}
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(ContentFinder<Texture2D>.Get(texPath, reportFailure)));
		}

		// Token: 0x06001CDE RID: 7390 RVA: 0x0001A0E4 File Offset: 0x000182E4
		public static Material MatFrom(string texPath)
		{
			if (texPath == null || texPath == "null")
			{
				return null;
			}
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(ContentFinder<Texture2D>.Get(texPath, true)));
		}

		// Token: 0x06001CDF RID: 7391 RVA: 0x0001A109 File Offset: 0x00018309
		public static Material MatFrom(Texture2D srcTex)
		{
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(srcTex));
		}

		// Token: 0x06001CE0 RID: 7392 RVA: 0x0001A116 File Offset: 0x00018316
		public static Material MatFrom(Texture2D srcTex, Shader shader, Color color)
		{
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(srcTex, shader, color));
		}

		// Token: 0x06001CE1 RID: 7393 RVA: 0x000F2034 File Offset: 0x000F0234
		public static Material MatFrom(Texture2D srcTex, Shader shader, Color color, int renderQueue)
		{
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(srcTex, shader, color)
			{
				renderQueue = renderQueue
			});
		}

		// Token: 0x06001CE2 RID: 7394 RVA: 0x0001A125 File Offset: 0x00018325
		public static Material MatFrom(string texPath, Shader shader)
		{
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader));
		}

		// Token: 0x06001CE3 RID: 7395 RVA: 0x000F205C File Offset: 0x000F025C
		public static Material MatFrom(string texPath, Shader shader, int renderQueue)
		{
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader)
			{
				renderQueue = renderQueue
			});
		}

		// Token: 0x06001CE4 RID: 7396 RVA: 0x0001A139 File Offset: 0x00018339
		public static Material MatFrom(string texPath, Shader shader, Color color)
		{
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader, color));
		}

		// Token: 0x06001CE5 RID: 7397 RVA: 0x000F2088 File Offset: 0x000F0288
		public static Material MatFrom(string texPath, Shader shader, Color color, int renderQueue)
		{
			return AvaliMaterialPool.MatFrom(new AvaliMaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader, color)
			{
				renderQueue = renderQueue
			});
		}

		// Token: 0x06001CE6 RID: 7398 RVA: 0x000F20B4 File Offset: 0x000F02B4
		public static Material MatFrom(AvaliMaterialRequest req)
		{
			//Log.Message("thisine");
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to get a material from a different thread.", false);
				return null;
			}
			if (req.mainTex == null)
			{
				Log.Error("MatFrom with null sourceTex.", false);
				return AvaliBaseContent.BadMat;
			}
			if (req.shader == null)
			{
				Log.Warning("Matfrom with null shader.", false);
				return AvaliBaseContent.BadMat;
			}
			/*
			if (req.maskTex != null && !req.shader.SupportsMaskTex())
			{
				Log.Error("MaterialRequest has maskTex but shader does not support it. req=" + req.ToString(), false);
				req.maskTex = null;
			}
			*/
			req.color = req.color;
			req.colorTwo = req.colorTwo;
			req.colorThree = req.colorThree;
			Material material;
			if (!AvaliMaterialPool.matDictionary.TryGetValue(req, out material))
			{
				material = AvaliMaterialAllocator.Create(req.shader);
				material.name = req.shader.name + "_" + req.mainTex.name;
				material.mainTexture = req.mainTex;
				material.color = req.color;
				material.SetTexture(AvaliShaderPropertyIDs.MaskTex, req.maskTex);
				material.SetColor(AvaliShaderPropertyIDs.ColorTwo, req.colorTwo);
				material.SetColor(AvaliShaderPropertyIDs.ColorThree, req.colorThree);
				// liQd Comment there it is
				material.SetTexture(ShaderPropertyIDs.MaskTex, req.maskTex);
				//Log.Message("thisthinghascolor3: " + req.colorThree);
				if (req.renderQueue != 0)
				{
					material.renderQueue = req.renderQueue;
				}
				if (!req.shaderParameters.NullOrEmpty<ShaderParameter>())
				{
					for (int i = 0; i < req.shaderParameters.Count; i++)
					{
						req.shaderParameters[i].Apply(material);
					}
				}
				AvaliMaterialPool.matDictionary.Add(req, material);
				if (req.shader == ShaderDatabase.CutoutPlant || req.shader == ShaderDatabase.TransparentPlant)
				{
					WindManager.Notify_PlantMaterialCreated(material);
				}
			}
			return material;
		}

		// Token: 0x0400149D RID: 5277
		private static Dictionary<AvaliMaterialRequest, Material> matDictionary = new Dictionary<AvaliMaterialRequest, Material>();
	}
}
