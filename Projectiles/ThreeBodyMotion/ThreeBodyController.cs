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
        public const float G = 6000;
        public const float FOCAL = 1000;

        private Texture2D tex;

        private Sun[] suns;
        private bool summoned;


        //private 
        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
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
            Timer = 0;

            Projectile.width = tex.Width;
            Projectile.height = tex.Height;

            suns = new Sun[3];
            summoned = false;
        }

        public override void AI()
        {
            Projectile.velocity *= 0;

            if (summoned) {

                for (int i = 0; i < 3; i++) {
                    Vector2 acc = Vector2.Zero;

                    for (int j = 0; j < 3; j++) {
                        if (j == i)
                            continue;

                        acc += G * suns[j].Mass / 
                            Math.Max((suns[j].Projectile.Center - suns[i].Projectile.Center).LengthSquared(), 400f)
                            * (suns[j].Projectile.Center - suns[i].Projectile.Center).SafeNormalize(Vector2.Zero);
                    }

                    suns[i].Projectile.velocity += acc;

                    float dist = (suns[i].Projectile.Center - Projectile.Center).Length();

                    if (dist > Sun.DIST_LIMIT)
                        suns[i].Projectile.velocity -= 0.035f * (dist - Sun.DIST_LIMIT) 
                            * (suns[i].Projectile.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

                    if (suns[i].Projectile.velocity.Length() > Sun.SPEED_LIMIT) {
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

                #region 3d
                /*
                for (int i = 0; i < 3; i++) {
                    Vector3 acc = Vector3.Zero;

                    for (int j = 0; j < 3; j++)
                    {
                        if (j == i)
                            continue;

                        acc += G * suns[j].Mass / Math.Max((suns[j].RealPos - suns[i].RealPos).LengthSquared(), 100f)
                            * SafeNormalize(suns[j].RealPos - suns[i].RealPos, Vector3.Zero);
                        
                    }

                    suns[i].RealVel += acc;

                    if (suns[i].RealVel.Length() > Sun.SPEED_LIMIT) {
                        suns[i].RealVel.Normalize();
                        suns[i].RealVel *= Sun.SPEED_LIMIT;
                    }
                }

                Vector3 cm = Vector3.Zero;
                float massSum = 0;

                for (int i = 0; i < 3; i++)
                {
                    suns[i].RealPos += suns[i].RealVel;
                    cm += suns[i].RealPos * suns[i].Mass;
                    massSum += suns[i].Mass;
                }

                cm /= massSum;

                for (int i = 0; i < 3; i++) {
                    // render

                    // normalize wrt center of mass
                    suns[i].RealPos -= cm;


                    // constrain
                    if (suns[i].RealPos.Length() > Sun.DIST_LIMIT)
                        suns[i].RealVel -= (float)(suns[i].RealPos.Length() - Sun.DIST_LIMIT) 
                            * (SafeNormalize(suns[i].RealPos));

                    float factor = FOCAL;
                    if (suns[i].RealPos.Z >= FOCAL)
                        factor /= 0.1f;
                    else
                        factor /= (FOCAL - suns[i].RealPos.Z);

                    suns[i].Projectile.Center = new Vector2(
                        Projectile.Center.X + suns[i].RealPos.X * factor,
                        Projectile.Center.Y + suns[i].RealPos.Y * factor);

                    // sustain
                    suns[i].Projectile.timeLeft++;
                }
                */
                #endregion
            }

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
                    /*
                    suns[i].RealPos = new Vector3(suns[i].Projectile.Center.X - Projectile.Center.X,
                        suns[i].Projectile.Center.Y - Projectile.Center.Y, 0);
                    suns[i].RealVel = 20 * new Vector3((float)Main.rand.NextFloat() - 0.5f,
                        (float)Main.rand.NextFloat() - 0.5f, (float)Main.rand.NextFloat() - 0.5f);
                    */
                    suns[i].Timer = Sun.PERIOD * Main.rand.NextFloat();
                }
                summoned = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
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
            base.Kill(timeLeft);
        }
    }
}
