﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoder : Singleton<SceneLoder>
{
    public enum Scene
    {
        Start,Select,Game,GameOver,GameClear,NULL
    }
    [Header("スタートシーン")]
    public Scene sceneList1 = Scene.Select;
    [Header("ブリーフィング")]
    public Scene sceneList2 = Scene.Game;
    [Header("戦闘")]
    public Scene sceneList3 = Scene.GameOver;
    [Header("ゲームオーバー")]
    public Scene sceneList4 = Scene.Start;
    [Header("ゲームクリア")]
    public Scene scemeList5 = Scene.GameClear;
    FadeCon fade;
    string sceneName;

    private void Start()
    {
        fade = GameObject.Find("GameStatus").AddComponent<FadeCon>();
        fade = GameObject.Find("GameStatus").GetComponent<FadeCon>();
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>ブリーフィング</summary>
    public void SceneAcsept()
    {
        Scene scene;
        scene = sceneList1;
        switch (scene)
        {
            case Scene.Start:
                fade.fadeInFlag = true;
                sceneName = "Start";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Select:
                fade.fadeInFlag = true;
                sceneName = "PlayerSelect";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Game:
                fade.fadeInFlag = true;
                sceneName = "Game map2";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.GameOver:
                fade.fadeInFlag = true;
                sceneName = "GameOver";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.GameClear:
                sceneName = "GameClear";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.NULL:
                return;
        }
    }
    /// <summary>ゲーム画面</summary>

    public void SceneAcsept2()
    {
        Scene scene;
        scene = sceneList2;
        switch (scene)
        {
            case Scene.Start:
                fade.fadeInFlag = true;
                sceneName = "Start";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Select:
                fade.fadeInFlag = true;
                sceneName = "PlayerSelect";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Game:
                fade.fadeInFlag = true;
                sceneName = "Game map2";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.GameOver:
                fade.fadeInFlag = true;
                sceneName = "GameOver";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.GameClear:
                sceneName = "GameClear";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.NULL:
                return;
        }
    }
    /// <summary>ゲームオーバー</summary>
    public void SceneAcsept3()
    {
        Scene scene;
        scene = sceneList3;
        switch (scene)
        {
            case Scene.Start:
                fade.fadeInFlag = true;
                sceneName = "Start";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Select:
                fade.fadeInFlag = true;
                sceneName = "PlayerSelect";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Game:
                fade.fadeInFlag = true;
                sceneName = "Game map2";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.GameOver:
                fade.fadeInFlag = true;
                sceneName = "GameOver";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.GameClear:
                sceneName = "GameClear";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.NULL:
                return;
        }
    }
    /// <summary>タイトル</summary>
    public void SceneAcsept4()
    {
        Scene scene;
        scene = sceneList4;
        switch (scene)
        {
            case Scene.Start:
                fade.fadeInFlag = true;
                sceneName = "Start";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Select:
                fade.fadeInFlag = true;
                sceneName = "PlayerSelect";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Game:
                fade.fadeInFlag = true;
                sceneName = "Game map2";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.GameOver:
                fade.fadeInFlag = true;
                sceneName = "GameOver";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.GameClear:
                sceneName = "GameClear";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.NULL:
                return;
        }
    }

    public void SceneAcsept5()
    {
        Scene scene;
        scene = scemeList5;
        switch (scene)
        {
            case Scene.Start:
                fade.fadeInFlag = true;
                sceneName = "Start";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Select:
                fade.fadeInFlag = true;
                sceneName = "PlayerSelect";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.Game:
                fade.fadeInFlag = true;
                sceneName = "Game map2";
                SceneManager.LoadScene(sceneName);
                fade.fadeInFlag = false;
                break;
            case Scene.GameOver:
                fade.fadeInFlag = true;
                sceneName = "GameOver";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.GameClear:
                sceneName = "GameClear";
                SceneManager.LoadScene(sceneName);
                break;
            case Scene.NULL:
                return;
        }
    }
}
