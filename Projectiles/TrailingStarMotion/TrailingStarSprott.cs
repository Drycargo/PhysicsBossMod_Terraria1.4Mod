using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarSprott: TrailingStarChaotic
    {
        public const int SCALE_TRANSIT = 30;
        public const int KILL_TRANSIT = 60;
    
        public override float SHRINK_CONST
        {
            get { return 20f; }
        }

        public override float STEP => 5;

        public override Matrix Transform =>
            Matrix.CreateRotationX(MathHelper.PiOver2)
            * Matrix.CreateTranslation(-0.7f, 0, -2f)
            * Matrix.CreateScale(10f * Math.Min(1, Timer / (float)SCALE_TRANSIT))
            * Matrix.CreateRotationZ(Timer * 0.01f);

        private Texture2D warningRing;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Sprott");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-Sprott吸引子");
            base.SetStaticDefaults();
        }
        protected override void setBasicDefaults()
        {
            base.setBasicDefaults();
            Projectile.timeLeft = KILL_TRANSIT;
            drawColor = Color.Lerp(Color.White, Color.LightBlue, 0.5f);
            Projectile.damage = 0;

            warningRing = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/ButterflyEffect/IceSpikeWarning").Value;
        }

        protected override void motionUpdate()
        {
            float a = 2.07f, b = 1.79f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                y + a*x*y + x*z,
                1 - b*x*x + y*z,
                x - x*x - y*y) / 60;

            float speed = realVel.Length() * SHRINK_CONST;

            if (speed > SPEED_LIMIT)
            {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;

            if (Main.rand.NextFloat() < 0.25)
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Ice);

            base.motionUpdate();
        }

        public override void releaseProj(Player t)
        {
            Projectile.velocity = 0.35f * SPEED_LIMIT *
                    (Projectile.position - Projectile.oldPos[0]).SafeNormalize(Main.rand.NextVector2Unit());
            base.releaseProj(t);
        }

        protected override void releaseAction()
        {
            Projectile.velocity *= 0.92f;

            Projectile.rotation = Projectile.velocity.ToRotation() - 0.05f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++) {
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center,
                        (Projectile.rotation + MathHelper.TwoPi / 6f * (float)i).ToRotationVector2(),
                        ModContent.ProjectileType<IceSpike>(), 45, 0);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (released)
                Main.spriteBatch.Draw(warningRing, new Rectangle((int)(Projectile.Center.X - IceSpike.LENGTH - Main.screenPosition.X),
                    (int)(Projectile.Center.Y - IceSpike.LENGTH - Main.screenPosition.Y), IceSpike.LENGTH * 2, IceSpike.LENGTH * 2),
                    Color.Blue * ((float)(KILL_TRANSIT - Projectile.timeLeft)/ KILL_TRANSIT));
            return base.PreDraw(ref lightColor);
        }
    }
}
