using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsBoss.NPCs.Boss.ChaosTheory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace PhysicsBoss.Skies
{
    public class OpenTheGate : CustomSky
    {
        public const float PERIOD = 8 * 60;
        private float progress = 0f;
        private bool active;
        private int bossIndex = -1;
        public override void Activate(Vector2 position, params object[] args)
        {
            active = true;
            bossIndex = (int)args[0];
            progress = 0;
        }

        public override void Deactivate(params object[] args)
        {
            progress = 0f;
            bossIndex = -1;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            throw new NotImplementedException();
        }

        public override bool IsActive()
        {
            return active || progress > 0;
        }

        public override void Reset()
        {
            bossIndex = -1;
            active = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (active) {
                if (bossIndex >= 0 && (!Main.npc[bossIndex].active || Main.npc[bossIndex].life <= 0
                    || Main.npc[bossIndex].type != ModContent.NPCType<ChaosTheory>())) {
                    active = false;
                    bossIndex = -1;
                }
            }

            if (active && progress < 1f)
                progress += 1f / PERIOD;
            else if (!active && progress > 0)
                progress -= 0.05f;
        }
    }
}
