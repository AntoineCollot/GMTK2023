using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    public List<SpellData> previousSpell; // setUp quand le joueur vainc le boss

    [Header("Player")]
    public GameObject player;
    public GameObject fakePlayer; // fait apparaitre un faux joueur pour le placer dans la scene
    

    [Header("Arena start")]
    public Transform playerSpawn; // ou faire spawn le fake player
    public Transform entryPoint; // point jusqu'auquel le faux joueur se place

    [Header("Ennemies")]
    public int minEmmeniesNumber;
    public int maxEnnemiesNumber;
    public GameObject ennemyPrefab;
    public Transform ennemyParent;
    public List<Transform> spawnPoints;
    private List<Health> notDead = new List<Health>();

    [Header("Boss")]
    public bool isBoss;
    public GameObject bossPrefab;
    private GameObject bossEnnemy;
    public Transform bossSpawnPoint;

    [Header("Upgrades")]
    public List<SpellData> spells;
    

    private void Start()
    {
        for (int i = 0; i < maxEnnemiesNumber; i++) // créer le nombre d'ennemis max
        {
            GameObject ennemy = Instantiate(ennemyPrefab, ennemyParent);
            ennemy.transform.localPosition = Vector3.zero;
            ennemy.GetComponent<Health>().onDie.AddListener(CheckWaveStatus);
            ennemy.SetActive(false);
        }
        SetUpEnnemies(); // assigne les spells avant de mettre le boss dans la pool

        if (isBoss)
        {
            GameObject boss = Instantiate(bossPrefab, ennemyParent);
            notDead.Add(boss.GetComponent<Health>());
            bossEnnemy = boss;
        }

        StartCoroutine(StartArena());
    }

    void SetUpEnnemies()
    {
        for (int i = 0; i < maxEnnemiesNumber; i++)
        {
            int randomSpell = Random.Range(0, previousSpell.Count);
            IA ennemy = ennemyParent.GetChild(i).GetComponent<IA>();
            ennemy.spells.Clear(); // enlève ses spells
            ennemy.spells.Add(previousSpell[randomSpell]); // lui ajoute un spell aléatoire
        }
    }

    IEnumerator StartArena()
    {
        fakePlayer.SetActive(true);
        fakePlayer.transform.position = playerSpawn.position;
        GameObject trueOne = Instantiate(player, null);
        trueOne.SetActive(false);

        yield return new WaitForSeconds(1); // temps d'attente, le temps que le joueur comprenne son environement
        
        if (isBoss) // si salle du boss
        {
            bossEnnemy.transform.position = bossSpawnPoint.position;
            bossEnnemy.SetActive(true);
        }

        float placement = 3;
        while (placement > 0)
        {
            // déplace le faux joueur de l'entré au point défini 
            Vector3 newPos = Vector3.Lerp(fakePlayer.transform.position, entryPoint.position, 1 * Time.deltaTime);
            fakePlayer.transform.position = newPos;

            placement -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        // remplace le faux joueur par le vrai
        trueOne.transform.position = fakePlayer.transform.position;
        trueOne.SetActive(trueOne);
        fakePlayer.SetActive(false);

        SpawnWave();
    }

    void SpawnWave()
    {
        List<Transform> spawnPointAvailable = new List<Transform>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            spawnPointAvailable.Add(spawnPoints[i]);
        }
        int randomNumber = Random.Range(minEmmeniesNumber, maxEnnemiesNumber + 1);
        for (int i = 0; i < randomNumber; i++)
        {
            int randomPoint = Random.Range(0, spawnPointAvailable.Count);
            ennemyParent.GetChild(i).position = spawnPointAvailable[randomPoint].position;
            spawnPointAvailable.RemoveAt(randomPoint);

            ennemyParent.transform.GetChild(i).gameObject.SetActive(true);

            notDead.Add(ennemyParent.GetChild(i).GetComponent<Health>());
        }
    }

    public void CheckWaveStatus() // call every time a ennemy die
    {
        for (int i = 0; i < notDead.Count; i++)
        {
            if (notDead[i].isDead)
            {
                notDead.RemoveAt(i);
            }
        }

        if (notDead.Count == 0)
        {
            if (!isBoss) // si l'arene terminé n'est pas la salle du boss
            {
                EndWave();
            } else // si c'est la salle du boss
            {
                EndBoss();
            }
        }
    }

    void EndWave()
    {
        Debug.Log("EndWave");
        // ouvre la porte
        // offre 3 améliorations
    }

    void EndBoss()
    {
        // ?
    }
}