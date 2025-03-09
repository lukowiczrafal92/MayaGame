namespace BoardGameBackend.Models
{
    public class FullGameData
    {
        public required string GameId {get; set;}
        public required List<FullPlayerData> PlayersData { get; set; }
        public required List<TileGame> TilesData {get; set; }
        public required FullRulerData FullRulerData {get; set;}
        public required List<PlayerBasicSetData> PlayerSetData {get; set; }
        public required PhaseData PhaseData {get; set;}
        public required ActionCardsPlayerData PlayerActionCards {get; set;}
        public required List<KonstelacjaSendInfo> Konstelacje {get; set;}
        public required StartGameModel startGameModel {get; set;}
        public required List<int> EraEffects {get; set;}
        public required List<List<int>> EventsLists {get; set;}
    }

    public class FullRulerData
    {
        public required int DeckAmount {get; set;}
        public required List<int> RulerPool {get; set;}
    }

    public class FullPlayerData
    {
        public required PlayerViewModelData Player {get;set;}
    }
}