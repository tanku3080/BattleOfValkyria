﻿using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
public class TankCon : PlayerBase
{
    //ティーガー戦車は上下に0から∔65度
    //AddRelativeForceを使えば斜面での移動に最適かも
    //xの射角は入れない
    Transform tankHead = null;
    private Transform tankGun = null;
    private Transform tankBody = null;

    public GameObject tankGunFire = null;

    [SerializeField] public CinemachineVirtualCamera defaultCon;
    [SerializeField] public CinemachineVirtualCamera aimCom;

    /// <summary>エイム状態かどうかを判定する</summary>
    public bool aimFlag = false;
    //移動キー。HはCompassと連動するためpublicにした
    private float moveV;
    [HideInInspector] public float moveH;
    //これがtureじゃないとPlayerの操作権は渡せない
    public bool controlAccess = false;
    //カメラをオンにして操作キャラにカメラを切り替える
    [HideInInspector] public bool cameraActive = true;
    /// <summary>特殊コマンド「必中」がアクティブ化しているか</summary>
    bool perfectHit = false;
    /// <summary>特殊コマンド「敵ロックオン」がアクティブ化しているか</summary>
    bool turretCorrection = false;
    /// <summary>移動制限になったかどうか</summary>
    bool limitRangeFlag = true;
    /// <summary>攻撃が敵に当たったか</summary>
    [HideInInspector] public bool atackCheck = false;

    /// <summary>移動バー</summary>
    [HideInInspector] public Slider moveLimitRangeBar;
    /// <summary>HPバー</summary>
    [HideInInspector] public Slider tankHpBar;
    //攻撃に必要なレイキャスト
    RaycastHit hit;
    //移動音を鳴らすために使う
    bool isMoveBGM = true;
    //プレイヤーの車体前後に動いているならTrue
    bool isTankMove = false;
    /// <summary>プレイヤーの車体が曲がっているとTrue</summary>
    bool isTankRot = false;

