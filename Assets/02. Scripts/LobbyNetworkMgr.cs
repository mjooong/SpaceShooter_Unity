using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class UserInfo
{
    public string m_Id = "";
    public string m_Nick = "";
    public int m_BestScore = 0;
}

public class LobbyNetworkMgr : MonoBehaviour
{
    //--- ������ ������ ��Ŷ ó���� ť ���� ����
    bool isNetworkLock = false;
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //�ܼ��� � ��Ŷ�� ���� �ʿ䰡 �ִٶ�� ���� PakgetBuffer <ť>
    //--- ������ ������ ��Ŷ ó���� ť ���� ����

    string GetRankListUrl = "";
    List<UserInfo> m_RkList = new List<UserInfo>();

    [HideInInspector] public float RestoreTimer = 0.0f;    //��ŷ ���� Ÿ�̸�
    //--- �̱��� ������ ���� �ν��Ͻ� ���� ����
    public static LobbyNetworkMgr Inst = null;

    void Awake()
    {
        Inst = this;     //LobbyNetworkMgr Ŭ���� �ν��Ͻ��� ����
    }
    //--- �̱��� ������ ���� �ν��Ͻ� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        GetRankListUrl = "http://minjong0712.dothome.co.kr/_WebProgram/Get_ID_Rank.php";

        RestoreTimer = 3.0f;  //��ŷ ���� Ÿ�̸�
        LobbyMgr.Inst.MessageOnOff("�ε���...", true, 100);
        GetRankingList(); //��ŷ �ҷ�����...
    }

    // Update is called once per frame
    void Update()
    {
#if AutoRestore
        //--- �ڵ� ��ŷ ������ ���
        RestoreTimer -= Time.deltaTime;
        if (RestoreTimer <= 0.0f)
        {
            GetRankingList();
            RestoreTimer = 7.0f;    //�ֱ�
        }
#else
        //--- ���� ��ŷ ������ ���
        if (0.0f < RestoreTimer)
            RestoreTimer -= Time.deltaTime;
        //--- ���� ��ŷ ������ ���
#endif

    }

    public void GetRankingList()  //���� �ҷ�����
    {
        StartCoroutine(GetRankListCo());
    }

    IEnumerator GetRankListCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;      //�α��� ���� ���¶�� �׳� ����

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID,
                                        System.Text.Encoding.UTF8);

        UnityWebRequest a_www = UnityWebRequest.Post(GetRankListUrl, form);
        yield return a_www.SendWebRequest(); //������ �ö����� ����ϱ�...

        if (a_www.error == null) //������ ���� �ʾ��� �� ����
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            //<--- �̷��� �ؾ� �ȵ���̵忡�� �ѵ��� �ȱ�����.
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            if (a_ReStr.Contains("Get_Rank_List_Success~") == true)
            {
                LobbyMgr.Inst.MessageOnOff("", false); //�޽��� ����
                //������ ǥ���ϴ� �Լ��� ȣ��
                RecRankList_MyRank(a_ReStr);
            }
            else
            {
                LobbyMgr.Inst.MessageOnOff("���� �ҷ����� ���� ��� �� �ٽ� �õ��� �ּ���.", true);
            }
        }
        else
        {
            LobbyMgr.Inst.MessageOnOff("���� �ҷ����� ���� ��� �� �ٽ� �õ��� �ּ���.", true);
            Debug.Log(a_www.error);
        }

        a_www.Dispose();

    }//IEnumerator GetRankListCo()

    void RecRankList_MyRank(string strJsonData)
    {
        if (strJsonData.Contains("RkList") == false)
            return;

        m_RkList.Clear();

        //JSON ���� �Ľ�
        var N = JSON.Parse(strJsonData);

        int ranking = 0;
        UserInfo a_UserND;
        for (int i = 0; i < N["RkList"].Count; i++)
        {
            ranking = i + 1;
            string userID = N["RkList"][i]["user_id"];
            string nick_name = N["RkList"][i]["nick_name"];
            int best_score = N["RkList"][i]["best_score"].AsInt;

            a_UserND = new UserInfo();
            a_UserND.m_Id = userID;
            a_UserND.m_Nick = nick_name;
            a_UserND.m_BestScore = best_score;
            m_RkList.Add(a_UserND);
        }//for(int i = 0; i < N["RkList"].Count; i++)

        LobbyMgr.Inst.RefreshRankUI(m_RkList);

        if (N["my_rank"] != null)
            LobbyMgr.Inst.m_MyRank = N["my_rank"].AsInt;

        LobbyMgr.Inst.RefreshMyInfo();

    }// void RecRankList_MyRank(string strJsonData)
}