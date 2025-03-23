using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class PhaseManager
    {
        private readonly GameContext _gameContext;
        private PhaseStartedEffectManager OnStartEffects;
    //    private PhaseEndedEffectManager OnEndEffects;
        public List<Phase> PhaseQueue = new List<Phase>();
        public int CurrentEra = 0;
        public int CurrentRound = 0;
        public int CurrentAction = 0;

        public PhaseManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            OnStartEffects = new PhaseStartedEffectManager(gameContext);
        //    OnEndEffects = new PhaseEndedEffectManager(gameContext);

            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.DummyPreStart, ActivePlayers = new List<Guid>()});
            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.EraStart, ActivePlayers = new List<Guid>()});
            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.EraEnd, ActivePlayers = new List<Guid>()});
            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.EraStart, ActivePlayers = new List<Guid>()});
            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.EraEnd, ActivePlayers = new List<Guid>()});
            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.EraStart, ActivePlayers = new List<Guid>()});
            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.EraEnd, ActivePlayers = new List<Guid>()});
            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.PreEndGameChecks, ActivePlayers = new List<Guid>()});
            PhaseQueue.Add(new Phase(){PhaseType = PhaseType.EndGame, ActivePlayers = new List<Guid>()});
        }
        public PhaseManager(GameContext gameContext, PhaseData pData, List<Phase> qBackup)
        {
            _gameContext = gameContext;
            OnStartEffects = new PhaseStartedEffectManager(gameContext);
            PhaseQueue = qBackup;
            CurrentEra = pData.CurrentEra;
            CurrentAction = pData.CurrentAction;
            CurrentRound = pData.CurrentRound;
            foreach(var playerid in GetCurrentPhase().ActivePlayers)
                _gameContext.PlayerManager.GetPlayerById(playerid).Active = true;
        }
        public PhaseData GetPhaseData()
        {
            return new PhaseData(){
                Phase = GetCurrentPhase(),
                CurrentEra = CurrentEra,
                CurrentAction = CurrentAction,
                CurrentRound = CurrentRound
            };
        }

        public Phase GetCurrentPhase()
        {
            return PhaseQueue[0];
        }
        public void StartNewPhase()
        {
            Phase newPhase = GetCurrentPhase();
            OnStartEffects.Trigger(newPhase.PhaseType, newPhase);

            foreach(var p in _gameContext.PlayerManager.Players)
                p.Active = newPhase.ActivePlayers.Contains(p.Id);

            if(DoCheckCurrentPhase())
            {
                Console.WriteLine("OnNewPhaseStarted " + newPhase.PhaseType.ToString());
                _gameContext.TimerManager.ResetTimer();
                _gameContext.ActionManager.PhaseStarted = true;
                _gameContext.ActionManager.BroadCastSimlpleChanges();
            }
        }
        public bool DoCheckCurrentPhase()
        {
            if(GetCurrentPhase().ActivePlayers.Count == 0)
            {
                PhaseType cPhaseType = GetCurrentPhase().PhaseType;
                if(cPhaseType != PhaseType.EndGame)
                {   
                    PhaseQueue.RemoveAt(0);
                //    OnEndEffects.Trigger(cPhaseType);
                    StartNewPhase();
                    return false;
                }
            }
            return true;
        }
        public void PlayerFinishedCurrentPhase(PlayerInGame player)
        {
            var phase = GetCurrentPhase();
            player.Active = false;
            phase.ActivePlayers.Remove(player.Id);
            _gameContext.TimerManager.ProceedTimerForPlayer(player);
            if(DoCheckCurrentPhase())
            {
                // it means that current phase is still blocked by other player => send info this player is done
                _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = player.Id, DataType = PlayerBasicSetDataType.TurnInactive});
                _gameContext.ActionManager.BroadCastSimlpleChanges();
            }
        }
    }
}