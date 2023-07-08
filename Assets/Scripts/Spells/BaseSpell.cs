using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBaseSpell", menuName = "Custom/BaseSpell", order = 1)]
public class BaseSpell : ScriptableObject
{
    public SpellData data;
}
