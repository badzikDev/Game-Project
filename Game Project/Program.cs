using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

public class Hrac
{
    public string Jmeno { get; set; }
    public int Level { get; set; } = 1;
    public int Zkusenosti { get; set; } = 0;
    public int Zdravi { get; set; } = 100;
    public int MaxZdravi { get; set; } = 100;
    public int Penize { get; set; } = 50;
    public int Utok { get; set; } = 15;
    public List<string> Inventar { get; set; } = new List<string>();

    public Hrac() { }

    public Hrac(string jmeno)
    {
        Jmeno = jmeno;
        Inventar.Add("Rezavý nůž");
    }
}

public class HerniEngine
{
    private Hrac hrac;
    private string savePath = "savegame.json";
    private Random rnd = new Random();

    public void Spustit(Hrac nactenyHrac = null)
    {
        if (nactenyHrac == null)
        {
            Console.Clear();
            Console.Write("Zadej jméno hrdiny: ");
            string jmeno = Console.ReadLine();
            hrac = new Hrac(jmeno);
        }
        else
        {
            hrac = nactenyHrac;
        }

        HlavniSmycka();
    }

    private void HlavniSmycka()
    {
        bool hrajeme = true;
        while (hrajeme)
        {
            Console.Clear();
            Console.WriteLine($"--- {hrac.Jmeno} | LVL: {hrac.Level} | HP: {hrac.Zdravi}/{hrac.MaxZdravi} | Peníze: {hrac.Penize} ---");
            Console.WriteLine("1. Jít bojovat");
            Console.WriteLine("2. Inventář");
            Console.WriteLine("3. Uložit a odejít");
            Console.Write("\nVolba: ");

            string volba = Console.ReadLine();
            switch (volba)
            {
                case "1": Boj(); break;
                case "2": Inventar(); break;
                case "3": Ulozit(); hrajeme = false; break;
            }
        }
    }

    private void Boj()
    {
        int nepritelHP = 30 + (hrac.Level * 10);
        int nepritelUtok = 5 + (hrac.Level * 2);
        string[] nepratele = { "Vlk", "Skřet", "Zloděj" };
        string jmenoNepritele = nepratele[rnd.Next(nepratele.Length)];

        Console.Clear();
        Console.WriteLine($"Souboj: {jmenoNepritele} se objevil!");

        while (nepritelHP > 0 && hrac.Zdravi > 0)
        {
            Console.WriteLine($"\nTvůj život: {hrac.Zdravi} | {jmenoNepritele}: {nepritelHP}");
            Console.WriteLine("1. Útok | 2. Obrana");
            string akce = Console.ReadLine();

            if (akce == "1")
            {
                int dmg = rnd.Next(hrac.Utok - 5, hrac.Utok + 5);
                nepritelHP -= dmg;
                Console.WriteLine($"Zasáhl jsi {jmenoNepritele} za {dmg} poškození!");
            }
            else
            {
                Console.WriteLine("Zaujal jsi obranou pozici.");
            }

            if (nepritelHP > 0)
            {
                int dmgNepritel = rnd.Next(nepritelUtok - 2, nepritelUtok + 2);
                if (akce == "2") dmgNepritel /= 2;
                hrac.Zdravi -= dmgNepritel;
                Console.WriteLine($"{jmenoNepritele} tě zasáhl za {dmgNepritel}!");
            }

            if (hrac.Zdravi <= 0)
            {
                Console.WriteLine("\nByl jsi poražen...");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
        }

        if (nepritelHP <= 0)
        {
            int loot = 15 + (hrac.Level * 5);
            int xp = 25;
            hrac.Penize += loot;
            hrac.Zkusenosti += xp;
            Console.WriteLine($"\nZvítězil jsi! Získal jsi {loot} peněz a {xp} zkušeností.");
            if (hrac.Zkusenosti >= 100) LevelUp();
        }
        Console.ReadKey();
    }

    private void LevelUp()
    {
        hrac.Level++;
        hrac.Zkusenosti = 0;
        hrac.MaxZdravi += 20;
        hrac.Zdravi = hrac.MaxZdravi;
        hrac.Utok += 5;
        Console.WriteLine("\nLEVEL UP! Tvé statistiky se zvýšily.");
    }

    private void Inventar()
    {
        Console.Clear();
        Console.WriteLine("Tvůj inventář:");
        foreach (var vec in hrac.Inventar) Console.WriteLine("- " + vec);
        Console.WriteLine("\nStiskni klávesu...");
        Console.ReadKey();
    }

    private void Ulozit()
    {
        string json = JsonSerializer.Serialize(hrac);
        File.WriteAllText(savePath, json);
        Console.WriteLine("Hra uložena.");
    }
}

public class StartMenu
{
    private string saveFilePath = "savegame.json";

    public void ShowMenu()
    {
        Console.Clear(); 
        Console.WriteLine("=== MOJE RPG HRA ===");
        Console.WriteLine("1. New Game");
        Console.WriteLine("2. Load Save"); 
        Console.WriteLine("3. Exit");
        Console.Write("\nVyberte akci: ");
        
        string choice = Console.ReadLine();
        HerniEngine engine = new HerniEngine();

        switch (choice)
        {
            case "1":
                engine.Spustit();
                ShowMenu();
                break;
            case "2":
                LoadSave(engine);
                break;
            case "3":
                Environment.Exit(0);
                break;
            default:
                ShowMenu();
                break;
        }
    }

    void LoadSave(HerniEngine engine)
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            Hrac nacteny = JsonSerializer.Deserialize<Hrac>(json);
            engine.Spustit(nacteny);
        }
        else 
        {
            Console.WriteLine("No save file found.");
            Console.ReadKey();
        }
        ShowMenu();
    }
}

class Program 
{
    static void Main(string[] args)
    {
        StartMenu menu = new StartMenu();
        menu.ShowMenu();
    }
}