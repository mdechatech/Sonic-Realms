using UnityEngine;

namespace Hedgehog.Core.Utils
{
    /// <summary>
    /// Helpers that wrap around the Physics2D class.
    /// </summary>
    public static class Physics2DUtility
    {
        #region Raycast Allocation Variables
        private const int MaxRaycastResults = 16;
        private static readonly RaycastHit2D[] RaycastResults = new RaycastHit2D[MaxRaycastResults];

        private static int _previousRaycastResultAmount;
        private static int _raycastResultAmount = 0;
        #endregion
        
        /// <summary>
        /// Calls LinecastNonAlloc always using the same allocated array.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <param name="layerMask">Which layers to cast against.</param>
        /// <returns></returns>
        public static RaycastHit2D[] LinecastNonAlloc(Vector2 start, Vector2 end,
            int layerMask = Physics2D.DefaultRaycastLayers)
        {
            _previousRaycastResultAmount = _raycastResultAmount;
            _raycastResultAmount = Physics2D.LinecastNonAlloc(start, end, RaycastResults, layerMask);

            if (_raycastResultAmount < _previousRaycastResultAmount)
            {
                for (var i = _raycastResultAmount;
                    i < _previousRaycastResultAmount;
                    i++)
                {
                    RaycastResults[i] = default(RaycastHit2D);
                }
            }

            return RaycastResults;
        }
    }
}
