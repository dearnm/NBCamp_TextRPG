﻿public struct CharacterStats
{
    public CharacterStats(int maxHealth = 100, int minAttack = 10, int maxAttack = 20, int minArmor = 5, int maxArmor = 10)
    {
        this.maxHealth = maxHealth;
        this.minAttack = minAttack;
        this.maxAttack = maxAttack;
        this.minArmor = minArmor;
        this.maxArmor = maxArmor;
    }

    public int maxHealth { get; set; }
    public int minAttack { get; set; }
    public int maxAttack { get; set; }
    public int minArmor { get; set; }
    public int maxArmor { get; set; }

    public static CharacterStats operator +(CharacterStats stat1, CharacterStats stat2)
    {
        CharacterStats result = new CharacterStats(
            stat1.maxHealth + stat2.maxHealth,
            stat1.minAttack + stat2.minAttack,
            stat1.maxAttack + stat2.maxAttack,
            stat1.minArmor + stat2.minArmor,
            stat1.maxArmor + stat2.maxArmor);
        return result;
    }

    public static CharacterStats operator -(CharacterStats stat1, CharacterStats stat2)
    {
        CharacterStats result = new CharacterStats(
            stat1.maxHealth - stat2.maxHealth,
            stat1.minAttack - stat2.minAttack,
            stat1.maxAttack - stat2.maxAttack,
            stat1.minArmor - stat2.minArmor,
            stat1.maxArmor - stat2.maxArmor);
        return result;
    }
}