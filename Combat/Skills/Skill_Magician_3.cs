﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

public class Skill_Magician_3 : Skill
{
    public Skill_Magician_3(Character owner)
        : base(owner, "진혼곡(鎭魂曲)", "적 모두에게 아름다운 노래를 불러줍니다.")
    {
    }

    public override bool IsValidFor(Character target)
    {
        if (target.isDead || (owner is PlayerCharacter && target is PlayerCharacter) || (owner is Monster && target is Monster))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override void OnUse(List<Character> targets)
    {
        foreach (Character target in targets)
        {
            AttackOrderInfo attackOrder = owner.CreateAttackOrder(target);
            attackOrder.damage = (int)(attackOrder.damage * 1.3f);
            DefenseOrderInfo defenseOrder = target.CreateDefenseOrder(attackOrder);
            target.health -= defenseOrder.actualDamage;
            Console.WriteLine($"{target.name}이(가) {defenseOrder.actualDamage}의 데미지를 받았습니다.");
            Thread.Sleep(1000);
            //# Todo : 데미지 얼마나 줬는지 표시하기
        }
        Console.WriteLine();
        Console.WriteLine($"가게 두어라! 저놈들은 나의 것이니...   {skillName}!");
        Thread.Sleep(2000);

    }

    public override List<Character> SelectTargetsFrom(List<Character> targets)
    {
        return ParseValidTargetCandidatesFrom(targets);
    }
}