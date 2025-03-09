using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class ScorePointsManager
    {
        private readonly GameContext _gameContext;
        private Dictionary<Guid, ScorePointsTable> playerScores = new Dictionary<Guid, ScorePointsTable>();

        public ScorePointsManager(GameContext gameContext)
        {
            _gameContext = gameContext;
        }

        public GameEventPlayerTable EndEraCapitalPoints(ScorePointType sctype)
        {
            var gept = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.EraCapitalScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var crow = new PlayerTableRow(){Player = p.Id};
                int iScoreFromCapital = 0;
                int iExpansion = _gameContext.BoardManager.GetCapitalCityLevel(p.Id);
                if(iExpansion > 0)
                {
                    iScoreFromCapital += iExpansion;
                    foreach(var op in _gameContext.PlayerManager.Players)
                    {
                        if(iExpansion > _gameContext.BoardManager.GetCapitalCityLevel(op.Id))
                            iScoreFromCapital++;
                    }
                }
                crow.Value1 = iExpansion;
                crow.Value2 = iScoreFromCapital;
                gept.ListPlayerRows.Add(crow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromCapital, sctype);
            }
            return gept;
        }

        public GameEventPlayerTable EndEraWarfarePoints(ScorePointType sctype)
        {
            var gept = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.EraWarfareScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var prow = new PlayerTableRow(){Player = p.Id};
                int iScoreFromWarfare = 0;
                foreach(var op in _gameContext.PlayerManager.Players)
                {
                    if(op.WarfareScore < p.WarfareScore)
                        iScoreFromWarfare += 2;
                }

                prow.Value1 = p.WarfareScore;
                prow.Value2 = iScoreFromWarfare;
                gept.ListPlayerRows.Add(prow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromWarfare, sctype);
            }
            return gept;
        }

        public GameEventPlayerTable EndEraLuxuryPoints(ScorePointType sctype)
        {
            var gept = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.EraLuxuryScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var lrow = new PlayerTableRow(){Player = p.Id};
                int iScoreFromLuxuries = 0;
                int inumdifresource = p.PlayerLuxuries.GetNumDifferentResources();
                if(inumdifresource >= 6)
                    iScoreFromLuxuries += 2;

                foreach(var op in _gameContext.PlayerManager.Players)
                {
                    if(op.PlayerLuxuries.GetNumDifferentResources() < inumdifresource)
                        iScoreFromLuxuries += 1;
                }

                lrow.Value1 = inumdifresource;
                lrow.Value2 = iScoreFromLuxuries;
                gept.ListPlayerRows.Add(lrow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromLuxuries, sctype);
            }
            return gept;
        }
        public void TriggerEndEra()
        {
            GameEventSendData ge = new GameEventSendData(){gameEventSendType = GameEventSendType.EraEnd};
            ge.gameEventPlayerTable = EndEraWarfarePoints(ScorePointType.EraWarfare);
            ge.gameEventPlayerTableTwo = EndEraLuxuryPoints(ScorePointType.Luxuries);
            ge.gameEventPlayerTableThird = EndEraCapitalPoints(ScorePointType.DuringGameCapitalCity);
            ge.IntValue1 = _gameContext.EraEffectManager.EndCurrentEraEffect();
            _gameContext.ActionManager.AddNewGameEvent(ge);
        }
        public void TriggerEndGameScores()
        {
            int NUM_PLAYERS = _gameContext.PlayerManager.Players.Count;
            foreach(var p in _gameContext.PlayerManager.Players)
            {
                // deities
                int iScoreFromDeity = 0;
                int iMin = 100;
                foreach(var deity in p.PlayerDeities.Deities)
                {
                    iMin = Math.Min(iMin, deity.Level);
                    foreach(var op in _gameContext.PlayerManager.Players)
                    {
                        var rivaldeity = op.PlayerDeities.GetDeityById(deity.Id);
                        if(rivaldeity.Level < deity.Level || ((rivaldeity.Level == deity.Level) && (deity.TieBreaker > rivaldeity.TieBreaker)))
                            iScoreFromDeity += 1;
                    }
                }
                iScoreFromDeity += iMin * NUM_PLAYERS; //GameConstants.DEITY_SET_POINTS;
                if(iScoreFromDeity > 0)
                    _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromDeity, ScorePointType.EndGameDeity);
                // angles
                int iScoreFromAngles = 0;
                for(int it = 0; it < 12; it ++)
                {
                    int iAngle = it * 30;
                    if(p.PlayerAngleBoard.Angles.FirstOrDefault(pa => pa.bChecked && pa.dbInfo.Angle == iAngle) != null)
                    {
                    //    iScoreFromAngles += 1;  teraz się zdobywa za określanie kątów jako pierwszy
                        foreach(var op in _gameContext.PlayerManager.Players)
                        {
                            if(op.PlayerAngleBoard.Angles.FirstOrDefault(pa => pa.bChecked && pa.dbInfo.Angle == iAngle) == null)
                                iScoreFromAngles += 1;
                        }
                    }
                }
                if(iScoreFromAngles > 0)
                    _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromAngles, ScorePointType.EndGameAngles);

                // rulers
                int iScoreFromRulers = 0;
                if(p.Rulers.Count > 0)
                {
                    foreach(var op in _gameContext.PlayerManager.Players)
                    {
                        if(op.Rulers.Count < p.Rulers.Count)
                            iScoreFromRulers += 2;
                    }
                }

                // end resource score also from rulers
                foreach(var resource in p.PlayerResources.Resources)
                {
                    if(resource.EndGameScore > 0)
                    {
                        int iScore = (resource.Amount * resource.EndGameScore) / 100;
                        if(iScore > 0)
                            iScoreFromRulers += iScore;
                    }
                }


                if(iScoreFromRulers > 0)
                    _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromRulers, ScorePointType.EndGameRulers);
            }
        }

        public Dictionary<Guid, ScorePointsTable> FinalScore()
        {
            var players = _gameContext.PlayerManager.Players;

            foreach (var player in players)
            {
                playerScores[player.Id] = new ScorePointsTable();
            }

            foreach (var player in players)
            {
                var scoreTable = playerScores[player.Id];
                scoreTable.PointsOverall = player.Points;
                scoreTable.FinalOrder = player.CurrentOrder;
                scoreTable.DuringGameDeity = player.GetScoreFromSource(ScorePointType.DuringGameDeity);
                scoreTable.DuringGameCapitalCity = player.GetScoreFromSource(ScorePointType.DuringGameCapitalCity);
                scoreTable.EraWarfare = player.GetScoreFromSource(ScorePointType.EraWarfare);
                scoreTable.EndGameDeity = player.GetScoreFromSource(ScorePointType.EndGameDeity);
                scoreTable.EndGameRulers = player.GetScoreFromSource(ScorePointType.EndGameRulers);
                scoreTable.EndGameAngles = player.GetScoreFromSource(ScorePointType.EndGameAngles);
                scoreTable.Konstelacja = player.GetScoreFromSource(ScorePointType.Konstelacja);
                scoreTable.Luxuries = player.GetScoreFromSource(ScorePointType.Luxuries);
                scoreTable.DuringGameAngle = player.GetScoreFromSource(ScorePointType.DuringGameAngle);
                scoreTable.ErasAndEvents = player.GetScoreFromSource(ScorePointType.ErasAndEvents);
            }

            var sortedPlayerScores = playerScores
                .OrderByDescending(kv => kv.Value.PointsOverall) 
                .ThenBy(kv => kv.Value.FinalOrder) 
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return sortedPlayerScores;
        }
    }
}