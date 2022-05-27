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

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarHalvorsen : TrailingStarChaotic
    {
        public static Color[] colors = {Color.Red, Color.DarkOrange, Color.Goldenrod, Color.Crimson};
        public override float SPEED_LIMIT => 65 * 2f;

        public override float SHRINK_CONST => 100f;

        public virtual float DECCELERATE => 0.675f;

        public override float STEP => 3;

        public const float AIM_TIME = 100;
        public const float PREPARE_TIME = 40;

        private bool stopDec;
        protected bool drawRayLine;
        private float lastDir;

        public override Matrix Transform =>
            Matrix.CreateTranslation(5f, 4f, 0) * Matrix.CreateScale(0.15f);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Halvorsen");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-Halvorsen吸引子");
            base.SetStaticDefaults();
        }

        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            Projectile.timeLeft = 8 * 60;
            drawColor = Color.Crimson * 2;
            stopDec = false;
            drawRayLine = false;
            lastDir = -100;
            Projectile.rotation = 0;
        }

        public override void AI()
        {
            base.AI();

            if (Projectile.timeLeft < 30)
                drawColor *= 0.9f;
            Projectile.rotation += 0.05f;
        }

        protected override void motionUpdate()
        {
            float a = 1.89f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                -a*x - 4*y - 4*z - y*y,
                -a*y - 4*z - 4*x - z*z,
                -a*z - 4*x - 4*y - x*x)/60;

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT)
            {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;

            for (int i = 0; i < 3; i++) {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.CrimsonTorch);
                d.noGravity = true;
                d.velocity = 5 * Main.rand.NextVector2Unit();
            }

        }

        protected override void releaseAction()
        {
            decelerate();

            aimLaser();
            Timer++;
        }

        protected void decelerate()
        {
            if (!stopDec && Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = 0.5f * SPEED_LIMIT *
                    (Projectile.position - Projectile.oldPos[0]).SafeNormalize(Main.rand.NextVector2Unit());
                Timer = 0;
            }
            else if (!stopDec)
            {
                if (Projectile.velocity.Length() <= 1.5f)
                {
                    Projectile.velocity *= 0;
                    stopDec = true;
                }
                else
                {
                    //Vector2 dec = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    Projectile.velocity *= DECCELERATE;
                }
            }
        }

        public override void PostDraw(Color lightColor)
        {
            if (drawRayLine && target != null && target.active && lastDir > -99)
            {
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center,
                    Projectile.Center + lastDir.ToRotationVector2(),
                    drawColor * 0.8f * Math.Min((PREPARE_TIME + AIM_TIME - Timer) / PREPARE_TIME, Math.Min(1, Timer / (AIM_TIME / 2))), 10);
            }

            specialDraw(lightColor);

        }

        protected void specialDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            PhysicsBoss.shineEffect.Parameters["shineColor"].SetValue(drawColor.ToVector4());
            PhysicsBoss.shineEffect.Parameters["texSize"].SetValue(tex.Size());
            PhysicsBoss.shineEffect.CurrentTechnique.Passes["Contour"].Apply();

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                null, lightColor, Projectile.rotation, tex.Size() / 2, 1f, SpriteEffects.None, 0);

            lightColor.A = drawColor.A;

            Main.spriteBatch.Draw(ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/TrailingStarMotion/TrailingStarHalvorsenMask").Value,
                Projectile.Center - Main.screenPosition, null, drawColor * 3.5f, Projectile.rotation, tex.Size() / 2, 0.6f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        protected virtual void aimLaser()
        {
            if (target == null || !target.active)
            {
                float minDist = 2000f;
                foreach (var player in Main.player)
                {
                    if (player.active && Vector2.Distance(player.Center, Projectile.Center) < minDist)
                    {
                        minDist = Vector2.Distance(player.Center, Projectile.Center);
                        target = player;
                    }
                }
            }

            if (target == null || !target.active)
                return;

            if (Timer < AIM_TIME)
            {
                drawRayLine = true;
                if (Timer < 0.5 * AIM_TIME)
                lastDir = (target.Center - Projectile.Center).ToRotation();
            }
            else if ((int)Timer == (int)(AIM_TIME + PREPARE_TIME))
            {
                drawRayLine = false;
                // if direction is initialized
                if (lastDir > -99)
                {
                    BlockFractalLaser p = (BlockFractalLaser)Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(),
                        Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlockFractalLaser>(),
                        100, 0).ModProjectile;
                    p.Projectile.rotation = lastDir;
                    p.setColor(drawColor);

                    Projectile.timeLeft = p.Projectile.timeLeft;
                }
                else {
                    Projectile.Kill();
                }
            }

        }

        public override void setColor(int colorIndex)
        {
            drawColor = colors[colorIndex % colors.Length] * 2;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }

        public override void Kill(int timeLeft)
        {
            //base.Kill(timeLeft);
        }
    }
}
