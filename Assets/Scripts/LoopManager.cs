using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopManager : MonoBehaviour
{
    public ScriptableSpell[] defaultSpells;

    public static List<SpellData> currentSpells;
    public static List<SpellData> LastLoopSpells {
        get
        {
            if (lastLoopSpells == null)
                lastLoopSpells = new List<SpellData>();

            int id = 0;
            while (lastLoopSpells.Count < 2)
            {
                lastLoopSpells.Add(Instance.defaultSpells[id].data);
                id++;
            }

            return lastLoopSpells;
        }
    }
    static List<SpellData> lastLoopSpells;

    public int maxHealth;
    public int loops;

    public static LoopManager Instance;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        GetComponentInChildren<TextMeshProUGUI>().text = "Loops : " + loops.ToString();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (scene.name == "Level")
        {
            currentSpells.Clear();
            maxHealth--;
            loops++;

            GetComponentInChildren<TextMeshProUGUI>().text = "Loops : " + loops.ToString();
        }
        if(scene.name == "MainMenu")
        {
            lastLoopSpells.Clear();
            currentSpells.Clear();
        }

        maxHealth = Mathf.Max(maxHealth, 3);

        Health playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        playerHealth.health = Mathf.Min(playerHealth.health, maxHealth);
    }

    private void Awake()
    {
        //OnFirstStart();
    }

    //public void OnFirstStart()
    //{
    //    if (LastLoopSpells == null)
    //        LastLoopSpells = new List<SpellData>();

    //    int id = 0;
    //    while(LastLoopSpells.Count < 2)
    //    {
    //        LastLoopSpells.Add(defaultSpells[id].data);
    //        id++;
    //    }
    //}

    public void OnGameEnd()
    {
        lastLoopSpells = currentSpells;
    }
}
