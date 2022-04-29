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
    public class ElectricCharge: ModProjectile
    {
        private float charge = 0.0f;
        private bool initialized = false;
        private bool activated = false;
        Texture2D tex;
        Texture2D texNeg;
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
            Projectile.damage = 30;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            texNeg = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/ElectricChargeNegative").Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;
        }

        public override void AI()
        {
            if (!initialized) {
                charge = 0.5f * Main.rand.NextFloat() + 0.75f;
                if (Main.rand.NextBool())
                {
                    charge *= -1;
                }
                initialized = true;
            }

            int dustId = (charge > 0f) ? DustID.FlameBurst: DustID.Clentaminator_Cyan;

            for (int i = 0; i < 5; i++) {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustId);
                d.velocity *= 0;
                d.noGravity = true;
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (charge > 0)
                Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, Color.White);
            else
                Main.spriteBatch.Draw(texNeg, Projectile.position - Main.screenPosition, Color.White);

            return false;
        }


        public override void Kill(int timeLeft)
        {
            int dustId = (charge > 0f) ? DustID.FlameBurst : DustID.Clentaminator_Cyan;
            for (int i = 0; i < 30; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, dustId);
                d.velocity = Main.rand.NextVector2Unit()*15f;
                d.noGravity = true;
            }

            base.Kill(timeLeft);
        }

        public float getCharge() { 
            return charge;
        }

        public void activate() {
            activated = true;
        }
    }
}
