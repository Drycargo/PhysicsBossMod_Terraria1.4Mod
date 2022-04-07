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

namespace PhysicsBoss.NPC.Boss.ChaosTheory
{
    [AutoloadBossHead]
    public class ChaosTheory : ModNPC
    {
        public static readonly float MAX_DISTANCE = 2000f;
        public enum phase
        {
            INIT = 0,
        }

        private Texture2D tex;
        private Player target;
        private phase currentPhase;

        public override string BossHeadTexture => base.BossHeadTexture;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Chaos Theory");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "混沌理论");

            Main.npcFrameCount[NPC.type] = 2;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            tex = ModContent.Request<Texture2D>(Texture).Value;
            target = null;

            NPC.width = tex.Width;
            NPC.height = tex.Height / Main.npcFrameCount[NPC.type];

            NPC.damage = 50;

            NPC.lifeMax = 1100;
            NPC.defense = 100;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.value = Item.buyPrice(0, 0, 15, 0);

            NPC.buffImmune[BuffID.Burning] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.Frostburn2] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;
            NPC.buffImmune[BuffID.OnFire3] = true;
            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.Venom] = true;

            Banner = NPC.type;
            BannerItem = ItemID.Gel; //stub


            NPC.noGravity = true;
            NPC.noTileCollide = true;
            AIType = -3;

            AnimationType = -3;

            NPC.boss = true;
            currentPhase = phase.INIT;

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Sound/Music/CanonRockCut");
            }
        }

        public override void AI()
        {
            // frame animation
            if ((++NPC.frameCounter) % 10 == 0)
                NPC.frameCounter = 0;

            // seek target
            if (target == null)
            {
                target = EnemyInterface1.seekTarget(NPC.Center, MAX_DISTANCE);
            }

            if (target != null) {
                Main.NewText(target);

                switch (currentPhase) {
                    case phase.INIT: {
                            init();
                            break;
                        }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (int)(NPC.frameCounter/10 * Main.npcFrameCount[NPC.type]) * NPC.height;
        }

        private void init()
        {
            
        }

        private void hover(Vector3 hoverCenter, float hoverRadius) {

        }
    }
}
