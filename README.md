radplatformer-tileless
======================

This is a 360° platformer inspired by the old Sonic the Hedgehog games. It has tileless at the end because it is a deviation from my other radplatformer repo, basing collision off of Unity's 2D colliders rather than rolling its own mask data based on StH's programming.

This does have drawbacks in that the curves can't be represented as well with polygon and circle colliders. To counteract this, the player controller combines the data of two surface sensors to simulate a surface similar to that of a bézier curve.

Right now the engine simply handles physics between the player and anything that has the tag "Terrain" and a 2D collider.
