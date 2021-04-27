using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Match_Maker : MonoBehaviour
{
    public bool reveal_enemy = false;
    public bool game_start = false;
    public bool turn_ended = false;
    public int level = 0;
    public int player_hp = 30;
    public int enemy_hp = 30;
    public Card[] Collection;
    public int Collection_Size = 0;
    public Button end_button;
    public Image end_panel;
    public Sprite victory;
    public Sprite defeat;
    public AudioClip turn_sound;
    public AudioClip victory_sound;
    public AudioClip defeat_sound;

    private Coin_Manager c;
    private Deck_Manager d;
    private Enemy_AI ai;
    private Attack_Controller a;
    private Game_Manager gm;
    private GameObject player_hand;
    private GameObject enemy_hand;
    private Text player_hp_text;
    private Text enemy_hp_text;
    private AudioSource source;
    private Vector3 original_pos;
    private bool ending = false;
    public struct Card
    {
        public string id;
        public string img;
        public string name;
        public int cost;
        public int atk;
        public int hp;
        public string effect;
        public int i;
    }

    void Start()
    {
        TextAsset textFile = Resources.Load<TextAsset>("Card_Data");
        string[] lines = textFile.text.Split("\n"[0]);
        Collection = new Card[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            string[] item = lines[i].Split(',');
            Collection[i].id = item[0];
            Collection[i].img = item[1];
            Collection[i].name = item[2];
            Collection[i].cost = int.Parse(item[3]);
            Collection[i].atk = int.Parse(item[4]);
            Collection[i].hp = int.Parse(item[5]);
            Collection[i].effect = item[6];
            Collection[i].i = int.Parse(item[7]);
            Collection_Size++;
        }
        c = GameObject.Find("Coin_Manager").GetComponent<Coin_Manager>();
        d = GameObject.Find("Deck_Manager").GetComponent<Deck_Manager>();
        ai = GameObject.Find("Enemy_Hand").GetComponent<Enemy_AI>();
        a = GameObject.Find("Attack_Controller").GetComponent<Attack_Controller>();
        gm = GameObject.Find("Game_Manager").GetComponent<Game_Manager>();
        player_hand = GameObject.Find("Player_Hand");
        enemy_hand = GameObject.Find("Enemy_Hand");
        player_hp_text = GameObject.Find("Player_HP").GetComponent<Text>();
        enemy_hp_text = GameObject.Find("Enemy_HP").GetComponent<Text>();
        source = GameObject.Find("Music_Player").GetComponent<AudioSource>();
        level = gm.current_level;
        original_pos = enemy_hand.transform.position;
        end_panel.gameObject.SetActive(false);
    }
    void Update()
    {
        if (player_hp > 30)
        {
            player_hp = 30;
        }
        else if (player_hp <= 0 && !turn_ended)
        {
            if (!ending)
            {
                end_panel.sprite = defeat;
                end_panel.gameObject.SetActive(true);
                source.PlayOneShot(defeat_sound, 0.5f);
                ending = true;
            }
        }
        if (enemy_hp > 30)
        {
            enemy_hp = 30;
        }
        else if (enemy_hp <= 0)
        {
            if (!ending)
            {
                end_panel.sprite = victory;
                end_panel.gameObject.SetActive(true);
                source.PlayOneShot(victory_sound, 0.5f);
                if (level >= gm.unlocked_level)
                {
                    gm.unlocked_level += 1;
                }
                ending = true;
            }

        }

        player_hp_text.text = player_hp.ToString();
        enemy_hp_text.text = enemy_hp.ToString();

        // Keyboard Shortcut
        if (Input.GetKeyDown(KeyCode.R))
        {
            reveal_enemy = !reveal_enemy;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            d.Add_Card(13, enemy_hand.transform);
            d.Add_Card(5, enemy_hand.transform);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            d.Add_Card(20, player_hand.transform);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            c.Add_Coin("All", 10);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            enemy_hp = 1;
            player_hp = 1;
        }

        // Reveal Enemy Hand
        if (reveal_enemy)
        {
            enemy_hand.transform.position = new Vector3(original_pos.x, original_pos.y - 1, original_pos.z);
            foreach (Transform Card in enemy_hand.transform)
            {
                Card.GetComponent<Card_Detail>().show_img = true;
            }
        }
        else
        {
            enemy_hand.transform.position = original_pos;
            foreach (Transform Card in enemy_hand.transform)
            {
                Card.GetComponent<Card_Detail>().show_img = false;
            }
        }
    }

    public void End_Turn()
    {
        turn_ended = !turn_ended;
        source.PlayOneShot(turn_sound, 1);
        if (turn_ended)
        {
            a.clicked = false;
            c.enemy_current_turn++;
            if (c.enemy_current_turn < 10)
            {
                c.enemy_coin = c.enemy_current_turn;
            }
            else
            {
                c.enemy_coin = 10;
            }
            d.Draw_Card("Enemy");
            end_button.interactable = false;
            StartCoroutine(ai.Turn_Start());
        }
        else
        {
            c.player_current_turn++;
            if (c.player_current_turn < 10)
            {
                c.player_coin = c.player_current_turn;
            }
            else
            {
                c.player_coin = 10;
            }
            d.Draw_Card("Player");
            end_button.interactable = true;
        }
    }
}