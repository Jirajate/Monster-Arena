using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drop_Zone : MonoBehaviour , IDropHandler , IPointerEnterHandler , IPointerExitHandler
{
    private Attack_Controller a;

    private void Start()
    {
        a = GameObject.Find("Attack_Controller").GetComponent<Attack_Controller>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }
        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        if (d != null && !d.dropped)
        {
            Card_Detail card = eventData.pointerDrag.GetComponent<Card_Detail>();
            Coin_Manager c = GameObject.Find("Coin_Manager").GetComponent<Coin_Manager>();
            if (card.cost <= c.player_coin && this.transform.childCount < 7)
            {
                d.can_drop = true;
            }
            else
            {
                d.can_drop = false;
            }
            d.placeholderParrent = this.transform;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }
        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        if (d != null && !d.dropped && d.placeholderParrent == this.transform)
        {
            d.placeholderParrent = d.parentToReturnTo;
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        if(d != null && d.can_drop && !d.dropped && !a.clicked)
        {
            //Debug.Log(eventData.pointerDrag.name + " was drop on " + gameObject.name);
            Card_Detail card = eventData.pointerDrag.GetComponent<Card_Detail>();
            Coin_Manager c = GameObject.Find("Coin_Manager").GetComponent<Coin_Manager>();
            if (card.cost <= c.player_coin && this.name == "Player_Board")
            {
                c.player_coin -= card.cost;
                d.parentToReturnTo = this.transform;
                card.Drop_Card();
                card.GetComponent<BoxCollider2D>().enabled = true;
            }
            else
            {
                // Do Nothing
            }
        }
    }
}
