using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeCtrl : MonoBehaviour
{
    [HideInInspector] public bool m_SelOnOff = false;
    RawImage m_SelectImg = null;

    // Start is called before the first frame update
    void Start()
    {
        m_SelectImg = gameObject.GetComponentInChildren<RawImage>(true);

        Button a_SelBtn = gameObject.GetComponent<Button>();
        if (a_SelBtn != null)
            a_SelBtn.onClick.AddListener(() =>
            {
                m_SelOnOff = !m_SelOnOff;
                if (m_SelectImg != null)
                    m_SelectImg.gameObject.SetActive(m_SelOnOff);
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
