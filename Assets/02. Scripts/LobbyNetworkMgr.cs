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
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수
    bool isNetworkLock = false;
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //단순히 어떤 패킷을 보낼 필요가 있다라는 버퍼 PakgetBuffer <큐>
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수

    string GetRankListUrl = "";
    List<UserInfo> m_RkList = new List<UserInfo>();

    [HideInInspector] public float RestoreTimer = 0.0f;    //랭킹 갱신 타이머
    //--- 싱글턴 패턴을 위한 인스턴스 변수 선언
    public static LobbyNetworkMgr Inst = null;

    void Awake()
    {
        Inst = this;     //LobbyNetworkMgr 클래스 인스턴스를 대입
    }
    //--- 싱글턴 패턴을 위한 인스턴스 변수 선언

    // Start is called before the first frame update
    void Start()
    {
        GetRankListUrl = "http://minjong0712.dothome.co.kr/_WebProgram/Get_ID_Rank.php";

        RestoreTimer = 3.0f;  //랭킹 갱신 타이머
        LobbyMgr.Inst.MessageOnOff("로딩중...", true, 100);
        GetRankingList(); //랭킹 불러오기...
    }

    // Update is called once per frame
    void Update()
    {
#if AutoRestore
        //--- 자동 랭킹 갱신인 경우
        RestoreTimer -= Time.deltaTime;
        if (RestoreTimer <= 0.0f)
        {
            GetRankingList();
            RestoreTimer = 7.0f;    //주기
        }
#else
        //--- 수동 랭킹 갱신인 경우
        if (0.0f < RestoreTimer)
            RestoreTimer -= Time.deltaTime;
        //--- 수동 랭킹 갱신인 경우
#endif

    }

    public void GetRankingList()  //순위 불러오기
    {
        StartCoroutine(GetRankListCo());
    }

    IEnumerator GetRankListCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;      //로그인 실패 상태라면 그냥 리턴

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID,
                                        System.Text.Encoding.UTF8);

        UnityWebRequest a_www = UnityWebRequest.Post(GetRankListUrl, form);
        yield return a_www.SendWebRequest(); //응답이 올때까지 대기하기...

        if (a_www.error == null) //에러가 나지 않았을 때 동장
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            //<--- 이렇게 해야 안드로이드에서 한들이 안깨진다.
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            if (a_ReStr.Contains("Get_Rank_List_Success~") == true)
            {
                LobbyMgr.Inst.MessageOnOff("", false); //메시지 끄기
                //점수를 표시하는 함수를 호출
                RecRankList_MyRank(a_ReStr);
            }
            else
            {
                LobbyMgr.Inst.MessageOnOff("순위 불러오기 실패 잠시 후 다시 시도해 주세요.", true);
            }
        }
        else
        {
            LobbyMgr.Inst.MessageOnOff("순위 불러오기 실패 잠시 후 다시 시도해 주세요.", true);
            Debug.Log(a_www.error);
        }

        a_www.Dispose();

    }//IEnumerator GetRankListCo()

    void RecRankList_MyRank(string strJsonData)
    {
        if (strJsonData.Contains("RkList") == false)
            return;

        m_RkList.Clear();

        //JSON 파일 파싱
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