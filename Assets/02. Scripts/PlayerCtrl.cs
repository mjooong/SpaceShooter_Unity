using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; //UI 항목에 접근하기 위해 반드시 추가

//클래스에 System.Serializable이라는 어트리뷰트(Attribute)를 명시해야 Inspector 뷰에 노출됨
[System.Serializable]
public class Anis
{
    public AnimationClip idle;
    public AnimationClip runForward;
    public AnimationClip runBackward;
    public AnimationClip runRight;
    public AnimationClip runLeft;
}

public class PlayerCtrl : MonoBehaviour
{
    private float h = 0.0f;
    private float v = 0.0f;

    // 이동 속도 변수
    public float moveSpeed = 10.0f;

    // 회전 속도 변수
    public float rotSpeed = 100.0f;

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anis anim;

    //아래에 있는 3D 모델의 Animation 컴포넌트에 접근하기 위한 변수
    public Animation _animation;

    //Player의 생명 변수
    public int hp = 100;
    //Player의 생명 초깃값
    private int initHp;
    //Player의 Health bar 이미지
    public Image imgHpbar;

    CharacterController m_ChrController; //현재 캐릭터가 가지고 있는 캐릭터 컨트롤러

    public GameObject bloodEffect;  //혈흔 효과 프리팹

    FireCtrl m_FireCtrl = null;

    //--- 쉴드 스킬
    float m_SdDuration = 20.0f;
    float m_SdOnTime = 0.0f;
    public GameObject ShieldObj = null;
    //--- 쉴드 스킬

    [Header("------ Sound ------")]
    public AudioClip m_CoinSdClip;
    public AudioClip m_DiamondSdClip;
    AudioSource Ad_Source = null;

    //--- Door 관련 변수
    bool isShift = false;
    float m_CkTimer = 0.3f; //엔딩씬 로딩 즉시 충돌되는 현상을 방지하기 위한 타이머
    //--- Door 관련 변수

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;  //모니터 주사율(플레임율)이 다른 컴퓨터일 경우 캐릭터 조작시 빠르게 움직일 수 있다.
        Application.targetFrameRate = 60; //실행 프레임 속도 60프레임으로 고정 시키기.. 코드

        //생명 초깃값 설정
        initHp = hp;

        //자신의 하위에 있는 Animation 컴포넌트를 찾아와 변수에 할당
        _animation = GetComponentInChildren<Animation>();

        //Animation 컴포넌트의 애니메이션 클립을 지정하고 실행
        _animation.clip = anim.idle;
        _animation.Play();

        m_ChrController = GetComponent<CharacterController>();

        m_FireCtrl = GetComponent<FireCtrl>();

