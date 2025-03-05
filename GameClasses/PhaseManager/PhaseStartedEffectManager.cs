using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class PhaseStartedEffectManager
    {
        private readonly GameContext _gameContext;
        private readonly Dictionary<PhaseType, Action<Phase?>> triggerActions;

        public PhaseStartedEffectManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            triggerActions = new Dictionary<PhaseType, Action<Phase?>>
            {
                { PhaseType.EraStart, OnEraStart },
                { PhaseType.EraEnd, OnEraEnd },
                { PhaseType.RoundStart, OnRoundStart },
                { PhaseType.ActionsStart, OnActionsStart },
                { PhaseType.RoundEnd, OnRoundEnd },
                { PhaseType.EndGame, OnEndGame },
                { PhaseType.PreEndGameChecks, OnPreEndGameChecks },
            };
        }
        
        public void Trigger(PhaseType phasetype, Phase? phase)
        {
            if (triggerActions.ContainsKey(phasetype))
                triggerActions[phasetype](phase);
        }
        public void OnEraStart(Phase? phase)
        {
            _gameContext.ActionCardManager.CreateActionCardDeck();
            _gameContext.PhaseManager.CurrentEra++;
            _gameContext.PhaseManager.CurrentRound = 0;
            _gameContext.RulerCardsManager.OnAgeStart();
            _gameContext.ActionManager.EraStart = true;
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundStart, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundStart, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundStart, ActivePlayers = new List<Guid>()});

            GameEventSendData ge = new GameEventSendData(){gameEventSendType = GameEventSendType.EraStart};
            _gameContext.ActionManager.AddNewGameEvent(ge);
        }

        public void OnEraEnd(Phase? phase)
        {
            _gameContext.ActionCardManager.ClearAllPlayersActionCards();
            _gameContext.ScorePointsManager.TriggerEndEra();
        }

        public void OnRoundEnd(Phase? phase)
        {
            _gameContext.PlayerManager.RefreshOrder();
        }

        public void OnPreEndGameChecks(Phase? phase)
        {
            foreach(var h in _gameContext.PlayerManager.GetPlayersInReverseOrder())
            {
                if(_gameContext.PlayerManager.HasNeedOfEndGameConverting(h))
                    _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.EndScoreConverting, ActivePlayers = new List<Guid>(){h.Id}});
            }
        }
        public void OnRoundStart(Phase? phase)
        {
            // income
            _gameContext.ActionCardManager.DistributeCards();
            //
            _gameContext.PhaseManager.CurrentRound++;
            _gameContext.PhaseManager.CurrentAction = 0;
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsStart, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsStart, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsStart, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsStart, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionsStart, ActivePlayers = new List<Guid>()});
            List<Guid> p = new List<Guid>();
            foreach(var h in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                _gameContext.PlayerManager.TriggerIncome(h);
                p.Add(h.Id);
            }

            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ActionCardSelection, ActivePlayers = p});
            foreach(var h in _gameContext.PlayerManager.GetPlayersInReverseOrder())
            {
                if(_gameContext.PlayerManager.HasNeedOfExtraConverting(h))
                    _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.IncomeConverting, ActivePlayers = new List<Guid>(){h.Id}});
            }
        }

        public void OnActionsStart(Phase? phase)
        {
            _gameContext.PhaseManager.CurrentAction++;
            foreach(var h in _gameContext.PlayerManager.GetPlayersInReverseOrder())
                _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.PlayerAction, ActivePlayers = new List<Guid>(){h.Id}});
        }

        public void OnEndGame(Phase? phase)
        {
            _gameContext.ScorePointsManager.TriggerEndGameScores();
        }
    }
}