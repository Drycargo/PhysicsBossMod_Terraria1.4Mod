using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
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

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarController: ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "托尾星控制");
            base.SetStaticDefaults();
        }
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(ChaosTheory.ELE_CHARGE_DURATION * 0.5);//(int)(8 * 60);
            Projectile.damage = 0;
            Projectile.velocity = Vector2.Zero;

            Projectile.width = 0;
            Projectile.height = 0;
            Timer = 0;
        }

        public override void AI()
        {
            Projectile.velocity *= 0;
            if (Timer == 0) {
                int id = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TrailingStarChua>(), 50, 0);
                TrailingStarChaotic tsc = (TrailingStarChaotic)Main.projectile[id].ModProjectile;
                tsc.setOwner(this);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
    }
}
