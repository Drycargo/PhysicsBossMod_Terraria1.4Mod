using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.Items.SummonItems
{
    public class ChaosAttractor: ModItem
    {
        public override void SetStaticDefaults()
        {
            base.SetDefaults();
            DisplayName.SetDefault("Chaos Attractor");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "混沌吸引子");

            Tooltip.SetDefault("Spawn Chaos Theory");
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, "召唤混沌理论");
        }

        public override void SetDefaults()
        {
            Item.damage = 0;

            Item.DamageType = DamageClass.Default;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.RaiseLamp;

            Item.autoReuse = false;
            Item.consumable = false;

            Item.UseSound = SoundID.NPCDeath62;
        }

        public override bool CanUseItem(Player player)
        {
            int requiredType = ModContent.NPCType<ChaosTheory>();
            foreach (NPC n in Main.npc) {
                if (n.type == requiredType && n.active)
                    return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.NewNPC(player.GetSource_FromThis(), (int)player.Center.X, (int)(player.Center.Y - 1000),
                    ModContent.NPCType<ChaosTheory>());
            }
            return true;
        }
    }
}
