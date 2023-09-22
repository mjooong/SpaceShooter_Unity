using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleJSON;

public enum PacketType
{
    BestScore,      // 최고점수
    UserGold,       // 유저골드
    NickUpdate,     // 닉네임갱신
    InfoUpdate,     // 아이템 보유수 등 각종 정보 갱신
    BlockUpdate,    // 층갱신

    ClearSave,      // 서버에 저장된 내용 초기화
}


public class NetworkMgr : MonoBehaviour
{
    // --- 서버에 전송할 패킷 처리용 큐 관련 변수
    bool isNetworkLock = false; // Network 대기 상태 여부 변수
    List<PacketType> m_PacketBuff = new List<PacketType>();
    // 단순히 어떤 패킷을 보낼 필요가 있다라는 버퍼 PacketBuff <큐>
    // --- 서버에 전송할 패킷 처리용 큐 관련 변수

    string BestScoreUrl = "";
    string MyGoldUrl = "";
    string InfoUpdateUrl = "";
    string m_SvStrJson = "";    //서버에 전달하려고 하는 JSON형식이 뭔지?

    // 싱글턴 패턴을 위한 인스턴스 변수 선언
    public static NetworkMgr Inst = null;

    void Awake()
    {
        Inst = this;   
    }
    // 싱글턴 패턴을 위한 인스턴스 변수 선언

    void Start()
    {
        BestScoreUrl = "http://minjong0712.dothome.co.kr/_WebProgram/UpdateBScore.php";
        MyGoldUrl = "http://minjong0712.dothome.co.kr/_WebProgram/UpdateMyGold.php";
        InfoUpdateUrl = "http://minjong0712.dothome.co.kr/_WebProgram/InfoUpdate.php";

    }//void Start()

    void Update()
    {
        if(isNetworkLock == false)  // 지금 패킷 처리 중인 상태가 아니라면..
        {
            if(0 < m_PacketBuff.Count)  // 대기 패킷이 존재한다면..
            {
                Req_Network();
            }
            else    // 처리할 패킷이 하나도 없다면..
            {
                // 매번 처리할 패킷이 하나도 없을 때만 종료처리 해야 할지 확인
                Exe_GameEnd();
            }
        }

    }//void Update()

    void Req_Network()  // RequestNetwork
    {
        if (m_PacketBuff[0] == PacketType.BestScore)
            StartCoroutine(UpadateScoreCo());
        else if (m_PacketBuff[0] == PacketType.UserGold)
            StartCoroutine(UpdateGoldCo());
        else if (m_PacketBuff[0] == PacketType.InfoUpdate)
            StartCoroutine(UpdateInfoCo());

        m_PacketBuff.RemoveAt(0);

    }//void Req_Network()  // RequestNetwork

    void Exe_GameEnd()      // execute : 실행하다. 
    {// 매번 처리할 패킷이 하나도 없을때만 종료처리 해야할지 판단하는 함수
        if (GameMgr.s_GameState == GameState.GameExit)
        {
            SceneManager.LoadScene("scLobby");
        }

    }//void Exe_GameEnd()

    IEnumerator UpadateScoreCo()    // 점수 갱신 코루틴
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_score", GlobalValue.g_BestScore);

        isNetworkLock = true;

        UnityWebRequest a_www = UnityWebRequest.Post(BestScoreUrl, form);
        yield return a_www.SendWebRequest();    // 응답이 올때까지 대기..

        if(a_www.error == null) // 에러가 없으면..
        {
            //Debug.Log("UpdateSuccess");
        }
        else
        {
            Debug.Log(a_www.error);
        }

        a_www.Dispose();

        isNetworkLock = false;

    }//IEnumerator UpadateScoreCo

    IEnumerator UpdateGoldCo()  // 골드 갱신 코루틴
    {
        if(GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_gold", GlobalValue.g_UserGold);

        isNetworkLock = true;

        UnityWebRequest a_www = UnityWebRequest.Post(MyGoldUrl, form);
        yield return a_www.SendWebRequest();    // 응답이 올때까지 대기..

        if(a_www.error == null) // 에러가 없다면 동작
        {
            Debug.Log("UpdateGoldSucess");
        }
        else
        {
            Debug.Log(a_www.error);
        }

        a_www.Dispose();

        isNetworkLock = false;

        //yield return null;

    }//IEnumerator UpdateGoldCo()

    IEnumerator UpdateInfoCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        //---- JSON 만들기 ... 
        JSONObject a_MkJSON = new JSONObject();
        JSONArray jArray = new JSONArray(); //배열이 필요할 때
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
        {
            jArray.Add(GlobalValue.g_SkillCount[ii]);
        }
        a_MkJSON.Add("SkList", jArray); //배열을 넣음
        m_SvStrJson = a_MkJSON.ToString();
        //---- JSON 만들기 ...

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID,
                                        System.Text.Encoding.UTF8);

        form.AddField("Info_data", m_SvStrJson, System.Text.Encoding.UTF8);

        isNetworkLock = true;

        UnityWebRequest a_www = UnityWebRequest.Post(InfoUpdateUrl, form);
        yield return a_www.SendWebRequest();    //응답이 올때까지 대기하기...

        if (a_www.error == null)  //에러가 나지 않았을 때 동작
        {
            //Debug.Log("UpDateSuccess~");
        }
        else
        {
            Debug.Log(a_www.error);
        }

        a_www.Dispose();

        isNetworkLock = false;

    }//IEnumerator UpdateInfoCo()

    public void PushPacket(PacketType a_PType)
    {
        bool a_isExist = false;
        for (int ii = 0; ii < m_PacketBuff.Count; ii++)
        {
            if (m_PacketBuff[ii] == a_PType)// 아직 처리 되지 않은 패킷이 존재하면..
                a_isExist = true;
            // 또 추가하지 않고 기존 버퍼의 패킷으로 업데이트한다.
        }

        // 대기 중인 이 타입의 패킷이 없으면 새로 추가한다.
        if (a_isExist == false)
            m_PacketBuff.Add(a_PType);

    }//public void PushPacket(PacketType a_PType)

}
