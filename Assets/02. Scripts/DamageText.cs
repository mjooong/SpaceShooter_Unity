using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    [HideInInspector] public Text m_RefText = null;
    [HideInInspector] public float m_DamageVal = 0.0f;
    [HideInInspector] public Vector3 m_BaseWdPos = Vector3.zero;

    Animator m_RefAnimator = null;   

    // Start is called before the first frame update
    void Start()
    {
        m_RefText = this.gameObject.GetComponentInChildren<Text>();
        if(m_RefText != null)
        {
            if (m_DamageVal < 0)
                m_RefText.text = "-" + m_DamageVal.ToString() + " Dmg";
            else //if(0 <= a_DamageVal)
                m_RefText.text = "+" + m_DamageVal.ToString() + " Dmg";
        }

        m_RefAnimator = GetComponentInChildren<Animator>();
        if(m_RefAnimator != null)
        {
            AnimatorStateInfo animatorStateInfo =
                       m_RefAnimator.GetCurrentAnimatorStateInfo(0); //첫번째 레이어

            float a_LifeTime = animatorStateInfo.length; //애니메이션 플레이 시간
            Destroy(gameObject, a_LifeTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
