using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

namespace PhysicsBoss.Projectiles
{
    public class ElectricChargeController: ModProjectile
    {
        public const int CAPACITY = 16;
        public const int RADIUS = 250;
        public const float SE_CONST = 500f;

        private ElectricCharge[] charges;
        private bool initialized;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electric Charge Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "电荷控制");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(8 * 60);
            Projectile.damage = 0;
            Projectile.velocity = Vector2.Zero;

            Projectile.width = 0;
            Projectile.height = 0;

            charges = new ElectricCharge[CAPACITY];
            initialized = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void AI()
        {
            Projectile.velocity *= 0;
            if (!initialized)
            {
                for (int i = 0; i < CAPACITY; i++) {
                    int id = Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(),
                        Projectile.Center + RADIUS * (MathHelper.TwoPi * ((float) i)/((float)CAPACITY)).ToRotationVector2(),
                        Vector2.Zero, ModContent.ProjectileType<ElectricCharge>(), 30, 0);

                    charges[i] = (ElectricCharge)Main.projectile[id].ModProjectile;
                }
                initialized = true;
            }

            for (int i = 0; i < CAPACITY; i++)
            {
                Vector2 acc = Vector2.Zero;

                for (int j = 0; j < CAPACITY; j++) {
                    if (j != i) {
                        
                        acc += SE_CONST * (charges[i].getCharge() * charges[j].getCharge()) /
                            ((charges[i].Projectile.Center).DistanceSQ(charges[j].Projectile.Center))
                            * (charges[i].Projectile.Center - charges[j].Projectile.Center).SafeNormalize(Vector2.UnitX);
                        

                        //acc += (charges[i].getCharge() * charges[j].getCharge()) * (charges[i].Projectile.Center - charges[j].Projectile.Center).SafeNormalize(Vector2.UnitX);
                    }
                }

                charges[i].Projectile.velocity += acc;

                float speed = charges[i].Projectile.velocity.Length();
                if (speed > 25) {
                    charges[i].Projectile.velocity *= (25f / speed);
                }
            }

        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
    }
}
