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
                targetPos += 1500f * (targetPos - center).SafeNormalize(Vector2.UnitX);
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
    }
}
