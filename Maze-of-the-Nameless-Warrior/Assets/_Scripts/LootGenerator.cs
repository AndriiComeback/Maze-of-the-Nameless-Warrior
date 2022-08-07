using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootGenerator
{
    public Item Generate(LootRarity rarity) {
        LootStatUpgrade stat = (LootStatUpgrade)Random.Range(0, 3);
        int value = ((int)rarity + 1) * Random.Range(1, 3);
        string name = GenerateName(stat, value);
        return new Item(stat, value, name);
    }
    protected string GenerateName(LootStatUpgrade stat, int value) {
        return $"Artifacto (+{value} {stat.ToString()})";
    }
}

public enum LootRarity { Common, Rare, Epic, Legendary }

public enum LootStatUpgrade { Damage, Health, Initiative }

public class Item {
    public LootStatUpgrade stat;
    public int value;
    public string name;
    public Item(LootStatUpgrade stat, int value, string name) {
        this.stat = stat;
        this.value = value;
        this.name = name;
    }
}