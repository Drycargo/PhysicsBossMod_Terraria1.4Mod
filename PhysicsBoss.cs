using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria;
using PhysicsBoss.Effects;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent;
using PhysicsBoss.Skies;

namespace PhysicsBoss
{
	public class PhysicsBoss : Mod
	{
        public static RenderTarget2D tempRender;

        public static Effect trailingEffect;
        public static Effect shineEffect;
        public static Effect maskEffect;
        public static Effect worldEffect;

        private static PhysicsBoss instance;

        public static Texture2D galaxyTex;
        public static Texture2D fractalTex;

        public static SoundStyle weakTing;
        public static SoundStyle weakClang;
        public PhysicsBoss()
        {
            instance = this;
        }

        public static PhysicsBoss Instance
        {
            get;
        }
        public override void Load()
        {
            loadEffects();

            base.Load();
        }

        // same contents as load
        public override void PostSetupContent()
        {
            loadEffects();
            base.PostSetupContent();
        }

        private void loadEffects()
        {
            // instance effects
            trailingEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Trailing").Value;
            shineEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Shine").Value;
            maskEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/Mask").Value;

            // world effects
            worldEffect = ModContent.Request<Effect>("PhysicsBoss/Effects/Content/WorldM").Value;
            Filters.Scene["PhysicsBoss:Inverse"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "Inverse"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Shake"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "Shake"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Blur"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "GaussBlur"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Bloom"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "BlurOnThreshold"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:BlurH"] = new Filter(
                new ScreenShaderApplier(new Ref<Effect>(worldEffect), "BlurThresholdH"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:BlurV"] = new Filter(
                 new ScreenShaderApplier(new Ref<Effect>(worldEffect), "BlurThresholdV"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:CenterTwist"] = new Filter(
                 new ScreenShaderApplier(new Ref<Effect>(worldEffect), "CenterTwist"), EffectPriority.Medium);
            Filters.Scene["PhysicsBoss:Vignette"] = new Filter(
                 new ScreenShaderApplier(new Ref<Effect>(worldEffect), "SimpleVignette"), EffectPriority.Medium);

            Filters.Scene["PhysicsBoss:Inverse"].Load();
            Filters.Scene["PhysicsBoss:Shake"].Load();
            Filters.Scene["PhysicsBoss:Blur"].Load();
            Filters.Scene["PhysicsBoss:Bloom"].Load();
            Filters.Scene["PhysicsBoss:BlurH"].Load();
            Filters.Scene["PhysicsBoss:BlurV"].Load();
            Filters.Scene["PhysicsBoss:CenterTwist"].Load();
            Filters.Scene["PhysicsBoss:Vignette"].Load();

            CameraPlayer.deActivate();

            SkyManager.Instance["PhysicsBoss:BlackSky"] = new BlackSky();
            SkyManager.Instance["PhysicsBoss:BlackSky"].Load();

            SkyManager.Instance["PhysicsBoss:OpenTheGate"] = new OpenTheGate();
            SkyManager.Instance["PhysicsBoss:OpenTheGate"].Load();

            SkyManager.Instance["PhysicsBoss:ColorSky"] = new ColorSky();
            SkyManager.Instance["PhysicsBoss:ColorSky"].Load();

            SkyManager.Instance["PhysicsBoss:HindSky"] = new HindSky();
            SkyManager.Instance["PhysicsBoss:HindSky"].Load();

            // render target
            On.Terraria.Graphics.Effects.FilterManager.EndCapture += FilterManager_EndCapture;
            Main.OnResolutionChanged += Main_OnResolutionChanged;

            galaxyTex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LogisticMap/BlockMap").Value;
            fractalTex = ModContent.Request<Texture2D>("PhysicsBoss/Projectiles/LogisticMap/FractalMap").Value;

            weakTing = SoundID.Item25;
            weakTing.Volume *= 0.35f;

            weakClang = SoundID.NPCHit4;
            weakClang.Volume *= 0.25f;
        }

        private void Main_OnResolutionChanged(Vector2 obj)
        {
            createRenderTarget();
        }

        private void createRenderTarget()
        {
            tempRender = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        }

        private void FilterManager_EndCapture(On.Terraria.Graphics.Effects.FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            
            GlobalEffectController.Main_applyBloom();
            GlobalEffectController.Main_applyFlash();
            GlobalEffectController.Main_applyVignette();
            GlobalEffectController.Main_applyCenterTwist();
            GlobalEffectController.drawSpecialDusts();

            orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }
        
        public override void Unload()
        {
            trailingEffect = null;
            shineEffect = null;
            worldEffect = null;
            maskEffect = null;

            Filters.Scene.Deactivate("PhysicsBoss:Inverse");
            Filters.Scene.Deactivate("PhysicsBoss:Shake");
            Filters.Scene.Deactivate("PhysicsBoss:Blur");
            Filters.Scene.Deactivate("PhysicsBoss:Bloom");
            Filters.Scene.Deactivate("PhysicsBoss:BlurH");
            Filters.Scene.Deactivate("PhysicsBoss:BlurV");
            Filters.Scene.Deactivate("PhysicsBoss:CenterTwist");
            Filters.Scene.Deactivate("PhysicsBoss:Vignette");

            SkyManager.Instance.Deactivate("PhysicsBoss:BlackSky");
            SkyManager.Instance.Deactivate("PhysicsBoss:OpenTheGate");
            SkyManager.Instance.Deactivate("PhysicsBoss:ColorSky");
            SkyManager.Instance.Deactivate("PhysicsBoss:HindSky");

            On.Terraria.Graphics.Effects.FilterManager.EndCapture -= FilterManager_EndCapture;
            Main.OnResolutionChanged -= Main_OnResolutionChanged;

            base.Unload();
        }
    }

}