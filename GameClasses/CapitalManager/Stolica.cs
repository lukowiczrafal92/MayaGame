namespace BoardGameBackend.Models
{
    public class StolicaCard
    {
        public required StolicaData dbInfo;
    }
    public class StolicaData
    {
        public int Id;
        public int ConverterId;
        public int ExtraReward;
    }
}
