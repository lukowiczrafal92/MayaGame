using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace BoardGameBackend.Models
{
    [BsonIgnoreExtraElements]
    public class FullMayabBackup 
    {
        public required string GameId {get; set;}
        public required int GameVersion {get; set;}
        public required LobbyManagerInfo lobbyinfo {get; set;}
        public required FullGameBackup FullGameBackup {get; set;}
        
    }

    public class FullGameBackup
    {
        public required List<FullPlayerData> PlayersData { get; set; }
        public required List<TileGame> TilesData {get; set; }
        public required FullRulerBackup FullRulerData {get; set;}
        public required List<int> FullStolicaData {get; set;}
        public required List<PlayerBasicSetData> PlayerSetData {get; set; }
        public required PhaseData PhaseData {get; set;}
        public required List<KonstelacjaSendInfo> Konstelacje {get; set;}
        public required List<int> EraEffects {get; set;}
        public required List<List<int>> EventsLists {get; set;}
        public required List<Phase> PhaseQueue {get; set;}
        public required List<ActionCard> ActionDeck {get; set;}
        public required List<PlayerBackupActionCards> PlayerActionCards {get; set;}
        public required int LuxuryBonus {get; set;}
    }
    public class FullGameData
    {
        public required string GameId {get; set;}
        public required List<FullPlayerData> PlayersData { get; set; }
        public required List<TileGame> TilesData {get; set; }
        public required FullRulerData FullRulerData {get; set;}
        public required List<int> FullStoliceData {get; set;}
        public required List<PlayerBasicSetData> PlayerSetData {get; set; }
        public required PhaseData PhaseData {get; set;}
        public required ActionCardsPlayerData PlayerActionCards {get; set;}
        public required List<KonstelacjaSendInfo> Konstelacje {get; set;}
        public required StartGameModel startGameModel {get; set;}
        public required List<int> EraEffects {get; set;}
        public required List<List<int>> EventsLists {get; set;}
        public required int LuxuryBonus {get; set;}
    }

    public class FullRulerBackup
    {
        public required List<int> RulerDeck {get; set;}
        public required List<int> RulerPool {get; set;}
    }

    public class FullRulerData
    {
        public required int DeckAmount {get; set;}
        public required List<int> RulerPool {get; set;}
    }

    public class PlayerBackupActionCards
    {
        public required Guid PlayerId {get; set;}
        public required List<ActionCard> HandActionCards {get; set;}
        public required List<ActionCard> ReserveActionCards {get; set;}
        
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public required Dictionary<int, int> ScoreLog {get; set;}
        
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public required Dictionary<int, int> ActionsLog {get; set;}
        
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public required Dictionary<int, int> ExtraLog {get; set;}
    }

    public class FullPlayerData
    {
        public required PlayerViewModelData Player {get;set;}
    }
}