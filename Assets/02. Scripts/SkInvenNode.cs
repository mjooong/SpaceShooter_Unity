using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkInvenNode : MonoBehaviour
{
    [HideInInspector] public SkillType m_SkType;
    [HideInInspector] public Text m_SkCountText;      //스킬 카운트 텍스트

    private void Awake()
    {
        m_SkCountText = GetComponentInChildren<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Button a_BtnCom = this.GetComponent<Button>();
        if (a_BtnCom != null)
            a_BtnCom.onClick.AddListener(() =>
            {
                if (GlobalValue.g_SkillCount[(int)m_SkType] <= 0)
                    return; //스킬 소진으로 사용할 수 없음

                PlayerCtrl a_Palyer = GameObject.FindObjectOfType<PlayerCtrl>();
                if (a_Palyer != null)
                    a_Palyer.UseSkill_Item(m_SkType);

                int a_SkCount = GlobalValue.g_SkillCount[(int)m_SkType];
                if (m_SkCountText != null)
                    m_SkCountText.text = a_SkCount.ToString();
            });
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
