using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalLines.Common;
using SignalLines.Common.GamePieces;
using SignalR.Hubs;

namespace SignalR.Silverlight.Web.Hubs
{
    [HubName("GameHub")]
    public class GameHub : Hub, IDisconnect
    {
        private GameWorld _world;

        public GameHub()
        {
            _world = GameWorld.Instance;
        }

        public void Send(string message)
        {
            var player = _world.State.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player != null)
            {
                // Call the addMessage method on all clients
                Clients.addMessage(player.Name + ": " + message);
            }
            else
            {
                Clients.addMessage("Anon: " + message);
            }
        }

        public void JoinGame(string playerName)
        {
            var state = _world.State;
            var player = _world.Join(playerName, Context.ConnectionId);
            if (player != null)
            {
                Caller.addedToGame(state);
                Clients.newPlayerJoined(player);
            }
        }

        public void ClickLine(int row, int column)
        {
            var firstOrDefault = _world.State.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (firstOrDefault != null)
            {
                var playerId = firstOrDefault.PlayerId;

                var item = _world.GameModel.GetElementAt(row, column) as Line;

                if (item != null && item.Occupy(playerId))
                {
                    _world.State.OccupiedLines.Add(item);
                    Clients.lineClicked(row, column, playerId);
                }
            }
        }

        public void ResetGame()
        {
            _world.StartNewGame();

            Clients.resetGame(_world.State);
        }

        public Task Disconnect()
        {
            var player = _world.State.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            _world.State.Players.Remove(player);
            Clients.playerLeft(player);
            return null;
        }
    }
}