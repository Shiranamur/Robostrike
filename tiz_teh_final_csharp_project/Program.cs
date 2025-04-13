﻿using System;
using System.IO;

namespace tiz_teh_final_csharp_project;

public class Program
{
    static void Main(string[] args)
    {
        string mapFilePath = "../../../Map/map_test.json";
        string currentDir = Directory.GetCurrentDirectory();
        string fullMapFilePath = Path.GetFullPath(mapFilePath);

        Console.WriteLine(currentDir, fullMapFilePath);
        List<Player> players = new List<Player>
        {
            new Player { id = 1, x = 0, y = 4, direction = 'S', inputs = " ", health = 5},
            new Player { id = 2, x = 0, y = 7, direction = 'N', inputs = " ", health = 2},

        };
        
        Game game = new Game(mapFilePath, players);
        
        game.StartGame();
    }
}

