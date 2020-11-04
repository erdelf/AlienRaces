using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace AvaliMod
{
	// Token: 0x02000477 RID: 1143
	public struct AvaliMaterialRequest : IEquatable<AvaliMaterialRequest>
	{
		// Token: 0x17000583 RID: 1411
		// (set) Token: 0x06001CE8 RID: 7400 RVA: 0x0001A15A File Offset: 0x0001835A
		public string BaseTexPath
		{
			set
			{
				this.mainTex = ContentFinder<Texture2D>.Get(value, true);
			}
		}

		// Token: 0x06001CE9 RID: 7401 RVA: 0x000F228C File Offset: 0x000F048C
		public AvaliMaterialRequest(Texture2D tex)
		{
			Log.Message("this mat req");
			this.shader = ShaderDatabase.Cutout;
			this.mainTex = tex;
			this.color = Color.red;
			this.colorTwo = Color.green;
			this.colorThree = Color.blue;
			this.maskTex = null;
			this.renderQueue = 0;
			this.shaderParameters = null;
		}

		// Token: 0x06001CEA RID: 7402 RVA: 0x000F22E4 File Offset: 0x000F04E4
		public AvaliMaterialRequest(Texture2D tex, Shader shader)
		{
			Log.Message("matreq2");
			this.shader = shader;
			this.mainTex = tex;
			this.color = Color.green;
			this.colorTwo = Color.blue;
			this.colorThree = Color.red;
			this.maskTex = null;
			this.renderQueue = 0;
			this.shaderParameters = null;
		}

		// Token: 0x06001CEB RID: 7403 RVA: 0x000F2338 File Offset: 0x000F0538
		public AvaliMaterialRequest(Texture2D tex, Shader shader, Color color)
		{
			Log.Message("matreq3");
			this.shader = shader;
			this.mainTex = tex;
			this.color = color;
			this.colorTwo = Color.red;
			this.colorThree = Color.blue;
			this.maskTex = null;
			this.renderQueue = 0;
			this.shaderParameters = null;
		}

		// Token: 0x06001CEC RID: 7404 RVA: 0x000F2388 File Offset: 0x000F0588
		public override int GetHashCode()
		{
			return Gen.HashCombine<List<ShaderParameter>>(Gen.HashCombineInt(Gen.HashCombine<Texture2D>(Gen.HashCombine<Texture2D>(Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombine<Shader>(0, this.shader), this.color), this.colorTwo), this.mainTex), this.maskTex), this.renderQueue), this.shaderParameters);
		}

		// Token: 0x06001CED RID: 7405 RVA: 0x0001A169 File Offset: 0x00018369
		public override bool Equals(object obj)
		{
			return obj is AvaliMaterialRequest && this.Equals((AvaliMaterialRequest)obj);
		}

		// Token: 0x06001CEE RID: 7406 RVA: 0x000F23E4 File Offset: 0x000F05E4
		public bool Equals(AvaliMaterialRequest other)
		{
			return other.shader == this.shader && other.mainTex == this.mainTex && other.color == this.color && other.colorTwo == this.colorTwo && other.maskTex == this.maskTex && other.renderQueue == this.renderQueue && other.shaderParameters == this.shaderParameters;
		}

		// Token: 0x06001CEF RID: 7407 RVA: 0x0001A181 File Offset: 0x00018381
		public static bool operator ==(AvaliMaterialRequest lhs, AvaliMaterialRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		// Token: 0x06001CF0 RID: 7408 RVA: 0x0001A18B File Offset: 0x0001838B
		public static bool operator !=(AvaliMaterialRequest lhs, AvaliMaterialRequest rhs)
		{
			return !(lhs == rhs);
		}

		// Token: 0x06001CF1 RID: 7409 RVA: 0x000F2470 File Offset: 0x000F0670
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"AvaliMaterialRequest(",
				this.shader.name,
				", ",
				this.mainTex.name,
				", ",
				this.color.ToString(),
				", ",
				this.colorTwo.ToString(),
				", ",
				this.colorThree.ToString(),
				",",
				this.maskTex.ToString(),
				", ",
				this.renderQueue.ToString(),
				")"
			});
		}

		// Token: 0x0400149E RID: 5278
		public Shader shader;

		// Token: 0x0400149F RID: 5279
		public Texture2D mainTex;

		// Token: 0x040014A0 RID: 5280
		public Color color;

		// Token: 0x040014A1 RID: 5281
		public Color colorTwo;

		// Token: 0x040014A2 RID: 5282
		public Texture2D maskTex;

		// Token: 0x040014A3 RID: 5283
		public int renderQueue;

		// Token: 0x040014A4 RID: 5284
		public List<ShaderParameter> shaderParameters;

		// Token: 0x040014A5 RID: 5285
		public Color colorThree;
	}
}
