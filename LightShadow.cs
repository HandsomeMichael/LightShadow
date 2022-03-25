using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using On.Terraria;
using On.Terraria.Graphics.Effects;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace LightShadow
{
	public class LightShadow : Mod
	{
		public static bool useLight = true;

		public static bool useBloom = true;

		public static float LightIntensity = 1f;

		public static float ShadowIntensity = 1f;

		public static float bloomIntensity = 1f;

		public static float MoonLightIntensity;

		public static bool fasttime = false;

		public RenderTarget2D screen;

		public RenderTarget2D light;

		public RenderTarget2D bloom;

		public static Effect Light;

		public static Effect Shadow;

		public static Effect Bloom;

		public override void Load()
		{
			if (!Terraria.Main.dedServ)
			{
				LightShadow.Light = GetEffect("Effects/Light");
				LightShadow.Shadow = GetEffect("Effects/Shadow");
				LightShadow.Bloom = GetEffect("Effects/Bloom1");
			}
			On.Terraria.Graphics.Effects.FilterManager.EndCapture += FilterManager_EndCapture;
			On.Terraria.Main.LoadWorlds += Main_LoadWorlds;
			Terraria.Main.OnResolutionChanged += Main_OnResolutionChanged;
		}

		public override void PostSetupContent()
		{
			if (!Terraria.Main.dedServ)
			{
				LightShadow.Light = GetEffect("Effects/Light");
				LightShadow.Shadow = GetEffect("Effects/Shadow");
				LightShadow.Bloom = GetEffect("Effects/Bloom1");
			}
		}

		public override void Unload()
		{
			On.Terraria.Graphics.Effects.FilterManager.EndCapture -= FilterManager_EndCapture;
			On.Terraria.Main.LoadWorlds -= Main_LoadWorlds;
			Terraria.Main.OnResolutionChanged -= Main_OnResolutionChanged;
			LightShadow.Light = null;
			LightShadow.Shadow = null;
			LightShadow.Bloom = null;
			this.screen = null;
			this.light = null;
			this.bloom = null;
		}

		private void Main_LoadWorlds(On.Terraria.Main.orig_LoadWorlds orig)
		{
			orig();
			if (this.screen == null)
			{
				GraphicsDevice gd = Terraria.Main.instance.GraphicsDevice;
				this.screen = new RenderTarget2D(gd, gd.PresentationParameters.BackBufferWidth / 3, gd.PresentationParameters.BackBufferHeight / 3, mipMap: false, gd.PresentationParameters.BackBufferFormat, DepthFormat.None);
				this.light = new RenderTarget2D(gd, gd.PresentationParameters.BackBufferWidth, gd.PresentationParameters.BackBufferHeight, mipMap: false, gd.PresentationParameters.BackBufferFormat, DepthFormat.None);
				this.bloom = new RenderTarget2D(gd, gd.PresentationParameters.BackBufferWidth / 3, gd.PresentationParameters.BackBufferHeight / 3, mipMap: false, gd.PresentationParameters.BackBufferFormat, DepthFormat.None);
			}
		}

		private void Main_OnResolutionChanged(Vector2 obj)
		{
			this.screen = new RenderTarget2D(Terraria.Main.instance.GraphicsDevice, Terraria.Main.screenWidth / 3, Terraria.Main.screenHeight / 3);
			this.light = new RenderTarget2D(Terraria.Main.instance.GraphicsDevice, Terraria.Main.screenWidth, Terraria.Main.screenHeight);
			this.bloom = new RenderTarget2D(Terraria.Main.instance.GraphicsDevice, Terraria.Main.screenWidth / 3, Terraria.Main.screenHeight / 3);
		}

		private void FilterManager_EndCapture(On.Terraria.Graphics.Effects.FilterManager.orig_EndCapture orig, Terraria.Graphics.Effects.FilterManager self)
		{
			if (ModContent.GetInstance<LightShadowConfig>().test) {
				LightShadow.fasttime = !LightShadow.fasttime;
			}
			GraphicsDevice gd = Terraria.Main.graphics.GraphicsDevice;
			SpriteBatch sb = Terraria.Main.spriteBatch;
			if (Terraria.Main.myPlayer != 255)
			{
				if (LightShadow.useBloom)
				{
					this.UseBloom(gd);
				}
				if (LightShadow.useLight)
				{
					this.UseLightAndShadow(gd, sb);
				}
			}
			orig(self);
		}

		private void UseBloom(GraphicsDevice graphicsDevice)
		{
			graphicsDevice.SetRenderTarget(Terraria.Main.screenTargetSwap);
			graphicsDevice.Clear(Color.Transparent);
			Terraria.Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			Terraria.Main.spriteBatch.Draw(Terraria.Main.screenTarget, Vector2.Zero, Color.White);
			Terraria.Main.spriteBatch.End();
			graphicsDevice.SetRenderTarget(this.screen);
			graphicsDevice.Clear(Color.Transparent);
			Terraria.Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			Color c = Terraria.Main.bgColor;
			float i2 = 1f - (float)(c.R + c.G + c.B) / 765f;
			i2 = ((!Terraria.Main.dayTime) ? (i2 - 0.25f) : (i2 - 0.4f));
			int dis = (int)Terraria.Main.LocalPlayer.Center.Y - (int)Terraria.Main.worldSurface * 16;
			if (dis > 500 && dis < 1000)
			{
				i2 += (float)(dis - 500) / 500f * 1f;
			}
			if (dis > 1000)
			{
				i2 = 1.2f;
			}
			if (Terraria.NPC.AnyNPCs(636))
			{
				i2 -= 0.5f;
			}
			if (Terraria.NPC.AnyNPCs(398))
			{
				i2 = 0.3f;
			}
			if (Terraria.Main.LocalPlayer.ZoneSnow)
			{
				i2 -= 0.2f;
				if (dis > 500)
				{
					i2 -= 0.5f;
				}
			}
			LightShadow.Bloom.CurrentTechnique.Passes[0].Apply();
			LightShadow.Bloom.Parameters["m"].SetValue(1f - i2 * 0.3f);
			LightShadow.Bloom.Parameters["n"].SetValue(2.5f);
			if (Terraria.Main.LocalPlayer.ZoneUnderworldHeight)
			{
				LightShadow.Bloom.Parameters["hell"].SetValue(value: true);
				LightShadow.Bloom.Parameters["m"].SetValue(0.6f);
			}
			Terraria.Main.spriteBatch.Draw(Terraria.Main.screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 0.333f, SpriteEffects.None, 0f);
			Terraria.Main.spriteBatch.End();
			Terraria.Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			LightShadow.Bloom.Parameters["uScreenResolution"].SetValue(new Vector2(Terraria.Main.screenWidth / 3, Terraria.Main.screenHeight / 3));
			LightShadow.Bloom.Parameters["uRange"].SetValue(1.5f);
			LightShadow.Bloom.Parameters["uIntensity"].SetValue(LightShadow.bloomIntensity);
			if (Terraria.Main.LocalPlayer.ZoneUnderworldHeight)
			{
				LightShadow.Bloom.Parameters["uIntensity"].SetValue(0.8f * LightShadow.bloomIntensity);
			}
			for (int i = 0; i < 2; i++)
			{
				LightShadow.Bloom.CurrentTechnique.Passes["GlurV"].Apply();
				graphicsDevice.SetRenderTarget(this.bloom);
				graphicsDevice.Clear(Color.Transparent);
				Terraria.Main.spriteBatch.Draw(this.screen, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				LightShadow.Bloom.CurrentTechnique.Passes["GlurH"].Apply();
				graphicsDevice.SetRenderTarget(this.screen);
				graphicsDevice.Clear(Color.Transparent);
				Terraria.Main.spriteBatch.Draw(this.bloom, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}
			Terraria.Main.spriteBatch.End();
			graphicsDevice.SetRenderTarget(Terraria.Main.screenTarget);
			graphicsDevice.Clear(Color.Transparent);
			Terraria.Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			Terraria.Main.spriteBatch.Draw(Terraria.Main.screenTargetSwap, Vector2.Zero, Color.White);
			Terraria.Main.spriteBatch.Draw(this.screen, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
			Terraria.Main.spriteBatch.End();
		}

		private void UseLightAndShadow(GraphicsDevice gd, SpriteBatch sb)
		{
			if (LightShadow.fasttime)
			{
				Terraria.Main.time += 50.0;
			}
			gd.SetRenderTarget(Terraria.Main.screenTargetSwap);
			gd.Clear(Color.Transparent);
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			sb.Draw(Terraria.Main.screenTarget, Vector2.Zero, Color.White);
			sb.End();
			gd.SetRenderTarget(this.light);
			gd.Clear(Color.Transparent);
			sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
			LightShadow.Light.CurrentTechnique.Passes["Light"].Apply();
			LightShadow.Light.Parameters["uScreenResolution"].SetValue(new Vector2(Terraria.Main.screenWidth, Terraria.Main.screenHeight));
			LightShadow.Light.Parameters["uPos"].SetValue(LightShadow.ToScreenCoords(this.GetSunPos()));
			LightShadow.Light.Parameters["tex0"].SetValue(ModContent.GetTexture("LightShadow/" + (Terraria.Main.dayTime ? "ColorTex" : "ColorTex2")));
			Color c = Terraria.Main.bgColor;
			float intensity = 1f - (float)(c.R + c.G + c.B) / 765f;
			float k = 1.1f * LightShadow.LightIntensity;
			if (Terraria.Main.LocalPlayer.ZoneSnow && !Terraria.Main.LocalPlayer.ZoneCrimson && !Terraria.Main.LocalPlayer.ZoneCorrupt)
			{
				intensity -= Terraria.Main.bgAlpha[7] * 0.1f;
			}
			if (Terraria.Main.LocalPlayer.ZoneCrimson)
			{
				intensity += 0.2f;
			}
			if (Terraria.Main.snowBG[0] == 263 || Terraria.Main.snowBG[0] == 258 || Terraria.Main.snowBG[0] == 267)
			{
				intensity -= Terraria.Main.bgAlpha[7] * 0.6f;
			}
			if (Terraria.Main.desertBG[0] == 248)
			{
				intensity -= Terraria.Main.bgAlpha[2] * 0.4f;
			}
			float moonlightI = 1f;
			if (Terraria.Main.moonPhase == 0)
			{
				moonlightI = 1.01f;
			}
			if (Terraria.Main.moonPhase == 3 || Terraria.Main.moonPhase == 5)
			{
				moonlightI = 0.9f;
			}
			if (Terraria.Main.moonPhase == 4)
			{
				moonlightI = 0.6f;
			}
			LightShadow.Light.Parameters["intensity"].SetValue(Terraria.Main.dayTime ? (k * (0.8f + intensity * 0.3f)) : (LightShadow.MoonLightIntensity * moonlightI));
			LightShadow.Light.Parameters["t"].SetValue((float)Terraria.Main.time / 54000f);
			if (!Terraria.Main.dayTime)
			{
				LightShadow.Light.Parameters["t"].SetValue((float)Terraria.Main.time / 32400f);
			}
			if ((double)Terraria.Main.LocalPlayer.Center.Y < Terraria.Main.worldSurface * 16.0 + 800.0)
			{
				sb.Draw(ModContent.GetTexture("LightShadow/PixelEX"), new Rectangle(0, 0, Terraria.Main.screenWidth, Terraria.Main.screenHeight), Color.White);
			}
			sb.End();
			gd.SetRenderTarget(this.screen);
			gd.Clear(Color.Transparent);
			sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			LightShadow.Shadow.CurrentTechnique.Passes[0].Apply();
			LightShadow.Shadow.Parameters["uScreenResolution"].SetValue(new Vector2(Terraria.Main.screenWidth, Terraria.Main.screenHeight));
			float desertI = Terraria.Main.bgAlpha[2] * 0.2f;
			if (Terraria.Main.desertBG[0] == 248)
			{
				desertI = 0f;
			}
			LightShadow.Shadow.Parameters["m"].SetValue(1f - desertI);
			if (!Terraria.Main.dayTime)
			{
				LightShadow.Shadow.Parameters["m"].SetValue(0.02f);
			}
			LightShadow.Shadow.Parameters["uPos"].SetValue(LightShadow.ToScreenCoords(this.GetSunPos()));
			sb.Draw(Terraria.Main.screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 0.333f, SpriteEffects.None, 0f);
			sb.End();
			gd.SetRenderTarget(Terraria.Main.screenTarget);
			gd.Clear(Color.Transparent);
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive);
			if (Terraria.Main.dayTime)
			{
				for (int j = 0; j < 30; j++)
				{
					float shaderIntens2 = 105f;
					if (Terraria.Main.snowBG[0] == 263 || Terraria.Main.snowBG[0] == 258 || Terraria.Main.snowBG[0] == 267)
					{
						shaderIntens2 -= Terraria.Main.bgAlpha[7] * 30f;
					}
					float a2 = (30f - (float)j) / shaderIntens2;
					sb.Draw(this.screen, this.GetSunPos() / 3f, null, Color.White * a2 * LightShadow.ShadowIntensity, 0f, this.GetSunPos() / 3f, 1f * (1f + (float)j * 0.01f), SpriteEffects.None, 0f);
				}
			}
			else
			{
				for (int i = 0; i < 20; i++)
				{
					float shaderIntens = 195f;
					float a = (20f - (float)i) / shaderIntens;
					sb.Draw(this.screen, this.GetSunPos() / 3f, null, Color.White * a, 0f, this.GetSunPos() / 3f, 1f * (1f + (float)i * 0.015f), SpriteEffects.None, 0f);
				}
			}
			sb.End();
			gd.SetRenderTarget(this.screen);
			gd.Clear(Color.Transparent);
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			sb.Draw(Terraria.Main.screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			sb.End();
			gd.SetRenderTarget(Terraria.Main.screenTarget);
			gd.Clear(Color.Transparent);
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive);
			sb.Draw(Terraria.Main.screenTargetSwap, Vector2.Zero, Color.White);
			sb.End();
			sb.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			LightShadow.Shadow.CurrentTechnique.Passes[1].Apply();
			LightShadow.Shadow.Parameters["tex0"].SetValue(this.screen);
			sb.Draw(this.light, Vector2.Zero, Color.White);
			sb.End();
		}

		public static Vector2 ToScreenCoords(Vector2 vec)
		{
			return vec / new Vector2(Terraria.Main.screenWidth, Terraria.Main.screenHeight);
		}

		public Vector2 GetSunPos()
		{
			float bgTop = (int)((0.0 - (double)Terraria.Main.screenPosition.Y) / (Terraria.Main.worldSurface * 16.0 - 600.0) * 200.0);
			int num31 = (int)(Terraria.Main.time / (Terraria.Main.dayTime ? 54000.0 : 32400.0) * (double)(Terraria.Main.screenWidth + 200)) - 100;
			int num29 = 0;
			if (Terraria.Main.dayTime)
			{
				if (Terraria.Main.time < 27000.0)
				{
					double num30 = Math.Pow(1.0 - Terraria.Main.time / 54000.0 * 2.0, 2.0);
					num29 = (int)((double)bgTop + num30 * 250.0 + 180.0);
				}
				else
				{
					double num30 = Math.Pow((Terraria.Main.time / 54000.0 - 0.5) * 2.0, 2.0);
					num29 = (int)((double)bgTop + num30 * 250.0 + 180.0);
				}
			}
			return new Vector2(num31, num29 + Terraria.Main.sunModY);
		}
	}
}
