namespace _Project.Code.Scripts.Game
{
    public interface IManualUpdateRegistrar
    {
        void Register(IManualUpdate manualUpdate);
        void Unregister(IManualUpdate manualUpdate);
    }
}