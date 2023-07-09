using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    public void OnGameEnd()
    {
        LoopManager.Instance.OnGameEnd();
    }
}
