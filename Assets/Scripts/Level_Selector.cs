using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level_Selector : MonoBehaviour
{
    public Image[] level_img;
    public Sprite[] unlock_sprite;
    public Sprite[] locked_sprite;

    private Game_Manager gm;

    void Start()
    {
        gm = GameObject.Find("Game_Manager").GetComponent<Game_Manager>();
        for (int i = 0; i < level_img.Length; i++)
        {
            if (i <= gm.unlocked_level)
            {
                level_img[i].sprite = unlock_sprite[i];
            }
            else
            {
                level_img[i].sprite = locked_sprite[i];
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            gm.unlocked_level = 4;
            Start();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        gm.current_level = other.transform.GetSiblingIndex();
    }
}
