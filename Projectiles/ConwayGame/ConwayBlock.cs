using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PhysicsBoss.Projectiles.ConwayGame
{
    public class ConwayBlock
    {
        public enum phase { 
            INITIALIZING = 0,
            LIVE = 1,
            DEAD = 2,
        }

        private phase currPhase;
        private phase toSet;

        private float progress;

        private bool mid;
        private Texture2D tex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/ConwayGame/ConwayBlock").Value;

        private Vector2 drawPos = Vector2.Zero;
        private int row, col;

        public ConwayBlock(int r, int c, float prog) {
            progress = prog;

            mid = (-1 <= r && r <= 1 && -1 <= c && c <= 1);
            currPhase = phase.INITIALIZING;
            toSet = (Main.rand.NextFloat() < 2.5f/8f)? phase.LIVE : phase.DEAD;

            row = r;
            col = c;
            drawPos = new Vector2(((float)c - 0.5f) * tex.Width, ((float)r - 0.5f) * tex.Height);
        }

        public void launch(Projectile parent) {
            if (mid || currPhase != phase.LIVE)
                return;

            for (int i = 0; i < 30; i++)
            {
                float a = 2f * (Main.rand.NextFloat() - 0.5f) * 0.4f * tex.Width/2;
                float b = (Main.rand.NextBool() ? 1 : -1) * 0.4f * tex.Width / 2;

                float x, y;

                if (Main.rand.NextBool())
                {
                    x = a; y = b;
                }
                else
                {
                    x = b; y = a;
                }

                Dust d = Dust.NewDustDirect(new Vector2(x, y) + parent.Center + drawPos + tex.Size()/2, 0,0,DustID.FlameBurst);
                d.noGravity = true;
                d.velocity = -5f * Vector2.UnitY;
            }

            float direction = 0f;
            /* //include diagonal
            if ((row <= -2 && col <= -2) || (row >= 2 && col >= 2))
            {
                direction = (row <= 2 ? 0 : MathHelper.Pi) + MathHelper.PiOver4;
            }
            else if ((row <= -2 && col >= 2) || (row >= 2 && col <= -2))
            {
                direction = (row <= -2 ? 0 : MathHelper.Pi) - MathHelper.PiOver4;
            }
            else if (col <= -2 || col >= 2)
            {
                direction = (col <= -2 ? 0 : MathHelper.Pi);
            }
            else {
                direction = (row <= -2 ? 0 : MathHelper.Pi) + MathHelper.PiOver2;
            }*/

            if (row <= -2 && col >= -1)
            {
                direction = MathHelper.PiOver2;
            }
            else if (row >= 2 && col <= 1)
            {
                direction = -MathHelper.PiOver2;
            }
            else if (row >= -1 && col >= 2) {
                direction = MathHelper.Pi;
            }

            Projectile.NewProjectile(parent.GetSource_FromThis(), parent.Center + drawPos + tex.Size()/2, 
                -direction.ToRotationVector2() * ConwayGlider.INIT_SPEED, ModContent.ProjectileType<ConwayGlider>(), 25, 0);
            
        }

        public void drawBlock1(Vector2 origin) {
            float factor = mid ? 0.5f : 1.2f;
            Color baseColor = Color.Ivory;

            switch (currPhase)
            {
                case phase.INITIALIZING:
                    {
                        factor *= (progress < 0 ? 0 : progress);
                        baseColor = Color.Ivory;
                        break;
                    }
                case phase.LIVE:
                    {
                        baseColor = Color.Green;
                        break;
                    }
                case phase.DEAD:
                    {
                        baseColor = Color.Ivory;
                        break;
                    }
                default: break;
            }

            baseColor *= factor;

            Main.spriteBatch.Draw(tex, origin + drawPos, baseColor);
        }

        public void drawBlock2(Vector2 origin, float timer)
        {
            if (toSet == phase.LIVE)
            {
                bool factor = timer % 7 < 3.5;
                Main.spriteBatch.Draw(tex, origin + drawPos, 
                    Color.DarkRed * (factor ? 1.2f : 0.2f) * (mid ? 0.5f : 1f));
            } else
            {
                drawBlock1(origin);
            }
        }

        public void incProgress(float inc) {
            progress += inc;
            if (progress > 1f)
                progress = 1f;
        }

        public float getProgress() { 
            return progress;
        }

        public phase getPhase()
        {
            return currPhase;
        }
        public void setPhase()
        {
            currPhase = toSet;
        }
        public void setPhase(phase p) { 
            currPhase = p;
        }

        public void setPrePhase(phase p)
        {
            toSet = p;
        }

        public bool alive() {
            return currPhase == phase.LIVE;
        }

        public bool isMid()
        {
            return mid;
        }
            
    }
}
