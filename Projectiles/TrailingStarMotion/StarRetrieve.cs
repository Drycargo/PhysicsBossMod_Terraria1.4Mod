using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class StarRetrieve : ModProjectile
    {
        public const int CHARGE_PERIOD = 45;
        public const float RADIUS = 50f;
        public const int ROTATION_PERIOD = 15;
        public const int TRAILING_CONST = 10;

        private Vector2 dir = Vector2.Zero;
        private Projectile star = null;
        private Texture2D tex;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star Retrieve");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "回航卫星");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(1 * 60);
            Projectile.damage = 0;

            Timer = 0;

            tex = ModContent.Request<Texture2D>(Texture).Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            base.SetDefaults();
        }

        public override void AI()
        {
            if (Timer == 0) {
                dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.velocity = -15 * dir;
            }

            if (Projectile.velocity.Length() < 20f) {
                Projectile.velocity += 0.3f * dir;
            }

            if (Projectile.velocity.X * dir.X + Projectile.velocity.Y * dir.Y > 0)
            {
                if (star == null)
                {
                    star = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<TrailingStarPlain>(), 25, 0);
                    SoundEngine.PlaySound(SoundID.Item25, Projectile.position);
                    for (int i = 0; i < 30; i++)
                    {
                        Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowRod);
                        d.noGravity = true;
                        d.velocity = Main.rand.NextVector2Unit() * 16f;
                        d.color = Color.Green;
                    }
                }
            }
            else {
                Projectile.timeLeft++;
            }

            Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowMk2);
            dust.noGravity = true;
            dust.color = Color.White;

            if (star != null) {
                float indicator = dir.X > 0 ? -1 : 1;
                star.Center = Projectile.Center + ((indicator * Timer / (float)ROTATION_PERIOD
                    + (indicator > 0 ? 0 : 0.5f)) * MathHelper.TwoPi).ToRotationVector2() * RADIUS;
                star.timeLeft++;
                ((TrailingStarPlain)star.ModProjectile).setColor(Color.Lerp(Color.Green, Color.Blue, (Timer % (2 * ROTATION_PERIOD)) / (float)(2 * ROTATION_PERIOD)));
            }

            Timer++;
        }

        public override void Kill(int timeLeft)
        {
            if (star != null) {
                star.Kill();
                star = null;
            }
            base.Kill(timeLeft);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < TRAILING_CONST; i++)
            {
                Main.spriteBatch.Draw(tex, Projectile.oldPos[i] - Main.screenPosition, Color.White * (1f - (float)i/(float)TRAILING_CONST) * 0.8f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
