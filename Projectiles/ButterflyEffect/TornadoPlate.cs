using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.ButterflyEffect
{
    
    public class TornadoPlate:ModProjectile
    {
        public const int WIDTH = 420;
        public const int HEIGHT = 100;
        public const float PERIOD = 45;
        public const float RADIUS = 80;
        public const float FOCAL = 6 * RADIUS;
        public const float TILT_ANGLE = 5f / 180f * MathHelper.Pi;
        public const int FRAME_COUNT = 6;

        private Texture2D tex;
        private bool released;
        private float dispX;
        private float dispY;
        private float factor;
        private float tiltAngle;
        private float phaseDev;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tornado Plate");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "龙卷风盘");

            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.damage = 80;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            Projectile.timeLeft = (int)(3 * 60);
            Projectile.frameCounter = 0;
            Projectile.frame = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height / FRAME_COUNT;
            Projectile.hide = true;

            released = false;
            factor = 1;
            dispX = dispY = 0;
            phaseDev = 0;
        }

        public override void AI()
        {
            if (Timer < 10) {
                for (int i = 0; i < 5 * (10 - Timer); i++) {
                    summonDust();
                }
            }else if (Main.rand.NextFloat() < 0.5)
                summonDust();

            if (!released)
            {
                Projectile.velocity *= 0;
                updateRect();
                Projectile.timeLeft++;
            }
            else {
                if (Projectile.velocity.Length() < 25)
                    Projectile.velocity *= 1.05f;
            }

            tiltAngle = (float)(Math.Sin(Timer / PERIOD * MathHelper.Pi) * TILT_ANGLE);

            Projectile.frameCounter++;
            if (Projectile.frameCounter % 3 == 0) {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                Projectile.frame %= FRAME_COUNT;
            }

            Timer++;
        }

        private void summonDust()
        {
            Dust d = Dust.NewDustDirect(new Vector2(Projectile.Center.X + dispX, Projectile.Center.Y + dispY),
                                (int)(factor * WIDTH), (int)(factor * HEIGHT), DustID.AncientLight);
            d.velocity *= 2f;
        }

        private void updateRect()
        {
            float orbitAngle = (Timer / PERIOD) * MathHelper.TwoPi;

            float x = (float)(Math.Cos(orbitAngle) * RADIUS);
            float z = (float)(Math.Sin(orbitAngle) * RADIUS);

            factor = FOCAL / (FOCAL - z);

            dispX = factor * x - factor * WIDTH * 0.5f;
            dispY = -factor * HEIGHT * 0.5f;

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.Draw(tex, new Rectangle((int)(Projectile.Center.X + dispX - Main.screenPosition.X),
                (int)(Projectile.Center.Y + dispY - Main.screenPosition.Y), (int)(factor * WIDTH), (int)(factor * HEIGHT)), 
                new Rectangle(0,Projectile.frame * Projectile.height, Projectile.width, Projectile.height),
                Color.Lerp(Color.Black, Color.White, (factor * 0.6f + 0.4f)), tiltAngle, Vector2.Zero,SpriteEffects.None, 0);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvAABBCollision(new Vector2(Projectile.Center.X + dispX, Projectile.Center.Y + dispY),
                new Vector2(factor * WIDTH, factor * HEIGHT), targetHitbox.TopLeft(), targetHitbox.Size());

            /*new Rectangle((int)(Projectile.Center.X + dispX), (int)(Projectile.Center.Y + dispY),
                (int)(factor * WIDTH), (int)(factor * HEIGHT)), targetHitbox*/
        }

        public void release(Vector2 dir) {
            released = true;
            Projectile.velocity = (dir == Vector2.Zero ? Vector2.UnitX : dir);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
    }
}
