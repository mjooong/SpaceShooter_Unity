using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//< ���̵� ���� >
// 1, ���ʹ� 3������ �Ѿ��� �߻��ϰ� ���� 
// 2, ������ �Ѿ� �߻� �ֱ� : 2��(3��)���� ~ 1��(99��)���� ���ϰ�...
// 3, ������ ������ �Ѿ��� �̵��ӵ� ���� :
//	 ���̵� 4������ 15�� �þ���� 800 ~ 3000���� ����
// 4, �� ���� ���� �ִ� ���� ������ ���� :
//	 8������ 1������ �þ����... 
//        //������ maxMonster = 10; (�ʵ忡 Ȱ�� ���� ���� ������ ����) : 10 ~ 25��������
//        //������ m_MonLimit = 20; (���� ī��Ʈ ������ : ������ ���̾Ƹ� ����) : 20 ~ 35��������
// 5, ���� ������ �ö� ������ ���� ������ ������ ���� �� �ְ� �ؼ� �����ǽ��� �ش�.

public enum GameState
{
    GameIng,
    GameEnd
}

public class GameMgr : MonoBehaviour
{
    public static GameState s_GameState = GameState.GameIng;
    //Text UI �׸� ������ ���� ����
    public Text txtScore;
    //���� ������ ����ϱ� ���� ����
    private int totScore = 0;
    int m_CurScore = 0;     //�̹� ������������ ���� ��������

    public Button BackBtn;

    [Header("------ Monster Spawn ------")]
    //���Ͱ� ����� ��ġ�� ���� �迭
    public Transform[] points;
    //���� �������� �Ҵ��� ����
    public GameObject monsterPrefab;
    //���͸� �̸� ������ ������ ����Ʈ �ڷ���
    public List<GameObject> monsterPool = new List<GameObject>();

    //���͸� �߻���ų �ֱ�
    public float createTime = 2.0f;
    //������ �ִ� �߻� ����
    public int maxMonster = 10;

    int m_MonCurNum = 0;     //���� ������ ������ ���� ī��Ʈ ����
    int m_MonLimit = 20;     //���� ������ ���� �ִ� ���� ������

    //���� ���� ���� ����
    public bool isGameOver = false;

    //������ ���� ���� ����
    public float sfxVolumn = 0.2f;
    //���� ��Ʈ ���
    public bool isSfxMute = false;

    PlayerCtrl m_RefHero = null;

    //--- �Ӹ����� ������ ����� ���� ����
    [Header("------ DamageText ------")]
    public Transform m_Damage_Canvas = null;
    public GameObject m_DamagePrefab = null;
    //--- �Ӹ����� ������ ����� ���� ����

    [Header("------ Skill Timer ------")]
    public GameObject m_SkCoolObj = null;
    public Transform m_SkillCoolRoot = null;
    public SkInvenNode[] m_SkInvenNode;    //Skill �κ��丮 ��ư ���� ����

    //--- Coin Item ���� ����
    public static GameObject m_CoinItem = null;
    //--- Coin Item ���� ����

    //--- ���� ��� ǥ�� UI
    [Header("------ Gold UI ------")]
    public Text m_UserGoldText = null;  //���� ��尪 ǥ�� UI
    int m_CurGold = 0;  //�̹� ������������ ���� ��尪 ����

    [Header("------ Game Over ------")]
    public GameObject ResultPanel = null;
    public Text Title_Text = null;
    public Text Result_Text = null;
    public Button Replay_Btn = null;
    public Button RstLobby_Btn = null;

    [Header("------ Door Ctrl ------")]
    public Text m_BL_Tm_Text = null;
    public Text m_LastBlockText = null;
    public Text m_DoorOpenText = null;
    float m_Block_TimeOut = 0.0f;       //�̹��� Ż�� �ð� Ÿ�̸�
    GameObject[] m_DoorObj = new GameObject[3];
    public static GameObject m_DiamondItem = null;

    //�̱��� ������ ���� �ν��Ͻ� ���� ����
    public static GameMgr Inst = null;

    void Awake()
    {
        //GameMgr Ŭ������ �ν��Ͻ��� ����
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsEndingScene() == true)
            return;

        Time.timeScale = 1.0f;      //���� �ӵ���...
        s_GameState = GameState.GameIng;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        //ó�� ���� �� ����� ���ھ� ���� �ε�
        DispScore(0);

