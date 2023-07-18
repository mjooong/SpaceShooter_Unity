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

        //MessageOnOff("�޽��� �׽�Ʈ");
    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if(0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if(ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);    // �޼��� ����
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
            MessageOnOff("ID, PW�� ��ĭ ���� �Է��ϼ���.");
            return;
        }

        if(!(3 <= a_IdStr.Length && a_IdStr.Length < 20))
        {
            MessageOnOff("ID�� 3���� �̻�, 20���� ���Ϸ� �ۼ��� �ּ���.");
            return;
        }

        if(!(4 <= a_PwStr.Length && a_PwStr.Length < 20))
        {
            MessageOnOff("PW�� 4���� �̻�, 20���� ���Ϸ� �ۼ��� �ּ���.");
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

        yield return a_www.SendWebRequest();    // ������ �� ������ ���..

        if(a_www.error == null) // ������ �߻����� ���� ���
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);
            a_www.Dispose();

            //Debug.Log(sz);
            if(sz.Contains("Login-Success!!") == false)
            {
                if (sz.Contains("ID does not exist") == true)
                    MessageOnOff("ID�� �������� �ʽ��ϴ�.");
                else if (sz.Contains("PassWord does not Match") == true)
                    MessageOnOff("PW�� ��ġ���� �ʽ��ϴ�.");
                else 
                    MessageOnOff("�α��� ����, �ٽ� �õ��� �ּ���.");

                yield break;    // �Ϲ� �Լ��� return; ȿ��
            }

            if(sz.Contains("{\"") == false)     // JSON ������ �´��� Ȯ���ϴ� �ڵ�
            {
                MessageOnOff("������ ������ ���������� �ʽ��ϴ�. : " + sz);

                yield break;
            }

            GlobalValue.g_Unique_ID = a_IdStr;  // ���߿� ��ȣȭ �ʿ�

            string a_GetStr = sz.Substring(sz.IndexOf("{\""));

            // JSON �Ľ�
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
            MessageOnOff("ID, PW, ���� ��ĭ ���� �Է��� �ּ���.");
            return;
        }

        if(!(3 <= a_IdStr.Length && a_IdStr.Length < 20))
        {
            MessageOnOff("ID�� 3���� �̻� 20���� ���Ϸ� �ۼ��� �ּ���.");
            return;
        }
        
        if(!(4 <= a_PwStr.Length && a_PwStr.Length < 20))
        {
            MessageOnOff("PW�� 4���� �̻� 20���� ���Ϸ� �ۼ��� �ּ���.");
            return;
        }

        if(!(2 <= a_NickStr.Length && a_NickStr.Length <20))
        {
            MessageOnOff("������ 2���� �̻� 20���� ���Ϸ� �ۼ��� �ּ���.");
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
        //���������� ���� �� �ѱ��� �ȱ����� �ϱ� ���� �ɼ�

        UnityWebRequest a_www = UnityWebRequest.Post(CreateUrl, form);
        yield return a_www.SendWebRequest();    // ������ �ö����� ���..

        if(a_www.error == null)  // ���� ���� ��
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);
            a_www.Dispose();

            if (sz.Contains("Create Success.") == true)
                MessageOnOff("���� ����");
            else if (sz.Contains("ID does exist.") == true)
                MessageOnOff("�ߺ��� ID�� �����մϴ�.");
            else if (sz.Contains("Nickname does exist.") == true)
                MessageOnOff("�ߺ��� ������ �����մϴ�.");
            else
                MessageOnOff(sz);
        }
        else
        {
            MessageOnOff("���� ���� : " + a_www.error);
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
