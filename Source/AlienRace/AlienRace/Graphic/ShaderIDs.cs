using System;
using UnityEngine;
using Verse;
namespace AvaliMod
{
	// Token: 0x0200087F RID: 2175
	[StaticConstructorOnStartup]
	public static class AvaliShaderPropertyIDs
	{
		// Token: 0x06003600 RID: 13824 RVA: 0x0015B5C0 File Offset: 0x001597C0
		static AvaliShaderPropertyIDs()
		{
			AvaliShaderPropertyIDs.MaskTexName = "_MaskTex";
			AvaliShaderPropertyIDs.SwayHeadName = "_SwayHead";
			AvaliShaderPropertyIDs.ShockwaveSpanName = "_ShockwaveSpan";
			AvaliShaderPropertyIDs.AgeSecsName = "_AgeSecs";
			AvaliShaderPropertyIDs.PlanetSunLightDirection = Shader.PropertyToID(AvaliShaderPropertyIDs.PlanetSunLightDirectionName);
			AvaliShaderPropertyIDs.PlanetSunLightEnabled = Shader.PropertyToID(AvaliShaderPropertyIDs.PlanetSunLightEnabledName);
			AvaliShaderPropertyIDs.PlanetRadius = Shader.PropertyToID(AvaliShaderPropertyIDs.PlanetRadiusName);
			AvaliShaderPropertyIDs.MapSunLightDirection = Shader.PropertyToID(AvaliShaderPropertyIDs.MapSunLightDirectionName);
			AvaliShaderPropertyIDs.GlowRadius = Shader.PropertyToID(AvaliShaderPropertyIDs.GlowRadiusName);
			AvaliShaderPropertyIDs.GameSeconds = Shader.PropertyToID(AvaliShaderPropertyIDs.GameSecondsName);
			AvaliShaderPropertyIDs.AgeSecs = Shader.PropertyToID(AvaliShaderPropertyIDs.AgeSecsName);
			AvaliShaderPropertyIDs.Color = Shader.PropertyToID(AvaliShaderPropertyIDs.ColorName);
			AvaliShaderPropertyIDs.ColorTwo = Shader.PropertyToID(AvaliShaderPropertyIDs.ColorTwoName);
			AvaliShaderPropertyIDs.ColorThree = Shader.PropertyToID(AvaliShaderPropertyIDs.ColorThreeName);
			AvaliShaderPropertyIDs.MaskTex = Shader.PropertyToID(AvaliShaderPropertyIDs.MaskTexName);
			AvaliShaderPropertyIDs.SwayHead = Shader.PropertyToID(AvaliShaderPropertyIDs.SwayHeadName);
			AvaliShaderPropertyIDs.ShockwaveColor = Shader.PropertyToID("_ShockwaveColor");
			AvaliShaderPropertyIDs.ShockwaveSpan = Shader.PropertyToID(AvaliShaderPropertyIDs.ShockwaveSpanName);
			AvaliShaderPropertyIDs.WaterCastVectSun = Shader.PropertyToID("_WaterCastVectSun");
			AvaliShaderPropertyIDs.WaterCastVectMoon = Shader.PropertyToID("_WaterCastVectMoon");
			AvaliShaderPropertyIDs.WaterOutputTex = Shader.PropertyToID("_WaterOutputTex");
			AvaliShaderPropertyIDs.WaterOffsetTex = Shader.PropertyToID("_WaterOffsetTex");
			AvaliShaderPropertyIDs.ShadowCompositeTex = Shader.PropertyToID("_ShadowCompositeTex");
			AvaliShaderPropertyIDs.FallIntensity = Shader.PropertyToID("_FallIntensity");
		}

		// Token: 0x04002591 RID: 9617
		private static readonly string PlanetSunLightDirectionName = "_PlanetSunLightDirection";

		// Token: 0x04002592 RID: 9618
		private static readonly string PlanetSunLightEnabledName = "_PlanetSunLightEnabled";

		// Token: 0x04002593 RID: 9619
		private static readonly string PlanetRadiusName = "_PlanetRadius";

		// Token: 0x04002594 RID: 9620
		private static readonly string MapSunLightDirectionName = "_CastVect";

		// Token: 0x04002595 RID: 9621
		private static readonly string GlowRadiusName = "_GlowRadius";

		// Token: 0x04002596 RID: 9622
		private static readonly string GameSecondsName = "_GameSeconds";

		// Token: 0x04002597 RID: 9623
		private static readonly string ColorName = "_Color";

		// Token: 0x04002598 RID: 9624
		private static readonly string ColorTwoName = "_ColorTwo";

		// Token: 0x04002599 RID: 9625
		private static readonly string MaskTexName;

		// Token: 0x0400259A RID: 9626
		private static readonly string SwayHeadName;

		// Token: 0x0400259B RID: 9627
		private static readonly string ShockwaveSpanName;

		// Token: 0x0400259C RID: 9628
		private static readonly string AgeSecsName;

		// Token: 0x0400259D RID: 9629
		public static int PlanetSunLightDirection;

		// Token: 0x0400259E RID: 9630
		public static int PlanetSunLightEnabled;

		// Token: 0x0400259F RID: 9631
		public static int PlanetRadius;

		// Token: 0x040025A0 RID: 9632
		public static int MapSunLightDirection;

		// Token: 0x040025A1 RID: 9633
		public static int GlowRadius;

		// Token: 0x040025A2 RID: 9634
		public static int GameSeconds;

		// Token: 0x040025A3 RID: 9635
		public static int AgeSecs;

		// Token: 0x040025A4 RID: 9636
		public static int Color;

		// Token: 0x040025A5 RID: 9637
		public static int ColorTwo;

		// Token: 0x040025A6 RID: 9638
		public static int MaskTex;

		// Token: 0x040025A7 RID: 9639
		public static int SwayHead;

		// Token: 0x040025A8 RID: 9640
		public static int ShockwaveColor;

		// Token: 0x040025A9 RID: 9641
		public static int ShockwaveSpan;

		// Token: 0x040025AA RID: 9642
		public static int WaterCastVectSun;

		// Token: 0x040025AB RID: 9643
		public static int WaterCastVectMoon;

		// Token: 0x040025AC RID: 9644
		public static int WaterOutputTex;

		// Token: 0x040025AD RID: 9645
		public static int WaterOffsetTex;

		// Token: 0x040025AE RID: 9646
		public static int ShadowCompositeTex;

		// Token: 0x040025AF RID: 9647
		public static int FallIntensity;

		// Token: 0x040025B0 RID: 9648
		private static readonly string ColorThreeName = "_ColorThree";

		// Token: 0x040025B1 RID: 9649
		public static int ColorThree;
	}
}
