using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class AizawaController:TrailingStarController
    {
        public const int TRAILING_CONST = 15;
        public const float ACC_PERIOD = 5f;

        private Vector2 dir;
        private VertexStrip tail = new VertexStrip();
        private Texture2D tex;
        private bool activated;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aizawa Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "Aizawa托尾星控制");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.damage = 30;
            dir = Vector2.Zero;

            Projectile.oldPos = new Vector2[TRAILING_CONST];
            Projectile.oldRot = new float[TRAILING_CONST];

            for (int i = 0; i < TRAILING_CONST; i++) {
                Projectile.oldPos[i] = Vector2.Zero;
                Projectile.rotation = -MathHelper.PiOver2;
            }

            tex = ModContent.Request<Texture2D>(Texture).Value;
            activated = false;
            Projectile.timeLeft = (int)(3 * 60);
        }

        public override void AI()
        {
            if ((int)Timer == 0)
                Projectile.velocity.Y = -15;
            if (!activated)
            {
                if (Projectile.velocity.Y >=0)
                {
                    activated = true;
                    summonStarBundle<TrailingStarAizawa>();
                }
            }
            else {
                Projectile.velocity.X = 20*(float)Math.Sin(Timer/MathHelper.TwoPi * 1.5f);
            }
            Projectile.velocity.Y += 0.4f;

            // update old pos & rot
            if ((int)Timer % 3 == 0) {
                for (int i = TRAILING_CONST - 1; i > 0; i--)
                {
                    Projectile.oldPos[i] = Projectile.oldPos[i-1];
                    Projectile.oldRot[i] = Projectile.oldRot[i-1];
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.oldPos[0] = Projectile.position;
            Projectile.oldRot[0] = Projectile.rotation;

            Timer++;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/Smoke").Value;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            PhysicsBoss.trailingEffect.Parameters["tailStart"].SetValue(3 * Color.White.ToVector4());
            PhysicsBoss.trailingEffect.Parameters["tailEnd"].SetValue(2 * Color.Purple.ToVector4());
            PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue((float)Main.time * 0.01f);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["DynamicTrailSimple"].Apply();

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => Color.White,
                progress => Projectile.width * 0.15f * (1-progress),
                tex.Size() / 2 - Main.screenPosition, TRAILING_CONST);

            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value,
                Projectile.position - Main.screenPosition, Color.Lerp(Color.Purple, Color.White, 0.8f) * (activated ? 1:0.5f));

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!activated)
                return false;
            return base.Colliding(projHitbox, targetHitbox);
        }

        public void release(Vector2 vel) {
            activated = true;
            dir = vel.SafeNormalize(Vector2.UnitX);
        }
    }
}
