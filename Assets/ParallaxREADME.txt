NEBULA - Orthographic Parallax
------------------------------

open Assets/GalaxyExample.unity to view the sample project.

Here you'll find seven layers of Quad GameObjects with each one a ParallaxQuad script attached to it.
Note the increasing parallax setting values:

0 means non moving, thus object that are far back (in example the sky)
1 means normal distance, thus representing the distance that are at the same level as your interactive game objects. 
use values lower than 1 to place layers away from the camera and behind your game level.
use values higher than 1 to bring layers to the front (in example the dust layer at parallax 2)

Best practice is to first define the standard orthographic size for your camera, then add quads to your stage, using the z position to order them. keep the quad squared to prevent texture streching. To alter texture scaling you can change the quads scale or use the material's texture tiling property. To alter the texture offset, you can change the quads x/y offset from the camera or use the material's texture offset property.  Use the Game window to preview the result. When you hit play, the textures will be converted to match the viewport but texture scaling and positioning will remain exactly the same, disregarding the parallax setting.
You can change the parallax value during runtime to test out different effects.

Texture settings:
Use tiled textures for seamless repetition, setup the wrap mode to 'repeat'. For using transparent textures, you can set the correct material shader, ie. Unlit/Transparent  or Transparent/Diffuse (to have some extra control on the texture's tint and alpha properties)

!important:
please setup the ParallaxQuad script to execute AFTER the camera updates.
You can do this by adding the script and placing it below the Default Time block in The Script Execution Order inspector view. (Edit->Project Settings->Script Execution Order).

*Notes
- The current version does not support rotational conversion of the camera (camera rolling).

