using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public enum SkillName { Berserker, Giant, Bomb, Void, Heal, Reflect, AuraSword, WaterPool };
    public SkillName skillName;  
    public GameObject skillEffect;
    public Sprite skillIcon;
    public Image c_skillIcon;

    private float cooltime;
    private float runtime;
    private bool useSkill=true;
    Player_knights pUser;

    
    void Start()
    {
        pUser = Player_knights.instance;
        c_skillIcon = GameObject.Find("Cooltime").GetComponent<Image>();
    }
   
    void Update()
    {
        Cooltime();        
        
    }

    public void Cooltime ()
    {
        runtime += Time.deltaTime*1;
        c_skillIcon.fillAmount = Mathf.Clamp (((cooltime - runtime) / cooltime),0,1);
        if (useSkill)
            return;

        if (cooltime - runtime <= 0)
            useSkill = true;
    }

    public void Use()
    {
        if (!useSkill)
            return;
        string name = typeof(SkillName).ToString();
        StartCoroutine(name);
        runtime = 0;
        useSkill = false;
    }

    IEnumerator Giant() // 거대화 : 플레이어 캐릭터 오브젝트의 Scale을 확대, 발 부분의 트리거를 활성화해 Enemy 오브젝트들이 경우 데미지를 입히는 기술
    {
        if (pUser.MP >= 50)
        {
            cooltime = 20.0f;
            pUser.MP -= 50;
            pUser.gameObject.tag = "Giant";
            pUser.isGiant = true;
            pUser.transform.localScale += new Vector3(10, 10, 10);
            pUser.leftGiantArea.enabled = true;
            pUser.RightGiantArea.enabled = true;
            yield return new WaitForSeconds(10f);            
            pUser.gameObject.tag = "Untagged";
            pUser.isGiant = false;
            pUser.transform.localScale -= new Vector3(10, 10, 10);
            pUser.leftGiantArea.enabled = false;
            pUser.RightGiantArea.enabled = false;
        }
    }

    IEnumerator Bomb() // 폭탄 : 마우스 포인트 부분에 폭발을 일으켜 범위에 있는 Enemy 오브젝트들에 데미지를 입히는 기술 
    {
        if (pUser.MP >= 20 )
        {
            cooltime = 10.0f;
            pUser.MP -= 20;
            Ray ray = pUser.followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point;
                nextVec.y = 0;
                GameObject instantEffect = Instantiate(skillEffect, nextVec, transform.rotation);
                RaycastHit[] rayHits = Physics.SphereCastAll(nextVec, 15, Vector3.up, 0, LayerMask.GetMask("Enemy"));

                foreach (RaycastHit hitObject in rayHits)
                {
                    hitObject.transform.GetComponent<Enemy>().HitByMagic(nextVec, "Bomb");
                }
                Destroy(instantEffect, 5f);

            }
            yield return null;
        }
    }

    IEnumerator Berserker() // 버서커 : 데미지 입히는 부분에서 해당 기술이 켜져 있다면 플레이어의 체력을 데미지 만큼 회복하는 기술 
    {
        if(pUser.MP >= 20)
        {
            cooltime = 15.0f; 
            pUser.MP -= 20;
            gameObject.tag = "Berserker";
            pUser.BerserkerEffect.SetActive(true);
            yield return new WaitForSeconds(10f);
            gameObject.tag = "Melee";
            pUser.BerserkerEffect.SetActive(false);
        }
    }

    IEnumerator Void() // 보이드 : 마우스 포인트 부분에 마법진을 생성하여 범위내에 있는 Enemy 오브젝트들에 데미지를 입히며 가운데 부분으로 끌어당기는 기술 
    {
        if(pUser.MP >= 40)
        {
            cooltime = 15.0f;
            pUser.MP -= 40;
            Ray ray = pUser.followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point;
                nextVec.y = 0;

                GameObject instantEffect = Instantiate(skillEffect, nextVec, transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0)));
                for (int i = 0; i < 5; i++)
                {
                    RaycastHit[] rayHits = Physics.SphereCastAll(nextVec, 30, Vector3.up, 0, LayerMask.GetMask("Enemy"));// 해당 포지션 주변의 오브젝트들을 탐색하기 위해 up으로 레이를 쏨

                    foreach (RaycastHit hitObject in rayHits)
                    {
                        hitObject.transform.GetComponent<Enemy>().HitByMagic(nextVec, "Void");
                    }
                    yield return new WaitForSeconds(1f);
                }
                Destroy(instantEffect);

            }

            yield return new WaitForSeconds(0f);
        }

    }

    IEnumerator Heal() // 힐 : 플레이어의 체력을 회복시키는 기술 
    {
        if(pUser.MP >= 30)
        {
            cooltime = 8.0f;
            pUser.MP -= 30;
            pUser.HealEffect.SetActive(true);
            pUser.heart = pUser.maxHeart;
            yield return new WaitForSeconds(3f);
            pUser.HealEffect.SetActive(false);
        }
    }

    IEnumerator Reflect() // 반사 : 플레이어가 피해를 입을 때, 해당 기술이 활성화 되어 있다면 입은 피해만큼 피해를 준 Enemy 오브젝트에 되돌려 주는 기술 
    {
        if (pUser.MP >= 100)
        {
            cooltime = 20.0f;
            pUser.gameObject.tag = "Reflect";
            pUser.ReflectEffect.SetActive(true);
            yield return new WaitForSeconds(8f);
            pUser.gameObject.tag = "Untagged";
            pUser.ReflectEffect.SetActive(false);
        }
    }

    IEnumerator AuraSword() // 오라 : 플레이어 주변으로 검 오브젝트들이 회전하며 해당 오브젝트에 적이 닿을 경우 Enemy 오브젝트에 데미지를 입히는 기술 
    {
        if (pUser.MP >= 60)
        {
            cooltime = 15.0f;
            pUser.MP -= 60;

            
            pUser.AuraSwordEffect.SetActive(true);
            SoundManager.instance.SFXPlay("AuraSword", pUser.auraSwordSound, true, 6.5f);

            for (int i = 0; i < 14; i++)
            {
                Vector3 nextVec = gameObject.transform.position;
                RaycastHit[] rayHits = Physics.SphereCastAll(nextVec, 5, Vector3.up, 0, LayerMask.GetMask("Enemy"));
                foreach (RaycastHit hitObject in rayHits)
                {
                    hitObject.transform.GetComponent<Enemy>().HitByMagic(nextVec, "AuraSword");
                }


                yield return new WaitForSeconds(0.5f);
            }
            pUser.AuraSwordEffect.SetActive(false);
        }
    }

    IEnumerator WaterPool() // 물 웅덩이 : 보이드와 비슷한 형태
    {
        if (pUser.MP >= 50)
        {
            cooltime = 15.0f;
            pUser.MP -= 50;
            Ray ray = pUser.followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point;
                nextVec.y = 0;

                GameObject instantEffect = Instantiate(skillEffect, nextVec, transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0)));
                for (int i = 0; i < 4; i++)
                {
                    RaycastHit[] rayHits = Physics.SphereCastAll(nextVec, 40, Vector3.up, 0, LayerMask.GetMask("Enemy"));

                    foreach (RaycastHit hitObject in rayHits)
                    {
                        hitObject.transform.GetComponent<Enemy>().HitByMagic(nextVec, "WaterPool");
                    }
                    yield return new WaitForSeconds(2f);
                }
                Destroy(instantEffect);

            }

            yield return new WaitForSeconds(0f);
        }

    }



}
