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
        else hrac = nactenyHrac;

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
            Console.WriteLine("4. Odejít bez uložení");
            Console.Write("\nVolba: ");

            string volba = Console.ReadLine();
            switch (volba)
            {
                case "1": Boj(); break;
                case "2": Inventar(); break;
                case "3": Ulozit(); hrajeme = false; break;
                case "4": 
                    Console.Write("Opravdu odejít bez uložení? (a/n): ");
                    if (Console.ReadLine().ToLower() == "a") hrajeme = false;
                    break;
            }
        }
    }

    private void Boj()
    {
        int maxNepritelHP = 30 + (hrac.Level * 10);
        int nepritelHP = maxNepritelHP;
        int nepritelUtok = 5 + (hrac.Level * 2);
        string[] nepratele = { "Vlk", "Skřet", "Zloděj", "Temný rytíř" };
        string jmenoNepritele = nepratele[rnd.Next(nepratele.Length)];

        Console.Clear();
        Console.WriteLine($"Souboj: {jmenoNepritele} se objevil!");

        while (nepritelHP > 0 && hrac.Zdravi > 0)
        {
            Console.WriteLine($"\nTvůj život: {hrac.Zdravi} | {jmenoNepritele}: {nepritelHP}");
            Console.WriteLine("1. Útok | 2. Obrana");
            string akce = Console.ReadLine();

            bool hracSeBrani = (akce == "2");
            bool nepritelSeBrani = (nepritelHP < (maxNepritelHP * 0.3) && rnd.Next(100) < 50);

            if (akce == "1")
            {
                int dmg = rnd.Next(hrac.Utok - 5, hrac.Utok + 5);
                if (nepritelSeBrani) dmg /= 3;
                nepritelHP -= dmg;
                Console.WriteLine($"Zasáhl jsi {jmenoNepritele} za {dmg} poškození!");
            }

            if (nepritelHP > 0)
            {
                if (!nepritelSeBrani)
                {
                    int dmgNepritel = rnd.Next(nepritelUtok - 2, nepritelUtok + 2);
                    if (hracSeBrani)
                    {
                        dmgNepritel /= 3;
                        Console.WriteLine($"Vykryl jsi útok! (Jen {dmgNepritel} dmg)");
                        if (rnd.Next(100) < 70)
                        {
                            int protidmg = hrac.Utok / 2;
                            nepritelHP -= protidmg;
                            Console.WriteLine($"PROTIÚTOK za {protidmg}!");
                        }
                    }
                    hrac.Zdravi -= dmgNepritel;
                    if (!hracSeBrani) Console.WriteLine($"{jmenoNepritele} tě zasáhl za {dmgNepritel}!");
                }
                else Console.WriteLine($"{jmenoNepritele} se brání.");
            }

            if (hrac.Zdravi <= 0)
            {
                Console.WriteLine("\nZemřel jsi...");
                Thread.Sleep(2000);
                return;
            }
        }

        if (nepritelHP <= 0)
        {
            int loot = 15 + (hrac.Level * 5);
            hrac.Penize += loot;
            hrac.Zkusenosti += 25;
            Console.WriteLine($"\nVítězství! +{loot} peněz.");
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
        Console.WriteLine("LEVEL UP!");
    }

    private void Inventar()
    {
        Console.Clear();
        Console.WriteLine("Inventář:");
        hrac.Inventar.ForEach(v => Console.WriteLine("- " + v));
        Console.ReadKey();
    }

    private void Ulozit()
    {
        string json = JsonSerializer.Serialize(hrac);
        File.WriteAllText(savePath, json);
        Console.WriteLine("Hra uložena.");
        Thread.Sleep(1000);
    }
}

public class StartMenu
{
    private string saveFilePath = "savegame.json";

    public void ShowMenu()
    {
        Console.Clear(); 
        Console.WriteLine("=== RPG HRA ===");
        Console.WriteLine("1. New Game");
        Console.WriteLine("2. Load Save"); 
        Console.WriteLine("3. Exit");
        Console.Write("\nVolba: ");
        
        string choice = Console.ReadLine();
        HerniEngine engine = new HerniEngine();

        if (choice == "1") engine.Spustit();
        else if (choice == "2" && File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            engine.Spustit(JsonSerializer.Deserialize<Hrac>(json));
        }
        else if (choice == "3") Environment.Exit(0);
        
        ShowMenu();
    }
}

class Program 
{
    static void Main(string[] args)
    {
        new StartMenu().ShowMenu();
    }
}