using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    //몬스터의 상태 정보가 있는 Enumerable 변수 선언
    public enum MonsterState { idle, trace, attack, die };

    //몬스터의 현재 상태 정보를 저장할 Enum 변수
    public MonsterState monsterState = MonsterState.idle;

    private Transform playerTr;
    //private NavMeshAgent nvAgent;
    private Animator animator;

    //추적 사정거리
    public float traceDist = 10.0f;
    //공격 사정거리
    public float attackDist = 2.0f;

    //몬스터의 사망 여부
    private bool isDie = false;

    //혈흔 효과 프리팹
    public GameObject bloodEffect;
    //혈흔 데칼 효과 프리팹
    public GameObject bloodDecal;

    //몬스터 생명 변수
    private int hp = 100;

    //GameMgr에 접근하기 위한 변수
    private GameMgr gameMgr;

    Rigidbody m_Rigid;

    //--- 총알 발사 관련 변수
    public GameObject bullet;   //총알 프리팹
    float m_BLTime = 0.0f;
    LayerMask m_LaserMask = -1;
    //--- 총알 발사 관련 변수

    //Awake() --> OnEnable() --> Start()

    // Start is called before the first frame update
    void Awake()
    {
        traceDist = 10.0f;  //100.0f;
        attackDist = 1.6f;  //1.8f;

        //추적 대상인 Player의 Transform 할당
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        ////NavMeshAgent 컴포넌트 할당
        //nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        //Animator 컴포넌트 할당
        animator = this.gameObject.GetComponent<Animator>();

        gameMgr = GameObject.Find("GameMgr").GetComponent<GameMgr>();



        //////추적 대상의 위치를 설정하면 바로 추적 시작
        ////nvAgent.destination = playerTr.position;

        ////일정한 간격으로 몬스터의 행동 상태를 체크하는 코루틴 함수 실행
        //StartCoroutine(this.CheckMonsterState());

        ////몬스터의 상태에 따라 동작하는 루틴을 실행하는 코루틴 함수 실행
        //StartCoroutine(this.MonsterAction());

        m_Rigid = GetComponent<Rigidbody>();

    }//void Awake()

    void Start()
    {
        m_LaserMask = 1 << LayerMask.NameToLayer("Default");  //건물벽
        m_LaserMask |= 1 << LayerMask.NameToLayer("PLAYER");  //주인공
    }

    ////이벤트 발생 시 수행할 함수 연결
    //void OnEnable() //Active가 켜질 때 호출
    //{
    //    //일정한 간격으로 몬스터의 행동 상태를 체크하는 코루틴 함수 실행
    //    StartCoroutine(this.CheckMonsterState());

    //    //몬스터의 상태에 따라 동작하는 루틴을 실행하는 코루틴 함수 실행
    //    StartCoroutine(this.MonsterAction());
    //}


    // Update is called once per frame
    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        //지금 추적중이던 주인공 캐릭터 엑테브가 꺼져 있으면... 
        if (playerTr.gameObject.activeSelf == false) 
            playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        //GameObject.FindWithTag("Player") 함수는 새로 엑티브가 켜져 있는 주인공만 찾아줌

        CheckMonStateUpdate();
        MonActionUpdate();

        if (isDie == false)
            m_Rigid.AddForce(Vector3.down * 100.0f); //중력값 강제로 주기

    }//void Update()

    float m_AI_Delay = 0.0f;
    //일정한 간격으로 몬스터의 행동 상태를 체크하고 monsterState 값 변경
    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;

        //0.1초 주기로만 체크하기 위한 딜레이 계산 부분
        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;

        //몬스터와 플레이어 사이의 거리 측정
        float dist = Vector3.Distance(playerTr.position, transform.position);

        if (dist <= attackDist) //공격거리 범위 이내로 들어왔는지 확인
        {
            monsterState = MonsterState.attack;
        }
        else if (dist <= traceDist) //추적거리 범위 이내로 들어왔는지 확인
        {
            monsterState = MonsterState.trace;  //몬스터의 상태를 추적으로 설정
        }
        else
        {
            monsterState = MonsterState.idle;   //몬스터의 상태를 idle 모드로 설정
        }

    } //void CheckMonStateUpdate()

    //몬스터의 상태값에 따라 적절한 동작을 수행하는 함수
    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch (monsterState)
        {
            //idle 상태
            case MonsterState.idle:
                //추적 중지
                //Animator의 IsTrace 변수를 false로 설정
                animator.SetBool("IsTrace", false);
                break;

            //추적 상태
            case MonsterState.trace:
                {
                    //-- 이동 구현
                    float a_MoveVelocity = 2.0f; //평명 초당 이동 속도...
                    Vector3 a_MoveDir = Vector3.zero;
                    a_MoveDir = playerTr.position - transform.position;
                    float a_RayUDLimit = 3.0f;
                    if (a_MoveDir.y < -a_RayUDLimit || a_RayUDLimit < a_MoveDir.y)
                    { //높이값 한계치
                        return;
                    }
                    a_MoveDir.y = 0.0f;
                    Vector3 a_StepVec = (a_MoveDir.normalized * Time.deltaTime * a_MoveVelocity);
                    transform.Translate(a_StepVec, Space.World);

                    if(0.1f < a_MoveDir.magnitude) //캐릭터 회전
                    {
                        Quaternion a_TargetRot;
                        float a_RotSpeed = 7.0f;
                        a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
                        transform.rotation =
                                    Quaternion.Slerp(transform.rotation, a_TargetRot,
                                    Time.deltaTime * a_RotSpeed);
                    }//캐릭터 회전
                    //-- 이동 구현

                    //Animator의 IsAttack 변수를 false로 설정
                    animator.SetBool("IsAttack", false);
                    //Animator의 IsTrace 변수를 true로 설정
                    animator.SetBool("IsTrace", true);
                }
                break;

            //공격 상태
            case MonsterState.attack:
                {
                    //추적 중지
                    //IsAttack을 true로 설정해 attack State로 전이
                    animator.SetBool("IsAttack", true);

                    //몬스터가 주인공을 공격하면서 바라 보도록 해야 한다.
                    float a_RotSpeed = 6.0f;    //초당 회전 속도
                    Vector3 a_CacVLen = playerTr.position - transform.position;
                    a_CacVLen.y = 0.0f;
                    Quaternion a_TargetRot = Quaternion.LookRotation(a_CacVLen.normalized);
                    transform.rotation =
                                Quaternion.Slerp(transform.rotation, a_TargetRot,
                                                            Time.deltaTime * a_RotSpeed);
                    //몬스터가 주인공을 공격하면서 바라 보도록 해야 한다.
                }
                break;
        }//switch(monsterState)

        //--- 총알 발사
        if(2 < GlobalValue.g_CurBlockNum) //몬스터는 3층부터 총알발사
            FireUpdate();
        //--- 총알 발사

    } //void MonActionUpdate()

    //Bullet과 충돌 체크
    void OnCollisionEnter(Collision coll)
    {
        if(coll.gameObject.tag == "BULLET")
        {
            //혈흔 효과 함수 호출
            CreateBloodEffect( coll.transform.position + (coll.transform.forward * -0.3f) );

            //맞은 총알의 Damage를 추출해 몬스터 hp 차감
            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
            if(hp <= 0)
            {
                MonsterDie();
            }

            //Bullet 삭제
            Destroy(coll.gameObject);

            //IsHit Trigger 를 발생시키면 Any State에서 gothit로 전이됨
            animator.SetTrigger("IsHit");
        }
    }

    //몬스터 사망 시 처리 루틴
    void MonsterDie()
    {
        //사망한 몬스터의 태그를 Untagged로 변경
        gameObject.tag = "Untagged";

        //모든 코루틴을 정지
        StopAllCoroutines();

        isDie = true;
        monsterState = MonsterState.die;
        animator.SetTrigger("IsDie");

        m_Rigid.useGravity = false;

        //몬스터에 추가된 Collider를 비활성화
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

        foreach(Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = false;
        }

        //GameUI의 스코어 누적과 스코어 표시 함수 호출
        gameMgr.DispScore(1); //50);

        //몬스터 오브젝트 풀로 환원시키는 코루틴 함수 호출
        StartCoroutine(this.PushObjectPool());

        //--- 보상으로 코인 아이템 드롭
        GameMgr.Inst.SpawnCoin(this.transform.position);
        //--- 보상으로 코인 아이템 드롭

    }//void MonsterDie()

    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        //각종 변수 초기화
        isDie = false;
        hp = 100;
        gameObject.tag = "MONSTER";
        monsterState = MonsterState.idle;

        m_Rigid.useGravity = true;

        //몬스터에 추가된 Collider을 다시 활성화
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

        foreach(Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = true;
        }

        //몬스터를 비활성화
        gameObject.SetActive(false);
    }//IEnumerator PushObjectPool()

    void CreateBloodEffect(Vector3 pos)
    {
        //혈흔 효과 생성
        GameObject blood1 = (GameObject)Instantiate(bloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        //데칼 생성 위치 - 바닥에서 조금 올린 위치 산출
        Vector3 decalPos = transform.position + (Vector3.up * 0.05f);
        //데칼의 회전값을 무작위로 설정
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        //데칼 프리팹 생성
        GameObject blood2 = (GameObject)Instantiate(bloodDecal, decalPos, decalRot);
        //데칼의 크기도 불규칙적으로 나타나게끔 스케일 조정
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        //5초 후에 혈흔효과 프리팹을 삭제
        Destroy(blood2, 5.0f);
    }

    //플레이어가 사망했을 때 실행되는 함수
    void OnPlayerDie()
    {
        //몬스터의 상태를 체크하는 코루틴 함수를 모두 정지시킴
        StopAllCoroutines();
        //추적을 정지하고 애니메이션을 수행
        animator.SetTrigger("IsPlayerDie");
    }

    void FireUpdate()
    {
        Vector3 a_PlayerPos = playerTr.position;
        a_PlayerPos.y = a_PlayerPos.y + 1.5f;
        Vector3 a_MonPos = transform.position;
        a_MonPos.y = a_MonPos.y + 1.5f;
        Vector3 a_CacDist = a_PlayerPos - a_MonPos;
        Vector3 a_H_Dist = a_CacDist;
        float a_RayUDLimit = 3.0f;

        bool isRayOK = false;
        if(Physics.Raycast(a_MonPos, a_H_Dist.normalized,
                            out RaycastHit hit, 100.0f, m_LaserMask) == true)
        {
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
            {
                isRayOK = true;
            }
        }

        if(isRayOK == true && traceDist < a_H_Dist.magnitude)
        {
            if(-a_RayUDLimit <= a_CacDist.y && a_CacDist.y <= a_RayUDLimit)
            { //높이값 한계치
                //--- 몬스터가 주인공을 공격하면서 바라 보도록 해야 한다.
                Vector3 a_CacVLen = playerTr.position - transform.position;
                a_CacVLen.y = 0.0f;
                Quaternion a_TargetRot =
                                Quaternion.LookRotation(a_CacVLen.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        a_TargetRot, Time.deltaTime * 6.0f);
            }
        }//if(isRayOK == true && traceDist < a_H_Dist.magnitude)

        m_BLTime = m_BLTime - Time.deltaTime;
        if(m_BLTime <= 0.0f)
        {
            m_BLTime = 0.0f;

            if (a_H_Dist.magnitude <= traceDist) //추적 거리 안쪽이면 그냥 추적
                return; //추적 거리 밖에 있으면 총알을 쏘게 하기 위한 코드

            if (a_CacDist.y < -a_RayUDLimit || a_RayUDLimit < a_CacDist.y)
                return; //조준의 기준 위, 아래 -3 ~ 3 높이로 제한 걸어줌
            //1층에 있는 몬스터가 2층에 있는 주인공을 조준하지는 못하게 하기 위해서...

            if (isRayOK == false) //몬스터와 주인공 사이에 벽이 있으면 제외
                return;

            Vector3 a_StartPos = a_MonPos + a_CacDist.normalized * 1.5f;
            GameObject a_Bullet = Instantiate(bullet, a_StartPos, Quaternion.identity);
            a_Bullet.layer = LayerMask.NameToLayer("E_BULLET");
            a_Bullet.tag = "E_BULLET";
            a_Bullet.transform.forward = a_CacDist.normalized;

            float a_CacDF = (GlobalValue.g_CurBlockNum - 10) * 0.012f;
            if (a_CacDF < 0.0f)
                a_CacDF = 0.0f;
            if (1.0f < a_CacDF)
                a_CacDF = 1.0f; 

            m_BLTime = 2.0f - a_CacDF;

        }//if(m_BLTime <= 0.0f)

    }//void FireUpdate()

    public void TakeDamage(int a_Value)
    {
        if (hp <= 0.0f)
            return;

        //혈흔 효과 함수 호출
        CreateBloodEffect(transform.position);

        hp -= a_Value;
        if (hp <= 0)
        {
            hp = 0;
            MonsterDie();
        }
        animator.SetTrigger("IsHit");
    }

}//public class MonsterCtrl
