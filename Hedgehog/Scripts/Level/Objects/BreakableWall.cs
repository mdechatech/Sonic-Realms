﻿using System.Collections.Generic;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
{
    /// <summary>
    /// A wall that can be broken by rolling into it. Imitates the walls in Green Hill Zone.
    /// </summary>
    public class BreakableWall : ReactivePlatform
    {
        /// <summary>
        /// Minimum absolute ground speed to break the wall.
        /// </summary>
        [SerializeField]
        public float MinGroundSpeed;

        /// <summary>
        /// Duration of the freeze frame when the wall is broken, in seconds.
        /// </summary>
        [SerializeField]
        public float FreezeTime;

        public override bool ActivatesObject
        {
            get { return true; }
        }

        public void Reset()
        {
            MinGroundSpeed = 2.7f;
            FreezeTime = 0.03333333f;
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            if (hit.Controller == null || !hit.Controller.Grounded) return;
            if (hit.Side == ControllerSide.Bottom || hit.Side == ControllerSide.Top) return;
            if (!hit.Controller.IsActive<Roll>() || Mathf.Abs(hit.Controller.GroundVelocity) < MinGroundSpeed) return;

            hit.Controller.IgnoreNextCollision = true;
            ActivateObject(hit.Controller);
            Destroy(gameObject);

            if (FreezeTime <= 0.0f) return;
            hit.Controller.Interrupt(FreezeTime);
        }

        public override void OnPlatformStay(TerrainCastHit hit)
        {
            OnPlatformEnter(hit);
        }
    }
}