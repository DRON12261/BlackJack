#region Подключение модулей
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Media;
using System.IO;
#endregion

namespace BlackJack
{
    #region Ключевые структуры и перечисления
    struct Player
    {
        public string name;
        public List<List<Card>> splits;
        public bool isBot;
        public List<SessionStates> sessionStates; 
        public byte[] totalScores;
    }               
    struct Card
    {
        public Rank rank;
        public Suit suit;
        public bool side;
    }
    struct MenuItem
    {
        public string label;
        public Submenu submenu;
    }
    struct Submenu
    {
        public string[] items;
        public int currentItem;
    }
    struct GameInfo
    {
        public Player[] Players;
        public Player House;
        public bool houseTurn, houseEnough, houseEnds, playerEnough, achiveEnabled;
        public List<Card> Deck;
        public byte[] currentSplit;
        public int currentRound, currentCard;
        public SessionLenght currentSessionLenght;
        public GameLogicLoops GameLogicLoop;
        public List<string> completedAchivements;
    }
    enum Rank
    {
        Two = 50,
        Three = 51,
        Four = 52,
        Five = 53,
        Six = 54,
        Seven = 55,
        Eight = 56,
        Nine = 57,
        Ten = 88,
        Jack = 74,
        Queen = 81,
        King = 75,
        Ace = 65
    }
    enum Suit
    {
        Hearts = 9829,
        Spades = 9824,
        Clubs = 9827,
        Diamonds = 9830
    }
    enum GameStates
    {
        MainMenu,
        Game,
        LoadGame,
        SaveGame,
        Options,
        Achivements,
        Records,
        Rules,
        UpdateList,
        Quit
    }
    enum SessionStates
    {
        InGame,
        Victory,
        Lose,
        BlackJack,
        Leave,
        Draw
    }
    enum SessionLenght
    {
        One = 1,
        Small = 5,
        Medium = 10,
        Large = 15,
        BigGame = 25
    }
    enum StartGameLoops
    {
        PlayersListGeneration,
        PlayersInitialization,
        SessionLenghtInitialization,
        Preparing
    }
    enum GameLogicLoops
    {
        GameInitialization,
        PreparingGameSession,
        PlayersTurn,
        HouseTurn,
        GameEnding,
        GameResults
    }
    #endregion

    class Program
    {
        #region Обьявление глобальных переменных
        static Random RandomGenerator;
        static bool gameCycle = true, isMusicPlay;
        static GameStates currentState;
        static string[] MainMenuItems, RulesItems, RecordsItems, QuitItems, PlayersCountItems, BotOrPlayerItems, SplitItems, InGameActionsItems, GameSavesItems, MusicList, SessionLenghtsItems, AchivementsItems;
        static string musicPath, savesPath, recordsPath;
        static MenuItem[] OptionsItems;
        static int currentMenuPosition, botDelay;
        static SoundPlayer musicPlayer;
        static StartGameLoops StartGameLoop;
        static GameInfo currentGameInfo;
        static Dictionary<string, object[]> Achivements;
        #endregion

        #region Ядро программы
        static void Main(string[] args)
        {
            Initialization();
            MainLoop();
        }
        static void Initialization()
        {
            musicPath = @"Soundtrack\";
            savesPath = @"Saves\";
            recordsPath = @"Records\";
            FileStream configFile = new FileStream("config.ini", FileMode.OpenOrCreate);
            StreamReader configReader = new StreamReader(configFile);
            string startingMusicName = configReader.ReadLine();
            GameSavesItems = Directory.GetFiles(savesPath);
            string[] tempSaves = GameSavesItems;
            GameSavesItems = new string[10];
            for (int i = 0; i < tempSaves.Length; i++)
            {
                GameSavesItems[i] = tempSaves[i].Substring(savesPath.Length, tempSaves[i].Length - savesPath.Length - 3);
            }
            for (int i = tempSaves.Length; i < GameSavesItems.Length - 1; i++)
            {
                GameSavesItems[i] = "Слот " + (i+1);
                File.Create(savesPath + "Слот " + (i+1) + ".sv");
            }
            GameSavesItems[9] = "Вернуться назад";
            Console.Title = "BlackJack - Console Game Project";
            Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;
            Console.CursorVisible = false;
            Console.WindowWidth = 100;
            Console.BufferWidth = 100;
            Console.WindowHeight = 50;
            Console.BufferHeight = 50;
            RandomGenerator = new Random();
            currentState = GameStates.MainMenu;
            MainMenuItems = new string[8] { "Новая игра", "Загрузить игру", "Настройки" ,"Достижения", "О проекте и правилах игры", "Рекорды", "Хронология версий", "Выход" };
            RulesItems = new string[1] { "Вернуться назад" };
            RecordsItems = new string[1] { "Вернуться назад" };
            QuitItems = new string[2] { "Нет", "Да" };
            PlayersCountItems = new string[5] { "Один игрок", "Два игрока", "Три игрока", "Четыре игрока" , "Вернуться в главное меню" };
            BotOrPlayerItems = new string[3] { "Человек", "Бот", "Вернуться в главное меню" };
            SplitItems = new string[2] { "Разделить руку", "Не делить руку"};
            InGameActionsItems = new string[6] { "Взять карту", "Пропустить ход", "Разделить руку", "Покинуть стол", "Сохранить игру", "Покинуть игру" };
            SessionLenghtsItems = new string[6] { "Один раунд (1 раунд)", "Малая партия (5 раундов)", "Средняя партия (10 раундов)", "Большая партия (15 раундов)", "Наибольшая партия (25 раундов)", "Вернуться в главное меню" };
            AchivementsItems = new string[1] { "Вернуться назад" };
            MusicList = Directory.GetFiles(musicPath);
            string[] tempMusicList = MusicList;
            MusicList = new string[tempMusicList.Length];
            for (int i = 0; i < tempMusicList.Length; i++)
            {
                MusicList[i] = tempMusicList[i].Substring(musicPath.Length, tempMusicList[i].Length - musicPath.Length - 4);
            }
            if (MusicList.Length == 0) MusicList = null;
            currentGameInfo = new GameInfo
            {
                currentSplit = new byte[2]
            };
            currentMenuPosition = 0;
            botDelay = int.Parse(configReader.ReadLine());
            OptionsItems = new MenuItem[] {
                new MenuItem { label = "Выбрать музыку:", submenu = new Submenu { items = MusicList, currentItem = 0 } },
                new MenuItem { label = "Выбрать стиль ИИ ботов:", submenu = new Submenu { items = new string[2] { "Вдумчивый", "Быстрый" }, currentItem = botDelay == 200 ? 1 : 0 } },
                new MenuItem { label = "Обнулить достижения", submenu = new Submenu { items = null, currentItem = 0 } },
                new MenuItem { label = "Обнулить таблицу рекордов", submenu = new Submenu { items = null, currentItem = 0 } },
                new MenuItem { label = "Выключить\\Включить музыку", submenu = new Submenu { items = null, currentItem = 0 } },
                new MenuItem { label = "Выйти в главное меню", submenu = new Submenu { items = null, currentItem = 0 } }
            };
            isMusicPlay = bool.Parse(configReader.ReadLine());
            try
            {

                musicPlayer = new SoundPlayer(musicPath + startingMusicName + ".wav");
                musicPlayer.Load();
                if (isMusicPlay) musicPlayer.PlayLooping();
            }
            catch { }
            Achivements = new Dictionary<string, object[]>();
            string line;
            line = configReader.ReadLine(); Achivements.Add("Джекпот", new object[4] { "Взять 1 BlackJack", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 0 });
            line = configReader.ReadLine(); Achivements.Add("Везунчик", new object[4] { "Взять 10 BlackJack", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 1 });
            line = configReader.ReadLine(); Achivements.Add("Счастливчик", new object[4] { "Взять 50 BlackJack", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 2 });
            line = configReader.ReadLine(); Achivements.Add("Начинающий", new object[4] { "Победить 1 раз", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 3 });
            line = configReader.ReadLine(); Achivements.Add("Опытный", new object[4] { "Победить 10 раз", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 4 });
            line = configReader.ReadLine(); Achivements.Add("Мастер", new object[4] { "Победить 50 раз", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 5 });
            line = configReader.ReadLine(); Achivements.Add("Случайность", new object[4] { "Сыграть в ничью 1 раз", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 6 });
            line = configReader.ReadLine(); Achivements.Add("Закономерность", new object[4] { "Сыграть в ничью 10 раз", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 7 });
            line = configReader.ReadLine(); Achivements.Add("Отступление", new object[4] { "Покинуть стол 1 раз", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 8 });
            line = configReader.ReadLine(); Achivements.Add("Поражение", new object[4] { "Покинуть стол 10 раз", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 9 });
            line = configReader.ReadLine(); Achivements.Add("Король фортуны", new object[4] { "Набрать 110 баллов в наибольшей партии", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 10 });
            line = configReader.ReadLine(); Achivements.Add("Этому дну нужен новый герой", new object[4] { "Пройграть все раунды в наибольшей партии", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 11 });
            line = configReader.ReadLine(); Achivements.Add("Фантом", new object[4] { "Закончить большую или наибольшую партию, имея 0 баллов", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 12 });
            line = configReader.ReadLine(); Achivements.Add("Инвалид", new object[4] { "Первый раз сделать разделение руки", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 13 });
            line = configReader.ReadLine(); Achivements.Add("Всё себе", new object[4] { "Получить везде в разделенной руке BlackJack", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 14 });
            line = configReader.ReadLine(); Achivements.Add("Вне правил", new object[4] { "На раздаче карт получить BlackJack", bool.Parse(line.Split(' ')[0]), int.Parse(line.Split(' ')[1]), 15 });
            configReader.Close();
            configFile.Close();
        }
        static void MainLoop()
        {
            while (gameCycle)
            {
                Console.Clear();
                switch (currentState)
                {
                    case GameStates.MainMenu:
                        MainMenuLoop();
                        break;
                    case GameStates.Game:
                        GameLoop();
                        break;
                    case GameStates.LoadGame:
                        SaveStatesGameLoop();
                        break;
                    case GameStates.SaveGame:
                        SaveStatesGameLoop(true);
                        break;
                    case GameStates.Options:
                        OptionsLoop();
                        break;
                    case GameStates.Achivements:
                        AchivementsLoop();
                        break;
                    case GameStates.Rules:
                        RulesLoop();
                        break;
                    case GameStates.Records:
                        RecordsLoop();
                        break;
                    case GameStates.UpdateList:
                        UpdateListLoop();
                        break;
                    case GameStates.Quit:
                        QuitLoop();
                        break;
                }
            }
        }
        #endregion

