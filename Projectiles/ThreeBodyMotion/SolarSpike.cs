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
    public class SolarSpike : ModProjectile
    {
        public const int TRANSIT = 45;
        public const int TRAILING_CONST = 10;
        public const float WIDTH = 35;

        private Texture2D tex;
        private VertexStrip tail = new VertexStrip();

        private float aimLineTransparency;
        private bool released;
        private Vector2 vel;

        //private 
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Solar Spike");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "太阳刺");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(0.5 * 60);
            Projectile.damage = 50;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            aimLineTransparency = 0;
            released = false;
            vel = Vector2.Zero;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (!released)
            {
                if (Timer == 0)
                {
                    vel = Projectile.velocity;
                    Projectile.velocity *= 0;
                }

                Projectile.timeLeft++;
                if (Timer >= TRANSIT)
                {
                    released = true;
                    aimLineTransparency = -1;
                }
                aimLineTransparency += 0.8f * Math.Min(1, 1f / (float)(TRANSIT * 0.6f));
            }
            else
            {
                if (Timer == TRANSIT + 1)
                {
                    Projectile.velocity = vel.SafeNormalize(Vector2.UnitX) * 100f;
                    SoundEngine.PlaySound(SoundID.Item74);
                }
                else
                    Projectile.velocity *= 0.95f;
                Dust.NewDust(Projectile.Center, 0,0,DustID.GoldFlame);
            }

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!released && aimLineTransparency > 0)
            {
                GlobalEffectController.drawRayLine(Main.spriteBatch, Projectile.Center, Projectile.Center + vel, Color.Orange * aimLineTransparency, 20f);
            }

            if (released)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                Main.graphics.GraphicsDevice.Textures[0] = tex;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

                PhysicsBoss.trailingEffect.Parameters["tailStart"].SetValue(3 * Color.White.ToVector4());
                PhysicsBoss.trailingEffect.Parameters["tailEnd"].SetValue(3 * Color.Orange.ToVector4());
                PhysicsBoss.trailingEffect.Parameters["uTime"].SetValue((float)Main.time * 0.01f);
                PhysicsBoss.trailingEffect.CurrentTechnique.Passes["DynamicTrailSimpleFade"].Apply();

                tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot,
                    progress => Color.White,
                    progress => WIDTH * progress,
                    tex.Size() / 2 - Main.screenPosition, TRAILING_CONST);

                tail.DrawTrail();
            }
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!released)
                return false;

            float point = 0;
            for (int i = 0; i < TRAILING_CONST - 1; i++)
            {
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   Projectile.oldPos[i] + tex.Size()/2, Projectile.oldPos[i + 1] + tex.Size() / 2,
                   WIDTH * (float)i / (float)TRAILING_CONST, ref point))
                    return true;
            }

            return false;
        }
    }
}