        Transform a_PMesh = transform.Find("PlayerModel");
        if (a_PMesh != null)
            Ad_Source = a_PMesh.GetComponent<AudioSource>();
    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "scLevel02")
        {
            if(0.0f < m_CkTimer)
            {
                transform.position = new Vector3(11.0f, 0.07f, 12.7f);
                transform.eulerAngles = new Vector3(0.0f, -127.7f, 0.0f);
                m_CkTimer -= Time.deltaTime;
                return;
            }
        }//if(SceneManager.GetActiveScene().name == "scLevel02")

        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        h = Input.GetAxis("Horizontal"); //-1.0f ~ 1.0f
        v = Input.GetAxis("Vertical");

        //전후좌우 이동 방향 벡터 계산
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        if (1.0f < moveDir.magnitude)
            moveDir.Normalize();

        ////Translate(이동방향 * Time.deltaTime * 변위값 * 속도, 기준좌표)
        //transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);

        if(m_ChrController != null)
        {
            // 벡터를 로컬 좌표계 기준에서 월드 좌표계 기준으로 변환한다.
            moveDir = transform.TransformDirection(moveDir);

            m_ChrController.SimpleMove(moveDir * moveSpeed);
        }

        if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        if (GameMgr.IsPointerOverUIObject() == false)
        {
            //Vector3.up 축을 기준으로 rotSpeed 만큼의 속도로 회전
            transform.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X") * 3.0f);
        }

        //키보드 입력값을 기준으로 동작할 애니메이션 수행
        if(v >= 0.1f)
        {
            //전진 애니메이션
            _animation.CrossFade(anim.runForward.name, 0.3f);
        }
        else if(v <= -0.1f)
        {
            //후진 애니메이션
            _animation.CrossFade(anim.runBackward.name, 0.3f);
        }
        else if(h >= 0.1f)
        {
            //오른쪽 이동 애니메이션
            _animation.CrossFade(anim.runRight.name, 0.3f);

        }
        else if(h <= -0.1f)
        {
            //왼쪽 이동 애니메이션
            _animation.CrossFade(anim.runLeft.name, 0.3f);
        }
        else
        {
            //정지시 idle 애니메이션
            _animation.CrossFade(anim.idle.name, 0.3f);
        }

        SkillUpdate();

        if(Input.GetKey(KeyCode.LeftShift))
        {
            isShift = true;
        }
        else
        {
            isShift = false;
        }

    }//void Update()

    //충돌한 Collider의 OsTrigger 옵션이 체크됐을 때 발생
    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP차감
        if(coll.gameObject.tag == "PUNCH")
        {
            if (0.0f < m_SdOnTime) //쉴드 발동 중이면...
                return;

            if (hp <= 0.0f)
                return;

            hp -= 10;

            //Image UI 항목의 fillAmount 속성을 조절해 생명 게이지 값 조절
            imgHpbar.fillAmount = (float)hp / (float)initHp;

            //Debug.Log("Player HP = " + hp.ToString());

            //Player의 생명이 0이하이면 사망 처리
            if(hp <= 0)
            {
                PlayerDie();
            }
        }//if(coll.gameObject.tag == "PUNCH")
        else if(coll.gameObject.name.Contains("CoinPrefab") == true)
        {
            //--- 높은 층으로 올라 갈수록 높은 가격의 동전을 먹을 수 있게 해서 목적의식을 준다.
            int a_CacGold = (GlobalValue.g_CurBlockNum - 5) * 2;
            if (a_CacGold < 0)
                a_CacGold = 0;
            a_CacGold = 10 + a_CacGold; //기본 10원
            if (200 < a_CacGold)
                a_CacGold = 200;    //최대 200원까지만 지급 제한
            //--- 높은 층으로 올라 갈수록 높은 가격의 동전을 먹을 수 있게 해서 목적의식을 준다.
            GameMgr.Inst.AddGold(a_CacGold);

            if (Ad_Source != null && m_CoinSdClip != null)
                Ad_Source.PlayOneShot(m_CoinSdClip, 0.3f);

            Destroy(coll.gameObject);
        }
        else if(coll.gameObject.name == "Gate_Exit_1" ||
                coll.gameObject.name == "Gate_Exit_2")
        {
            GlobalValue.g_CurBlockNum++;
            PlayerPrefs.SetInt("BlockNumber", GlobalValue.g_CurBlockNum);

            if(GlobalValue.g_BestBlock < GlobalValue.g_CurBlockNum)
            {
                GlobalValue.g_BestBlock = GlobalValue.g_CurBlockNum;
                PlayerPrefs.SetInt("BestBlockNum", GlobalValue.g_BestBlock);
            }

            if (GlobalValue.g_CurBlockNum < 100)
            {
                SceneManager.LoadScene("scLevel01");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadScene("scLevel02");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            }

        }
        else if(coll.gameObject.name.Contains("DiamondPrefab") == true)
        {
            GameMgr.Inst.ShowDoor();    //다이아몬드를 먹어야 문이 열린다.

            if (Ad_Source != null && m_DiamondSdClip != null)
                Ad_Source.PlayOneShot(m_DiamondSdClip, 0.3f);

            Destroy(coll.gameObject);
        }//else if(coll.gameObject.name.Contains("DiamondPrefab") == true)
        else if(coll.gameObject.name.Contains("RTS15_desert") == true)
        {
            if (m_CkTimer <= 0.0f)
                PlayerDie();        //게임 엔딩
        }

    }//void OnTriggerEnter(Collider coll)

    void OnTriggerStay(Collider coll)
    {
        if(coll.gameObject.name == "Gate_In_1")
        {
            if (isShift == false)
                return;

            GlobalValue.g_CurBlockNum--;
            if (GlobalValue.g_CurBlockNum < 1)
                GlobalValue.g_CurBlockNum = 1;

            PlayerPrefs.SetInt("BlockNumber", GlobalValue.g_CurBlockNum);

            SceneManager.LoadScene("scLevel01");
            SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
        }
    }

    void OnCollisionEnter(Collision coll) //총알 충돌 처리
    {
        if(coll.gameObject.tag == "E_BULLET")
        {
            //혈흔 효과 생성
            GameObject blood =
                            (GameObject)Instantiate(bloodEffect,
                                coll.transform.position, Quaternion.identity);
            blood.GetComponent<ParticleSystem>().Play();
            Destroy(blood, 3.0f);
            //혈흔 효과 생성

            //Bullet 삭제
            Destroy(coll.gameObject);

            if (hp <= 0.0f)
                return;
            
            hp -= 10;

            if (imgHpbar == null)
                imgHpbar = GameObject.Find("HP_Image").GetComponent<Image>();

            if (imgHpbar != null)
                imgHpbar.fillAmount = (float)hp / (float)initHp;

            if(hp <= 0)
            {
                PlayerDie();
            }
        }//if(coll.gameObject.tag == "E_BULLET")

    }//void OnCollisionEnter(Collision coll) //총알 충돌 처리

    //Plsyer의 사망 처리 루틴
    void PlayerDie()
    {
        //Debug.Log("Player Die !!");

        //MONSTER라는 Tag를 가진 모든 게임오브젝트를 찾아옴
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        foreach(GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
            //monster.GetComponent<MonsterCtrl>().OnPlayerDie();
            //SendMessageOptions.DontRequireReceiver : 해당 함수가 없더라도 함수가 없다는
            //메시지를 리턴받지 않겠다는 옵션이다.
        }

        _animation.Stop();

        GameMgr.s_GameState = GameState.GameEnd;
        Time.timeScale = 0.0f;  //일시정지
        GameMgr.Inst.GameOverFunc();

    }//void PlayerDie()

    void SkillUpdate()
    {
        //---- 쉴드 상태 업데이트
        if(0.0f < m_SdOnTime)
        {
            m_SdOnTime -= Time.deltaTime;
            if (ShieldObj != null && ShieldObj.activeSelf == false)
                ShieldObj.SetActive(true);
        }
        else
        {
            if (ShieldObj != null && ShieldObj.activeSelf == true)
                ShieldObj.SetActive(false);
        }
        //---- 쉴드 상태 업데이트
    }

    public void UseSkill_Item(SkillType a_SkType)
    {
        if (a_SkType < 0 || SkillType.SkCount <= a_SkType)
            return;

        if(a_SkType == SkillType.Skill_0)   //힐링 아이템
        {
            hp += (int)(initHp * 0.3f);

            //데미지 텍스트 띄우기
            GameMgr.Inst.SpawnDamageText((int)(initHp * 0.3f),
                                        transform, new Color(0.18f, 0.5f, 0.34f));
            if (initHp < hp)
                hp = initHp;
            if (imgHpbar != null)
                imgHpbar.fillAmount = hp / (float)initHp;
        }
        else if(a_SkType == SkillType.Skill_1) //수류탄
        {
            if (m_FireCtrl != null)
                m_FireCtrl.FireGrenade();
        }
        else if(a_SkType == SkillType.Skill_2) //보호막
        {
            if (0.0f < m_SdOnTime)
                return;

            m_SdOnTime = m_SdDuration;

            GameMgr.Inst.SkillTimeMethod(m_SdOnTime, m_SdDuration);            
        }

        GlobalValue.g_SkillCount[(int)a_SkType]--;
        //string a_MkKey = "SkItem_" + ((int)a_SkType).ToString();
        //PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[(int)a_SkType]);
        NetworkMgr.Inst.PushPacket(PacketType.InfoUpdate);

    }//public void UseSkill_Item(SkillType a_SkType)

}
