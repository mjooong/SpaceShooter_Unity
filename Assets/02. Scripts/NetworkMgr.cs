using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

        m_PacketBuff.RemoveAt(0);

    }//void Req_Network()  // RequestNetwork

    void Exe_GameEnd()      // execute : �����ϴ�. 
    {// �Ź� ó���� ��Ŷ�� �ϳ��� �������� ����ó�� �ؾ����� �Ǵ��ϴ� �Լ�


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
