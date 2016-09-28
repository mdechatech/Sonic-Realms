namespace SonicRealms.Core.Triggers
{
    public interface ITriggerLimiter<in T>
    {
        bool Allows(T contact);
    }
}
