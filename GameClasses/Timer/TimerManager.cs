using System.Diagnostics;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class TimerManager
    {
        private Stopwatch _gameStopwatch;                          
        private Dictionary<Guid, TimeSpan> _playerTimes;                  
        private Stopwatch _playerStopwatch;   
        private GameContext _gameContext;                     

        public TimerManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            _gameStopwatch = new Stopwatch();
            _playerTimes = new Dictionary<Guid, TimeSpan>();
            _playerStopwatch = new Stopwatch();
            _gameStopwatch.Start();
        }

        public void ProceedTimerForPlayer(PlayerInGame player)
        {
            UpdatePlayerTime(player, _playerStopwatch.Elapsed);
        } 
        public void ResetTimer()
        {
            _playerStopwatch.Reset();
            _playerStopwatch.Start();
        }
        public void EndTimer()
        {
            _playerStopwatch.Reset();
        }
        private void UpdatePlayerTime(PlayerInGame player, TimeSpan elapsedTime)
        {
            if (!_playerTimes.ContainsKey(player.Id))
                _playerTimes[player.Id] = elapsedTime;
            else
                _playerTimes[player.Id] = _playerTimes[player.Id].Add(elapsedTime);
        }

        public TimeSpan GetPlayerTime(PlayerInGame player)
        {
            return _playerTimes.ContainsKey(player.Id) ? _playerTimes[player.Id] : TimeSpan.Zero;
        }

        public Dictionary<Guid, TimeSpan> GetPlayerTimes()
        {
            return _playerTimes;
        }

        public TimeSpan GetTotalGameTime()
        {
            return _gameStopwatch.Elapsed;
        }
    }
}