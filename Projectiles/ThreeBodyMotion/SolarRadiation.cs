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
    public class SolarRadiation : ModProjectile
    {
        private Player target;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Solar Radiation");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "太阳射线");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;

            Projectile.timeLeft = (int)(0.75f * 60);
            Projectile.damage = 20;

            Projectile.width = 15;
            Projectile.height = 15;

            target = null;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = 0; i < 1; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.SolarFlare);
                d.velocity *= 0.01f;
                d.scale *= 2.5f;
                d.noGravity = true;
            }

            if (target != null) {
                Projectile.velocity += 0.3f * (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

                if (Projectile.velocity.Length() > 15f) {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= 15f;
                }
            }
        }

        public void setTarget(Player t) {
            target = t;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }


        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            Projectile.Kill();
            target.AddBuff(BuffID.OnFire3, 5 * 60);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = -1; i <= 1; i++)
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center,
                    (MathHelper.Pi/12 * (float)i + Projectile.rotation).ToRotationVector2() * 15f,
                    ModContent.ProjectileType<SolarFlame>(), 20, 0);
        }
    }
}
