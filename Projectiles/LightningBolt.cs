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
    public class LightningBolt: ModProjectile
    {
        public static readonly int TRAILING_CONST = 30;
        public static readonly float DEV_ANGLE = 10f/180f*MathHelper.Pi;
        public static readonly float PERIOD = 0.1f*60;
        private VertexStrip tail = new VertexStrip();

        private int turn;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        private Texture2D tex;
        private Vector2 dir;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lightning Bolt");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "闪电");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(10 * 60);
            Projectile.damage = 50;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            //tex = ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNBlock").Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = PERIOD/2;

            Projectile.width = 7;//tex.Width;
            Projectile.height = 10;//tex.Height;

            dir = Vector2.Zero;
            turn = -1;
        }

        public override void AI()
        {
            if (dir == Vector2.Zero)
            {
                dir = Projectile.velocity;
                Projectile.velocity = Projectile.velocity.RotatedBy(DEV_ANGLE);
                Projectile.rotation = Projectile.velocity.ToRotation();
                turn = -1;
            }
            if (Timer >= PERIOD) {
                Timer = 0;
                Projectile.rotation = turn * DEV_ANGLE + dir.ToRotation();
                Projectile.velocity = Projectile.velocity.Length() * Projectile.rotation.ToRotationVector2();
                turn *= -1;
            }
            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            #region drawtail
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = tex;

            tail.PrepareStrip(Projectile.oldPos, Projectile.oldRot, progress => Color.Red,
                progress => {
                    double scaleFactor = 1;
                    if (progress < 0.25)
                    {
                        scaleFactor = progress / 0.25;
                    }
                    else if (progress > 0.75)
                    {
                        scaleFactor = (1f - progress) / 0.25;
                    }
                    return (float)scaleFactor * Projectile.width/2;
                },
                Projectile.Size/ 2 - Main.screenPosition, TRAILING_CONST);
            tail.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            #endregion

            //Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            if (Projectile.oldPos[TRAILING_CONST - 1] == Vector2.Zero)
                return base.Colliding(projHitbox, targetHitbox);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, Projectile.oldPos[TRAILING_CONST - 1] + Projectile.Size / 2, Projectile.width * 0.8f, ref point);
        }
    }
}
