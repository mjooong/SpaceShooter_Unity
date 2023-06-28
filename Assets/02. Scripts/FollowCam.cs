using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SideWall
{
    public bool m_IsColl = false;
    public GameObject m_SideWalls = null;
    public Material m_WallMaterial = null;

    public SideWall()
    {
        m_IsColl = false;
        m_SideWalls = null;
        m_WallMaterial = null;
    }
}

public class FollowCam : MonoBehaviour
{
    public Transform targetTr;      //추적할 타깃 게임오브젝트의 Transform 변수
    public float dist = 10.0f;      //카메라와의 일정 거리
    public float height = 3.0f;     //카메라의 높이 설정
    public float dampTrace = 20.0f; //부드러운 추적을 위한 변수

    Vector3 m_PlayerVec = Vector3.zero;
    float rotSpeed = 10.0f;

    //--- Side Wall 리스트 관련 변수
    Vector3 a_CacVLen = Vector3.zero;
    Vector3 a_CacDirVec = Vector3.zero;

    GameObject[] a_SideWalls = null;
    [HideInInspector] public LayerMask m_WallLyMask = -1;
    List<SideWall> m_SW_List = new List<SideWall>();
    //--- Side Wall 리스트 관련 변수

    //--- 캐릭터 변경 관련 변수
    public GameObject[] CharObjects;    //캐릭터 종류
    int    CharType = 0;
    //--- 캐릭터 변경 관련 변수

    //--- 카메라 위치 계산용 변수
    float m_RotV = 0.0f;        //마우스 상하 조작값 계산용 변수
    float m_DefaltRotV = 25.2f; //높이 기준의 회전 각도
    float m_MarginRotV = 22.3f; //총구와의 마진 각도
    float m_MinLimitV = -17.9f; //위 아래 각도 제한
    float m_MaxLimitV = 52.9f;  //위 아래 각도 제한
    float maxDist     = 4.0f;   //마우스 줌 아웃 최대 거리 제한값
    float minDist     = 2.0f;   //마우스 줌 인 최대 거리 제한값
    float zoomSpeed   = 0.7f;   //마우스 휠 조작에 대한 줌인아웃 스피트 설정값

    Quaternion a_BuffRot;       //카메라 회전 계산용 변수
    Vector3    a_BuffPos;       //카메라 회전에 대한 위치 좌표 계산용 변수
    Vector3    a_BasicPos = Vector3.zero;  //위치 계산용 변수
    //--- 카메라 위치 계산용 변수

    //--- 총 조준 방향 계산용 변수
    public static Vector3 m_RifleDir = Vector3.zero;    //총 조준 방향
    Quaternion a_RFCacRot;
    Vector3 a_RFCacPos = Vector3.forward;
    //--- 총 조준 방향 계산용 변수

    // Start is called before the first frame update
    void Start()
    {
        dist = 3.4f;
        height = 2.8f;

        //--- Side Wall 리스트에 만들기...
        m_WallLyMask = 1 << LayerMask.NameToLayer("SideWall");
        //"SideWall"번 레이어만 선택적으로 레이케스트(레이저광선) 체크 위해...

        a_SideWalls = GameObject.FindGameObjectsWithTag("SideWall");
        if(0 < a_SideWalls.Length)
        {
            SideWall a_SdWall = null;
            for(int ii = 0; ii < a_SideWalls.Length; ii++)
            {
                a_SdWall = new SideWall();
                a_SdWall.m_IsColl = false;
                a_SdWall.m_SideWalls = a_SideWalls[ii];
                a_SdWall.m_WallMaterial =
                    a_SideWalls[ii].GetComponent<Renderer>().material;
                WallAlphaOnOff(a_SdWall.m_WallMaterial, false);
                m_SW_List.Add(a_SdWall);
            }//for(int ii = 0; ii < a_SideWalls.Length; ii++)
        }//if(0 < a_SideWalls.Length)
        //--- Side Wall 리스트에 만들기...

        //--- 카메라 위치 계산
        m_RotV = m_DefaltRotV;  //높이 기준의 회전 각도
        //--- 카메라 위치 계산

        if (SceneManager.GetActiveScene().name == "scLevel02")
            m_RotV = 10.2f;

    }//void Start()

    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        {
            if (GameMgr.IsPointerOverUIObject() == true)
                return;

            ////--- 카메라 위아래 바라보는 각도 조절을 위한 코드
            //height -= (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));
            //if (height < 0.1f)
            //    height = 0.1f;

            //if (5.7f < height)
            //    height = 5.7f;
            ////--- 카메라 위아래 바라보는 각도 조절을 위한 코드

            //--- (구좌표계를 직각좌표계로 환산하는 부분)
            rotSpeed = 235.0f;  //카메라 위아래 회전 속도
            m_RotV -= (rotSpeed * Time.deltaTime * Input.GetAxisRaw("Mouse Y"));
            //마우스를 위아래로 움직였을 때 값
            if (m_RotV < m_MinLimitV)
                m_RotV = m_MinLimitV;
            if (m_MaxLimitV < m_RotV)
                m_RotV = m_MaxLimitV;
            //--- (구좌표계를 직각좌표계로 환산하는 부분)
        }//if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)

        //--- 카메라 줌인 줌아웃
        if(Input.GetAxis("Mouse ScrollWheel") < 0 && dist < maxDist)
        {
            dist += zoomSpeed;
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0 && dist > minDist)
        {
            dist -= zoomSpeed;
        }
        //--- 카메라 줌인 줌아웃

