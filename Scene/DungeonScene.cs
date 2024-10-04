﻿using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;


public class DungeonScene : IScene
{
    private List<PlayerCharacter> playerCharacters = new List<PlayerCharacter>();
    private List<Monster> monsters = new List<Monster>();
    List<Character> turnOrder;
    int turnOrderIndex = 0;

    public DungeonScene(List<Monster> monsters)
    {
        playerCharacters.Add(GameManager.instance.playerCharacter);
        //#Todo : Add troops into playerCharacters

        foreach (var key in GameManager.instance.mercenarySlot.Keys)
        {
            Mercenary value;
            GameManager.instance.mercenarySlot.TryGetValue(key, out value);
            if (value.name == "빈 슬롯")
                continue;
            playerCharacters.Add(value);
        }

        //========================================



        this.monsters = monsters;
    }

    public IScene GetNextScene()
    {
        return new DungeonSelectionScene();
    }

    public void OnShow()
    {
        SoundPlayer soundPlayer = new SoundPlayer();
        soundPlayer.Play("sound\\pokemon-battle.mp3");
        AsciiArt.Draw("img\\MeetEnemy01.jpg", 90);
        Console.WriteLine();
        Console.WriteLine("┌────────────────────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                                                                                        │");
        Console.WriteLine("│                         앗! 야생의 몬스터가 나타났다! 어떡하지?                        │");
        Console.WriteLine("│                                                                                        │");
        Console.WriteLine("└────────────────────────────────────────────────────────────────────────────────────────┘");
        Thread.Sleep(2500);
        bool isOnBattle = true;
        while (isOnBattle)
        {
            Console.Clear();
            ShowCharactersInfo();
            Character turnOwner = GetCurrentTurnOwner();
            if (turnOwner == null)
            {
                OnInitTurnCycle();
                turnOwner = GetCurrentTurnOwner();
            }
            if (turnOwner.isDead)
            {
                OnTurnEnd();
                continue;
            }
            DoTurn(turnOwner);
            Thread.Sleep(1000);
            if (IsAllPlayerCharacterDead())
            {
                isOnBattle = false;
                soundPlayer.Stop();
                ShowLose();
                break;
            }
            else if (IsAllMonsterDead())
            {
                isOnBattle = false;
                soundPlayer.Stop();
                ShowWin();
                break;
            }
            OnTurnEnd();
        }

        foreach (PlayerCharacter playerCharacter in playerCharacters)
        {
            if (playerCharacter.isDead)
            {
                playerCharacter.health = 1;
            }
        }
    }

    private void ShowCharactersInfo()
    {
        ShowPlayerCharactersInfo();
        ShowMonstersInfo();
    }

    private void ShowPlayerCharactersInfo()
    {
        Console.WriteLine($"┌─────────────────────────────────── 내 정보 ─────────────────────────────────┐");
        foreach (PlayerCharacter playerCharacter in playerCharacters)
        {
            if (playerCharacter.isDead)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.Write($"[ {playerCharacter.name} ] : ");
            Console.ResetColor();
            Console.WriteLine($"체력({playerCharacter.health}/{playerCharacter.stats.maxHealth}) 공격력({playerCharacter.stats.minAttack}~{playerCharacter.stats.maxAttack}) 방어력({playerCharacter.stats.minArmor}~{playerCharacter.stats.maxArmor}) 민첩({playerCharacter.stats.minAgility}~{playerCharacter.stats.maxAgility}) 크리티컬확률({(playerCharacter.stats.criticalRate * 100).ToString("n2")}%)");
        }
        Console.WriteLine($"└─────────────────────────────────────────────────────────────────────────────┘");
    }

    private void ShowMonstersInfo()
    {
        Console.WriteLine($"┌───────────────────────────────── 몬스터 정보 ───────────────────────────────┐");
        foreach (Monster monster in monsters)
        {
            if (monster.isDead)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write($"[ {monster.name} ] : ");
            Console.ResetColor();
            Console.WriteLine($"체력({monster.health}/{monster.stats.maxHealth}) 공격력({monster.stats.minAttack}~{monster.stats.maxAttack}) 방어력({monster.stats.minArmor}~{monster.stats.maxArmor}) 민첩({monster.stats.minAgility}~{monster.stats.maxAgility}) 크리티컬확률({(monster.stats.criticalRate * 100).ToString("n2")}%)");
        }
        Console.WriteLine($"└─────────────────────────────────────────────────────────────────────────────┘");
    }

    private List<Character> GetAllCharacters()
    {
        List<Character> characters = new List<Character>();
        characters.AddRange(playerCharacters);
        characters.AddRange(monsters);
        return characters;
    }

