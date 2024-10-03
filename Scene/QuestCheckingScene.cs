﻿using ConsoleApp1;
using System;

class QuestCheckingScene : IScene
{
    public IScene GetNextScene()
    {
        return new SpartaTownScene();
    }

    public void OnShow()
    {
        BoxMaker.Draw(30, 4);
        Console.SetCursorPosition(10, 1);
        Console.Write("퀘스트 목록");
        Console.SetCursorPosition(4, 2);
        Console.Write($"NPC와 대화하기 ({GameManager.instance.playerState.questChecker.getUnlockedCount()}/5)");
        Console.SetCursorPosition(0, 4);
        Console.WriteLine("[스페이스바]를 눌러 돌아가기");

        if(GameManager.instance.playerState.questChecker.getUnlockedCount() == 5)
        {
            Console.WriteLine("\n1번 퀘스트 조건이 충족되었습니다. 퀘스트를 완료하려면 1번을 누르세요");
        }
        
        while(true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if(keyInfo.Key == ConsoleKey.Spacebar)
            {
                break;
            }
            else if(keyInfo.KeyChar == '1')
            {
                Console.WriteLine("퀘스트가 완료되었습니다.");
                Console.WriteLine("보상으로 '하급 체력포션' 3개가 지급됩니다.");
                GameManager.instance.playerState.inventory.Add(new HealthPotion());
                GameManager.instance.playerState.inventory.Add(new HealthPotion());
                GameManager.instance.playerState.inventory.Add(new HealthPotion());
            }
        }
    }
}
