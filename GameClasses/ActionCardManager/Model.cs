namespace BoardGameBackend.Models
{
    public class ActionCardGameData
    {
        public required int Id {get; set;}
        public required int StartPerPlayer {get; set;}
    }

    public class ActionGameData
    {
        public required int Id {get; set;}
        public required int Resource {get; set;}
        public required int StaticCost {get; set;}
        public required int PerEraCost {get; set;}
        public required int PerRulerCost {get; set;}
        public required int PerCityLevelCost {get; set;}
        public required bool RequiresLocation {get; set;}
        public required bool Joker {get; set;}
        public required bool ShowCardAll {get; set;}
        public required int PerCityCost100 {get; set;}
        public required int RequiresEffectId {get; set;}
        public required bool SpecialExtra {get; set;}
        public required int MaxCost {get; set;}

    }
    public class ActionCard
    {
        public required int Id {get; set;}
        public required int GameIndex {get; set;}
        public int LocationId {get; set;} = -1;
    }

    public class ActionCardsPlayerData
    {
        public required Guid Player {get; set; }
        public required List<ActionCard> Reserve {get; set;}
        public required List<ActionCard> Hand {get; set;}
    }
}