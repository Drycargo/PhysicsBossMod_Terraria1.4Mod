using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsBoss.NPCs.Boss.ChaosTheory
{
    public class BrightNodePhantom: TargetEnemy
    {
        public const int TRAILING_CONST = 10;

        private Texture2D tex;

        public float Timer
        {
            get { return NPC.ai[0]; }
            set { NPC.ai[0] = value; }
        }

        public override void SetStaticDefaults()
        {
            base.SetDefaults();
            DisplayName.SetDefault("Bright Node Phantom");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "明节点幻影");
            NPCID.Sets.TrailingMode[NPC.type] = 0;
            NPCID.Sets.TrailCacheLength[NPC.type] = TRAILING_CONST;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = 30;
            tex = ModContent.Request<Texture2D>(Texture).Value;
            NPC.friendly = false;

            SoundStyle s = SoundID.NPCHit4;
            s.Volume *= 0.15f;
            NPC.HitSound = s;
            NPC.DeathSound = SoundID.NPCDeath6;

            NPC.width = tex.Width;
            NPC.height = tex.Height;
            NPC.rotation = 0;

            NPC.lifeMax = 200;
            NPC.defense = 10;
            NPC.lifeRegen = 0;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 0, 0, 0);

            Timer = 0f;

            NPC.target = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.life = NPC.lifeMax - 2;
        }

        public override void AI()
        {
            NPC.rotation = NPC.velocity.ToRotation();

            if (target == null || !target.active || target.statLife <= 0)
                target = seekTarget(NPC.Center, 1200f);

            if (!(target == null || !target.active || target.statLife <= 0))
                NPC.velocity = 4f * (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);

            if (Timer > 120)
            {
                PreKill();
                NPC.life = 0;
                NPC.active = false;
            }

            Dust.NewDustDirect(NPC.Center, 0,0,DustID.RedMoss).noGravity = true;

            Timer++;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            PreKill();
            NPC.life = 0;
            NPC.active = false;
        }

        public override bool PreKill()
        {
            for (int i = 0; i < 30; i++) {
                Dust d = Dust.NewDustDirect(NPC.Center, 0,0, DustID.FireworkFountain_Red);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Unit() * 15f;
            }

            SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            for (int i = 0; i < TRAILING_CONST; i++)
                Main.spriteBatch.Draw(tex, NPC.oldPos[i] - screenPos + tex.Size() / 2, 
                    null, Color.DarkRed * 0.25f, NPC.oldRot[i],
                    tex.Size() / 2, 1, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(tex, NPC.Center - screenPos,null, Color.Red, NPC.rotation,
                tex.Size()/2, 1, SpriteEffects.None, 0);

            return false;
        }
    }
}
