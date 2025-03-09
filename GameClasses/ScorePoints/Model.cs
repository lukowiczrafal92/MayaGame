
namespace BoardGameBackend.Models
{
    public enum ScorePointType
    {
        DuringGameDeity,
        DuringGameCapitalCity,
        EraWarfare,
        EndGameDeity,
        EndGameRulers,
        EndGameAngles,
        Konstelacja,
        Luxuries,
        DuringGameAngle,
        ErasAndEvents
    }
    public class PointsWithPower{
        public int Points { get; set; } = 0;
        public int Power { get; set; } = 0;
    }

    public class ScorePointsTable{
/*        public PointsWithPower MoralePoints { get; set; } = new PointsWithPower();
        public PointsWithPower SiegePoints { get; set; } = new PointsWithPower();
        public PointsWithPower ArmyPoints { get; set; }  = new PointsWithPower();
        public PointsWithPower MagicPoints { get; set;}  = new PointsWithPower(); */

        public int DuringGameDeity {get; set;} = 0;
        public int DuringGameCapitalCity {get; set;} = 0;
        public int EraWarfare {get; set;} = 0;
        public int EndGameDeity {get; set;} = 0;
        public int EndGameRulers {get; set;} = 0;
        public int EndGameAngles {get; set;} = 0;
        public int PointsOverall {get; set;} = 0;
        public int FinalOrder {get; set;} = 0;
        public int Konstelacja {get; set;} = 0;
        public int Luxuries {get; set;} = 0;
        public int DuringGameAngle {get; set;} = 0;
        public int ErasAndEvents {get; set;} = 0;
    }


}