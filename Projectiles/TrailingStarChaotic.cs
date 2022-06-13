using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Projectiles.TrailingStarMotion;
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
    public class TrailingStarChaotic : TrailingStarPlain
    {
        public virtual float PERSPECTIVE_CONST{
            get { return 0.05f; } 
        }
        public virtual float SHRINK_CONST
        {
            get { return 8.5f; }
        }

        public virtual float SPEED_LIMIT
        {
            get { return 60f; }
        }

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public virtual float FOCAL
        {
            get { return 150f; }
        }

        public virtual float STEP => 1;

        public virtual Matrix Transform => Matrix.Identity;

        protected Vector3 realCenter;
        protected Vector3[] oldRealPos;
        protected TrailingStarController controller;
        protected bool released;

        protected Player target;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Chaotic");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星混沌");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            setBasicDefaults();

            oldRealPos = new Vector3[TRAILING_CONST];

            for (int i = 0; i < TRAILING_CONST; i++) {
                oldRealPos[i] = Vector3.Zero;
            }

            controller = null;
            realCenter = Vector3.Zero;
            released = false;

            target = null;
            Timer = 0;
        }

        public override void AI()
        {
            if (released)
            {
                releaseAction();
            }
            else
            {
                Projectile.timeLeft++;
                if ((int)Timer == 0)
                {
                    
                    if (controller != null && controller.Projectile.active)
                    {
                        realCenter.X = (Projectile.Center.X - controller.Projectile.Center.X) / SHRINK_CONST;
                        realCenter.Y = (Projectile.Center.Y - controller.Projectile.Center.Y) / SHRINK_CONST;
                    }
                    /*
                    realCenter.X = 50 / SHRINK_CONST * (Main.rand.NextFloat() - 0.5f);
                    realCenter.Y = 50 / SHRINK_CONST * (Main.rand.NextFloat() - 0.5f);*
                    */
                }

                for (int i = TRAILING_CONST - 1; i > 0; i--)
                {
                    oldRealPos[i] = oldRealPos[i - 1];
                }
                oldRealPos[0] = realCenter;

                Projectile.Center = render(realCenter);

                Projectile.velocity *= 0;

                for (int i = 0; i < STEP; i++)
                    motionUpdate();
            }
    
            Timer++;
        }

        protected virtual void releaseAction() { }

        public virtual void releaseProj(Player t) {
            if (!released) {
                released = true;
                target = t;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!released)
            {
                for (int i = 0; i < TRAILING_CONST; i++)
                {
                    Projectile.oldPos[i] = render(oldRealPos[i]) - Projectile.Size / 2;
                }
            }

            return base.PreDraw(ref lightColor);
        }

        protected virtual void motionUpdate() {
            Projectile.rotation = (Projectile.oldPos[1] - Projectile.oldPos[0]).ToRotation();
        }

        public Vector2 render(Vector3 pos) {
            Vector2 origin;

            if (controller == null || !controller.Projectile.active)
            {
                origin = Vector2.Zero;
            }
            else {
                origin = controller.Projectile.Center;
                //Transform = Matrix.CreateRotationZ((float)(controller.Timer * 0.025));
            }

            Matrix t = Transform;

            Vector3.Transform(ref pos, ref t, out pos);



            /*
            float factor = (float)Math.Atan(pos.Z) / (MathHelper.PiOver2);

            return new Vector2(
                origin.X + pos.X * (factor * PERSPECTIVE_CONST + 1f) * SHRINK_CONST,
                origin.Y + pos.Y * (factor * PERSPECTIVE_CONST + 1f) * SHRINK_CONST);
            */

            float factor = FOCAL;

            if (pos.Z >= FOCAL)
                factor /= 0.1f;
            else
                factor /= (FOCAL - pos.Z);
            return new Vector2(
                origin.X + pos.X * factor * SHRINK_CONST,
                origin.Y + pos.Y * factor * SHRINK_CONST);
        }

        public void setOwner(TrailingStarController owner) {
            controller = owner;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (released)
                Projectile.Kill();
        }
    }
}
