using System;
using System.Threading.Tasks;

namespace Maze
{
    static class Program
    {
        static async Task Main(string[] args)
        {
          
            var host = "https://maze.hightechict.nl";
            var key = "HTI Thanks You [6313]"; 
            var playerName = "Mostafa Alsagher";

            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", key);
            var client = new AmazeingClient(host, httpClient);

            await client.ForgetPlayer();
            await client.RegisterPlayer(playerName);            
            var availableMazes = (await client.AllMazes());			         
            foreach (var maze in availableMazes)
            {
               
                Console.Error.WriteLine($"now we are in maze {maze.Name} , tiles =  {maze.TotalTiles}");
                await new MazeSolver(client, maze).Solve();
                Console.WriteLine($"boyaaaaa! {maze.Name} is done ");
            }
                Console.WriteLine("Now to get the egg");
                await getTheEgg(client);
                Console.WriteLine("we got the egg !");

        }
        // todo understand getTheEgg method
        private static async Task getTheEgg(IAmazeingClient mazeClient)
        {
            try{ await mazeClient.Move(Direction.Up);} catch { }
            try{ await mazeClient.Move(Direction.Up);} catch { }
            try{ await mazeClient.Move(Direction.Down);} catch { }
            try{ await mazeClient.Move(Direction.Down);} catch { }
            try{ await mazeClient.Move(Direction.Left);} catch { }
            try{ await mazeClient.Move(Direction.Right);} catch { }
            try{ await mazeClient.Move(Direction.Left);} catch { }
            try{ await mazeClient.Move(Direction.Right);} catch { }
        }
    }
}
