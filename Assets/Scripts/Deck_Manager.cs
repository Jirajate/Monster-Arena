using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck_Manager : MonoBehaviour
{
    public GameObject prepare_panel;
    public GameObject Card_Prefab;
    public int max_number = 25;
    public int deck_size = 30;
    public List<int> deck;
    public List<int> enemy_deck;

    private GameObject graveyard;
    private GameObject player_hand;
    private GameObject enemy_hand;
    private Match_Maker m;
    private List<int> numbers;
    private List<int> temp;

    void Start()
    {
        graveyard = GameObject.Find("Graveyard");
        m = GameObject.Find("Match_Maker").GetComponent<Match_Maker>();
        player_hand = GameObject.Find("Player_Hand");
        enemy_hand = GameObject.Find("Enemy_Hand");

        Generate_Deck();
    }

    public void Generate_Deck()
    {
        numbers = new List<int>(max_number);
        for (int i = 1; i <= max_number; i++)
        {
            numbers.Add(i);
        }
        for (int i = 1; i <= max_number; i++)
        {
            numbers.Add(i);
        }
        temp = new List<int>(numbers);
        for (int i = 0; i < deck_size; i++)
        {
            int number = Random.Range(0, temp.Count);
            deck.Add(temp[number]);
            temp.RemoveAt(number);
        }
        temp = new List<int>(numbers);
        for (int i = 0; i < deck_size; i++)
        {
            int number = Random.Range(0, temp.Count);
            enemy_deck.Add(temp[number]);
            temp.RemoveAt(number);
        }
        prepare_panel.SetActive(true);
    }

    public void Draw_Starting_Cards(GameObject starting_hand)
    {
        int id;
        for (int i = 0; i < 3; i++)
        {
            id = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            GameObject card = Instantiate(Card_Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            card.GetComponent<Draggable>().dropped = true;
            card.name = id.ToString();
            card.transform.SetParent(starting_hand.transform);
            card.transform.localScale = new Vector3(17, 17, 1);
        }
    }

    public void Insert_Card(int id)
    {
        int number = Random.Range(0, deck.Count);
        deck.Insert(number, id);
    }

    public void Draw_Card(string target)
    {
        int id;
        if (target == "Player")
        {
            if (deck.Count > 0)
            {
                id = deck[deck.Count - 1];
                deck.RemoveAt(deck.Count - 1);
                GameObject card = Instantiate(Card_Prefab, new Vector3(0, 0, 0), Quaternion.identity);
                card.name = id.ToString();
                card.GetComponent<Draggable>().enabled = false;
                if (player_hand.transform.childCount < 7)
                {
                    card.GetComponent<Draggable>().enabled = true;
                    card.transform.SetParent(player_hand.transform);
                }
                else
                {
                    card.transform.position = graveyard.transform.position;
                    card.transform.SetParent(graveyard.transform);
                }
            }
            else
            {
                // Deal Dmg to Hero
            }
        }
        else if (target == "Enemy")
        {
            if (enemy_deck.Count > 0)
            {
                id = enemy_deck[enemy_deck.Count - 1];
                enemy_deck.RemoveAt(enemy_deck.Count - 1);
                GameObject card = Instantiate(Card_Prefab, new Vector3(0, 0, 0), Quaternion.identity);
                card.name = id.ToString();
                card.GetComponent<Draggable>().enemy_card = true;
                if (enemy_hand.transform.childCount < 7)
                {
                    card.transform.SetParent(enemy_hand.transform);
                }
                else
                {
                    card.transform.position = graveyard.transform.position;
                    card.transform.SetParent(graveyard.transform);
                }
            }
            else
            {
                // Deal Dmg to Hero
            }
        }
    }

    public GameObject Add_Card(int id , Transform target)
    {
        GameObject card = Instantiate(Card_Prefab, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
        card.name = id.ToString();
        if (target.childCount < 7)
        {
            card.transform.SetParent(target);
            if(target.name == "Enemy_Hand" || target.name == "Enemy_Board")
            {
                card.GetComponent<Draggable>().enemy_card = true;
            }
        }
        else
        {
            card.transform.SetParent(graveyard.transform);
        }
        card.transform.localPosition = new Vector3(0, 0, 0);
        return card;
    }
}
