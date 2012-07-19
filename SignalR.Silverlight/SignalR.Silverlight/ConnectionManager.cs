using System;
using System.Windows.Threading;
using SignalLines.Common;
using SignalR.Client.Hubs;

namespace SignalR.Silverlight
{
    public class ConnectionManager
    {
        private readonly Dispatcher _dispatcher;
        private readonly IHubProxy _gameHub;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<LineClickedEventArgs> LineClicked;
        public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
        public event EventHandler<PlayerLeftEventArgs> PlayerLeft;
        public event EventHandler<GameResetEventArgs> GameReset;
        public event EventHandler<AddedToGameEventArgs> AddedToGame;

        public ConnectionManager(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            var hub = new HubConnection("http://localhost:20449/");

            _gameHub = hub.CreateProxy("GameHub");
            _gameHub.On<string>("addMessage", ProcessMessage);
            _gameHub.On<int, int, int>("lineClicked", HandleLineClicked);
            _gameHub.On<Player>("newPlayerJoined", HandleNewPlayerJoined);
            _gameHub.On<GameState>("resetGame", HandleResetGame);
            _gameHub.On<Player>("playerLeft", HandlePlayerLeft);
            _gameHub.On<GameState>("addedToGame", HandleAddedToGame);

            hub.Start().ContinueWith(delegate
                                         {
                                             return IsReady = true;
                                         }
                );
        }

        private void HandleAddedToGame(GameState gameState)
        {
            if (AddedToGame != null)
                _dispatcher.BeginInvoke(() => AddedToGame(this, new AddedToGameEventArgs(gameState)));
            }

        protected bool IsReady { get; set; }

        private void HandlePlayerLeft(Player player)
        {
            if (PlayerLeft != null)
                _dispatcher.BeginInvoke(() => PlayerLeft(this, new PlayerLeftEventArgs(player)));
        }

        private void HandleResetGame(GameState gameState)
        {
            if (GameReset != null)
                _dispatcher.BeginInvoke(() => GameReset(this, new GameResetEventArgs(gameState)));
        }

        private void HandleNewPlayerJoined(Player player)
        {
            if (PlayerJoined != null)
                _dispatcher.BeginInvoke(() => PlayerJoined(this, new PlayerJoinedEventArgs(player)));
        }

        public void JoinGame(string name)
        {
            _gameHub.Invoke("JoinGame", name);
        }

        private void ProcessMessage(string message)
        {
            if (MessageReceived != null)
            {
                _dispatcher.BeginInvoke(() => MessageReceived(this, new MessageReceivedEventArgs(message)));
            }
        }

        public void SendMessage(string message)
        {
            _gameHub.Invoke("Send", message);
        }

        public void ResetGame()
        {
            _gameHub.Invoke("ResetGame");
        }

        public void HandleLineClicked(int row, int column, int playerId)
        {
            if (LineClicked != null)
            {
                _dispatcher.BeginInvoke(() => LineClicked(this, new LineClickedEventArgs(row, column, playerId)));
            }
        }

        public void ClickLine(int row, int column)
        {
            _gameHub.Invoke("ClickLine", row, column);
        }
    }

    public class AddedToGameEventArgs :EventArgs
    {
        public GameState GameState { get; set; }

        public AddedToGameEventArgs(GameState gameState)
        {
            GameState = gameState;
        }
    }

    public class GameResetEventArgs : EventArgs
    {
        public GameState GameState { get; set; }

        public GameResetEventArgs(GameState gameState)
        {
            GameState = gameState;
        }
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        private readonly dynamic _message;

        public MessageReceivedEventArgs(dynamic message)
        {
            _message = message;
        }

        public dynamic Message
        {
            get { return _message; }
        }
    }

    public class PlayerJoinedEventArgs : EventArgs
    {
        public Player Player { get; set; }

        public PlayerJoinedEventArgs(Player player)
        {
            Player = player;
        }
    }

    public class PlayerLeftEventArgs : EventArgs
    {
        public Player Player { get; set; }

        public PlayerLeftEventArgs(Player player)
        {
            Player = player;
        }
    }

    public class LineClickedEventArgs : EventArgs
    {
        public int Row { get; private set; }
        public int Column { get; private set; }
        public int PlayerId { get; private set; }

        public LineClickedEventArgs(int row, int column, int playerId)
        {
            Row = row;
            Column = column;
            PlayerId = playerId;
        }
    }
}