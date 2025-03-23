using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class KonstelacjeManager
    {
        public List<Konstelacja> lKonstelacje;
        private readonly GameContext _gameContext;
        public KonstelacjeManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            lKonstelacje = new List<Konstelacja>();

            bool bIncludeThree = _gameContext.GameOptions.ThreeStar;
            var slist = new List<KonstelacjaGameData>();
            foreach(var l in GameDataManager.GetKonstelacje())
            {
                if(bIncludeThree || l.Angles.Count > 3)
                    slist.Add(l);
            }

            Random rng = new Random();
            slist = slist.OrderBy(m => rng.Next()).ToList();

            for(int i = 0; i < 3; i++)
            {
                var k = new Konstelacja(){dbInfo = slist[i], sendInfo = new KonstelacjaSendInfo(){Id = slist[i].Id}};
                lKonstelacje.Add(k);
            }
        }

        public KonstelacjeManager(GameContext gameContext, List<KonstelacjaSendInfo> Konstelacje)
        {
            _gameContext = gameContext;
            lKonstelacje = new List<Konstelacja>();
            foreach(var h in Konstelacje)
            {
                var k = new Konstelacja(){dbInfo = GameDataManager.GetKonstelacjaById(h.Id), sendInfo = h};
                lKonstelacje.Add(k);
            }
        }

        public List<KonstelacjaSendInfo> GetFullData()
        {
            List<KonstelacjaSendInfo> a = new List<KonstelacjaSendInfo>();
            for(int i = 0; i < lKonstelacje.Count(); i++)
                a.Add(lKonstelacje[i].sendInfo);

            return a;
        }

        public bool PlayerCompletedConstellation(PlayerInGame p, int iAngle, List<int> angles)
        {
            if(angles.Contains(iAngle))
            {
                bool bCorrect = true;
                foreach(var it in angles)
                {
                    if(!p.PlayerAngleBoard.GetAngleById(it).bChecked)
                    {
                        bCorrect = false;
                        break;
                    }
                }
                if(bCorrect)
                    return true;
            }

            if(_gameContext.GameOptions.AllowRotateConstellations)
            {
                var dblist = GameDataManager.GetAngleBoardTiles();
                for(int i = 1; i <= 5; i++)
                {
                    bool bCorrect = true;
                    foreach(var it in angles)
                    {
                        var idb = dblist.FirstOrDefault(c => c.Id == it);
                        int iNewAngle = (idb.Angle + i * 60) % 360;
                        var newdb = dblist.FirstOrDefault(c => c.Distance == idb.Distance && c.Angle == iNewAngle);
                        if(!p.PlayerAngleBoard.GetAngleById(newdb.Id).bChecked)
                        {
                            bCorrect = false;
                            break;
                        }
                    }

                    if(bCorrect)
                        return true;
                }
            }
            return false;
        }
        public void CheckNewAngleByPlayer(PlayerInGame p, int iAngle)
        {
            foreach(var k in lKonstelacje)
            {
                if(k.sendInfo.PlayerId == Guid.Empty)
                {
                    if(PlayerCompletedConstellation(p, iAngle, k.dbInfo.Angles))
                        PlayerAcquireConstellation(p, k.dbInfo.Id);
                }
            }
        }

        public void PlayerAcquireConstellation(PlayerInGame p, int CardId)
        {
            var card = lKonstelacje.FirstOrDefault(k => k.dbInfo.Id == CardId);
            if(card != null)
            {
                card.sendInfo.PlayerId = p.Id;
                if(card.dbInfo.Angles.Count == 4)
                    _gameContext.PlayerManager.ChangeScorePoints(p, GameConstants.SCORE_KONSTELACJA_FOUR, ScorePointType.Konstelacja);
                else
                    _gameContext.PlayerManager.ChangeScorePoints(p, GameConstants.SCORE_KONSTELACJA_THREE, ScorePointType.Konstelacja);

                _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){DataType = PlayerBasicSetDataType.Constellation, Player = p.Id, Value1 = CardId});
            }
        }
    }
}
