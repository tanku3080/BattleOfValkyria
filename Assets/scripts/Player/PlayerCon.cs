﻿using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.UI;
public class PlayerCon : MonoBehaviour
{
    [Tooltip("基本情報")]
    [HideInInspector] public float speed = 0.05f;
    //プレイヤーの最高体力。現在の体力の定数はGameManagerにある
    [HideInInspector] public float HpM { get { return manager.playerHp; } set { HpM = 1000f; } }
    /// <summary>移動制限</summary>
    [HideInInspector]  public float limitDistance;
    float h, v;
    //以下はエネミー
    [HideInInspector] public GameObject Test;
    private float searchTime = 0;
    /// <summary>メイン武器,サブ武器弾数</summary>
    [SerializeField] private AudioClip footSound,RadarSound;
    AudioSource source;
    Animator anime;
    GameManager manager;
    GameObject limited;
    AtackCon atack;
    StatusCon status;
    Rigidbody _rb;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        anime = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        atack = GameObject.Find("GameManager").GetComponent<AtackCon>();
        status = GameObject.Find("GameManager").GetComponent<StatusCon>();
        Test = SerchTag(gameObject);
        limited = GameObject.Find("Limit");//？

    }
    private void FixedUpdate()
    {
        _rb.AddForce(100 * Physics.gravity, ForceMode.Force);
    }
    void Update()
    {
        if (manager.playerSide)
        {
            searchTime += Time.deltaTime;
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            if (searchTime >= 1.0f)
            {
                Test = SerchTag(gameObject);
                source.PlayOneShot(RadarSound);
                searchTime = 0;
            }
            if (status.mouse > 0 || status.mouse < 0) status.ChangeStart();

            if (Input.GetKeyDown(KeyCode.M))
            {
                manager.enemyAtackStop = false;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                atack.Atacks();
            }
            if (h != 0 || v != 0 || h != 0 && v != 0)
            {
                Moving();
            }
        }
    }
    public void Moving()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += (transform.forward * speed).normalized;
            anime.SetBool("WalkF", true);
            anime.speed = speed;
            //limited.GetComponent<LimitCon>().Avoid();
        }
        else anime.SetBool("WalkF", false);

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= (transform.forward * speed).normalized;
            anime.SetBool("Back", true);
            anime.speed = speed;
            //limited.GetComponent<LimitCon>().Avoid();

        }
        else anime.SetBool("Back", false);

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= (transform.right * speed).normalized;
            anime.SetBool("Left", true);
            anime.speed = speed;
            //limited.GetComponent<LimitCon>().Avoid();
        }
        else anime.SetBool("Left", false);

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += (transform.right * speed).normalized;
            anime.SetBool("Right", true);
            anime.speed = speed;
            //limited.GetComponent<LimitCon>().Avoid();
        }
        else anime.SetBool("Right", false);
    }
    //レーダー機能元
    GameObject SerchTag(GameObject nowObj)
    {
        float timDis = 0;//一時変数
        float nearDis = 0;
        GameObject targetObj = null;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            timDis = Vector3.Distance(obj.transform.position, nowObj.transform.position);
            if (nearDis == 0 || nearDis > timDis)
            {
                nearDis = timDis;
                targetObj = obj;
            }
        }
        return targetObj;
    }
    void Foot()
    {
        source.PlayOneShot(footSound);
    }
}
