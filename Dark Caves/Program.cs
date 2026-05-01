using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public enum TridaPostavy { Warrior, Archer, Mage }

public class ItemStats {
    public int DmgBonus;
    public int ArmorReduction;
    public int HpBonus;
    public int EnergyBonus;
    public int SellPrice;
    public string Description;
    public string Category; // Pro určení třídy
}

public class Hrac
{
    public string Jmeno { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TridaPostavy Trida { get; set; }
    
    public int Level { get; set; } = 1;
    public int Zkusenosti { get; set; } = 0;
    public int Zdravi { get; set; }
    public int BaseMaxZdravi { get; set; }
    public int Energie { get; set; }
    public int BaseMaxEnergie { get; set; }
    public int Penize { get; set; } = 50;
    public int BaseUtok { get; set; }
    public int Lektvary { get; set; } = 1;
    public int ChargeCounter { get; set; } = 0;
    public List<string> Inventar { get; set; } = new List<string>();
    
    public string Zbran { get; set; } = "Nic";
    public string Brneni { get; set; } = "Hadry";
    public string Doplnek { get; set; } = "Nic";

    public Hrac() { }
    public Hrac(string jmeno, TridaPostavy trida)
    {
        Jmeno = jmeno;
        Trida = trida;
        if (trida == TridaPostavy.Warrior) { BaseMaxZdravi = 150; BaseMaxEnergie = 40; BaseUtok = 12; Zbran = "Tupý meč"; }
        else if (trida == TridaPostavy.Archer) { BaseMaxZdravi = 100; BaseMaxEnergie = 70; BaseUtok = 18; Zbran = "Krátký luk"; }
        else if (trida == TridaPostavy.Mage) { BaseMaxZdravi = 80; BaseMaxEnergie = 100; BaseUtok = 25; Zbran = "Dřevěná hůl"; }
        Zdravi = BaseMaxZdravi;
        Energie = BaseMaxEnergie;
    }
}

public class HerniEngine
{
    private Hrac hrac;
    private string savePath = "savegame.json";
    private Random rnd = new Random();

    public void Spustit(Hrac nactenyHrac = null)
    {
        if (nactenyHrac == null) {
            Console.Clear();
            Console.Write("Zadej jméno hrdiny: ");
            string jmeno = Console.ReadLine();
            Console.WriteLine("Vyber třídu: 1. Bojovník, 2. Lukostřelec, 3. Mág");
            string v = Console.ReadLine();
            TridaPostavy zvolenaTrida = v == "2" ? TridaPostavy.Archer : v == "3" ? TridaPostavy.Mage : TridaPostavy.Warrior;
            hrac = new Hrac(jmeno, zvolenaTrida);
        }
        else hrac = nactenyHrac;
        HlavniSmycka();
    }

    private ItemStats GetItemStats(string item) {
        return item switch {
            "Ostrý meč" => new ItemStats { DmgBonus = 5, SellPrice = 40, Description = "Způsobuje o 5 více poškození.", Category = "Warrior" },
            "Dlouhý luk" => new ItemStats { DmgBonus = 8, SellPrice = 50, Description = "Způsobuje o 8 více poškození.", Category = "Archer" },
            "Magická hůl" => new ItemStats { DmgBonus = 10, SellPrice = 60, Description = "Způsobuje o 10 více poškození.", Category = "Mage" },
            "Kožená zbroj" => new ItemStats { ArmorReduction = 2, SellPrice = 30, Description = "Snižuje poškození o 2.", Category = "ArcherMage" },
            "Železná zbroj" => new ItemStats { ArmorReduction = 5, SellPrice = 70, Description = "Snižuje poškození o 5.", Category = "Warrior" },
            "Prsten síly" => new ItemStats { HpBonus = 20, SellPrice = 40, Description = "Dává +20 max HP.", Category = "All" },
            "Amulet energie" => new ItemStats { EnergyBonus = 10, SellPrice = 40, Description = "Dává +10 max Energie.", Category = "All" },
            _ => new ItemStats { SellPrice = 10, Description = "Běžný předmět.", Category = "All" }
        };
    }

    private bool CanEquip(string item) {
        var stats = GetItemStats(item);
        if (stats.Category == "All") return true;
        if (hrac.Trida == TridaPostavy.Warrior && stats.Category == "Warrior") return true;
        if (hrac.Trida == TridaPostavy.Archer && (stats.Category == "Archer" || stats.Category == "ArcherMage")) return true;
        if (hrac.Trida == TridaPostavy.Mage && (stats.Category == "Mage" || stats.Category == "ArcherMage")) return true;
        return false;
    }