        #region Методы, отвечающие за некоторую логику
        static void SetDeck()
        {
            currentGameInfo.Deck = new List<Card>();
            foreach(Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach(Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    Card card = new Card
                    {
                        rank = rank,
                        suit = suit,
                        side = true
                    };
                    currentGameInfo.Deck.Add(card);
                }
            }
        }
        static void ShuffleDeck()
        {
            for (int i = 0; i < currentGameInfo.Deck.Count*4; i++)
            {
                Card tmp = currentGameInfo.Deck[0];
                currentGameInfo.Deck.RemoveAt(0);
                currentGameInfo.Deck.Insert(RandomGenerator.Next(currentGameInfo.Deck.Count), tmp);
            }
        }
        static int CalculateSplitCost(List<Card> cards)
        {
            int score = 0;
            foreach (Card card in cards)
            {
                switch (card.rank)
                {
                    case Rank.Two:
                        score += 2;
                        break;
                    case Rank.Three:
                        score += 3;
                        break;
                    case Rank.Four:
                        score += 4;
                        break;
                    case Rank.Five:
                        score += 5;
                        break;
                    case Rank.Six:
                        score += 6;
                        break;
                    case Rank.Seven:
                        score += 7;
                        break;
                    case Rank.Eight:
                        score += 8;
                        break;
                    case Rank.Nine:
                        score += 9;
                        break;
                    case Rank.Ten:
                        score += 10;
                        break;
                    case Rank.Jack:
                        score += 10;
                        break;
                    case Rank.Queen:
                        score += 10;
                        break;
                    case Rank.King:
                        score += 10;
                        break;
                }
            }
            foreach (Card card in cards){
                if (card.rank == Rank.Ace)
                {
                    if (score <= 10) score += 11;
                    else score += 1;
                }
            }
            return score;
        }
        static int CalculateScore(byte[] totalScores)
        {
            return totalScores[0] * 4 + totalScores[1] * 2 + totalScores[2] * 0 + totalScores[3] * (-1) + totalScores[4] * (-2);
        }
        static int CalculateStartingScore()
        {
            int startingScore = 0;
            List<int> allScores = new List<int>();
            foreach (Player player in currentGameInfo.Players)
            {
                foreach (List<Card> split in player.splits)
                {
                    if (CalculateSplitCost(split) > 15 && CalculateSplitCost(split) < 21)
                    {
                        allScores.Add(CalculateSplitCost(split));
                    }
                }
            }
            foreach (int score in allScores)
            {
                startingScore += score;
            }
            if (startingScore == 0)
            {
                bool ifAllWinOrLose = true;
                foreach (Player player in currentGameInfo.Players)
                {
                    foreach (SessionStates state in player.sessionStates)
                    {
                        if (state == SessionStates.InGame)
                        {
                            ifAllWinOrLose = false;
                        }
                    }
                    if (ifAllWinOrLose) startingScore = 0; else startingScore = 16;
                }
            }
            else
            {
                startingScore /= allScores.Count;
                startingScore = (int)Math.Ceiling((decimal)startingScore);
            }
            return startingScore;
        }
        static void NextPlayer()
        {
            if (currentGameInfo.currentSplit[1] + 1 == currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits.Count)
            {
                if (!currentGameInfo.Players[currentGameInfo.currentSplit[0]].isBot && currentGameInfo.achiveEnabled)
                {
                    if (!(bool)Achivements["Всё себе"][1] && currentGameInfo.Players[currentGameInfo.currentSplit[0]].sessionStates.Count == 2)
                    {
                        if (currentGameInfo.Players[currentGameInfo.currentSplit[0]].sessionStates[0] == SessionStates.BlackJack && currentGameInfo.Players[currentGameInfo.currentSplit[0]].sessionStates[1] == SessionStates.BlackJack)
                        {
                            Achivements["Всё себе"][1] = true;
                            RewriteAchivement((int)Achivements["Всё себе"][3], true, (int)Achivements["Всё себе"][2]);
                            currentGameInfo.completedAchivements.Add("Всё себе");
                        }
                    }
                }
                if (currentGameInfo.currentSplit[0] + 1 == currentGameInfo.Players.Length)
                {
                    currentGameInfo.currentSplit[0] = 0;
                    currentGameInfo.houseTurn = true;
                    Card tempCard = currentGameInfo.House.splits[0][1];
                    tempCard.side = true;
                    currentGameInfo.House.splits[0][1] = tempCard;
                    currentGameInfo.GameLogicLoop = GameLogicLoops.HouseTurn;
                }
                else currentGameInfo.currentSplit[0]++;
                currentGameInfo.currentSplit[1] = 0;
            }
            else currentGameInfo.currentSplit[1]++;
        }
        static void TakeCard(Player player)
        {
            currentGameInfo.currentCard = RandomGenerator.Next(currentGameInfo.Deck.Count);
            player.splits[currentGameInfo.currentSplit[1]].Add(currentGameInfo.Deck[currentGameInfo.currentCard]);
            currentGameInfo.Deck.RemoveAt(currentGameInfo.currentCard);
            if (CalculateSplitCost(player.splits[currentGameInfo.currentSplit[1]]) == 21 && player.sessionStates[currentGameInfo.currentSplit[1]] != SessionStates.BlackJack)
            {
                player.sessionStates[currentGameInfo.currentSplit[1]] = SessionStates.BlackJack;
                player.totalScores[0]++;
                if (!player.isBot && currentGameInfo.achiveEnabled)
                {
                    Achivements["Везунчик"][2] = (int)Achivements["Везунчик"][2] + 1;
                    RewriteAchivement((int)Achivements["Везунчик"][3], (bool)Achivements["Везунчик"][1], (int)Achivements["Везунчик"][2]);
                    Achivements["Счастливчик"][2] = (int)Achivements["Счастливчик"][2] + 1;
                    RewriteAchivement((int)Achivements["Счастливчик"][3], (bool)Achivements["Счастливчик"][1], (int)Achivements["Счастливчик"][2]);
                    if (!(bool)Achivements["Джекпот"][1])
                    {
                        Achivements["Джекпот"][1] = true;
                        RewriteAchivement((int)Achivements["Джекпот"][3], true, (int)Achivements["Джекпот"][2]);
                        currentGameInfo.completedAchivements.Add("Джекпот");
                    }
                    if (!(bool)Achivements["Везунчик"][1] && (int)Achivements["Везунчик"][2] >= 10)
                    {
                        Achivements["Везунчик"][1] = true;
                        RewriteAchivement((int)Achivements["Везунчик"][3], true, (int)Achivements["Везунчик"][2]);
                        currentGameInfo.completedAchivements.Add("Везунчик");
                    }
                    if (!(bool)Achivements["Счастливчик"][1] && (int)Achivements["Счастливчик"][2] >= 50)
                    {
                        Achivements["Счастливчик"][1] = true;
                        RewriteAchivement((int)Achivements["Счастливчик"][3], true, (int)Achivements["Счастливчик"][2]);
                        currentGameInfo.completedAchivements.Add("Счастливчик");
                    }
                }
            }
            if (CalculateSplitCost(player.splits[currentGameInfo.currentSplit[1]]) > 21 && player.sessionStates[currentGameInfo.currentSplit[1]] != SessionStates.Lose)
            {
                player.sessionStates[currentGameInfo.currentSplit[1]] = SessionStates.Lose;
                player.totalScores[4]++;
            }
        }
        static void InitializePlayers(int count)
        {
            currentGameInfo.Players = new Player[count];
            for (int i = 0; i < currentGameInfo.Players.Length; i++)
            {
                currentGameInfo.Players[i] = new Player
                {
                    splits = new List<List<Card>>()
                };
                currentGameInfo.Players[i].splits.Add(new List<Card>());
                currentGameInfo.Players[i].sessionStates = new List<SessionStates>
                {
                    new SessionStates()
                };
                currentGameInfo.Players[i].totalScores = new byte[5];
            }
        }
        static void PrepareGameSession()
        {
            for (int i = 0; i < currentGameInfo.Players.Length; i++)
            {
                currentGameInfo.Players[i].splits = new List<List<Card>>
                {
                    new List<Card>()
                };
                currentGameInfo.Players[i].sessionStates = new List<SessionStates>
                {
                    new SessionStates()
                };
                currentGameInfo.Players[i].sessionStates[0] = SessionStates.InGame;
            }
            currentGameInfo.House.splits = new List<List<Card>>
            {
                new List<Card>()
            };
            currentGameInfo.House.sessionStates = new List<SessionStates> { SessionStates.InGame };
            SetDeck();
            ShuffleDeck();
            currentGameInfo.currentCard = RandomGenerator.Next(currentGameInfo.Deck.Count);
            currentGameInfo.House.splits[0].Add(currentGameInfo.Deck[currentGameInfo.currentCard]);
            currentGameInfo.Deck.RemoveAt(currentGameInfo.currentCard);
            currentGameInfo.currentCard = RandomGenerator.Next(currentGameInfo.Deck.Count);
            currentGameInfo.House.splits[0].Add(currentGameInfo.Deck[currentGameInfo.currentCard]);
            Card tempCard = currentGameInfo.House.splits[0][1];
            tempCard.side = false;
            currentGameInfo.House.splits[0][1] = tempCard;
            currentGameInfo.Deck.RemoveAt(currentGameInfo.currentCard);
            foreach (Player player in currentGameInfo.Players)
            {
                currentGameInfo.currentCard = RandomGenerator.Next(currentGameInfo.Deck.Count);
                player.splits[0].Add(currentGameInfo.Deck[currentGameInfo.currentCard]);
                currentGameInfo.Deck.RemoveAt(currentGameInfo.currentCard);
                currentGameInfo.currentCard = RandomGenerator.Next(currentGameInfo.Deck.Count);
                player.splits[0].Add(currentGameInfo.Deck[currentGameInfo.currentCard]);
                currentGameInfo.Deck.RemoveAt(currentGameInfo.currentCard);
                currentGameInfo.houseTurn = false;
                currentGameInfo.houseEnough = false;
                currentGameInfo.houseEnds = false;
                currentGameInfo.playerEnough = false;
            }
            currentGameInfo.currentSplit = new byte[] { 0, 0 };
        }
        static void LoadState(int slot)
        {
            FileStream SaveSlot = new FileStream(savesPath + GameSavesItems[slot] + ".sv", FileMode.OpenOrCreate);
            if (SaveSlot.Length != 0)
            {
                BinaryReader SaveLoader = new BinaryReader(SaveSlot);
                int deckLength = SaveLoader.ReadInt32();
                currentGameInfo.Deck = new List<Card>();
                for (int i = 0; i < deckLength; i++)
                {
                    Card tempCard = new Card
                    {
                        rank = (Rank)Enum.Parse(typeof(Rank), Enum.GetName(typeof(Rank), SaveLoader.ReadInt32())),
                        suit = (Suit)Enum.Parse(typeof(Suit), Enum.GetName(typeof(Suit), SaveLoader.ReadInt32())),
                        side = SaveLoader.ReadBoolean()
                    };
                    currentGameInfo.Deck.Add(tempCard);
                }
                int playersLenght = SaveLoader.ReadInt32();
                currentGameInfo.Players = new Player[playersLenght];
                for (int i = 0; i < playersLenght; i++)
                {
                    currentGameInfo.Players[i].name = SaveLoader.ReadString();
                    currentGameInfo.Players[i].isBot = SaveLoader.ReadBoolean();
                    int splitsCount = SaveLoader.ReadInt32();
                    currentGameInfo.Players[i].splits = new List<List<Card>>();
                    for (int j = 0; j < splitsCount; j++)
                    {
                        int splitCount = SaveLoader.ReadInt32();
                        currentGameInfo.Players[i].splits.Add(new List<Card>());
                        for (int k = 0; k < splitCount; k++)
                        {
                            Card tempCard = new Card
                            {
                                rank = (Rank)Enum.Parse(typeof(Rank), Enum.GetName(typeof(Rank), SaveLoader.ReadInt32())),
                                suit = (Suit)Enum.Parse(typeof(Suit), Enum.GetName(typeof(Suit), SaveLoader.ReadInt32())),
                                side = SaveLoader.ReadBoolean()
                            };
                            currentGameInfo.Players[i].splits[j].Add(tempCard);
                        }
                    }
                    int sessionStatesCount = SaveLoader.ReadInt32();
                    currentGameInfo.Players[i].sessionStates = new List<SessionStates>();
                    for (int j = 0; j < sessionStatesCount; j++)
                    {
                        currentGameInfo.Players[i].sessionStates.Add((SessionStates)Enum.Parse(typeof(SessionStates), Enum.GetName(typeof(SessionStates), SaveLoader.ReadInt32())));
                    }
                    currentGameInfo.Players[i].totalScores = SaveLoader.ReadBytes(5);
                }
                currentGameInfo.currentSplit = SaveLoader.ReadBytes(2);
                currentGameInfo.currentRound = SaveLoader.ReadInt32();
                currentGameInfo.houseEnds = SaveLoader.ReadBoolean();
                currentGameInfo.houseEnough = SaveLoader.ReadBoolean();
                currentGameInfo.houseTurn = SaveLoader.ReadBoolean();
                currentGameInfo.playerEnough = SaveLoader.ReadBoolean();
                currentGameInfo.House.name = SaveLoader.ReadString();
                currentGameInfo.House.isBot = SaveLoader.ReadBoolean();
                currentGameInfo.House.splits = new List<List<Card>>();
                int houseSplitCount = SaveLoader.ReadInt32();
                currentGameInfo.House.splits.Add(new List<Card>());
                for (int k = 0; k < houseSplitCount; k++)
                {
                    Card tempCard = new Card
                    {
                        rank = (Rank)Enum.Parse(typeof(Rank), Enum.GetName(typeof(Rank), SaveLoader.ReadInt32())),
                        suit = (Suit)Enum.Parse(typeof(Suit), Enum.GetName(typeof(Suit), SaveLoader.ReadInt32())),
                        side = SaveLoader.ReadBoolean()
                    };
                    currentGameInfo.House.splits[0].Add(tempCard);
                }
                int houseSessionStatesCount = SaveLoader.ReadInt32();
                currentGameInfo.House.sessionStates = new List<SessionStates>();
                for (int j = 0; j < houseSessionStatesCount; j++)
                {
                    currentGameInfo.House.sessionStates.Add((SessionStates)Enum.Parse(typeof(SessionStates), Enum.GetName(typeof(SessionStates), SaveLoader.ReadInt32())));
                }
                currentGameInfo.currentRound = SaveLoader.ReadInt32();
                currentGameInfo.House.totalScores = new byte[5];
                currentGameInfo.currentSessionLenght = (SessionLenght)Enum.Parse(typeof(SessionLenght), Enum.GetName(typeof(SessionLenght), SaveLoader.ReadInt32()));
                currentGameInfo.GameLogicLoop = (GameLogicLoops)Enum.Parse(typeof(GameLogicLoops), Enum.GetName(typeof(GameLogicLoops), SaveLoader.ReadInt32()));
                currentGameInfo.achiveEnabled = SaveLoader.ReadBoolean();
                int achivesLenght = SaveLoader.ReadInt32();
                currentGameInfo.completedAchivements = new List<string>();
                for (int i = 0; i < achivesLenght; i++)
                {
                    currentGameInfo.completedAchivements.Add(SaveLoader.ReadString());
                }
                SaveLoader.Close();
                currentState = GameStates.Game;
                currentMenuPosition = 0;
            }
            SaveSlot.Close();
        }
        static void SaveState(int slot)
        {
            FileStream SaveSlot = new FileStream(savesPath + GameSavesItems[slot] + ".sv", FileMode.Open);
            BinaryWriter SaveWriter = new BinaryWriter(SaveSlot);
            SaveWriter.Write(currentGameInfo.Deck.Count);
            foreach (Card card in currentGameInfo.Deck)
            {
                SaveWriter.Write((int)card.rank);
                SaveWriter.Write((int)card.suit);
                SaveWriter.Write(card.side);
            }
            SaveWriter.Write(currentGameInfo.Players.Length);
            foreach (Player player in currentGameInfo.Players)
            {
                SaveWriter.Write(player.name);
                SaveWriter.Write(player.isBot);
                SaveWriter.Write(player.splits.Count);
                foreach (List<Card> split in player.splits)
                {
                    SaveWriter.Write(split.Count);
                    foreach (Card card in split)
                    {
                        SaveWriter.Write((int)card.rank);
                        SaveWriter.Write((int)card.suit);
                        SaveWriter.Write(card.side);
                    }
                }
                SaveWriter.Write(player.sessionStates.Count);
                foreach (SessionStates sessionState in player.sessionStates)
                {
                    SaveWriter.Write((int)sessionState);
                }
                SaveWriter.Write(player.totalScores);
            }
            SaveWriter.Write(currentGameInfo.currentSplit);
            SaveWriter.Write(currentGameInfo.currentRound);
            SaveWriter.Write(currentGameInfo.houseEnds);
            SaveWriter.Write(currentGameInfo.houseEnough);
            SaveWriter.Write(currentGameInfo.houseTurn);
            SaveWriter.Write(currentGameInfo.playerEnough);
            SaveWriter.Write(currentGameInfo.House.name);
            SaveWriter.Write(currentGameInfo.House.isBot);
            foreach (List<Card> split in currentGameInfo.House.splits)
            {
                SaveWriter.Write(split.Count);
                foreach (Card card in split)
                {
                    SaveWriter.Write((int)card.rank);
                    SaveWriter.Write((int)card.suit);
                    SaveWriter.Write(card.side);
                }
            }
            SaveWriter.Write(currentGameInfo.House.sessionStates.Count);
            foreach (SessionStates sessionState in currentGameInfo.House.sessionStates)
            {
                SaveWriter.Write((int)sessionState);
            }
            SaveWriter.Write(currentGameInfo.currentRound);
            SaveWriter.Write((int)currentGameInfo.currentSessionLenght);
            SaveWriter.Write((int)currentGameInfo.GameLogicLoop);
            SaveWriter.Write(currentGameInfo.achiveEnabled);
            SaveWriter.Write(currentGameInfo.completedAchivements.Count);
            foreach (string achive in currentGameInfo.completedAchivements)
            {
                SaveWriter.Write(achive);
            }
            SaveWriter.Close();
            SaveSlot.Close();
            if (GameSavesItems[slot].Length < 8)
            {
                File.Move(savesPath + GameSavesItems[slot] + ".sv", savesPath + GameSavesItems[slot] + " (занято).sv");
                GameSavesItems[slot] = GameSavesItems[slot] + " (занято)";
            }
        }
        static void RewriteConfigFile(string value, int position)
        {
            FileStream configFile = new FileStream("config.ini", FileMode.Open);
            StreamReader configReader = new StreamReader(configFile);
            string[] tempConfig = new string[19];
            {
                string line; int i = 0;
                while ((line = configReader.ReadLine()) != null || i < 19) tempConfig[i++] = line;
            }
            configReader.Close();
            configFile.Close();
            tempConfig[position] = value;
            configFile = new FileStream("config.ini", FileMode.Create);
            StreamWriter configWriter = new StreamWriter(configFile);
            foreach (string line in tempConfig) configWriter.WriteLine(line);
            configWriter.Close();
            configFile.Close();
        }
        static void RewriteAchivement(int position, bool value, int count)
        {
            FileStream configFile = new FileStream("config.ini", FileMode.Open);
            StreamReader configReader = new StreamReader(configFile);
            string[] tempConfig = new string[19];
            {
                string line; int i = 0;
                while ((line = configReader.ReadLine()) != null || i < 19) tempConfig[i++] = line;
            }
            configReader.Close();
            configFile.Close();
            tempConfig[3 + position] = value.ToString() + " " + count;
            configFile = new FileStream("config.ini", FileMode.Create);
            StreamWriter configWriter = new StreamWriter(configFile);
            foreach (string line in tempConfig) configWriter.WriteLine(line);
            configWriter.Close();
            configFile.Close();
        }
        static string[] ReadRecordFile(string path)
        {
            string[] recordsData = new string[3];
            FileStream RecordsFile = new FileStream(recordsPath + path, FileMode.OpenOrCreate);
            StreamReader RecordsReader = new StreamReader(RecordsFile);
            for (int i = 0; i < recordsData.Length; i++) recordsData[i] = RecordsReader.ReadLine();
            RecordsReader.Close();
            RecordsFile.Close();
            return recordsData;
        }
        static void RewriteRecords(Player player, SessionLenght lenght, ref bool checkNewRecords)
        {
            string path = "";
            switch (lenght)
            {
                case SessionLenght.One:
                    path = "One.txt";
                    break;
                case SessionLenght.Small:
                    path = "Small.txt";
                    break;
                case SessionLenght.Medium:
                    path = "Medium.txt";
                    break;
                case SessionLenght.Large:
                    path = "Large.txt";
                    break;
                case SessionLenght.BigGame:
                    path = "BigGame.txt";
                    break;
            }
            string[] recordsData = ReadRecordFile(path);
            string[] newRecordsData = new string[3];
            int position = -1;
            for (int i = 0; i < recordsData.Length; i++)
            {
                if (CalculateScore(player.totalScores) > int.Parse(recordsData[i].Split((char)9830)[1]))
                {
                    checkNewRecords = true;
                    position = i;
                    break;
                }
            }
            if (position != -1)
            {
                for (int i = 0; i < position; i++)
                {
                    newRecordsData[i] = recordsData[i];
                }
                newRecordsData[position] = $"{player.name}{((char)9830)}{CalculateScore(player.totalScores)}{((char)9830)}{player.totalScores[0]}{((char)9830)}{player.totalScores[1]}{((char)9830)}{player.totalScores[2]}{((char)9830)}{player.totalScores[3]}{((char)9830)}{player.totalScores[4]}";
                for (int i = position + 1; i < newRecordsData.Length; i++)
                {
                    newRecordsData[i] = recordsData[i - 1];
                }
                FileStream RecordsFile = new FileStream(recordsPath + path, FileMode.Create);
                StreamWriter RecordsWriter = new StreamWriter(RecordsFile);
                foreach (string line in newRecordsData) RecordsWriter.WriteLine(line);
                RecordsWriter.Close();
                RecordsFile.Close();
            }
        }
        #endregion

