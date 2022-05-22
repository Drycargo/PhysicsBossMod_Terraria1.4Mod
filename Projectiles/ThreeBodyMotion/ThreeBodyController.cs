using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.Projectiles.TrailingStarMotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles.ThreeBodyMotion
{
    public class ThreeBodyController:ModProjectile
    {
        public const int TRAILING_CONST = 10;
        public const float G = 7000;
        public const float FOCAL = 1000;
        public const float INTENSITY_MAX = 10f;

        private Texture2D tex;

        private Sun[] suns;
        private bool summoned;

        private float visualEffectIntensity;
        private float bloomIntensity;

        //private 
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public Sun this[int i]
        {
            get { return suns[i]; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Three Body Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "三体运动控制");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(20 * 60);
            Projectile.damage = 0;

            tex = ModContent.Request<Texture2D>(Texture).Value;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAILING_CONST;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Timer = (float)(Main.rand.Next() % 600);

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            suns = new Sun[3];
            summoned = false;
            visualEffectIntensity = 0;
            bloomIntensity = 0;
        }

        public override void AI()
        {
            Projectile.velocity *= 0;

            if (summoned)
            {

                for (int i = 0; i < 3; i++)
                {
                    suns[i].Projectile.timeLeft++;
                    Vector2 acc = Vector2.Zero;

                    for (int j = 0; j < 3; j++)
                    {
                        if (j == i)
                            continue;

                        acc += G * suns[j].Mass /
                            Math.Max((suns[j].Projectile.Center - suns[i].Projectile.Center).LengthSquared(), 400f)
                            * (suns[j].Projectile.Center - suns[i].Projectile.Center).SafeNormalize(Vector2.Zero);
                    }

                    suns[i].Projectile.velocity += acc;

                    float dist = (suns[i].Projectile.Center - Projectile.Center).Length();

                    if (dist > Sun.DIST_LIMIT)
                        suns[i].Projectile.velocity -= 0.05f * (dist - Sun.DIST_LIMIT)
                            * (suns[i].Projectile.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

                    if (suns[i].Projectile.velocity.Length() > Sun.SPEED_LIMIT)
                    {
                        suns[i].Projectile.velocity.Normalize();
                        suns[i].Projectile.velocity *= Sun.SPEED_LIMIT;
                    }
                }

                Vector2 cm = Vector2.Zero;
                float massTot = 0;

                for (int i = 0; i < 3; i++)
                {
                    cm += suns[i].Projectile.Center * suns[i].Mass;
                    massTot += suns[i].Mass;
                }

                cm /= massTot;

                for (int i = 0; i < 3; i++)
                {
                    suns[i].Projectile.Center -= (cm - Projectile.Center);
                    suns[i].Projectile.timeLeft++;
                }

                if (bloomIntensity > 1)
                    bloomIntensity *= 0.95f;
                else
                    bloomIntensity = 0;
            }
            else {
                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 8; j++) {
                        Dust d = Dust.NewDustDirect(Projectile.Center + 0.7f * Sun.DIST_LIMIT * 
                            (Timer/600 * MathHelper.TwoPi + (float)i * MathHelper.TwoPi/3).ToRotationVector2()
                            + Main.rand.NextVector2Unit() * 30f,
                            0,0,DustID.FlameBurst);
                        d.velocity *= 0.5f;
                        d.scale *= 3f;
                        d.noGravity = true;
                    }
                }

                bloomIntensity = 0.02f * INTENSITY_MAX * Math.Min(1, (Timer / 75f));
            }

            if (visualEffectIntensity > 0)
            {
                GlobalEffectController.centerTwist(visualEffectIntensity * 0.2f,
                    (INTENSITY_MAX - visualEffectIntensity) / INTENSITY_MAX * 2500 + 50, 50, Projectile.Center);
                GlobalEffectController.shake(2 * visualEffectIntensity);
                GlobalEffectController.blur(visualEffectIntensity / INTENSITY_MAX * 0.5f);
            }

            if (visualEffectIntensity > 0)
                visualEffectIntensity -= 0.1f;
            else
                visualEffectIntensity = 0;

            Timer++;
        }

        public void summonSuns() {
            if (!summoned)
            {
                for (int i = 0; i < 3; i++)
                {
                    suns[i] = (Sun)(Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(),
                        Projectile.Center + (Timer / 600 * MathHelper.TwoPi + (float)i * MathHelper.TwoPi / 3).ToRotationVector2() * Sun.DIST_LIMIT * 0.7f,
                        Main.rand.NextVector2Unit() * 10f, ModContent.ProjectileType<Sun>(), 50, 0).ModProjectile);

                    suns[i].Timer = Sun.PERIOD * Main.rand.NextFloat();
                }
                summoned = true;

                visualEffectIntensity = INTENSITY_MAX;
                bloomIntensity = 1.5f * INTENSITY_MAX;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            if (bloomIntensity > 0)
                GlobalEffectController.bloom(bloomIntensity, 0.3f);
            if (Projectile.timeLeft <= 1)
                GlobalEffectController.bloom(-1, 0.9f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }

        public static Vector3 SafeNormalize(Vector3 v,Vector3 defaultV = default(Vector3)) {
            if (v.Length() == 0)
                return defaultV;
            v.Normalize();
            return v;
        }

        public override void Kill(int timeLeft)
        {
            if (summoned) {
                for (int i = 0; i < 3; i++) {
                    suns[i].Projectile.Kill();
                }
            }

            GlobalEffectController.centerTwist(-1,0,0, Projectile.Center);
            GlobalEffectController.shake(-1);
            GlobalEffectController.blur(-1);
            base.Kill(timeLeft);
        }
    }
}
