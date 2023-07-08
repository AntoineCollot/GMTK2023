using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewScriptableUpgrade", menuName = "Custom/ScriptableUpgrade", order = 1)]
public class ScriptableUpgrade : ScriptableObject
{
    public Sprite icon;
    [TextArea()]
    public string description;
    public SpellUpgradeData data;
}
