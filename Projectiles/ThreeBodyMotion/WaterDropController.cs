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
        public const float RADIUS = 850;
        public const float CHARGE_TOT = 90f;
        public const float CHARGE_PERIOD = 16f;
        public const float CHARGE_TIMES = 3;

        private float angle;
        private WaterDrop[] waterDrops;
        private int count;
        private Vector2 center;

        private bool aiming;
        private float aimProgress;

        public WaterDropController(Vector2 c, float angle) {
            this.angle = angle;
            waterDrops = new WaterDrop[TOTAL];
            count = 0;
            center = c;
            aiming = false;
            aimProgress = 0;
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
                    o == null? null: o.GetSource_FromAI(), getPosition(count) - getRotationAngle(count).ToRotationVector2() * 800f,
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

            if (aiming && aimProgress < 1f)
                aimProgress += 1f / CHARGE_TOT;
            for (int i = 0; i < count; i++) {
                updatePosition(i);
            }
        }

        private void updatePosition(int index) {
            if (index >= count)
                return;

            if (aiming)
            {

                if (aimProgress <= CHARGE_PERIOD / CHARGE_TOT * CHARGE_TIMES)
                {
                    float currProgress = (aimProgress % (CHARGE_PERIOD / CHARGE_TOT)) / (CHARGE_PERIOD / CHARGE_TOT);
                    int factor = (int)(aimProgress / (CHARGE_PERIOD / CHARGE_TOT));

                    Vector2 targetPos = getTargetPos(index, factor);

                    waterDrops[index].Projectile.Center = Vector2.Lerp(getTargetPos(index, factor - 1), targetPos, currProgress);

                    waterDrops[index].Projectile.rotation = (targetPos - waterDrops[index].Projectile.Center).ToRotation();
                }
                else {
                    float rotation = (float)index / (float)TOTAL * MathHelper.TwoPi + angle;
                    Vector2 disp = (650f + 450f * (aimProgress - CHARGE_PERIOD / CHARGE_TOT * CHARGE_TIMES) /
                        (1 - CHARGE_PERIOD / CHARGE_TOT * CHARGE_TIMES)) * rotation.ToRotationVector2();

                    waterDrops[index].Projectile.Center = disp + center;
                    waterDrops[index].Projectile.rotation = rotation + MathHelper.Pi;
                }
            }
            else
            {
                Vector2 targetPos = getPosition(index);
                if ((targetPos - waterDrops[index].Projectile.Center).Length() > 20f)
                {
                    waterDrops[index].Projectile.Center = Vector2.Lerp(waterDrops[index].Projectile.Center,
                        targetPos, 0.085f);
                }
                else
                {
                    waterDrops[index].Projectile.Center = targetPos;
                }

                waterDrops[index].Projectile.rotation = getRotationAngle(index);
                waterDrops[index].Projectile.timeLeft++;
            }
        }

        private Vector2 getTargetPos(int index, int factor)
        {
            return center +(Vector2.UnitX * 650f).RotatedBy((MathHelper.TwoPi * 3f/10f + angle) * factor
                  + (float)index / (float)TOTAL * MathHelper.TwoPi);
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
            if(!aiming)
                aiming = true;
            for (int i = 0; i < count; i++)
                waterDrops[i].setStateAim(aimPos);
            center = aimPos;
        }

        public void launchAll()
        {
            for (int i = 0; i < count; i++)
                waterDrops[i].launch();
        }
    }
}
