﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AtackCon : MonoBehaviour
{
    //射撃システムの構築中。攻撃ボタンを押したら射撃を繰り返すスクリプトをコルーチンで行う。
    //enumの種類を一種類にしてenumからフィールドでの設定にする
    //intervalは攻撃間隔
    public float interval { get { return status.gunInterval; } }
    float accuracy { get { return status._accuracy; } }
    float gunAccuracy { get { return status._gunAccuracy; } }
    float hitPercent;
    //何発当たったか格納する
    System.Collections.Generic.List<int> atackCount = null;
    float healthM { get { return players.HpM; } }
    float Health { get { return manager.playerHp; } }
    public Transform sight, objSize;
    public GameObject _enemy;
    Slider hp;
    GameManager manager;
    StatusCon status;
    PlayerCon players;

    private void Start()
    {
        sight = GameObject.Find("Aim").transform;
        objSize = GameObject.Find("center").transform;
        hp = GameObject.Find("HpBer").GetComponent<Slider>();
        players = GameObject.Find("Player").GetComponent<PlayerCon>();
        manager = this.gameObject.GetComponent<GameManager>();
        status = GameObject.Find("GameManager").GetComponent<StatusCon>();
    }
    public void Atacks()
    {
        //以下のコードはプレイヤーの照準を同期する
        Vector2 sightpos = RectTransformUtility.WorldToScreenPoint(Camera.main,sight.position);
        sightpos.x = sightpos.x - ((Screen.width / 2) * Random.insideUnitSphere.x * GunFireCalculation());
        sightpos.y = sightpos.y - ((Screen.height / 2) * Random.insideUnitSphere.y * GunFireCalculation());
        //Ray ray = sightpos;//x,yに計算の答えを入れた
        if (manager.weponIs1 == true)
        {
            for (int i = 0; i < status.bullet; i++)
            {
                //if (Physics.Raycast(ray, out RaycastHit hit))
                //{
                //    if (hit.collider.tag == "Enemy") atackCount.Add(1);
                //    else atackCount.Add(0);
                //}
                if (Physics2D.Raycast(sightpos,sightpos))
                {
                    Debug.Log("命中");
                    atackCount.Add(1);
                }
                else
                {
                    Debug.Log("外れ");
                    atackCount.Add(0);
                }
            }
            StartCoroutine(Fire1());
        }
        if (manager.weponIs2 == true)
        {
            for (int i = 0; i < status.bullet2; i++)
            {
                //if (Physics.Raycast(ray, out RaycastHit hit))
                //{
                //    if (hit.collider.tag == "Enemy") atackCount.Add(1);
                //    else atackCount.Add(0);
                //}
                if (Physics2D.Raycast(sightpos, sightpos))
                {
                    Debug.Log("命中");
                    atackCount.Add(1);
                }
                else
                {
                    Debug.Log("外れ");
                    atackCount.Add(0);
                }
            }
            StartCoroutine(Fire2());
        }
    }

    float GunFireCalculation()
    {
        //プレイヤーのtransformの位置にこの計算を入れる。
        float accuracyPenalty = (float)(1 * 0.75 + (0.25 * Health / healthM));//命中精度のペナルティ
        hitPercent = accuracy * accuracyPenalty * gunAccuracy;
        return hitPercent = 0.87f;
    }

    IEnumerator Fire1()
    {
        foreach (int t in atackCount)
        {
            //以下の分岐で射撃アニメーションを記載する
            if (t == 0)
            {

            }
            else//命中
            {
                manager.enemyHp -= status.damage1;
                hp.value = manager.playerHp / players.HpM;
            }
            yield return new WaitForSeconds(interval);

        }
    }
    IEnumerator Fire2()
    {
        foreach (int t in atackCount)
        {
            if (t == 0)
            {
                Debug.Log("外れ");
                atackCount.Remove(t);
            }
            else//命中
            {
                Debug.Log("命中");
                manager.enemyHp -= status.damage2;
                atackCount.Remove(t);
                hp.value = manager.playerHp / players.HpM;
            }
            yield return new WaitForSeconds(interval);

        }
    }
}