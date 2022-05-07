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

        public const float AIM_TIME = 100;
        public const float PREPARE_TIME = 40;


        private bool stopDec;
        private bool drawRayLine;
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
            drawColor = Color.Crimson;
            stopDec = false;
            drawRayLine = false;
            lastDir = -100;
        }

        protected override void motionUpdate()
        {
            float a = 1.89f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                -a*x - 4*y - 4*z - y*y,
                -a*y - 4*z - 4*x - z*z,
                -a*z - 4*x - 4*y - x*x);

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

            base.motionUpdate();
        }

        protected override void releaseAction()
        {
            if (!stopDec && Projectile.velocity == Vector2.Zero) {
                Projectile.velocity = 0.5f * SPEED_LIMIT *
                    (Projectile.position - Projectile.oldPos[0]).SafeNormalize(Main.rand.NextVector2Unit());
                Timer = 0;
            } else if (!stopDec) {
                if (Projectile.velocity.Length() <= 1f)
                {
                    Projectile.velocity *= 0;
                    stopDec = true;
                }
                else {
                    //Vector2 dec = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    Projectile.velocity *= 0.8f;
                }
            }

            aimLaser();
            Timer++;
        }

        public override void PostDraw(Color lightColor)
        {
            base.PostDraw(lightColor);
            if (drawRayLine && target != null && target.active && lastDir > -99) {
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center,
                    Projectile.Center + lastDir.ToRotationVector2(), 
                    Color.Red * 0.8f * Math.Min((PREPARE_TIME + AIM_TIME - Timer)/PREPARE_TIME,Math.Min(1,Timer/(AIM_TIME / 2))), 10);
            }
                
        }

        private void aimLaser()
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
                }
            }

        }

        public override void setColor(int colorIndex)
        {
            drawColor = colors[colorIndex % colors.Length];
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
    }
}
