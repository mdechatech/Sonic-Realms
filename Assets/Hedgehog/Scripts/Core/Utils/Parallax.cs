using System;
using UnityEngine;

namespace Hedgehog.Core.Utils
{
    public class Parallax : MonoBehaviour
    {
        public bool PixelSnap;
        public Camera Target;

        public Vector2 Center;

        public Vector2 BottomLeftWorld;
        public Vector2 BottomLeftParallax;

        public Vector2 TopRightWorld;
        public Vector2 TopRightParallax;

        public void Reset()
        {
            PixelSnap = true;
            Target = Camera.main;
            BottomLeftWorld = new Vector2(0.0f, 0.0f);
            BottomLeftParallax = new Vector2(0.0f, 0.0f);

            TopRightWorld = new Vector2(1.0f, 1.0f);
            TopRightParallax = new Vector2(0.1f, 1.0f);
        }

        public void Awake()
        {
            Target = Target ? Target : Camera.main;
        }

        public void FixedUpdate()
        {
            if (!Target) return;

            var pos = new Vector2();

            var worldSize = new Vector2(TopRightWorld.x - BottomLeftWorld.x, TopRightWorld.y - BottomLeftWorld.y);
            var ratioX = worldSize.x == 0.0f
                ? 0.0f
                : (Target.transform.position.x - BottomLeftWorld.x)/
                  (TopRightWorld.x - BottomLeftWorld.x);
            var ratioY = worldSize.y == 0.0f
                ? 0.0f
                : (Target.transform.position.y - BottomLeftWorld.y)/
                  (TopRightWorld.y - BottomLeftWorld.y);

            var offsetX = ratioX*(TopRightParallax.x - BottomLeftParallax.x) + BottomLeftParallax.x;
            var offsetY = ratioY*(TopRightParallax.y - BottomLeftParallax.y) + BottomLeftParallax.y;

            pos = new Vector2(
                DMath.Flip(Target.transform.position.x + offsetX, Target.transform.position.x),
                DMath.Flip(Target.transform.position.y + offsetY, Target.transform.position.y));
            
            transform.position = pos;
            if (PixelSnap)
                transform.position = new Vector3(
                    DMath.Round(transform.position.x, 0.01f),
                    DMath.Round(transform.position.y, 0.01f));
        }
    }
}