        #region Методы, отвечающие за отрисовку
        static void VerticalMenuRender(string[] menu)
        {
            for (int i = 0; i < menu.Length; i++)
            {
                Console.Write(" ");
                string pref = "──";
                if (i == currentMenuPosition)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    pref = ">>";
                }
                Console.WriteLine(pref + menu[i]);
                Console.ResetColor();
            }
        }
        static void VerticalPostMenuRender(MenuItem[] menu)
        {
            int i = 0;
            foreach (var item in menu)
            {
                Console.Write(" ");
                string pref = "──";
                if (i == currentMenuPosition)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    pref = ">>";
                }
                Console.Write(pref + item.label);
                if (item.submenu.items != null)
                {
                    Console.CursorLeft = 40;
                    Console.Write("<");
                    Console.CursorLeft = 42;
                    Console.Write(item.submenu.items[item.submenu.currentItem]);
                    Console.CursorLeft = 98;
                    Console.Write(">\n");
                }
                else
                {
                    Console.Write("\n");
                }
                Console.ResetColor();
                i++;
            }
        }
        static void HorizontalMenuRender(string[] menu)
        {
            Console.Write("  ");
            for (int i = 0; i < menu.Length; i++)
            {
                string pref = "─", post = "─";
                if (i == currentMenuPosition)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    pref = ">";
                    post = "<";
                }
                Console.Write(pref + menu[i] + post);
                Console.ResetColor();
                Console.Write(" ");
            }
        }
        static void CardRender(string playerName, bool isBot, List<Card> cards, bool plays, SessionStates state, byte[] totalScores = null)
        {
            if (playerName != null)
            {
                if (playerName == "D") Console.Write(" Карты дилера:\n");
                else
                {
                    if (isBot) Console.Write($" Карты бота {playerName}:");
                    else Console.Write($" Карты игрока {playerName}:");
                    Console.CursorLeft = 70;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"Баллы: {CalculateScore(totalScores)}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.CursorLeft = 85;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write($"{totalScores[0],2}");
                    Console.ResetColor();
                    Console.Write("|");
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"{totalScores[1],2}");
                    Console.ResetColor();
                    Console.Write("|");
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{totalScores[2],2}");
                    Console.ResetColor();
                    Console.Write("|");
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.Write($"{totalScores[3],2}");
                    Console.ResetColor();
                    Console.Write("|");
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write($"{totalScores[4],2}");
                    Console.ResetColor();
                    Console.Write("\n");
                }
            }
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.White;
            for (int curCard = 0; curCard < cards.Count; curCard++)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write(@"╔══╗");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }
            Console.CursorLeft = 70;
            if (plays)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("/" + new string('█', 28));
                Console.ForegroundColor = ConsoleColor.White;
            }
            else Console.Write(@"\");
            Console.Write("\n ");
            for (int curCard = 0; curCard < cards.Count; curCard++)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write("║");
                if (cards[curCard].side)
                {
                    if (cards[curCard].suit == Suit.Hearts || cards[curCard].suit == Suit.Diamonds) Console.ForegroundColor = ConsoleColor.DarkRed;
                    else Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write((char)cards[curCard].rank);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("·║");
                }
                else
                {
                    Console.Write("##║");
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }
            Console.CursorLeft = 70;
            if (plays) Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("█ ");
            Console.ForegroundColor = ConsoleColor.White;
            if (playerName != "D" || plays) {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"Очки: {CalculateSplitCost(cards)}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.Write("\n ");
            for (int curCard = 0; curCard < cards.Count; curCard++)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                if (cards[curCard].side)
                {
                    Console.Write("║·");
                    if (cards[curCard].suit == Suit.Hearts || cards[curCard].suit == Suit.Diamonds) Console.ForegroundColor = ConsoleColor.DarkRed;
                    else Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write((char)cards[curCard].suit);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.Write("║##");
                }
                Console.Write("║");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }
            Console.CursorLeft = 70;
            if (plays) Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("█ ");
            Console.ForegroundColor = ConsoleColor.White;
            if (state == SessionStates.Victory)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.Write(" ПОБЕДА ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            if (state == SessionStates.BlackJack)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write(" BLACKJACK ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            if (state == SessionStates.Lose)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write(" ПРОЙГРЫШ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            if (state == SessionStates.Leave)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write(" НЕ УЧАСТВУЕТ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            if (state == SessionStates.Draw)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.Write(" НИЧЬЯ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            Console.Write("\n ");
            for (int curCard = 0; curCard < cards.Count; curCard++)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write(@"╚══╝");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }
            Console.CursorLeft = 70;
            if (plays)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\\" + new string('█', 28));
                Console.ForegroundColor = ConsoleColor.White;
            }
            else Console.Write(@"\");
            Console.Write("\n");
            Console.ResetColor();
        }
        static void LogoRender()
        {
            Console.WriteLine("\n\n\n\n");
            Console.WriteLine(" " + new string('─', 26) + "██████████████████████████████████████████████" + new string('─', 26) + "\n" +
                              " " + new string('─', 26) + "█────██─███────█────█─██─███──█────█────█─██─█" + new string('─', 26) + "\n" +
                              " " + new string('─', 26) + "█─██──█─███─██─█─██─█─█─█████─█─██─█─██─█─█─██" + new string('─', 26) + "\n" +
                              " " + new string('─', 26) + "█────██─███────█─████──██████─█────█─████──███" + new string('─', 26) + "\n" +
                              " " + new string('─', 26) + "█─██──█─███─██─█─██─█─█─██─██─█─██─█─██─█─█─██" + new string('─', 26) + "\n" +
                              " " + new string('─', 26) + "█────██───█─██─█────█─██─█────█─██─█────█─██─█" + new string('─', 26) + "\n" +
                              " " + new string('─', 26) + "██████████████████████████████████████████████" + new string('─', 26) + "\n\n\n");
        }
        static void GameFieldRender()
        {
            Console.WriteLine();
            CardRender("D", true, currentGameInfo.House.splits[0], (currentGameInfo.houseTurn || currentGameInfo.houseEnds), currentGameInfo.House.sessionStates[0]);
            Console.WriteLine();
            for (int j = 0; j < currentGameInfo.Players.Length; j++)
            {
                for (int i = 0; i < currentGameInfo.Players[j].splits.Count; i++)
                {
                    bool check = false;
                    if (j == currentGameInfo.currentSplit[0] && i == currentGameInfo.currentSplit[1] && (!currentGameInfo.houseTurn && !currentGameInfo.houseEnds)) check = true;
                    string renderName;
                    if (i == 0)
                        renderName = currentGameInfo.Players[j].name;
                    else
                        renderName = null;
                    CardRender(renderName, currentGameInfo.Players[j].isBot,  currentGameInfo.Players[j].splits[i], check, currentGameInfo.Players[j].sessionStates[i], currentGameInfo.Players[j].totalScores);
                }
                Console.WriteLine();
            }
            Console.CursorTop = 46;
            Console.WriteLine(" " + new string('─', 98) + "\n");
        }
        static void CleanMessageBox()
        {
            Console.CursorLeft = 2;
            Console.Write(new string(' ', 97));
            Console.CursorLeft = 2;
        }
        static void RecordsRender(string prefix, string[] recordsData)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(" " + new string('█', 98));
            Console.Write(" █");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Write(prefix + new string(' ', 96 - prefix.Length));
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("█");
            Console.ResetColor();
            Console.Write("\n");
            for (int i = 0; i < recordsData.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write(" █");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{i+1})");
                Console.ResetColor();
                Console.Write(" " + recordsData[i].Split((char)9830)[0]);
                Console.CursorLeft = 21;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Баллы: " + recordsData[i].Split((char)9830)[1], 2);
                Console.ResetColor();
                Console.CursorLeft = 32;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write($"BlackJacks: {recordsData[i].Split((char)9830)[2],2}");
                Console.ResetColor();
                Console.Write("|");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.Write($"Побед: {recordsData[i].Split((char)9830)[3],2}");
                Console.ResetColor();
                Console.Write("|");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.Write($"В ничью: {recordsData[i].Split((char)9830)[4],2}");
                Console.ResetColor();
                Console.Write("|");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write($"Безучастий: {recordsData[i].Split((char)9830)[5],2}");
                Console.ResetColor();
                Console.Write("|");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write($"Пройгрышей: {recordsData[i].Split((char)9830)[6],2}");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write("█");
                Console.ResetColor();
                Console.Write("\n");
            }
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(" " + new string('█', 98));
            Console.ResetColor();
        }
        #endregion

        #region Основные циклы программы
        static void MainMenuLoop()
        {
            LogoRender();
            VerticalMenuRender(MainMenuItems);
            Console.CursorLeft = 1;
            Console.CursorTop = 49;
            Console.WriteLine("v1.0.2 - 2019 - DRON12261");
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    currentMenuPosition--;
                    if (currentMenuPosition < 0) currentMenuPosition = MainMenuItems.Length - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentMenuPosition++;
                    if (currentMenuPosition > MainMenuItems.Length - 1) currentMenuPosition = 0;
                    break;
                case ConsoleKey.Enter:
                    switch (currentMenuPosition)
                    {
                        case 0:
                            currentMenuPosition = 0;
                            currentState = GameStates.Game;
                            currentGameInfo.GameLogicLoop = GameLogicLoops.GameInitialization;
                            break;
                        case 1:
                            currentMenuPosition = 0;
                            currentState = GameStates.LoadGame;
                            break;
                        case 2:
                            currentMenuPosition = 0;
                            currentState = GameStates.Options;
                            break;
                        case 3:
                            currentMenuPosition = 0;
                            currentState = GameStates.Achivements;
                            break;
                        case 4:
                            currentMenuPosition = 0;
                            currentState = GameStates.Rules;
                            break;
                        case 5:
                            currentMenuPosition = 0;
                            currentState = GameStates.Records;
                            break;
                        case 6:
                            currentMenuPosition = 0;
                            currentState = GameStates.UpdateList;
                            break;
                        case 7:
                            currentMenuPosition = 0;
                            currentState = GameStates.Quit;
                            break;
                    }
                    break;
            }
        }
        static void GameLoop()
        {
            switch (currentGameInfo.GameLogicLoop)
            {
                case GameLogicLoops.GameInitialization:
                    StartGameLoop = StartGameLoops.PlayersListGeneration;
                    StartGame();
                    break;
                case GameLogicLoops.PreparingGameSession:
                    PrepareGameSession();
                    currentGameInfo.GameLogicLoop = GameLogicLoops.PlayersTurn;
                    break;
                case GameLogicLoops.PlayersTurn:
                    foreach (Player player in currentGameInfo.Players)
                    {
                        for (int i = 0; i < player.splits.Count; i++)
                        {
                            if (CalculateSplitCost(player.splits[i]) == 21 && player.sessionStates[i] != SessionStates.BlackJack)
                            {
                                player.sessionStates[i] = SessionStates.BlackJack;
                                player.totalScores[0]++;
                                if (!player.isBot && currentGameInfo.achiveEnabled)
                                {
                                    Achivements["Везунчик"][2] = (int)Achivements["Везунчик"][2] + 1;
                                    RewriteAchivement((int)Achivements["Везунчик"][3], (bool)Achivements["Везунчик"][1], (int)Achivements["Везунчик"][2]);
                                    Achivements["Счастливчик"][2] = (int)Achivements["Счастливчик"][2] + 1;
                                    RewriteAchivement((int)Achivements["Счастливчик"][3], (bool)Achivements["Счастливчик"][1], (int)Achivements["Счастливчик"][2]);
                                    if (!(bool)Achivements["Джекпот"][1])
                                    {
                                        Achivements["Джекпот"][1] = true;
                                        RewriteAchivement((int)Achivements["Джекпот"][3], true, (int)Achivements["Джекпот"][2]);
                                        currentGameInfo.completedAchivements.Add("Джекпот");
                                    }
                                    if (!(bool)Achivements["Везунчик"][1] && (int)Achivements["Везунчик"][2] >= 10)
                                    {
                                        Achivements["Везунчик"][1] = true;
                                        RewriteAchivement((int)Achivements["Везунчик"][3], true, (int)Achivements["Везунчик"][2]);
                                        currentGameInfo.completedAchivements.Add("Везунчик");
                                    }
                                    if (!(bool)Achivements["Счастливчик"][1] && (int)Achivements["Счастливчик"][2] >= 50)
                                    {
                                        Achivements["Счастливчик"][1] = true;
                                        RewriteAchivement((int)Achivements["Счастливчик"][3], true, (int)Achivements["Счастливчик"][2]);
                                        currentGameInfo.completedAchivements.Add("Счастливчик");
                                    }
                                    if (!(bool)Achivements["Вне правил"][1])
                                    {
                                        Achivements["Вне правил"][1] = true;
                                        RewriteAchivement((int)Achivements["Вне правил"][3], true, (int)Achivements["Вне правил"][2]);
                                        currentGameInfo.completedAchivements.Add("Вне правил");
                                    }
                                }
                            }
                            if (CalculateSplitCost(player.splits[i]) > 21 && player.sessionStates[i] != SessionStates.Lose)
                            {
                                player.sessionStates[i] = SessionStates.Lose;
                                player.totalScores[4]++;
                            }
                        }
                    }
                    GameFieldRender();
                    if (currentGameInfo.Players[currentGameInfo.currentSplit[0]].sessionStates[currentGameInfo.currentSplit[1]] == SessionStates.InGame)
                    {
                        if (currentGameInfo.Players[currentGameInfo.currentSplit[0]].isBot)
                        {
                            bool botIfLeave = false;
                            if (currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]].Count == 2 && currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits.Count == 1)
                            {
                                int choise = RandomGenerator.Next(0, 101);
                                if (CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) >= 13 && CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) < 16)
                                {
                                    if (choise < 15)
                                    {
                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].sessionStates[currentGameInfo.currentSplit[1]] = SessionStates.Leave;
                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].totalScores[3]++;
                                        botIfLeave = true;
                                    }
                                }
                            }
                            if (CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) < 21 && !botIfLeave && currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits.Count == 1)
                            {
                                bool searchEqualsRank = false;
                                foreach (Card card1 in currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]])
                                {
                                    foreach (Card card2 in currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]])
                                    {
                                        if (!card1.Equals(card2))
                                        {
                                            if (card1.rank == card2.rank)
                                            {
                                                searchEqualsRank = true;
                                            }
                                        }
                                    }
                                }
                                if (searchEqualsRank)
                                {
                                    int choise = RandomGenerator.Next(0, 101);
                                    if (currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[0].Count == 2 && choise < 60)
                                    {
                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits.Add(new List<Card>());
                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[1].Add(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[0][1]);
                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[0].RemoveAt(1);
                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].sessionStates.Add(new SessionStates());
                                        Console.CursorLeft = 1;
                                        Console.Write($" Бот {currentGameInfo.Players[currentGameInfo.currentSplit[0]].name} сделал разделение руки.");
                                        Thread.Sleep(botDelay);
                                    }
                                }
                            }
                            if (CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) < 21 && !currentGameInfo.playerEnough && !botIfLeave)
                            {
                                Console.Write($"  Ход бота {currentGameInfo.Players[currentGameInfo.currentSplit[0]].name}.");
                                Thread.Sleep(botDelay);
                                int choise = RandomGenerator.Next(0, 101);
                                if (CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) < 13)
                                {
                                    TakeCard(currentGameInfo.Players[currentGameInfo.currentSplit[0]]);
                                    Console.CursorLeft = 1;
                                    Console.Write($" Бот {currentGameInfo.Players[currentGameInfo.currentSplit[0]].name} взял карту.");
                                    Thread.Sleep(botDelay);
                                }
                                else if (CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) >= 13 && CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) < 16)
                                {
                                    if (choise < 70)
                                    {
                                        TakeCard(currentGameInfo.Players[currentGameInfo.currentSplit[0]]);
                                        Console.CursorLeft = 1;
                                        Console.Write($" Бот {currentGameInfo.Players[currentGameInfo.currentSplit[0]].name} взял карту.");
                                        Thread.Sleep(botDelay);
                                    }
                                    else currentGameInfo.playerEnough = true;
                                }
                                else if (CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) >= 16 && CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) < 19)
                                {
                                    if (choise < 50)
                                    {
                                        TakeCard(currentGameInfo.Players[currentGameInfo.currentSplit[0]]);
                                        Console.CursorLeft = 1;
                                        Console.Write($" Бот {currentGameInfo.Players[currentGameInfo.currentSplit[0]].name} взял карту.");
                                        Thread.Sleep(botDelay);
                                    }
                                    else currentGameInfo.playerEnough = true;
                                }
                                else if (CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) >= 19 && CalculateSplitCost(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]]) < 21)
                                {
                                    if (choise < 10)
                                    {
                                        TakeCard(currentGameInfo.Players[currentGameInfo.currentSplit[0]]);
                                        Console.CursorLeft = 1;
                                        Console.Write($" Бот {currentGameInfo.Players[currentGameInfo.currentSplit[0]].name} взял карту.");
                                        Thread.Sleep(botDelay);
                                    }
                                    else currentGameInfo.playerEnough = true;
                                }
                            }
                            else { NextPlayer(); currentGameInfo.playerEnough = false; }
                        }
                        else
                        {
                            HorizontalMenuRender(InGameActionsItems);
                            switch (Console.ReadKey(true).Key)
                            {
                                case ConsoleKey.LeftArrow:
                                    currentMenuPosition--;
                                    if (currentMenuPosition < 0) currentMenuPosition = InGameActionsItems.Length - 1;
                                    break;
                                case ConsoleKey.RightArrow:
                                    currentMenuPosition++;
                                    if (currentMenuPosition > InGameActionsItems.Length - 1) currentMenuPosition = 0;
                                    break;
                                case ConsoleKey.Enter:
                                    switch (currentMenuPosition)
                                    {
                                        case 0:
                                            TakeCard(currentGameInfo.Players[currentGameInfo.currentSplit[0]]);
                                            break;
                                        case 1:
                                            currentMenuPosition = 0;
                                            NextPlayer();
                                            break;
                                        case 2:
                                            currentMenuPosition = 0;
                                            CleanMessageBox();
                                            if (currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits.Count == 1)
                                            {
                                                bool searchEqualsRank = false;
                                                foreach (Card card1 in currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]])
                                                {
                                                    foreach (Card card2 in currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[currentGameInfo.currentSplit[1]])
                                                    {
                                                        if (!card1.Equals(card2))
                                                        {
                                                            if (card1.rank == card2.rank)
                                                            {
                                                                searchEqualsRank = true;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (searchEqualsRank)
                                                {
                                                    if (currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[0].Count != 2)
                                                    {
                                                        Console.WriteLine("Руку можно разделить только в начале раунда. Нажмите любую клавишу для продолжения...");
                                                        Console.ReadKey(true);
                                                    }
                                                    else
                                                    {
                                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits.Add(new List<Card>());
                                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[1].Add(currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[0][1]);
                                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[0].RemoveAt(1);
                                                        currentGameInfo.Players[currentGameInfo.currentSplit[0]].sessionStates.Add(new SessionStates());
                                                        if (!currentGameInfo.Players[currentGameInfo.currentSplit[0]].isBot && currentGameInfo.achiveEnabled)
                                                        {
                                                            if (!(bool)Achivements["Инвалид"][1])
                                                            {
                                                                Achivements["Инвалид"][1] = true;
                                                                RewriteAchivement((int)Achivements["Инвалид"][3], true, (int)Achivements["Инвалид"][2]);
                                                                currentGameInfo.completedAchivements.Add("Инвалид");
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("У вас в руке карты с разным рангом. Нажмите любую клавишу для продолжения...");
                                                    Console.ReadKey(true);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Вы уже делали разделение руки. Нажмите любую клавишу для продолжения...");
                                                Console.ReadKey(true);
                                            }
                                            break;
                                        case 3:
                                            currentMenuPosition = 0;
                                            if (currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits[0].Count != 2 || currentGameInfo.Players[currentGameInfo.currentSplit[0]].splits.Count != 1)
                                            {
                                                CleanMessageBox();
                                                Console.Write("Покинуть стол можно только в начале раунда. Нажмите любую клавишу для продолжения...");
                                                Console.ReadKey(true);
                                            }
                                            else
                                            {
                                                currentGameInfo.Players[currentGameInfo.currentSplit[0]].sessionStates[currentGameInfo.currentSplit[1]] = SessionStates.Leave;
                                                currentGameInfo.Players[currentGameInfo.currentSplit[0]].totalScores[3]++;
                                                if (!currentGameInfo.Players[currentGameInfo.currentSplit[0]].isBot && currentGameInfo.achiveEnabled)
                                                {
                                                    Achivements["Поражение"][2] = (int)Achivements["Поражение"][2] + 1;
                                                    RewriteAchivement((int)Achivements["Поражение"][3], (bool)Achivements["Поражение"][1], (int)Achivements["Поражение"][2]);
                                                    if (!(bool)Achivements["Отступление"][1])
                                                    {
                                                        Achivements["Отступление"][1] = true;
                                                        RewriteAchivement((int)Achivements["Отступление"][3], true, (int)Achivements["Отступление"][2]);
                                                        currentGameInfo.completedAchivements.Add("Отступление");
                                                    }
                                                    if (!(bool)Achivements["Поражение"][1] && (int)Achivements["Поражение"][2] >= 10)
                                                    {
                                                        Achivements["Поражение"][1] = true;
                                                        RewriteAchivement((int)Achivements["Поражение"][3], true, (int)Achivements["Поражение"][2]);
                                                        currentGameInfo.completedAchivements.Add("Поражение");
                                                    }
                                                }
                                            }
                                            break;
                                        case 4:
                                            currentMenuPosition = 0;
                                            currentState = GameStates.SaveGame;
                                            break;
                                        case 5:
                                            currentMenuPosition = 0;
                                            currentGameInfo.houseTurn = false;
                                            currentState = GameStates.MainMenu;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    else NextPlayer();
                    break;
                case GameLogicLoops.HouseTurn:
                    GameFieldRender();
                    if (CalculateSplitCost(currentGameInfo.House.splits[0]) < 21 && !currentGameInfo.houseEnough)
                    {
                        Console.Write("  Ход дилера.");
                        Thread.Sleep(botDelay);
                        int choise = RandomGenerator.Next(0, 101);
                        if (CalculateSplitCost(currentGameInfo.House.splits[0]) <= CalculateStartingScore())
                        {
                            TakeCard(currentGameInfo.House);
                            Console.CursorLeft = 1;
                            Console.Write(" Дилер взял карту.");
                            Thread.Sleep(botDelay);
                        }
                        else currentGameInfo.houseEnough = true;
                    }
                    else
                    {
                        foreach (Player player in currentGameInfo.Players)
                        {
                            for (int i = 0; i < player.splits.Count; i++)
                            {
                                if (CalculateSplitCost(currentGameInfo.House.splits[0]) <= 21)
                                {
                                    if (CalculateSplitCost(player.splits[i]) < CalculateSplitCost(currentGameInfo.House.splits[0]) && player.sessionStates[i] != SessionStates.Lose && player.sessionStates[i] != SessionStates.Leave)
                                    {
                                        player.sessionStates[i] = SessionStates.Lose;
                                        player.totalScores[4]++;
                                    }
                                    if (CalculateSplitCost(player.splits[i]) == CalculateSplitCost(currentGameInfo.House.splits[0]) && CalculateSplitCost(player.splits[i]) < 21 && player.sessionStates[i] != SessionStates.Draw && player.sessionStates[i] != SessionStates.Leave)
                                    {
                                        player.sessionStates[i] = SessionStates.Draw;
                                        player.totalScores[2]++;
                                        if (!player.isBot && currentGameInfo.achiveEnabled)
                                        {
                                            Achivements["Закономерность"][2] = (int)Achivements["Закономерность"][2] + 1;
                                            RewriteAchivement((int)Achivements["Закономерность"][3], (bool)Achivements["Закономерность"][1], (int)Achivements["Закономерность"][2]);
                                            if (!(bool)Achivements["Случайность"][1])
                                            {
                                                Achivements["Случайность"][1] = true;
                                                RewriteAchivement((int)Achivements["Случайность"][3], true, (int)Achivements["Случайность"][2]);
                                                currentGameInfo.completedAchivements.Add("Случайность");
                                            }
                                            if (!(bool)Achivements["Закономерность"][1] && (int)Achivements["Закономерность"][2] >= 10)
                                            {
                                                Achivements["Закономерность"][1] = true;
                                                RewriteAchivement((int)Achivements["Закономерность"][3], true, (int)Achivements["Закономерность"][2]);
                                                currentGameInfo.completedAchivements.Add("Закономерность");
                                            }
                                        }
                                    }
                                    if (CalculateSplitCost(player.splits[i]) > CalculateSplitCost(currentGameInfo.House.splits[0]) && CalculateSplitCost(player.splits[i]) < 21 && player.sessionStates[i] != SessionStates.Victory && player.sessionStates[i] != SessionStates.Leave)
                                    {
                                        player.sessionStates[i] = SessionStates.Victory;
                                        player.totalScores[1]++;
                                        if (!player.isBot && currentGameInfo.achiveEnabled)
                                        {
                                            Achivements["Опытный"][2] = (int)Achivements["Опытный"][2] + 1;
                                            RewriteAchivement((int)Achivements["Опытный"][3], (bool)Achivements["Опытный"][1], (int)Achivements["Опытный"][2]);
                                            Achivements["Мастер"][2] = (int)Achivements["Мастер"][2] + 1;
                                            RewriteAchivement((int)Achivements["Мастер"][3], (bool)Achivements["Мастер"][1], (int)Achivements["Мастер"][2]);
                                            if (!(bool)Achivements["Начинающий"][1])
                                            {
                                                Achivements["Начинающий"][1] = true;
                                                RewriteAchivement((int)Achivements["Начинающий"][3], true, (int)Achivements["Начинающий"][2]);
                                                currentGameInfo.completedAchivements.Add("Начинающий");
                                            }
                                            if (!(bool)Achivements["Опытный"][1] && (int)Achivements["Опытный"][2] >= 10)
                                            {
                                                Achivements["Опытный"][1] = true;
                                                RewriteAchivement((int)Achivements["Опытный"][3], true, (int)Achivements["Опытный"][2]);
                                                currentGameInfo.completedAchivements.Add("Опытный");
                                            }
                                            if (!(bool)Achivements["Мастер"][1] && (int)Achivements["Мастер"][2] >= 50)
                                            {
                                                Achivements["Мастер"][1] = true;
                                                RewriteAchivement((int)Achivements["Мастер"][3], true, (int)Achivements["Мастер"][2]);
                                                currentGameInfo.completedAchivements.Add("Мастер");
                                            }
                                        }
                                    }
                                }
                                if (CalculateSplitCost(currentGameInfo.House.splits[0]) > 21)
                                {
                                    if (CalculateSplitCost(player.splits[i]) < 21 && player.sessionStates[i] != SessionStates.Victory && player.sessionStates[i] != SessionStates.BlackJack && player.sessionStates[i] != SessionStates.Leave)
                                    {
                                        player.sessionStates[i] = SessionStates.Victory;
                                        player.totalScores[1]++;
                                        if (!player.isBot && currentGameInfo.achiveEnabled)
                                        {
                                            Achivements["Опытный"][2] = (int)Achivements["Опытный"][2] + 1;
                                            RewriteAchivement((int)Achivements["Опытный"][3], (bool)Achivements["Опытный"][1], (int)Achivements["Опытный"][2]);
                                            Achivements["Мастер"][2] = (int)Achivements["Мастер"][2] + 1;
                                            RewriteAchivement((int)Achivements["Мастер"][3], (bool)Achivements["Мастер"][1], (int)Achivements["Мастер"][2]);
                                            if (!(bool)Achivements["Начинающий"][1])
                                            {
                                                Achivements["Начинающий"][1] = true;
                                                RewriteAchivement((int)Achivements["Начинающий"][3], true, (int)Achivements["Начинающий"][2]);
                                                currentGameInfo.completedAchivements.Add("Начинающий");
                                            }
                                            if (!(bool)Achivements["Опытный"][1] && (int)Achivements["Опытный"][2] >= 10)
                                            {
                                                Achivements["Опытный"][1] = true;
                                                RewriteAchivement((int)Achivements["Опытный"][3], true, (int)Achivements["Опытный"][2]);
                                                currentGameInfo.completedAchivements.Add("Опытный");
                                            }
                                            if (!(bool)Achivements["Мастер"][1] && (int)Achivements["Мастер"][2] >= 50)
                                            {
                                                Achivements["Мастер"][1] = true;
                                                RewriteAchivement((int)Achivements["Мастер"][3], true, (int)Achivements["Мастер"][2]);
                                                currentGameInfo.completedAchivements.Add("Мастер");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        currentGameInfo.GameLogicLoop = GameLogicLoops.GameEnding;
                        currentGameInfo.houseTurn = false;
                        currentGameInfo.houseEnough = false;
                        currentGameInfo.houseEnds = true;
                    }
                    break;
                case GameLogicLoops.GameEnding:
                    GameFieldRender();
                    if (currentGameInfo.currentRound < (int)currentGameInfo.currentSessionLenght)
                    {
                        Console.WriteLine($"  Раунд {currentGameInfo.currentRound} закончился. [Esc - Выход, S - Сохранить игру, Enter - Продолжить]");
                        switch (Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.Escape:
                                currentMenuPosition = 0;
                                currentGameInfo.houseTurn = false;
                                currentState = GameStates.MainMenu;
                                break;
                            case ConsoleKey.S:
                                currentState = GameStates.SaveGame;
                                break;
                            case ConsoleKey.Enter:
                                currentGameInfo.GameLogicLoop = GameLogicLoops.PreparingGameSession;
                                currentGameInfo.currentRound++;
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("  Партия окончена. Нажмите любую клавишу для продолжения...");
                        Console.ReadKey(true);
                        currentGameInfo.GameLogicLoop = GameLogicLoops.GameResults;
                    }
                    break;
                case GameLogicLoops.GameResults:
                    bool checkNewRecords = false;
                    foreach (Player player in currentGameInfo.Players)
                    {
                        if (!player.isBot) RewriteRecords(player, currentGameInfo.currentSessionLenght, ref checkNewRecords);
                    }
                    LogoRender();
                    Console.WriteLine(" Итоги партии\n");
                    Console.WriteLine($" Раундов в партии: {(int)currentGameInfo.currentSessionLenght}\n");
                    foreach (Player player in currentGameInfo.Players)
                    {
                        if (!player.isBot && currentGameInfo.achiveEnabled)
                        {
                            if (!(bool)Achivements["Король фортуны"][1] && CalculateScore(player.totalScores) >= 110)
                            {
                                Achivements["Король фортуны"][1] = true;
                                RewriteAchivement((int)Achivements["Король фортуны"][3], true, (int)Achivements["Король фортуны"][2]);
                                currentGameInfo.completedAchivements.Add("Король фортуны");
                            }
                            if (!(bool)Achivements["Этому дну нужен новый герой"][1] && player.totalScores[0] == 0 && player.totalScores[1] == 0 && player.totalScores[2] == 0 && player.totalScores[3] == 0 && currentGameInfo.currentSessionLenght == SessionLenght.BigGame)
                            {
                                Achivements["Этому дну нужен новый герой"][1] = true;
                                RewriteAchivement((int)Achivements["Этому дну нужен новый герой"][3], true, (int)Achivements["Этому дну нужен новый герой"][2]);
                                currentGameInfo.completedAchivements.Add("Этому дну нужен новый герой");
                            }
                            if (!(bool)Achivements["Фантом"][1] && CalculateScore(player.totalScores) == 0)
                            {
                                Achivements["Фантом"][1] = true;
                                RewriteAchivement((int)Achivements["Фантом"][3], true, (int)Achivements["Фантом"][2]);
                                currentGameInfo.completedAchivements.Add("Фантом");
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.WriteLine(" " + new string('█', 98));
                        Console.Write($" █");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        if (player.isBot)
                            Console.Write($"Бот {player.name}:");
                        else
                            Console.Write($"Игрок {player.name}:");
                        Console.CursorLeft = 85;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"Баллы: {CalculateScore(player.totalScores)}");
                        Console.CursorLeft = 98;
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.Write("█\n █");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.Write($"BlackJacks:     {player.totalScores[0],2}");
                        Console.ResetColor();
                        Console.Write("|");
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.Write($"Побед:           {player.totalScores[1],2}");
                        Console.ResetColor();
                        Console.Write("|");
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                        Console.Write($"В ничью:        {player.totalScores[2],2}");
                        Console.ResetColor();
                        Console.Write("|");
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.Write($"Безучастий:      {player.totalScores[3],2}");
                        Console.ResetColor();
                        Console.Write("|");
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.Write($"Пройгрышей:     {player.totalScores[4],2}");
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("█\n");
                        Console.WriteLine(" " + new string('█', 98));
                        Console.ResetColor();
                        Console.Write("\n\n");
                    }
                    if (currentGameInfo.achiveEnabled)
                    {
                        if (currentGameInfo.completedAchivements.Count == 0)
                            Console.WriteLine(" В данной партии не было получено достижений.\n");
                        else
                        {
                            Console.WriteLine(" В данной партии вы получили следующие достижения: ");
                            foreach (string achive in currentGameInfo.completedAchivements)
                            {
                                Console.Write(" |" + achive + "|");
                            }
                            Console.WriteLine("\n");
                        }
                    }
                    else
                    {
                        Console.WriteLine(" Достижения в данной партии были неактивны.\n");
                    }
                    if (checkNewRecords) Console.WriteLine(" Таблица рекордов обновлена!\n");
                    Console.WriteLine(" Для возвращения в главное меню нажмите любую клавишу...");
                    Console.ReadKey(true);
                    currentState = GameStates.MainMenu;
                    break;
            }
        }
        static void SaveStatesGameLoop(bool save = false)
        {
            LogoRender();
            if (save)
                Console.WriteLine(" " + new string(' ', 42) + "Сохранить игру\n");
            else
                Console.WriteLine(" " + new string(' ', 42) + "Загрузить игру\n");
            VerticalMenuRender(GameSavesItems);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    currentMenuPosition--;
                    if (currentMenuPosition < 0) currentMenuPosition = GameSavesItems.Length - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentMenuPosition++;
                    if (currentMenuPosition > GameSavesItems.Length - 1) currentMenuPosition = 0;
                    break;
                case ConsoleKey.Enter:
                    switch (currentMenuPosition)
                    {
                        case 0:
                            if (save) SaveState(0);
                            else LoadState(0);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 1:
                            if (save) SaveState(1);
                            else LoadState(1);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 2:
                            if (save) SaveState(2);
                            else LoadState(2);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 3:
                            if (save) SaveState(3);
                            else LoadState(3);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 4:
                            if (save) SaveState(4);
                            else LoadState(4);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 5:
                            if (save) SaveState(5);
                            else LoadState(5);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 6:
                            if (save) SaveState(6);
                            else LoadState(6);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 7:
                            if (save) SaveState(7);
                            else LoadState(7);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 8:
                            if (save) SaveState(8);
                            else LoadState(8);
                            if (save) currentState = GameStates.Game;
                            break;
                        case 9:
                            currentMenuPosition = 0;
                            if (save) currentState = GameStates.Game;
                            else currentState = GameStates.MainMenu;
                            break;
                    }
                    break;
            }
        }
        static void OptionsLoop()
        {
            LogoRender();
            Console.WriteLine(" " + new string(' ', 44) + "Настройки\n");
            VerticalPostMenuRender(OptionsItems);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    currentMenuPosition--;
                    if (currentMenuPosition < 0) currentMenuPosition = OptionsItems.Length - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentMenuPosition++;
                    if (currentMenuPosition > OptionsItems.Length - 1) currentMenuPosition = 0;
                    break;
                case ConsoleKey.LeftArrow:
                    OptionsItems[currentMenuPosition].submenu.currentItem--;
                    if (OptionsItems[currentMenuPosition].submenu.currentItem < 0) OptionsItems[currentMenuPosition].submenu.currentItem = OptionsItems[currentMenuPosition].submenu.items.Length - 1;
                    break;
                case ConsoleKey.RightArrow:
                    OptionsItems[currentMenuPosition].submenu.currentItem++;
                    if (OptionsItems[currentMenuPosition].submenu.currentItem > OptionsItems[currentMenuPosition].submenu.items.Length - 1) OptionsItems[currentMenuPosition].submenu.currentItem = 0;
                    break;
                case ConsoleKey.Enter:
                    switch (currentMenuPosition)
                    {
                        case 0:
                            musicPlayer.Stop();
                            musicPlayer = new SoundPlayer(musicPath + MusicList[OptionsItems[0].submenu.currentItem] + ".wav");
                            RewriteConfigFile(MusicList[OptionsItems[0].submenu.currentItem], 0);
                            musicPlayer.LoadAsync();
                            musicPlayer.PlayLooping();
                            break;
                        case 1:
                            if (OptionsItems[1].submenu.currentItem == 0)
                            {
                                botDelay = 1500;
                                RewriteConfigFile("1500", 1);
                            }
                            else
                            {
                                botDelay = 200;
                                RewriteConfigFile("200", 1);
                            }
                            break;
                        case 2:
                            for (int i = 0; i < 16; i++)
                            {
                                RewriteAchivement(i, false, 0);
                                Achivements.ElementAt(i).Value[1] = false;
                                Achivements.ElementAt(i).Value[2] = 0;
                            }
                            break;
                        case 3:
                            FileStream RecordsFile = new FileStream(recordsPath + "One.txt", FileMode.Create);
                            StreamWriter RecordsWriter = new StreamWriter(RecordsFile);
                            for (int i = 0; i < 3; i++)
                            {
                                RecordsWriter.WriteLine($"Empty{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0");
                            }
                            RecordsWriter.Close();
                            RecordsFile.Close();
                            RecordsFile = new FileStream(recordsPath + "Small.txt", FileMode.Create);
                            RecordsWriter = new StreamWriter(RecordsFile);
                            for (int i = 0; i < 3; i++)
                            {
                                RecordsWriter.WriteLine($"Empty{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0");
                            }
                            RecordsWriter.Close();
                            RecordsFile.Close();
                            RecordsFile = new FileStream(recordsPath + "Medium.txt", FileMode.Create);
                            RecordsWriter = new StreamWriter(RecordsFile);
                            for (int i = 0; i < 3; i++)
                            {
                                RecordsWriter.WriteLine($"Empty{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0");
                            }
                            RecordsWriter.Close();
                            RecordsFile.Close();
                            RecordsFile = new FileStream(recordsPath + "Large.txt", FileMode.Create);
                            RecordsWriter = new StreamWriter(RecordsFile);
                            for (int i = 0; i < 3; i++)
                            {
                                RecordsWriter.WriteLine($"Empty{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0");
                            }
                            RecordsWriter.Close();
                            RecordsFile.Close();
                            RecordsFile = new FileStream(recordsPath + "BigGame.txt", FileMode.Create);
                            RecordsWriter = new StreamWriter(RecordsFile);
                            for (int i = 0; i < 3; i++)
                            {
                                RecordsWriter.WriteLine($"Empty{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0{((char)9830)}0");
                            }
                            RecordsWriter.Close();
                            RecordsFile.Close();
                            break;
                        case 4:
                            if (isMusicPlay)
                            {
                                musicPlayer.Stop();
                                isMusicPlay = false;
                                RewriteConfigFile("False", 2);
                            }
                            else
                            {
                                musicPlayer.PlayLooping();
                                isMusicPlay = true;
                                RewriteConfigFile("True", 2);
                            }
                            break;
                        case 5:
                            currentMenuPosition = 0;
                            currentState = GameStates.MainMenu;
                            break;
                    }
                    break;
            }
        }
        static void AchivementsLoop()
        {
            LogoRender();
            Console.WriteLine(" " + new string(' ', 44) + "Достижения\n");
            Console.WriteLine(" " + new string('█', 98));
            foreach (var achivement in Achivements)
            {
                Console.Write(" ");
                if ((bool)achivement.Value[1]) Console.ForegroundColor = ConsoleColor.DarkGreen;
                else Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("██ ");
                Console.Write(achivement.Key + new string (' ', 35 - achivement.Key.Length));
                Console.Write("█ ");
                Console.Write((string)achivement.Value[0] + new string(' ', 56 - ((string)achivement.Value[0]).Length));
                Console.Write("██\n");
                Console.ResetColor();
            }
            Console.WriteLine(" " + new string('█', 98) + "\n");
            VerticalMenuRender(AchivementsItems);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    currentMenuPosition--;
                    if (currentMenuPosition < 0) currentMenuPosition = AchivementsItems.Length - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentMenuPosition++;
                    if (currentMenuPosition > AchivementsItems.Length - 1) currentMenuPosition = 0;
                    break;
                case ConsoleKey.Enter:
                    switch (currentMenuPosition)
                    {
                        case 0:
                            currentMenuPosition = 0;
                            currentState = GameStates.MainMenu;
                            break;
                    }
                    break;
            }
        }
        static void RulesLoop()
        {
            LogoRender();
            Console.WriteLine(  " " + new string(' ', 36) + "О проекте и правилах игры\n");
            Console.WriteLine(  "     Блэк-джек  —  одна из  самых популярных  карточных игр  по всему  миру.  Большая  популярность\n" +
                                " игры обуславливается простыми правилами,  скоростью игры и  наиболее простой стратегией в подсчете\n" +
                                " карт.  Тем не менее,  популярность  игра завоевала не сразу.  Считается, что предшественником этой\n" +
                                " игры   была   карточная  игра  «vingt-et-un»  («двадцать один»),   которая  появилась  во  Франции\n" +
                                " приблизительно в  XIX веке.  В России,  например,  блэк-джек по  сей день  часто называют двадцать\n" +
                                " одно  или  очко.  (Но у традиционной игры очко несколько отличаются правила.)  Внимание!  В рамках\n" +
                                " данного проекта правила игры несколько отличаются!\n");
            Console.WriteLine(  "     Игра начинается с того, что дилер раздает карты:  каждому игроку,  включая дилера,  дается две\n" +
                                " карты из перетасованной колоды,  при этом у дилера одна карта  будет перевернута вверх рубашкой до\n" +
                                " его хода.  Далее игроки ходят  по очереди,  последним ходит диллер.  Игрок за ход может  попросить\n" +
                                " дилера дать карту из колоды. Когда у игрока будет достаточно карт, он может пропустить ход. Задача\n" +
                                " игрока - набрать комбинацию карт,  сумма рангов которых не будет превышать 21 и будет больше суммы\n" +
                                " рангов карт дилера. Карты Валет, Дама, Король считаются за 10 очков, Туз же, если у игрока в сумме\n" +
                                " менее 11 очков будет равняться 11 очкам,  иначе будет равен 1 очку. Если игрок выбьет комбинацию в\n" +
                                " 21 очко,  то будет считаться,  что он получил  BlackJack, причем считается, что игрок победил, вне\n" +
                                " зависимости от комбинации карт дилера.  Если у игрока и дилера будет  равное количество очков,  то\n" +
                                " это означает,  что игрок сыграл в ничью.  Если игрок чувствует,  что скорее всего он не выиграет в\n" +
                                " текущем раунде,  то он может покинуть  стол на время текущего раунда,  при этом игрок за время его\n" +
                                " хода не должен был брать карты. Также  если у игрока  на выдаче карт в руке окажутся  карты одного\n" +
                                " ранга, то он может  сделать разделение руки,  т.е. у игрока станет два набора карт,  по каждому из\n" +
                                " которых игрок ходит отдельно. Игроку дается за: BlackJack 4 балла, Победу 2 балла, Ничью 0 баллов,\n" +
                                " Безучастие -1 балл, Пройгрыш -2 балла.\n");
            Console.WriteLine(  "     Внимание! Достижения активны только в той партии, где среди всех игроков будет только один под\n" +
                                " управлением пользователя!\n");
            Console.WriteLine(  "     Проект разработал Скочко Андрей Евгеньевич. Почта: andronovec2000@mail.ru\n");
            Console.WriteLine(  "     Спасибо за то, что обратили внимание на проект. Приятной игры.\n");

            VerticalMenuRender(RulesItems);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    currentMenuPosition--;
                    if (currentMenuPosition < 0) currentMenuPosition = RulesItems.Length - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentMenuPosition++;
                    if (currentMenuPosition > RulesItems.Length - 1) currentMenuPosition = 0;
                    break;
                case ConsoleKey.Enter:
                    switch (currentMenuPosition)
                    {
                        case 0:
                            currentMenuPosition = 0;
                            currentState = GameStates.MainMenu;
                            break;
                    }
                    break;
            }
        }
        static void RecordsLoop()
        {
            LogoRender();
            Console.WriteLine(" " + new string(' ', 40) + "Рекорды игроков\n");
            RecordsRender(" Один раунд", ReadRecordFile("One.txt"));
            RecordsRender(" Малая партия", ReadRecordFile("Small.txt"));
            RecordsRender(" Средняя партия", ReadRecordFile("Medium.txt"));
            RecordsRender(" Большая партия", ReadRecordFile("Large.txt"));
            RecordsRender(" Наибольшая партия", ReadRecordFile("BigGame.txt"));
            Console.WriteLine();
            VerticalMenuRender(RecordsItems);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    currentMenuPosition--;
                    if (currentMenuPosition < 0) currentMenuPosition = RecordsItems.Length - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentMenuPosition++;
                    if (currentMenuPosition > RecordsItems.Length - 1) currentMenuPosition = 0;
                    break;
                case ConsoleKey.Enter:
                    switch (currentMenuPosition)
                    {
                        case 0:
                            currentMenuPosition = 0;
                            currentState = GameStates.MainMenu;
                            break;
                    }
                    break;
            }
        }
        static void UpdateListLoop()
        {
            LogoRender();
            Console.WriteLine(" " + new string(' ', 36) + "Хронология обновлений\n");
            Console.WriteLine(  " [1.0.0]\n" +
                                "     - Релиз игры.\n" +
                                " [1.0.1]\n" +
                                "     - Добавлена функция вкл\\выкл музыки в настройках.\n" +
                                "     - Улучшен ИИ Дилера\n" +
                                "     - Исправлен баг с подсчетом очков при наличии туза в руках\n" +
                                "     - Исправлен баг с возможностью выхода со стола после разделения руки\n" +
                                "     - Исправлен баг, который позволял ботам делать более одного разделения руки\n" +
                                "     - Исправлен баг с получением достижения \"Этому дну нужен новый герой\"\n" +
                                "     - Улучшено визуальное оформление\n" +
                                " [1.0.2]\n" +
                                "     - Исправлен баг с незагружаемыми сохранениями\n" +
                                "     - Исправлены баги, связанные с таблицей рекордов\n");
            VerticalMenuRender(RulesItems);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    currentMenuPosition--;
                    if (currentMenuPosition < 0) currentMenuPosition = RulesItems.Length - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentMenuPosition++;
                    if (currentMenuPosition > RulesItems.Length - 1) currentMenuPosition = 0;
                    break;
                case ConsoleKey.Enter:
                    switch (currentMenuPosition)
                    {
                        case 0:
                            currentMenuPosition = 0;
                            currentState = GameStates.MainMenu;
                            break;
                    }
                    break;
            }
        }
        static void QuitLoop()
        {
            LogoRender();
            Console.WriteLine(" Вы действительно хотите выйти из игры?");
            VerticalMenuRender(QuitItems);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    currentMenuPosition--;
                    if (currentMenuPosition < 0) currentMenuPosition = QuitItems.Length - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentMenuPosition++;
                    if (currentMenuPosition > QuitItems.Length - 1) currentMenuPosition = 0;
                    break;
                case ConsoleKey.Enter:
                    switch (currentMenuPosition)
                    {
                        case 0:
                            currentMenuPosition = 0;
                            currentState = GameStates.MainMenu;
                            break;
                        case 1:
                            gameCycle = false;
                            break;
                    }
                    break;
            }
        }
        static void StartGame()
        {
            bool isReady = false;
            bool startGame = false;
            bool isNamed = false;
            int pNumber = 0;
            while (!isReady)
            {
                switch (StartGameLoop)
                {
                    case StartGameLoops.PlayersListGeneration:
                        Console.Clear();
                        LogoRender();
                        Console.WriteLine(" Сколько игроков примет участие в игре?");
                        VerticalMenuRender(PlayersCountItems);
                        switch (Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.UpArrow:
                                currentMenuPosition--;
                                if (currentMenuPosition < 0) currentMenuPosition = PlayersCountItems.Length - 1;
                                break;
                            case ConsoleKey.DownArrow:
                                currentMenuPosition++;
                                if (currentMenuPosition > PlayersCountItems.Length - 1) currentMenuPosition = 0;
                                break;
                            case ConsoleKey.Enter:
                                switch (currentMenuPosition)
                                {
                                    case 0:
                                        currentMenuPosition = 0;
                                        InitializePlayers(1);
                                        StartGameLoop = StartGameLoops.PlayersInitialization;
                                        continue;
                                    case 1:
                                        currentMenuPosition = 0;
                                        InitializePlayers(2);
                                        StartGameLoop = StartGameLoops.PlayersInitialization;
                                        continue;
                                    case 2:
                                        currentMenuPosition = 0;
                                        InitializePlayers(3);
                                        StartGameLoop = StartGameLoops.PlayersInitialization;
                                        continue;
                                    case 3:
                                        currentMenuPosition = 0;
                                        InitializePlayers(4);
                                        StartGameLoop = StartGameLoops.PlayersInitialization;
                                        continue;
                                    case 4:
                                        currentMenuPosition = 0;
                                        currentState = GameStates.MainMenu;
                                        isReady = true;
                                        break;
                                }
                                break;
                        }
                        break;
                    case StartGameLoops.PlayersInitialization:
                        Console.Clear();
                        LogoRender();
                        if (!isNamed)
                        {
                            Console.WriteLine($" Введите имя игрока №{pNumber + 1} (имя должно содержать от 2 до 16 символов):");
                            Console.CursorVisible = true;
                            Console.Write(" ");
                            currentGameInfo.Players[pNumber].name = "";
                            currentGameInfo.Players[pNumber].name = Console.ReadLine();
                            Console.CursorVisible = false;
                            if (currentGameInfo.Players[pNumber].name.Length >= 2 && currentGameInfo.Players[pNumber].name.Length <= 16)
                            {
                                isNamed = true;
                            }
                            continue;
                        }
                        Console.WriteLine($" Кем будет {currentGameInfo.Players[pNumber].name}?");
                        VerticalMenuRender(BotOrPlayerItems);
                        switch (Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.UpArrow:
                                currentMenuPosition--;
                                if (currentMenuPosition < 0) currentMenuPosition = BotOrPlayerItems.Length - 1;
                                break;
                            case ConsoleKey.DownArrow:
                                currentMenuPosition++;
                                if (currentMenuPosition > BotOrPlayerItems.Length - 1) currentMenuPosition = 0;
                                break;
                            case ConsoleKey.Enter:
                                switch (currentMenuPosition)
                                {
                                    case 0:
                                        currentGameInfo.Players[pNumber].isBot = false;
                                        pNumber++;
                                        isNamed = false;
                                        if (pNumber >= currentGameInfo.Players.Length) StartGameLoop = StartGameLoops.SessionLenghtInitialization;
                                        continue;
                                    case 1:
                                        currentGameInfo.Players[pNumber].isBot = true;
                                        pNumber++;
                                        isNamed = false;
                                        if (pNumber >= currentGameInfo.Players.Length) StartGameLoop = StartGameLoops.SessionLenghtInitialization;
                                        continue;
                                    case 2:
                                        currentMenuPosition = 0;
                                        currentState = GameStates.MainMenu;
                                        isReady = true;
                                        break;
                                }
                                break;
                        }
                        break;
                    case StartGameLoops.SessionLenghtInitialization:
                        Console.Clear();
                        LogoRender();
                        Console.WriteLine(" Выберите длину партии:");
                        VerticalMenuRender(SessionLenghtsItems);
                        switch (Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.UpArrow:
                                currentMenuPosition--;
                                if (currentMenuPosition < 0) currentMenuPosition = SessionLenghtsItems.Length - 1;
                                break;
                            case ConsoleKey.DownArrow:
                                currentMenuPosition++;
                                if (currentMenuPosition > SessionLenghtsItems.Length - 1) currentMenuPosition = 0;
                                break;
                            case ConsoleKey.Enter:
                                switch (currentMenuPosition)
                                {
                                    case 0:
                                        currentMenuPosition = 0;
                                        currentGameInfo.currentSessionLenght = SessionLenght.One;
                                        StartGameLoop = StartGameLoops.Preparing;
                                        break;
                                    case 1:
                                        currentMenuPosition = 0;
                                        currentGameInfo.currentSessionLenght = SessionLenght.Small;
                                        StartGameLoop = StartGameLoops.Preparing;
                                        break;
                                    case 2:
                                        currentMenuPosition = 0;
                                        currentGameInfo.currentSessionLenght = SessionLenght.Medium;
                                        StartGameLoop = StartGameLoops.Preparing;
                                        break;
                                    case 3:
                                        currentMenuPosition = 0;
                                        currentGameInfo.currentSessionLenght = SessionLenght.Large;
                                        StartGameLoop = StartGameLoops.Preparing;
                                        break;
                                    case 4:
                                        currentMenuPosition = 0;
                                        currentGameInfo.currentSessionLenght = SessionLenght.BigGame;
                                        StartGameLoop = StartGameLoops.Preparing;
                                        break;
                                    case 5:
                                        currentMenuPosition = 0;
                                        currentState = GameStates.MainMenu;
                                        isReady = true;
                                        break;
                                }
                                break;
                        }
                        break;
                    case StartGameLoops.Preparing:
                        Console.Clear();
                        LogoRender();
                        Console.WriteLine(" Приятной игры");
                        if (!startGame)
                        {
                            Console.WriteLine(" Нажмите Enter для старта игры");
                            if (Console.ReadKey(true).Key == ConsoleKey.Enter) startGame = true;
                        }
                        else
                        {
                            Console.Write(" ");
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine("Нажмите Enter для старта игры");
                            Console.ResetColor();
                            Thread.Sleep(1000);
                            currentGameInfo.House = new Player
                            {
                                name = "Дилер",
                                isBot = true,
                                splits = new List<List<Card>>()
                            };
                            currentGameInfo.House.splits.Add(new List<Card>());
                            currentGameInfo.House.sessionStates = new List<SessionStates> { SessionStates.InGame };
                            currentGameInfo.House.totalScores = new byte[5];
                            int playersCount = 0;
                            foreach (Player player in currentGameInfo.Players)
                            {
                                if (player.isBot == false) playersCount++;
                            }
                            if (playersCount == 1) currentGameInfo.achiveEnabled = true; else currentGameInfo.achiveEnabled = false;
                            currentGameInfo.completedAchivements = new List<string>();
                            isReady = true;
                            currentGameInfo.GameLogicLoop = GameLogicLoops.PreparingGameSession;
                            currentGameInfo.currentRound = 1;
                        }
                        break;
                }
            }
        }
        #endregion
    }
}