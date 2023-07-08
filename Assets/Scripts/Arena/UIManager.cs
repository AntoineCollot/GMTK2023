using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private SpellsRef spellsRef;
    private PlayerSpells player;

    public List<GameObject> tooltips;
    public List<TextMeshProUGUI> descriptions;
    public List<Image> icons;

    public List<Image> actualSpells;
    public List<ScriptableSpell> availableSpellData = new List<ScriptableSpell>();
    private List<ScriptableSpell> selectedSpells = new List<ScriptableSpell>();


    private void Start()
    {
        spellsRef = GetComponent<SpellsRef>();
        player = GetComponent<ArenaManager>().player.GetComponent<PlayerSpells>();

        for (int i = 0; i < spellsRef.spells.Count; i++)
        {
            availableSpellData.Add(spellsRef.spells[i]);
        }
    }

    public void SetTooltips()
    {
        for (int i = 0; i < tooltips.Count; i++)
        {
            tooltips[i].SetActive(true);

            if (!player.HasSpell(i))
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
    }

    public void Selected(UpgradeButton button)
    {
        if (button.isSpell)
        {
            player.AddSpell(button.spellData.data, button.index);
            actualSpells[button.index].sprite = icons[button.index].sprite;
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
            player.ApplyUpgrade(button.index, button.upgradeData);
        }

        for (int i = 0; i < tooltips.Count; i++)
        {
            tooltips[i].SetActive(false);
        }

        StartCoroutine(gameObject.GetComponent<ArenaManager>().NextWave());
    }
}
