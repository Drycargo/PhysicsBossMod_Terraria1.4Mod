using Microsoft.Xna.Framework;
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
            Item.shoot = ModContent.ProjectileType<WaterDrop>();
            Item.shootSpeed = 10f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            WaterDropController wdc = new WaterDropController(player.Center);
            wdc.summonAll();

            /*
            Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed * (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI);
            */

            /*
            ThreeScrollController t = (ThreeScrollController) 
                Projectile.NewProjectileDirect(source, Main.MouseWorld, Item.shootSpeed*(Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX), 
                type, damage, knockback, player.whoAmI).ModProjectile;
            
            t.summonStarBundle<TrailingStarThreeScroll>();
            t.release();
            //t.release((Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX));
            //t.releaseStarBundle(player);
            */
            return false;
        }
    }
}
