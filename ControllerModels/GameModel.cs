namespace BoardGameBackend.Models
{
    public class StartGameModel
    {
        public bool AgeCards {get; set;} = true;
        public bool ThreeStar {get; set;} = true;
        public bool AllowRotateConstellations {get; set;} = true;
    }

    public enum TurnTypes{
        PHASE_BY_PHASE,
        FULL_TURN
    }
}