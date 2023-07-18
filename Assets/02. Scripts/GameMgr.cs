using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//< 난이도 설정 >
// 1, 몬스터는 3층부터 총알을 발사하게 조정 
// 2, 몬스터의 총알 발사 주기 : 2초(3층)에서 ~ 1초(99층)까지 변하게...
// 3, 층별로 몬스터의 총알의 이동속도 증가 :
//	 난이도 4층부터 15씩 늘어나도록 800 ~ 3000까지 증가
// 4, 층 별로 몬스터 최대 스폰 마릿수 증가 :
//	 8층부터 1마리씩 늘어나도록... 
//        //시작은 maxMonster = 10; (필드에 활동 가능 몬스터 마릿수 제한) : 10 ~ 25마리까지
//        //시작은 m_MonLimit = 20; (스폰 카운트 마릿수 : 마지막 다이아몬스 스폰) : 20 ~ 35마리까지
// 5, 높은 층으로 올라 갈수록 높은 가격의 동전을 먹을 수 있게 해서 목적의식을 준다.

public enum GameState
{
    GameIng,
    GameEnd
}

public class GameMgr : MonoBehaviour
{
    public static GameState s_GameState = GameState.GameIng;
    //Text UI 항목 연결을 위한 변수
    public Text txtScore;
    //누적 점수를 기록하기 위한 변수
    private int totScore = 0;
    int m_CurScore = 0;     //이번 스테이지에서 얻은 게임점수

    public Button BackBtn;

    [Header("------ Monster Spawn ------")]
    //몬스터가 출력할 위치를 담을 배열
    public Transform[] points;
    //몬스터 프리팹을 할당할 변수
    public GameObject monsterPrefab;
    //몬스터를 미리 생성해 저장할 리스트 자료형
    public List<GameObject> monsterPool = new List<GameObject>();

    //몬스터를 발생시킬 주기
    public float createTime = 2.0f;
    //몬스터의 최대 발생 개수
    public int maxMonster = 10;

    int m_MonCurNum = 0;     //현재 층에서 스폰된 몬스터 카운트 변수
    int m_MonLimit = 20;     //현재 층에서 몬스터 최대 스폰 마릿수

    //게임 종료 여부 변수
    public bool isGameOver = false;

    //사운드의 볼륨 설정 변수
    public float sfxVolumn = 0.2f;
    //사운드 뮤트 기능
    public bool isSfxMute = false;

    PlayerCtrl m_RefHero = null;

    //--- 머리위에 데미지 띄우기용 변수 선언
    [Header("------ DamageText ------")]
    public Transform m_Damage_Canvas = null;
    public GameObject m_DamagePrefab = null;
    //--- 머리위에 데미지 띄우기용 변수 선언

    [Header("------ Skill Timer ------")]
    public GameObject m_SkCoolObj = null;
    public Transform m_SkillCoolRoot = null;
    public SkInvenNode[] m_SkInvenNode;    //Skill 인벤토리 버튼 연결 변수

    //--- Coin Item 관련 변수
    public static GameObject m_CoinItem = null;
    //--- Coin Item 관련 변수

    //--- 보유 골드 표시 UI
    [Header("------ Gold UI ------")]
    public Text m_UserGoldText = null;  //누적 골드값 표기 UI
    int m_CurGold = 0;  //이번 스테이지에서 얻은 골드값 변수

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
    float m_Block_TimeOut = 0.0f;       //이번층 탈출 시간 타이머
    GameObject[] m_DoorObj = new GameObject[3];
    public static GameObject m_DiamondItem = null;

    //싱글턴 패턴을 위한 인스턴스 변수 선언
    public static GameMgr Inst = null;

    void Awake()
    {
        //GameMgr 클래스를 인스턴스에 대입
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsEndingScene() == true)
            return;

        Time.timeScale = 1.0f;      //원래 속도로...
        s_GameState = GameState.GameIng;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        //처음 실행 후 저장된 스코어 정보 로드
        DispScore(0);

