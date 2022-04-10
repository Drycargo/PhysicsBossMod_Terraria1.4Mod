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
using Terraria.DataStructures;

namespace PhysicsBoss.NPC.Boss.ChaosTheory
{
    public abstract class NodeMinion: TargetEnemy
    {

        protected ChaosTheory owner;
        protected int currentPhase;
        protected bool drawConnection;

        protected enum trail {
            DEFAULT = 0,
            SHADOW = 1,
            TAIL = 2
        }
        
        protected trail drawTrail;

        public float Timer {
            get { return NPC.ai[0];}
            set { NPC.ai[0] = value;} 
        }

        public override void SetDefaults()
        {
           // GlobalNPC
            base.SetDefaults();
            NPC.friendly = false;
            NPC.dontTakeDamage = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            drawTrail = trail.DEFAULT;
            drawConnection = false;
        }

        public void setOwner(ChaosTheory o) { 
            owner = o;
        }

        public void setPhase(int phase) { 
            currentPhase = phase;
        }
        public override void AI()
        {
            base.AI();
        }
    }
}
