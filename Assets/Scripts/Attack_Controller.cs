using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Attack_Controller : MonoBehaviour
{
    public bool auto_snap = true;
    public bool clicked = false;
    public bool anim_finish = true;
    public AnimationCurve atk_anim_curve;
    public float atk_duration = 0.25f;
    public GameObject floating_prefab;
    public AnimationCurve floating_anim_curve;
    public float float_duration = 1.5f;
    public GameObject fireball_prefab;
    public AnimationCurve fireball_anim_curve;
    public float fireball_duration = 0.25f;
    public GameObject explosion_prefab;
    public float explosion_duration = 0.5f;
    public AudioClip atk_sound;
    public AudioClip fireball_sound;
    public AudioClip explosion_sound;
    public AudioClip consume_sound;

    private Card_Detail cd;
    private RaycastHit2D hit;
    private GameObject target_1;
    private GameObject target_2;
    private Match_Maker m;
    private LineRenderer line;
    private Transform enemy_board;
    private Transform player_board;
    private GameObject enemy_hero;
    private GameObject player_hero;
    private Canvas world_canvas;
    private AudioSource source;
    private bool sound_playing = false;

    void Start()
    {
        m = GameObject.Find("Match_Maker").GetComponent<Match_Maker>();
        line = this.GetComponent<LineRenderer>();
        enemy_board = GameObject.Find("Enemy_Board").transform;
        player_board = GameObject.Find("Player_Board").transform;
        enemy_hero = GameObject.Find("Enemy_Hero");
        player_hero = GameObject.Find("Player_Hero");
        world_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        source = GameObject.Find("Music_Player").GetComponent<AudioSource>();
    }
    void Update()
    {
        Select_Atk_Target();
    }

    public void Select_Atk_Target()
    {
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 8;
        hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(screenPoint), Vector2.zero);

        if (!clicked)
        {
            target_1 = null;
            target_2 = null;
            line.SetPosition(0, new Vector3(0, 0, 0));
            line.SetPosition(1, new Vector3(0, 0, 0));
            line.enabled = false;
        }
        else
        {
            line.enabled = true;
            if (hit.collider != null && auto_snap)
            {
                if (hit.transform.tag == "Card" && hit.transform.parent.name == "Enemy_Board")
                {
                    Vector3 pos_2 = new Vector3(hit.transform.position.x, hit.transform.position.y, -1);
                    line.SetPosition(1, pos_2);
                }
                else
                {
                    line.SetPosition(1, Camera.main.ScreenToWorldPoint(screenPoint));
                }
            }
            else
            {
                line.SetPosition(1, Camera.main.ScreenToWorldPoint(screenPoint));
            }
        }

        if (Input.GetMouseButtonDown(0) && anim_finish && hit.collider != null)
        {
            if (!clicked && hit.transform.tag == "Card")
            {
                cd = hit.transform.gameObject.GetComponent<Card_Detail>();
                if (cd.attackable && cd.transform.parent.name == "Player_Board")
                {
                    target_1 = cd.gameObject;
                    clicked = true;
                    Vector3 pos_1 = new Vector3(target_1.transform.position.x, target_1.transform.position.y, -1);
                    line.SetPosition(0, pos_1);
                }
            }
            else if (clicked)
            {
                if (hit.transform.tag == "Card")
                {
                    if (hit.transform.parent.name == "Enemy_Board")
                    {
                        cd = hit.transform.gameObject.GetComponent<Card_Detail>();
                        target_2 = cd.gameObject;
                        StartCoroutine(Attack(target_1, target_2));
                        clicked = false;
                    }
                    else
                    {
                        Debug.Log("Invalid Target");
                    }
                }
                else
                {
                    if (hit.transform.name == "Enemy_Hero")
                    {
                        target_2 = hit.transform.gameObject;
                        StartCoroutine(Attack(target_1, target_2));
                        clicked = false;
                    }
                    else
                    {
                        Debug.Log("Invalid Target");
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && clicked)
        {
            clicked = false;
        }
    }

    public GameObject Random_Target(string type)
    {
        int number;
        if (type == "Enemy")
        {
            if (enemy_board.childCount == 0)
            {
                return enemy_hero;
            }
            else
            {
                number = Random.Range(0, enemy_board.transform.childCount + 1);
                if (number == 0)
                {
                    return enemy_hero;
                }
                else
                {
                    return enemy_board.GetChild(number - 1).gameObject;
                }
            }
        }
        else if (type == "Player")
        {
            if (player_board.childCount == 0)
            {
                return player_hero;
            }
            else
            {
                number = Random.Range(0, player_board.transform.childCount + 1);
                if (number == 0)
                {
                    return player_hero;
                }
                else
                {
                    return player_board.GetChild(number - 1).gameObject;
                }
            }
        }
        else
        {
            number = Random.Range(0, 2);
            if (number == 0)
            {
                return Random_Target("Enemy");
            }
            else
            {
                return Random_Target("Player");
            }
        }
    }

    public IEnumerator Destroy_Card(GameObject target)
    {
        Card_Detail card_cd = target.GetComponent<Card_Detail>();
        yield return StartCoroutine(Destroy_Anim(target));
        StartCoroutine(Floating_Anim(target, -card_cd.hp));
        card_cd.hp = 0;
    }

    public IEnumerator Destroy_Anim(GameObject target)
    {
        Color purple_color = new Color32(143, 0, 255, 255);
        Vector3 pos_1 = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z - 1);
        GameObject fireball = Instantiate(fireball_prefab, new Vector3(pos_1.x, pos_1.y, pos_1.z), Quaternion.identity);
        fireball.GetComponent<MeshRenderer>().material.color = purple_color;
        source.PlayOneShot(consume_sound, 1);
        yield return new WaitForSeconds(fireball_duration);
        Destroy(fireball);
        GameObject explosion = Instantiate(explosion_prefab, new Vector3(pos_1.x, pos_1.y, pos_1.z), Quaternion.identity);
        GradientColorKey[] grad_color = { new GradientColorKey(purple_color, 0.0f), new GradientColorKey(purple_color, 1.0f) };
        GradientAlphaKey[] grad_alpha = { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) };
        Gradient grad = new Gradient();
        grad.SetKeys(grad_color, grad_alpha);
        ParticleSystem.ColorOverLifetimeModule ps_col = explosion.GetComponent<ParticleSystem>().colorOverLifetime;
        ps_col.color = grad;
        yield return new WaitForSeconds(0.5f);
        source.PlayOneShot(explosion_sound, 1);
        Destroy(explosion);
    }

    public IEnumerator Floating_Anim(GameObject obj, int value)
    {
        yield return new WaitForSeconds(0.25f);
        GameObject floating_text = Instantiate(floating_prefab, obj.transform.position, Quaternion.identity);
        floating_text.transform.SetParent(world_canvas.transform);
        floating_text.transform.localScale = new Vector3(1, 1, 1);
        Text txt = floating_text.GetComponent<Text>();
        Outline outline = floating_text.GetComponent<Outline>();
        if (value > 0)
        {
            txt.text = "+" + value.ToString();
            if (obj.CompareTag("Coin"))
            {
                txt.color = Color.yellow;
            }
            else
            {
                txt.color = Color.green;
            }
        }
        else
        {
            txt.text = value.ToString();
            txt.color = Color.red;
        }
        Vector3 pos = floating_text.transform.position;
        Vector3 pos_1, pos_2;
        if (obj.CompareTag("Coin"))
        {
            pos_1 = new Vector3(pos.x + 0.55f, pos.y, pos.z);
            pos_2 = new Vector3(pos_1.x, pos_1.y + 1, pos_1.z);
        }
        else
        {
            pos_1 = new Vector3(pos.x + 0.55f, pos.y - 0.7f, pos.z);
            pos_2 = new Vector3(pos_1.x, pos_1.y + 1, pos_1.z);
        }
        float current_time = 0f;
        while (current_time <= float_duration)
        {
            current_time += Time.deltaTime;
            float percent = Mathf.Clamp01(current_time / float_duration);
            float curve_percent = floating_anim_curve.Evaluate(percent);
            floating_text.transform.position = Vector3.Lerp(pos_1, pos_2, curve_percent);
            float alpha = Mathf.Lerp(1f, 0f, current_time / float_duration);
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, alpha);
            outline.effectColor = new Color(0, 0, 0, alpha);
            yield return null;
        }
        yield return new WaitForSeconds(1);
        Destroy(floating_text);
    }

    public IEnumerator Attack(GameObject obj_1, GameObject obj_2)
    {
        anim_finish = false;
        Card_Detail cd_1 = obj_1.GetComponent<Card_Detail>();
        yield return StartCoroutine(Attack_Anim(obj_1, obj_2));
        StartCoroutine(Floating_Anim(obj_2, -cd_1.atk));
        if (obj_2.transform.CompareTag("Card"))
        {
            Card_Detail cd_2 = obj_2.GetComponent<Card_Detail>();
            StartCoroutine(Floating_Anim(obj_1, -cd_2.atk));
            cd_2.hp -= cd_1.atk;
            cd_1.hp -= cd_2.atk;
        }
        else
        {
            if (obj_2.name == "Enemy_Hero")
            {
                m.enemy_hp -= cd_1.atk;
            }
            else
            {
                m.player_hp -= cd_1.atk;
            }
        }
        cd_1.attackable = false;
        cd_1.Calculate_Turn();
    }

    public IEnumerator Attack_Anim(GameObject obj_1, GameObject obj_2)
    {
        Vector3 pos_1 = obj_1.transform.position;
        Vector3 pos_2 = obj_2.transform.position;
        float current_time = 0f;
        while (current_time <= atk_duration)
        {
            current_time += Time.deltaTime;
            float percent = Mathf.Clamp01(current_time / atk_duration);
            float curve_percent = atk_anim_curve.Evaluate(percent);
            obj_1.transform.position = Vector3.Lerp(pos_1, pos_2, curve_percent);
            yield return null;
        }
        source.PlayOneShot(atk_sound, 0.6f);
        current_time = 0f;
        while (current_time <= atk_duration)
        {
            current_time += Time.deltaTime;
            float percent = Mathf.Clamp01(current_time / atk_duration);
            float curve_percent = atk_anim_curve.Evaluate(percent);
            obj_1.transform.position = Vector3.Lerp(pos_2, pos_1, curve_percent);
            yield return null;
        }
        anim_finish = true;
    }
    public IEnumerator Deal_Damage(GameObject obj_1, GameObject obj_2, int dmg)
    {
        anim_finish = false;
        yield return StartCoroutine(Deal_Damage_Anim(obj_1, obj_2));
        StartCoroutine(Floating_Anim(obj_2, -dmg));
        if (obj_2.transform.CompareTag("Card"))
        {
            Card_Detail target_cd = obj_2.GetComponent<Card_Detail>();
            target_cd.hp -= dmg;
        }
        else
        {
            if (obj_2.name == "Enemy_Hero")
            {
                m.enemy_hp -= dmg;
            }
            else
            {
                m.player_hp -= dmg;
            }
        }
        anim_finish = true;
    }

    IEnumerator Deal_Damage_Anim(GameObject obj_1, GameObject obj_2)
    {
        Vector3 pos_1 = new Vector3(obj_1.transform.position.x, obj_1.transform.position.y, obj_1.transform.position.z - 1);
        Vector3 pos_2 = new Vector3(obj_2.transform.position.x, obj_2.transform.position.y, obj_2.transform.position.z - 1);
        GameObject fireball = Instantiate(fireball_prefab, new Vector3(0, 0, 0), Quaternion.identity);
        float current_time = 0f;
        if (!sound_playing)
        {
            sound_playing = true;
            source.PlayOneShot(fireball_sound, 1);
        }
        while (current_time <= fireball_duration)
        {
            current_time += Time.deltaTime;
            float percent = Mathf.Clamp01(current_time / fireball_duration);
            float curve_percent = fireball_anim_curve.Evaluate(percent);
            fireball.transform.position = Vector3.Lerp(pos_1, pos_2, curve_percent);
            yield return null;
        }
        yield return new WaitForSeconds(fireball_duration);
        Destroy(fireball);
        GameObject explosion = Instantiate(explosion_prefab, new Vector3(pos_2.x, pos_2.y, pos_2.z), Quaternion.identity);
        yield return new WaitForSeconds(0.35f);
        source.PlayOneShot(explosion_sound, 1);
        yield return new WaitForSeconds(0.15f);
        sound_playing = false;
        Destroy(explosion);
    }

}