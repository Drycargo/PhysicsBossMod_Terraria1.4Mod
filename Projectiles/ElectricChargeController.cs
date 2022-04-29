﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.NPC.Boss.ChaosTheory;
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
        public const int RADIUS = 250;
        public const float SE_CONST = 1250f;
        public const int CHARGE_LIMIT = (int)(1.25 * 60);
        public const float SPEED_LIMIT = 45f;

        private ElectricCharge[] charges;
        private bool initialized;
        private float currSpeed;

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
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (initialized)
            {
                for (int i = 0; i < CAPACITY; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (charges[i].getCharge() * charges[j].getCharge() < 0)
                            GlobalEffectController.drawAimLine(Main.spriteBatch, 
                                charges[i].Projectile.Center, charges[j].Projectile.Center, Color.SkyBlue * 0.5f, 10f);
                    }
                }
            }
            
            return false;
        }

        public override void AI()
        {
            Projectile.velocity *= 0;

            if (currSpeed + 0.75f <= SPEED_LIMIT)
                currSpeed += 0.75f;

            if (!initialized)
            {
                float offset = 0.25f + 0.5f * Main.rand.NextFloat();

                for (int i = 0; i < CAPACITY; i++) {
                    int id = Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(),
                        Projectile.Center + RADIUS * (MathHelper.TwoPi * ((float) i + offset)/((float)CAPACITY)).ToRotationVector2(),
                        Vector2.Zero, ModContent.ProjectileType<ElectricCharge>(), 30, 0);

                    charges[i] = (ElectricCharge)Main.projectile[id].ModProjectile;
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
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < CAPACITY; i++)
            {
                if (charges[i].getCharge() > 0)
                {
                    for (int j = 0; j < CAPACITY; j++)
                    {
                        if (charges[j].getCharge() < 0) {
                            Projectile.NewProjectile(charges[i].Projectile.GetProjectileSource_FromThis(),
                                charges[i].Projectile.Center, 
                                30f * (charges[j].Projectile.Center - charges[i].Projectile.Center).SafeNormalize(Vector2.UnitX),
                                ModContent.ProjectileType<LightningBolt>(), 50, 0);
                        }
                    }
                }
            }

            for (int i = 0; i < CAPACITY; i++)
            {
                charges[i].Projectile.Kill();
            }

            SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
    }
}
