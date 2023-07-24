using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public enum SkillType
{
    Skill_0 = 0,  //30% ����
    Skill_1,      //����ź
    Skill_2,      //��ȣ��
    SkCount
}

public class GlobalValue 
{
    public static string g_Unique_ID = "";  // ������ ������ȣ

    public static string g_NickName = "";   //������ ����
    public static int g_BestScore = 0;      //��������
    public static int g_UserGold  = 0;      //���ӸӴ�
    public static int g_Exp = 0;            //����ġ Experience
    public static int g_Level = 0;          //����

    public static int[] g_SkillCount = new int[3];

    public static int g_BestBlock = 1;      //���� ���� �ǹ� ����(Block == Floor)
    public static int g_CurBlockNum = 1;    //���� �ǹ� ����

    public static void LoadGameData()
    {
        //PlayerPrefs.SetInt("UserGold", 999999);

        //g_NickName  = PlayerPrefs.GetString("NickName", "�÷��̾��̸�");
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
