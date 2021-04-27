using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Transform parentToReturnTo = null;
    public Transform placeholderParrent = null;
    public bool enemy_card = false;
    public bool dropped = false;
    public bool can_drop = true;
    public float anim_duration = 0.1f;
    public GameObject detail_prefab;
    public AudioClip hover_sound;

    private Canvas World_Canvas;
    private Vector2 pos;
    private GameObject placeholder = null;
    private int original_index;
    private Match_Maker m;
    private Attack_Controller a;
    private Card_Detail cd;
    private GameObject detail;
    private Vector3 original_scale;
    private bool hovering = false;
    private HorizontalLayoutGroup hlg;
    private AudioSource source;
    void Start()
    {
        m = GameObject.Find("Match_Maker").GetComponent<Match_Maker>();
        a = GameObject.Find("Attack_Controller").GetComponent<Attack_Controller>();
        cd = this.gameObject.GetComponent<Card_Detail>();
        original_scale = this.transform.localScale;
        hlg = this.transform.parent.GetComponent<HorizontalLayoutGroup>();
        original_index = this.transform.GetSiblingIndex();
        source = GameObject.Find("Music_Player").GetComponent<AudioSource>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!dropped && !a.clicked && !enemy_card)
        {
            placeholder = new GameObject();
            placeholder.transform.SetParent(this.transform.parent);
            LayoutElement le = placeholder.AddComponent<LayoutElement>();
            le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
            le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;

            placeholder.transform.SetSiblingIndex(this.transform.parent.GetSiblingIndex());
            placeholder.GetComponent<RectTransform>().sizeDelta = new Vector2(le.preferredWidth, le.preferredHeight);

            parentToReturnTo = this.transform.parent;
            placeholderParrent = parentToReturnTo;
            this.transform.SetParent(this.transform.parent.parent);
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dropped && !a.clicked && !enemy_card)
        {
            this.transform.SetSiblingIndex(99);
            World_Canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(World_Canvas.transform as RectTransform, eventData.position, World_Canvas.worldCamera, out pos);
            this.transform.position = World_Canvas.transform.TransformPoint(pos);

            if (placeholder.transform.parent != placeholderParrent && can_drop)
            {
                placeholder.transform.SetParent(placeholderParrent);
            }

            int newSiblingIndex = placeholderParrent.childCount;
            if (placeholderParrent.name == "Player_Board" && can_drop)
            {
                for (int i = 0; i < placeholderParrent.childCount; i++)
                {
                    if (this.transform.position.x < placeholderParrent.GetChild(i).position.x)
                    {
                        newSiblingIndex = i;
                        if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                        {
                            newSiblingIndex--;
                        }
                        break;
                    }
                }
            }
            else
            {
                newSiblingIndex = original_index;
            }
            placeholder.transform.SetSiblingIndex(newSiblingIndex);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dropped && !a.clicked && !enemy_card)
        {
            StartCoroutine(Drop_Anim());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!m.turn_ended && !a.clicked && a.anim_finish)
        {
            hovering = true;
            if (this.transform.parent.name == "Player_Hand")
            {
                original_index = this.transform.GetSiblingIndex();
                this.transform.SetSiblingIndex(99);
                hlg.enabled = false;
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.6f, this.transform.position.z);
                this.transform.localScale = new Vector3(original_scale.x * 1.2f, original_scale.y * 1.2f, original_scale.z);
            }
            else if (this.transform.parent.name == "Enemy_Hand" && m.reveal_enemy)
            {
                original_index = this.transform.GetSiblingIndex();
                this.transform.SetSiblingIndex(99);
                hlg.enabled = false;
                this.transform.localScale = new Vector3(original_scale.x * 1.2f, original_scale.y * 1.2f, original_scale.z);
            }
            Show_Detail();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hovering)
        {
            hovering = false;
            if (this.transform.parent.name == "Player_Hand" || this.transform.parent.name == "Canvas")
            {
                hlg.enabled = true;
                this.transform.SetSiblingIndex(original_index);
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - 0.6f, this.transform.position.z);
                this.transform.localScale = original_scale;
            }
            else if(this.transform.parent.name == "Enemy_Hand" && m.reveal_enemy)
            {
                hlg.enabled = true;
                this.transform.SetSiblingIndex(original_index);
                this.transform.localScale = original_scale;
            }
            Hide_Detail();
        }
    }

    public void Show_Detail()
    {
        Vector3 original_pos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        detail = Instantiate(detail_prefab, new Vector3(0, 0, 0), Quaternion.identity);
        if (this.transform.parent.name == "Player_Hand")
        {
            pos = new Vector3(original_pos.x + 2.3f, original_pos.y + 0.55f, original_pos.z);
        }
        else if (this.transform.parent.name == "Starting_Hand")
        {
            pos = new Vector3(original_pos.x + 2.8f, original_pos.y + 1.1f, original_pos.z);
        }
        else if (this.transform.parent.name == "Enemy_Hand" && m.reveal_enemy)
        {
            pos = new Vector3(original_pos.x + 2.3f, original_pos.y + 0.55f, original_pos.z);
        }
        else if (this.transform.parent.name == "Player_Board" || this.transform.parent.name == "Enemy_Board")
        {
            pos = new Vector3(original_pos.x + 2.2f, original_pos.y + 0.33f, original_pos.z);
        }
        else
        {
            return;
        }
        detail.transform.SetParent(this.transform.parent.parent);
        detail.transform.position = pos;
        detail.transform.localScale = new Vector3(1, 1, 1);

        Vector2 screen_pos = Camera.main.WorldToScreenPoint(detail.transform.position);
        float size = detail.GetComponent<RectTransform>().sizeDelta.x;
        if ((screen_pos.x + size) > Screen.currentResolution.width)
        {
            if (this.transform.parent.name == "Player_Hand" || this.transform.parent.name == "Enemy_Hand")
            {
                pos = new Vector3(original_pos.x - 2.3f, original_pos.y + 0.55f, original_pos.z);
            }
            else if (this.transform.parent.name == "Player_Board" || this.transform.parent.name == "Enemy_Board")
            {
                pos = new Vector3(original_pos.x - 2.2f, original_pos.y + 0.33f, original_pos.z);
            }
            detail.transform.position = pos;
        }
        Text card_name = detail.transform.GetChild(0).GetComponent<Text>();
        Text card_effect = detail.transform.GetChild(1).GetComponent<Text>();
        card_name.text = cd.my_card.name;
        if (this.transform.parent.name == "Enemy_Hand")
        {
            float s;
            if (m.level < 4)
            {
                s = (cd.atk + cd.hp) / cd.cost;
            }
            else
            {
                s = (cd.atk + cd.hp + cd.intrinsic) / cd.cost;
            }
            card_effect.text = cd.my_card.effect + " (s = " + s + ")";
        }
        else
        {
            card_effect.text = cd.my_card.effect;
        }
        source.PlayOneShot(hover_sound, 1);
    }
    public void Hide_Detail()
    {
        Destroy(detail);
    }

    public IEnumerator Drop_Anim()
    {
        Vector3 pos_1 = this.transform.position;
        Vector3 pos_2 = parentToReturnTo.transform.position;
        float current_time = 0f;
        while (current_time <= anim_duration)
        {
            current_time += Time.deltaTime;
            float percent = Mathf.Clamp01(current_time / anim_duration);
            this.transform.position = Vector3.Lerp(pos_1, pos_2, percent);
            yield return null;
        }
        this.transform.SetParent(parentToReturnTo);
        this.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
        if (this.transform.parent.name == "Player_Board" && can_drop)
        {
            this.transform.localPosition = new Vector3(0, 0, 0);
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        Destroy(placeholder);
    }

    public IEnumerator Change_Dropped()
    {
        yield return new WaitForSeconds(anim_duration);
        dropped = !dropped;
    }

}
