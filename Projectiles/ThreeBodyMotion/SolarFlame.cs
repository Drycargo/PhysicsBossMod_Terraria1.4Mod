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
    public class SolarFlame: ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Solar Flame");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "太阳火球");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;

            Projectile.timeLeft = (int)(1.5 * 60);
            Projectile.damage = 20;

            Projectile.width = 15;
            Projectile.height = 15;
        }

        public override void AI()
        {
            for (int i = 0; i < 5; i++) {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FlameBurst);
                d.velocity *= 0.5f;
                d.scale *= 2f;
                d.noGravity = true;
            }

            Projectile.velocity += 0.2f * Vector2.UnitY;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }


        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 5 * 60);
            Projectile.Kill();
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.FlameBurst).velocity *= 2f;
            }
        }
    }
}
