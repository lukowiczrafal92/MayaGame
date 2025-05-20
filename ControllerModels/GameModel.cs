namespace BoardGameBackend.Models
{
    public class StartGameModel
    {
        public bool AgeCards {get; set;} = true;
        public bool EventsIntoQuests {get; set;} = true;
        public bool ThreeStar {get; set;} = true;
        public bool AllowRotateConstellations {get; set;} = true;
        public bool SpecialActions {get; set;} = false;
        public AngleScoreTypes AngleScoreType { get; set; } = AngleScoreTypes.ONLY_ANGLE;
    }
    public enum AngleScoreTypes
    {
        ONLY_ANGLE,
        ANY_TILE,
        NO_SCORE
    }
}