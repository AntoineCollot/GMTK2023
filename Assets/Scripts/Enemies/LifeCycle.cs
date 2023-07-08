using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycle : MonoBehaviour
{
    public ArenaManager arena;
    public bool living;

    [Header("FX")]
    public GameObject spawnFX;
    public GameObject deadFX;

    private void OnEnable()
    {
        gameObject.GetComponent<IA>().enabled = false;
        StartCoroutine(Spawning());
    }

    private void OnDisable()
    {
        if (!living)
        {
            return;
        }

        deadFX.SetActive(false);
        arena.CheckWaveStatus(gameObject);
        arena = null;
        living = false;
    }

    IEnumerator Spawning()
    {
        spawnFX.SetActive(true);
        yield return new WaitForSeconds(0.5f); // délai entre l'apparition et l'activation de l'ennemi 
                                               // à tester, sinon juste un void avec 0 délai
        spawnFX.SetActive(false);
        gameObject.GetComponent<IA>().enabled = true;
    }

    public IEnumerator Dead()
    {
        deadFX.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
