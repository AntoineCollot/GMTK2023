using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    public ArenaManager arena;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("BossDoor | " + collision.name);
            StartCoroutine(arena.DoorAlignement());
        }
    }
}