    private int GetCurrentMaxHp() => hrac.BaseMaxZdravi + GetItemStats(hrac.Doplnek).HpBonus;
    private int GetCurrentMaxEnergy() => hrac.BaseMaxEnergie + GetItemStats(hrac.Doplnek).EnergyBonus;
    private int GetTotalDamage() => hrac.BaseUtok + GetItemStats(hrac.Zbran).DmgBonus;
    private int GetArmorReduction() => GetItemStats(hrac.Brneni).ArmorReduction;

    private void HlavniSmycka()
    {
        bool hrajeme = true;
        while (hrajeme) {
            Console.Clear();
            Console.WriteLine($"--- {hrac.Jmeno} | LVL: {hrac.Level} (XP: {hrac.Zkusenosti}/100) ---");
            Console.WriteLine($"HP: {hrac.Zdravi}/{GetCurrentMaxHp()} | E: {hrac.Energie}/{GetCurrentMaxEnergy()}");
            Console.WriteLine("1. Lokace | 2. Inventář | 3. Loadout | 4. Uložit | 5. Odejít");
            string volba = Console.ReadLine();
            if (volba == "1") VyberLokace(); else if (volba == "2") Inventar(); else if (volba == "3") Loadout();
            else if (volba == "4") { Ulozit(); hrajeme = false; } else if (volba == "5") hrajeme = false;
        }
    }

    private void Loadout()
    {
        Console.Clear();
        Console.WriteLine($"--- LOADOUT ---");
        Console.WriteLine($"Zbraň: {hrac.Zbran} (+{GetItemStats(hrac.Zbran).DmgBonus} DMG)");
        Console.WriteLine($"Brnění: {hrac.Brneni} (-{GetArmorReduction()} DMG z útoků)");
        Console.WriteLine($"Doplněk: {hrac.Doplnek}");
        Console.WriteLine("\nStiskni klávesu pro návrat.");
        Console.ReadKey();
    }

    private void Inventar()
    {
        while (true) {
            Console.Clear();
            Console.WriteLine($"--- INVENTÁŘ (Peníze: {hrac.Penize}p | Lektvary: {hrac.Lektvary}) ---");
            for (int i = 0; i < hrac.Inventar.Count; i++) Console.WriteLine($"{i + 1}. {hrac.Inventar[i]}");
            Console.WriteLine("\nZadej číslo předmětu (0 pro návrat):");
            string v = Console.ReadLine();
            if (v == "0") break;
            if (int.TryParse(v, out int idx) && idx > 0 && idx <= hrac.Inventar.Count) {
                string item = hrac.Inventar[idx - 1];
                var stats = GetItemStats(item);
                Console.WriteLine($"Vybráno: {item} | Cena: {stats.SellPrice}p");
                Console.WriteLine($"1. Vybavit | 2. Prodat | 3. Prozkoumat | 4. Zpět");
                string akce = Console.ReadLine();
                if (akce == "1") {
                    if (CanEquip(item)) {
                        string oldItem = "";
                        if (item.Contains("meč") || item.Contains("luk") || item.Contains("hůl") || item.Contains("kuše") || item.Contains("sekera")) { oldItem = hrac.Zbran; hrac.Zbran = item; }
                        else if (item.Contains("zbroj") || item.Contains("hadry")) { oldItem = hrac.Brneni; hrac.Brneni = item; }
                        else { oldItem = hrac.Doplnek; hrac.Doplnek = item; }
                        if (oldItem != "Nic" && oldItem != "Hadry") hrac.Inventar.Add(oldItem);
                        hrac.Inventar.RemoveAt(idx - 1);
                        Console.WriteLine("Vybaveno!");
                    } else {
                        Console.WriteLine("Tento předmět tvá třída nemůže používat!");
                    }
                } else if (akce == "2") { hrac.Penize += stats.SellPrice; hrac.Inventar.RemoveAt(idx - 1); Console.WriteLine($"Prodáno za {stats.SellPrice}p!"); }
                else if (akce == "3") { Console.WriteLine($"Popis: {stats.Description}"); }
                Console.ReadKey();
            }
        }
    }

    private void VyberLokace()
    {
        Console.Clear();
        Console.WriteLine("1. Les (LVL 0+) | 2. Jeskyně (LVL 3+) | 3. Hrad (LVL 5+)");
        string volba = Console.ReadLine();
        if (volba == "1") Boj(1); else if (volba == "2" && hrac.Level >= 3) Boj(2); else if (volba == "3" && hrac.Level >= 5) Boj(3);
    }

