using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.Effects;
using PhysicsBoss.NPCs.Boss.ChaosTheory;
using PhysicsBoss.Projectiles;
using PhysicsBoss.Projectiles.ButterflyEffect;
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
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Items
{
    public class ProjectileTestItem:ModItem
    {
        public override void SetStaticDefaults()
        {
            base.SetDefaults();
            DisplayName.SetDefault("Projectile Test Staff");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "弹幕测试");

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
            Item.shoot = ModContent.ProjectileType <LargeLightningBolt>();
            Item.shootSpeed = 5f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*
            if (!SkyManager.Instance["PhysicsBoss:BlackSky"].IsActive())
            {
                SkyManager.Instance.Activate("PhysicsBoss:BlackSky");
            }*/
            /*
            for (int i = 0; i < 50; i++) {
                Dust d = Dust.NewDustDirect(player.Center, 0, 0, DustID.Fireworks);
            }
            */
            
            Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
                type, 50, knockback, player.whoAmI);
            /*
            
            TestEffectProjectile l = (TestEffectProjectile)
                Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI).ModProjectile;

            l.target = player;
            
            l.initialize();
            l.materialize();
            l.swing();

            
            Tornado t = (Tornado)(Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI).ModProjectile);

            t.spawn();
            /*

            /*
            NPC.NewNPC(player.GetSource_FromThis(), (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, ModContent.NPCType<ButterflySpiralSink>());
            

            TrailingStarController t = (TrailingStarController)
                Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI).ModProjectile;


            t.summonStarBundle<TrailingStarLorenz>();


            
            LorenzFinaleController t = (LorenzFinaleController) 
                Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed*(Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX), 
                type, damage, knockback, player.whoAmI).ModProjectile;
            
            for (int i = 0; i < 5; i++)
                t.summonStarBundle<TrailingStarLorenzFinale>();
            t.activateAll();
            //t.releaseStarBundle();
            */

            return false;
        }
    }
}
