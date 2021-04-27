using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Controller : MonoBehaviour
{
    public GameObject start_menu;
    public GameObject level_menu;
    public GameObject credit_panel;
    public AudioClip click_sound_1;
    public AudioClip click_sound_2;
    public AudioClip select_sound;

    private Game_Manager gm;
    private AudioSource source;
    void Start()
    {
        start_menu.SetActive(true);
        level_menu.SetActive(false);
        gm = GameObject.Find("Game_Manager").GetComponent<Game_Manager>();
        source = GameObject.Find("Music_Player").GetComponent<AudioSource>();
    }

    public void Play_Sound(int i)
    {
        switch (i)
        {
            case 1:
                {
                    source.PlayOneShot(click_sound_1, 1);
                }
                break;
            case 2:
                {
                    source.PlayOneShot(click_sound_2, 0.8f);
                }
                break;
            case 3:
                {
                    source.PlayOneShot(select_sound, 0.8f);
                }
                break;
        }
    }

    public void Show_Menu()
    {
        start_menu.SetActive(true);
        level_menu.SetActive(false);
        credit_panel.SetActive(false);
        Play_Sound(1);
    }

    public void Show_Level()
    {
        level_menu.SetActive(true);
        start_menu.SetActive(false);
        credit_panel.SetActive(false);
        Play_Sound(1);
    }
    public void Show_Credit()
    {
        level_menu.SetActive(false);
        start_menu.SetActive(false);
        credit_panel.SetActive(true);
        Play_Sound(1);
    }
    public void Play_Game()
    {
        Play_Sound(1);
        if (gm.current_level <= gm.unlocked_level)
        {
            gm.Load_Scene("Gameplay");
            Play_Sound(3);
        }
    }

    public void Exit_Game()
    {
        gm.Exit_Game();
        Play_Sound(1);
    }
}