    private List<Character> CreateTurnOrder()
    {
        List<Character> turnOrder = GetAllCharacters();
        for (int i = turnOrder.Count - 1; i >= 0; i--)
        {
            if (turnOrder[i].isDead)
            {
                turnOrder.RemoveAt(i);
            }
        }
        turnOrder.Sort((x, y) =>
        {
            Random rnd = new Random();
            int xOrder = rnd.Next(x.stats.minAgility, x.stats.maxAgility + 1);
            int yOrder = rnd.Next(y.stats.minAgility, y.stats.maxAgility + 1);
            return yOrder.CompareTo(xOrder);
        });
        return turnOrder;
    }

    private Character GetCurrentTurnOwner()
    {
        if (turnOrder == null || turnOrder.Count <= turnOrderIndex)
        {
            return null;
        }

        return turnOrder[turnOrderIndex];
    }

    private void OnInitTurnCycle()
    {
        turnOrderIndex = 0;
        turnOrder = CreateTurnOrder();
    }
    private void OnTurnEnd()
    {
        turnOrderIndex++;
    }

    private void DoTurn(Character turnOwner)
    {
        Console.WriteLine($"[ {turnOwner.name}의 턴 ]");
        if (turnOwner is PlayerCharacter)
        {
            DoPlayerTurn((PlayerCharacter)turnOwner, monsters);
        }
        else if (turnOwner is Monster)
        {
            DoMonsterTurn((Monster)turnOwner);
        }
    }

