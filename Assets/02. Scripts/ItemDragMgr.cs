using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ItemDragMgr : MonoBehaviour
{
    public SlotScript[] m_ProductSlots;
    public SlotScript[] m_InvenSlots;
    public Image m_MsObj = null;        //���콺�� ����ٴϴ� ������Ʈ ���� ����
    int m_SaveIndex = -1;   //-1 �� �ƴϸ� �������� ��ŷ ���¿��� �巡�� ���̶�� ��

    public Text m_BagsizeText;
    public Text m_HelpText;
    float m_HelpDuring = 2.5f;
    float m_HelpAddTimer = 0.0f;
    float m_CacTime = 0.0f;
    Color m_Color;

    //--- �������� ����� ���� ����
    int m_SvMyGold = 0;  //Backup
    int[] m_SvSkCount = new int[3];
    string m_SvStrJson = "";    //������ �����Ϸ��� �ϴ� JSON������ ����?

    bool isNetworkLock = false;
    string BuyRequestUrl = "";
    //--- �������� ����� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        for(int ii = 0; ii < m_InvenSlots.Length; ii++)
        {
            if(0 < GlobalValue.g_SkillCount[ii])
            {
                m_InvenSlots[ii].ItemCountTxt.text = GlobalValue.g_SkillCount[ii].ToString();
                m_InvenSlots[ii].ItemImg.sprite = m_ProductSlots[ii].ItemImg.sprite;
                m_InvenSlots[ii].ItemImg.gameObject.SetActive(true);
                m_InvenSlots[ii].m_CurItemIdx = ii;
            }
            else
            {
                m_InvenSlots[ii].ItemCountTxt.text = "0";
                m_InvenSlots[ii].ItemImg.gameObject.SetActive(false);
            }
        }//for(int ii = 0; ii < m_InvenSlots.Length; ii++)

        int a_CurBagSize = 0;
        for(int ii =  0; ii < GlobalValue.g_SkillCount.Length; ii++)
        {
            a_CurBagSize += GlobalValue.g_SkillCount[ii];
        }
        if (m_BagsizeText != null)
            m_BagsizeText.text = "��������� : " + a_CurBagSize + " / 10";

        BuyRequestUrl = "http://minjong0712.dothome.co.kr/_WebProgram/Buy_Request.php";
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) == true)
        {   //���� ���콺 ��ư Ŭ���ϴ� ����
            MouseBtnDown();
        }

        if(Input.GetMouseButton(0) == true)
        {   //���� ���콺 ��ư�� ������ �ִ� ����
            MouseBtnPress();
        }

        if(Input.GetMouseButtonUp(0) == true)
        {   //���� ���콺 ��ư�� �����ٰ� ���� ����
            MouseBtnUp();
        }

        //--- HelpText ������ ������� ó���ϴ� ����
        if(0.0f < m_HelpAddTimer)
        {
            m_HelpAddTimer -= Time.deltaTime;
            m_CacTime = m_HelpAddTimer / (m_HelpDuring - 2.0f);
            if (1.0f < m_CacTime)
                m_CacTime = 1.0f;
            m_Color = m_HelpText.color;
            m_Color.a = m_CacTime;
            m_HelpText.color = m_Color;

            if (m_HelpAddTimer <= 0.0f)
                m_HelpText.gameObject.SetActive(false);
        }
        //--- HelpText ������ ������� ó���ϴ� ����

    }

    void MouseBtnDown() //���콺 ���� ��ư�� ������ ���� ó�� ���� �Լ�
    {
        m_SaveIndex = -1;
        for(int ii = 0; ii < m_ProductSlots.Length; ii++)
        {
            if(m_ProductSlots[ii].ItemImg.gameObject.activeSelf == true &&
                IsCollSlot(m_ProductSlots[ii]) == true)
            {
                m_SaveIndex = ii;
                Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
                if (a_ChildImg != null)
                    a_ChildImg.GetComponent<Image>().sprite =
                                            m_ProductSlots[ii].ItemImg.sprite;
                m_MsObj.gameObject.SetActive(true);
                break;
            }
        }
    }//void MouseBtnDown()

    void MouseBtnPress() //���콺 ���� ��ư�� �����ִ� ���� ó�� ���� �Լ�
    {
        if (0 <= m_SaveIndex)
            m_MsObj.transform.position = Input.mousePosition;
    }

    void MouseBtnUp() //���콺 ���� ��ư�� �����ٰ� ���� ���� ó�� ���� �Լ�
    {
        if (m_SaveIndex < 0 || m_ProductSlots.Length <= m_SaveIndex)
            return;

        Sprite a_MsIconImg = null;
        Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
        if (a_ChildImg != null)
            a_MsIconImg = a_ChildImg.GetComponent<Image>().sprite;

        int a_BuyIndex = -1;
        for(int ii = 0; ii < m_InvenSlots.Length; ii++)
        {
            if(IsCollSlot(m_InvenSlots[ii]) == true)
            {
                if(m_SaveIndex == ii)
                {
                    if(BuySkItem(m_SaveIndex) == true) //���� �õ� �Լ� ȣ��
                    {
                        a_BuyIndex = ii;
                        break;
                    }//if(BuySkItem(m_SaveIndex) == true)
                }
                else
                    ShowMessage("�ش� ���Կ��� �������� ������ �� �����ϴ�.");
            }//if(IsCollSlot(m_InvenSlots[ii]) == true)
        }//for(int ii = 0; ii < m_InvenSlots.Length; ii++)

        if (0 <= a_BuyIndex)
        {
            m_InvenSlots[a_BuyIndex].ItemImg.sprite = a_MsIconImg;
            m_InvenSlots[a_BuyIndex].ItemImg.gameObject.SetActive(true);
            m_InvenSlots[a_BuyIndex].m_CurItemIdx = m_SaveIndex;

            StartCoroutine(BuyRequestCo());
        }

        m_SaveIndex = -1;
        m_MsObj.gameObject.SetActive(false);


    }//void MouseBtnUp()

    bool IsCollSlot(SlotScript a_CkObj)
    {  //���콺�� UI �������� �ִ���? �Ǵ��ϴ� �Լ�

        if (a_CkObj == null)
            return false;

        Vector3[] v = new Vector3[4];
        a_CkObj.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : �����ϴ�  v[1] : �������  v[2] : �������  v[3] : �����ϴ�
        //v[0] �����ϴ��� 0, 0 ��ǥ�� ���콺 ��ǥ��
        //RectTranform : �� UGUI ��ǥ ����

        if(v[0].x <= Input.mousePosition.x && Input.mousePosition.x <= v[2].x &&
           v[0].y <= Input.mousePosition.y && Input.mousePosition.y <= v[2].y)
        {
            return true;
        }

        return false;
    }

    bool BuySkItem(int a_SkIdx) //���� �õ� �Լ�
    {
        int a_Cost = 300;
        if (a_SkIdx == 1)
            a_Cost = 500;
        else if (a_SkIdx == 2)
            a_Cost = 1000;

        if(GlobalValue.g_UserGold < a_Cost)
        {
            ShowMessage("��尡 �����մϴ�.");
            return false;
        }

        int a_CurBagSize = 0;
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
            a_CurBagSize += GlobalValue.g_SkillCount[ii];

        if (10 <= a_CurBagSize)
        {
            ShowMessage("������ ���� á���ϴ�.");
            return false;
        }

        // ���� ���� ������ �巡�� �� �帨 �� Ȯ�� ���̾˷α׸� ���� ������ ���� ��
        // �������� ���� Ȯ��, ���� �� Ŭ���̾�Ʈ�� ������ �ְ� UI�� ������ �ִ�
        // �������� �����ؾ� �Ѵ�.

        //--- Backup �޾� ���� (���н� ���� ���� ���� �뵵)
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
            m_SvSkCount[ii] = GlobalValue.g_SkillCount[ii];
        m_SvMyGold = GlobalValue.g_UserGold;
        //--- Backup �޾� ���� (���н� ���� ���� ���� �뵵)

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        //--- ���� ���� ���ÿ� ����
        //string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        //PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        //--- ���� ���� ���ÿ� ����

        //--- UI ����
        m_InvenSlots[a_SkIdx].ItemCountTxt.text = GlobalValue.g_SkillCount[a_SkIdx].ToString();

        Store_Mgr a_StMgr = null;
        GameObject a_StObj = GameObject.Find("Store_Mgr");
        if (a_StObj != null)
            a_StMgr = a_StObj.GetComponent<Store_Mgr>();
        if (a_StMgr != null && a_StMgr.m_UserInfoText != null)
            a_StMgr.m_UserInfoText.text = "����(" + GlobalValue.g_NickName + ") : �������(" +
                                        GlobalValue.g_UserGold + ")";
        a_CurBagSize = 0;
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
            a_CurBagSize += GlobalValue.g_SkillCount[ii];
        m_BagsizeText.text = "��������� : " + a_CurBagSize + " / 10";
        //--- UI ����

        return true;

    }//bool BuySkItem(int a_SkIdx)

    void ShowMessage(string a_Mess)
    {
        if (m_HelpText == null)
            return;

        m_HelpText.text = a_Mess;
        //m_HelpText.color = Color.blue;
        m_HelpText.gameObject.SetActive(true);
        m_HelpAddTimer = m_HelpDuring;
    }

    //������ ������ �� �����ϱ�...
    IEnumerator BuyRequestCo()
    {
        //--- JSON �����...
        JSONObject a_MkJSON = new JSONObject();
        JSONArray jArray = new JSONArray(); //�迭�� �ʿ��� ��
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
        {
            jArray.Add(GlobalValue.g_SkillCount[ii]);
        }
        a_MkJSON.Add("SkList", jArray); //�迭�� ����
        m_SvStrJson = a_MkJSON.ToString();
        //--- JSON �����...

        if (string.IsNullOrEmpty(m_SvStrJson) == true)
        {
            RecoverItem();
            yield break;        //���� ���� ���¶�� �׳� ����
        }

        if (string.IsNullOrEmpty(GlobalValue.g_Unique_ID) == true)
        {
            RecoverItem();
            yield break;        //�α��� ���°� �ƴϸ� �׳� ����
        }

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID,
                                        System.Text.Encoding.UTF8);
        form.AddField("My_Gold", GlobalValue.g_UserGold);
        form.AddField("Item_list", m_SvStrJson, System.Text.Encoding.UTF8);

        isNetworkLock = true;

        UnityWebRequest a_www = UnityWebRequest.Post(BuyRequestUrl, form);
        yield return a_www.SendWebRequest();    //������ �ö����� ����ϱ�...

        if (a_www.error == null) //������ ���� �ʾ��� �� ����
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);
            //����Ϸᰡ �Ǹ� ��ü ����(��ü ���� �޾Ƽ� �����ϴ� ����� �ְ�,
            //m_SvMyPoint, m_BuyCrType �� ������ �����ϴ� ����� �ִ�.)
            //if (a_ReStr.Contains("BuySuccess~") == true)
            //   //RefreshMyInfoCo();
            //else
            if (a_ReStr.Contains("BuySuccess~") == false) //���� ���� ��
            {
                RecoverItem();
                Debug.Log(a_ReStr);
            }
        }
        else //���� ���� ��
        {
            RecoverItem();
            Debug.Log(a_www.error);
        }

        a_www.Dispose();

        isNetworkLock = false;
    }

    void RecoverItem()  //���� ���·� ����
    {
        GlobalValue.g_UserGold = m_SvMyGold;

        //--- UI ����
        for (int ii = 0; ii < m_InvenSlots.Length; ii++)
        {
            GlobalValue.g_SkillCount[ii] = m_SvSkCount[ii];

            if (0 < GlobalValue.g_SkillCount[ii])
            {
                m_InvenSlots[ii].ItemCountTxt.text = GlobalValue.g_SkillCount[ii].ToString();
                m_InvenSlots[ii].ItemImg.sprite = m_ProductSlots[ii].ItemImg.sprite;
                m_InvenSlots[ii].ItemImg.gameObject.SetActive(true);
                m_InvenSlots[ii].m_CurItemIdx = ii;
            }
            else
            {
                m_InvenSlots[ii].ItemCountTxt.text = "0";
                m_InvenSlots[ii].ItemImg.gameObject.SetActive(false);
            }
        }//for (int ii = 0; ii < m_InvenSlots.Length; ii++)

        Store_Mgr a_StMgr = null;
        GameObject a_StObj = GameObject.Find("Store_Mgr");
        if (a_StObj != null)
            a_StMgr = a_StObj.GetComponent<Store_Mgr>();
        if (a_StMgr != null && a_StMgr.m_UserInfoText != null)
            a_StMgr.m_UserInfoText.text = "����(" + GlobalValue.g_NickName + ") : �������(" +
                                        GlobalValue.g_UserGold + ")";

        int a_CurBagSize = 0;
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
            a_CurBagSize += GlobalValue.g_SkillCount[ii];
        m_BagsizeText.text = "��������� : " + a_CurBagSize + " / 10";
        //--- UI ����
    }
}
