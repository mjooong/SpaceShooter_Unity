using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMgr : MonoBehaviour
{
    public Text MyInfo_Text = null;
    public Text Ranking_Text = null;

    public Button m_Start_Btn;
    public Button m_Store_Btn;
    public Button m_LogOut_Btn;
    public Button m_Clear_Save_Btn;

    public Text MessageText;    //메시지 내용을 표기할 UI
    float ShowMsTimer = 0.0f;   //메시지를 몇 초동안 보이게 할 건지에 대한 타이머

    [HideInInspector] public int m_MyRank = 0;
    public Button RestRk_Btn;   //Restore Ranking Button
    //[HideInInspector] public float RestoreTimer = 0.0f;    //랭킹 갱신 타이머

    //--- 싱글턴 패턴
    public static LobbyMgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- 싱글턴 패턴

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;  //원래 속도로...
        GlobalValue.LoadGameData();

        if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);

        if (m_Store_Btn != null)
            m_Store_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("StoreScene");
            });

        if (m_LogOut_Btn != null)
            m_LogOut_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("TitleScene");
            });

        if (m_Clear_Save_Btn != null)
            m_Clear_Save_Btn.onClick.AddListener(Clear_Save_Click);

        RefreshMyInfo();

#if AutoRestore
        //-- 자동 랭킹 갱신인 경우
        if (RestRk_Btn != null)
            RestRk_Btn.gameObject.SetActive(false);
        //-- 자동 랭킹 갱신인 경우
#else
        //-- 수동 랭킹 갱신인 경우
        if (RestRk_Btn != null)
            RestRk_Btn.onClick.AddListener(RestoreRank);
        //-- 수동 랭킹 갱신인 경우
#endif

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if (0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if (ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);    //메시지 끄기
            }
        }
    }

    void StartBtnClick()
    {
        if (100 <= GlobalValue.g_CurBlockNum)
        {   //마지막 층에 도달한 상태에서 게임을 시작했다면...
            //바로 직전 층(99층)에서 시작하게 하기...
            GlobalValue.g_CurBlockNum = 99;
            PlayerPrefs.SetInt("BlockNumber", GlobalValue.g_CurBlockNum);
        }//if(100 <= GlobalValue.g_CurBlockNum)

        SceneManager.LoadScene("ScLevel01");
        SceneManager.LoadScene("ScPlay", LoadSceneMode.Additive);
    }

    void Clear_Save_Click()
    {
        PlayerPrefs.DeleteAll();
        GlobalValue.LoadGameData();
    }

    public void MessageOnOff(string Mess = "", bool isOn = true, float a_Time = 5.0f)
    {
        if (isOn == true)
        {
            MessageText.text = Mess;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = a_Time;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }
    }

    public void RefreshRankUI(List<UserInfo> a_RkList)
    {
        Ranking_Text.text = "";

        for (int i = 0; i < a_RkList.Count; i++)
        {
            if (a_RkList[i].m_Id == GlobalValue.g_Unique_ID)
                Ranking_Text.text += "<color=#00ff00>";

            Ranking_Text.text += (i + 1).ToString() + "등 : " +
                a_RkList[i].m_Id +
                " (" + a_RkList[i].m_Nick + ") : " +
                a_RkList[i].m_BestScore + "점" + "\n";

            //등수 안에 내가 있다면 색 표시
            if (a_RkList[i].m_Id == GlobalValue.g_Unique_ID)
                Ranking_Text.text += "</color>";
        }//for(int i = 0; i < a_RkList.Count; i++)

    }//public void RefreshRankUI(List<UserInfo> a_RkList)

    public void RefreshMyInfo()
    {
        MyInfo_Text.text = "내정보 : 별명(" + GlobalValue.g_NickName +
                        ") : 순위(" + m_MyRank + "등) : 점수(" +
                        GlobalValue.g_BestScore.ToString("N0") + "점) : 골드(" +
                        GlobalValue.g_UserGold.ToString("N0") + ")";
    }

    void RestoreRank()
    {
        if (0.0f < LobbyNetworkMgr.Inst.RestoreTimer)
        {
            MessageOnOff("최소 7초 주기로만 갱신됩니다.");
            return;
        }

        //딜레이 로딩 즉시 취고하고 수동로딩 우선으로...
        LobbyNetworkMgr.Inst.GetRankingList();

        LobbyNetworkMgr.Inst.RestoreTimer = 7.0f;
    }
}