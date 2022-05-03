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

namespace PhysicsBoss.Projectiles.TrailingStarMotion
{
    public class TrailingStarChua: TrailingStarChaotic
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trailing Star Chua");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "拖尾星-蔡氏电路");
            base.SetStaticDefaults();
        }

        protected override void motionUpdate()
        {
            float a = 36, b = 3, c = 20, u = -15.15f;
            float x = realCenter.X, y = realCenter.Y, z = realCenter.Z;
            Vector3 realVel = new Vector3(
                a*(y - x),
                (1 - z) * x + c * y + u,
                x*y - b*z);

            float speed = realVel.Length();

            if (speed > SPEED_LIMIT) {
                realVel *= (SPEED_LIMIT / speed);
            }

            realCenter += realVel;
            
            base.motionUpdate();
        }
    }
}
