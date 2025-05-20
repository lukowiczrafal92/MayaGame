

namespace BoardGameBackend.Models
{
    public enum PhaseType
    {
        DummyPreStart,
        EraStart,
        RoundStart,
        RoundEnd,
        EraEnd,
        IncomeConverting,
        ActionCardSelection,
        PlayerAction,
        EndGame,
        ActionsStart,
        ActionsEnd,
        PreEndGameChecks,
        EndScoreConverting,
        InGameEvent,
        SpecialPlayerAction,
        ChooseCapital,
        ChooseSpecialist
    }
    public class PhaseData
    {
        public required Phase Phase {get; set;}
        public required int CurrentEra {get; set;}
        public required int CurrentRound {get; set;}
        public required int CurrentAction {get; set;}
    }

    public class Phase
    {
        public required PhaseType PhaseType {get; set;}
        public required List<Guid> ActivePlayers {get; set;}
        public int Value1 {get; set;} = -1;
        public int Value2 {get; set;} = -1;
    }
}