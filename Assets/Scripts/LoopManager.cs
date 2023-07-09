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
    public static List<SpellData> lastLoopSpells;
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
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == "Level")
        {
            maxHealth--;
            loops++;

            GetComponentInChildren<TextMeshProUGUI>().text = "Loops : " + loops.ToString();
        }

        maxHealth = Mathf.Max(maxHealth, 3);

        Health playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        playerHealth.health = Mathf.Min(playerHealth.health, maxHealth);
    }

    private void Awake()
    {
        OnGameStart();
    }

    public void OnGameStart()
    {
        if (lastLoopSpells == null)
            lastLoopSpells = new List<SpellData>();

        int id = 0;
        while(lastLoopSpells.Count < 2)
        {
            lastLoopSpells.Add(defaultSpells[id].data);
            id++;
        }
    }

    public void OnGameEnd()
    {
        lastLoopSpells = currentSpells;
    }
}
