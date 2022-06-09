using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.NPCs.Boss.ChaosTheory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles.ThreeBodyMotion
{
    public class SolarLaser : ModProjectile
    {
        public const float LENGTH = 2500;
        public const float TRANSIT = 10;
        public const float WIDTH = 95;
        private VertexStrip tail = new VertexStrip();
        public const int TRAILING_CONST = 30;

        private float prog;
        private Color drawColor = Color.Yellow;

        protected Texture2D tex;
        protected Texture2D coverTex;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Solar Laser");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "太阳激光");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(10 * 60);
            Projectile.damage = 100;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            coverTex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/ThreeBodyMotion/SolarLaserCover").Value;

            Projectile.width = tex.Width;
            Projectile.height = (int)WIDTH;
            Projectile.rotation = 0f;

            Timer = 0;

            prog = 0.0f;

            Projectile.oldPos = new Vector2[TRAILING_CONST];
            Projectile.oldRot = new float[TRAILING_CONST];
        }

        public override void AI()
        {
            prog = Math.Min(Math.Min(Timer / TRANSIT, Projectile.timeLeft / TRANSIT), 1f);
            Projectile.velocity *= 0;

            if ((int)Timer == 0)
                SoundEngine.PlaySound(SoundID.Item33);


            if (Timer < TRANSIT)
            {
                GlobalEffectController.blur(1f * (1 - (Timer / TRANSIT)));
                GlobalEffectController.shake(12f * (1 - (Timer / TRANSIT)));
            }

            GlobalEffectController.bloom(Math.Max(2f * (1 - (Timer / (3 * TRANSIT))), 0), 0.3f);

            if (Timer < TRANSIT || Timer % 10 == 0)
            {
                for (int j = 0; j < 50; j++)
                {
                    Vector2 pos = (Vector2.UnitX * ((float)j + Main.rand.NextFloat() - 0.5f) /
                        50 * LENGTH).RotatedBy(Projectile.rotation) + Projectile.Center;
                    for (int i = 0; i < 2; i++)
                    {
                        Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.SolarFlare);
                        d.color = drawColor;
                        d.noGravity = true;
                        d.velocity = ((float)j+1f)/50f * 35f * (Projectile.rotation +
                            (i % 2 == 0 ? 1 : -1) * MathHelper.PiOver2).ToRotationVector2();
                    }
                }
            }

            if ((int)Timer % 30 == 0) {
                SoundEngine.PlaySound(SoundID.Item34);
            }

            update();

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            update();

            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = tex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            PhysicsBoss.trailingEffect.Parameters["tailStart"].SetValue(3 * Color.White.ToVector4());
            PhysicsBoss.trailingEffect.Parameters["tailEnd"].SetValue(3 * Color.Yellow.ToVector4());
            PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue(-(float)Main.time * 0.03f);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["DynamicTrailSimple"].Apply();

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => Color.White,
                progress => WIDTH * prog * (float)Math.Sqrt(progress) + 10f,
                -Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();


            PhysicsBoss.maskEffect.Parameters["timer"].SetValue((float)Main.time * 0.03f);
            PhysicsBoss.maskEffect.Parameters["tint"].SetValue(Color.Orange.ToVector4() * 0.8f);
            PhysicsBoss.maskEffect.Parameters["fadeThreshold"].SetValue(0.1f);
            PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicColorTintVFade"].Apply();

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => Color.White,
                progress => 1.15f * WIDTH * prog * (float)Math.Sqrt(progress) + 10f,
                -Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;

            for (int i = 0; i < TRAILING_CONST - 1; i++) {
                float width = (float)Math.Sqrt((float)i / (float)TRAILING_CONST);

                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   Projectile.oldPos[i], Projectile.oldPos[i + 1],
                   width * prog, ref point))
                    return true;
            }

            return false;
        }


        public void setColor(Color c)
        {
            drawColor = c;
        }

        private void update()
        {
            for (int i = 0; i < TRAILING_CONST; i++)
            {
                Projectile.oldRot[i] = Projectile.rotation;

                float factor = (float)i / ((float)TRAILING_CONST);

                Vector2 horizontalDisp = new Vector2(factor * LENGTH,0);

                Projectile.oldPos[i] = Projectile.Center + horizontalDisp.RotatedBy(Projectile.rotation);
            }
        }
    }
}
