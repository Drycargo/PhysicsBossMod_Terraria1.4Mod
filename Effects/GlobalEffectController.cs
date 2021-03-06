using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace PhysicsBoss.Effects
{
    public static class GlobalEffectController
    {
        public const int SHRINK_CONST = 2;

        public static Texture2D beamTex  = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Beam").Value;
        public static Texture2D ColorGradient  = ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/ColorGradient").Value;
        public static Texture2D circleTex  = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Circle").Value;

        private static float bloomInten = 0f;
        private static float bloomThreshold = 1;

        private static Vector2 CTCenter = Vector2.Zero;
        private static float CTInten = 0;
        private static float CTRadius = 0;
        private static float CTWidth = 0;

        public const float FLASH_PERIOD = 16;
        public const float FLASH_TRANSPARENCY = 0.9f;
        public const int FLASH_WIDTH = 12;
        private static float flashThreshold = 1;
        private static Vector2 flashCenterPoint = Vector2.Zero;
        private static Vector2 realCenterPoint;
        private static int flashTimeLeft = 0;
        private static float dispIntensity = 0;

        private static int skyTimeLeft = 0;
        private static Color skyColor = Color.Transparent;

        private static float vigInten = 0;
        private static float vigStartPercent = 0;
        private static float vigIntenNoise = 0;
        private static float vigStartPercentNoise = 0;

        public static void changeSkyColor(Color c, int duration) {
            skyTimeLeft = duration;
            skyColor = c;
        }

        public static void applySkyColor() {
            if (skyTimeLeft <= 0)
            {
                skyColor = Color.Transparent;
                return;
            }
            skyTimeLeft--;
            Main.ColorOfTheSkies = skyColor;
        }

        public static void drawAimLine(SpriteBatch spriteBatch, Vector2 center, Vector2 targetPos, Color color, float width)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);


            spriteBatch.Draw(beamTex, (center + targetPos) / 2 - Main.screenPosition,
                null, color, (center - targetPos).ToRotation() + MathHelper.PiOver2, beamTex.Size() / 2f,
                new Vector2(width / (float)beamTex.Width, (center - targetPos).Length() / (float)beamTex.Height), SpriteEffects.None, 0);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public static void drawRayLine(SpriteBatch spriteBatch, Vector2 center, Vector2 targetPos, Color color, float width)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            if (targetPos.Distance(center) != 0)
            {
                targetPos += 2000f * (targetPos - center).SafeNormalize(Vector2.UnitX);
            }

            spriteBatch.Draw(beamTex, (center + targetPos) / 2 - Main.screenPosition,
                null, color, (center - targetPos).ToRotation() + MathHelper.PiOver2, beamTex.Size() / 2f,
                new Vector2(width / (float)beamTex.Width, (center - targetPos).Length() / (float)beamTex.Height), SpriteEffects.None, 0);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public static void drawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color c) {
            if (radius <= 0)
                return;

            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            

            Vector2 pos = center - Main.screenPosition - radius * Vector2.One;

            spriteBatch.Draw(ModContent.Request<Texture2D>("PhysicsBoss/Asset/Circle").Value,
                new Rectangle((int)(pos.X), (int)pos.Y,
                (int)(2* radius), (int)(2 * radius)), null, c * 2f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public static void vignette(float intensity, float startPercent, float intenNoise = 0, float percentNoise = 0) {
            vigInten = intensity;
            vigStartPercent = startPercent;
            vigIntenNoise = intenNoise;
            vigStartPercentNoise = percentNoise;
        }


        public static void Main_applyVignette()
        {
            if (vigInten <= 0)
            {
                if (Filters.Scene["PhysicsBoss:Vignette"].IsActive())
                    Filters.Scene.Deactivate("PhysicsBoss:Vignette");
                return;
            }

            if (!Filters.Scene["PhysicsBoss:Vignette"].IsActive())
            {
                Filters.Scene.Activate("PhysicsBoss:Vignette");
            }

            PhysicsBoss.worldEffect.Parameters["vigInten"].SetValue(vigInten * (1 + vigIntenNoise * (Main.rand.NextFloat() - 0.5f)));
            PhysicsBoss.worldEffect.Parameters["vigStartPercent"].SetValue(vigStartPercent * (1 + vigStartPercentNoise * (Main.rand.NextFloat() - 0.5f)));

            if(vigInten > 0)
                vigInten -= 0.1f;
        }

        public static void shake(float intensity) {
            /*
            if (intensity < 0 && Filters.Scene["PhysicsBoss:Shake"].IsActive())
            {
                Filters.Scene.Deactivate("PhysicsBoss:Shake");
                return;
            }

            if (!Filters.Scene["PhysicsBoss:Shake"].IsActive())
            {
                Filters.Scene.Activate("PhysicsBoss:Shake");
            }

            PhysicsBoss.worldEffect.Parameters["vibInten"].SetValue(
                     Main.rand.NextVector2Unit() * intensity);
            */

            if (intensity < 0)
                return;

            CameraPlayer.setShake(Main.rand.NextVector2Unit() * intensity);
        }

        public static void blur(float intensity)
        {
            if (intensity < 0 && Filters.Scene["PhysicsBoss:Blur"].IsActive()) {
                Filters.Scene.Deactivate("PhysicsBoss:Blur");
                return;
            }

            if (!Filters.Scene["PhysicsBoss:Blur"].IsActive())
            {
                Filters.Scene.Activate("PhysicsBoss:Blur");
            }

            PhysicsBoss.worldEffect.Parameters["blurInten"].SetValue(intensity);
        }

        public static void centerTwist(float intensity, float radius, float width, Vector2 Center)
        {
            CTInten = intensity;
            CTRadius = radius;
            CTWidth = width;
            CTCenter = Center;
        }

        public static void Main_applyCenterTwist() {
            if (CTInten <= 0) {
                if (Filters.Scene["PhysicsBoss:CenterTwist"].IsActive())
                    Filters.Scene.Deactivate("PhysicsBoss:CenterTwist");
                return;
            }

            if (CTInten > 0 && !Filters.Scene["PhysicsBoss:CenterTwist"].IsActive())
            {
                Filters.Scene.Activate("PhysicsBoss:CenterTwist");
            }

            PhysicsBoss.worldEffect.Parameters["twistInten"].SetValue(CTInten);
            PhysicsBoss.worldEffect.Parameters["twistRadius"].SetValue(CTRadius);
            PhysicsBoss.worldEffect.Parameters["twistWidth"].SetValue(CTWidth);
            PhysicsBoss.worldEffect.Parameters["twistCenter"].SetValue(CTCenter - Main.screenPosition);
            PhysicsBoss.worldEffect.Parameters["texSize"].SetValue(Main.ScreenSize.ToVector2());
        }

        public static void bloom(float intensity, float threshold)
        {
            bloomInten = intensity;
            bloomThreshold = threshold;
        }

        public static void Main_applyBloom()
        {
            /*
            if (bloomInten < 0 && Filters.Scene["PhysicsBoss:Bloom"].IsActive())
            {
                Filters.Scene.Deactivate("PhysicsBoss:Bloom");
                return;
            }

            if (!Filters.Scene["PhysicsBoss:Bloom"].IsActive())
            {
                Filters.Scene.Activate("PhysicsBoss:Bloom");
            }

            PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(bloomInten);
            PhysicsBoss.worldEffect.Parameters["blurbloomThreshold"].SetValue(threashold);
            */

            if (bloomInten <= 0)
                return;

            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width / 2, Main.screenTarget.Height / 2);
            SpriteBatch spriteBatch = Main.spriteBatch;

            #region original
            /*
            for (int i = 0; i < 3; i++)
            {
                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);

                PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(bloomInten * 0.25f);
                PhysicsBoss.worldEffect.Parameters["blurbloomThreshold"].SetValue(threashold);

                //PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurOnbloomThreshold"].Apply();
                PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurbloomThresholdH"].Apply();

                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(bloomInten * 0.25f);
                PhysicsBoss.worldEffect.Parameters["blurbloomThreshold"].SetValue(threashold);

                PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurbloomThresholdV"].Apply();

                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            */
            #endregion


            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            PhysicsBoss.worldEffect.Parameters["extractThreshold"].SetValue(bloomThreshold);
            PhysicsBoss.worldEffect.CurrentTechnique.Passes["Extract"].Apply();
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            for (int i = 0; i < 3; i++)
            {
                graphicsDevice.SetRenderTarget(screenTemp);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(bloomInten);
                PhysicsBoss.worldEffect.Parameters["blurThreshold"].SetValue(0);
                PhysicsBoss.worldEffect.Parameters["targetRes"].SetValue(screenTemp.Size());
                PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurThresholdH"].Apply();
                spriteBatch.Draw(Main.screenTargetSwap,
                    new Rectangle(0, 0, screenTemp.Width, screenTemp.Height), Color.White);
                spriteBatch.End();

                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(bloomInten);
                PhysicsBoss.worldEffect.Parameters["blurThreshold"].SetValue(0);
                PhysicsBoss.worldEffect.Parameters["targetRes"].SetValue(Main.screenTargetSwap.Size());
                PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurThresholdV"].Apply();
                spriteBatch.Draw(screenTemp,
                    new Rectangle(0, 0, Main.screenTargetSwap.Width, Main.screenTargetSwap.Height), Color.White);
                spriteBatch.End();
            }

            graphicsDevice.SetRenderTarget(screenTemp);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            spriteBatch.Draw(Main.screenTargetSwap,
                new Rectangle(0, 0, screenTemp.Width, screenTemp.Height), Color.White);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.Draw(screenTemp,
                new Rectangle(0, 0, Main.screenTarget.Width, Main.screenTarget.Height), Color.White);
            spriteBatch.End();

            if (bloomInten > 0)
                bloomInten -= 0.1f;
        }

        public static void flash(float threshold, Vector2 centerPos, int timeLeft, float intensity) {
            flashThreshold = threshold;
            flashTimeLeft = timeLeft;
            flashCenterPoint = centerPos;
            dispIntensity = intensity;
        }

        public static void Main_applyFlash() {
            if (flashTimeLeft <= 0)
            {
                realCenterPoint = Vector2.Zero;
                return;
            }

            shake(Math.Max(0,(float)(flashTimeLeft - 1)/30f));

            if (realCenterPoint == Vector2.Zero || flashTimeLeft % (int)(FLASH_PERIOD/2) == 0) {
                realCenterPoint = flashCenterPoint + Main.rand.NextVector2Circular(450f,300f);
            }

            Texture2D noise = ModContent.Request<Texture2D>("PhysicsBoss/Asset/PerlinNoise").Value;
            //Texture2D noise = ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNNormal").Value;
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            //RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width / 2, Main.screenTarget.Height / 2);
            RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width, Main.screenTarget.Height);
            SpriteBatch spriteBatch = Main.spriteBatch;

            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            PhysicsBoss.worldEffect.Parameters["extractThreshold"].SetValue(flashThreshold);
            PhysicsBoss.worldEffect.Parameters["fillColor"].SetValue(Color.White.ToVector4());
            PhysicsBoss.worldEffect.CurrentTechnique.Passes["FillOnThreshold"].Apply();
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);

            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle((int)realCenterPoint.X - FLASH_WIDTH/2, 0, FLASH_WIDTH, Main.screenTargetSwap.Height), Color.White);
            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0,(int)realCenterPoint.Y - FLASH_WIDTH / 2, Main.screenTargetSwap.Width, FLASH_WIDTH), Color.White);
            spriteBatch.Draw(ModContent.Request<Texture2D>("PhysicsBoss/Asset/Circle").Value,
                new Rectangle((int)(realCenterPoint.X - 150), (int)(realCenterPoint.Y - 150),300, 300), Color.White);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(screenTemp);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            PhysicsBoss.worldEffect.Parameters["dispCenter"].SetValue(realCenterPoint);
            PhysicsBoss.worldEffect.Parameters["dispInten"].SetValue(dispIntensity);
            PhysicsBoss.worldEffect.Parameters["dispMap"].SetValue(noise);
            PhysicsBoss.worldEffect.Parameters["dispTimer"].SetValue((int)(flashTimeLeft/(int)(FLASH_PERIOD/2)) * 0.01f);
            PhysicsBoss.worldEffect.Parameters["texSize"].SetValue(screenTemp.Size());
            PhysicsBoss.worldEffect.CurrentTechnique.Passes["CenterDisplacement"].Apply();
            spriteBatch.Draw(Main.screenTargetSwap,
                new Rectangle(0, 0, screenTemp.Width, screenTemp.Height), Color.White);

            spriteBatch.End();

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear((flashTimeLeft % FLASH_PERIOD < (FLASH_PERIOD / 2) ? Color.White : Color.Black) * FLASH_TRANSPARENCY);
            //graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            
            if (flashTimeLeft % FLASH_PERIOD < (FLASH_PERIOD/2)) {
                PhysicsBoss.worldEffect.CurrentTechnique.Passes["Inverse"].Apply();
            }
            
            spriteBatch.Draw(screenTemp,
                new Rectangle(0, 0, Main.screenTarget.Width, Main.screenTarget.Height), Color.White * FLASH_TRANSPARENCY);
            spriteBatch.End();


            if (flashTimeLeft > 0)
                flashTimeLeft--;
            
        }

        public static void drawSpecialDusts() {
            if (BlockDust.count > 0 || TwistedDust.count > 0)
            {
                GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
                RenderTarget2D screenTemp = null;
                RenderTarget2D screenTemp2 = null;

                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Transparent);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                Main.spriteBatch.End();

                if (BlockDust.count > 0)
                {
                    screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width/SHRINK_CONST, Main.screenTarget.Height / SHRINK_CONST);
                    graphicsDevice.SetRenderTarget(screenTemp);
                    graphicsDevice.Clear(Color.Transparent);
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    BlockDust.drawAll(Main.spriteBatch, SHRINK_CONST);
                    Main.spriteBatch.End();
                }

                if (TwistedDust.count > 0) {
                    screenTemp2 = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width / SHRINK_CONST, Main.screenTarget.Height / SHRINK_CONST);
                    graphicsDevice.SetRenderTarget(screenTemp2);
                    graphicsDevice.Clear(Color.Transparent);
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    TwistedDust.drawAll(Main.spriteBatch, SHRINK_CONST);
                    Main.spriteBatch.End();
                }

                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Transparent);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                if (BlockDust.count > 0)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                    PhysicsBoss.maskEffect.Parameters["threshold"].SetValue(0.99f);
                    PhysicsBoss.maskEffect.Parameters["texContent"].SetValue(PhysicsBoss.galaxyTex);
                    PhysicsBoss.maskEffect.Parameters["ordinaryTint"].SetValue(Color.LightBlue.ToVector4());
                    PhysicsBoss.maskEffect.Parameters["contentTint"].SetValue(Color.White.ToVector4());
                    PhysicsBoss.maskEffect.CurrentTechnique.Passes["MaskTint"].Apply();
                    Main.spriteBatch.Draw(screenTemp, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
                }

                if (TwistedDust.count > 0) {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                    PhysicsBoss.maskEffect.Parameters["threshold"].SetValue(0.8f);
                    PhysicsBoss.maskEffect.Parameters["texContent"].SetValue(PhysicsBoss.fractalTex);
                    PhysicsBoss.maskEffect.Parameters["ordinaryTint"].SetValue(Color.Purple.ToVector4());
                    PhysicsBoss.maskEffect.Parameters["contentTint"].SetValue(Color.White.ToVector4());
                    PhysicsBoss.maskEffect.CurrentTechnique.Passes["MaskTint"].Apply();
                    Main.spriteBatch.Draw(screenTemp2, new Rectangle(0,0,Main.screenWidth, Main.screenHeight), Color.White);
                }

                Main.spriteBatch.End();
            }
        }
    }
}
