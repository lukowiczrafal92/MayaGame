using System;
using System.Windows;

namespace BoardGameFrontend.Models
{
    public static class GameConstants
    {
        public const int DeityLevelOne = 1;
        public const int DeityLevelTwo = 2;
        public const int DeityLevelThree = 3;
        public const int DeityLevelFour = 4;
        public const int MAX_RESOURCE_STORAGE = 6;
        public const int DEITY_LVL_FIVE_POINTS = 1;
        public const int DEITY_SET_POINTS = 4;
        public const int JOKER_COST = 3;
        public const int SCORE_KONSTELACJA_THREE = 3;
        public const int SCORE_KONSTELACJA_FOUR = 4;

        public static List<int> GetGameConstants()
        {
            return new List<int>(){ MAX_RESOURCE_STORAGE, DEITY_LVL_FIVE_POINTS, DEITY_SET_POINTS, JOKER_COST, SCORE_KONSTELACJA_THREE, SCORE_KONSTELACJA_FOUR
            };
        }

    }
}