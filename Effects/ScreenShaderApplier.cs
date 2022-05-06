using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;

namespace PhysicsBoss.Effects
{
    public class ScreenShaderApplier: ScreenShaderData
    {
        public ScreenShaderApplier(string passName) : base(passName)
        { }

        public ScreenShaderApplier(Ref<Effect> shader, string passName) : base(shader, passName) { }

        public override void Apply()
        {
            base.Apply();
        }
    }
}
