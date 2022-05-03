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
    public class TrailingStarChaotic: TrailingStarPlain
    {
        public const float PERSPECTIVE_CONST = 0.01f;
        public const float SHINK_CONST = 500f;
        public const float SPEED_LIMIT = 5f;

        protected Vector3 realCenter;
        protected Vector3[] oldRealPos;
        protected ModProjectile controller;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Chaotic");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星混沌");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            setBasicDefaults();

            oldRealPos = new Vector3[TRAILING_CONST];

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            //Projectile.oldPos = new Vector2[TRAILING_CONST];
            for (int i = 0; i < TRAILING_CONST; i++) {
                oldRealPos[i] = Vector3.Zero;
            }

            controller = null;
            realCenter = Vector3.Zero;
        }

        public override void AI()
        {
            if (realCenter == Vector3.Zero) {
                if (controller != null && controller.Projectile.active) {
                    realCenter.X = (Projectile.Center.X - controller.Projectile.Center.X)/SHINK_CONST;
                    realCenter.Y = (Projectile.Center.Y - controller.Projectile.Center.Y)/SHINK_CONST;
                }
            }

            for (int i = TRAILING_CONST - 1; i > 0; i--)
            {
                oldRealPos[i] = oldRealPos[i - 1];
            }
            oldRealPos[0] = realCenter;

            for (int i = 0; i < TRAILING_CONST; i++)
            {
                Projectile.oldPos[i] = render(oldRealPos[i]) - tex.Size()/2;
            }

            Projectile.Center = render(realCenter);


            motionUpdate();

            Main.NewText(realCenter);
        }

        protected virtual void motionUpdate() {
            Projectile.velocity *= 0;
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
            }

            return new Vector2(
                origin.X + realCenter.X * (realCenter.Z * PERSPECTIVE_CONST + 1f) * SHINK_CONST,
                origin.Y + realCenter.Y * (realCenter.Z * PERSPECTIVE_CONST + 1f) * SHINK_CONST);
        }

        public void setOwner(ModProjectile owner) {
            controller = owner;
        }
    }
}
