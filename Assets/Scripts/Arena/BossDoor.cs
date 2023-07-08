using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDoor : MonoBehaviour
{
    public string sceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("BossDoor");
        }
    }

    IEnumerator CutScene()
    {
        yield return null;

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
