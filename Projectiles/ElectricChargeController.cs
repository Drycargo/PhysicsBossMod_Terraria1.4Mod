using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.NPCs.Boss.ChaosTheory;
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

namespace PhysicsBoss.Projectiles
{
    public class ElectricChargeController: ModProjectile
    {
        public const int CAPACITY = 8;
        public const int RADIUS = 325;
        public const float SE_CONST = 1250f;
        public const int CHARGE_LIMIT = (int)(1.25 * 60);
        public const float SPEED_LIMIT = 45f;

        private ElectricCharge[] charges;
        private bool initialized;
        private float currSpeed;
        private bool dead;

        public float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electric Charge Controller");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "电荷控制");
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)(ChaosTheory.ELE_CHARGE_DURATION * 0.5);//(int)(8 * 60);
            Projectile.damage = 0;
            Projectile.velocity = Vector2.Zero;

            Projectile.width = 0;
            Projectile.height = 0;

            charges = new ElectricCharge[CAPACITY];
            initialized = false;
            Timer = 0;
            currSpeed = 0;
            dead = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (dead)
            {
                float prog = ((Projectile.timeLeft - 1) / 30f);
                GlobalEffectController.bloom((float)prog * 2f, 0.3f);
                GlobalEffectController.shake((float)prog * 2.5f);
            } else if (initialized) {
                for (int i = 0; i < CAPACITY; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        float factor = (Timer > ChaosTheory.ELE_CHARGE_DURATION * 0.35f) ?
                            0.75f : (0.75f * Timer / (ChaosTheory.ELE_CHARGE_DURATION * 0.35f));
                        if (charges[i].getCharge() * charges[j].getCharge() < 0)
                        {
                            Vector2 source, dest;
                            if (charges[i].getCharge() > 0)
                            {
                                source = charges[i].Projectile.Center;
                                dest = charges[j].Projectile.Center;
                            }
                            else
                            {
                                source = charges[j].Projectile.Center;
                                dest = charges[i].Projectile.Center;
                            }
                            GlobalEffectController.drawRayLine(Main.spriteBatch,
                                source, dest,
                                Color.SkyBlue * factor, 10f);
                        }
                    }
                }
            }
            
            return false;
        }

        public override void AI()
        {
            Projectile.velocity *= 0;
            if (! dead && Projectile.timeLeft == 1)
            {
                dead = true;
                Projectile.timeLeft = 30;
                lastWords();
            }

            if (dead) {
                return;
            }

            if (currSpeed + 0.75f <= SPEED_LIMIT)
                currSpeed += 0.75f;

            if (!initialized)
            {
                float offset = 0.25f + 0.5f * Main.rand.NextFloat();

                for (int i = 0; i < CAPACITY; i++) {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        charges[i] = (ElectricCharge)Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                            Projectile.Center + RADIUS * (MathHelper.TwoPi * ((float)i + offset) / ((float)CAPACITY)).ToRotationVector2(),
                            new Vector2((i == CAPACITY - 1 || (i != 0 && Main.rand.NextBool())) ? -1f : 1f,0), ModContent.ProjectileType<ElectricCharge>(), 20, 0).ModProjectile;

                        /*
                        if (i == CAPACITY || (i != 0 && Main.rand.NextBool()))
                            charges[i].alterCharge();
                        */
                    }
                }
                initialized = true;
            }

            for (int i = 0; i < CAPACITY; i++)
            {
                Vector2 acc = Vector2.Zero;

                for (int j = 0; j < CAPACITY; j++) {
                    if (j != i) {
                        
                        acc += SE_CONST * (charges[i].getCharge() * charges[j].getCharge()) /
                            ((charges[i].Projectile.Center).DistanceSQ(charges[j].Projectile.Center))
                            * (charges[i].Projectile.Center - charges[j].Projectile.Center).SafeNormalize(Vector2.UnitX);

                    }
                }

                charges[i].Projectile.velocity += acc;

                float speed = charges[i].Projectile.velocity.Length();
                if (speed > currSpeed) {
                    charges[i].Projectile.velocity *= (currSpeed / speed);
                }
            }

            Timer++;
        }

        private void lastWords()
        {
            for (int i = 0; i < CAPACITY; i++)
            {
                if (charges[i].getCharge() > 0)
                {
                    for (int j = 0; j < CAPACITY; j++)
                    {
                        if (charges[j].getCharge() < 0)
                        {
                            Vector2 displacement = charges[j].Projectile.Center - charges[i].Projectile.Center;

                            Projectile.NewProjectileDirect(charges[i].Projectile.GetSource_FromThis(),
                                charges[i].Projectile.Center,
                                30f * displacement.SafeNormalize(Vector2.UnitX),
                                ModContent.ProjectileType<LightningBoltAdvance>(), 30, 0);
                        }
                    }
                }
            }

            for (int i = 0; i < CAPACITY; i++)
            {
                charges[i].Projectile.Kill();
            }

            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.Center);
        }

        /*
        public override void Kill(int timeLeft)
        {
            lastWords();
        }*/


        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
    }
}
