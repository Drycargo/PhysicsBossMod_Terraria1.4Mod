﻿using Terraria.Graphics.Effects;
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
        public const int ORBIT_DIST = 50;
        public const int ORBIT_PERIOD = (int)(5 * 60);

        protected Texture2D tex;
        protected ChaosTheory owner;
        protected int currentPhase;
        protected bool drawConnection;
        protected Texture2D beamTex;

        protected bool onSummon;

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
            NPC.HitSound = SoundID.NPCHit4;

            drawTrail = trail.DEFAULT;
            drawConnection = false;
            onSummon = false;

            beamTex = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Beam").Value;
        }

        public void setOwner(ChaosTheory o) { 
            owner = o;
            NPC.realLife = o.NPC.whoAmI;
            NPC.dontTakeDamage = false;
            NPC.lifeMax = o.NPC.lifeMax;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            base.HitEffect(hitDirection, damage);
        }

        public void setPhase(int phase) { 
            currentPhase = phase;
        }
        public override void AI()
        {
            if (onSummon == false) {
                summonEvent();
                onSummon = true;
            }

            //if (NPC.)
            base.AI();
        }

        protected abstract void summonEvent();

        public void orbit(float degree)
        {
            if (owner != null && owner.NPC.active) {
                // return to position
                Vector2 aim = owner.NPC.Center + Vector2.UnitY.RotatedBy(degree)
                    * (ORBIT_DIST + owner.NPC.width/2 + NPC.width/2);

                if (aim.Distance(NPC.Center) > 0.2 * (float)ORBIT_DIST)
                {
                    NPC.Center = 0.8f * NPC.Center + 0.2f * aim;
                }
                else {
                    NPC.velocity *= 0;
                    NPC.Center = aim;
                }
            }
        }

        public void setDrawConnection(bool b)
        {
            drawConnection = b;
        }

        protected void drawConnectionLine(SpriteBatch spriteBatch,Vector2 targetPos, Color color, float width) {
            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            PhysicsBoss.shineEffect.Parameters["shineColor"].SetValue(color.ToVector4());
            PhysicsBoss.shineEffect.Parameters["threashold"].SetValue(0.9f);
            PhysicsBoss.shineEffect.CurrentTechnique.Passes["Beam"].Apply();

            spriteBatch.Draw(beamTex, (NPC.Center + targetPos) / 2 - Main.screenPosition, 
                null, Color.White, (NPC.Center - targetPos).ToRotation() + MathHelper.PiOver2, beamTex.Size() / 2f,
                new Vector2(width / (float)beamTex.Width, (NPC.Center - targetPos).Length() / (float)beamTex.Height), SpriteEffects.None, 0);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        protected void drawShadow(SpriteBatch spriteBatch, Color tint) {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            int l = NPC.oldPos.Length;
            for (int i = l - 1; i >= 0; i--)
            {
                if (NPC.oldPos[i] != Vector2.Zero)
                {
                    Color c = tint * (1f - (float)i / l) * 0.5f;
                    spriteBatch.Draw(tex, NPC.oldPos[i] - Main.screenPosition, new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height), c);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
