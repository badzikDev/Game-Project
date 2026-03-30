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
    public int Energie { get; set; } = 50;
    public int MaxEnergie { get; set; } = 50;
    public int Penize { get; set; } = 50;
    public int Utok { get; set; } = 15;
    public int Lektvary { get; set; } = 1;
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
            Console.WriteLine($"--- {hrac.Jmeno} | LVL: {hrac.Level} | HP: {hrac.Zdravi}/{hrac.MaxZdravi} | E: {hrac.Energie}/{hrac.MaxEnergie} | Lektvary: {hrac.Lektvary} ---");
            Console.WriteLine("1. Jít bojovat (Pozor, nebezpečí!)");
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
                case "4": hrajeme = false; break;
            }
        }
    }

    private void Boj()
    {
        // Nepřátelé jsou nyní o 40% silnější než dříve
        int maxNepritelHP = 40 + (hrac.Level * 15);
        int nepritelHP = maxNepritelHP;
        int nepritelUtok = 8 + (hrac.Level * 4);
        string[] nepratele = { "Vzteklý vlk", "Zkušený skřet", "Horský troll", "Přízrak" };
        string jmenoNepritele = nepratele[rnd.Next(nepratele.Length)];

        Console.Clear();
        Console.WriteLine($"!!! SOUBOJ: {jmenoNepritele} !!!");

        while (nepritelHP > 0 && hrac.Zdravi > 0)
        {
            Console.WriteLine($"\n{hrac.Jmeno}: {hrac.Zdravi} HP | {hrac.Energie} E | Lektvary: {hrac.Lektvary}");
            Console.WriteLine($"{jmenoNepritele}: {nepritelHP} HP");
            Console.WriteLine("1. Útok (-15 E) | 2. Obrana (+20 E) | 3. Lektvar (Léčí 40 HP)");
            string akce = Console.ReadLine();

            bool hracSeBrani = (akce == "2");
            
            // Tah Hráče
            if (akce == "1")
            {
                if (hrac.Energie >= 15)
                {
                    int dmg = rnd.Next(hrac.Utok - 3, hrac.Utok + 7);
                    hrac.Energie -= 15;
                    nepritelHP -= dmg;
                    Console.WriteLine($"Zasáhl jsi za {dmg}!");
                }
                else
                {
                    Console.WriteLine("Jsi příliš unavený! Útok byl velmi slabý.");
                    nepritelHP -= 3;
                }
            }
            else if (akce == "3")
            {
                if (hrac.Lektvary > 0)
                {
                    hrac.Zdravi = Math.Min(hrac.MaxZdravi, hrac.Zdravi + 40);
                    hrac.Lektvary--;
                    Console.WriteLine("Vypil jsi lektvar. Cítíš se lépe.");
                }
                else Console.WriteLine("Nemáš žádné lektvary!");
            }
            else if (hracSeBrani)
            {
                hrac.Energie = Math.Min(hrac.MaxEnergie, hrac.Energie + 20);
                Console.WriteLine("Odpočíváš a kryješ se.");
            }

            // Tah Nepřítele
            if (nepritelHP > 0)
            {
                int dmgNepritel = rnd.Next(nepritelUtok - 3, nepritelUtok + 3);
                
                // Kritický zásah nepřítele (15% šance)
                if (rnd.Next(100) < 15) { dmgNepritel *= 2; Console.WriteLine("KRITICKÝ ZÁSAH NEPŘÍTELE!"); }

                if (hracSeBrani)
                {
                    dmgNepritel /= 4;
                    Console.WriteLine($"Vykryl jsi ránu. Ztratil jsi jen {dmgNepritel} HP.");
                }

                hrac.Zdravi -= dmgNepritel;
                if (!hracSeBrani) Console.WriteLine($"{jmenoNepritele} tě zasáhl za {dmgNepritel}!");
            }

            if (hrac.Zdravi <= 0)
            {
                Console.WriteLine("\nBYL JSI PORAŽEN. HRA KONČÍ.");
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }

        if (nepritelHP <= 0)
        {
            int loot = 20 + (hrac.Level * 10);
            hrac.Penize += loot;
            hrac.Zkusenosti += 35;
            
            // Šance na nalezení lektvaru po boji
            if (rnd.Next(100) < 40) { hrac.Lektvary++; Console.WriteLine("Našel jsi u nepřítele lektvar!"); }

            Console.WriteLine($"Vítězství! +{loot} peněz.");
            if (hrac.Zkusenosti >= 100) LevelUp();
        }
        Console.ReadKey();
    }

    private void LevelUp()
    {
        hrac.Level++;
        hrac.Zkusenosti = 0;
        hrac.MaxZdravi += 25;
        hrac.MaxEnergie += 10;
        hrac.Zdravi = hrac.MaxZdravi;
        hrac.Energie = hrac.MaxEnergie;
        hrac.Utok += 6;
        Console.WriteLine("!!! LEVEL UP !!! Jsi silnější a máš více energie.");
    }

    private void Inventar()
    {
        Console.Clear();
        Console.WriteLine($"Peníze: {hrac.Penize} | Lektvary: {hrac.Lektvary}");
        Console.WriteLine("Vybavení:");
        hrac.Inventar.ForEach(v => Console.WriteLine("- " + v));
        Console.ReadKey();
    }

    private void Ulozit()
    {
        File.WriteAllText(savePath, JsonSerializer.Serialize(hrac));
        Console.WriteLine("Uloženo.");
        Thread.Sleep(1000);
    }
}

public class StartMenu
{
    private string saveFilePath = "savegame.json";

    public void ShowMenu()
    {
        Console.Clear(); 
        Console.WriteLine("=== HARDCORE RPG ===");
        Console.WriteLine("1. New Game | 2. Load Save | 3. Exit");
        string choice = Console.ReadLine();
        HerniEngine engine = new HerniEngine();

        if (choice == "1") engine.Spustit();
        else if (choice == "2" && File.Exists(saveFilePath))
            engine.Spustit(JsonSerializer.Deserialize<Hrac>(File.ReadAllText(saveFilePath)));
        else if (choice == "3") Environment.Exit(0);
        else ShowMenu();
    }
}

class Program { static void Main() => new StartMenu().ShowMenu(); }