using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewScriptableSpell", menuName = "Custom/ScriptableSpell", order = 1)]
public class ScriptableSpell : ScriptableObject
{
    public Sprite icon;
    [TextArea()]
    public string description;
    public SpellData data;
}
