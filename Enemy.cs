using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AI;
using System;
using Random = UnityEngine.Random;
using System.Net.NetworkInformation;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

public class Enemy : MonoBehaviour
{

    // 포트폴리오 작성을 위한 Skill 부분에 사용했던 함수 부분만 남겨놓은 Script 입니다.
    
    // ---------------------------------------------------------------------------변수 -----------------------------------------------------------------------------------
    public enum Type { Goblin, Golem, Mage, Ghost, Boomer, InstantMelee, InstantRage, Skeleton, BossGolin, BossMage };
    public Type enemyType;
    public int maxHealth; //최대체력
    public int curHealth; //현재체력
    public BoxCollider meleeArea;
    public GameObject attackAlert;
    public GameObject magic;
    public GameObject itemMoney;
    public GameObject itemHeart;
    public GameObject itemShild;
    public GameObject itemWeapon;
    public GameObject itemRange;
    public GameObject target;
    public bool isChase;
    public bool isAttack;
    public bool isDead;
    public bool isGetHit;
    public const int unarmDamage = 5;
    public GameObject HPbar;

    public AudioClip audioAttack;
    public AudioClip audioHit;
    public AudioClip audioDeath;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator anim;
    public Vector3 orgPos;
    public Player_knights pUser;
    public AudioSource audio;
    // ---------------------------------------------------------------------------변수 -----------------------------------------------------------------------------------
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        audio = GetComponent<AudioSource>();
        pUser = GameObject.FindWithTag("Player").GetComponent<Player_knights>();
        if(transform.Find("AttackAlert").gameObject != null)
        {
            attackAlert = transform.Find("AttackAlert").gameObject;
        }
        else
        {
            attackAlert = transform.Find("attackAlert").gameObject;
        }
        

