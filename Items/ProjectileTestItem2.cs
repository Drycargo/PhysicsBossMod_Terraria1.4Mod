using Microsoft.Xna.Framework;
using PhysicsBoss.Effects;
using PhysicsBoss.Projectiles;
using PhysicsBoss.Projectiles.ConwayGame;
using PhysicsBoss.Projectiles.DoublePendulum;
using PhysicsBoss.Projectiles.ThreeBodyMotion;
using PhysicsBoss.Projectiles.TrailingStarMotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Items
{
    public class ProjectileTestItem2:ModItem
    {
        public override void SetStaticDefaults()
        {
            base.SetDefaults();
            DisplayName.SetDefault("Projectile Test Staff2");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "弹幕测试2");

            Tooltip.SetDefault("As description.");
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, "如题");
        }

        public override void SetDefaults()
        {
            Item.damage = 50;

            Item.DamageType = DamageClass.Magic;

            Item.noMelee = true;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 10;
            Item.useStyle = 5;

            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<ElectricChargeController>();
            Item.shootSpeed = 10f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            NPC.NewNPC(player.GetSource_FromThis(), (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y,
                ModContent.NPCType<BrightNodePhantom>());
            /*
            LorenzFinaleController t = (LorenzFinaleController)
               Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
               type, damage, knockback, player.whoAmI).ModProjectile;

            for (int i = 0; i < 24; i++)
                t.summonStar();

            t.activateAll();
            
            
            TrailingStarController t = (TrailingStarController)
               Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
               type, damage, knockback, player.whoAmI).ModProjectile;

            t.summonStarBundle<TrailingStarLorenzFinale>();
            
            
            FractalMap l = (FractalMap)
                Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI).ModProjectile;

            l.initialize();
            l.materialize();

            
            
            int tot = Enum.GetNames(typeof(ParticleOrchestraType)).Length;
            for (int i = 0; i < tot; i++) {
                ParticleOrchestraSettings settings = new ParticleOrchestraSettings
                {
                    PositionInWorld = player.Center,
                    MovementVector = ((float)i / (float)tot * MathHelper.TwoPi).ToRotationVector2() * 20f
                };

                ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, (ParticleOrchestraType)(Main.rand.Next(tot - 1)), settings);
            }

            Main.UseStormEffects = true;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                Sandstorm.StartSandstorm();
                Main.StartRain();
                Main.windSpeedCurrent = 1f;
            }
            
            
            
            Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI);
            

            
            Projectile p = Projectile.NewProjectileDirect(source, Main.MouseWorld, Vector2.Zero,
                ProjectileID.CoolWhipProj, damage, knockback, player.whoAmI);
            
            p.friendly = false;
            p.hostile = true;
            p.aiStyle = 1;
           
            
            ThreeBodyController tbc = (ThreeBodyController) (Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI).ModProjectile);
            */
            //tbc.summonSuns();

            return false;
        }
    }
}
