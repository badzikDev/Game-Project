using System;
using System.IO;

public class StartMenu
{
    private string saveFilePath = "savegame.json";

    public void ShowMenu()
    {
        Console.WriteLine("Start Menu");
        Console.WriteLine("1. New Game");
        Console.WriteLine("2. Load Save"); 
        Console.WriteLine("3. Exit");
        
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                StartNewGame();
                break;
            case "2":
                LoadSave();
                break;
            case "3":
                Environment.Exit(0);
                ShowMenu();
                break;
        }
    }

    void StartNewGame()
    {
        Console.WriteLine("Starting new game...");
    }

    void LoadSave()
    {
        if (!File.Exists(saveFilePath))
        {
            Console.WriteLine("Loading save file...");
        }
        else Console.WriteLine("No save file found.");
        ShowMenu();
    }
}