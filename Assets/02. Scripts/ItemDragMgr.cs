using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDragMgr : MonoBehaviour
{
    public SlotScript[] m_ProductSlots;
    public SlotScript[] m_InvenSlots;
    public Image m_MsObj = null;        //마우스를 따라다니는 오브젝트 참조 변수
    int m_SaveIndex = -1;   //-1 이 아니면 아이템을 피킹 상태에서 드래그 중이라는 뜻

    public Text m_BagsizeText;
    public Text m_HelpText;
    float m_HelpDuring = 2.5f;
    float m_HelpAddTimer = 0.0f;
    float m_CacTime = 0.0f;
    Color m_Color;

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
            m_BagsizeText.text = "가방사이즈 : " + a_CurBagSize + " / 10";
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) == true)
        {   //왼쪽 마우스 버튼 클릭하는 순간
            MouseBtnDown();
        }

        if(Input.GetMouseButton(0) == true)
        {   //왼쪽 마우스 버튼을 누르고 있는 동안
            MouseBtnPress();
        }

        if(Input.GetMouseButtonUp(0) == true)
        {   //왼쪽 마우스 버튼을 누르다가 떼는 순간
            MouseBtnUp();
        }

        //--- HelpText 서서히 사라지게 처리하는 연출
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
        //--- HelpText 서서히 사라지게 처리하는 연출

    }

    void MouseBtnDown() //마우스 왼쪽 버튼을 누르는 순간 처리 위한 함수
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

    void MouseBtnPress() //마우스 왼쪽 버튼을 누르있는 동안 처리 위한 함수
    {
        if (0 <= m_SaveIndex)
            m_MsObj.transform.position = Input.mousePosition;
    }

    void MouseBtnUp() //마우스 왼쪽 버튼을 누르다가 떼는 순간 처리 위한 함수
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
                    if(BuySkItem(m_SaveIndex) == true) //구매 시도 함수 호출
                    {
                        a_BuyIndex = ii;
                        break;
                    }//if(BuySkItem(m_SaveIndex) == true)
                }
                else
                    ShowMessage("해당 슬롯에는 아이템을 장착할 수 없습니다.");
            }//if(IsCollSlot(m_InvenSlots[ii]) == true)
        }//for(int ii = 0; ii < m_InvenSlots.Length; ii++)

        if (0 <= a_BuyIndex)
        {
            m_InvenSlots[a_BuyIndex].ItemImg.sprite = a_MsIconImg;
            m_InvenSlots[a_BuyIndex].ItemImg.gameObject.SetActive(true);
            m_InvenSlots[a_BuyIndex].m_CurItemIdx = m_SaveIndex;
        }

        m_SaveIndex = -1;
        m_MsObj.gameObject.SetActive(false);

    }//void MouseBtnUp()

    bool IsCollSlot(SlotScript a_CkObj)
    {  //마우스가 UI 슬롯위에 있는지? 판단하는 함수

        if (a_CkObj == null)
            return false;

        Vector3[] v = new Vector3[4];
        a_CkObj.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : 좌측하단  v[1] : 좌측상단  v[2] : 우측상단  v[3] : 우측하단
        //v[0] 좌측하단이 0, 0 좌표인 마우스 좌표계
        //RectTranform : 즉 UGUI 좌표 기준

        if(v[0].x <= Input.mousePosition.x && Input.mousePosition.x <= v[2].x &&
           v[0].y <= Input.mousePosition.y && Input.mousePosition.y <= v[2].y)
        {
            return true;
        }

        return false;
    }

    bool BuySkItem(int a_SkIdx) //구매 시도 함수
    {
        int a_Cost = 300;
        if (a_SkIdx == 1)
            a_Cost = 500;
        else if (a_SkIdx == 2)
            a_Cost = 1000;

        if(GlobalValue.g_UserGold < a_Cost)
        {
            ShowMessage("골드가 부족합니다.");
            return false;
        }

        int a_CurBagSize = 0;
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
            a_CurBagSize += GlobalValue.g_SkillCount[ii];

        if (10 <= a_CurBagSize)
        {
            ShowMessage("가방이 가득 찼습니다.");
            return false;
        }

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        //--- 변동 사항 로컬에 저장
        string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        //--- 변동 사항 로컬에 저장

        //--- UI 갱신
        m_InvenSlots[a_SkIdx].ItemCountTxt.text = GlobalValue.g_SkillCount[a_SkIdx].ToString();

        Store_Mgr a_StMgr = null;
        GameObject a_StObj = GameObject.Find("Store_Mgr");
        if (a_StObj != null)
            a_StMgr = a_StObj.GetComponent<Store_Mgr>();
        if (a_StMgr != null && a_StMgr.m_UserInfoText != null)
            a_StMgr.m_UserInfoText.text = "별명(" + GlobalValue.g_NickName + ") : 보유골드(" +
                                        GlobalValue.g_UserGold + ")";
        a_CurBagSize = 0;
        for (int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
            a_CurBagSize += GlobalValue.g_SkillCount[ii];
        m_BagsizeText.text = "가방사이즈 : " + a_CurBagSize + " / 10";
        //--- UI 갱신

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
}
