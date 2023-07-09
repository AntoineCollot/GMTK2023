using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaManager : MonoBehaviour
{
    private Transition transitionObject;
    public List<SpellData> previousSpell; // setUp quand le joueur vainc le boss

    [Header("Player")]
    public GameObject player;
    public float transitionSpeed;
    

    [Header("Arena start")]
    public Transform playerSpawn; // ou faire spawn le fake player
    public Transform entryPoint; // point jusqu'auquel le faux joueur se place

    [Header("Ennemies")]
    public int minEmmeniesNumber;
    public int maxEnnemiesNumber;
    public GameObject ennemyPrefab;
    public Transform ennemyParent;
    public List<Transform> spawnPoints;
    public List<Health> notDead = new List<Health>();
    

    [Header("Boss")]
    public bool isBoss;
    public GameObject bossEnnemy;
    public Transform bossSpawnPoint;

    [Header("BossTransition")]
    public GameObject bossDoor;
    public string sceneName;
    public GameObject darkScreen;
    public GameObject bossDeathFX;

    [Header("Upgrades")]
    public List<SpellData> spells;
    public List<SpellData> upgrades;

    [Header("FXs")]
    public Transform fxFolder;
    public GameObject spawnFxPrefab;
    public GameObject deathFxPrefab;
    private List<GameObject> spawnFxs = new List<GameObject>();
    private List<GameObject> deathFxs = new List<GameObject>();

    private CompositeStateToken lockToken;

    private void Awake()
    {
        GameObject _transition = GameObject.Find("TransitionObject");
        if (_transition != null)
        {
            GameObject.Find("TransitionObject").TryGetComponent<Transition>(out transitionObject);
            if (transitionObject != null)
            {
                transitionObject.arenaManager = this;
                transitionObject.defeat = false;
                transitionObject.AssignParameters(isBoss);
            }
        }
    }

    private void Start()
    {
        if (transitionObject == null)
        {
            GameObject.Find("TransitionObject").TryGetComponent<Transition>(out transitionObject);
            if (transitionObject != null)
            {
                transitionObject.arenaManager = this;
                transitionObject.defeat = false;
                transitionObject.AssignParameters(isBoss);
            }
        }

        lockToken = new CompositeStateToken();
        PlayerMovement.Instance.lockMovementState.Add(lockToken);

        for (int i = 0; i < maxEnnemiesNumber; i++) // créer le nombre d'ennemis max
        {
            GameObject ennemy = Instantiate(ennemyPrefab, ennemyParent);
            ennemy.transform.localPosition = Vector3.zero;
            ennemy.GetComponent<Health>().onDie.AddListener(CheckWaveStatus);
            ennemy.SetActive(false);

            GameObject spawnFx = Instantiate(spawnFxPrefab, fxFolder);
            GameObject deathFx = Instantiate(deathFxPrefab, fxFolder);
            spawnFxs.Add(spawnFx);
            deathFxs.Add(deathFx);
            spawnFx.SetActive(false);
            deathFx.SetActive(false);
        }
        SetUpEnnemies(); // assigne les spells avant de mettre le boss dans la pool

        if (isBoss)
        {
            notDead.Add(bossEnnemy.GetComponent<Health>());
            bossEnnemy.GetComponent<Health>().onDie.AddListener(CheckWaveStatus);
            for (int i = 0; i < previousSpell.Count; i++)
            {
                bossEnnemy.GetComponent<IA>().spells[i] = previousSpell[i];
            }
            bossEnnemy.GetComponent<IA>().enabled = false;
        } else
        {
            bossDoor.GetComponent<BossDoor>().arena = this;
        }

        StartCoroutine(StartArena());
    }

    private void OnDestroy()
    {
        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.lockMovementState.Remove(lockToken);
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
        player.transform.position = playerSpawn.position;
        lockToken.SetOn(true);
        PlayerMovement.Instance.SetAnimationDirection(Direction.Up);
        player.GetComponent<CircleCollider2D>().isTrigger = true;

        yield return new WaitForSeconds(1); // temps d'attente, le temps que le joueur comprenne son environement
        
        if (isBoss) // si salle du boss
        {
            bossEnnemy.transform.position = bossSpawnPoint.position;
            bossEnnemy.SetActive(true);
        }

        float placement = 2;
        while (placement > 0)
        {
            // déplace le faux joueur de l'entré au point défini 
            Vector3 newPos = Vector3.Lerp(player.transform.position, entryPoint.position, transitionSpeed * Time.deltaTime);
            player.transform.position = newPos;

            placement -= Time.deltaTime;
            yield return null;
        }

        player.GetComponent<CircleCollider2D>().isTrigger = false;
        lockToken.SetOn(false);

        if (!isBoss)
        {
            GetComponent<UIManager>().SetTooltips();
        } else
        {
            bossEnnemy.GetComponent<IA>().enabled = true;
            SpawnWave();
        }
    }

    public IEnumerator NextWave()
    {
        bossDoor.SetActive(false);
        yield return new WaitForSeconds(3);
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

            spawnFxs[i].transform.position = spawnPointAvailable[randomPoint].position;
            spawnFxs[i].SetActive(true);
            StartCoroutine(ActivateEnnemy(i));

            spawnPointAvailable.RemoveAt(randomPoint);
        }
    }

    IEnumerator ActivateEnnemy(int index)
    {
        yield return new WaitForSeconds(0.3f);
        ennemyParent.transform.GetChild(index).gameObject.SetActive(true);
        notDead.Add(ennemyParent.GetChild(index).GetComponent<Health>());
    }

    public void CheckWaveStatus() // call every time a ennemy die
    {
        for (int i = 0; i < notDead.Count; i++)
        {
            if (!notDead[i].gameObject.activeSelf)
            {
                for (int j = 0; j < deathFxs.Count; j++)
                {
                    if (!deathFxs[j].activeSelf)
                    {
                        deathFxs[j].transform.position = notDead[i].transform.position;
                        deathFxs[j].SetActive(true);
                        break;
                    }
                }
                notDead.RemoveAt(i);
            }
        }

        if (notDead.Count == 0)
        {
            if (!isBoss) // si l'arene terminé n'est pas la salle du boss
            {
                StartCoroutine(EndWave());
            } else // si c'est la salle du boss
            {
                EndBoss();
            }
        }
    }

    IEnumerator EndWave()
    {
        // FX        
        yield return new WaitForSeconds(0.5f);
        bossDoor.SetActive(true); // ouvre la porte

        GetComponent<UIManager>().SetTooltips(); // offre 3 améliorations
    }

    void EndBoss()
    {
        // ?
        bossDeathFX.transform.position = bossEnnemy.transform.position;
        bossDeathFX.SetActive(true);
        Debug.Log("Victory");
        // start animation
        GetComponent<UIManager>().Victory();
    }

    public IEnumerator DoorAlignement()
    {
        transitionObject.SaveParameters(isBoss);

        lockToken.SetOn(true);
        player.GetComponent<CircleCollider2D>().isTrigger = true;

        bool up = false;
        float time = 5;
        while (time > 0)
        {
            if (!up)
            {
                Vector3 firstPoint = new Vector3(bossDoor.transform.position.x, player.transform.position.y, player.transform.position.z);
                float dist = Vector3.Distance(player.transform.position, firstPoint);

                Vector3 newPos = Vector3.Lerp(player.transform.position, firstPoint, transitionSpeed * Time.deltaTime * 2);
                player.transform.position = newPos;

                float dir = bossDoor.transform.position.x - player.transform.position.x;
                if (dir < 0)
                {
                    PlayerMovement.Instance.SetAnimationDirection(Direction.Left);
                }
                else
                {
                    PlayerMovement.Instance.SetAnimationDirection(Direction.Right);
                }

                if (dist < 0.1f)
                {
                    PlayerMovement.Instance.SetAnimationDirection(Direction.Up);
                    yield return new WaitForSeconds(0.5f);                    
                    StartCoroutine(CutScene(5));
                    up = true;
                }
            } else
            {
                Vector3 nextPoint = new Vector3(player.transform.position.x, bossDoor.transform.position.y + 5, player.transform.position.z);
                Vector3 newPos = Vector3.Lerp(player.transform.position, nextPoint, transitionSpeed * Time.deltaTime *  0.5f);
                player.transform.position = newPos;                
            }

            yield return null;
        }
    }

    public IEnumerator CutScene(float time)
    {
        darkScreen.SetActive(true);
        yield return new WaitForSeconds(time);
        darkScreen.SetActive(false);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void ReturnToStart(float time)
    {
        Time.timeScale = 1;
        StartCoroutine(CutScene(time));
        transitionObject.defeat = true;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void NewRun()
    {
        Time.timeScale = 1;
        StartCoroutine(CutScene(1));
        transitionObject.defeat = true;
    }

    public void StopTime()
    {
        StartCoroutine(SlowTime());
    }

    public IEnumerator SlowTime()
    {
        yield return new WaitForSeconds(5);

        float time = 1;
        while (time > 0) 
        {
            time -= Time.deltaTime;
            Time.timeScale = time;
            yield return null;
        }
    }
}