using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LightShadow
{
	public class LightShadowConfig : ModConfig
	{
		[Label("开启光照（Use LightShadow）")]
		[DefaultValue(true)]
		public bool UseLight;

		[Label("光照强度（LightShadow intensity）")]
		[Range(0.8f, 1.5f)]
		[DefaultValue(1f)]
		public float lightIntensity;

		[Label("月光强度（Moon Light intensity）")]
		[Range(0.8f, 1.5f)]
		[DefaultValue(1f)]
		public float moonlightIntensity;

		[Label("阴影强度（Shadow intensity）")]
		[Range(0.5f, 1.5f)]
		[DefaultValue(1f)]
		public float shadowIntensity;

		[Label("开启外发光（Use Bloom）")]
		[DefaultValue(false)]
		public bool UseBloom;

		[Label("外发光强度（Bloom Intensity）")]
		[Range(0.9f, 1.5f)]
		[DefaultValue(1f)]
		public float BloomIntensity;

		[Label("测试仪（Tester）")]
		[DefaultValue(false)]
		public bool test;



		public override ConfigScope Mode => ConfigScope.ClientSide;

		public override void OnChanged()
		{
			LightShadow.LightIntensity = this.lightIntensity;
			LightShadow.useBloom = this.UseBloom;
			LightShadow.useLight = this.UseLight;
			LightShadow.ShadowIntensity = this.shadowIntensity;
			LightShadow.bloomIntensity = this.BloomIntensity;
			LightShadow.MoonLightIntensity = this.moonlightIntensity;
		}
	}
}
