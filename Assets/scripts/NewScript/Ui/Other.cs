﻿using UnityEngine;
using UnityEngine.UI;

public class Other : MonoBehaviour
{
    public Button startBunt;
    public AudioClip sfx;
    AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
        source.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameStart();
        }
    }

    public void GameStart()
    {
        source.PlayOneShot(sfx);
        SceneFadeManager.Instance.SceneFadeAndChanging(SceneFadeManager.SceneName.Meeting, true, true);
    }
}