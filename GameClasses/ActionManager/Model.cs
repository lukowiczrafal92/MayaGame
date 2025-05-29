namespace BoardGameBackend.Models
{
    public enum PlayerBasicSetDataType
    {
        BoardAngle,
        ResourceAmount,
        ResourceIncomeAmount,
        DeityLevel,
        ActionCardsAmount,
        ResourceConverter,
        RulerCard,
        CityClaim,
        TurnInactive,
        IncomingOrder,
        ScorePoints,
        UsedActionCard,
        PlayerOrder,
        WarfareScore,
        LogRespecialization,
        Constellation,
        StelaeToken,
        Luxury,
        AuraEffect,
        BoardRotation,
        RemoveAuraEffect,
        CityLevel,
        CityDestroyed,
        AngleLost,
        EstScoreEnd,
        LuxuryPermanent,
        HasLuxury,
        ClearLuxury,
        GameLuxuryBonus,
        CapitalCard,
        MarkedAsConquered,
        ClearConqueredMarkers
    }

    public enum PlayerTableType
    {
        EraWarfareScore,
        EraLuxuryScore,
        EraCapitalScore,
        SimpleScore
    }
    public enum GameEventSendType
    {
        EraEnd,
        EraStart,
        GenericEvent
    }

    public class PlayerTableRow
    {
        public required Guid Player { get; set; }
        public int Value1 {get; set;} = -1;
        public int Value2 {get; set;} = -1;
        public int Value3 {get; set;} = -1;
        public int Value4 {get; set;} = -1;
    }

    public class GameEventSendData
    {
        public required GameEventSendType gameEventSendType {get; set;}
        public GameEventPlayerTable? gameEventPlayerTable {get; set;}
        public GameEventPlayerTable? gameEventPlayerTableTwo {get; set;}
        public GameEventPlayerTable? gameEventPlayerTableThird {get; set;}
        public int IntValue1 {get; set;}
    }

    public class GameEventPlayerTable
    {
        public required PlayerTableType PlayerTableType {get; set;}
        public List<PlayerTableRow> ListPlayerRows {get; set;} = new List<PlayerTableRow>();
    }
    public class PlayerBasicSetData
    {
        public required Guid Player { get; set; }
        public required PlayerBasicSetDataType DataType {get; set;}
        public int Value1 {get; set;} = -1;
        public int Value2 {get; set;} = -1;
    }

    public class PhaseSendData
    {
        public required ActionLogReturn ActionLog {get; set;}
        public required List<PlayerBasicSetData> PlayerDataChanges {get; set;}
        public required PhaseData PhaseData {get; set;}
        public required List<GameEventSendData> GameEvents {get; set;}
    }
    public class SimpleSendData
    {
        public required ActionLogReturn ActionLog {get; set;}
        public required List<PlayerBasicSetData> PlayerDataChanges {get; set;}
    }
    public class EraStartSendData
    {
        public required ActionLogReturn ActionLog {get; set;}
        public required List<PlayerBasicSetData> PlayerDataChanges {get; set;}
        public required PhaseData PhaseData {get; set;}
        public required FullRulerData FullRulerData {get; set;}
        public required List<GameEventSendData> GameEvents {get; set;}
    }
}