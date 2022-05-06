﻿using Microsoft.Xna.Framework;
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

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarChua: TrailingStarChaotic
    {
        protected bool stopAcc;
        public static Color[] colors = { Color.Green, Color.Cyan, Color.Blue, Color.DarkViolet};

        public override Matrix Transform =>
            Matrix.CreateScale(0.75f) 
            * Matrix.CreateTranslation(-30f, 0, 0)
            * Matrix.CreateRotationY(MathHelper.PiOver4)
            * Matrix.CreateRotationZ(Timer * 0.01f);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Chua");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-蔡氏电路");
            base.SetStaticDefaults();
        }

        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            Projectile.timeLeft = 8 * 60;
            stopAcc = false;
        }

        protected override void motionUpdate()
        {
            float a = 36, b = 3, c = 20, u = -15.15f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                a*(y - x),
                (1 - z) * x + c * y + u,
                x*y - b*z);

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT) {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;
            
            base.motionUpdate();
        }

        protected override void releaseAction()
        {
            if (Projectile.velocity == Vector2.Zero)
                Projectile.velocity = 0.6f * SPEED_LIMIT *
                    (Projectile.position - Projectile.oldPos[0]).SafeNormalize(Main.rand.NextVector2Unit());
            chase();
        }

        private void chase()
        {
            if (stopAcc)
            {
                return;
            }

            if (target == null)
            {
                float minDist = 2000f;
                foreach (var player in Main.player)
                {
                    if (player.active && Vector2.Distance(player.Center, Projectile.Center) < minDist)
                    {
                        minDist = Vector2.Distance(player.Center, Projectile.Center);
                        target = player;
                    }
                }
            }

            if (target == null)
                return;

            Vector2 disp = target.Center - Projectile.Center;
            if (Projectile.velocity.Length() >= SPEED_LIMIT * 0.8 || disp.Length() < 180f)
                stopAcc = true;
            else if (target.active)
            {
                Projectile.velocity = 0.8f * Projectile.velocity +
                    3f * disp.SafeNormalize(Vector2.UnitX);
            }
        }

        public override void setColor(int colorIndex)
        {
            drawColor = colors[colorIndex % colors.Length];
        }
    }
}
