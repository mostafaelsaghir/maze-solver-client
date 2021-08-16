using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Maze
{
    public interface IAmazeingClient
    {
        Task<ICollection<MazeInfo>> AllMazes();
        Task<PossibleActionsAndCurrentScore> CollectScore();
        Task<PossibleActionsAndCurrentScore> EnterMaze(string mazeName);
        Task<PossibleActionsAndCurrentScore> Move(Direction direction);
        Task<PossibleActionsAndCurrentScore> PossibleActions();
        Task RegisterPlayer(string name);
        Task<PlayerInfo> GetPlayerInfo();
        Task ExitMaze();
        Task ForgetPlayer();
    }
}