namespace BoardGameBackend.Models
{
    public class SingleIntModel
    {
        public required int Value { get; set; }
    }
    public class ActionCardsPickModel
    {
        public required List<int> ActionCardsIds { get; set; }
    }

    public class ActionLogReturn
    {
        public bool Active {get; set;} = false;
        public Guid PlayerId {get; set;}
        public int ActionId {get; set;} = -1;
        public int JokerActionId {get; set;} = -1;
        public int CardId {get; set;} = -1;
        public int CardGameId {get; set; } = -1;
        public int CardLocationId {get; set;} = -1;
        public int Resource1Id {get; set;} = -1;
        public int Resource2Id {get; set;} = -1;
        public int DeityId {get; set;} = -1;
        public int ExtraInfoTypeId {get; set;} = -1;
        public int ExtraInfoId {get; set;} = -1;
        public int EventCardId {get; set;} = -1;
        public bool PassOnAction {get; set;} = false;
    }
    public class ActionFormSend
    {
        public int CardId {get; set;} = -1;
        public int ActionId {get; set;} = -1;
        public bool Joker {get; set;} = false;
        public int JokerActionId {get; set;} = -1;
        public int DeityId {get; set;} = -1;
        public int TileId {get; set;} = -1;
        public int TileSecondId {get; set;} = -1;
        public int Resource1Id {get; set;} = -1;
        public int Resource2Id {get; set;} = -1;
        public int RulerCardId {get; set;} = -1;
        public int ExtraInfoTypeId {get; set;} = -1;
        public int ExtraInfoId {get; set;} = -1;
        public int EventCardId {get; set;} = -1;
        public int CapitalCardId {get; set;} = -1;
        public bool PassOnAction {get; set;} = false;
    }
    public enum ActionTypes{
        NO_ACTION,
        PILGRIMAGE,
        FOUND_CITY,
        DISCARD_CARD,
        RECRUIT_BUILDERS,
        RECRUIT_ASTRONOMERS,
        RECRUIT_PRIESTS,
        RECRUIT_STONE_CARVERS,
        RECRUIT_WARRIORS,
        WAR_TRIBUTE,
        ERECT_STELAE,
        SKY_OBSERVATION,
        JOKER,
        CITY_EXPAND,
        CITY_EXPAND_CARVERS,
        WAR_CONQUEST,
        WAR_STAR,
        JOKER_WARRIOR,
        JOKER_BUILDER,
        JOKER_CARVER,
        JOKER_ASTRONOM,
        JOKER_PRIEST,
        SKY_RULER_STAR,
        ERA_MERCENARIES,
        PILGRIMAGE_RIVAL,
        MIRRORED_ANGLE,
        SWAP_DEITIES_LEVELS,
        LOOSE_CITY,
        SPECIALISTS_INTO_POINTS,
        LOOSE_ANGLE,
        RULER_DEITY,
        TRADE_POINTS,
        CHECK_ANY_LUXURY
    }
}