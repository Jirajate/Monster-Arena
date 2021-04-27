using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Enemy_AI : MonoBehaviour
{
    public Material[] hero_material;

    private Match_Maker m;
    private Coin_Manager c;
    private Attack_Controller a;
    private Transform player_board;
    private Transform enemy_board;
    private GameObject player_hero;
    private MeshRenderer enemy_mesh;
    void Start()
    {
        m = GameObject.Find("Match_Maker").GetComponent<Match_Maker>();
        c = GameObject.Find("Coin_Manager").GetComponent<Coin_Manager>();
        a = GameObject.Find("Attack_Controller").GetComponent<Attack_Controller>();
        player_hero = GameObject.Find("Player_Hero");
        enemy_mesh = GameObject.Find("Enemy_Hero").GetComponent<MeshRenderer>();
        player_board = GameObject.Find("Player_Board").transform;
        enemy_board = GameObject.Find("Enemy_Board").transform;
        enemy_mesh.material = hero_material[m.level];
    }

    public int Pick_A_Card(List<int> index, bool zero)
    {
        float max = 0;
        float s = 0;
        int selected = 0;
        Card_Detail cd;
        for (int i = 0; i < index.Count; i++)
        {
            cd = this.transform.GetChild(index[i]).GetComponent<Card_Detail>();
            if (zero)
            {
                s = (cd.atk + cd.hp) / cd.cost;
            }
            else
            {
                s = (cd.atk + cd.hp + cd.intrinsic) / cd.cost;
            }
            if (max <= s)
            {
                max = s;
                selected = index[i];
            }
        }
        return selected;
    }

    public void Drop_Card(int index)
    {
        Transform card;
        Card_Detail cd;
        card = this.transform.GetChild(index);
        cd = card.GetComponent<Card_Detail>();
        if (cd.cost <= c.enemy_coin && enemy_board.childCount < 7)
        {
            cd.Drop_Card();
            card.SetParent(enemy_board);
            card.transform.localPosition = new Vector3(0, 0, 0);
            cd.GetComponent<BoxCollider2D>().enabled = true;
            c.enemy_coin -= cd.cost;
            StartCoroutine(Show_Detail(card));
        }
    }

    public List<GameObject> Sort_Max_Atk()
    {
        List<GameObject> card = new List<GameObject>();
        if (player_board.childCount == 0)
        {
            return null;
        }
        else
        {
            foreach (Transform item in player_board)
            {
                card.Add(item.gameObject);
            }
            card = card.OrderByDescending(x => x.GetComponent<Card_Detail>().atk).ToList();
            return card;
        }
    }

    public bool Domination_Check()
    {
        int enemy_sum = 0;
        int player_sum = 0;
        foreach (Transform item in enemy_board)
        {
            enemy_sum += item.GetComponent<Card_Detail>().atk;
        }
        foreach (Transform item in player_board)
        {
            player_sum += item.GetComponent<Card_Detail>().atk;
        }
        if (enemy_sum > player_sum)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator Turn_Start()
    {
        yield return new WaitForSeconds(1);
        enemy_board.SetSiblingIndex(4);
        yield return StartCoroutine(Play_Card());
        yield return StartCoroutine(Attack_Player());
        enemy_board.SetSiblingIndex(3);
        yield return new WaitForSeconds(0.25f);
        m.End_Turn();
    }

    public IEnumerator Show_Detail(Transform card)
    {
        yield return new WaitForEndOfFrame();
        card.GetComponent<Draggable>().Show_Detail();
        yield return new WaitForSeconds(2.5f);
        card.GetComponent<Draggable>().Hide_Detail();
    }

    public IEnumerator Play_Card()
    {
        if (this.transform.childCount > 0)
        {
            yield return new WaitForSeconds(0.5f);
            List<int> playable_index = new List<int>();
            foreach (Transform card in this.transform)
            {
                Card_Detail cd = card.GetComponent<Card_Detail>();
                if (cd.cost <= c.enemy_coin)
                {
                    playable_index.Add(card.GetSiblingIndex());
                }
            }
            if (playable_index.Count != 0)
            {
                int index;
                switch (m.level)
                {
                    case 0:
                        int number = Random.Range(0, playable_index.Count);
                        index = playable_index[number];
                        Drop_Card(index);
                        break;
                    case 1:
                        index = Pick_A_Card(playable_index, true);
                        Drop_Card(index);
                        break;
                    case 2:
                        index = Pick_A_Card(playable_index, true);
                        Drop_Card(index);
                        break;
                    case 3:
                        index = Pick_A_Card(playable_index, true);
                        Drop_Card(index);
                        break;
                    case 4:
                        index = Pick_A_Card(playable_index, false);
                        Drop_Card(index);
                        break;
                }
                yield return new WaitForSeconds(2.5f);
            }
        }
    }

    public IEnumerator Attack_Player()
    {
        if (enemy_board.transform.childCount > 0)
        {
            yield return new WaitForSeconds(0.25f);
            foreach (Transform item in enemy_board.transform)
            {
                Card_Detail cd = item.GetComponent<Card_Detail>();
                Card_Detail target_cd = null;
                List<GameObject> targets = null;
                bool attacked = false;
                if (cd.attackable)
                {
                    yield return new WaitForSeconds(1);
                    switch (m.level)
                    {
                        case 0:
                            yield return StartCoroutine(a.Attack(item.gameObject, a.Random_Target("Player")));
                            break;
                        case 1:
                            targets = Sort_Max_Atk();
                            if (targets != null)
                            {
                                yield return StartCoroutine(a.Attack(item.gameObject, targets[0]));
                            }
                            else
                            {
                                yield return StartCoroutine(a.Attack(item.gameObject, player_hero));
                            }
                            break;
                        case 2:
                            targets = Sort_Max_Atk();
                            if (targets != null)
                            {
                                for (int i = 0; i < targets.Count; i++)
                                {
                                    target_cd = targets[i].GetComponent<Card_Detail>();
                                    if (target_cd.hp <= cd.atk && !target_cd.undead)
                                    {
                                        yield return StartCoroutine(a.Attack(item.gameObject, targets[i]));
                                        attacked = true;
                                        break;
                                    }
                                }
                                if (!attacked)
                                {
                                    yield return StartCoroutine(a.Attack(item.gameObject, player_hero));
                                }
                            }
                            else
                            {
                                yield return StartCoroutine(a.Attack(item.gameObject, player_hero));
                            }
                            break;
                        case 3:
                            targets = Sort_Max_Atk();
                            if (targets != null && !Domination_Check())
                            {
                                for (int i = 0; i < targets.Count; i++)
                                {
                                    target_cd = targets[i].GetComponent<Card_Detail>();
                                    if (target_cd.hp <= cd.atk && !target_cd.undead)
                                    {
                                        yield return StartCoroutine(a.Attack(item.gameObject, targets[i]));
                                        attacked = true;
                                        break;
                                    }
                                }
                                if (!attacked)
                                {
                                    yield return StartCoroutine(a.Attack(item.gameObject, player_hero));
                                }
                            }
                            else
                            {
                                yield return StartCoroutine(a.Attack(item.gameObject, player_hero));
                            }
                            break;
                        case 4:
                            targets = Sort_Max_Atk();
                            if (targets != null && !Domination_Check())
                            {
                                for (int i = 0; i < targets.Count; i++)
                                {
                                    target_cd = targets[i].GetComponent<Card_Detail>();
                                    if (target_cd.hp <= cd.atk && !target_cd.undead)
                                    {
                                        yield return StartCoroutine(a.Attack(item.gameObject, targets[i]));
                                        attacked = true;
                                        break;
                                    }
                                }
                                if (!attacked)
                                {
                                    yield return StartCoroutine(a.Attack(item.gameObject, player_hero));
                                }
                            }
                            else
                            {
                                yield return StartCoroutine(a.Attack(item.gameObject, player_hero));
                            }
                            break;
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}