        //--- Rifle 방향 계산
        a_RFCacRot = Quaternion.Euler(
                        Camera.main.transform.eulerAngles.x - m_MarginRotV,
                        targetTr.eulerAngles.y,
                        0.0f);
        m_RifleDir = a_RFCacRot * a_RFCacPos;
        //--- Rifle 방향 계산

    }//void Update()

    //Update 함수 호출 이후 한 번씩 호출되는 함수인 LateUpdate 사용
    //추적할 타깃의 이동이 종료된 이후에 카메라가 추적하기 위해 LateUpdate 사용
    // Update is called once per frame
    void LateUpdate()
    {
        m_PlayerVec = targetTr.position;
        m_PlayerVec.y = m_PlayerVec.y + 1.2f;

        ////--- 카메라 위치 잡아 주는 절대강좌 소스 
        ////카메라의 위치를 추적대상의 dist 변수만큼 뒤쪽으로 배치하고
        ////height 변수만큼 위로 올림
        //transform.position = Vector3.Lerp(transform.position,
        //                                    targetTr.position
        //                                    - (targetTr.forward * dist)
        //                                    + (Vector3.up * height),
        //                                    Time.deltaTime * dampTrace);
        ////--- 카메라 위치 잡아 주는 절대강좌 소스 

        //-- (구좌표계를 직각좌표계로 환산하는 부분)
        a_BuffRot = Quaternion.Euler(m_RotV, targetTr.eulerAngles.y, 0.0f);
        a_BasicPos.x = 0.0f;
        a_BasicPos.y = 0.0f;
        a_BasicPos.z = -dist;
        a_BuffPos = m_PlayerVec + (a_BuffRot * a_BasicPos);
        transform.position = Vector3.Lerp(transform.position, a_BuffPos,
                                                Time.deltaTime * dampTrace);
        //-- (구좌표계를 직각좌표계로 환산하는 부분)

        //카메라가 타깃 게임오브젝트를 바라보게 설정
        transform.LookAt(m_PlayerVec);  //targetTr.position);

        //--- Wall 카메라 레이저 충돌 처리 부분
        a_CacVLen = this.transform.position - targetTr.position;
        a_CacDirVec = a_CacVLen.normalized;
        GameObject a_FindObj = null;
        RaycastHit a_hitInfo;
        if(Physics.Raycast(targetTr.position + (-a_CacDirVec * 1.0f),
                a_CacDirVec, out a_hitInfo, a_CacVLen.magnitude + 4.0f,
                m_WallLyMask.value))
        {
            a_FindObj = a_hitInfo.collider.gameObject;
        }

        for (int ii = 0; ii < m_SW_List.Count; ii++)
        {
            if (m_SW_List[ii].m_SideWalls == null)
                continue;

            if (m_SW_List[ii].m_SideWalls == a_FindObj) //투명시켜야 할 벽
            {
                if (m_SW_List[ii].m_IsColl == false)
                {
                    WallAlphaOnOff(m_SW_List[ii].m_WallMaterial, true);
                    m_SW_List[ii].m_IsColl = true;
                }
            }//if(m_SW_List[ii].m_SideWalls == a_FindObj)
            else  //투명화 시키지 말아야 할 벽
            {
                if (m_SW_List[ii].m_IsColl == true)
                {
                    WallAlphaOnOff(m_SW_List[ii].m_WallMaterial, false);
                    m_SW_List[ii].m_IsColl = false;
                }
            }
        }//for(int ii = 0; ii < m_SW_List.Count; ii++)
         //----- Wall 카메라 레이저 충돌 처리 부분

        //if(Input.GetKeyDown(KeyCode.C) == true)
        //{
        //    CharacterChange();
        //}//if(Input.GetKeyDown(KeyCode.C) == true)

        //--- Rifle 방향 계산
        a_RFCacRot = Quaternion.Euler(
                        Camera.main.transform.eulerAngles.x - m_MarginRotV,
                        targetTr.eulerAngles.y,
                        0.0f);
        m_RifleDir = a_RFCacRot * a_RFCacPos;
        //--- Rifle 방향 계산

    }//void LateUpdate()

    void WallAlphaOnOff(Material mtrl, bool isOn = true)
    {
        if(isOn == true)
        {
            mtrl.SetFloat("_Mode", 3);  //Transparent
            mtrl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mtrl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mtrl.SetInt("_ZWrite", 0);
            mtrl.DisableKeyword("_ALPHATEST_ON");
            mtrl.DisableKeyword("_ALPHABLEND_ON");
            mtrl.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mtrl.renderQueue = 3000;
            mtrl.color = new Color(1, 1, 1, 0.3f);
        }
        else
        {
            mtrl.SetFloat("_Mode", 0);  //Opaque
            mtrl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mtrl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mtrl.SetInt("_ZWrite", 1);
            mtrl.DisableKeyword("_ALPHATEST_ON");
            mtrl.DisableKeyword("_ALPHABLEND_ON");
            mtrl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mtrl.renderQueue = -1;
            mtrl.color = new Color(1, 1, 1, 1);
        }

    }//void WallAlphaOnOff(Material mtrl, bool isOn = true)

    void CharacterChange()
    {
        Vector3 a_Pos    = CharObjects[CharType].transform.position;
        Quaternion a_Rot = CharObjects[CharType].transform.rotation;
        CharObjects[CharType].SetActive(false);
        CharType++;
        if (1 < CharType)
            CharType = 0;
        CharObjects[CharType].SetActive(true);
        CharObjects[CharType].transform.position = a_Pos;
        CharObjects[CharType].transform.rotation = a_Rot;
        targetTr = CharObjects[CharType].transform;
    }
}
