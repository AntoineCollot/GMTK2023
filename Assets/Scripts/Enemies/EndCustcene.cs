using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCustcene : MonoBehaviour
{
    public float afterBossDeathDelay = 2;
    public GameObject heroMessage;
    public float messageDelay = 3;
    public Transform playerCustceneSprite;
    public float animationDelay = 5;
    public GameObject endMessage;

    Vector2 bossDeathPos;
    public Health boss;

    private void Start()
    {
        bossDeathPos = boss.transform.position;
        TriggerEnd();
    }

    public void TriggerEnd()
    {
        PlayerMovement.Instance.transform.GetComponent<Collider2D>().enabled = false;
        ArenaManager arena = FindObjectOfType<ArenaManager>();
        foreach (var ene in arena.spawnedEnemies)
        {
            Destroy(ene.gameObject);
        }
        arena.spawnedEnemies.Clear();
        StartCoroutine(EndCutscene());
    }

    IEnumerator EndCutscene()
    {
        yield return new WaitForSeconds(afterBossDeathDelay);
        heroMessage.SetActive(true);
        yield return new WaitForSeconds(messageDelay);
        heroMessage.SetActive(false);
        yield return new WaitForSeconds(1);

        PlayerMovement.Instance.gameObject.SetActive(false);
        playerCustceneSprite.transform.position = bossDeathPos;
        playerCustceneSprite.gameObject.SetActive(true);

        yield return new WaitForSeconds(animationDelay);

        endMessage.SetActive(true);
    }
}
