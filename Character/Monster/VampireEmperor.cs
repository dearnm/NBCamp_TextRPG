﻿using System;

public class VampireEmperor : Monster
{
    public VampireEmperor()
        : base("뱀파이어 황제", new CharacterStats(200, 20, 30, 15, 20, 10, 20, 15))
    {
        rewardGold = 1000;
        rewardExp = 18;
        Random rnd = new Random();

        if (rnd.Next(0, 10) < 1)
        {
            rewardItems.Add(new LongSword());
        }

        if (rnd.Next(0, 10) < 1)
        {
            rewardItems.Add(new MagicArmor());
        }

        monsterAiComponent.AddActionWeight(new AiAction_AttackOne(this), 7);
        monsterAiComponent.AddActionWeight(new AiAction_AttackThree(this), 12);
        monsterAiComponent.AddActionWeight(new AiAction_HealOne(this), 3);
    }
}