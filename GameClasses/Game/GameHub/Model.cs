namespace BoardGameBackend.Managers
{
    public interface IEventListener
    {
        void SubscribeEvents(GameContext gameContext);
    }
}