        if (BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });

        //--- ���̵� �� ���� ���� �ִ� ���� ������ ����
        //8������ 1������ �þ����... 20���� ~ 35�������� ���� �ǵ���...
        //������ maxMonster = 10; (�ʵ忡 Ȱ�� ���� ���� ������ ����) : 10 ~ 25��������
        //������ m_MonLimit = 20; (���� ī��Ʈ ������ : ������ ���̾Ƹ� ����) : 20 ~ 35��������
        int a_CacMaxMon = GlobalValue.g_CurBlockNum - 7;
        if (a_CacMaxMon < 0)
            a_CacMaxMon = 0;
        a_CacMaxMon = 10 + a_CacMaxMon;
        if (25 < a_CacMaxMon)
            a_CacMaxMon = 25;
        maxMonster = a_CacMaxMon;       //10���� ~ 25���� 7������ �Ѹ����� �þ

        m_MonLimit = 10 + a_CacMaxMon;  //20���� ~ 35���� 7������ �Ѹ����� �þ
        //--- ���̵� �� ���� ���� �ִ� ���� ������ ����

        //--- GameOver ��ư ó�� �ڵ�
        if (RstLobby_Btn != null)
            RstLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });

        if (Replay_Btn != null)
            Replay_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLevel01");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);

            });
        //--- GameOver ��ư ó�� �ڵ�

        //---- Monster Spawn
        // Hierarchy ���� SpawnPoint�� ã�� ������ �ִ� ��� Transform ������Ʈ�� ã�ƿ�
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        //���͸� ������ ������Ʈ Ǯ�� ����
        for(int i = 0; i < maxMonster; i++)
        {
            //���� �������� ����
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
            //������ ������ �̸� ����
            monster.name = "Monster_" + i.ToString();
            //������ ���͸� ��Ȱ��ȭ
            monster.SetActive(false);
            //������ ���͸� ������Ʈ Ǯ�� �߰�
            monsterPool.Add(monster);
        }

        m_BL_Tm_Text.text = GlobalValue.g_CurBlockNum + "��(����:" +
                            GlobalValue.g_BestBlock + "��)";

        if(points.Length > 0)
        { //���� ���� �ڷ�ƾ �Լ� ȣ��
            StartCoroutine(this.CreateMonster());
        }

        m_RefHero = GameObject.FindObjectOfType<PlayerCtrl>();

        m_CoinItem = Resources.Load("CoinItem/CoinPrefab") as GameObject;

        //--- Find Door
        GameObject a_DoorObj = GameObject.Find("Gate_In_1");
        if (a_DoorObj != null)
            m_DoorObj[0] = a_DoorObj;
        a_DoorObj = GameObject.Find("Gate_Exit_1");
        if(a_DoorObj != null)
        {
            m_DoorObj[1] = a_DoorObj;
            m_DoorObj[1].SetActive(false);
        }
        a_DoorObj = GameObject.Find("Gate_Exit_2");
        if(a_DoorObj != null)
        {
            m_DoorObj[2] = a_DoorObj;
            m_DoorObj[2].SetActive(false); 
        }

        if (GlobalValue.g_CurBlockNum <= 1)
            m_DoorObj[0].SetActive(false);

        if(GlobalValue.g_CurBlockNum < GlobalValue.g_BestBlock)
        { //�ְ� ���� ���� ���̸� �׳� �����ش�.
            ShowDoor();
        }

        m_DiamondItem = Resources.Load("DiamondItem/DiamondPrefab") as GameObject;
        //--- Find Door

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if(0.0f < m_Block_TimeOut)
        {
            m_Block_TimeOut -= Time.deltaTime;
            m_BL_Tm_Text.text = GlobalValue.g_CurBlockNum + "��(����:" +
                                GlobalValue.g_BestBlock + "��) / " +
                                m_Block_TimeOut.ToString("F1");

            if(m_Block_TimeOut <= 0.0f)
            {
                //"GameOber"
                s_GameState = GameState.GameEnd;
                Time.timeScale = 0.0f;      //�Ͻ�����
                GameOverFunc();
            }
        }//if(0.0f < m_Block_TimeOut)

        Skill_Update();

        MissionUIUpdate();
    }

    // ���� ���� �� ȭ�� ǥ��
    public void DispScore(int score)
    {
        //totScore += score;
        //txtScore.text = "SCORE <color=#ff0000>" + totScore.ToString() + "</color>";

        m_CurScore += score;
        if (m_CurScore < 0)
            m_CurScore = 0;

        GlobalValue.g_BestScore += score;

        if (GlobalValue.g_BestScore < 0)
            GlobalValue.g_BestScore = 0;

        int a_MaxValue = int.MaxValue - 10;
        if (a_MaxValue < GlobalValue.g_BestScore)
            GlobalValue.g_BestScore = a_MaxValue;

        txtScore.text = "SCORE <color=#ff0000>" + m_CurScore.ToString() +
                        "</color>" + " / " +
                        "BEST <color=#ff0000>" + GlobalValue.g_BestScore.ToString() +
                        "</color>";

        //PlayerPrefs.SetInt("BestScore", GlobalValue.g_BestScore);
        NetworkMgr.Inst.PushPacket(PacketType.BestScore);

    }//public void DispScore(int score)

    public void AddGold(int Value = 10)
    {
        m_CurGold += Value;
        if (m_CurGold < 0)
            m_CurGold = 0;

        GlobalValue.g_UserGold += Value;

        if (GlobalValue.g_UserGold < 0)
            GlobalValue.g_UserGold = 0;

        int a_MaxValue = int.MaxValue - 10;
        if (a_MaxValue < GlobalValue.g_UserGold)
            GlobalValue.g_UserGold = a_MaxValue;

        m_UserGoldText.text = "Gold <color=#ffff00>" +
                            GlobalValue.g_UserGold + "</color>";

        PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
    }

    public static bool IsPointerOverUIObject() //UGUI�� UI���� ���� ��ŷ�Ǵ��� Ȯ���ϴ� �Լ�
    {
        PointerEventData a_EDCurPos = new PointerEventData(EventSystem.current);

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)

			List<RaycastResult> results = new List<RaycastResult>();
			for (int i = 0; i < Input.touchCount; ++i)
			{
				a_EDCurPos.position = Input.GetTouch(i).position;  
				results.Clear();
				EventSystem.current.RaycastAll(a_EDCurPos, results);
                if (0 < results.Count)
                    return true;
			}

			return false;
