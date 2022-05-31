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
    public class SkyUnloadSystem:ModSystem
    {
        public override void OnWorldLoad()
        {
            base.OnWorldLoad();
        }

        public override void PreSaveAndQuit()
        {
            CameraPlayer.deActivate();
            SkyManager.Instance.Reset();
            SkyManager.Instance.Deactivate("PhysicsBoss:BlackSky");
            SkyManager.Instance.Deactivate("PhysicsBoss:OpenTheGate");
            base.PreSaveAndQuit();
        }
        public override void OnWorldUnload()
        {
            CameraPlayer.deActivate();
            SkyManager.Instance.Deactivate("PhysicsBoss:BlackSky");
            SkyManager.Instance.Deactivate("PhysicsBoss:OpenTheGate");
            base.OnWorldUnload();
        }
    }
}
