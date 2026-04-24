using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization; 
using System.Threading;

public enum TridaPostavy { Warrior, Archer, Mage }

public class Hrac
{
    public string Jmeno { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public TridaPostavy Trida { get; set; }
    
    public int Level { get; set; } = 1;
    public int Zkusenosti { get; set; } = 0;
    public int Zdravi { get; set; }
    public int MaxZdravi { get; set; }
    public int Energie { get; set; }
    public int MaxEnergie { get; set; }
    public int Penize { get; set; } = 50;
    public int Utok { get; set; }
    public int Lektvary { get; set; } = 1;
    public int ChargeCounter { get; set; } = 0;
    public List<string> Inventar { get; set; } = new List<string>();

    public Hrac() { }
    public Hrac(string jmeno, TridaPostavy trida)
    {
        Jmeno = jmeno;
        Trida = trida;
        if (trida == TridaPostavy.Warrior) { MaxZdravi = 150; MaxEnergie = 40; Utok = 12; Inventar.Add("Tupý meč"); }
        else if (trida == TridaPostavy.Archer) { MaxZdravi = 100; MaxEnergie = 70; Utok = 18; Inventar.Add("Krátký luk"); }
        else if (trida == TridaPostavy.Mage)
        {
            MaxZdravi = 80;
            MaxEnergie = 100;
            Utok = 25;
            Inventar.Add("Dřevěná hůl");
        }
        else
        {
            MaxZdravi = 0;
            MaxEnergie = 0;
            Utok = 0;
        }
        Zdravi = MaxZdravi;
        Energie = MaxEnergie;
    }

    public void PridatLoot(string predmet)
    {
        bool muze = (Trida == TridaPostavy.Warrior && (predmet.Contains("meč") || predmet.Contains("sekera"))) ||
                    (Trida == TridaPostavy.Archer && (predmet.Contains("luk") || predmet.Contains("kuše"))) ||
                    (Trida == TridaPostavy.Mage && (predmet.Contains("hůl") || predmet.Contains("kniha")));

        if (muze) { Inventar.Add(predmet); Console.WriteLine($"Našel jsi: {predmet}!"); }
        else { Penize += 15; Console.WriteLine($"Našel jsi {predmet}, ale neumíš to použít. Prodáno za 15p"); }
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
            Console.WriteLine("Vyber třídu: 1. Bojovník, 2. Lukostřelec, 3. Mág");
            string v = Console.ReadLine();
            TridaPostavy zvolenaTrida;

            if (v == "1") zvolenaTrida = TridaPostavy.Warrior;
            else if (v == "2") zvolenaTrida = TridaPostavy.Archer;
            else if (v == "3") zvolenaTrida = TridaPostavy.Mage;
            else { Console.WriteLine("Neplatné číslo"); return; }

            hrac = new Hrac(jmeno, zvolenaTrida);
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
            Console.WriteLine($"--- {hrac.Jmeno} ({hrac.Trida}) | LVL: {hrac.Level} | HP: {hrac.Zdravi}/{hrac.MaxZdravi} | E: {hrac.Energie}/{hrac.MaxEnergie} ---");
            Console.WriteLine("1. Jít bojovat | 2. Inventář | 3. Uložit | 4. Odejít");
            string volba = Console.ReadLine();
            if (volba == "1") Boj(); else if (volba == "2") Inventar(); else if (volba == "3") { Ulozit(); hrajeme = false; } else if (volba == "4") hrajeme = false;
        }
    }

