using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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

        public ConwayBlock(int r, int c, float prog) {
            progress = prog;

            mid = (-1 <= r && r <= 1 && -1 <= c && c <= 1);
            currPhase = phase.INITIALIZING;
            toSet = Main.rand.NextBool()? phase.LIVE : phase.DEAD;

            drawPos = new Vector2(((float)c - 0.5f) * tex.Width, ((float)r - 0.5f) * tex.Height);
        }

        public void drawBlock1(Vector2 origin) {
            float factor = mid ? 0.4f : 2f;
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
                        baseColor = Color.DarkBlue;
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
                Main.spriteBatch.Draw(tex, origin + drawPos, Color.DarkRed * (timer % 7 < 3.5 ? 0.7f : 0.2f));
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
