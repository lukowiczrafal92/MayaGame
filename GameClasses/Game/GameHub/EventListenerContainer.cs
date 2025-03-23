using BoardGameBackend.Providers;
using BoardGameBackend.Repositories;

namespace BoardGameBackend.Managers.EventListeners
{
    public class EventListenerContainer
    {
        private readonly List<IEventListener> _listeners = new();

        public EventListenerContainer(IHubContextProvider hubContextProvider)
        {
            _listeners.Add( new PhaseEventListener(hubContextProvider));
            _listeners.Add( new OtherEventListener(hubContextProvider));
        }

        public void SubscribeAll(GameContext gameContext)
        {
            foreach (var listener in _listeners)
            {
                listener.SubscribeEvents(gameContext);
            }
        }
    }
}