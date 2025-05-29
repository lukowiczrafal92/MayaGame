namespace BoardGameBackend.Models
{
    public class RulerCard
    {
        public required RulerGameData dbInfo;
    }

    public class RulerGameData
    {
        public int Id;
        public int ConverterId;
        public int DeityId;
        public int LuxuryId;
        public int EndGameResource;
        public int InstantResource;
        public int AbilityInfo;
        public int AuraEffectId;
        public int Angle;
        public bool ComboScore;
        public bool EndGameDeity;
    }
}
