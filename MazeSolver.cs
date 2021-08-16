using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maze
{
    internal class MazeSolver
    {
        private readonly IAmazeingClient _client;
        private readonly MazeInfo _maze;
        private readonly Stack<Direction> _arroundNodes = new Stack<Direction>();
        private readonly List<Stack<Direction>> _collectNodes = new List<Stack<Direction>>();
        private readonly List<Stack<Direction>> _exitNodes = new List<Stack<Direction>>();

        public MazeSolver( IAmazeingClient client,MazeInfo maze )
        {
            _client = client;
            this._maze = maze;
        }
        public async Task Solve()
        {
            var options = await _client.EnterMaze(_maze.Name);
            KeepOnTheExits(options, _exitNodes);
            KeepOnTheCollection(options, _collectNodes);

            options = await CollectAllPoints(options);
            options = await CollectScoreInHand(options);
            await LetUsExit(options);
        }

        private async Task LetUsExit(PossibleActionsAndCurrentScore options)
        {
            while (true)
            {
                
                if (options.CanExitMazeHere)
                {
                    await _client.ExitMaze();
                    return;
                }

                var stack = _exitNodes.OrderBy(st => st.Count).FirstOrDefault();
                if (stack != null)
                {
                    var dir = stack.Peek();
                    var step = ReverseDir(dir);
                    options = await MakeMove(step, _exitNodes, _collectNodes, _arroundNodes);
                    continue;
                }
                
                var mostUsefulDirection = BestForLocatExit(options, _arroundNodes);
                if (mostUsefulDirection != null)
                {
                    options = await MakeMove(mostUsefulDirection.Direction, _exitNodes, _collectNodes, _arroundNodes);
                    continue;
                }

                if (_arroundNodes.Any())
                {
                    var dir = ReverseDir(_arroundNodes.Peek());
                    options = await MakeMove(dir, _exitNodes, _collectNodes, _arroundNodes);
                    continue;
                }

            }
        }




        private static Stack<Direction> BestStackForCollect(List<Stack<Direction>> collectNodes,
            List<Stack<Direction>> exitNodes)
        {
            var stack = collectNodes.OrderBy(st => st.Count).FirstOrDefault();
            return stack;
        }





        private static void KeepOnTheExits(PossibleActionsAndCurrentScore options,
            List<Stack<Direction>> exitNodes)
        {
            foreach (var dir in options.PossibleMoveActions.Where(ma => ma.AllowsExit))
            {
                var stack = new Stack<Direction>();
                stack.Push(ReverseDir(dir.Direction));
                exitNodes.Add(stack);
            }
        }






        private static void KeepOnTheCollection(PossibleActionsAndCurrentScore options,
            List<Stack<Direction>> collectNodes)
        {
            foreach (var dir in options.PossibleMoveActions.Where(ma => ma.AllowsScoreCollection))
            {
                var stack = new Stack<Direction>();
                stack.Push(ReverseDir(dir.Direction));
                collectNodes.Add(stack);
            }
        }





        private async Task<PossibleActionsAndCurrentScore> MakeMove(Direction direction,
            List<Stack<Direction>> exitNodes, List<Stack<Direction>> collectNodes,
            Stack<Direction> Nodes)
        {
            try
            {
                var newOptions = await _client.Move(direction);
                if (newOptions == null)
                    throw new ArgumentException();
                
                // Record moves
                foreach (var st in exitNodes)
                    Push(st, direction);

                foreach (var st in collectNodes)
                    Push(st, direction);

                Push(Nodes, direction);

                // Check nearby
                KeepOnTheExits(newOptions, _exitNodes);
                KeepOnTheCollection(newOptions, _collectNodes);

                return newOptions;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }






        private async Task<PossibleActionsAndCurrentScore> CollectScoreInHand(PossibleActionsAndCurrentScore options)
        {
            while (options.CurrentScoreInHand != 0)
            {
                if (options.CanCollectScoreHere)
                {
                    options = await _client.CollectScore();
                    continue;
                }
                var stack = BestStackForCollect(_collectNodes, _exitNodes);
                if (stack != null)
                {
                    var dir = stack.Peek();
                    var step = ReverseDir(dir);
                    options = await MakeMove(step, _exitNodes, _collectNodes, _arroundNodes);
                    continue;
                }

                var mostUsefulDirection = BestForLocatCollect(options, _arroundNodes);
                if (mostUsefulDirection != null)
                {
                    options = await MakeMove(mostUsefulDirection.Direction, _exitNodes, _collectNodes, _arroundNodes);
                    continue;
                }
                if (_arroundNodes.Any())
                {
                    var dir = ReverseDir(_arroundNodes.Peek());
                    options = await MakeMove(dir, _exitNodes, _collectNodes, _arroundNodes);
                    continue;
                }

            }

            return options;
        }







        private async Task<PossibleActionsAndCurrentScore> CollectAllPoints(PossibleActionsAndCurrentScore options)
        {
            while (options.CurrentScoreInHand + options.CurrentScoreInBag < _maze.PotentialReward)
            {
                var mostUsefulDirection = BestForCollect(options, _arroundNodes);
                if (mostUsefulDirection != null)
                {
                    options = await MakeMove(mostUsefulDirection.Direction, _exitNodes, _collectNodes, _arroundNodes);
                    continue;
                }

                if (_arroundNodes.Any())
                {
                    var dir = ReverseDir(_arroundNodes.Peek());
                    options = await MakeMove(dir, _exitNodes, _collectNodes, _arroundNodes);
                    continue;
                }

            }

            return options;
        }








        private static MoveAction BestForCollect(PossibleActionsAndCurrentScore options,
            Stack<Direction> arroundNodes)
        {
            var mostUsefulDir = options
                .PossibleMoveActions
                .Where(ma => !ma.HasBeenVisited) 
                .OrderBy(_ => 1)
                .ThenBy(ma => ma.RewardOnDestination == 0)
                .ThenBy(ma => LeftWallAlgorithm(ma.Direction, arroundNodes))
                .ThenByDescending(ma => ma.Direction)
                .FirstOrDefault();
            return mostUsefulDir;
        }






        private static MoveAction BestForLocatCollect(PossibleActionsAndCurrentScore options,
            Stack<Direction> arroundNodes)
        {
            var mostUsefulDir = options
                .PossibleMoveActions
                .Where(ma => !ma.HasBeenVisited)
                .OrderBy(ma => ma.AllowsExit)
                .ThenBy(ma => LeftWallAlgorithm(ma.Direction, arroundNodes))
                .FirstOrDefault();
            return mostUsefulDir;
        }






        private static MoveAction BestForLocatExit(PossibleActionsAndCurrentScore options,
            Stack<Direction> arroundNodes)
        {
            var mostUsefulDir = options
                .PossibleMoveActions
                .Where(ma => !ma.HasBeenVisited)
                .OrderBy(ma => LeftWallAlgorithm(ma.Direction, arroundNodes))
                .FirstOrDefault();
            return mostUsefulDir;
        }






        private static int LeftWallAlgorithm(Direction possible, Stack<Direction> arroundNodes)
        {
            if (arroundNodes.Count == 0)
                return 0;
            var incoming = arroundNodes.Peek();
            return LeftWallAlgorithm(possible, incoming);
        }





        static int LeftWallAlgorithm(Direction possible, Direction incoming)
        {
         
            return (5 + possible - incoming) % 4;
           
        }





        static void Push(Stack<Direction> stack, Direction dir)
        {
            if (stack.Count == 0)
            {
                stack.Push(dir);
                return;
            }

            if (stack.Count != 0 && stack.Peek() == ReverseDir(dir))
                stack.Pop();
            else
                stack.Push(dir);
        }





        private static Direction ReverseDir(Direction dir)
        {
            switch (dir)
            {
                case Direction.Down: return Direction.Up;
                case Direction.Up: return Direction.Down;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default:
                    throw new ArgumentException("no dir within this");
            }
        }
    }
}