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

namespace PhysicsBoss.Projectiles.DoublePendulum
{
    public class FractalRing:ModProjectile
    {
        public const int TRAILING_CONST = 45;
        public const int RADIUS = 1000;
        public const int TRANSIT = 60;

        private Texture2D tex;
        protected VertexStrip tail = new VertexStrip();
        private float prog;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fractal Ring");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "杂色环");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(100 * 60);
            Projectile.damage = 100;

            tex = ModContent.Request<Texture2D>(Texture).Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            prog = 0;

            createRing();
        }

        private void createRing()
        {
            Projectile.oldPos = new Vector2[TRAILING_CONST];
            Projectile.oldRot = new float[TRAILING_CONST];
            for (int i = 0; i < TRAILING_CONST; i++)
            {
                Projectile.oldRot[i] = (float)i / (float)(TRAILING_CONST - 1) * MathHelper.TwoPi + MathHelper.PiOver2;
                Projectile.oldPos[i] = Vector2.Zero;
            }
        }

        private void updateRing()
        {
            for (int i = 0; i < TRAILING_CONST; i++)
            {
                Projectile.oldPos[i] = prog * RADIUS * (Projectile.oldRot[i] - MathHelper.PiOver2).ToRotationVector2();
            }
        }

        public override void AI()
        {
            Projectile.velocity *= 0;
            if (prog <= 1)
            {
                prog += 1f / (float)TRANSIT;
                updateRing();
            }
            else if (Projectile.timeLeft < TRANSIT && prog > 0) {
                prog -= 1f / (float)TRANSIT;
                if (prog < 0)
                    prog = 0;
                updateRing();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {

            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            RenderTarget2D screenTemp = new RenderTarget2D(graphicsDevice, Main.screenWidth, Main.screenHeight);
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(screenTemp);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);

            #region drawtail
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);

            drawRing();

            #endregion
            Main.spriteBatch.End();

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            PhysicsBoss.maskEffect.Parameters["timer"].SetValue((float)(Main.time * 0.005));
            PhysicsBoss.maskEffect.Parameters["threshold"].SetValue(prog);
            PhysicsBoss.maskEffect.Parameters["texMask"].SetValue(Main.screenTargetSwap);
            PhysicsBoss.maskEffect.Parameters["texContent"].SetValue(
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNBlock").Value);

            
            Main.graphics.GraphicsDevice.Textures[0] = screenTemp;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            

            PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicMask"].Apply();
            spriteBatch.Draw(screenTemp, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void drawRing()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = tex;

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => Color.White * prog, progress => Projectile.width * prog,
                Projectile.Center - Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (prog < 1)
                return false;
            float dist = Vector2.Distance(targetHitbox.Center.ToVector2(), Projectile.Center);
            return dist > RADIUS - Projectile.width * 0.7 && dist < RADIUS + Projectile.width * 0.7;
        }
    }
}
