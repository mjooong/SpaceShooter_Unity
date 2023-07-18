using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;


public class Title_Mgr : MonoBehaviour
{
    [Header("------- LoginPanel -------")]
    public GameObject m_LoginPanelObj;
    public Button m_LoginBtn = null;
    public Button m_CreateAccOpenBtn = null;
    public InputField IDInputField;
    public InputField PWInputField;

    [Header("----- CreateAccountPanel -----")]
    public GameObject m_CreateAccPanelObj;
    public InputField New_IDInputField;
    public InputField New_PWInputField;
    public InputField New_NickInputField;
    public Button m_CreateAccountBtn = null;
    public Button m_CancelBtn = null;

    [Header("----- Message -----")]
    public Text MessageText;
    float ShowMsTimer = 0.0f;

    string LoginUrl = "";
    string CreateUrl = "";

    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.LoadGameData();

        //--- LoginPanel
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtn);

        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        // --- CreatAccountPanel
        if (m_CancelBtn != null)
            m_CancelBtn.onClick.AddListener(CreateCancelBtn);

        if (m_CreateAccountBtn != null)
            m_CreateAccountBtn.onClick.AddListener(CreateAccountBtn);

        LoginUrl = "http://minjong0712.dothome.co.kr/_WebProgram/Login.php";
        CreateUrl = "http://minjong0712.dothome.co.kr/_WebProgram/CreateAccount.php";

        //MessageOnOff("메시지 테스트");
    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if(0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if(ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);    // 메세지 끄기
            }
        }

    }//void Update()

    public void LoginBtn()
    {
        string a_IdStr = IDInputField.text;
        string a_PwStr = PWInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();

        if(string.IsNullOrEmpty(a_IdStr) == true ||
            string.IsNullOrEmpty(a_PwStr) == true)
        {
            MessageOnOff("ID, PW는 빈칸 없이 입력하세요.");
            return;
        }

        if(!(3 <= a_IdStr.Length && a_IdStr.Length < 20))
        {
            MessageOnOff("ID는 3글자 이상, 20글자 이하로 작성해 주세요.");
            return;
        }

        if(!(4 <= a_PwStr.Length && a_PwStr.Length < 20))
        {
            MessageOnOff("PW는 4글자 이상, 20글자 이하로 작성해 주세요.");
            return;
        }

        StartCoroutine(LoginCo(a_IdStr, a_PwStr));

        //SceneManager.LoadScene("scLobby");

    }//public void LoginBtn()

    IEnumerator LoginCo(string a_IdStr, string a_PwStr)
    {
        WWWForm form = new WWWForm();

        form.AddField("Input_user", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pass", a_PwStr);

        // using UnityEngine.Networking;
        UnityWebRequest a_www = UnityWebRequest.Post(LoginUrl, form);

        yield return a_www.SendWebRequest();    // 응답이 올 때까지 대기..

        if(a_www.error == null) // 에러가 발생하지 않은 경우
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);
            a_www.Dispose();

            //Debug.Log(sz);
            if(sz.Contains("Login-Success!!") == false)
            {
                if (sz.Contains("ID does not exist") == true)
                    MessageOnOff("ID가 존재하지 않습니다.");
                else if (sz.Contains("PassWord does not Match") == true)
                    MessageOnOff("PW가 일치하지 않습니다.");
                else 
                    MessageOnOff("로그인 실패, 다시 시도해 주세요.");

                yield break;    // 일반 함수의 return; 효과
            }

            if(sz.Contains("{\"") == false)     // JSON 형식이 맞는지 확인하는 코드
            {
                MessageOnOff("서버의 응답이 정상적이지 않습니다. : " + sz);

                yield break;
            }

            GlobalValue.g_Unique_ID = a_IdStr;  // 나중에 암호화 필요

            string a_GetStr = sz.Substring(sz.IndexOf("{\""));

            // JSON 파싱
            var N = JSON.Parse(a_GetStr);
            if (N == null)
                yield break;

            if (N["nick_name"] != null)
                GlobalValue.g_NickName = N["nick_name"];

            if (N["best_score"] != null)
                GlobalValue.g_BestScore = N["best_score"].AsInt;

            if (N["game_gold"] != null)
                GlobalValue.g_UserGold = N["game_gold"].AsInt;
            
            //if (N["block_info"] != null)
            //    GlobalValue.g_BestBlock = N["block_info"].AsInt;

            SceneManager.LoadScene("scLobby");
        }
        else
        {
            MessageOnOff(a_www.error);
            a_www.Dispose();
        }

    }//IEnumerator LoginCo(string a_IdStr, string a_PwStr)
    
    void OpenCreateAccBtn()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(false);

        if (m_CreateAccPanelObj != null)
            m_CreateAccPanelObj.SetActive(true);

    }//void OpenCreateAccBtn()

    void CreateCancelBtn()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(true);

        if (m_CreateAccPanelObj != null)
            m_CreateAccPanelObj.SetActive(false);

    }//void CreateCancelBtn()

    void CreateAccountBtn()
    {
        string a_IdStr = New_IDInputField.text;
        string a_PwStr = New_PWInputField.text;
        string a_NickStr = New_NickInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();
        a_NickStr = a_NickStr.Trim();

        if( string.IsNullOrEmpty(a_IdStr) == true ||
            string.IsNullOrEmpty(a_PwStr) == true ||
            string.IsNullOrEmpty(a_NickStr) == true )
        {
            MessageOnOff("ID, PW, 별명 빈칸 없이 입력해 주세요.");
            return;
        }

        if(!(3 <= a_IdStr.Length && a_IdStr.Length < 20))
        {
            MessageOnOff("ID는 3글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }
        
        if(!(4 <= a_PwStr.Length && a_PwStr.Length < 20))
        {
            MessageOnOff("PW는 4글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        if(!(2 <= a_NickStr.Length && a_NickStr.Length <20))
        {
            MessageOnOff("별명은 2글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        StartCoroutine(CreateCo(a_IdStr, a_PwStr, a_NickStr));

    }//void CreateAccountBtn()

    IEnumerator CreateCo(string a_IdStr, string a_PwStr, string a_NickStr)
    {
        WWWForm form = new WWWForm();
        form.AddField("Input_user", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pass", a_PwStr);
        form.AddField("Input_nick", a_NickStr, System.Text.Encoding.UTF8);
        //System.Text.Encoding.UTF8
        //웹서버에서 받을 때 한글이 안깨지게 하기 위한 옵션

        UnityWebRequest a_www = UnityWebRequest.Post(CreateUrl, form);
        yield return a_www.SendWebRequest();    // 응답이 올때까지 대기..

        if(a_www.error == null)  // 에러 없을 때
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);
            a_www.Dispose();

            if (sz.Contains("Create Success.") == true)
                MessageOnOff("가입 성공");
            else if (sz.Contains("ID does exist.") == true)
                MessageOnOff("중복된 ID가 존재합니다.");
            else if (sz.Contains("Nickname does exist.") == true)
                MessageOnOff("중복된 별명이 존재합니다.");
            else
                MessageOnOff(sz);
        }
        else
        {
            MessageOnOff("가입 실패 : " + a_www.error);
            Debug.Log(a_www.error);
            a_www.Dispose();
        }

    }//IEnumerator CreateCo(string a_IdStr, string a_PwStr, string a_NickStr)

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if(isOn == true)
        {
            MessageText.text = Mess;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = 7.0f;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }

    }//void MessageOnOff(string Mess = "", bool isOn = true)
}