#else
        a_EDCurPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return (0 < results.Count);
#endif
    }//public bool IsPointerOverUIObject() 

    // ���� ���� �ڷ�ƾ �Լ�
    IEnumerator CreateMonster()
    {
        // ���� ���� �ñ��� ���� ����
        while ( !isGameOver )
        {
            //���� ���� �ֱ� �ð���ŭ ���� ������ �纸
            yield return new WaitForSeconds(createTime);

            if (m_MonLimit <= m_MonCurNum)
                continue;

            //�÷��̾ ������� �� �ڷ�ƾ�� ������ ���� ��ƾ�� �������� �ʰ� ó����
            if (GameMgr.s_GameState == GameState.GameEnd)
                yield break;  //�ڷ�ƾ �Լ����� �Լ��� ���������� ��ɾ�

            //������Ʈ Ǯ�� ó������ ������ ��ȸ
            foreach(GameObject monster in monsterPool)
            {
                //��Ȱ��ȭ ���η� ��� ������ ���͸� �Ǵ�
                if(!monster.activeSelf)
                {
                    //���͸� ������ų ��ġ�� �ε������� ����
                    int idx = Random.Range(1, points.Length);

                    //--- ���� ī��Ʈ �� ������ ���� ���� ���� üũ �ڵ�
                    m_MonCurNum++;
                    if(m_MonLimit <= m_MonCurNum)
                    {
                        if(GlobalValue.g_BestBlock <= GlobalValue.g_CurBlockNum)
                        {  //���� ���������� �Ѿ �� �ִ� ���� ���� ����
                            //���̾Ƹ� ����
                            //60�� Ÿ�̸� ������
                            if (m_DiamondItem != null)
                            {
                                GameObject a_DmdObj = (GameObject)Instantiate(m_DiamondItem);
                                a_DmdObj.transform.position = points[idx].position;
                            }
                            m_Block_TimeOut = 60.0f;

                            break;
                        }//if(GlobalValue.g_BestBlock <= GlobalValue.g_CurBlockNum)
                    }//if(m_MonLimit <= m_MonCurNum)
                    //--- ���� ī��Ʈ �� ������ ���� ���� ���� üũ �ڵ�

                    //������ ������ġ�� ����
                    monster.transform.position = points[idx].position;
                    //���͸� Ȱ��ȭ��
                    monster.SetActive(true);

                    //������Ʈ Ǯ���� ���� ������ �ϳ��� Ȱ��ȭ�� �� for ������ ��������
                    break;
                }
            }//foreach(GameObject monster in monsterPool)

        }//while ( !isGameOver )



        //// ���� ���� �ñ��� ���� ����
        //while(!isGameOver)
        //{
        //    //�÷��̾ ������� �� �ڷ�ƾ�� ������ ���� ��ƾ�� �������� �ʰ� ó����
        //    if (GameMgr.s_GameState == GameState.GameEnd)
        //        yield break;  //�ڷ�ƾ �Լ����� �Լ��� ���������� ��ɾ�

        //    // ���� ������ ���� ���� ����
        //    int monsterCount = (int)GameObject.FindGameObjectsWithTag("MONSTER").Length;

        //    //������ �ִ� ���� �������� ���� ���� ���� ����
        //    if(monsterCount < maxMonster)
        //    {
        //        //������ ���� �ֱ� �ð���ŭ ���
        //        yield return new WaitForSeconds(createTime);

        //        //�ұ�Ģ���� ��ġ ����
        //        int idx = Random.Range(1, points.Length);
        //        //������ ���� ����
        //        Instantiate(monsterPrefab, points[idx].position, points[idx].rotation);
        //    }
        //    else
        //    {
        //        yield return null; //���÷����� ���� ���� ���
        //        //Update()�Լ��� ȣ�� �ӵ��� ���缭 while ���� ���ư��� �ϱ� ���� �ǵ�
        //    }
        //}//while(!isGameOver)

    }//IEnumerator CreateMonster()

    //���� ���� �Լ�
    public void PlaySfx(Vector3 pos, AudioClip sfx)
    {
        //���Ұ� �ɼ��� �����Ǹ� �ٷ� ��������
        if (isSfxMute) return;

        //���ӿ�����Ʈ�� �������� ����
        GameObject soundObj = new GameObject("Sfx");
        //���� �߻� ��ġ ����
        soundObj.transform.position = pos;

        //������ ���� ������Ʈ�� AudioSource ������Ʈ �߰�
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        //AudioSource �Ӽ� ����
        audioSource.clip = sfx;
        audioSource.minDistance = 10.0f;
        audioSource.maxDistance = 30.0f;
        //sfxVolumn ������ ������ ��ü���� ���� ���� ����
        audioSource.volume = sfxVolumn;
        //���� ����
        audioSource.Play();

        //������ �÷��̰� ����Ǹ� �������� ������ ���ӿ�����Ʈ�� ����
        Destroy(soundObj, sfx.length);
    }

    void Skill_Update()
    {
        //���콺 �߾ӹ�ư(�� Ŭ��)
        if(Input.GetMouseButtonDown(2))
        {
            UseSkill_Key(SkillType.Skill_1);  //����ź ���
        }

        //--- ����Ű �̿����� ��ų ����ϱ�...
        if(Input.GetKeyDown(KeyCode.Alpha1) ||     //����Ű 1
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseSkill_Key(SkillType.Skill_0);  //���������۽�ų
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2) || 
                Input.GetKeyDown(KeyCode.Keypad2))  //����Ű 2
        {
            UseSkill_Key(SkillType.Skill_1);  //����ź
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3) ||
                Input.GetKeyDown(KeyCode.Keypad3))   //����Ű 3
        {
            UseSkill_Key(SkillType.Skill_2);   //��ȣ��
        }
        //--- ����Ű �̿����� ��ų ����ϱ�...
    }

    public void UseSkill_Key(SkillType a_SkType)
    {
        if (GlobalValue.g_SkillCount[(int)a_SkType] <= 0)
            return;

        if (m_RefHero != null)
            m_RefHero.UseSkill_Item(a_SkType);

        if ((int)a_SkType < m_SkInvenNode.Length)
            m_SkInvenNode[(int)a_SkType].m_SkCountText.text =
                        GlobalValue.g_SkillCount[(int)a_SkType].ToString();
    }

    public void SpawnDamageText(int dmg, Transform a_SpawnTr, Color a_Color)
    {
        if (m_Damage_Canvas == null || m_DamagePrefab == null)
            return;

        GameObject a_DamageObj = Instantiate(m_DamagePrefab) as GameObject;
        Vector3 a_StCacPos = new Vector3(a_SpawnTr.position.x,
                                        a_SpawnTr.position.y + 2.21f, a_SpawnTr.position.z);
        a_DamageObj.transform.SetParent(m_Damage_Canvas, false);
        DamageText a_DamageTx = a_DamageObj.GetComponent<DamageText>();
        a_DamageTx.m_BaseWdPos = a_StCacPos;
        a_DamageTx.m_DamageVal = dmg;

        //�ʱ� ��ġ ��� �ֱ� //---World ��ǥ�� UGUI ��ǥ�� ȯ���� �ִ� �ڵ�
        RectTransform a_CanvasRect = m_Damage_Canvas.GetComponent<RectTransform>();
        Vector2 a_ScreenPos = Camera.main.WorldToViewportPoint(a_StCacPos);
        Vector2 a_WdScPos = Vector2.zero;
        a_WdScPos.x = (a_ScreenPos.x * a_CanvasRect.sizeDelta.x) -
                                        (a_CanvasRect.sizeDelta.x * 0.5f);
        a_WdScPos.y = (a_ScreenPos.y * a_CanvasRect.sizeDelta.y) -
                                        (a_CanvasRect.sizeDelta.y * 0.5f);
        a_DamageObj.GetComponent<RectTransform>().anchoredPosition = a_WdScPos;
        //�ʱ� ��ġ ��� �ֱ� //---World ��ǥ�� UGUI ��ǥ�� ȯ���� �ִ� �ڵ�

        Text a_RefText = a_DamageObj.GetComponentInChildren<Text>();
        a_RefText.color = a_Color;  

    }//public void SpawnDamageText(int dmg, Transform a_SpawnTr, Color a_Color)

    public void SkillTimeMethod(float a_Time, float a_Delay)
    {
        GameObject obj = Instantiate(m_SkCoolObj) as GameObject;
        obj.transform.SetParent(m_SkillCoolRoot, false);
        SkCool_NodeCtrl skill = obj.GetComponent<SkCool_NodeCtrl>();
        skill.InitState(a_Time, a_Delay);
    }

    public void SpawnCoin(Vector3 a_Pos) //���� ������ ���
    {
        if (m_CoinItem == null)
            return;

        GameObject a_CoinObj = Instantiate(m_CoinItem) as GameObject;
        a_CoinObj.transform.position = a_Pos;
        Destroy(a_CoinObj, 10.0f);  //10�ʳ��� �Ծ�� �Ѵ�.
    }

    void RefreshGameUI()
    {
        if (m_UserGoldText != null)
            m_UserGoldText.text = "Gold <color=#ffff00>" +
                                    GlobalValue.g_UserGold + "</color>";

        for(int ii = 0; ii < GlobalValue.g_SkillCount.Length; ii++)
        {
            if (m_SkInvenNode.Length <= ii)
                continue;

            m_SkInvenNode[ii].m_SkType = (SkillType)ii;
            m_SkInvenNode[ii].m_SkCountText.text = GlobalValue.g_SkillCount[ii].ToString();
        }
    }

    public void GameOverFunc()
    {
        ResultPanel.SetActive(true);

        Result_Text.text = "NickName\n" + GlobalValue.g_NickName + "\n\n" +
                            "ȹ�� ����\n" + m_CurScore + "\n\n" + "ȹ�� ���\n" + m_CurGold;   

        if(SceneManager.GetActiveScene().name == "scLevel02")
        {
            Title_Text.text = "< Game Ending >";
            Result_Text.text = "NickName\n" + GlobalValue.g_NickName + "\n\n" +
                            "Made by\n" + "SBS Game Academy" + "\n\n" + "Since\n" + "2023.6.28";
        }
    }

    public void ShowDoor()
    {
        int a_Idx = (GlobalValue.g_CurBlockNum % 2) + 1;
        if ((1 <= a_Idx && a_Idx <= 2) && m_DoorObj[a_Idx] != null)
            m_DoorObj[a_Idx].SetActive(true);

        if (m_LastBlockText != null)
            m_LastBlockText.gameObject.SetActive(false);

        if (m_DoorOpenText != null)
            m_DoorOpenText.gameObject.SetActive(true);
    }

    void MissionUIUpdate()
    {
        if (m_LastBlockText == null)
            return;

        if (m_LastBlockText.gameObject.activeSelf == false)
            return;

        if(0.0f < m_Block_TimeOut)
        {
            m_LastBlockText.text = "<color=#00ffff>���̾Ƹ�尡 �� ��򰡿� �����Ǿ����ϴ�.</color>";
        }
        else
        {
            m_LastBlockText.text = "<color=#ffff00>(" + m_MonCurNum +
                        " / " + m_MonLimit + " Mon) " +
                        "���� 100��</color>";
        }
    }

    bool IsEndingScene()
    {
        if(SceneManager.GetActiveScene().name != "scLevel02")
        {
            return false; //�̹��� �ε��� ���� �������� �ƴ϶��...
        }

        Time.timeScale = 1.0f;      //���� �ӵ���...
        s_GameState = GameState.GameIng;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        //ó�� ���� �� ����� ���ھ� ���� �ε�
        DispScore(0);

        if (BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });

        m_BL_Tm_Text.text = GlobalValue.g_CurBlockNum + "��(����:" +
                         GlobalValue.g_BestBlock + "��)";

        m_RefHero = GameObject.FindObjectOfType<PlayerCtrl>();

        if (RstLobby_Btn != null)
            RstLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });

        return true; 
    }

}//public class GameMgr : MonoBehaviour
