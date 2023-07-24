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

    public Text MessageText;    //�޽��� ������ ǥ���� UI
    float ShowMsTimer = 0.0f;   //�޽����� �� �ʵ��� ���̰� �� ������ ���� Ÿ�̸�

    [HideInInspector] public int m_MyRank = 0;
    public Button RestRk_Btn;   //Restore Ranking Button
    //[HideInInspector] public float RestoreTimer = 0.0f;    //��ŷ ���� Ÿ�̸�

    //--- �̱��� ����
    public static LobbyMgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- �̱��� ����

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;  //���� �ӵ���...
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
        //-- �ڵ� ��ŷ ������ ���
        if (RestRk_Btn != null)
            RestRk_Btn.gameObject.SetActive(false);
        //-- �ڵ� ��ŷ ������ ���
#else
        //-- ���� ��ŷ ������ ���
        if (RestRk_Btn != null)
            RestRk_Btn.onClick.AddListener(RestoreRank);
        //-- ���� ��ŷ ������ ���
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
                MessageOnOff("", false);    //�޽��� ����
            }
        }
    }

    void StartBtnClick()
    {
        if (100 <= GlobalValue.g_CurBlockNum)
        {   //������ ���� ������ ���¿��� ������ �����ߴٸ�...
            //�ٷ� ���� ��(99��)���� �����ϰ� �ϱ�...
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

            Ranking_Text.text += (i + 1).ToString() + "�� : " +
                a_RkList[i].m_Id +
                " (" + a_RkList[i].m_Nick + ") : " +
                a_RkList[i].m_BestScore + "��" + "\n";

            //��� �ȿ� ���� �ִٸ� �� ǥ��
            if (a_RkList[i].m_Id == GlobalValue.g_Unique_ID)
                Ranking_Text.text += "</color>";
        }//for(int i = 0; i < a_RkList.Count; i++)

    }//public void RefreshRankUI(List<UserInfo> a_RkList)

    public void RefreshMyInfo()
    {
        MyInfo_Text.text = "������ : ����(" + GlobalValue.g_NickName +
                        ") : ����(" + m_MyRank + "��) : ����(" +
                        GlobalValue.g_BestScore.ToString("N0") + "��) : ���(" +
                        GlobalValue.g_UserGold.ToString("N0") + ")";
    }

    void RestoreRank()
    {
        if (0.0f < LobbyNetworkMgr.Inst.RestoreTimer)
        {
            MessageOnOff("�ּ� 7�� �ֱ�θ� ���ŵ˴ϴ�.");
            return;
        }

        //������ �ε� ��� ����ϰ� �����ε� �켱����...
        LobbyNetworkMgr.Inst.GetRankingList();

        LobbyNetworkMgr.Inst.RestoreTimer = 7.0f;
    }
}