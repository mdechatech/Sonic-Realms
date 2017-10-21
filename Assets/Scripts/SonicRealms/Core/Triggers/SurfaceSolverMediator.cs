using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    public class SurfaceSolverMediator : ISurfaceSolverMediator
    {
        private HedgehogController _controller;

        private Vector2 _proposedTranslation;
        private Vector2? _position;

        private Transform _surface;
        private float? _surfaceAngle;

        private bool _calledDetach;

        // Inbound values
        public HedgehogController Controller { get { return _controller; } }
        public bool IsFacingForward { get { return Controller.IsFacingForward; } }
        public Vector2 CurrentPosition { get { return Controller.transform.position; } }
        public Vector2 ProposedTranslation { get { return _proposedTranslation; } }
        public Vector2 ProposedPosition { get { return CurrentPosition + ProposedTranslation; } }

        // Outbound values
        public Vector2 Position { get { return _position.GetValueOrDefault(); } }
        public Transform Surface { get { return _surface; } }
        public float SurfaceAngle { get { return _surfaceAngle.GetValueOrDefault(); } }

        public bool CalledDetach { get { return _calledDetach; } }

        public SurfaceSolverMediator(HedgehogController controller)
        {
            _controller = controller;
        }

        public void SetAll(Vector2 position, Transform surface, float surfaceAngle)
        {
            SetPosition(position);
            SetSurface(surface);
            SetSurfaceAngle(surfaceAngle);
        }

        public void Reset(Vector2 proposedTranslation)
        {
            _proposedTranslation = proposedTranslation;

            _calledDetach = false;

            _position = null;
            _surface = null;
            _surfaceAngle = null;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
        }

        public void SetSurface(Transform surface)
        {
            _surface = surface;
        }

        public void SetSurfaceAngle(float surfaceAngle)
        {
            _surfaceAngle = SrMath.PositiveAngle_d(surfaceAngle);
        }

        public void Detach()
        {
            _calledDetach = true;
        }

        public ValidationError Validate()
        {
            var error = ValidationError.None;

            if (_calledDetach)
                return error;

            if(!_position.HasValue)
                error |= ValidationError.Position;

            if (_surface == null)
                error |= ValidationError.Surface;

            if (!_surfaceAngle.HasValue)
                error |= ValidationError.SurfaceAngle;

            return error;
        }

        [Flags]
        public enum ValidationError
        {
            None = 0,
            Position = 1 << 0,
            Surface = 1 << 1,
            SurfaceAngle = 1 << 2,
        }
    }
}
