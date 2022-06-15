using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace PhysicsBoss.NPCs
{
    public abstract class TargetEnemy: ModNPC {
        protected Player target;
        public static Player seekTarget(Vector2 myCenter, float minDist)
        {
            Player t = null;
            foreach (var player in Main.player)
            {
                if (player.active && !player.dead && Vector2.Distance(player.Center, myCenter) < minDist)
                {
                    minDist = Vector2.Distance(player.Center, myCenter);
                    t = player;
                }
            }
            return t;
        }

        public void setTarget(Player t) {
            target = t;
        }

        public Player getTarget()
        {
            return target;
        }

        public float targetDist() {
            if (target == null)
                return -1;
            return Vector2.Distance(target.Center, NPC.Center);
        }

        public float targetAngle() {
            if (target == null)
                return 0;
            float angle = (float)Math.Atan((target.Center.Y - NPC.Center.Y)/
                (target.Center.X - NPC.Center.X));
            if (target.Center.X < NPC.Center.X)
                angle += MathHelper.Pi;
            if (angle < 0)
                angle += MathHelper.TwoPi;
            return angle;
        }

        /*
        protected void hover(Vector2 hoverCenter, float hoverRadius, float noise, float period)
        {
            hover(hoverCenter, hoverRadius, noise, period, 10f,-1f, 0.7f);
        }
        */

        // If period is negative, rotate counter-clockwise
        protected void hover(Vector2 hoverCenter, float hoverRadius, float noise, float period, float safetyDistance = 10f, float traceDistance = -1f,float inertia = 0.7f)
        {
            float degree = (NPC.Center - hoverCenter).ToRotation() + MathHelper.TwoPi / period;

            Vector2 aim = degree.ToRotationVector2() * hoverRadius + hoverCenter
                + noise * 2 * (Main.rand.NextFloat() - 0.5f) * Vector2.One;

            if (aim.Distance(NPC.Center) > safetyDistance)
            {
                if (traceDistance >= 0 && aim.Distance(NPC.Center) > traceDistance)
                    NPC.Center = 0.8f * inertia * NPC.Center + (1f - inertia * 0.8f) * aim;
                else
                    NPC.Center = inertia * NPC.Center + (1f - inertia) * aim;
            } else
                NPC.Center = aim;

            NPC.rotation = (hoverCenter - NPC.Center).ToRotation();
        }

        protected void fireWork(Color c) {
            for (int i = 0; i < 30; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.RainbowRod);
                d.velocity = Main.rand.NextVector2Unit() * 7.5f;
                d.color = c;
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item4, NPC.Center);
        }

        protected void follow(float dist = 1000f, float speed = 5f) {
            if (target == null || !target.active)
                return;
            if ((target.Center - NPC.Center).Length() > dist)
            {
                NPC.Center = (NPC.Center - target.Center).SafeNormalize(Vector2.UnitX) * dist + target.Center;
            }
            else {
                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * speed;
            }
        }

        protected void ellipseSurround(float semiX, float semiY, float thisAngle, float ellipseAngle, float inertia = 0) {
            if (target != null && target.active) {
                inertia = Math.Min(1,Math.Max(0,inertia));
                Vector2 basicDisp = new Vector2((float)(semiX * Math.Cos(thisAngle)), (float)(semiY * Math.Sin(thisAngle)));
                NPC.Center = NPC.Center * inertia + (1 - inertia) * (target.Center + basicDisp.RotatedBy(ellipseAngle));
            }
        }
    }
}
