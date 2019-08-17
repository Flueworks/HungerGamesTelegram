﻿using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace HungerGamesTelegram
{
    class Program
    {
        static async Task Main(string[] args)
        {
            WriteLine("Hunger Games");

            var telegramGameHost = new TelegramGameHost();
            telegramGameHost.Start();

            Game game = new Game(new ConsoleNotificator());
            while(true)
            {
                await game.StartGame(new ConsolePlayer(game));
            }
        }
    }

    class ConsoleNotificator : INotificator
    {
        public void GameAreaIsReduced()
        {
            WriteLine("Området har blitt begrenset");
        }

        public void GameHasEnded()
        {
            WriteLine("Spillet er slutt");
        }

        public void GameHasStarted()
        {
            WriteLine("Spillet har startet");
        }

        public void RoundHasEnded(int round)
        {
            WriteLine($"{round}. runde er over");
        }
    }

}
