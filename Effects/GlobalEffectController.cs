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

namespace PhysicsBoss.Effects
{
    public static class GlobalEffectController
    {
        public static Texture2D beamTex  = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Beam").Value;
        public static Texture2D ColorGradient  = ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/ColorGradient").Value;
        public static Texture2D circleTex  = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Circle").Value;
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
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            

            Vector2 pos = center - Main.screenPosition - radius * Vector2.One;

            spriteBatch.Draw(ModContent.Request<Texture2D>("PhysicsBoss/Asset/Circle").Value,
                new Rectangle((int)(pos.X), (int)pos.Y,
                (int)(2* radius), (int)(2 * radius)), null, c);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public static void shake(float intensity) {
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

        public static void centerTwist(float intensity, float radius, float width, Vector2 Center) {
            if (intensity <= 0 && Filters.Scene["PhysicsBoss:CenterTwist"].IsActive()) {
                Filters.Scene.Deactivate("PhysicsBoss:CenterTwist");
                return;
            }

            if (intensity > 0 && !Filters.Scene["PhysicsBoss:CenterTwist"].IsActive())
            {
                Filters.Scene.Activate("PhysicsBoss:CenterTwist");
            }

            PhysicsBoss.worldEffect.Parameters["twistInten"].SetValue(intensity);
            PhysicsBoss.worldEffect.Parameters["twistRadius"].SetValue(radius);
            PhysicsBoss.worldEffect.Parameters["twistWidth"].SetValue(width);
            PhysicsBoss.worldEffect.Parameters["twistCenter"].SetValue(Center - Main.screenPosition);
            PhysicsBoss.worldEffect.Parameters["texSize"].SetValue(Main.ScreenSize.ToVector2());
        }

        public static void bloom(float intensity, float threashold) {
            /*
            if (intensity < 0 && Filters.Scene["PhysicsBoss:Bloom"].IsActive())
            {
                Filters.Scene.Deactivate("PhysicsBoss:Bloom");
                return;
            }

            if (!Filters.Scene["PhysicsBoss:Bloom"].IsActive())
            {
                Filters.Scene.Activate("PhysicsBoss:Bloom");
            }

            PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(intensity);
            PhysicsBoss.worldEffect.Parameters["blurThreshold"].SetValue(threashold);
            */

            if (intensity < 0)
                return;

            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width/2, Main.screenTarget.Height/2);
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();

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

                PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(intensity * 0.25f);
                PhysicsBoss.worldEffect.Parameters["blurThreshold"].SetValue(threashold);

                //PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurOnThreshold"].Apply();
                PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurThresholdH"].Apply();

                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(intensity * 0.25f);
                PhysicsBoss.worldEffect.Parameters["blurThreshold"].SetValue(threashold);

                PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurThresholdV"].Apply();

                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            */
            #endregion


            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            spriteBatch.Draw(Main.screenTarget,Vector2.Zero, Color.White);
            spriteBatch.End();

            for (int i = 0; i < 3; i++)
            {
                graphicsDevice.SetRenderTarget(screenTemp);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(intensity);
                PhysicsBoss.worldEffect.Parameters["blurThreshold"].SetValue(threashold);
                PhysicsBoss.worldEffect.Parameters["targetRes"].SetValue(screenTemp.Size());
                PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurThresholdH"].Apply();
                spriteBatch.Draw(Main.screenTargetSwap, 
                    new Rectangle(0,0, screenTemp.Width, screenTemp.Height), Color.White);
                spriteBatch.End();

                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(intensity);
                PhysicsBoss.worldEffect.Parameters["blurThreshold"].SetValue(threashold);
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

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