    private void Boj(int diff)
    {
        int nepritelHP = (40 + (hrac.Level * 15)) * diff;
        int nepritelUtok = (8 + (hrac.Level * 4)) * diff;
        string jmenoNepritele = (new[] { "Vzteklý vlk", "Skřet", "Troll", "Přízrak" })[rnd.Next(4)];
        if (diff == 3) { jmenoNepritele = "Vlkodlačí král"; nepritelHP = 300; nepritelUtok = 35; }
        
        Console.Clear(); Console.WriteLine($"!!! SOUBOJ !!!");
        while (nepritelHP > 0 && hrac.Zdravi > 0) {
            bool enemyCanAttack = true; 
            
            Console.WriteLine($"\nNEPŘÍTEL: {jmenoNepritele} | HP: {nepritelHP} | TVÉ HP: {hrac.Zdravi}/{GetCurrentMaxHp()} | E: {hrac.Energie}/{GetCurrentMaxEnergy()}");
            Console.WriteLine($"1. Útok (10E) | 2. Obrana (+15E) | 3. Lektvar ({hrac.Lektvary}) | 4. Speciální ({hrac.ChargeCounter}/3)");
            string akce = Console.ReadLine();
            
            if (akce == "1") { 
                if (hrac.Energie >= 10) {
                    hrac.Energie -= 10;
                    int dmg = rnd.Next(GetTotalDamage() - 3, GetTotalDamage() + 7);
                    nepritelHP -= dmg; 
                    if (hrac.ChargeCounter < 3) hrac.ChargeCounter++; 
                    Console.WriteLine($"Zasáhl jsi za {dmg}!"); 
                } else {
                    Console.WriteLine("Nemáš dost energie na útok!");
                }
            } 
            else if (akce == "2") {
                hrac.Energie = Math.Min(GetCurrentMaxEnergy(), hrac.Energie + 15);
                Console.WriteLine("Bráníš se a regeneruješ 15 energie.");
            }
            else if (akce == "3" && hrac.Lektvary > 0) { 
                hrac.Zdravi = Math.Min(GetCurrentMaxHp(), hrac.Zdravi + 40); 
                hrac.Lektvary--; 
                enemyCanAttack = false; 
                Console.WriteLine("Vypil jsi lektvar a získal 40 HP!");
            }
            else if (akce == "4" && hrac.ChargeCounter >= 3) { 
                int dmg = GetTotalDamage() * 3; 
                nepritelHP -= dmg; 
                hrac.ChargeCounter = 0; 
                Console.WriteLine($"Speciální útok! {dmg} dmg!"); 
            }
            
            if (nepritelHP > 0 && enemyCanAttack) { 
                int rawDmg = rnd.Next(nepritelUtok - 3, nepritelUtok + 3);
                int finalDmg = Math.Max(1, rawDmg - GetArmorReduction());
                hrac.Zdravi -= finalDmg;
                Console.WriteLine($"{jmenoNepritele} tě zasáhl za {finalDmg} (Brnění snížilo {GetArmorReduction()} DMG)!");
            }
        }
        if (nepritelHP <= 0) {
            int lDrop = rnd.Next(1, 3); hrac.Lektvary += lDrop;
            string[] loot = { "Ostrý meč", "Dlouhý luk", "Magická hůl", "Kožená zbroj", "Železná zbroj", "Prsten síly", "Amulet energie" };
            string found = loot[rnd.Next(loot.Length)];
            hrac.Inventar.Add(found);
            
            int xpGain = 35 * diff;
            hrac.Zkusenosti += xpGain;
            Console.WriteLine($"Vyhrál jsi! Našel jsi: {found}, {lDrop} lektvarů a získal {xpGain} XP.");
            
            if (hrac.Zkusenosti >= 100) { hrac.Level++; hrac.Zkusenosti = 0; hrac.BaseMaxZdravi += 25; hrac.Zdravi = GetCurrentMaxHp(); }
        }
        Console.ReadKey();
    }
    private void Ulozit() { File.WriteAllText(savePath, JsonSerializer.Serialize(hrac)); }
}

public class StartMenu
{
    public void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("1. New Game | 2. Load | 3. Exit");
        string c = Console.ReadLine();
        HerniEngine e = new HerniEngine();
        if (c == "1") e.Spustit();
        else if (c == "2" && File.Exists("savegame.json")) e.Spustit(JsonSerializer.Deserialize<Hrac>(File.ReadAllText("savegame.json")));
        else if (c == "3") return;
        else ShowMenu();
    }
}

class Program { static void Main() => new StartMenu().ShowMenu(); }
