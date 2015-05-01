unity-the-hedgehog
======================

This is a 360° platformer inspired by the old Sonic the Hedgehog games. It has tileless at the end because it is a deviation from my other radplatformer repo, basing collision off of Unity's 2D colliders rather than rolling its own mask data based on StH's programming.

Right now the engine simply handles physics between the player and anything that has the layer "Terrain" and a 2D collider. Platform layering has been implemented with the path switcher prefab and numbered terrain layers. It's crude but it works well. 

Working on moving platforms right now. Using Unity's transform system it should be possible to implement sloped moving platforms. Yes, soon you'll be going up a loop THAT MOVES!!

Oh yeah, and the algorithm for ground velocity on landing is too stiff right now. This should be a priority.
