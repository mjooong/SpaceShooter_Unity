using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleJSON;

public enum PacketType
{
    BestScore,      // �ְ�����
    UserGold,       // �������
    NickUpdate,     // �г��Ӱ���
    InfoUpdate,     // ������ ������ �� ���� ���� ����
    BlockUpdate,    // ������

    ClearSave,      // ������ ����� ���� �ʱ�ȭ
}


public class NetworkMgr : MonoBehaviour
{
    // --- ������ ������ ��Ŷ ó���� ť ���� ����
    bool isNetworkLock = false; // Network ��� ���� ���� ����
    List<PacketType> m_PacketBuff = new List<PacketType>();
    // �ܼ��� � ��Ŷ�� ���� �ʿ䰡 �ִٶ�� ���� PacketBuff <ť>
    // --- ������ ������ ��Ŷ ó���� ť ���� ����

    string BestScoreUrl = "";
    string MyGoldUrl = "";
    string InfoUpdateUrl = "";
    string m_SvStrJson = "";    //������ �����Ϸ��� �ϴ� JSON������ ����?

    // �̱��� ������ ���� �ν��Ͻ� ���� ����
    public static NetworkMgr Inst = null;

    void Awake()
    {
        Inst = this;   
    }
    // �̱��� ������ ���� �ν��Ͻ� ���� ����

    void Start()
    {
        BestScoreUrl = "http://minjong0712.dothome.co.kr/_WebProgram/UpdateBScore.php";
        MyGoldUrl = "http://minjong0712.dothome.co.kr/_WebProgram/UpdateMyGold.php";
        InfoUpdateUrl = "http://minjong0712.dothome.co.kr/_WebProgram/InfoUpdate.php";

    }//void Start()

    void Update()
    {
        if(isNetworkLock == false)  // ���� ��Ŷ ó�� ���� ���°� �ƴ϶��..
        {
            if(0 < m_PacketBuff.Count)  // ��� ��Ŷ�� �����Ѵٸ�..
            {
                Req_Network();
            }
            else    // ó���� ��Ŷ�� �ϳ��� ���ٸ�..
            {
                // �Ź� ó���� ��Ŷ�� �ϳ��� ���� ���� ����ó�� �ؾ� ���� Ȯ��
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

    void Exe_GameEnd()      // execute : �����ϴ�. 
    {// �Ź� ó���� ��Ŷ�� �ϳ��� �������� ����ó�� �ؾ����� �Ǵ��ϴ� �Լ�
        if (GameMgr.s_GameState == GameState.GameExit)
        {
            SceneManager.LoadScene("scLobby");
        }

    }//void Exe_GameEnd()

    IEnumerator UpadateScoreCo()    // ���� ���� �ڷ�ƾ
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_score", GlobalValue.g_BestScore);

        isNetworkLock = true;

        UnityWebRequest a_www = UnityWebRequest.Post(BestScoreUrl, form);
        yield return a_www.SendWebRequest();    // ������ �ö����� ���..

        if(a_www.error == null) // ������ ������..
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

    IEnumerator UpdateGoldCo()  // ��� ���� �ڷ�ƾ
    {
        if(GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_gold", GlobalValue.g_UserGold);

        isNetworkLock = true;

        UnityWebRequest a_www = UnityWebRequest.Post(MyGoldUrl, form);
        yield return a_www.SendWebRequest();    // ������ �ö����� ���..

        if(a_www.error == null) // ������ ���ٸ� ����
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

        //---- JSON ����� ... 
        JSONObject a_MkJSON = new JSONObject();
        JSONArray jArray = new JSONArray(); //�迭�� �ʿ��� ��
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
        {
            jArray.Add(GlobalValue.g_SkillCount[ii]);
        }
        a_MkJSON.Add("SkList", jArray); //�迭�� ����
        m_SvStrJson = a_MkJSON.ToString();
        //---- JSON ����� ...

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID,
                                        System.Text.Encoding.UTF8);

        form.AddField("Info_data", m_SvStrJson, System.Text.Encoding.UTF8);

        isNetworkLock = true;

        UnityWebRequest a_www = UnityWebRequest.Post(InfoUpdateUrl, form);
        yield return a_www.SendWebRequest();    //������ �ö����� ����ϱ�...

        if (a_www.error == null)  //������ ���� �ʾ��� �� ����
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
            if (m_PacketBuff[ii] == a_PType)// ���� ó�� ���� ���� ��Ŷ�� �����ϸ�..
                a_isExist = true;
            // �� �߰����� �ʰ� ���� ������ ��Ŷ���� ������Ʈ�Ѵ�.
        }

        // ��� ���� �� Ÿ���� ��Ŷ�� ������ ���� �߰��Ѵ�.
        if (a_isExist == false)
            m_PacketBuff.Add(a_PType);

    }//public void PushPacket(PacketType a_PType)

}
