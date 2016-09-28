using SonicRealms.Core.Actors;

namespace SonicRealms.Core.Triggers
{
    public interface ISurfaceSolver
    {
        void Solve(ISurfaceSolverMediator mediator);
        void OnSolverAdded(HedgehogController controller);
        void OnSolverRemoved(HedgehogController controller);
    }
}
