using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarLorenzFinale: TrailingStarChaotic
    {
        public const int TRANSIT = 90;
        public const int FRAME_COUNT = 3;
        public static Color DEACTIVATED_COLOR = Color.White * 0.65f;
        public override float STEP => Timer < 120 ? (120 - Timer)/40 + 1 : 1;
        public override float SPEED_LIMIT => MathHelper.Lerp(45, 2.5f, Math.Min(1,Timer/TRANSIT));

        public override int TRAILING_CONST => 45;

        public static float generalRotation = 0;
        public const float ROTATION_STEP = MathHelper.TwoPi / (90 * 60);

        public override Matrix Transform =>
            Matrix.CreateRotationX(MathHelper.PiOver2)
            * Matrix.CreateTranslation(0, 25, 0)
            * Matrix.CreateScale(4f)
            * Matrix.CreateRotationY((float)generalRotation);


        private int activationCharge;

        /*
        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            drawColor = DEACTIVATED_COLOR;
            activationCharge = -1;
        }
        */

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Lorenz Finale");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-最终洛伦茨吸引子");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            //base.SetStaticDefaults();
        }

        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();

            Projectile.width = tex.Width;
            Projectile.height = tex.Height / FRAME_COUNT;

            Projectile.timeLeft = (int)(1.5 * 60);
            drawColor = DEACTIVATED_COLOR;
            Projectile.frameCounter = 0;
            activationCharge = -1;
            /*
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(3.75 * 60);
            Projectile.damage = 20;

            tex = ModContent.Request<Texture2D>(Texture).Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height / FRAME_COUNT;

            Projectile.timeLeft = (int)(1.5 * 60);
            drawColor = DEACTIVATED_COLOR;
            Projectile.frameCounter = 0;
            */
        }

        public override void AI()
        {
            base.AI();

            if ((int)Timer % 5 == 0)
            {
                Projectile.frameCounter++;
                Projectile.frameCounter %= FRAME_COUNT;
            }

            if (Main.rand.NextFloat() < 0.25f)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowMk2);
                d.color = drawColor;
                d.noGravity = true;
            }

            if (activationCharge > 0)
            {
                activationCharge--;
                if (activationCharge == 0)
                    activate();
            }
        }

        protected override void motionUpdate()
        {
            float p = 28, s = 10, b = 8f / 3f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
               s * (y - x),
               x * (p - z) - y,
               x * y - b * z) / 60;

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT)
            {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;

            Projectile.velocity *= 0;

            base.motionUpdate();
        }

        public void activate() {
            activationCharge = 0;
            drawColor = Color.White;

            for (int i = 0; i < 30; i++) {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0,0,DustID.FireworksRGB);
                d.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                d.velocity *= 3;
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
        }

        protected override float widthFun(float progress)
        {
            return (1f - progress) * tex.Width * 0.45f;
        }
        protected override Color colorFun(float progress)
        {
            if (activationCharge == 0)
                return Main.hslToRgb(progress, 0.9f, 0.7f, (byte)(255 * (1 - progress))) * 0.75f;

            return drawColor * (1 - progress);
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                new Rectangle(0, Projectile.frameCounter * Projectile.height, Projectile.width, Projectile.height),
                drawColor, 0, Projectile.Size / 2, 1f, ((Projectile.position.X < Projectile.oldPos[1].X) ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 0);

            Lighting.AddLight(Projectile.Center, drawColor.ToVector3());

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                ParticleOrchestraSettings settings = new ParticleOrchestraSettings
                {
                    PositionInWorld = Projectile.Center,
                    MovementVector = Main.rand.NextVector2Unit() * 5f
                };

                ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.RainbowRodHit, settings);
            }
            base.Kill(timeLeft);
        }


        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if(activationCharge == 0)
                return base.Colliding(projHitbox, targetHitbox);

            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (activationCharge == 0)
                activationCharge = 120;
            drawColor = DEACTIVATED_COLOR;
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowRod);
                d.velocity *= 3;
                d.color = Color.White;
                d.noGravity = true;
            }

            base.OnHitPlayer(target, damage, crit);
        }
    }
}