        if (BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });

        //--- 난이도 층 별로 몬스터 최대 스폰 마릿수 증가
        //8층부터 1마리씩 늘어나도록... 20마리 ~ 35마리까지 스폰 되도록...
        //시작은 maxMonster = 10; (필드에 활동 가능 몬스터 마릿수 제한) : 10 ~ 25마리까지
        //시작은 m_MonLimit = 20; (스폰 카운트 마릿수 : 마지막 다이아몬스 스폰) : 20 ~ 35마리까지
        int a_CacMaxMon = GlobalValue.g_CurBlockNum - 7;
        if (a_CacMaxMon < 0)
            a_CacMaxMon = 0;
        a_CacMaxMon = 10 + a_CacMaxMon;
        if (25 < a_CacMaxMon)
            a_CacMaxMon = 25;
        maxMonster = a_CacMaxMon;       //10마리 ~ 25마리 7층부터 한마리씩 늘어남

        m_MonLimit = 10 + a_CacMaxMon;  //20마리 ~ 35마리 7층부터 한마리씩 늘어남
        //--- 난이도 층 별로 몬스터 최대 스폰 마릿수 증가

        //--- GameOver 버튼 처리 코드
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
        //--- GameOver 버튼 처리 코드

        //---- Monster Spawn
        // Hierarchy 뷰의 SpawnPoint를 찾아 하위에 있는 모든 Transform 컴포넌트를 찾아옴
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        //몬스터를 생성해 오브젝트 풀에 저장
        for(int i = 0; i < maxMonster; i++)
        {
            //몬스터 프리팹을 생성
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
            //생성한 몬스터의 이름 설정
            monster.name = "Monster_" + i.ToString();
            //생성한 몬스터를 비활성화
            monster.SetActive(false);
            //생성한 몬스터를 오브젝트 풀에 추가
            monsterPool.Add(monster);
        }

        m_BL_Tm_Text.text = GlobalValue.g_CurBlockNum + "층(도달:" +
                            GlobalValue.g_BestBlock + "층)";

        if(points.Length > 0)
        { //몬스터 생성 코루틴 함수 호출
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
        { //최고 도달 이하 층이면 그냥 열어준다.
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
            m_BL_Tm_Text.text = GlobalValue.g_CurBlockNum + "층(도달:" +
                                GlobalValue.g_BestBlock + "층) / " +
                                m_Block_TimeOut.ToString("F1");

            if(m_Block_TimeOut <= 0.0f)
            {
                //"GameOber"
                s_GameState = GameState.GameEnd;
                Time.timeScale = 0.0f;      //일시정지
                GameOverFunc();
            }
        }//if(0.0f < m_Block_TimeOut)

        Skill_Update();

        MissionUIUpdate();
    }

    // 점수 누적 및 화면 표시
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

    public static bool IsPointerOverUIObject() //UGUI의 UI들이 먼저 피킹되는지 확인하는 함수
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

    // 몬스터 생성 코루틴 함수
    IEnumerator CreateMonster()
    {
        // 게임 종료 시까지 무한 루프
        while ( !isGameOver )
        {
            //몬스터 생성 주기 시간만큼 메인 루프에 양보
            yield return new WaitForSeconds(createTime);

            if (m_MonLimit <= m_MonCurNum)
                continue;

            //플레이어가 사망했을 때 코루틴을 종료해 다음 루틴을 진행하지 않게 처리함
            if (GameMgr.s_GameState == GameState.GameEnd)
                yield break;  //코루틴 함수에서 함수를 빠져나가는 명령어

            //오브젝트 풀의 처음부터 끝까지 순회
            foreach(GameObject monster in monsterPool)
            {
                //비활성화 여부로 사용 가능한 몬스터를 판단
                if(!monster.activeSelf)
                {
                    //몬스터를 출현시킬 위치의 인덱스값을 추출
                    int idx = Random.Range(1, points.Length);

                    //--- 몬스터 카운트 및 마지막 몬스터 스폰 상태 체크 코드
                    m_MonCurNum++;
                    if(m_MonLimit <= m_MonCurNum)
                    {
                        if(GlobalValue.g_BestBlock <= GlobalValue.g_CurBlockNum)
                        {  //현재 다음층으로 넘어갈 수 있는 층에 있을 때만
                            //다이아몬스 스폰
                            //60초 타이머 돌리기
                            if (m_DiamondItem != null)
                            {
                                GameObject a_DmdObj = (GameObject)Instantiate(m_DiamondItem);
                                a_DmdObj.transform.position = points[idx].position;
                            }
                            m_Block_TimeOut = 60.0f;

                            break;
                        }//if(GlobalValue.g_BestBlock <= GlobalValue.g_CurBlockNum)
                    }//if(m_MonLimit <= m_MonCurNum)
                    //--- 몬스터 카운트 및 마지막 몬스터 스폰 상태 체크 코드

                    //몬스터의 출현위치를 설정
                    monster.transform.position = points[idx].position;
                    //몬스터를 활성화함
                    monster.SetActive(true);

                    //오브젝트 풀에서 몬스터 프리팹 하나를 활성화한 후 for 루프를 빠져나감
                    break;
                }
            }//foreach(GameObject monster in monsterPool)

        }//while ( !isGameOver )



        //// 게임 종료 시까지 무한 루프
        //while(!isGameOver)
        //{
        //    //플레이어가 사망했을 때 코루틴을 종료해 다음 루틴을 진행하지 않게 처리함
        //    if (GameMgr.s_GameState == GameState.GameEnd)
        //        yield break;  //코루틴 함수에서 함수를 빠져나가는 명령어

        //    // 현재 생성된 몬스터 개수 산출
        //    int monsterCount = (int)GameObject.FindGameObjectsWithTag("MONSTER").Length;

        //    //몬스터의 최대 생성 개수보다 적을 때만 몬스터 생성
        //    if(monsterCount < maxMonster)
        //    {
        //        //몬스터의 생성 주기 시간만큼 대기
        //        yield return new WaitForSeconds(createTime);

        //        //불규칙적인 위치 산출
        //        int idx = Random.Range(1, points.Length);
        //        //몬스터의 동적 생성
        //        Instantiate(monsterPrefab, points[idx].position, points[idx].rotation);
        //    }
        //    else
        //    {
        //        yield return null; //한플레임이 도는 동안 대기
        //        //Update()함수의 호출 속도와 맞춰서 while 문이 돌아가게 하기 위한 의도
        //    }
        //}//while(!isGameOver)

    }//IEnumerator CreateMonster()

    //사운드 공용 함수
    public void PlaySfx(Vector3 pos, AudioClip sfx)
    {
        //음소거 옵션이 설정되면 바로 빠져나감
        if (isSfxMute) return;

        //게임오브젝트를 동적으로 생성
        GameObject soundObj = new GameObject("Sfx");
        //사운드 발생 위치 지정
        soundObj.transform.position = pos;

        //생성한 게임 오브젝트에 AudioSource 컴포넌트 추가
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        //AudioSource 속성 설정
        audioSource.clip = sfx;
        audioSource.minDistance = 10.0f;
        audioSource.maxDistance = 30.0f;
        //sfxVolumn 변수로 게임의 전체적인 볼륨 설정 가능
        audioSource.volume = sfxVolumn;
        //사운드 실행
        audioSource.Play();

        //사운드의 플레이가 종료되면 동적으로 생성한 게임오브젝트를 삭제
        Destroy(soundObj, sfx.length);
    }

    void Skill_Update()
    {
        //마우스 중앙버튼(휠 클릭)
        if(Input.GetMouseButtonDown(2))
        {
            UseSkill_Key(SkillType.Skill_1);  //수류탄 사용
        }

        //--- 단축키 이용으로 스킬 사용하기...
        if(Input.GetKeyDown(KeyCode.Alpha1) ||     //단축키 1
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseSkill_Key(SkillType.Skill_0);  //힐링아이템스킬
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2) || 
                Input.GetKeyDown(KeyCode.Keypad2))  //단축키 2
        {
            UseSkill_Key(SkillType.Skill_1);  //수류탄
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3) ||
                Input.GetKeyDown(KeyCode.Keypad3))   //단축키 3
        {
            UseSkill_Key(SkillType.Skill_2);   //보호막
        }
        //--- 단축키 이용으로 스킬 사용하기...
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

        //초기 위치 잡아 주기 //---World 좌표를 UGUI 좌표로 환산해 주는 코드
        RectTransform a_CanvasRect = m_Damage_Canvas.GetComponent<RectTransform>();
        Vector2 a_ScreenPos = Camera.main.WorldToViewportPoint(a_StCacPos);
        Vector2 a_WdScPos = Vector2.zero;
        a_WdScPos.x = (a_ScreenPos.x * a_CanvasRect.sizeDelta.x) -
                                        (a_CanvasRect.sizeDelta.x * 0.5f);
        a_WdScPos.y = (a_ScreenPos.y * a_CanvasRect.sizeDelta.y) -
                                        (a_CanvasRect.sizeDelta.y * 0.5f);
        a_DamageObj.GetComponent<RectTransform>().anchoredPosition = a_WdScPos;
        //초기 위치 잡아 주기 //---World 좌표를 UGUI 좌표로 환산해 주는 코드

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

    public void SpawnCoin(Vector3 a_Pos) //코인 아이템 드롭
    {
        if (m_CoinItem == null)
            return;

        GameObject a_CoinObj = Instantiate(m_CoinItem) as GameObject;
        a_CoinObj.transform.position = a_Pos;
        Destroy(a_CoinObj, 10.0f);  //10초내에 먹어야 한다.
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
                            "획득 점수\n" + m_CurScore + "\n\n" + "획득 골드\n" + m_CurGold;   

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
            m_LastBlockText.text = "<color=#00ffff>다이아몬드가 맵 어딘가에 생성되었습니다.</color>";
        }
        else
        {
            m_LastBlockText.text = "<color=#ffff00>(" + m_MonCurNum +
                        " / " + m_MonLimit + " Mon) " +
                        "최종 100층</color>";
        }
    }

    bool IsEndingScene()
    {
        if(SceneManager.GetActiveScene().name != "scLevel02")
        {
            return false; //이번에 로딩된 씬이 엔딩씬이 아니라면...
        }

        Time.timeScale = 1.0f;      //원래 속도로...
        s_GameState = GameState.GameIng;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        //처음 실행 후 저장된 스코어 정보 로드
        DispScore(0);

        if (BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });

        m_BL_Tm_Text.text = GlobalValue.g_CurBlockNum + "층(도달:" +
                         GlobalValue.g_BestBlock + "층)";

        m_RefHero = GameObject.FindObjectOfType<PlayerCtrl>();

        if (RstLobby_Btn != null)
            RstLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });

        return true; 
    }

}//public class GameMgr : MonoBehaviour
