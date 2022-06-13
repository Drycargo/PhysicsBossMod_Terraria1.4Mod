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
    public class LightningBoltAdvance : ModProjectile
    {
        public const int TRAILING_CONST = 18;
        public const float DEV_ANGLE = 45f / 180f * MathHelper.Pi;
        public const int MAX_PERIOD = (int)(2f * 60);
        public const float MAX_DEV = 15f;
        private VertexStrip tail = new VertexStrip();

        private int turn;
        private Vector2 origin;
        private Vector2 dest;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        // private Texture2D tex;
        private Vector2 dir;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lightning Bolt Advance");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "高级闪电");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(2.5 * 60);
            Projectile.damage = 10;

            //tex = ModContent.Request<Texture2D>(Texture).Value;

            Timer = 0;

            Projectile.width = 15;//tex.Width;
            Projectile.height = 15;//tex.Height;

            dir = Vector2.Zero;
            origin = Vector2.Zero;
            turn = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (dir == Vector2.Zero)
            {
                dir = Projectile.velocity;
                origin = Projectile.Center;
                turn = Main.rand.NextBool() ? 1 : -1;
                Timer = MAX_PERIOD;
            }

            bool timeReached = (Timer >= MAX_PERIOD || (Main.rand.NextFloat() < 0.2f));

            Vector2 displacement = Projectile.Center - origin;
            float projection = Vector2.Dot(displacement, dir.SafeNormalize(Vector2.UnitX));
            bool exceededRange = (Math.Sqrt(displacement.LengthSquared() - projection * projection)) > MAX_DEV;

            if (Timer >= 0.02 * MAX_PERIOD && (timeReached || exceededRange))
            {
                if (exceededRange)
                {
                    turn *= -1;
                    Projectile.velocity = dir.RotatedBy(
                        (Main.rand.NextFloat() * 0.8f + 0.2f) * 0.5 * DEV_ANGLE * (float)turn);
                }
                else
                {
                    turn = Main.rand.NextBool() ? -1 : 1;
                    Projectile.velocity = Projectile.velocity.RotatedBy(
                        (Main.rand.NextFloat() * 0.8f + 0.2f) * DEV_ANGLE * (float)turn);
                }
                Timer = 0;
            }

            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position - Vector2.One * 15f, 30, 30, DustID.Electric);
                d.noGravity = true;
                d.scale *= 0.5f;
            }

            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            #region drawtail

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);


            Main.graphics.GraphicsDevice.Textures[0] =
                ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LightningBoltAdvanceTransparent").Value;

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot, progress => Color.Cyan,
                progress => Projectile.width * 0.5f, -Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot, progress => Color.Cyan * 0.75f,
                progress => Projectile.width, -Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            #endregion

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (origin == Vector2.Zero || Projectile.oldPos[TRAILING_CONST - 1] == Vector2.Zero)
                return base.Colliding(projHitbox, targetHitbox);

            for (int i = 0; i < TRAILING_CONST - 1; i++)
            {
                float point = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    Projectile.oldPos[i] + Projectile.Size / 2, Projectile.oldPos[i + 1] + Projectile.Size / 2, Projectile.width / 2, ref point))
                    return true;
            }

            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < TRAILING_CONST; i++)
            {
                Projectile.oldPos[i] = Projectile.position;
                Projectile.oldRot[i] = Projectile.rotation;
            }
        }
    }
}
