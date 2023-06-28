using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title_Mgr : MonoBehaviour
{
    [Header("------- LoginPanel -------")]
    public GameObject m_LoginPanelObj;
    public Button m_LoginBtn = null;

    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.LoadGameData();

        //--- LoginPanel
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtn);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoginBtn()
    {
        SceneManager.LoadScene("scLobby");
    }
}
