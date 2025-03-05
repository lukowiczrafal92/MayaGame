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
        public void TriggerEndEra()
        {
            GameEventSendData ge = new GameEventSendData(){gameEventSendType = GameEventSendType.EraEnd};
            ge.gameEventPlayerTable = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.EraWarfareScore};
            ge.gameEventPlayerTableTwo = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.EraLuxuryScore};
            ge.gameEventPlayerTableThird = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.EraCapitalScore};

            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var prow = new PlayerTableRow(){Player = p.Id};
                var lrow = new PlayerTableRow(){Player = p.Id};
                var crow = new PlayerTableRow(){Player = p.Id};
                int iScoreFromWarfare = 0;
                int iScoreFromLuxuries = 0;
                int iScoreFromCapital = 0;

                int inumdifresource = p.PlayerLuxuries.GetNumDifferentResources();
                if(inumdifresource >= 6)
                    iScoreFromLuxuries += 2;

                foreach(var op in _gameContext.PlayerManager.Players)
                {
                    if(op.WarfareScore < p.WarfareScore)
                        iScoreFromWarfare += 2;

                    if(op.PlayerLuxuries.GetNumDifferentResources() < inumdifresource)
                        iScoreFromLuxuries += 1;
                }
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

                prow.Value1 = p.WarfareScore;
                prow.Value2 = iScoreFromWarfare;
                lrow.Value1 = inumdifresource;
                lrow.Value2 = iScoreFromLuxuries;
                crow.Value1 = iExpansion;
                crow.Value2 = iScoreFromCapital;
                ge.gameEventPlayerTable.ListPlayerRows.Add(prow);
                ge.gameEventPlayerTableTwo.ListPlayerRows.Add(lrow);
                ge.gameEventPlayerTableThird.ListPlayerRows.Add(crow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromWarfare, ScorePointType.EraWarfare);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromLuxuries, ScorePointType.Luxuries);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScoreFromCapital, ScorePointType.DuringGameCapitalCity);
            }
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
                        iScoreFromAngles += 1;
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
            }

            var sortedPlayerScores = playerScores
                .OrderByDescending(kv => kv.Value.PointsOverall) 
                .ThenBy(kv => kv.Value.FinalOrder) 
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return sortedPlayerScores;
        }
    }
}