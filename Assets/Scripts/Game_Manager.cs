using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Manager : MonoBehaviour
{
    public int unlocked_level = 0;
    public int current_level = 0;
    public Texture2D fadeOutTexture;
    public float fadeSpeed = 0.6f;

    private static GameObject instance;
    private int drawDepth = -1000;
    private float alpha = 1.0f;
    private int fadeDir = -1;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Screen.currentResolution.refreshRate;

        DontDestroyOnLoad(this.gameObject);
        if (instance == null)
        {
            instance = this.gameObject;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if (SceneManager.GetActiveScene().name == "Gameplay")
            {
                Load_Scene("Main_Menu");
            }
        }
    }

    void OnGUI()
    {
        alpha += fadeDir * fadeSpeed * Time.deltaTime;
        alpha = Mathf.Clamp01(alpha);

        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        GUI.depth = drawDepth;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);
    }

    public float BeginFade(int direction)
    {
        fadeDir = direction;
        return (fadeSpeed);
    }

    public void Load_Scene(string name)
    {
        StartCoroutine(Fade_Scene(name));
    }

    public void Exit_Game()
    {
        StartCoroutine(Fade_Exit());
    }

    public IEnumerator Fade_Scene(string name)
    {
        BeginFade(1);
        yield return new WaitForSeconds(fadeSpeed + 0.2f);
        SceneManager.LoadScene(name);
        yield return new WaitForSeconds(fadeSpeed);
        BeginFade(-1);
    }

    public IEnumerator Fade_Exit()
    {
        BeginFade(1);
        yield return new WaitForSeconds(fadeSpeed + 0.2f);
        Application.Quit();
    }
}
