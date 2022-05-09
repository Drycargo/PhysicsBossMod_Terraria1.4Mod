using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.NPC.Boss.ChaosTheory;
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

namespace PhysicsBoss.Projectiles
{
    public class SinLaser:ModProjectile
    {
        private VertexStrip tail = new VertexStrip();
        public const int TRAILING_CONST = 30;
        public const float AMPLITUDE = 30;
        public const float LENGTH = 2000;
        public const float TRANSIT = 30;
        public const float WIDTH = 10;

        private Vector2[] components;
        private float[] rotations;
        private int reverse;
        private float prog;

        protected Texture2D tex;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sin Laser");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "正弦激光");
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
            Projectile.rotation = 0f;
            components = new Vector2[TRAILING_CONST];
            rotations = new float[TRAILING_CONST];

            Timer = 0;
            reverse = 1;

            prog = 0.0f;
        }

        public override void AI()
        {
            prog = Math.Min(Math.Min(Timer / TRANSIT, Projectile.timeLeft / TRANSIT), 1f);
            Projectile.velocity *= 0;
            if (Timer == 0)
                update();
            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            update();

            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            PhysicsBoss.shineEffect.Parameters["shineColor"].SetValue(prog * 2f * Color.Red.ToVector4());
            PhysicsBoss.shineEffect.Parameters["threashold"].SetValue(0.6f);
            PhysicsBoss.shineEffect.Parameters["timer"].SetValue(-(float)Timer * 20);
            PhysicsBoss.shineEffect.Parameters["texSize"].SetValue(tex.Size());
            PhysicsBoss.shineEffect.Parameters["tex0"].SetValue(
                ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LightningBoltAdvanceTexture").Value);
            PhysicsBoss.shineEffect.CurrentTechnique.Passes["DynamicBeam"].Apply();

            Main.graphics.GraphicsDevice.Textures[0] = tex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            tail.PrepareStrip(components, rotations,
                progress => Color.White * 0.8f,
                progress=>WIDTH * prog * (float)Math.Pow(progress, 0.1),
                - Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion

            return false;
        }

        private void update() {
            for (int i = 0; i < TRAILING_CONST; i++)
            {
                rotations[i] = Projectile.rotation;

                float factor = (float)i / ((float)TRAILING_CONST);

                Vector2 horizontalDisp = new Vector2(factor * LENGTH,
                    reverse * AMPLITUDE * (float)Math.Sin(4 * factor * MathHelper.TwoPi)
                    * (float)Math.Cos(Timer * MathHelper.TwoPi/600f));

                components[i] = Projectile.Center + horizontalDisp.RotatedBy(Projectile.rotation);
            }
        }

        public void reverseAmp() {
            reverse *= -1;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            for (int i = 0; i < TRAILING_CONST - 1; i++)
            {
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   components[i], components[i + 1], WIDTH/2 * prog, ref point))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
