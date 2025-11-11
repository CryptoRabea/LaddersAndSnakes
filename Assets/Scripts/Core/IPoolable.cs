namespace LAS.Core
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}