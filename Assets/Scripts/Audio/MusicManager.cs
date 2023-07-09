using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    AudioSource source;
    bool isMuted;
    float baseVolume;

    const float FADE_IN_TIME = 2;

    public AudioClip levelMusic;
    public AudioClip bossMusic;

    public static MusicManager Instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        baseVolume = source.volume;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        source.volume = baseVolume;

        if (scene.name == "Boss")
            source.clip = bossMusic;
        else
            source.clip = levelMusic;

        source.Play();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        source.mute = isMuted;
    }

    public void FadeOutMusic()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        while(t<1)
        {
            t += Time.deltaTime/ FADE_IN_TIME;

            source.volume = Mathf.Lerp(baseVolume, 0, t);

            yield return null;
        }
    }

    public void FadeInMusic()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / FADE_IN_TIME;

            source.volume = Mathf.Lerp(0, baseVolume, t);

            yield return null;
        }
    }
}
