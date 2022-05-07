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
        public static void drawAimLine(SpriteBatch spriteBatch, Vector2 center, Vector2 targetPos, Color color, float width)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate,
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
            spriteBatch.Begin(SpriteSortMode.Immediate,
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

            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenTarget.Width/2, Main.screenTarget.Height/2);
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);

            PhysicsBoss.worldEffect.Parameters["bloomInten"].SetValue(intensity);
            PhysicsBoss.worldEffect.Parameters["blurThreshold"].SetValue(threashold);

            PhysicsBoss.worldEffect.CurrentTechnique.Passes["BlurOnThreshold"].Apply();

            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
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
