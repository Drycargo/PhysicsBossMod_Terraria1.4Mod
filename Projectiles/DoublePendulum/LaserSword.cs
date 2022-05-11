using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.Projectiles.TrailingStarMotion;
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

namespace PhysicsBoss.Projectiles.DoublePendulum
{
    public class LaserSword:ModProjectile
    {
        private VertexStrip tail = new VertexStrip();
        public const int TRAILING_CONST = 15;
        public const int CIRCLE_TRAILING_CONST = 10;
        public const int CIRCLE_RADIUS = 70;
        public const int CIRCLE_DURATION = 30;
        public const int TRANSIT = 45;
        private const float ACC = 1.2f;
        private Texture2D backTex;
        
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        private Texture2D tex;

        private Vector2 dir;
        private Color drawColor;

        private Vector2[] circlePos;
        private float[] circleRot;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Laser Sword");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "激光刃");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(3.75 * 60);
            Projectile.damage = 25;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = 0;

            Projectile.width = tex.Width/2;
            Projectile.height = tex.Height/2;

            backTex =
                ModContent.Request<Texture2D>("PhysicsBoss/Asset/White").Value;

            drawColor = Color.Red;

            circlePos = new Vector2[CIRCLE_TRAILING_CONST];
            circleRot = new float[CIRCLE_TRAILING_CONST];
        }

        public override void AI()
        {
            if (Timer == 0)
            {
                dir = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.rotation = dir.ToRotation();
                Projectile.velocity *= 0;
                for (int i = 0; i < CIRCLE_TRAILING_CONST; i++) {
                    circlePos[i] = CIRCLE_RADIUS * 0.5f * dir + Projectile.Center;
                    circleRot[i] = Projectile.rotation + MathHelper.PiOver2;
                }
            }
            else if (Timer > TRANSIT) {
                Projectile.velocity += ACC * dir;
            }

            if (Timer >= TRANSIT - CIRCLE_DURATION * 0.5
                && Timer <= TRANSIT + CIRCLE_DURATION * 0.5 + 45) {
                float factor = (Timer - (TRANSIT - CIRCLE_DURATION * 0.5f)) / (CIRCLE_DURATION);
                factor = (float)Math.Sqrt(factor);
                updateCircle(factor * 2.5f * MathHelper.TwoPi);
            }

            Timer++;

        }

        private void updateCircle(float angle)
        {
            for (int j = CIRCLE_TRAILING_CONST - 1; j > 0; j--) {
                circlePos[j] = circlePos[j - 1];
                circleRot[j] = circleRot[j - 1];
            }

            circleRot[0] = angle + dir.ToRotation() + MathHelper.PiOver2;
            circlePos[0] = CIRCLE_RADIUS * 0.5f * dir.RotatedBy(angle) + Projectile.Center ;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Timer < TRANSIT * 1.25) {
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center,
                    Projectile.Center + Projectile.rotation.ToRotationVector2(),
                    drawColor * 0.8f * (float)Math.Min(Math.Min(1, (TRANSIT * 1.25 - Timer)/((float)TRANSIT * 0.25)), 
                    Timer / ((float)TRANSIT * 0.25)), 8);
            }
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = backTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => drawColor * (float)Math.Sqrt(1f - progress),
                progress => Projectile.height * 0.15f,
                Projectile.Size / 2 - Projectile.width * 0.25f * Projectile.rotation.ToRotationVector2()
                - Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();

            if (Timer >= TRANSIT - CIRCLE_DURATION * 0.5
                && Timer <= TRANSIT + CIRCLE_DURATION * 0.5 + 45)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.Additive,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
                VertexStrip circleTail = new VertexStrip();
                Main.graphics.GraphicsDevice.Textures[0] = 
                    ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNMotion").Value;

                circleTail.PrepareStrip(circlePos, circleRot,
                    progress =>  Color.Lerp(drawColor, Color.White, 0.5f) * (1f - progress) * 2,
                    progress => 0.5f * CIRCLE_RADIUS,
                    - Main.screenPosition, TRAILING_CONST);
                circleTail.DrawTrail();
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, 
                drawColor * (Timer < TRANSIT ? 0.5f : 1), Projectile.rotation, Projectile.Size, 
                0.5f, SpriteEffects.None, 0);

            //Lighting.AddLight(Projectile.Center, Color.LightGreen.ToVector3());
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Timer < TRANSIT)
                return false;
            return base.Colliding(projHitbox, targetHitbox);
        }

        public void setColor(Color c) {
            drawColor = c;
        }
    }
}
