using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Console;

namespace HungerGamesTelegram
{

    public class TelegramPlayer : Actor
    {
        public Game Game { get; }
        public long Id {get;}
        private readonly ITelegramBotClient _client;

        public TelegramPlayer(Game game, long id, ITelegramBotClient client, string name)
        {
            Game = game;
            Id = id;
            _client = client;
            Name = name;
        }

        enum State 
        {
            AskForDirection,
            AskForAction,
            None,
            AskForEvent
        }

        State currentstate = State.None;

        internal void ParseMessage(Message message)
        {
            Logger.Log(this, $" > {message.Text}");

            if(IsDead || !Game.Started)
            {
                return;
            }

            if(currentstate == State.None){
                return;
            }
            if(currentstate == State.AskForDirection){
                var direction = message.Text.ToLower();
                if(Location.Directions.ContainsKey(direction))
                {
                    nextLocation = Location.Directions[direction];
                }
            }
            else
            {
                EventEncounterReply = message.Text;
            }
        }

        public void Write(params string[] message)
        {
            Logger.Log(this, string.Join("\n", message));
            _client.SendTextMessageAsync(Id, string.Join("\n", message), ParseMode.Markdown, true, false, 0, new ReplyKeyboardRemove());
        }

        public void Write(IReplyMarkup markup, params string[] message)
        {
            Logger.Log(this, string.Join("\n", message));
            _client.SendTextMessageAsync(Id, string.Join("\n", message), ParseMode.Markdown, true, false, 0, markup);
        }

        public override void EventPrompt(string message, string[] options)
        {
            currentstate = State.AskForEvent;

            List<KeyboardButton> optionButtons = new List<KeyboardButton>();
            foreach (var option in options)
            {
                optionButtons.Add(new KeyboardButton(option));
            }

            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(optionButtons);

            Write(keyboard, $"Du er her: *{Location.Name}*", $"Du er level *{Level}*", message);
        }

        public override void MovePrompt()
        {
            List<KeyboardButton> directions = new List<KeyboardButton>();
            List<string> locations = new List<string>();
            foreach (var location in Location.Directions)
            {
                if(!location.Value.IsDeadly)
                {
                    directions.Add(new KeyboardButton($"{location.Key}"));
                }
                locations.Add($"*{location.Key}*: {location.Value.Name}" + (location.Value.IsDeadly ? " (Storm)" : ""));
            }

            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(directions, false, false);

            Write(keyboard, $"Du er her: *{Location.Name}*", string.Join("\n", locations), "Hvor vil du gå?");
            
            nextLocation = null;
            currentstate = State.AskForDirection;
        }

        Location nextLocation;
        public override void Move()
        {
            Move(nextLocation);
        }

        public override void Result(int rank)
        {
            Write($"Du ble *#{rank}!*");
        }

        public override void Message(params string[] message)
        {
            Write(string.Join("\n", message));
        }

        public override void Loot()
        {
            base.Loot();
            Write("Du fant et bedre våpen **(+2 lvl)**", $"Du er level *{Level}*");
        }

        public override void RunAway(Actor player2)
        {
            Write($"Du løp vekk fra *{player2.Name}*");

            base.RunAway(player2);
        }

        public override void FailAttack(Actor actor)
        {
            base.FailAttack(actor);
            Write($"{actor.Name} løp vekk.", $"Du fant et bedre våpen **(+1 lvl)**", $"Du er level *{Level}*");
        }

        public override void SuccessAttack(Actor actor)
        {
            base.SuccessAttack(actor);
            Write($"Du beseiret *{actor.Name}*.", $"Du fant et bedre våpen **(+1 lvl)**", $"Du er level *{Level}*");
        }

        public override void Die(Actor actor)
        {
            Write($"*{actor.Name}* (level **{actor.Level}**) beseiret deg.", "*Du er ute av spillet.*");
            base.Die(actor);
        }

        public override void KillZone()
        {
            IsDead = true;
            Write("Du ble tatt av stormen","Du er ute av spillet.");
        }

        public override void Share(Actor actor)
        {
            base.Share(actor);
            Write($"Du og *{actor.Name}* delte på godene **(+1 lvl)**", $"Du er level *{Level}*");
        }
    }
}
