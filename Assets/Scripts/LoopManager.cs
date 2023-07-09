using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    public SpellData[] lastLoopSpells;

    public static LoopManager Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    
}
