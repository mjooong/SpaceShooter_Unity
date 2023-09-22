using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; //UI �׸� �����ϱ� ���� �ݵ�� �߰�

//Ŭ������ System.Serializable�̶�� ��Ʈ����Ʈ(Attribute)�� ����ؾ� Inspector �信 �����
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

    // �̵� �ӵ� ����
    public float moveSpeed = 10.0f;

    // ȸ�� �ӵ� ����
    public float rotSpeed = 100.0f;

    //�ν����ͺ信 ǥ���� �ִϸ��̼� Ŭ���� ����
    public Anis anim;

    //�Ʒ��� �ִ� 3D ���� Animation ������Ʈ�� �����ϱ� ���� ����
    public Animation _animation;

    //Player�� ���� ����
    public int hp = 100;
    //Player�� ���� �ʱ갪
    private int initHp;
    //Player�� Health bar �̹���
    public Image imgHpbar;

    CharacterController m_ChrController; //���� ĳ���Ͱ� ������ �ִ� ĳ���� ��Ʈ�ѷ�

    public GameObject bloodEffect;  //���� ȿ�� ������

    FireCtrl m_FireCtrl = null;

    //--- ���� ��ų
    float m_SdDuration = 20.0f;
    float m_SdOnTime = 0.0f;
    public GameObject ShieldObj = null;
    //--- ���� ��ų

    [Header("------ Sound ------")]
    public AudioClip m_CoinSdClip;
    public AudioClip m_DiamondSdClip;
    AudioSource Ad_Source = null;

    //--- Door ���� ����
    bool isShift = false;
    float m_CkTimer = 0.3f; //������ �ε� ��� �浹�Ǵ� ������ �����ϱ� ���� Ÿ�̸�
    //--- Door ���� ����

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;  //����� �ֻ���(�÷�����)�� �ٸ� ��ǻ���� ��� ĳ���� ���۽� ������ ������ �� �ִ�.
        Application.targetFrameRate = 60; //���� ������ �ӵ� 60���������� ���� ��Ű��.. �ڵ�

        //���� �ʱ갪 ����
        initHp = hp;

        //�ڽ��� ������ �ִ� Animation ������Ʈ�� ã�ƿ� ������ �Ҵ�
        _animation = GetComponentInChildren<Animation>();

        //Animation ������Ʈ�� �ִϸ��̼� Ŭ���� �����ϰ� ����
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

        //�����¿� �̵� ���� ���� ���
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        if (1.0f < moveDir.magnitude)
            moveDir.Normalize();

        ////Translate(�̵����� * Time.deltaTime * ������ * �ӵ�, ������ǥ)
        //transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);

        if(m_ChrController != null)
        {
            // ���͸� ���� ��ǥ�� ���ؿ��� ���� ��ǥ�� �������� ��ȯ�Ѵ�.
            moveDir = transform.TransformDirection(moveDir);

            m_ChrController.SimpleMove(moveDir * moveSpeed);
        }

        if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        if (GameMgr.IsPointerOverUIObject() == false)
        {
            //Vector3.up ���� �������� rotSpeed ��ŭ�� �ӵ��� ȸ��
            transform.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X") * 3.0f);
        }

        //Ű���� �Է°��� �������� ������ �ִϸ��̼� ����
        if(v >= 0.1f)
        {
            //���� �ִϸ��̼�
            _animation.CrossFade(anim.runForward.name, 0.3f);
        }
        else if(v <= -0.1f)
        {
            //���� �ִϸ��̼�
            _animation.CrossFade(anim.runBackward.name, 0.3f);
        }
        else if(h >= 0.1f)
        {
            //������ �̵� �ִϸ��̼�
            _animation.CrossFade(anim.runRight.name, 0.3f);

        }
        else if(h <= -0.1f)
        {
            //���� �̵� �ִϸ��̼�
            _animation.CrossFade(anim.runLeft.name, 0.3f);
        }
        else
        {
            //������ idle �ִϸ��̼�
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

    //�浹�� Collider�� OsTrigger �ɼ��� üũ���� �� �߻�
    void OnTriggerEnter(Collider coll)
    {
        //�浹�� Collider�� ������ PUNCH�̸� Player�� HP����
        if(coll.gameObject.tag == "PUNCH")
        {
            if (0.0f < m_SdOnTime) //���� �ߵ� ���̸�...
                return;

            if (hp <= 0.0f)
                return;

            hp -= 10;

            //Image UI �׸��� fillAmount �Ӽ��� ������ ���� ������ �� ����
            imgHpbar.fillAmount = (float)hp / (float)initHp;

            //Debug.Log("Player HP = " + hp.ToString());

            //Player�� ������ 0�����̸� ��� ó��
            if(hp <= 0)
            {
                PlayerDie();
            }
        }//if(coll.gameObject.tag == "PUNCH")
        else if(coll.gameObject.name.Contains("CoinPrefab") == true)
        {
            //--- ���� ������ �ö� ������ ���� ������ ������ ���� �� �ְ� �ؼ� �����ǽ��� �ش�.
            int a_CacGold = (GlobalValue.g_CurBlockNum - 5) * 2;
            if (a_CacGold < 0)
                a_CacGold = 0;
            a_CacGold = 10 + a_CacGold; //�⺻ 10��
            if (200 < a_CacGold)
                a_CacGold = 200;    //�ִ� 200�������� ���� ����
            //--- ���� ������ �ö� ������ ���� ������ ������ ���� �� �ְ� �ؼ� �����ǽ��� �ش�.
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
            GameMgr.Inst.ShowDoor();    //���̾Ƹ�带 �Ծ�� ���� ������.

            if (Ad_Source != null && m_DiamondSdClip != null)
                Ad_Source.PlayOneShot(m_DiamondSdClip, 0.3f);

            Destroy(coll.gameObject);
        }//else if(coll.gameObject.name.Contains("DiamondPrefab") == true)
        else if(coll.gameObject.name.Contains("RTS15_desert") == true)
        {
            if (m_CkTimer <= 0.0f)
                PlayerDie();        //���� ����
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

    void OnCollisionEnter(Collision coll) //�Ѿ� �浹 ó��
    {
        if(coll.gameObject.tag == "E_BULLET")
        {
            //���� ȿ�� ����
            GameObject blood =
                            (GameObject)Instantiate(bloodEffect,
                                coll.transform.position, Quaternion.identity);
            blood.GetComponent<ParticleSystem>().Play();
            Destroy(blood, 3.0f);
            //���� ȿ�� ����

            //Bullet ����
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

    }//void OnCollisionEnter(Collision coll) //�Ѿ� �浹 ó��

    //Plsyer�� ��� ó�� ��ƾ
    void PlayerDie()
    {
        //Debug.Log("Player Die !!");

        //MONSTER��� Tag�� ���� ��� ���ӿ�����Ʈ�� ã�ƿ�
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //��� ������ OnPlayerDie �Լ��� ���������� ȣ��
        foreach(GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
            //monster.GetComponent<MonsterCtrl>().OnPlayerDie();
            //SendMessageOptions.DontRequireReceiver : �ش� �Լ��� ������ �Լ��� ���ٴ�
            //�޽����� ���Ϲ��� �ʰڴٴ� �ɼ��̴�.
        }

        _animation.Stop();

        GameMgr.s_GameState = GameState.GameEnd;
        Time.timeScale = 0.0f;  //�Ͻ�����
        GameMgr.Inst.GameOverFunc();

    }//void PlayerDie()

    void SkillUpdate()
    {
        //---- ���� ���� ������Ʈ
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
        //---- ���� ���� ������Ʈ
    }

    public void UseSkill_Item(SkillType a_SkType)
    {
        if (a_SkType < 0 || SkillType.SkCount <= a_SkType)
            return;

        if(a_SkType == SkillType.Skill_0)   //���� ������
        {
            hp += (int)(initHp * 0.3f);

            //������ �ؽ�Ʈ ����
            GameMgr.Inst.SpawnDamageText((int)(initHp * 0.3f),
                                        transform, new Color(0.18f, 0.5f, 0.34f));
            if (initHp < hp)
                hp = initHp;
            if (imgHpbar != null)
                imgHpbar.fillAmount = hp / (float)initHp;
        }
        else if(a_SkType == SkillType.Skill_1) //����ź
        {
            if (m_FireCtrl != null)
                m_FireCtrl.FireGrenade();
        }
        else if(a_SkType == SkillType.Skill_2) //��ȣ��
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