    private void Boj()
    {
        int nepritelHP = 40 + (hrac.Level * 15), nepritelUtok = 8 + (hrac.Level * 4);
        string jmenoNepritele = (new[] { "Vzteklý vlk", "Skřet", "Troll", "Přízrak" })[rnd.Next(4)];
        Console.Clear(); Console.WriteLine($"!!! SOUBOJ: {jmenoNepritele} !!!");

        while (nepritelHP > 0 && hrac.Zdravi > 0)
        {
            Console.WriteLine($"\nNEPŘÍTEL: {jmenoNepritele} | HP: {nepritelHP}");
            Console.WriteLine($"{hrac.Jmeno}: {hrac.Zdravi} HP | {hrac.Energie} E | 1. Útok | 2. Obrana | 3. Lektvar | 4. Speciální Útok ({hrac.ChargeCounter}/3)");
            string akce = Console.ReadLine();
            bool hracSeBrani = (akce == "2");

            if (akce == "1") { 
                int dmg = hrac.Energie >= 15 ? rnd.Next(hrac.Utok - 3, hrac.Utok + 7) : 3;
                hrac.Energie = Math.Max(0, hrac.Energie - 15); 
                nepritelHP -= dmg; 
                hrac.ChargeCounter++;
                Console.WriteLine($"Zasáhl jsi za {dmg}!"); 
            }
            else if (akce == "3" && hrac.Lektvary > 0) { hrac.Zdravi = Math.Min(hrac.MaxZdravi, hrac.Zdravi + 40); hrac.Lektvary--; }
            else if (akce == "4") 
            {
                int cost = hrac.MaxEnergie / 2;
                if (hrac.ChargeCounter >= 3 && hrac.Energie >= cost) {
                    int dmg = 0;
                    string nazevSpecialky = "";
                    switch (hrac.Trida) {
                        case TridaPostavy.Warrior: dmg = hrac.Utok * 2 + 10; nazevSpecialky = "Těžký Úder"; break;
                        case TridaPostavy.Archer: dmg = hrac.Utok * 2 + 5; nazevSpecialky = "Zlatý Šíp"; break;
                        case TridaPostavy.Mage: dmg = hrac.Utok * 3; nazevSpecialky = "Ohnivá koule"; break;
                    }
                    nepritelHP -= dmg;
                    hrac.Energie -= cost;
                    hrac.ChargeCounter = 0;
                    Console.WriteLine($"POUŽIL JSI {nazevSpecialky.ToUpper()}! Udělil jsi {dmg} poškození!");
                } else {
                    Console.WriteLine($"Speciální útok není nabitý nebo nemáš dost energie ({cost} E)!");
                }
            }
            else if (hracSeBrani) hrac.Energie = Math.Min(hrac.MaxEnergie, hrac.Energie + 20);

            if (nepritelHP > 0 && akce != "4" && akce != "3") { 
                int dmgN = rnd.Next(nepritelUtok - 3, nepritelUtok + 3);
                if (hracSeBrani) dmgN /= 4;
                hrac.Zdravi -= dmgN; Console.WriteLine($"{jmenoNepritele} tě zasáhl za {dmgN}!");
            }

            if (hrac.Zdravi <= 0) {
                int pen = hrac.Penize / 4; hrac.Penize -= pen; hrac.Zdravi = hrac.MaxZdravi / 2;
                hrac.ChargeCounter = 0; 
                Console.WriteLine($"\nPADL JSI! Ztratil jsi {pen} peněz. Probouzíš se s polovinou HP.");
                Console.ReadKey(); return;
            }
        }
        if (nepritelHP <= 0) {
            hrac.Penize += 20 + (hrac.Level * 10); hrac.Zkusenosti += 35;
            if (rnd.Next(100) < 30) hrac.PridatLoot((new[] { "Ostrý meč", "Dlouhý luk", "Magická hůl" })[rnd.Next(3)]);
            if (hrac.Zkusenosti >= 100) LevelUp();
        }
        Console.ReadKey();
    }

    private void LevelUp()
    {
        hrac.Level++; hrac.Zkusenosti = 0; hrac.MaxZdravi += 25; hrac.MaxEnergie += 10; hrac.Utok += 6;
        hrac.Zdravi = hrac.MaxZdravi; hrac.Energie = hrac.MaxEnergie;
        Console.WriteLine("!!! LEVEL UP !!!");
    }

    private void Inventar() { Console.Clear(); hrac.Inventar.ForEach(v => Console.WriteLine("- " + v)); Console.ReadKey(); }
    private void Ulozit() { File.WriteAllText(savePath, JsonSerializer.Serialize(hrac)); }
}

public class StartMenu
{
    public void ShowMenu()
    {
        Console.Clear(); Console.WriteLine("1. New Game | 2. Load | 3. Exit");
        string c = Console.ReadLine(); HerniEngine e = new HerniEngine();
        if (c == "1") e.Spustit();
        else if (c == "2" && File.Exists("savegame.json")) e.Spustit(JsonSerializer.Deserialize<Hrac>(File.ReadAllText("savegame.json")));
        else if (c == "3") return; else ShowMenu();
    }
}

class Program { static void Main() => new StartMenu().ShowMenu(); }