using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coin_Manager : MonoBehaviour
{
    public int player_coin = 0;
    public int player_current_turn = 1;
    public int enemy_coin = 0;
    public int enemy_current_turn = 0;
    public GameObject player_text;
    public GameObject enemy_text;
    public AudioClip coin_sound;

    private Text player_coin_text;
    private Text enemy_coin_text;
    private Attack_Controller a;
    private AudioSource source;

    void Start()
    {
        player_coin_text = GameObject.Find("Player_Coin").GetComponent<Text>();
        enemy_coin_text = GameObject.Find("Enemy_Coin").GetComponent<Text>();
        a = GameObject.Find("Attack_Controller").GetComponent<Attack_Controller>();
        source = GameObject.Find("Music_Player").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player_coin > 10)
        {
            player_coin = 10;
        }
        if (enemy_coin > 10) 
        {
            enemy_coin = 10;
        }
        player_coin_text.GetComponent<Text>().text = player_coin.ToString() + "/10";
        enemy_coin_text.GetComponent<Text>().text = enemy_coin.ToString() + "/10";
    }

    public void Add_Coin(string target, int number)
    {
        if (target == "Enemy")
        {
            enemy_coin += number;
            StartCoroutine(a.Floating_Anim(enemy_text, number));
        }
        else if (target == "Player")
        {
            player_coin += number;
            StartCoroutine(a.Floating_Anim(player_text, number));
        }
        else
        {
            enemy_coin += number;
            player_coin += number;
            player_current_turn = 10;
            enemy_current_turn = 10;
            StartCoroutine(a.Floating_Anim(enemy_text, number));
            StartCoroutine(a.Floating_Anim(player_text, number));
        }
        source.PlayOneShot(coin_sound, 1);
    }

}
