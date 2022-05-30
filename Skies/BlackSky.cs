using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;

namespace PhysicsBoss.Skies
{
    public class BlackSky : CustomSky
    {
        private bool active;
        private float progress = 0f;
        public override void Activate(Vector2 position, params object[] args)
        {
            active = true;
            progress = 0f;
        }

        public override void Deactivate(params object[] args)
        {
            active = false;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), 
                Color.Black);
        }

        public override bool IsActive()
        {
            return active || progress > 0f;
        }

        public override void Reset()
        {
            active = false;
        }

        public override void Update(GameTime gameTime)
        {
            Main.NewText("Updated");
            if (active && progress < 1f)
                progress += 1f / 60f;
            else if (!active && progress > 0)
                progress -= 1f / 60f;
        }
    }
}