    private void DoPlayerTurn(PlayerCharacter turnOwner, List<Monster> monsters)
    {

        Console.WriteLine("행동 선택");
        Console.WriteLine("1.공격 2.스킬 3.아이템 사용");
        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.KeyChar == '1')
            {
                Monster attackTarget = PlayerAttackTargeting(turnOwner);
                AttackTurn(turnOwner, attackTarget);
                break;
            }
            else if (keyInfo.KeyChar == '2')
            {
                UseSkill(turnOwner);
                break;
            }
            else if (keyInfo.KeyChar == '3')
            {
                IItem item = SelectItemToUse();
                if (item is null)
                {
                    Console.WriteLine("사용 가능한 아이템이 없습니다.");
                    Console.WriteLine("다시 행동을 선택하세요(1.공격 2.스킬)");
                    continue;
                }

                Character itemTarget = PlayerItemTargeting();
                ItemTurn(item, itemTarget);
                break;
            }
        }
    }

    private Monster PlayerAttackTargeting(PlayerCharacter turnOwner)
    {
        Console.WriteLine("\n공격 대상 선택");
        int minCursorTop = Console.CursorTop;
        List<Monster> targets = new List<Monster>();
        foreach (Monster monster in monsters)
        {
            if (monster.isDead)
            {
                continue;
            }

            targets.Add(monster);
            Console.WriteLine($"- {monster.name}");
        }

        int selectedTargetIndex = 0;
        Monster attackTarget = null;
        while (true)
        {
            Console.SetCursorPosition(0, minCursorTop + selectedTargetIndex);
            Console.Write("*");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                attackTarget = targets[selectedTargetIndex];
                break;
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow && 0 < selectedTargetIndex)
            {
                Console.SetCursorPosition(0, minCursorTop + selectedTargetIndex);
                Console.Write("-");
                selectedTargetIndex--;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow && selectedTargetIndex < targets.Count - 1)
            {
                Console.SetCursorPosition(0, minCursorTop + selectedTargetIndex);
                Console.Write("-");
                selectedTargetIndex++;
            }
        }
        Console.SetCursorPosition(0, minCursorTop + targets.Count);
        return attackTarget;
    }

    private void AttackTurn(Character turnOwner, Character attackTarget)
    {
        if (attackTarget.isAvoid)
        {
            Console.WriteLine($"{attackTarget.name}이(가) {turnOwner.name}의 공격을 회피했습니다.");
        }
        else
        {
            AttackOrderInfo attackOrder = turnOwner.CreateAttackOrder(attackTarget);
            DefenseOrderInfo defenseOrder = attackTarget.CreateDefenseOrder(attackOrder);
            attackTarget.health -= defenseOrder.actualDamage;
            Console.WriteLine($"{turnOwner.name}이(가) {attackTarget.name}을(를) 공격했습니다.");
            Thread.Sleep(1000);
            if (attackOrder.attackType == AttackType.CRITICAL)
            {
                Console.WriteLine("치명적인 공격!");
                Thread.Sleep(1000);
            }
            else if (attackOrder.attackType == AttackType.WEAK)
            {
                Console.WriteLine($"{turnOwner.name}의 실수로 공격이 약해집니다.");
                Thread.Sleep(1000);
            }

            Console.WriteLine($"{turnOwner.name}이(가) {attackOrder.damage}의 데미지를 주었습니다.");
            Thread.Sleep(1000);
            Console.WriteLine($"{attackTarget.name}이(가) {defenseOrder.defenseAmount}만큼 방어해 {defenseOrder.actualDamage}의 데미지를 받았습니다.");
            Console.WriteLine();
        }
    }

    private void UseSkill(PlayerCharacter turnOwner)
    {
        Skill selectedSkill = SelectSkill(turnOwner);

        if (selectedSkill == null)
        {
            Console.WriteLine($"사용할 스킬이 없어 턴을 넘깁니다.");
            return;
        }

        List<Character> allCharacters = GetAllCharacters();

        List<Character> skillTargets = selectedSkill.SelectTargetsFrom(allCharacters);

        if (skillTargets != null && skillTargets.Count > 0)
        {
            Console.WriteLine();

            selectedSkill.OnUse(skillTargets);
        }
    }
    private Skill SelectSkill(PlayerCharacter turnOwner)
    {
        Console.WriteLine("\n사용할 스킬을 선택하세요:");
        int minCursorTop = Console.CursorTop;

        List<Skill> skills = turnOwner.skills;
        int selectedSkillIndex = 0;
        Skill selectedSkill = null;

        if (skills.Count == 0)
        {
            return null;
        }

        foreach (Skill skill in skills)
        {
            Console.WriteLine($"- {skill.skillName}: {skill.skillDescription}");
        }

        while (true)
        {
            Console.SetCursorPosition(0, minCursorTop + selectedSkillIndex);
            Console.Write("*");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                selectedSkill = skills[selectedSkillIndex];
                break;
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow && selectedSkillIndex > 0)
            {
                Console.SetCursorPosition(0, minCursorTop + selectedSkillIndex);
                Console.Write("-");
                selectedSkillIndex--;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow && selectedSkillIndex < skills.Count - 1)
            {
                Console.SetCursorPosition(0, minCursorTop + selectedSkillIndex);
                Console.Write("-");
                selectedSkillIndex++;
            }


            for (int i = 0; i < skills.Count; i++)
            {
                if (i == selectedSkillIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.SetCursorPosition(0, minCursorTop + i);
                    Console.Write($"* {skills[i].skillName}: {skills[i].skillDescription}");
                    Console.ResetColor();
                }
                else
                {
                    Console.SetCursorPosition(0, minCursorTop + i);
                    Console.Write($"- {skills[i].skillName}: {skills[i].skillDescription}");
                }
            }
        }

        Console.SetCursorPosition(0, minCursorTop + skills.Count);
        Console.WriteLine();

        return selectedSkill;
    }

    private IItem SelectItemToUse()
    {
        PlayerState playerState = GameManager.instance.playerState;
        List<IItem> itemListToUse = new List<IItem>();

        Console.WriteLine("[사용 가능한 아이템]");
        Console.WriteLine("이름 \t| 설명");

        int minCursorTop = Console.CursorTop;
        foreach (IItem item in playerState.inventory)
        {
            if (item is IConsumableItem)
            {
                Console.WriteLine($"- {item.name} \t| {item.description}");
                itemListToUse.Add(item);
            }
        }

        if (itemListToUse.Count == 0)
        {
            return null;
        }


        int selectedItemIndex = 0;
        IItem itemToUse;
        while (true)
        {
            Console.SetCursorPosition(0, minCursorTop + selectedItemIndex);
            Console.Write("*");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                itemToUse = itemListToUse[selectedItemIndex];
                break;
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow && 0 < selectedItemIndex)
            {
                Console.SetCursorPosition(0, minCursorTop + selectedItemIndex);
                Console.Write("-");
                selectedItemIndex--;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow && selectedItemIndex < itemListToUse.Count - 1)
            {
                Console.SetCursorPosition(0, minCursorTop + selectedItemIndex);
                Console.Write("-");
                selectedItemIndex++;
            }
        }
        Console.SetCursorPosition(0, minCursorTop + itemListToUse.Count);

        return itemToUse;
    }

    private Character PlayerItemTargeting()
    {
        Console.WriteLine("\n아이템 대상 선택");
        int minCursorTop = Console.CursorTop;
        List<Character> characters = GetAllCharacters();
        List<Character> targets = new List<Character>();
        foreach (Character character in characters)
        {
            if (character.isDead)
            {
                continue;
            }

            targets.Add(character);
            Console.WriteLine($"- {character.name}");
        }

        int selectedTargetIndex = 0;
        Character itemTarget = null;
        while (true)
        {
            Console.SetCursorPosition(0, minCursorTop + selectedTargetIndex);
            Console.Write("*");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                itemTarget = targets[selectedTargetIndex];
                break;
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow && 0 < selectedTargetIndex)
            {
                Console.SetCursorPosition(0, minCursorTop + selectedTargetIndex);
                Console.Write("-");
                selectedTargetIndex--;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow && selectedTargetIndex < targets.Count - 1)
            {
                Console.SetCursorPosition(0, minCursorTop + selectedTargetIndex);
                Console.Write("-");
                selectedTargetIndex++;
            }
        }
        Console.SetCursorPosition(0, minCursorTop + targets.Count);
        return itemTarget;
    }

    private void ItemTurn(IItem item, Character itemTarget)
    {
        PlayerState playerState = GameManager.instance.playerState;

        Console.WriteLine($"{itemTarget.name}에게 {item.name}을(를) 사용했습니다.");
        ((IConsumableItem)item).OnUsed(itemTarget);
        playerState.inventory.Remove(item);
    }

    private void DoMonsterTurn(Monster turnOwner)
    {
        Console.Write($"{turnOwner.name}의 행동 선택 : ?");
        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        Thread.Sleep(1000);

        List<Character> targetPool = new List<Character>();
        targetPool.AddRange(playerCharacters);
        targetPool.AddRange(monsters);

        AiAction aiAction;
        List<Character> selectedTargets;
        while (true)
        {
            aiAction = turnOwner.monsterAiComponent.NextAiAction();
            selectedTargets = aiAction.SelectTargetsFrom(targetPool);
            if (0 < selectedTargets.Count)
            {
                break;
            }
        }

        Console.WriteLine($"{aiAction.actionName}");
        Thread.Sleep(1000);
        if (aiAction is AiAction_AttackOne || aiAction is AiAction_AttackThree)
        {
            foreach (Character target in selectedTargets)
            {
                AttackTurn(turnOwner, target);
            }
        }
        else if (aiAction is AiAction_HealOne || aiAction is AiAction_HealAll)
        {
            foreach (Character target in selectedTargets)
            {
                HealTurn(turnOwner, target);
            }
        }
    }

    private void HealTurn(Character turnOwner, Character healTarget)
    {
        HealOrderInfo healOrderInfo = turnOwner.CreateHealOrder(healTarget);
        healTarget.health += healOrderInfo.healAmount;
        if (healTarget.health > healTarget.stats.maxHealth)
        {
            healTarget.health = healTarget.stats.maxHealth;
        }
        Console.WriteLine($"{turnOwner.name}이(가) {healTarget.name}을(를) {healOrderInfo.healAmount} 만큼 회복시켰습니다.");
        Thread.Sleep(1000);
    }

    private bool IsAllPlayerCharacterDead()
    {
        foreach (PlayerCharacter playerCharacter in playerCharacters)
        {
            if (!playerCharacter.isDead)
            {
                return false;
            }
        }
        return true;
    }

    private bool IsAllMonsterDead()
    {
        foreach (Monster monster in monsters)
        {
            if (!monster.isDead)
            {
                return false;
            }
        }
        return true;
    }

    private void ShowWin()
    {
        //#Todo : Play Win Sound
        Console.WriteLine("승리!");
        Thread.Sleep(1000);

        int rewardExp = 0;
        int rewardGold = 0;
        List<IItem> rewardItems = new List<IItem>();
        foreach (Monster monster in monsters)
        {
            rewardExp += monster.rewardExp;
            rewardGold += monster.rewardGold;
            rewardItems.AddRange(monster.rewardItems);
        }

        foreach (PlayerCharacter playerCharacter in playerCharacters)
        {
            int actualRewardExp = new Random().Next((int)(rewardExp * 0.8), rewardExp + 1);
            playerCharacter.AddExp(rewardExp);
            Console.WriteLine($"{playerCharacter.name}은(는) {rewardExp}EXP 를 획득했습니다.");
            Thread.Sleep(500);
        }

        Console.WriteLine($"{rewardGold}G 를 획득했습니다.");
        Thread.Sleep(500);

        foreach (IItem rewardItem in rewardItems)
        {
            Console.WriteLine($"{rewardItem.name}을(를) 획득했습니다.");
            GameManager.instance.playerState.inventory.Add(rewardItem);
            Thread.Sleep(500);
        }

        Thread.Sleep(1500);
    }

    private void ShowLose()
    {
        //#Todo : Play Lose sound
        Console.WriteLine("패배했습니다...");
        Thread.Sleep(1500);
    }
}