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
        public const float RADIUS = 800;

        private float angle;
        private WaterDrop[] waterDrops;
        private int count;
        private Vector2 center;

        public WaterDropController(Vector2 c, float angle) {
            this.angle = angle;
            waterDrops = new WaterDrop[TOTAL];
            count = 0;
            center = c;
        }

        public void summonAll(NPC owner) {
            for (int i = 0; i < TOTAL; i++)
                summonWaterDrop(owner);
        }

        public int getCount() {
            return count;
        }

        public void summonWaterDrop(NPC o) {
            if (count >= TOTAL)
                return;

            try
            {
                waterDrops[count] = (WaterDrop)(Projectile.NewProjectileDirect(
                    o.GetSource_FromAI(), getPosition(count) - getRotationAngle(count).ToRotationVector2() * 800f,
                    Vector2.Zero, ModContent.ProjectileType<WaterDrop>(), 80, 10).ModProjectile);
                waterDrops[count].Projectile.rotation = getRotationAngle(count);
                count++;
            }
            catch (IndexOutOfRangeException e)
            {

            }

        }

        public void updateAll(float newAngle) {
            angle = newAngle;
            for (int i = 0; i < count; i++) {
                updatePosition(i);
            }
        }

        private void updatePosition(int index) {
            if (index >= count)
                return;

            Vector2 targetPos = getPosition(index);
            if ((targetPos - waterDrops[index].Projectile.Center).Length() > 20f)
            {
                waterDrops[index].Projectile.Center = Vector2.Lerp(waterDrops[index].Projectile.Center,
                    targetPos, 0.085f);
            }
            else {
                waterDrops[index].Projectile.Center = targetPos;
            }

            waterDrops[index].Projectile.rotation = getRotationAngle(index);
            waterDrops[index].Projectile.timeLeft++;
        }

        private Vector2 getPosition(int index) {
            return center + getPositionalAngle(index).ToRotationVector2() * RADIUS;
        }

        private float getRotationAngle(int index) {
            return  getPositionalAngle(index)- MathHelper.Pi * 3f / 10f;
        }

        private float getPositionalAngle(int index) {
            return angle + (float)index / (float)TOTAL * MathHelper.TwoPi;
        }

        public void killAll() {
            for (int i = 0; i < count; i++)
            {
                waterDrops[i].Projectile.Kill();
            }
        }

        public Vector2 getCenter() {
            return center;
        }

        public void aimAll(Vector2 aimPos) {
            for (int i = 0; i < count; i++)
                waterDrops[i].setStateAim(aimPos);
        }

        public void launchAll()
        {
            for (int i = 0; i < count; i++)
                waterDrops[i].launch();
        }
    }
}
