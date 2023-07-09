using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition : MonoBehaviour
{
    public ArenaManager arenaManager;
    public bool firstTime;
    public bool defeat;

    public List<SpellData> bossSpells;
    public string bossName;
    public List<SpellData> playerSpells;
    public float playerhealth;
    public int playerMaxHealth;
    public string playerName;
    public List<Sprite> spellsIcons;

    private void Awake()
    {
        if (firstTime)
        {
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    public void AssignParameters(bool isBoss)
    {
        if (defeat)
        {
            return;
        }
        if (firstTime)
        {
            ForFirst();
            return;
        }
        Debug.Log("Assign");
        for (int i = 0; i < 3; i++)
        {
            arenaManager.previousSpell[i] = bossSpells[i];
        }
        PlayerSpells player = arenaManager.player.GetComponent<PlayerSpells>();
        for (int i = 0; i < playerSpells.Count; i++)
        {
            player.AddSpell(playerSpells[i], i);
            arenaManager.GetComponent<UIManager>().actualSpells[i].sprite = spellsIcons[i];
            arenaManager.GetComponent<UIManager>().cooldowns[i].sprite = spellsIcons[i];
        }
        arenaManager.GetComponent<UIManager>().maxHealth = playerMaxHealth;
        player.GetComponent<Health>().health = playerhealth;
        if (isBoss)
        {
            // assign bossName
        }
    }

    public void SaveParameters(bool isBoss)
    {
        if (!isBoss)
        {
            PlayerSpells player = arenaManager.player.GetComponent<PlayerSpells>();
            for (int i = 0; i < player.spells.Length; i++)
            {
                playerSpells[i] = player.spells[i];
                spellsIcons[i] = arenaManager.GetComponent<UIManager>().actualSpells[i].sprite;
            }
            playerhealth = player.GetComponent<Health>().health;
            playerMaxHealth = arenaManager.GetComponent<UIManager>().maxHealth;
            // playerName
        }
        else
        {
            bossName = playerName;
        }

        for (int i = 0; i < arenaManager.previousSpell.Count; i++)
        {
            bossSpells[i] = arenaManager.previousSpell[i];
        }
    }

    void ForFirst()
    {
        SpellsRef spellRef = arenaManager.GetComponent<SpellsRef>();
        List<ScriptableSpell> allSpells = new List<ScriptableSpell>();
        for (int i = 0; i < spellRef.spells.Count; i++)
        {
            allSpells.Add(spellRef.spells[i]);
        }
        for (int i = 0; i < 3; i++)
        {
            int random = Random.Range(0, allSpells.Count);
            arenaManager.previousSpell[i] = allSpells[random].data;
            allSpells.RemoveAt(random);
        }

        firstTime = false;
    }
}
