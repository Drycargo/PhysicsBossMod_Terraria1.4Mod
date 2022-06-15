using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.ThreeBodyMotion
{
    public class SolarExplosion: ModProjectile
    {
        public const int FRAME_TOT = 7;
        private Texture2D tex;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Solar Explosion");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "太阳爆破");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 3 * FRAME_TOT;
            Projectile.damage = 50;

            tex = ModContent.Request<Texture2D>(Texture).Value;

            Projectile.frame = 0;
            Projectile.frameCounter = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height / FRAME_TOT;
        }

        public override void AI()
        {
            if (Projectile.frame == 2 && Projectile.frameCounter % 3 == 0) {
                for (int i = 0; i < 2; i++) {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center,
                        Main.rand.NextVector2Unit() * 8f, ModContent.ProjectileType<SolarFlame>(), 15,0);
                }
            }

            if ((Projectile.frameCounter++) % 3 == 0)
            {
                Projectile.frame++;
                Projectile.frame %= FRAME_TOT;
            }

            for (int i = 0; i < Projectile.timeLeft * 0.25f; i++)
                Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.SolarFlare).velocity = 
                    8 * Main.rand.NextVector2Unit();
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity *= 0;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(tex, new Rectangle((int)(Projectile.Center.X - 1.5f * Projectile.width - Main.screenPosition.X),
                (int)(Projectile.Center.Y - 1.5f * Projectile.height - Main.screenPosition.Y), 3 * Projectile.width, 3 * Projectile.height),
                new Rectangle(0, Projectile.frame * Projectile.height,
                Projectile.width, Projectile.height), Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Projectile.frame <= 5) {
                target.AddBuff(BuffID.OnFire, 5 * 60);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.frame <= 5) 
                return false;

            return Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center - 1.5f * Projectile.Size, Projectile.Size * 3);
        }
    }
}
