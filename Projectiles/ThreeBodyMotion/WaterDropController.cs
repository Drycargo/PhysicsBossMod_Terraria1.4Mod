using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles.ThreeBodyMotion
{
    public class WaterDropController
    {
        public const int TOTAL = 10;
        public const float RADIUS = 1200;

        private float angle;
        private WaterDrop[] waterDrops;
        private int count;
        private Vector2 center;

        public WaterDropController(Vector2 c) {
            angle = 0;
            waterDrops = new WaterDrop[TOTAL];
            count = 0;
            center = c;
        }

        public void summonAll() {
            for (int i = 0; i < TOTAL; i++)
                summonWaterDrop();
        }

        private void summonWaterDrop() {
            if (count >= TOTAL)
                return;
            waterDrops[count] = (WaterDrop)(Projectile.NewProjectileDirect(
                null, center + (angle + (float)count / (float)TOTAL * MathHelper.TwoPi).ToRotationVector2() * RADIUS,
                Vector2.Zero, ModContent.ProjectileType<WaterDrop>(), 150, 10).ModProjectile);
            waterDrops[count].Projectile.rotation = getRotationAngle(count);
            count++;
        }

        private float getRotationAngle(int index) {
            return angle + (float)index / (float)TOTAL * MathHelper.TwoPi - MathHelper.Pi / 5;
        }
    }
}
