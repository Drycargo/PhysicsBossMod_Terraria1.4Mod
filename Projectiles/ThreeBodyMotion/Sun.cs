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
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles.ThreeBodyMotion
{
    public class Sun : ModProjectile
    {
        public const float SPEED_LIMIT = 45f;
        public const float DIST_LIMIT = 180f;
        public const float PERIOD = 1/ 0.0075f;

        public const int TRAILING_CONST = 30;

        private Texture2D tex;
        private Texture2D contentTex;
        private VertexStrip tail = new VertexStrip();

        private float mass;
        private Vector2 realPos;

        private Projectile laser;
        private float aimLineTransparency;
        private Vector2 target;

        private float bloomInten;

        public Vector2 RealPos {
            get { return realPos; }
            set { realPos = value; }
        }

        public float Mass {
            get { return mass; }
            set { mass = value; }
        }

        //private 
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sun");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "太阳");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(20 * 60);
            Projectile.damage = 50;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = 0;

            contentTex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/ThreeBodyMotion/SunContent").Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            mass = 1f + 0.5f * (Main.rand.NextFloat() - 0.5f);

            laser = null;
            aimLineTransparency = 0;
            target = new Vector2(-1, -1);

            bloomInten = 0;
        }

        public override void AI() {
            //Projectile.velocity *= 0;

            Projectile.rotation = Projectile.velocity.ToRotation();
            for (int i = 0; i < 1; i++) {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare);
            }

            if (target.X >= 0 && laser == null) {
                if (aimLineTransparency < 0.8f)
                    aimLineTransparency += 0.8f / 60f;
                if (aimLineTransparency >= 0.8f) {
                    if (laser == null)
                    {
                        laser = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                            ModContent.ProjectileType<SolarLaser>(), 45, 0);
                        aimLineTransparency = 0;
                    }
                }
            }

            if (laser != null)
            {
                laser.Center = Projectile.Center;
                laser.rotation = (target - Projectile.Center).ToRotation();
                laser.timeLeft++;
            }
            /*
            if (bloomInten > 0)
            {
                GlobalEffectController.bloom(bloomInten, 0.3f);
                bloomInten -= 1 / 20f;
                if (bloomInten <= 0) {
                    GlobalEffectController.bloom(-1, 0.3f);
                }
            }
            */

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3());
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            if (aimLineTransparency > 0 && target.X >= 0 && laser == null)
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center, target, 
                    Color.Yellow * aimLineTransparency, 50f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            Main.graphics.GraphicsDevice.Textures[0] =
                ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/ThickSmoke").Value;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            PhysicsBoss.trailingEffect.Parameters["tailStart"].SetValue(3 * Color.Red.ToVector4());
            PhysicsBoss.trailingEffect.Parameters["tailEnd"].SetValue(3 * Color.Yellow.ToVector4());
            PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue((float)Main.time * 0.01f);
            PhysicsBoss.trailingEffect.CurrentTechnique.Passes["DynamicTrailSimple"].Apply();

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                progress => Color.White,
                progress => Projectile.width * 0.3f * (1 - progress),
                tex.Size() / 2 - Main.screenPosition, TRAILING_CONST);

            tail.DrawTrail();

            PhysicsBoss.maskEffect.Parameters["texContent"].SetValue(contentTex);
            PhysicsBoss.maskEffect.Parameters["threshold"].SetValue(0.9f);
            PhysicsBoss.maskEffect.Parameters["ordinaryTint"].SetValue(2 * Color.Gold.ToVector4());
            PhysicsBoss.maskEffect.Parameters["contentTint"].SetValue(2 * Color.OrangeRed.ToVector4());
            PhysicsBoss.maskEffect.Parameters["timer"].SetValue(Timer / PERIOD);
            PhysicsBoss.maskEffect.CurrentTechnique.Passes["DynamicMaskTint"].Apply();

            Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, Color.White);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        public override void Kill(int timeLeft)
        {
            if (laser != null)
                laser.timeLeft = (int)SolarLaser.TRANSIT;
            base.Kill(timeLeft);
        }

        public void fireLaser(Vector2 targetPos) {
            target = targetPos;
        }

        public void fireSpike() {
            for (int i = 0; i < 8; i++) {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, ((float)i/8f * MathHelper.TwoPi).ToRotationVector2(),
                    ModContent.ProjectileType<SolarSpike>(), 25, 0);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2()) < tex.Width * 0.5f;
        }
    }
}
