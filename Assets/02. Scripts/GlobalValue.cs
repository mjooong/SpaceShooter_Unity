using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public enum SkillType
{
    Skill_0 = 0,  //30% 힐링
    Skill_1,      //수류탄
    Skill_2,      //보호막
    SkCount
}

public class GlobalValue 
{
    public static string g_Unique_ID = "";  // 유저의 고유번호

    public static string g_NickName = "";   //유저의 별명
    public static int g_BestScore = 0;      //게임점수
    public static int g_UserGold  = 0;      //게임머니
    public static int g_Exp = 0;            //경험치 Experience
    public static int g_Level = 0;          //레벨

    public static int[] g_SkillCount = new int[3];

    public static int g_BestBlock = 1;      //최종 도달 건물 층수(Block == Floor)
    public static int g_CurBlockNum = 1;    //현재 건물 층수

    public static void LoadGameData()
    {
        //PlayerPrefs.SetInt("UserGold", 999999);

        //g_NickName  = PlayerPrefs.GetString("NickName", "플레이어이름");
        //g_BestScore = PlayerPrefs.GetInt("BestScore", 0);
        //g_UserGold = PlayerPrefs.GetInt("UserGold", 0);

        string a_MkKey = "";
        for(int ii = 0; ii < g_SkillCount.Length; ii++)
        {
            a_MkKey = "SkItem_" + ii.ToString();
            g_SkillCount[ii] = PlayerPrefs.GetInt(a_MkKey, 0);
            //g_SkillCount[ii] = 3;
        }//for(int ii = 0; ii < g_SkillCount.Length; ii++)

        //PlayerPrefs.SetInt("BestBlockNum", 100);
        //PlayerPrefs.SetInt("BlockNumber", 100);

        g_BestBlock = PlayerPrefs.GetInt("BestBlockNum", 1);
        g_CurBlockNum = PlayerPrefs.GetInt("BlockNumber", 1);

    }//public static void LoadGameData()
}
