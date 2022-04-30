using System;
using System.Collections.Generic;

namespace BuildABot
{
    [Serializable]
    public class GameState
    {
        public int GameStage { get; set; } = 0;
        public int CompletedLevelCount { get; set; } = 0;
        public List<int> CompletedLevelSeeds { get; private set; } = new List<int>();
        public int KillCount { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalMoneyEarned { get; set; }
        public int TotalJumps { get; set; }
        public int ItemsBought { get; set; }
        public int ItemsSold { get; set; }
    }
}