# PhysicsBossMod_Terraria1.4Mod
Mod of Physics-themed Bosses in Terraria, based on TModLoader

## Introduction
This is a **[TModLoader](https://github.com/tModLoader/tModLoader)** based mod for the game Terraria. 
The initial intention of designing this mod is to implement Physics-related simulations into gaming, 
but not necessarily in the form of educational display like **[phet](https://phet.colorado.edu/)**. 
As a game mod, it should merge Physical patterns (Chaos, Quantums, Electromagnetism, Relativity, etc) into 
the various AI patterns of a boss fight.  
 
As a developer, I am personally interested in Physics simulations and animation display. 
I have developed a **[Special Relativity Simulator](https://github.com/Drycargo/SpecialRelativitySimulator-JAVA)** 
before, but have seldom created or witnessed a game with large-scale Physics patterns implemented into boss fights. 
These years, one of the Terraria Mods, **[Polarities](https://terrariamods.fandom.com/wiki/Polarities_Mod)**, has 
successfully implemented Physics and Math concepts (e.g., Electromagnetism and Fractals) into the game procedure; somewhat
inspired, I would like to seize the chance to develop my own Physics simulation-themed mod as a demonstration of my 
passion in Physics (and possibly animation effects).  
 
*Currently, I do not intend to incorporate any background stories into this mod, nor do I intend to make it into a 
fully integrated mod with biomes, friendly NPC systems, comprehensive loots or events. This might subject to change.*

## Techniques
Based on TModLoader, this mod is implemented mainly with C#. The implementation of visual effects also require some basic 
shaders written in HLSL.

## Reference List
- **TModLoader and Mod Design Tutorials:**
	- [TModLoader Offical Tutorials](https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-developers)
		- [Migration Guide to TModLoader 1.4](https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide)
	- [Comprehensive Introductions to TModLoader, Game Design, Geometry, Draw spritebatch and Shader, etc (in Chinese)](https://fs49.org/)
	- [Basic, Step-By-Step Video Tutorial (in Chinese) by *papyrus*](https://space.bilibili.com/325032498/channel/collectiondetail?sid=48308)
- **Shader and Render To Target Tutorials:**
	- [Vertex Shaders and Other High Level Render Effects (in Chinese) by *DXTsT*](https://space.bilibili.com/38386290)
	- [RenderTarget2D (in Chinese) and Other Extremely Well-designed Visual Effects by *yiyang233*](https://space.bilibili.com/24132024)

## Contents, Design Experience and Current Progress
### Boss: Chaos Theory *(In Progress)*
This boss features projectile and attack patterns that refer to the [Chaos Theory](https://en.wikipedia.org/wiki/Chaos_theory): 
there are underlying patterns behind the seemingly random superficial phenomenon, but they are usually so hard to be generalize into 
deterministic formulae that we can only simulate them with step-by-step computer simulation. There are plenty of Physical simulations 
of Chaos Phenomenon; with proper restrictions, they can be implemented into boss fights that largely enhance the visual effects and 
playability with the element of randomness.  
 
I intend to implement the following Patterns into the boss fight:
- **Single Pendulum:** This is not necessarily a Chaos pattern, but it involves differential equations for solving a dynamic system. It is 
regarded as an intro to the boss fight;
- **Electro-static Forces:** Following the formula $k\frac{Q_1Q_2}{r^2}$