        if(enemyType != Type.BossGolin)
        Invoke("ChaseStart", 1f);
    }




    void OnTriggerEnter(Collider other)
    {
        if (isGetHit)
            return;

        Vector3 reactVec = new Vector3();
        bool isMagic = false;
        switch (other.tag)
        {
            case "Unarm": // 맨손 공격
                {
                    {

                        isGetHit = true; // 트리거가 2번연속 동작하지 않게 
                        Invoke("GetHitOut", 0.3f);
                        curHealth -= unarmDamage; //데미지 입음
                        anim.SetTrigger("doGetHit");
                        PlaySound("Hit");
                        nav.enabled = false; // 데미지 입을 시 멈추게
                        Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                        isMagic = false;
                        reactVec = transform.position - other.transform.position; //넉백
                        
                    }
                        break;
                }
            case "Melee": //웨펀 공격
                {
                    {
                        Weapon weapon = other.GetComponent<Weapon>(); //근접무기 공격시
                        isGetHit = true; // 트리거가 2번연속 동작하지 않게 
                        Invoke("GetHitOut", 0.5f);
                        curHealth -= weapon.damage; //데미지 입음
                        anim.SetTrigger("doGetHit");
                        PlaySound("Hit");
                        nav.enabled = false; // 데미지 입을 시 멈추게
                        Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                        isMagic = false;
                        reactVec = transform.position - other.transform.position; //넉백
                        
                    }
                    break;
                }
            case "Giant":
                {
                    {
                        isGetHit = true; // 트리거가 2번연속 동작하지 않게 
                        Invoke("GetHitOut", 0.5f);
                        curHealth -= 50; //데미지 입음
                        anim.SetTrigger("doGetHit");
                        PlaySound("Hit");
                        nav.enabled = false; // 데미지 입을 시 멈추게
                        Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                        isMagic = true;
                        reactVec = transform.position - other.transform.position; //넉백
                        
                    }
                    break;
                }
            case "Berserker":
                {
                    {
                        isGetHit = true; // 트리거가 2번연속 동작하지 않게 
                        Invoke("GetHitOut", 0.5f);
                        curHealth -= 20; //데미지 입음
                        pUser.Heart += 20;
                        anim.SetTrigger("doGetHit");
                        PlaySound("Hit");
                        nav.enabled = false; // 데미지 입을 시 멈추게
                        Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                        isMagic = true;
                        reactVec = transform.position - other.transform.position; //넉백

                        
                    }
                    break;
                }

        }

        StartCoroutine(OnDamage(reactVec, isMagic));


    }

    public void HitByMagic(Vector3 explosionPos, string skillName, int damage = 0)
    {
        isHit();
        switch (skillName)
        {
            case "Bomb":
            {                
                curHealth -= 100; //데미지 입음                
                Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                Vector3 reatVec = transform.position - explosionPos;

                StartCoroutine(OnDamage(reatVec, true));
                    break;
            }

            case "Void":
            {
                curHealth -= 10; //데미지 입음                
                Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                Vector3 reatVec = transform.position - explosionPos;
                reatVec.y = 0;
                rigid.AddForce(reatVec / reatVec.magnitude * -300, ForceMode.Impulse);
                transform.LookAt(explosionPos);

                StartCoroutine(OnDamage(reatVec, true));
                    break;
        }

            case "Reflect":
            {
                curHealth -= damage; //데미지 입음                
                Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                Vector3 reatVec = transform.position - explosionPos;

                StartCoroutine(OnDamage(reatVec, true));
                    break;
            }

            case "AuraSword":
            {
                curHealth -= 10; //데미지 입음               
                Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                Vector3 reatVec = transform.position - explosionPos;

                StartCoroutine(OnDamage(reatVec, true));
                    break;
            }

            case "WaterPool":
            {                
                curHealth -= 15; //데미지 입음                
                Invoke("MoveStop", 0.7f); // 다시 움직일 수 있게
                Vector3 reatVec = transform.position - explosionPos;

                StartCoroutine(OnDamage(reatVec, true));
                    break;
            }
        }
    }

    public void isHit()
    {
        isGetHit = true; // 트리거가 2번연속 동작하지 않게 
        Invoke("GetHitOut", 0.5f);        
        anim.SetTrigger("doGetHit");
        PlaySound("Hit");
        nav.enabled = false; // 데미지 입을 시 멈추게
    }
    IEnumerator OnDamage(Vector3 reactVec, bool isMagic)
    {

        if (curHealth > 0 )
        {
            
        }
        else
        {
            isDead = true;
            gameObject.layer = 12;
            isChase = false; //사망시 네비게이션 꺼짐
            nav.enabled = false;
            anim.SetTrigger("doDie"); //사망시 사망모션 출력
            PlaySound("Death");

            if (isMagic)
            {

                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;

                rigid.AddForce(reactVec * +15, ForceMode.Impulse); //죽을시 넉백
                rigid.AddTorque(reactVec * +15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 15, ForceMode.Impulse); //죽을시 넉백
            }


            Destroy(gameObject, 3);
            

            int ran = Random.Range(0, 10);
            if (enemyType == Type.Goblin)
            {
                if (ran < 8.5f) //Money 85퍼
                {
                    Instantiate(itemMoney, transform.position, itemMoney.transform.rotation);
                }
                else if (ran < 10f) //Heart 15퍼
                {
                    Instantiate(itemHeart, transform.position, itemHeart.transform.rotation);
                }
            }
            else if (enemyType == Type.Mage)
            {
                if (ran < 8.5f) //Money 85퍼
                {
                    Instantiate(itemMoney, transform.position, itemMoney.transform.rotation);
                }
                else if (ran < 9.4f) //Heart 9퍼
                {
                    Instantiate(itemHeart, transform.position, itemHeart.transform.rotation);
                }
                else if (ran < 10f) //Weapon 6퍼 차후 세분화
                {
                    Instantiate(itemWeapon, transform.position + new Vector3(0, 0.5f, 0), itemWeapon.transform.rotation);
                }
            }
            else if (enemyType == Type.Ghost)
            {
                if (ran < 5f) //Money 50퍼
                {
                    Instantiate(itemMoney, transform.position, itemMoney.transform.rotation);
                }
                else if (ran < 10f) //Heart 50퍼
                {
                    Instantiate(itemHeart, transform.position, itemHeart.transform.rotation);
                }
            }
            else if (enemyType == Type.Golem)
            {
                if (ran < 8.5f) //Money 85퍼
                {
                    Instantiate(itemMoney, transform.position, itemMoney.transform.rotation) ;
                }
                else if (ran < 9.6f) //Heart 11퍼
                {
                    Instantiate(itemHeart, transform.position, itemHeart.transform.rotation);
                }
                else if (ran < 10f) //Shild 4퍼
                {
                    Instantiate(itemWeapon, transform.position + new Vector3(0, 0.5f, 0), itemWeapon.transform.rotation);
                }
            }
            if (enemyType != Type.BossGolin)
                Destroy(gameObject, 1.2f);
        }
        yield return new WaitForSeconds(0.1f);
    }
