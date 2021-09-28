using System;
using System.Diagnostics;
using System.Timers;

namespace NewWorldVoxel
{
    class RoundTimer
    {
        private static RoundTimer roundTimer_;
        private int gameRoundTime_;

        public Stopwatch Stopwatch;
        public TimeSpan Span;
        public Timer GameRound;

        public int GameRoundCounter;
        public int MarketPriceIncreaseRound;
        public int WinningRound { get; set; }

        public void SetupTimer(int gameRoundTime)
        {
            gameRoundTime_ = gameRoundTime * 1000;

            Stopwatch = new Stopwatch();

            Span = new TimeSpan(0, 0, 0, gameRoundTime);
            GameRound = new Timer(gameRoundTime * 1000);

            GameRoundCounter = 1;

            GameRound.Elapsed += Transactions.OnRoundTimerElapsed;
            GameRound.Elapsed += ResetStopwatch;
            GameRound.Elapsed += AddGameRoundCounter;
            GameRound.Elapsed += CheckMarketPrices;
            GameRound.Elapsed += CheckForEndOfGame;
            GameRound.Elapsed += ResetRoundTimeToDefault;
        }

        public static RoundTimer GetInstance()
        {
            if (roundTimer_ != null)
                return roundTimer_;
            else
            {
                roundTimer_ = new RoundTimer();
                return roundTimer_;
            }
        }

        public void Start()
        {
            GameRound.Start();
            Stopwatch.Start();
        }

        public void Stop()
        {
            GameRound.Stop();
            Stopwatch.Stop();
        }

        public void Set(int time)
        {
            if (time == 0)
                GameRound.Interval = 1000;
            else
                GameRound.Interval = time * 1000;
        }

        private void ResetStopwatch(object sender, EventArgs e)
        {
            Stopwatch.Reset();
            Stopwatch.Start();
        }

        private void AddGameRoundCounter(object sender, EventArgs e)
        {
            GameRoundCounter += 1;
        }

        private void ResetRoundTimeToDefault(object sender, EventArgs e)
        {
            GameRound.Interval = gameRoundTime_;
        }

        private void CheckMarketPrices(object sender, EventArgs e)
        {
            if (MarketPriceIncreaseRound == 0) return;

            if(GameRoundCounter % MarketPriceIncreaseRound == 0)
                Prices.IncreaseMarketPrices();
        }

        private void CheckForEndOfGame(object sender, EventArgs e)
        {
            if(GameRoundCounter == WinningRound || Game.GetInstance().ActivePlayer.Citizens == 0)
            {
                UiCollection.EndOfGameActive = true;

                if (Game.GetInstance().MultiplayerRunning)
                {
                    NetworkTransactions.PlayerRanking.Add(Network.GetInstance().UserName, Game.GetInstance().ActivePlayer.Land.ToString());
                    Network.GetInstance().SendMessage($"{Network.GetInstance().UserName} {Game.GetInstance().ActivePlayer.Land}", 28);
                }

                Stop();
            }
        }
    }
}