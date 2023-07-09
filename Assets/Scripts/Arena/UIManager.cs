using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private SpellsRef spellsRef;
    public GameObject firstPhrase;

    public List<GameObject> tooltips;
    public List<TextMeshProUGUI> descriptions;
    public List<Image> icons;

    public List<Image> actualSpells;
    public List<Image> cooldowns;
    private List<ScriptableSpell> availableSpellData = new List<ScriptableSpell>();
    private List<ScriptableSpell> selectedSpells = new List<ScriptableSpell>();

    [Header("Health")]
    public int maxHealth;
    public List<GameObject> FullHearts;
    public List<GameObject> blockHearts;
    public List<Sprite> heartStates;

    [Header("Defeat")]
    public GameObject defeatPanel;

    [Header("Victory")]
    public GameObject victoryPanel;


    private void Start()
    {
        spellsRef = GetComponent<SpellsRef>();

        for (int i = 0; i < spellsRef.spells.Count; i++)
        {
            availableSpellData.Add(spellsRef.spells[i]);
        }

        InitiateHealth();

        //Events
        PlayerSpells.Instance.onSpellCastFinished.AddListener(StartCooldown);
        PlayerSpells.Instance.GetComponent<Health>().onHit.AddListener(CheckHealth);
        PlayerSpells.Instance.GetComponent<Health>().onHeal.AddListener(Healed);

        ArenaManager arena = GetComponent<ArenaManager>();
        if (!arena.isBoss)
        {
            firstPhrase.SetActive(true);
        }
    }

    public void SetTooltips()
    {
        for (int i = 0; i < tooltips.Count; i++)
        {
            tooltips[i].SetActive(true);

            if (!PlayerSpells.Instance.HasSpell(i))
            {
                int random = Random.Range(0, availableSpellData.Count);
                icons[i].sprite = availableSpellData[random].icon;
                descriptions[i].text = availableSpellData[random].description;
                tooltips[i].GetComponent<UpgradeButton>().index = i;
                tooltips[i].GetComponent<UpgradeButton>().isSpell = true;
                tooltips[i].GetComponent<UpgradeButton>().spellData = availableSpellData[random];
                tooltips[i].GetComponent<UpgradeButton>().indexAvailable = random;
                availableSpellData.RemoveAt(random);
            } else
            {
                int random = Random.Range(0, spellsRef.upgrades.Count);
                icons[i].sprite = spellsRef.upgrades[random].icon;
                descriptions[i].text = spellsRef.upgrades[random].description;
                tooltips[i].GetComponent<UpgradeButton>().index = i;
                tooltips[i].GetComponent<UpgradeButton>().isSpell = false;
                tooltips[i].GetComponent<UpgradeButton>().upgradeData = spellsRef.upgrades[random].data;
            }
        }

        firstPhrase.SetActive(false);
    }

    public void Selected(UpgradeButton button)
    {
        if (button.isSpell)
        {
            PlayerSpells.Instance.AddSpell(button.spellData.data, button.index);
            actualSpells[button.index].sprite = icons[button.index].sprite;
            cooldowns[button.index].sprite = icons[button.index].sprite;
            selectedSpells.Add(button.spellData);

            availableSpellData.Clear();
            for (int i = 0; i < spellsRef.spells.Count; i++)
            {
                if (!selectedSpells.Contains(spellsRef.spells[i]))
                {
                    availableSpellData.Add(spellsRef.spells[i]);
                }
            }
        } else
        {
            PlayerSpells.Instance.ApplyUpgrade(button.index, button.upgradeData);
        }

        for (int i = 0; i < tooltips.Count; i++)
        {
            tooltips[i].SetActive(false);
        }

        StartCoroutine(gameObject.GetComponent<ArenaManager>().NextWave());
    }

    void InitiateHealth()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            FullHearts[i].SetActive(true);
        }
        if (maxHealth < FullHearts.Count)
        {
            int blocked = FullHearts.Count - maxHealth;
            for (int i = 0; i < blocked; i++)
            {
                blockHearts[i].SetActive(true);
            }
        }
    }

    public void CheckHealth()
    {
        int life = Mathf.FloorToInt(PlayerSpells.Instance.GetComponent<Health>().health);
        for (int i = maxHealth; i > life && i > 0; i--)
        {
            if (FullHearts[i-1].GetComponent<Image>().sprite == heartStates[0])
            {
                FullHearts[i-1].GetComponent<Animator>().SetTrigger("Hit");
            }
        }

        if (life <= 0)
        {
            Defeat();
        }
    }

    public void Healed()
    {
        int life = Mathf.FloorToInt(PlayerSpells.Instance.GetComponent<Health>().health);
        for (int i = 0; i < life; i++)
        {
            if (FullHearts[i].GetComponent<Image>().sprite == heartStates[1])
            {
                FullHearts[i].GetComponent<Animator>().SetTrigger("Heal");
            }
        }
    }

    public void StartCooldown()
    {
        for (int i = 0; i < actualSpells.Count; i++)
        {
            if (PlayerSpells.Instance.SpellCurrentCooldown01(i) < 1)
            {
                StartCoroutine(Cooldown(i));
            }
        }
    }

    IEnumerator Cooldown(int spellIndex)
    {
        float cd = 1;
        while (cd>0)
        {
            cd = PlayerSpells.Instance.SpellCurrentCooldown01(spellIndex);
            cooldowns[spellIndex].fillAmount = cd;
            yield return null;
        }
    }

    public void Defeat()
    {
        defeatPanel.SetActive(true);
    }

    public void Victory()
    {
        victoryPanel.SetActive(true);
        GetComponent<ArenaManager>().StopTime();
    }
}
