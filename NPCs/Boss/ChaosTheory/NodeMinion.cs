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
using Terraria.Graphics;
using PhysicsBoss.Projectiles.TrailingStarMotion;

namespace PhysicsBoss.NPCs.Boss.ChaosTheory
{
    public abstract class NodeMinion: TargetEnemy
    {
        public const int ORBIT_DIST = 25;
        public const int ORBIT_PERIOD = (int)(5 * 60);

        protected Texture2D tex;
        protected ChaosTheory owner;
        protected int currentPhase;
        protected bool drawConnection;
        protected ModNPC connectionTarget;
        protected Texture2D beamTex;
        protected Texture2D trailTex;
        protected Color contourColor;
        protected Color baseColor;

        protected bool onSummon;

        protected TrailingStarController trailingStarController;

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
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.HitSound = SoundID.NPCHit4;

            drawTrail = trail.DEFAULT;
            drawConnection = false;
            connectionTarget = null;
            onSummon = false;

            beamTex = ModContent.Request<Texture2D>("PhysicsBoss/Asset/Beam").Value;
            trailTex = tex;

            contourColor = baseColor = Color.White;
        }

        public void setOwner(ChaosTheory o) { 
            owner = o;
            NPC.realLife = o.NPC.whoAmI;
            NPC.lifeMax = o.NPC.lifeMax;
            NPC.life = o.NPC.life;
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

            base.AI();
        }

        protected abstract void summonEvent();

        public void orbit(float degree, float radius = ORBIT_DIST)
        {
            NPC.velocity *= 0;
            if (owner != null && owner.NPC.active) {
                // return to position
                Vector2 aim = owner.NPC.Center + Vector2.UnitY.RotatedBy(degree)
                    * (radius + owner.NPC.width/2 + NPC.width/2);

                if (aim.Distance(NPC.Center) > 0.3 * (float)radius)
                {
                    if (aim.Distance(NPC.Center) > ChaosTheory.MAX_DISTANCE)
                        NPC.Center = aim;
                    else
                        NPC.Center = 0.7f * NPC.Center + 0.3f * aim;
                }
                else {
                    NPC.Center = aim;
                }
            }
        }

        public void setDrawConnection(bool b)
        {
            drawConnection = b;
        }

        public void setConnectionTarget(ModNPC t)
        {
            connectionTarget = t;
        }

        protected void drawConnectionLine(SpriteBatch spriteBatch, Color color, float width) {
            if (connectionTarget == null || !drawConnection)
                return;

            Vector2 targetPos = connectionTarget.NPC.Center;

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

        protected void drawTail(SpriteBatch spriteBatch, Color tint)
        {
            Dust d = Dust.NewDustDirect(NPC.position, tex.Width, tex.Height, DustID.RainbowMk2);
            d.noGravity = true;
            d.color = baseColor;

            VertexStrip tail = new VertexStrip();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.Textures[0] = trailTex;
            //ModContent.Request<Texture2D>("PhysicsBoss/Effects/Materials/FNBlock").Value;
            tail.PrepareStrip(NPC.oldPos, NPC.oldRot,
                progress => Color.White, progress => tex.Width * (1f - progress),
                tex.Size() / 2 - Main.screenPosition, NPC.oldPos.Length);

            PhysicsBoss.worldEffect.Parameters["extractThreshold"].SetValue(0.3f);
            PhysicsBoss.worldEffect.Parameters["extractMin"].SetValue(0.95f);
            PhysicsBoss.worldEffect.Parameters["extractTint"].SetValue(contourColor.ToVector4());
            PhysicsBoss.worldEffect.CurrentTechnique.Passes["ExtractRangeTint"].Apply();
            tail.DrawTrail();

            
            PhysicsBoss.worldEffect.Parameters["extractThreshold"].SetValue(0.45f);
            PhysicsBoss.worldEffect.Parameters["extractMin"].SetValue(0.8f);
            PhysicsBoss.worldEffect.Parameters["extractTint"].SetValue(baseColor.ToVector4());
            PhysicsBoss.worldEffect.CurrentTechnique.Passes["ExtractRangeTint"].Apply();
            tail.DrawTrail();
            
            Main.spriteBatch.End();
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
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnKill()
        {
            if (trailingStarController != null)
            {
                trailingStarController.Projectile.Kill();
                trailingStarController = null;
            }

            base.OnKill();
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            if (projectile.type == ProjectileID.FallingStar)
            {
                projectile.Kill();
            }
        }

        protected void hyperbolicMotion(float totalPeriod)
        {
            float progress;
            if (Timer % (2 * totalPeriod) > totalPeriod)
                progress = 2 * (totalPeriod - (Timer % totalPeriod)) / totalPeriod - 1;
            else
                progress = 2 * (Timer % totalPeriod) / totalPeriod - 1;

            float altitude = 500 * progress;
            float radius = (float)(progress * progress) * 400 + 200;
            float innerAngle = (float)(MathHelper.TwoPi * ((Timer / (0.2 * totalPeriod)) % 1f));

            Vector3 realPos = new Vector3(
                radius * (float)Math.Cos(innerAngle),
                altitude,
                radius * (float)Math.Sin(innerAngle));

            float adjustment = (float)Math.Atan(realPos.Z) / (MathHelper.PiOver2) / 400;

            NPC.Center = new Vector2(
                target.Center.X + realPos.X * (1f + adjustment),
                target.Center.Y + realPos.Y * (1f + adjustment));
        }

        protected void horizontalShift(bool left = true, float startDist = 450f, float range = 600f, float period = 60f) {
            float dispX = (float)(startDist + (Math.Sin(Timer / period * MathHelper.TwoPi) + 1)* 0.5f * range);
            NPC.Center = new Vector2(owner.NPC.Center.X + (left ? -1: 1) * dispX, owner.NPC.Center.Y);
        }
    }
}
