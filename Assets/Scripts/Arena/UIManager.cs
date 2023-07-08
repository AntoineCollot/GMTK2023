using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private SpellsRef spellsRef;
    public PlayerSpells player;

    public List<GameObject> tooltips;
    public List<TextMeshProUGUI> descriptions;
    public List<Image> icons;

    public List<Image> actualSpells;

    public void SetTooltips()
    {
        for (int i = 0; i < tooltips.Count; i++)
        {
            tooltips[i].SetActive(true);


            if (!player.HasSpell(i))
            {
                int random = Random.Range(0, spellsRef.spells.Count);
                icons[i].sprite = spellsRef.spells[random].icon;
                descriptions[i].text = spellsRef.spells[random].description;
                tooltips[i].GetComponent<UpgradeButton>().index = i;
                tooltips[i].GetComponent<UpgradeButton>().isSpell = true;
                tooltips[i].GetComponent<UpgradeButton>().spellData = spellsRef.spells[random].data;
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
            player.AddSpell(button.spellData, button.index);
            actualSpells[button.index].sprite = icons[button.index].sprite;
        } else
        {
            player.ApplyUpgrade(button.index, button.upgradeData);
        }

        for (int i = 0; i < tooltips.Count; i++)
        {
            tooltips[i].SetActive(false);
        }

        //StartCoroutine(gameObject.GetComponent<ArenaManager>().NextWave());
    }
}
