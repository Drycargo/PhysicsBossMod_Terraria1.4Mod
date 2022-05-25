using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using PhysicsBoss.Projectiles;
using PhysicsBoss.Effects;

namespace PhysicsBoss.NPCs.Boss.ChaosTheory
{
    public class ButterflySpiralSink: ModNPC
    {
        public readonly Color[] colors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Purple};

        private NPC origin;

        private Color drawColor;

        private Texture2D tex;

        public float Timer
        {
            get { return NPC.ai[0]; }
            set { NPC.ai[0] = value; }
        }


        public override void SetStaticDefaults()
        {
            base.SetDefaults();
            DisplayName.SetDefault("Butterfly Spiral Sink");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "螺旋汇聚蝶");

            Main.npcFrameCount[NPC.type] = 3;
            NPCID.Sets.TrailingMode[NPC.type] = 2;
            NPCID.Sets.TrailCacheLength[NPC.type] = 15;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = 0;
            tex = ModContent.Request<Texture2D>(Texture).Value;
            NPC.friendly = false;

            NPC.width = tex.Width;
            NPC.height = tex.Height / Main.npcFrameCount[NPC.type];

            NPC.lifeMax = 300;
            NPC.defense = 20;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 0, 15, 0);

            Timer = 0f;

            origin = Main.npc[NPC.whoAmI];

            drawColor = colors[(int)(Main.rand.NextFloat() * colors.Length)];
        }

        public override void AI()
        {

            base.AI();
        }
    }
}
