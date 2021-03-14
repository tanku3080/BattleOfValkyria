﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>, InterfaceScripts.ITankChoice
{
    public enum TankChoice
    {
        Tiger, Panzer2, Shaman, Stuart,
    }
    [SerializeField,HideInInspector] public bool enemyAtackStop = false;
    [SerializeField,HideInInspector] public bool GameUi = false;
    public Renderer[] enemyRender { get; set; }

    //この値がtrueなら敵味方問わず攻撃を停止する
    public bool GameFlag { get; set; }
    [HideInInspector] public AudioSource source;
    [SerializeField, Tooltip("UIclickボタン")] public AudioClip click;//UIHitGameの音らしい
    [SerializeField, Tooltip("UICancelボタン")] public AudioClip cancel;//UIHitGameの音らしい
    [SerializeField, Tooltip("Fキーボタン")] public AudioClip Fsfx;
    [SerializeField, Tooltip("エイムキーボタン")] public AudioClip fire2sfx;
    [SerializeField, Tooltip("Rキーボタン")] public AudioClip Aimsfx;
    [SerializeField, Tooltip("space")] public AudioClip tankChengeSfx;
    [SerializeField, Tooltip("砲塔旋回")] public AudioClip tankHeadsfx;
    [SerializeField, Tooltip("攻撃ボタン")] public AudioClip atackSfx;
    [SerializeField, Tooltip("攻撃音")] public AudioClip atack;

    [SerializeField, Header("戦車切替確認ボタン")] public GameObject tankChengeObj = null;
    [SerializeField, Header("ポーズ画面UI")] public GameObject pauseObj = null;
    [SerializeField, Header("ターンエンドUI")] public GameObject endObj = null;
    [SerializeField, Header("レーダUI")] public GameObject radarObj = null;
    [SerializeField, Header("アナウンスUI")] public GameObject announceObj = null;
    [SerializeField, Header("移動制限")] public GameObject limitedBar = null;
    [SerializeField, Header("特殊状態")] public GameObject specialObj = null;
    [HideInInspector] public GameObject hittingTargetR = null;
    [HideInInspector] public GameObject turretCorrectionF = null;
    [SerializeField, HideInInspector] public GameObject nearEnemy = null;
    //ゲームシーンかの判定(ターンマネージャー限定)
    [SerializeField, HideInInspector] public bool isGameScene;

    public bool clickC = true;
    private int nowTurnValue = 0;
    Navigation nav;

    override protected void Awake()
    {
        //これは必須。start関数内に置いたら処理する前に呼び出されるので現状このように記述する
        hittingTargetR = specialObj.transform.GetChild(0).gameObject;
        turretCorrectionF = specialObj.transform.GetChild(1).gameObject;
    }

    void Start()
    {
        if (tankChengeObj == null)
        {
            tankChengeObj = GameObject.Find("TankChengeUI");
            pauseObj = GameObject.Find("PauseUI");
            endObj = GameObject.Find("TurnendUI");
            radarObj = GameObject.Find("Radar");
            limitedBar = GameObject.Find("MoveLimitBar");
            specialObj = GameObject.Find("specialStatusUI");
            announceObj = GameObject.Find("announceUI");
        }
        ChengePop(false,tankChengeObj);
        ChengePop(false, radarObj);
        ChengePop(false, pauseObj);
        ChengePop(false, limitedBar);
        ChengePop(false, endObj);
        ChengePop(false, hittingTargetR);
        ChengePop(false, turretCorrectionF);
        ChengePop(false, announceObj);
        source = gameObject.GetComponent<AudioSource>();
        source.playOnAwake = false;
        isGameScene = true;
        DontDestroyOnLoad(tankChengeObj.transform.parent);
    }
    bool oneTimeFlag = true;
    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name != "GamePlay" || SceneManager.GetActiveScene().name != "TestMap")
        {
            if (oneTimeFlag)
            {
                oneTimeFlag = false;
                TurnManager.Instance.PlayMusic();
            }
        }
        if (SceneManager.GetActiveScene().name == "Start")
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                source.PlayOneShot(click);
                SceneFadeManager.Instance.SceneFadeAndChanging(SceneFadeManager.SceneName.Meeting, true, true);
            }
        }
        if (SceneManager.GetActiveScene().name == "GamePlay" || SceneManager.GetActiveScene().name == "TestMap")
        {
            nowTurnValue = TurnManager.Instance.generalTurn;
            nearEnemy = SerchTag(TurnManager.Instance.nowPayer);

            if (Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.Return))
            {
                ButtonSelected();
            }

            //テスト用
            if (Input.GetKeyUp(KeyCode.H))
            {
                SceneFadeManager.Instance.SceneFadeAndChanging(SceneFadeManager.SceneName.GameClear,true,true);
            }
            if (Input.GetKeyUp(KeyCode.G))
            {
                SceneFadeManager.Instance.SceneFadeAndChanging(SceneFadeManager.SceneName.GameOver,true,true);
            }
            //以上
        }
        if (SceneManager.GetActiveScene().name == "GameClear" || SceneManager.GetActiveScene().name == "GameOver")
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                source.PlayOneShot(click);
                SceneFadeManager.Instance.SceneFadeAndChanging(SceneFadeManager.SceneName.Start,true,true);
            }
        }
    }

    GameObject SerchTag(GameObject nowObj)
    {
        float nearDis = 0;
        GameObject targetObj = null;
        foreach (Enemy obj in TurnManager.Instance.enemys)
        {
            var timDis = Vector3.Distance(obj.transform.position, nowObj.transform.position);
            if (nearDis == 0 || nearDis > timDis)
            {
                nearDis = timDis;
                targetObj = obj.gameObject;
            }
        }
        return targetObj;
    }

    public void ButtonSelected()
    {
        if (Input.GetKeyUp(KeyCode.P) && clickC)
        {
            source.PlayOneShot(click);
            ChengePop(clickC, pauseObj);
            TurnManager.Instance.playerIsMove = !clickC;
            TurnManager.Instance.enemyIsMove = !clickC;
            clickC = false;
        }
        else if (Input.GetKeyUp(KeyCode.P) && clickC == false)
        {
            source.PlayOneShot(cancel);
            ChengePop(clickC, pauseObj);
            TurnManager.Instance.playerIsMove = !clickC;
            TurnManager.Instance.enemyIsMove = !clickC;
            clickC = true;
        }
        if (Input.GetKeyUp(KeyCode.Space) && TurnManager.Instance.playerTurn && clickC)
        {
            source.PlayOneShot(click);
            ChengePop(clickC, tankChengeObj);
            TurnManager.Instance.playerIsMove = !clickC;
            TurnManager.Instance.enemyIsMove = !clickC;
            clickC = false;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && TurnManager.Instance.playerTurn && clickC == false && tankChengeObj.activeSelf == true)
        {
            source.PlayOneShot(cancel);
            ChengePop(clickC, tankChengeObj);
            TurnManager.Instance.playerIsMove = !clickC;
            TurnManager.Instance.enemyIsMove = !clickC;
            clickC = true;
        }
        if (Input.GetKeyUp(KeyCode.Return) && TurnManager.Instance.playerTurn && clickC)
        {
            source.PlayOneShot(click);
            ChengePop(clickC, endObj);
            TurnManager.Instance.playerIsMove = !clickC;
            TurnManager.Instance.enemyIsMove = !clickC;
            clickC = false;
        }
        else if (Input.GetKeyUp(KeyCode.Return) && TurnManager.Instance.playerTurn && clickC == false && endObj.activeSelf == true)
        {
            source.PlayOneShot(cancel);
            ChengePop(clickC, endObj);
            TurnManager.Instance.playerIsMove = !clickC;
            TurnManager.Instance.enemyIsMove = !clickC;
            clickC = true;
        }
        if (Input.GetKeyUp(KeyCode.Q) && TurnManager.Instance.playerTurn && clickC)//レーダー
        {
            source.PlayOneShot(click);
            ChengePop(clickC,radarObj);
            clickC = false;
        }
        else if (Input.GetKeyUp(KeyCode.Q) && TurnManager.Instance.playerTurn && clickC == false)
        {
            ChengePop(clickC, radarObj);
            clickC = true;
        }
    }

    /// <summary>ゲームクリア時に呼び出す</summary>
    public void EndStage()
    {
        TurnManager.Instance.players.Clear();
        TurnManager.Instance.enemys.Clear();
        SceneFadeManager.Instance.SceneFadeAndChanging(SceneFadeManager.SceneName.GameClear, true, true);
    }

    ///<summary>リスタートボタンをクリックしたら呼び出し</summary>
    public void Restart()
    {
        source.PlayOneShot(click);
        SceneFadeManager.Instance.SceneFadeAndChanging(SceneFadeManager.SceneName.GamePlay, true, true);
    }
    /// <summary>タイトルボタンをクリックしたら呼び出し</summary>
    public void Title()
    {
        source.PlayOneShot(click);
        SceneFadeManager.Instance.SceneFadeAndChanging(SceneFadeManager.SceneName.Start, true, true);
    }

    public int charactorHp;
    public float charactorSpeed;
    public float tankHeadSpeed;
    public float tankTurnSpeed;
    public float tankLimitedSpeed;
    public float tankLimitedRange;
    public float tankSearchRanges;
    public int tankDamage;
    public int atackCounter;
    /// <summary>
    /// 戦車を選択
    /// </summary>
    /// <param name="tank">選択する戦車の名前</param>
    public void TankChoiceStart(string num)
    {
        TankChoice tank = TankChoice.Tiger;
        while (num != tank.ToString())
        {
            tank++;
        }
        switch (tank)
        {
            case TankChoice.Tiger:
                charactorHp = 100;
                charactorSpeed = 1000f;
                tankHeadSpeed = 2.5f;
                tankTurnSpeed = 5f;
                tankLimitedSpeed = 1000f;
                tankLimitedRange = 10000f;
                tankSearchRanges = 50f;
                tankDamage = 35;
                atackCounter = 1;
                break;
            case TankChoice.Panzer2:
                charactorHp = 50;
                charactorSpeed = 1500f;
                tankHeadSpeed = 3f;
                tankTurnSpeed = 10f;
                tankLimitedSpeed = 1500f;
                tankLimitedRange = 100000f;
                tankSearchRanges = 100f;
                tankDamage = 20;
                atackCounter = 2;
                break;
            case TankChoice.Shaman:
                charactorHp = 80;
                charactorSpeed = 21f;
                tankHeadSpeed = 2.5f;
                tankTurnSpeed = 5f;
                tankLimitedSpeed = 1000f;
                tankLimitedRange = 10000f;
                tankSearchRanges = 50f;
                tankDamage = 35;
                atackCounter = 1;
                break;
            case TankChoice.Stuart:
                charactorHp = 30;
                charactorSpeed = 30f;
                tankHeadSpeed = 2.5f;
                tankTurnSpeed = 5f;
                tankLimitedSpeed = 100000f;
                tankSearchRanges = 100f;
                tankDamage = 20;
                atackCounter = 2;
                break;
        }
    }

    /// <summary>
    /// 確認メッセージやその他非表示オブジェクトを表示。第3引数がNUllの場合GameManagerで登録された全てのUIをチェックするので処理が重くなる
    /// </summary>
    public void ChengePop(bool isChenge = false, GameObject obj = null) => obj.SetActive(isChenge);

    public void TurnEnd()
    {
        TurnManager.Instance.playerTurn = true;
        ChengePop(false, tankChengeObj);
        ChengePop(false, radarObj);
        ChengePop(false, pauseObj);
        ChengePop(false, limitedBar);
        ChengePop(false, endObj);
        ChengePop(false, hittingTargetR);
        ChengePop(false, turretCorrectionF);
        ChengePop(false, announceObj);
    }
}