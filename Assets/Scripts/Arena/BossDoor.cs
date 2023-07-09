using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    public Collider2D col;
    public SpriteRenderer spriteRenderer;
    public ArenaManager arena;
    bool isOpen = false;

    private void Start()
    {
        Close();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOpen)
            return;

        if (collision.tag == "Player")
        {
            StartCoroutine(arena.DoorAlignement());
        }
    }

    public void Open()
    {
        spriteRenderer.enabled = false;
        col.enabled = true;
        isOpen = true;
    }

    public void Close()
    {
        spriteRenderer.enabled = true;
        col.enabled = false;
        isOpen = false;
    }
}
