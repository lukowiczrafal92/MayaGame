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
                { PhaseType.InGameEvent, OnEventInGame },
                { PhaseType.SpecialPlayerAction, OnSpecialPlayerActionCheck }
            };
        }
        
        public void Trigger(PhaseType phasetype, Phase? phase)
        {
            if (triggerActions.ContainsKey(phasetype))
                triggerActions[phasetype](phase);
        }

        public void OnSpecialPlayerActionCheck(Phase? phase)
        {
            var p = _gameContext.PlayerManager.GetPlayersInOrder()[phase.Value2];
            if(_gameContext.ActionManager.ActionChecksManager.DoPlayerNeedAction(p, phase.Value1))
                _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.PlayerAction, ActivePlayers = new List<Guid>(){p.Id}, Value1 = phase.Value1});
        }
        public void OnEraStart(Phase? phase)
        {
            _gameContext.ActionCardManager.CreateActionCardDeck();
            _gameContext.PhaseManager.CurrentEra++;
            _gameContext.PhaseManager.CurrentRound = 0;

            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundStart, ActivePlayers = new List<Guid>()});
            if(_gameContext.GameOptions.AgeCards)
                _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.InGameEvent, ActivePlayers = new List<Guid>()});

            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundStart, ActivePlayers = new List<Guid>()});
            if(_gameContext.GameOptions.AgeCards)
                _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.InGameEvent, ActivePlayers = new List<Guid>()});

            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundEnd, ActivePlayers = new List<Guid>()});
            _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.RoundStart, ActivePlayers = new List<Guid>()});

            _gameContext.EraEffectManager.TriggerNextEra();
            _gameContext.RulerCardsManager.OnAgeStart(false);
        }

        public void OnEventInGame(Phase? phase)
        {
            _gameContext.EventsInGameManager.TriggerEventIfPossible();
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
// TEST
//_gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.EndGame, ActivePlayers = new List<Guid>()});
// TEST
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

            if(_gameContext.ActionCardManager.IsThereNeedToSelectActionCards())
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