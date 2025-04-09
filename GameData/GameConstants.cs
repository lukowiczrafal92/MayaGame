using System;
using System.Windows;

namespace BoardGameFrontend.Models
{
    public static class GameConstants
    {
        public const int m_iGameVersion = 3;
        public const int DeityLevelOne = 1;
        public const int DeityLevelTwo = 2;
        public const int DeityLevelThree = 3;
        public const int DeityLevelFour = 4;
        public const int MAX_RESOURCE_STORAGE = 6;
        public const int DEITY_SET_POINTS = 0;
        public const int DEITY_SET_PER_PLAYERS = 0;
        public const int DEITY_LVL_FIVE_POINTS = 0;
        public const int DEITY_LVL_FOUR_PER_PATRON = 0;
        public const int DEITY_LVL_FOUR_PER_LVL3 = 1;
        public const int JOKER_COST = 3;
        public const int SCORE_KONSTELACJA_THREE = 2;
        public const int SCORE_KONSTELACJA_FOUR = 3;

        public static List<int> GetGameConstants()
        {
            return new List<int>(){ MAX_RESOURCE_STORAGE, DEITY_LVL_FIVE_POINTS, DEITY_SET_POINTS, JOKER_COST, SCORE_KONSTELACJA_THREE, SCORE_KONSTELACJA_FOUR, 
            DEITY_SET_PER_PLAYERS, DEITY_LVL_FOUR_PER_PATRON, DEITY_LVL_FOUR_PER_LVL3
            };
        }

    }
}