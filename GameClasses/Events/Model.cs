namespace BoardGameBackend.Models
{

    public class MercenariesLeftData
    {
        public required int TossedMercenariesAmount { get; set; }
        public required int MercenariesAmount { get; set; }
    }

    public class MoraleAdded
    {
        public required PlayerInGame Player { get; set; }
    }

    public class DummyPhaseStarted
    {
        public required PlayerInGame Player { get; set; }
    }

    public class HeroTurnEnded
    {
        public required PlayerInGame Player { get; set; }
    }

    public class ArtifactPhaseSkipped
    {
        public required Guid PlayerId { get; set; }
    }

    public class ArtifactToPickFromDataForOtherUsers
    {
        public required int ArtifactsLeft { get; set; }
        public required int ArtifactsLeftTossed { get; set; }
        public required int ArtifactsAmount { get; set; }
        public required PlayerInGame Player { get; set; }
    }

    public class ArtifactsTakenDataForOtherUsers
    {
        public required int ArtifactsLeft { get; set; }
        public required int ArtifactsLeftTossed { get; set; }
        public required int ArtifactsAmount { get; set; }
        public required PlayerInGame Player { get; set; }
    }

    public class ArtifactDiscardData
    {
        public required int ArtifactId { get; set; }
        public required PlayerInGame Player { get; set; }
    }

    public class ArtifactRerolledDataForOtherUsers
    {
        public required PlayerInGame Player { get; set; }
    }

    public class TeleportationData
    {
        public required PlayerInGame Player { get; set; }
        public int TileId { get; set; }
    }

    public class EndOfGame
    {
        public required Dictionary<Guid, ScorePointsTable> PlayerScores { get; set; }
        public required Dictionary<Guid, TimeSpan> PlayerTimeSpan { get; set; }
        public TimeSpan GameTimeSpan { get; set; }
        public required List<PlayerBasicSetData> PlayerDataChanges {get; set;}
        public required PhaseData PhaseData {get; set;}
        public required string GameId {get; set;}
        public required ActionLogReturn ActionLog {get; set;}
        public required List<GameEventSendData> GameEvents {get; set;}
    }

    public class StartOfGame
    {
        public required List<PlayerViewModelData> Players { get; set; }
        public required string GameId { get; set; }
        public required FullRulerData FullRulerData {get; set; }
        public required List<PlayerBasicSetData> PlayerDataChanges { get; set; }
        public required PhaseData PhaseData {get; set;}
        public required List<KonstelacjaSendInfo> Konstelacje {get; set;}
        public required StartGameModel startGameModel {get; set;}
        public required List<int> EraEffects {get; set;}
        public required List<List<int>> EventsLists {get; set;}
    }

    public class EndOfPlayerTurn
    {
        public required PlayerInGame Player { get; set; }
    }

    public class EndOfTurnEventData
    {
        public required int TurnCount { get; set; }
    }
}