    void Start()
    {
        Rd = GetComponent<Rigidbody>();
        Trans = GetComponent<Transform>();
        PlayerObj = gameObject;
        tankHead = Trans.GetChild(1);
        tankGun = tankHead.GetChild(0);
        tankGunFire = tankGun.GetChild(0).gameObject;
        tankBody = Trans.GetChild(0);
        aimCom = TurnManager.Instance.AimCon;
        defaultCon = TurnManager.Instance.DefCon;
        moveLimitRangeBar = TurnManager.Instance.limitedBar.transform.GetChild(0).GetComponent<Slider>();
        tankHpBar = TurnManager.Instance.hpBar.transform.GetChild(0).GetComponent<Slider>();
        aimCom = Trans.GetChild(2).GetChild(1).gameObject.GetComponent<CinemachineVirtualCamera>();
        defaultCon = Trans.GetChild(2).GetChild(0).GetComponent<CinemachineVirtualCamera>();
        borderLine = tankHead.GetComponent<BoxCollider>();
        borderLine.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (controlAccess && TurnManager.Instance.timeLineEndFlag)
        {
            Rd.isKinematic = false;

            if (limitRangeFlag)
            {
                limitRangeFlag = false;
                moveLimitRangeBar.maxValue = tankLimitRange;
                moveLimitRangeBar.value = tankLimitRange;
                tankHpBar.maxValue = playerLife;
                tankHpBar.value = playerLife;
            }
            if (cameraActive)
            {
                GameManager.Instance.ChengePop(true, defaultCon.gameObject);
                GameManager.Instance.ChengePop(true, aimCom.gameObject);
                cameraActive = false;
            }
            if (Input.GetKey(KeyCode.J)|| Input.GetKey(KeyCode.L))
            {
                if (TurnManager.Instance.playerIsMove)
                {
                    Quaternion rotetion;
                    bool keySet = false;
                    if (Input.GetKey(KeyCode.J)) keySet = true;
                    else if (Input.GetKey(KeyCode.L)) keySet = false;
                    rotetion = Quaternion.Euler((keySet ? Vector3.down : Vector3.up) * (aimFlag ? tankHead_R_SPD : tankHead_R_SPD / 0.5f) * Time.deltaTime);
                    tankHead.rotation *= rotetion;
                    if (isMoveBGM)
                    {
                        isMoveBGM = false;
                        TankMoveSFXPlay(true, BGMType.HEAD_MOVE);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.L))//砲塔旋回を辞めたら止まる
            {
                isMoveBGM = true;
                TankMoveSFXPlay(false,BGMType.HEAD_MOVE);
            }

            if (IsGranded)
            {
                if (TurnManager.Instance.playerIsMove)
                {
                    moveV = Input.GetAxis("Vertical");
                    moveH = Input.GetAxis("Horizontal");
                    if (moveH != 0)
                    {
                        isTankRot = true;
                        if (isMoveBGM)
                        {
                            isMoveBGM = false;
                            TankMoveSFXPlay(true, BGMType.MOVE);
                        }
                        float rot = moveH * tankTurn_Speed * Time.deltaTime;
                        Quaternion rotetion = Quaternion.Euler(0, rot, 0);
                        Rd.MoveRotation(Rd.rotation * rotetion);
                        MoveLimit();
                    }
                    else
                    {
                        if (isTankRot)
                        {
                            isTankRot = false;
                            isMoveBGM = true;
                            TankMoveSFXPlay(false,BGMType.MOVE);
                        }
                    }
                    //前進後退
                    if (moveV != 0 && Rd.velocity.magnitude != tankLimitSpeed || moveV != 0 && Rd.velocity.magnitude != -tankLimitSpeed)
                    {
                        isTankMove = true;
                        if (isMoveBGM)
                        {
                            Debug.Log("まえすすむ");
                            isMoveBGM = false;
                            TankMoveSFXPlay(true, BGMType.MOVE);
                        }
                        float mov = moveV * playerSpeed * Time.deltaTime;
                        Rd.AddForce(tankBody.transform.forward * mov, ForceMode.Force);
                        MoveLimit();
                    }
                    else
                    {
                        if (isTankMove)
                        {
                            Debug.Log("とまる");
                            isTankMove = false;
                            isMoveBGM = true;
                            TankMoveSFXPlay(false,BGMType.MOVE);
                        }
                    }
                }
            }


            //右クリック
            if (Input.GetButtonUp("Fire2"))
            {
                GameManager.Instance.source.PlayOneShot(GameManager.Instance.fire2sfx);
                if (aimFlag) aimFlag = false;
                else aimFlag = true;
            }
            AimMove(aimFlag);
        }
        else
        {
            limitCounter = 0;
            Rd.isKinematic = true;
        }


    }
    enum BGMType
    {
        MOVE,HEAD_MOVE,NONE
    }

    /// <summary>移動に関する音を鳴らす</summary>
    /// <param name="move">tureならアクティブ化</param>
    /// <param name="type">鳴らす音の種類</param>
    void TankMoveSFXPlay(bool move,BGMType type = BGMType.NONE)
    {
        var t = TurnManager.Instance.tankMove.GetComponent<AudioSource>();
        Debug.Log($"メソッド内{move}");
        if (move)
        {
            if (type == BGMType.MOVE || type == BGMType.HEAD_MOVE)
            {
                switch (type)
                {
                    case BGMType.MOVE:
                        GameManager.Instance.ChengePop(move, TurnManager.Instance.tankMove);
                        t.clip = GameManager.Instance.tankMoveSfx;
                        break;
                    case BGMType.HEAD_MOVE:
                        GameManager.Instance.ChengePop(move, TurnManager.Instance.tankMove);
                        t.clip = GameManager.Instance.tankHeadsfx;
                        break;
                }
                t.Play();
            }
        }
        else
        {
            t.clip = null;
            t.Stop();
            GameManager.Instance.ChengePop(move, TurnManager.Instance.tankMove);
        }
    }
    /// <summary>攻撃したらプラスする</summary>
    private int limitCounter = 0;
    /// <summary>
    /// aimFlagがtrueならtrue
    /// </summary>
    public void AimMove(bool aim)
    {
        if (aim)
        {
            GameManager.Instance.ChengePop(false,moveLimitRangeBar.gameObject);
            GameManager.Instance.ChengePop(true,aimCom.gameObject);
            GameManager.Instance.ChengePop(false, defaultCon.gameObject);
            GameManager.Instance.ChengePop(false,TurnManager.Instance.hpBar);
            if (Input.GetButtonUp("Fire1") && TurnManager.Instance.dontShoot == false)
            {
                Debug.Log("エイム攻撃dontShoot:" + TurnManager.Instance.dontShoot);
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
            if (TurnManager.Instance.PlayerMoveVal != 0 && Input.GetKeyUp(KeyCode.F) || TurnManager.Instance.PlayerMoveVal != 0 && Input.GetKeyUp(KeyCode.R))
            {
                if (turretCorrection && perfectHit || turretCorrection || perfectHit)
                {
                    TurnManager.Instance.playerIsMove = false;
                }
                if (Input.GetKeyUp(KeyCode.F))//砲塔を向ける
                {
                    if (turretCorrection != false)
                    {
                        turretCorrection = false;
                    }
                    else
                    {
                        if (TurnManager.Instance.FoundEnemy)
                        {
                            TurnManager.Instance.MoveCounterText(TurnManager.Instance.text1);
                            turretCorrection = true;
                            GunAccuracy(turretCorrection);
                        }
                        else TurnManager.Instance.AnnounceStart("Not Found Enemy");
                    }
                }
                if (Input.GetKeyUp(KeyCode.R))//敵に照準が合っていたら命中率を100
                {
                    if (perfectHit != false)
                    {
                        perfectHit = false;
                    }
                    else
                    {
                        if (TurnManager.Instance.FoundEnemy)
                        {
                            TurnManager.Instance.MoveCounterText(TurnManager.Instance.text1);
                            perfectHit = true;
                            GunDirctionIsEnemy(perfectHit);
                        }
                        else TurnManager.Instance.AnnounceStart("Not Found Enemy");
                    }
                }
            }
            else if(TurnManager.Instance.PlayerMoveVal == 0 && Input.GetKeyUp(KeyCode.F) || TurnManager.Instance.PlayerMoveVal != 0 && Input.GetKeyUp(KeyCode.R))
            {
                TurnManager.Instance.AnnounceStart("SP Value Zero");
            }
        }
        else
        {
            TurnManager.Instance.playerIsMove = true;
            GameManager.Instance.ChengePop(true, moveLimitRangeBar.gameObject);
            GameManager.Instance.ChengePop(true,defaultCon.gameObject);
            GameManager.Instance.ChengePop(true, TurnManager.Instance.hpBar);
            GameManager.Instance.ChengePop(false,aimCom.gameObject);
            var p = tankGun.transform.rotation;
            p.x = 0;
            p.z = 0;
        }
    }

    void Reload()
    {
        //リロード時間を取りたいなぁーと思う
        if (limitCounter == atackCount)
        {
            limitCounter = 0;
        }
        else TurnManager.Instance.AnnounceStart("bullets left.");
    }

    void GunAccuracy(bool flag)
    {
        if (flag)
        {
            TurnManager.Instance.PlayerMoveVal--;
            TurnManager.Instance.MoveCounterText(TurnManager.Instance.text1);
            GameManager.Instance.source.PlayOneShot(GameManager.Instance.Fsfx);
            tankHead.LookAt(TurnManager.Instance.nearEnemy.transform,Vector3.up);
            GameManager.Instance.ChengePop(true, TurnManager.Instance.turretCorrectionF);
        }
        else
        {
            GameManager.Instance.source.PlayOneShot(GameManager.Instance.cancel);
            GameManager.Instance.ChengePop(false, TurnManager.Instance.turretCorrectionF);
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
            GameManager.Instance.ChengePop(true,TurnManager.Instance.hittingTargetR);
            GameManager.Instance.source.PlayOneShot(GameManager.Instance.Fsfx);
            TurnManager.Instance.MoveCounterText(TurnManager.Instance.text1);
        }
        else
        {
            GameManager.Instance.source.PlayOneShot(GameManager.Instance.cancel);
            GameManager.Instance.ChengePop(false, TurnManager.Instance.hittingTargetR);
        }
    }
    void Atack()
    {
        if (perfectHit || perfectHit && turretCorrection)
        {
            if (perfectHit && turretCorrection)
            {
                TurnManager.Instance.nearEnemy.GetComponent<Enemy>().Damage(tankDamage * 2);
            }
            else if (perfectHit && turretCorrection == false)//命中率のみ
            {
                if (RayStart(tankGun.transform.position))
                {
                    hit.collider.gameObject.GetComponent<Enemy>().Damage(tankDamage);
                }
                else Debug.Log("そもそも敵に砲塔が向いていないから外れた");
            }
            else if (perfectHit == false && turretCorrection)//砲塔が向いているだけの場合
            {
                if (RayStart(tankGun.transform.position))
                {
                    if (HitCalculation()) hit.collider.gameObject.GetComponent<Enemy>().Damage(tankDamage);
                    else hit.collider.gameObject.GetComponent<Enemy>().Damage(tankDamage / 2);
                }
                else Debug.Log("砲塔が向いているけど命中率を呼んだ結果、外した");
            }
        }
        else
        {
            if (RayStart(tankGun.transform.position))
            {
                if (HitCalculation()) hit.collider.gameObject.GetComponent<Enemy>().Damage(tankDamage);
                else hit.collider.gameObject.GetComponent<Enemy>().Damage(tankDamage / 2);
            }
        }
        GameManager.Instance.source.PlayOneShot(GameManager.Instance.atack);
        ParticleSystemEXP.Instance.StartParticle(tankGunFire.transform,ParticleSystemEXP.ParticleStatus.GUN_FIRE);
        GameManager.Instance.ChengePop(true,tankGunFire);
        GunDirctionIsEnemy(perfectHit = false);
        GunAccuracy(turretCorrection = false);
        TurnManager.Instance.playerIsMove = true;
    }

    /// <summary>クリティカルヒットかどうか判定する</summary>
    /// <returns></returns>
    private bool HitCalculation()
    {
        bool result;
        if (Random.Range(0, 100) > 50) result = true;
        else result = false;
        return result;
    }

    /// <summary>rayを飛ばして当たっているか判定</summary>
    /// <param name="atackPoint">rayの発生地点</param>
    /// <param name="num">当たっているか判定するオブジェクトのTag名。初期値はEnemy</param>
    bool RayStart(Vector3 atackPoint, string num = "Enemy")
    {
        bool f = false;
        if (Physics.Raycast(atackPoint, transform.forward, out hit, tankLimitRange))
        {
            if (hit.collider.CompareTag(num))
            {
                Debug.Log("当たった");
                f = true;
            }
        }
        return f;
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
            //このキャラの移動権が無くなる
            controlAccess = false;
        }
    }

    /// <summary>地面についているかの判定</summary>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Grand"))
        {
            IsGranded = true;
        }
    }
    //敵がコライダーと接触したら使う
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") && TurnManager.Instance.FoundEnemy != true && TurnManager.Instance.playerTurn)
        {
            TurnManager.Instance.FoundEnemy = true;
            if (other.gameObject.GetComponent<Enemy>().enemyAppearance != true && !TurnManager.Instance.enemyDiscovery.Contains(other.gameObject))
            {
                TurnManager.Instance.enemyDiscovery.Add(other.gameObject);
                GameManager.Instance.source.PlayOneShot(GameManager.Instance.discoverySfx);
                other.gameObject.GetComponent<Enemy>().enemyAppearance = true;
            }
        }
    }
    //敵がコライダーから離れたら使う
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") && TurnManager.Instance.enemyDiscovery.Contains(other.gameObject) && TurnManager.Instance.playerTurn)
        {
            TurnManager.Instance.enemyDiscovery.Remove(other.gameObject);
            TurnManager.Instance.FoundEnemy = false;
        }
    }
    /// <summary>地面から離れているかの判定</summary>
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Grand"))
        {
            IsGranded = false;
        }
    }
}
