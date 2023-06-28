using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    //�Ѿ��� �ı���
    public int damage = 20;
    //�Ѿ� �߻� �ӵ�
    public float speed = 1000.0f;

    //����ũ ��ƼŬ ������ ������ ����
    public GameObject sparkEffect;

    // Start is called before the first frame update
    void Start()
    {
        speed = 3000.0f;

        if (gameObject.tag == "E_BULLET") //���Ͱ� �� �Ѿ��� ��
        {
            //--- ���̵� 4������ 15�� �þ���� ... 3000���� ���� (�Ѿ��� �̵��ӵ�)
            float a_CacSpeed = (GlobalValue.g_CurBlockNum - 3) * 15.0f;
            if (a_CacSpeed < 0.0f)
                a_CacSpeed = 0.0f;
            a_CacSpeed = 800.0f + a_CacSpeed;
            if (3000.0f < a_CacSpeed)
                a_CacSpeed = 3000.0f;   //3000.0f ������...
            //--- ���̵� 4������ 15�� �þ���� ... 3000���� ���� (�Ѿ��� �̵��ӵ�)

            speed = a_CacSpeed;
        }
        else  //���ΰ��� �� �Ѿ��� �� 
        {            
            transform.forward = FollowCam.m_RifleDir.normalized;
        }

        GetComponent<Rigidbody>().AddForce(transform.forward * speed);

        Destroy(this.gameObject, 4.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // �Ѿ��� ������ �浹�Ǿ��� �� ȣ��Ǵ� �Լ�
    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.name.Contains("Player") == true)
            return;

        if (coll.gameObject.name.Contains("Barrel") == true)
            return;

        if (coll.gameObject.name.Contains("Monster_") == true)
            return;

        if (coll.collider.tag == "SideWall")
            return;

        if (coll.collider.tag == "BULLET")
            return;

        if (coll.collider.tag == "E_BULLET")
            return;

        //����ũ ��ƼŬ�� �������� ����
        GameObject spark = Instantiate(sparkEffect,
                                        transform.position, Quaternion.identity);

        //ParticleSystem ������Ʈ�� ����ð�(duration)�� ���� �� ���� ó��
        Destroy(spark, spark.GetComponent<ParticleSystem>().main.duration + 0.2f);

        //�� �Ѿ� ���ӿ�����Ʈ ����
        Destroy(gameObject);
    }
}