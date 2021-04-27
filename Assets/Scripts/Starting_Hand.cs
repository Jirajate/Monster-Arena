using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Starting_Hand : MonoBehaviour, IPointerClickHandler
{
    public GameObject[] mark;
    public bool[] selected_card = new bool[3];
    public AudioClip mark_sound;
    public AudioClip click_sound;

    private Transform player_hand;
    private Deck_Manager d;
    private Coin_Manager c;
    private AudioSource source;

    public void Start()
    {
        player_hand = GameObject.Find("Player_Hand").transform;
        d = GameObject.Find("Deck_Manager").GetComponent<Deck_Manager>();
        c = GameObject.Find("Coin_Manager").GetComponent<Coin_Manager>();
        source = GameObject.Find("Music_Player").GetComponent<AudioSource>();
        d.Draw_Starting_Cards(this.gameObject);
    }

    public void Confirmed()
    {
        GameObject prepare_phase = this.transform.parent.gameObject;
        c.player_coin = c.player_current_turn;
        source.PlayOneShot(click_sound, 1);
        for (int i = 0; i < 3; i++)
        {
            Transform card = this.transform.GetChild(i);
            if (selected_card[i])
            {
                d.Draw_Card("Player");
                d.Insert_Card(int.Parse(card.name));
            }
            else
            {
                d.Add_Card(int.Parse(card.name), player_hand);
            }
        }
        for (int i = 0; i < 4; i++)
        {
            d.Draw_Card("Enemy");
        }
        prepare_phase.SetActive(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Transform card = eventData.pointerDrag.transform;
        int index = card.GetSiblingIndex();
        source.PlayOneShot(mark_sound, 1);
        if (!selected_card[index])
        {
            mark[index].SetActive(true);
            selected_card[index] = true;
        }
        else
        {
            mark[index].SetActive(false);
            selected_card[index] = false;
        }
    }

}
