using System;
using System.Collections.Generic;
using System.Linq;

namespace WealthWars
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
        }
    }

    public static class Dice
    {
        private static Random rand = new Random();

        public static int Roll()
        {
            return rand.Next(1, 7) + rand.Next(1, 7); // Simulating two six-sided dice rolls
        }

        public static int RandomAmount(int min, int max)
        {
            return rand.Next(min, max + 1);
        }
    }

    public class Game
    {
        private List<Player> players = new List<Player>();
        private Board board = new Board();
        private int currentPlayerIndex = 0;

        public void Start()
        {
            Console.WriteLine("Welcome to Wealth Wars!");
            InitializePlayers();
            PlayGame();
            AnnounceWinner();
        }

        private void InitializePlayers()
        {
            Console.Write("Enter number of players: ");
            int playerCount = int.Parse(Console.ReadLine());

            for (int i = 0; i < playerCount; i++)
            {
                Console.Write($"Enter name for Player {i + 1}: ");
                string name = Console.ReadLine();
                players.Add(new Player(name));
            }
        }

        private void PlayGame()
        {
            while (!IsGameOver())
            {
                PlayTurn();
            }
        }

        private void PlayTurn()
        {
            Player currentPlayer = players[currentPlayerIndex];
            Console.WriteLine($"{currentPlayer.Name}'s turn");

            int roll = Dice.Roll();
            Console.WriteLine($"{currentPlayer.Name} rolled a {roll}");

            currentPlayer.Move(roll, board);
            board.ProcessLanding(currentPlayer, players);

            if (currentPlayer.Money < 0)
            {
                Console.WriteLine($"{currentPlayer.Name} is bankrupt!");
                players.Remove(currentPlayer);
                currentPlayerIndex--;
            }
            else
            {
                Console.WriteLine($"{currentPlayer.Name} has ${currentPlayer.Money} remaining.");
            }

            WaitForNextTurn();
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        }

        private bool IsGameOver()
        {
            return players.Count <= 1;
        }

        private void AnnounceWinner()
        {
            if (players.Count == 1)
            {
                Console.WriteLine($"{players[0].Name} wins by default!");
            }
            else
            {
                Player winner = players.OrderByDescending(p => p.Money).First();
                Console.WriteLine($"{winner.Name} wins with ${winner.Money}!");
            }
        }

        private void WaitForNextTurn()
        {
            Console.WriteLine("Press the spacebar to proceed to the next player's turn...");
            while (Console.ReadKey(true).Key != ConsoleKey.Spacebar) { }
        }
    }

    public class Player
    {
        public string Name { get; private set; }
        public int Position { get; private set; }
        public int Money { get; set; } = 1500;
        public List<Property> OwnedProperties { get; private set; } = new List<Property>();

        public Player(string name)
        {
            Name = name;
        }

        public void Move(int roll, Board board)
        {
            Position = (Position + roll) % board.Spaces.Count;
            if (Position < roll) // Passed the start
            {
                Money += 200;
                Console.WriteLine($"{Name} passed Start and collected $200");
            }
        }
    }

    public class Board
    {
        public List<BoardSpace> Spaces { get; private set; }

        public Board()
        {
            Spaces = new List<BoardSpace>
            {
                new StartSpace(),
                new Property("Residential Area 1", 100, 10),
                new Property("Residential Area 2", 120, 12),
                new TaxSpace(),
                new Property("Commercial Hub 1", 200, 20),
                new FortuneSpace(),
                new Property("Commercial Hub 2", 220, 22),
                new CrisisSpace(),
                new Property("Industrial Zone 1", 300, 30),
                new Property("Industrial Zone 2", 320, 32),
                new TradingPostSpace(),
                new Property("Luxury Estates 1", 400, 40),
                new Property("Luxury Estates 2", 450, 45)
            };
        }

        public void ProcessLanding(Player player, List<Player> players)
        {
            BoardSpace space = Spaces[player.Position];
            space.OnLand(player, players);
        }
    }

    public abstract class BoardSpace
    {
        public string Name { get; private set; }

        protected BoardSpace(string name)
        {
            Name = name;
        }

        public abstract void OnLand(Player player, List<Player> players);
    }

    public class StartSpace : BoardSpace
    {
        public StartSpace() : base("Start") { }

        public override void OnLand(Player player, List<Player> players)
        {
            // No special action on landing
        }
    }

    public class Property : BoardSpace
    {
        public int Cost { get; private set; }
        public int Rent { get; private set; }
        public Player Owner { get; set; }

        public Property(string name, int cost, int rent) : base(name)
        {
            Cost = cost;
            Rent = rent;
        }

        public override void OnLand(Player player, List<Player> players)
        {
            if (Owner == null)
            {
                OfferPurchase(player);
            }
            else if (Owner != player)
            {
                PayRent(player);
            }
        }

        private void OfferPurchase(Player player)
        {
            Console.WriteLine($"{player.Name} landed on {Name}. It costs ${Cost}. Buy it? (Y/N)");
            if (Console.ReadLine().ToUpper() == "Y")
            {
                if (player.Money >= Cost)
                {
                    player.Money -= Cost;
                    Owner = player;
                    player.OwnedProperties.Add(this);
                    Console.WriteLine($"{player.Name} bought {Name}");
                }
                else
                {
                    Console.WriteLine("Not enough money to buy this property.");
                }
            }
        }

        private void PayRent(Player player)
        {
            Console.WriteLine($"{player.Name} landed on {Name} owned by {Owner.Name}. Paying rent of ${Rent}");
            player.Money -= Rent;
            Owner.Money += Rent;
        }
    }

    public class TaxSpace : BoardSpace
    {
        public TaxSpace() : base("Tax") { }

        public override void OnLand(Player player, List<Player> players)
        {
            int tax = player.Money / 10;
            Console.WriteLine($"{player.Name} landed on Tax. Paying ${tax}.");
            player.Money -= tax;
        }
    }

    public class FortuneSpace : BoardSpace
    {
        public FortuneSpace() : base("Fortune") { }

        public override void OnLand(Player player, List<Player> players)
        {
            int fortune = Dice.RandomAmount(100, 500);
            Console.WriteLine($"{player.Name} landed on Fortune and gains ${fortune}");
            player.Money += fortune;
        }
    }

    public class CrisisSpace : BoardSpace
    {
        public CrisisSpace() : base("Crisis") { }

        public override void OnLand(Player player, List<Player> players)
        {
            int crisis = Dice.RandomAmount(100, 500);
            Console.WriteLine($"{player.Name} landed on Crisis and loses ${crisis}");
            player.Money -= crisis;
        }
    }

    public class TradingPostSpace : BoardSpace
    {
        public TradingPostSpace() : base("Trading Post") { }

        public override void OnLand(Player player, List<Player> players)
        {
            Console.WriteLine($"{player.Name} landed on Trading Post.");
            Player chosenPlayer = ChoosePlayerToTradeWith(player, players);
            if (chosenPlayer != null)
            {
                Property chosenProperty = ChoosePropertyToTake(chosenPlayer);
                if (chosenProperty != null)
                {
                    ExecuteTrade(player, chosenPlayer, chosenProperty);
                }
            }
        }

        private Player ChoosePlayerToTradeWith(Player player, List<Player> players)
        {
            List<Player> otherPlayers = players.Where(p => p != player && p.OwnedProperties.Any()).ToList();
            if (!otherPlayers.Any())
            {
                Console.WriteLine("No available players to trade with.");
                return null;
            }

            Console.WriteLine("Choose a player to trade with:");
            for (int i = 0; i < otherPlayers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {otherPlayers[i].Name}");
            }

            int chosenPlayerIndex = int.Parse(Console.ReadLine()) - 1;
            return otherPlayers[chosenPlayerIndex];
        }

        private Property ChoosePropertyToTake(Player chosenPlayer)
        {
            Console.WriteLine($"{chosenPlayer.Name}'s properties:");
            for (int i = 0; i < chosenPlayer.OwnedProperties.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {chosenPlayer.OwnedProperties[i].Name}");
            }

            int chosenPropertyIndex = int.Parse(Console.ReadLine()) - 1;
            return chosenPlayer.OwnedProperties[chosenPropertyIndex];
        }

        private void ExecuteTrade(Player player, Player chosenPlayer, Property chosenProperty)
        {
            chosenPlayer.OwnedProperties.Remove(chosenProperty);
            player.OwnedProperties.Add(chosenProperty);
            chosenProperty.Owner = player;
            Console.WriteLine($"{player.Name} took {chosenProperty.Name} from {chosenPlayer.Name}");
        }
    }
}
