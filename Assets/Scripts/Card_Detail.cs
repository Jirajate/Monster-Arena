using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card_Detail : MonoBehaviour
{
    public bool show_img = true;
    public Image img;
    public Image holder;
    public Text cost_text;
    public Text atk_text;
    public Text hp_text;
    public Match_Maker.Card my_card;
    public int cost;
    public int atk;
    public int hp;
    public int intrinsic;
    public bool undead = false;
    public bool dropped = false;
    public bool attackable = false;
    public bool died = false;

    public float dead_duration = 0.35f;
    public int wait_for = 1;
    public int current_turn;
    public int attackable_on;
    public AudioClip drop_sound;
    public AudioClip heal_sound;
    public AudioClip buff_sound;
    public AudioClip coin_sound;

    private Attack_Controller a;
    private Match_Maker m;
    private Coin_Manager c;
    private Deck_Manager d;
    private GameObject player_hero;
    private GameObject enemy_hero;
    private Transform enemy_board;
    private Transform player_board;
    private Transform enemy_hand;
    private Transform player_hand;
    private Transform graveyard;
    private Transform parent;
    private AudioSource source;
    void Start()
    {
        a = GameObject.Find("Attack_Controller").GetComponent<Attack_Controller>();
        m = GameObject.Find("Match_Maker").GetComponent<Match_Maker>();
        c = GameObject.Find("Coin_Manager").GetComponent<Coin_Manager>();
        d = GameObject.Find("Deck_Manager").GetComponent<Deck_Manager>();
        enemy_hero = GameObject.Find("Enemy_Hero");
        player_hero = GameObject.Find("Player_Hero");
        enemy_board = GameObject.Find("Enemy_Board").transform;
        player_board = GameObject.Find("Player_Board").transform;
        enemy_hand = GameObject.Find("Enemy_Hand").transform;
        player_hand = GameObject.Find("Player_Hand").transform;
        graveyard = GameObject.Find("Graveyard").transform;
        source = GameObject.Find("Music_Player").GetComponent<AudioSource>();
        my_card = m.Collection[int.Parse(this.name) - 1]; // * Collection Array Start at 0 *
        cost = my_card.cost;
        atk = my_card.atk;
        hp = my_card.hp;
        intrinsic = my_card.i;
    }

    void LateUpdate()
    {
        cost_text.text = cost.ToString();
        atk_text.text = atk.ToString();
        hp_text.text = hp.ToString();
        parent = this.transform.parent;
        Text_Update();

        if (show_img)
        {
            holder.enabled = true;
            cost_text.enabled = true;
            atk_text.enabled = true;
            hp_text.enabled = true;
            img.sprite = Resources.Load<Sprite>("Cards/" + my_card.img);
        }
        else
        {
            holder.enabled = false;
            cost_text.enabled = false;
            atk_text.enabled = false;
            hp_text.enabled = false;
            if (parent.name == "Enemy_Hand" || parent.name == "Enemy_Board")
            {
                img.sprite = Resources.Load<Sprite>("Cards/Card_Back_01");
            }
            else
            {
                img.sprite = Resources.Load<Sprite>("Cards/Card_Back_02");
            }
        }

        if (parent.name == "Enemy_Board" && c.enemy_current_turn >= attackable_on)
        {
            attackable = true;
        }
        else if (parent.name == "Player_Board" && c.player_current_turn >= attackable_on)
        {
            attackable = true;
        }
    }

    public void Text_Update()
    {
        // Cost Text
        if (cost < my_card.cost)
        {
            cost_text.color = Color.green;
        }
        else if (cost > my_card.cost)
        {
            cost_text.color = Color.red;
        }
        else
        {
            cost_text.color = Color.white;
        }

        //ATK Text
        if (atk < my_card.atk)
        {
            atk_text.color = Color.red;
        }
        else if (atk > my_card.atk)
        {
            atk_text.color = Color.green;
        }
        else
        {
            atk_text.color = Color.white;
        }

        //HP Text
        if (hp < my_card.hp)
        {
            hp_text.color = Color.red;
            if (hp <= 0 && !died && dropped)
            {
                if (!undead)
                {
                    StartCoroutine(Move_To_Graveyard());
                    died = true;
                }
                else
                {
                    hp = 0;
                }
            }
        }
        else if (hp > my_card.hp)
        {
            hp_text.color = Color.green;
        }
        else
        {
            hp_text.color = Color.white;
        }
    }

    public void Set_Default()
    {
        cost = my_card.cost;
        atk = my_card.atk;
        hp = my_card.hp;
        cost_text.text = cost.ToString();
        atk_text.text = atk.ToString();
        hp_text.text = hp.ToString();
    }
    public void Calculate_Turn()
    {
        if (parent.name == "Enemy_Board")
        {
            current_turn = c.enemy_current_turn;
        }
        else
        {
            current_turn = c.player_current_turn;
        }
        attackable_on = current_turn + wait_for;
        attackable = false;
    }

    public void Drop_Card()
    {
        Calculate_Turn();
        dropped = true;
        show_img = true;
        StartCoroutine(this.GetComponent<Draggable>().Change_Dropped());
        StartCoroutine(Resolve_Effect());
        source.PlayOneShot(drop_sound, 1);
        source.PlayOneShot(coin_sound, 0.6f);
    }

    public IEnumerator Clone_Card()
    {
        GameObject clone = d.Add_Card(int.Parse(this.name), parent);
        Card_Detail clone_cd = clone.GetComponent<Card_Detail>();
        StartCoroutine(clone_cd.GetComponent<Draggable>().Change_Dropped());
        yield return new WaitForEndOfFrame();
        clone_cd.cost = cost;
        clone_cd.atk = atk;
        clone_cd.hp = hp;
        clone_cd.dropped = true;
        clone_cd.Calculate_Turn();
        source.PlayOneShot(drop_sound, 1);
    }

    public IEnumerator Move_To_Graveyard()
    {
        yield return new WaitForSeconds(0.25f);
        Vector3 pos_1 = this.transform.position;
        Vector3 pos_2 = graveyard.transform.position;
        float journey = 0f;
        while (journey <= dead_duration)
        {
            journey += Time.deltaTime;
            float percent = Mathf.Clamp01(journey / dead_duration);
            this.transform.position = Vector3.Lerp(pos_1, pos_2, percent);
            yield return null;
        }
        this.transform.SetParent(graveyard);
        this.transform.localPosition = new Vector3(0, 0, 0);
        this.GetComponent<BoxCollider2D>().enabled = false;
        Set_Default();
    }

    public IEnumerator Resolve_Effect()
    {
        GameObject target;
        Card_Detail target_cd;
        yield return new WaitForSeconds(1);
        switch (int.Parse(this.name))
        {
            case 1:
                // Corrupted Crap,3,2,2,Summon a copy of it when play this card.
                if (parent.childCount < 7)
                {
                    StartCoroutine(Clone_Card());
                }
                break;
            case 2:
                // Tundra Beast,2,4,3,Randomly attack enemy when play this card.
                if (parent == player_board)
                {
                    target = a.Random_Target("Enemy");
                }
                else
                {
                    target = a.Random_Target("Player");
                }
                StartCoroutine(a.Attack(this.gameObject, target));
                break;
            case 3:
                // Red Parrot,1,1,2,
                // No Effect
                break;
            case 4:
                // Tiny Devourer,6,1,1,Randomly destroy an enemy monster when play this card.
                if (parent == player_board)
                {
                    if (enemy_board.childCount > 0)
                    {
                        target = enemy_hero;
                        while (target == enemy_hero || target == player_hero)
                        {
                            target = a.Random_Target("Enemy");
                        }
                        target_cd = target.GetComponent<Card_Detail>();
                        StartCoroutine(a.Destroy_Card(target));
                    }
                }
                else
                {
                    if (player_board.childCount > 0)
                    {
                        target = player_hero;
                        while (target == enemy_hero || target == player_hero)
                        {
                            target = a.Random_Target("Player");
                        }
                        target_cd = target.GetComponent<Card_Detail>();
                        StartCoroutine(a.Destroy_Card(target));
                    }
                }
                break;
            case 5:
                // Red Fin Lizard,3,2,2,
                // No Effect
                break;
            case 6:
                // Skull Tribe,2,2,3,Randomly deal 2 damage to enemy when play this card.
                if (parent == player_board)
                {
                    target = a.Random_Target("Enemy");
                }
                else
                {
                    target = a.Random_Target("Player");
                }
                StartCoroutine(a.Deal_Damage(this.gameObject, target, 2));
                break;
            case 7:
                // Kangasaurus,3,3,5,Randomly attack something when play this card.
                target = this.gameObject;
                while (target == this.gameObject)
                {
                    target = a.Random_Target("All");
                }
                StartCoroutine(a.Attack(this.gameObject, target));
                break;
            case 8:
                // Predator Of The Depth,8,3,3,Randomly destroy an enemy monster when play this card and gain attack and health of it.
                if (parent == player_board)
                {
                    if (enemy_board.childCount > 0)
                    {
                        target = enemy_hero;
                        while (target == enemy_hero || target == player_hero)
                        {
                            target = a.Random_Target("Enemy");
                        }
                        target_cd = target.GetComponent<Card_Detail>();
                        int target_atk = target_cd.atk;
                        int target_hp = target_cd.hp;
                        yield return StartCoroutine(a.Destroy_Card(target));
                        yield return new WaitForSeconds(1);
                        atk += target_atk;
                        hp += target_hp;
                    }
                }
                else
                {
                    if (player_board.childCount > 0)
                    {
                        target = player_hero;
                        while (target == enemy_hero || target == player_hero)
                        {
                            target = a.Random_Target("Player");
                        }
                        target_cd = target.GetComponent<Card_Detail>();
                        int target_atk = target_cd.atk;
                        int target_hp = target_cd.hp;
                        yield return StartCoroutine(a.Destroy_Card(target));
                        yield return new WaitForSeconds(1);
                        atk += target_atk;
                        hp += target_hp;
                    }
                }
                source.PlayOneShot(buff_sound, 0.6f);
                break;
            case 9:
                // Despaired Knight,1,3,3,Randomly deal 3 damage to a monster if it not die this monster will be destroy.
                if (parent == player_board)
                {
                    if (enemy_board.childCount > 0)
                    {
                        target = enemy_hero;
                        while (target == enemy_hero || target == player_hero)
                        {
                            target = a.Random_Target("Enemy");
                        }
                        target_cd = target.GetComponent<Card_Detail>();
                        yield return StartCoroutine(a.Deal_Damage(this.gameObject, target, 3));
                        yield return new WaitForSeconds(1);
                        if (!target_cd.died)
                        {
                            yield return StartCoroutine(a.Deal_Damage(this.gameObject, this.gameObject, 3));

                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(1);
                        yield return StartCoroutine(a.Deal_Damage(this.gameObject, this.gameObject, 3));
                    }
                }
                else
                {
                    if (player_board.childCount > 0)
                    {
                        target = player_hero;
                        while (target == enemy_hero || target == player_hero)
                        {
                            target = a.Random_Target("Player");
                        }
                        target_cd = target.GetComponent<Card_Detail>();
                        yield return StartCoroutine(a.Deal_Damage(this.gameObject, target, 3));
                        yield return new WaitForSeconds(1);
                        if (!target_cd.died)
                        {
                            yield return StartCoroutine(a.Deal_Damage(this.gameObject, this.gameObject, 3));
                            hp = 0;
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(1);
                        yield return StartCoroutine(a.Deal_Damage(this.gameObject, this.gameObject, 3));
                        hp = 0;
                    }
                }
                break;
            case 10:
                // Forest Frog,3,1,1,Summon 2 copy of it when play this card.
                for (int i = 0; i < 2; i++)
                {
                    if (parent.childCount < 7)
                    {
                        StartCoroutine(Clone_Card());
                    }
                }
                break;
            case 11:
                // Goblin Miner,1,1,1,Give you 2 coin when play this card. 
                if (parent == player_board)
                {
                    c.Add_Coin("Player", 2);
                }
                else
                {
                    c.Add_Coin("Enemy", 2);
                }
                break;
            case 12:
                // Mountain Yak,9,10,10,Can attack instantly when play this card.
                attackable_on = current_turn;
                attackable = true;
                break;
            case 13:
                // Brainless Ghoul,3,1,1,Randomly give +3 health to a card in your hand when play this card.
                source.PlayOneShot(buff_sound, 0.6f);
                if (parent == player_board)
                {
                    int number;
                    if (player_hand.childCount > 0)
                    {
                        number = Random.Range(0, player_hand.transform.childCount);
                        Card_Detail selected_cd = player_hand.GetChild(number).GetComponent<Card_Detail>();
                        selected_cd.hp += 3;
                    }
                }
                else
                {
                    int number;
                    if (enemy_hand.childCount > 0)
                    {
                        number = Random.Range(0, enemy_hand.transform.childCount);
                        Card_Detail selected_cd = enemy_hand.GetChild(number).GetComponent<Card_Detail>();
                        selected_cd.hp += 3;
                    }
                }
                break;
            case 14:
                // Halloween Spirit,6,6,6,Deal 6 Damage to all monsters when play this card.
                if (player_board.childCount > 0)
                {
                    foreach (Transform item in player_board)
                    {
                        if (item != this.transform)
                        {
                            StartCoroutine(a.Deal_Damage(this.gameObject, item.gameObject, 6));
                        }
                    }
                }
                if (enemy_board.childCount > 0)
                {
                    foreach (Transform item in enemy_board)
                    {
                        if (item != this.transform)
                        {
                            StartCoroutine(a.Deal_Damage(this.gameObject, item.gameObject, 6));
                        }
                    }
                }
                break;
            case 15:
                // Angler Man,2,2,2,Restore 2 health to your hero when play this card.
                source.PlayOneShot(heal_sound, 0.5f);
                if (parent == player_board)
                {
                    m.player_hp += 2;
                    yield return StartCoroutine(a.Floating_Anim(player_hero, 2));
                }
                else
                {
                    m.enemy_hp += 2;
                    yield return StartCoroutine(a.Floating_Anim(enemy_hero, 2));
                }
                break;
            case 16:
                // Baby Dragon,1,1,1,Draw a card when play this card.
                source.PlayOneShot(drop_sound, 1);
                if (parent == player_board)
                {
                    d.Draw_Card("Player");
                }
                else
                {
                    d.Draw_Card("Enemy");
                }
                break;
            case 17:
                // Armorturtle,5,4,8,
                // No Effect
                break;
            case 18:
                // Baal The Soulkeeper,10,1,1,+1 attack and +1 health for each cards in graveyard when play this card.
                source.PlayOneShot(buff_sound, 0.6f);
                atk += graveyard.childCount;
                hp += graveyard.childCount;
                break;
            case 19:
                // Merman,4,4,3,Can attack instantly when play this card.
                attackable_on = current_turn;
                attackable = true;
                break;
            case 20:
                // Tiny Skeleton,1,1,1,This monster can't die.
                undead = true;
                break;
            case 21:
                // Scavenger,5,3,4,Give +1 attack to all cards in your hand when play this card.
                source.PlayOneShot(buff_sound, 0.6f);
                if (parent == player_board)
                {
                    if (player_hand.childCount > 0)
                    {
                        foreach (Transform item in player_hand)
                        {
                            Card_Detail item_cd = item.GetComponent<Card_Detail>();
                            item_cd.atk += 1;
                        }
                    }
                }
                else
                {
                    if (enemy_hand.childCount > 0)
                    {
                        foreach (Transform item in enemy_hand)
                        {
                            Card_Detail item_cd = item.GetComponent<Card_Detail>();
                            item_cd.atk += 1;
                        }
                    }
                }
                break;
            case 22:
                // Cultist Mage,5,3,3,Deal 3 damage to all enemy monsters when play this card.
                if (parent == player_board)
                {
                    if (enemy_board.childCount > 0)
                    {
                        foreach (Transform item in enemy_board)
                        {
                            StartCoroutine(a.Deal_Damage(this.gameObject, item.gameObject, 3));
                        }
                    }
                }
                else
                {
                    if (player_board.childCount > 0)
                    {
                        foreach (Transform item in player_board)
                        {
                            StartCoroutine(a.Deal_Damage(this.gameObject, item.gameObject, 3));
                        }
                    }
                }
                break;
            case 23:
                // Baby Knight,1,3,3,
                // No Effect
                break;
            case 24:
                // Gargoyle,6,5,5,Restore 5 health to your hero when play this card.
                source.PlayOneShot(heal_sound, 0.5f);
                if (parent == player_board)
                {
                    m.player_hp += 5;
                    yield return StartCoroutine(a.Floating_Anim(player_hero, 5));
                }
                else
                {
                    m.enemy_hp += 5;
                    yield return StartCoroutine(a.Floating_Anim(enemy_hero, 5));
                }
                break;
            case 25:
                // Grand Dwarf,7,4,5,Give +2 health to all cards in your hand when play this card.
                source.PlayOneShot(buff_sound, 0.6f);
                if (parent == player_board)
                {
                    if (player_hand.childCount > 0)
                    {
                        foreach (Transform item in player_hand)
                        {
                            Card_Detail item_cd = item.GetComponent<Card_Detail>();
                            item_cd.hp += 2;
                        }
                    }
                }
                else
                {
                    if (enemy_hand.childCount > 0)
                    {
                        foreach (Transform item in enemy_hand)
                        {
                            Card_Detail item_cd = item.GetComponent<Card_Detail>();
                            item_cd.hp += 2;
                        }
                    }
                }
                break;
        }
    }

}
