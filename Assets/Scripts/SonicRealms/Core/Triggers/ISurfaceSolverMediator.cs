using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    public interface ISurfaceSolverMediator
    {
        /// <summary>
        /// The controller that is querying the current solver.
        /// </summary>
        HedgehogController Controller { get; }

        /// <summary>
        /// The position of the controller at the moment of querying the current solver.
        /// </summary>
        Vector2 CurrentPosition { get; }

        /// <summary>
        /// Whether the controller is currently facing forward.
        /// </summary>
        bool IsFacingForward { get; }

        /// <summary>
        /// The translation amount that the controller would move under normal physics conditions.
        /// </summary>
        Vector2 ProposedTranslation { get; }

        /// <summary>
        /// The position that the controller would end up at under normal physics conditions.
        /// </summary>
        Vector2 ProposedPosition { get; }

        /// <summary>
        /// Fills in the values required for a valid solution.
        /// </summary>
        /// <param name="position">What the controller's position should be..</param>
        /// <param name="surface">What the controller's surface should be.</param>
        /// <param name="surfaceAngle">What the controller's surface angle should be, in degrees.</param>
        void SetAll(Vector2 position, Transform surface, float surfaceAngle);

        /// <summary>
        /// Sets what the controller's new position should be.
        /// </summary>
        void SetPosition(Vector2 position);

        /// <summary>
        /// Sets what the controller's surface should be.
        /// </summary>
        void SetSurface(Transform surface);

        /// <summary>
        /// Sets what the controller's surface angle should be, in degrees.
        /// </summary>
        void SetSurfaceAngle(float surfaceAngle);

        /// <summary>
        /// Signals that the controller should leave the surface given by the current surface solver.
        /// </summary>
        void Detach();
    }
}
