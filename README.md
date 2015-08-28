Unity the Hedgehog
======================

Under the [CC BY](https://creativecommons.org/licenses/by/4.0/) license.

This is a 360° platformer inspired by the old Sonic the Hedgehog games. It uses Unity's 2D colliders unlike the [other repo](https://github.com/mdechatech/radplatformer)

Right now the engine simply handles physics between the player and anything that has the layer "Terrain" and a 2D collider. Platform layering has been implemented with the path switcher prefab and numbered terrain layers. It's crude but it works well. 

Working on moving platforms right now. Using Unity's transform system it should be possible to implement sloped moving platforms, something the Genesis engine didn't have.

Oh yeah, and the algorithm for ground velocity on landing is too stiff right now. This should be a priority.
