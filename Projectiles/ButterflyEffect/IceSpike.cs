using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Projectiles.ButterflyEffect
{
    public class IceSpike: ModProjectile
    {
        public const int WIDTH = 50;
        public const int LENGTH = 250;
        public const int TRANSIT = 6;
        public const int COUNT = 1;
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        private Texture2D tex;
        private Texture2D maskTex;
        private float progress;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Spike");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "冰刺");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(1 * 60);
            Projectile.damage = 30;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            maskTex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/ButterflyEffect/IceSpikeMask").Value;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            progress = 0;
        }

        public override void AI()
        {
            for (int i = 0; i < Math.Max(0, 5 * (TRANSIT - Timer)); i++) {
                Dust d = Dust.NewDustDirect(Vector2.Lerp(Projectile.Center, 
                    Projectile.Center + progress * Projectile.rotation.ToRotationVector2() * LENGTH, Main.rand.NextFloat()),
                    10,10,DustID.IceRod);
                d.velocity += Projectile.rotation.ToRotationVector2() * 10f;
            }

            Dust.NewDust(Vector2.Lerp(Projectile.Center,
                    Projectile.Center + progress * Projectile.rotation.ToRotationVector2() * LENGTH, Main.rand.NextFloat()),
                    10, 10, DustID.IceRod);

            if (Timer < TRANSIT) {
                float factor = Timer / TRANSIT - 1;
                progress = 1 - factor * factor;
            }

            if (Timer < 2 * TRANSIT) {
                GlobalEffectController.shake(Math.Max(0,(1 - (Timer - 1)/(2 * TRANSIT)) * 1.5f));
            }

            Timer++;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0;
            SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Rectangle dest = new Rectangle((int)((int)Projectile.Center.X - Main.screenPosition.X), 
                (int)(Projectile.Center.Y  - Main.screenPosition.Y),
                (int)(progress * LENGTH), WIDTH);

            Main.spriteBatch.Draw(tex,dest,null,
                lightColor, Projectile.rotation, 0.25f * Vector2.UnitY * WIDTH, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(maskTex,dest, null,
                Color.White, Projectile.rotation, 0.25f * Vector2.UnitY * WIDTH, SpriteEffects.None, 0);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + progress * Projectile.rotation.ToRotationVector2() * LENGTH * 0.8f, WIDTH * 0.25f, 
                ref point);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDustDirect(Vector2.Lerp(Projectile.Center,
                    Projectile.Center + progress * Projectile.rotation.ToRotationVector2() * LENGTH, Main.rand.NextFloat()),
                    10, 10, DustID.IceRod);
            }

            SoundEngine.PlaySound(SoundID.Item107, Projectile.Center);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
             target.AddBuff(BuffID.Frostburn, 180);
        }
    }
}
