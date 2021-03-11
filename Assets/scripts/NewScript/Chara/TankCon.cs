﻿using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
public class TankCon : PlayerBase
{
    //ティーガー戦車は上下に0から∔65度
    //AddRelativeForceを使えば斜面での移動に最適らしい
    //xの射角は入れない
    Transform tankHead = null;
    private Transform tankGun = null;
    private Transform tankBody = null;

    public GameObject tankGunFire = null;

    [SerializeField] public CinemachineVirtualCamera defaultCon;
    [SerializeField] public CinemachineVirtualCamera aimCom;
    //移動制限用
    [SerializeField] float limitRange = 50f;

    bool AimFlag = false;
    //これがtureじゃないとPlayerの操作権はない
    public bool controlAccess = false;
    //カメラをオンにするのに必要
    public bool cameraActive = true;

    bool perfectHit = false;//命中率
    bool turretCorrection = false;//精度
    bool limitRangeFlag = true;//移動制限値
    public bool atackCheck = false;
    bool MoveAudioFlag;

    //以下は移動制限
    [HideInInspector] public Slider moveLimitRangeBar;
    AudioSource playerMoveAudio;


    void Start()
    {
        Rd = GetComponent<Rigidbody>();
        Trans = GetComponent<Transform>();
        Renderer = GetComponent<MeshRenderer>();
        tankHead = Trans.GetChild(1);
        tankGun = tankHead.GetChild(0);
        tankGunFire = tankGun.GetChild(0).transform.gameObject;
        
        tankBody = Trans.GetChild(0);
        aimCom = TurnManager.Instance.AimCon;
        defaultCon = TurnManager.Instance.DefCon;
        moveLimitRangeBar = GameManager.Instance.limitedBar.transform.GetChild(0).GetComponent<Slider>();
        aimCom = Trans.GetChild(2).GetChild(1).gameObject.GetComponent<CinemachineVirtualCamera>();
        defaultCon = Trans.GetChild(2).GetChild(0).GetComponent<CinemachineVirtualCamera>();
        borderLine = tankHead.GetComponent<BoxCollider>();
        borderLine.isTrigger = true;
        playerMoveAudio = gameObject.GetComponent<AudioSource>();
        playerMoveAudio.playOnAwake = false;
        playerMoveAudio.loop = true;
        playerMoveAudio.clip = GameManager.Instance.TankSfx;
        limitCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Rd.isKinematic = false;
        if (controlAccess)
        {
            if (MoveAudioFlag) TankMoveSFXPlay(true,false);
            else TankMoveSFXPlay(false,true);
            if (limitRangeFlag)
            {
                limitRangeFlag = false;
                moveLimitRangeBar.maxValue = tankLimitRange;
                moveLimitRangeBar.value = tankLimitRange;
            }
            if (cameraActive)
            {
                GameManager.Instance.ChengePop(true, defaultCon.gameObject);
                GameManager.Instance.ChengePop(true, aimCom.gameObject);
                cameraActive = false;
            }
            if (TurnManager.Instance.playerIsMove)
            {
                //マウスを「J」「L」での旋回に変更

                if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L))
                {
                    Quaternion rotetion = Quaternion.identity;
                    if (Input.GetKey(KeyCode.J))
                    {
                        rotetion = Quaternion.Euler(Vector3.down / 2 * (AimFlag ? tankHead_R_SPD : tankHead_R_SPD / 0.5f) * Time.deltaTime);
                    }
                    else if (Input.GetKey(KeyCode.L))
                    {
                        rotetion = Quaternion.Euler(Vector3.up / 2 * (AimFlag ? tankHead_R_SPD : tankHead_R_SPD / 0.5f) * Time.deltaTime);
                    }
                    tankHead.rotation *= rotetion;
                }

                if (IsGranded)
                {

                    if (TurnManager.Instance.playerIsMove)
                    {
                        float v = Input.GetAxis("Vertical");
                        float h = Input.GetAxis("Horizontal");
                        if (h != 0)
                        {
                            MoveAudioFlag = true;
                            float rot = h * tankTurn_Speed * Time.deltaTime;
                            Quaternion rotetion = Quaternion.Euler(0, rot, 0);
                            Rd.MoveRotation(Rd.rotation * rotetion);
                            MoveLimit();
                        }
                        else MoveAudioFlag = false;
                        //前進後退
                        if (v != 0 && Rd.velocity.magnitude != tankLimitSpeed || v != 0 && Rd.velocity.magnitude != -tankLimitSpeed)
                        {
                            MoveAudioFlag = true;
                            float mov = v * playerSpeed * Time.deltaTime;// * Time.deltaTime;
                            Rd.AddForce(tankBody.transform.forward * mov, ForceMode.Force);
                            MoveLimit();
                        }
                        else MoveAudioFlag = false;
                    }
                }
            }
            //右クリック
            if (Input.GetButtonUp("Fire2"))
            {
                GameManager.Instance.source.PlayOneShot(GameManager.Instance.fire2sfx);
                if (AimFlag) AimFlag = false;
                else AimFlag = true;
            }
            AimMove(AimFlag);
        }
        else
        {
            limitCounter = 0;
            Rd.isKinematic = true;
        }


        if (playerLife <= 0)
        {
            PlayerDie(Renderer);
        }
    }

    void TankMoveSFXPlay(bool isPlay = false,bool isStop = false)
    {
        if (isPlay && MoveAudioFlag)
        {
            playerMoveAudio.Play();
        }
        if (isStop && MoveAudioFlag == false)
        {
            playerMoveAudio.Stop();
        }
    }
    private int limitCounter = 0;
    /// <summary>
    /// aimFlagがtrueならtrue
    /// </summary>
    void AimMove(bool aim)
    {
        if (aim)
        {
            TurnManager.Instance.playerIsMove = false;

            GameManager.Instance.ChengePop(true,aimCom.gameObject);
            GameManager.Instance.ChengePop(false, defaultCon.gameObject);
            if (Input.GetButtonUp("Fire1"))
            {
                if (atackCount > limitCounter)
                {
                    limitCounter++;
                    TurnManager.Instance.MoveCounterText(TurnManager.Instance.text1);
                    //攻撃
                    Atack();
                }
                else
                {
                    TurnManager.Instance.AnnounceStart("Atack Limit");
                }
            }
            if (Input.GetKeyUp(KeyCode.F))//砲塔を向ける
            {
                if (TurnManager.Instance.FoundEnemy)
                {
                    TurnManager.Instance.MoveCounterText(TurnManager.Instance.text1);
                    if (turretCorrection) turretCorrection = false;
                    else turretCorrection = true; //精度100％
                    GunAccuracy(turretCorrection);
                }
            }
            if (Input.GetKeyUp(KeyCode.R))//命中率を100。注意：敵に照準があっている前提
            {
                if (perfectHit) perfectHit = false;
                else perfectHit = true;
                GunDirctionIsEnemy(perfectHit);
            }
        }
        else
        {
            TurnManager.Instance.playerIsMove = true;
            defaultCon.gameObject.SetActive(true);
            aimCom.gameObject.SetActive(false);
            var p = tankGun.transform.rotation;
            p.x = 0;
            p.z = 0;
        }
    }

    void GunAccuracy(bool flag)
    {
        if (flag)
        {
            GameManager.Instance.source.PlayOneShot(GameManager.Instance.Fsfx);
            tankHead.LookAt(GameManager.Instance.nearEnemy.transform,Vector3.up);
            GameManager.Instance.ChengePop(true, GameManager.Instance.turretCorrectionF);
        }
        else
        {
            GameManager.Instance.source.PlayOneShot(GameManager.Instance.cancel);
            GameManager.Instance.ChengePop(false, GameManager.Instance.turretCorrectionF);
        }
    }

    /// <summary>
    /// 命中率を100にする
    /// </summary>
    void GunDirctionIsEnemy(bool flag)
    {
        if (flag)
        {
            TurnManager.Instance.PlayerMoveVal--;
            GameManager.Instance.ChengePop(true,GameManager.Instance.hittingTargetR);
            GameManager.Instance.source.PlayOneShot(GameManager.Instance.Fsfx);
            TurnManager.Instance.MoveCounterText(TurnManager.Instance.text1);
        }
        else
        {
            GameManager.Instance.source.PlayOneShot(GameManager.Instance.cancel);
            GameManager.Instance.ChengePop(false, GameManager.Instance.hittingTargetR);
        }
    }
    void Atack()
    {
        RaycastHit rayhit;
        if (perfectHit || perfectHit && turretCorrection)
        {
            if (perfectHit && turretCorrection)
            {
                tankDamage *= 2;
                GameManager.Instance.nearEnemy.GetComponent<Enemy>().Damage(tankDamage);
                tankDamage /= 2;
                Debug.Log("EnemyLife" + GameManager.Instance.nearEnemy.gameObject.GetComponent<Enemy>().enemyLife);
            }
            else if (perfectHit && turretCorrection == false)//命中率のみ
            {
                if (Physics.Raycast(tankGunFire.transform.position,Vector3.forward,out rayhit,tankLimitRange))
                {
                    if (rayhit.collider.tag == "Enemy")
                    {
                        rayhit.collider.gameObject.GetComponent<Enemy>().Damage(tankDamage);
                        Debug.Log("EnemyLife" + rayhit.collider.gameObject.GetComponent<Enemy>().enemyLife);
                    }
                }
            }
        }
        else
        {
            if (Physics.Raycast(tankGunFire.transform.position,Vector3.forward,out rayhit,tankLimitRange))
            {
                if (rayhit.collider.tag == "Enemy")
                {
                    if (HitCalculation()) rayhit.collider.gameObject.GetComponent<Enemy>().Damage(tankDamage);
                    else rayhit.collider.gameObject.GetComponent<Enemy>().Damage(tankDamage / 2);
                    Debug.Log("EnemyLife" + rayhit.collider.gameObject.GetComponent<Enemy>().enemyLife);
                }
            }
        }
        GameManager.Instance.source.PlayOneShot(GameManager.Instance.atack);
        //tankGunFire = Instantiate((GameObject)Resources.Load("GunFirering"));
        GunDirctionIsEnemy(perfectHit = false);
        GunAccuracy(turretCorrection = false);
    }

    void Particle()
    {

    }

    /// <summary>命中率の結果を真偽値で入れる</summary>
    /// <returns></returns>
    private bool HitCalculation()
    {
        bool result;
        if (Random.Range(0, 100) > 50) result = true;
        else result = false;
        return result;
    }

    /// <summary>
    /// 移動制限をつけるメソッド
    /// </summary>
    void MoveLimit()
    {
        if (moveLimitRangeBar.value > moveLimitRangeBar.minValue)
        {
            moveLimitRangeBar.value -= 1;
        }
        else
        {
            //これだと砲塔が動かなくなる
            controlAccess = false;
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Grand")
        {
            IsGranded = true;
        }
    }

    //敵を見つけた際に使う物
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            TurnManager.Instance.FoundEnemy = true;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Grand")
        {
            IsGranded = false;
        }
    }
}
