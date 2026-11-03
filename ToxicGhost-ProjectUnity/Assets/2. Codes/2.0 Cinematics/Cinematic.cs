using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
public class Cinematic : MonoBehaviour
{
    public PlayableDirector Scene;
    public float duracionCinematica;
    float t;

    public GameObject SceneObj;
    public GameObject GameplayObj;

    private void Start()
    {
        duracionCinematica = (float)Scene.duration;
        GameplayObj.SetActive(false);

    }

    private void Update()
    {
        t += Time.deltaTime;

        if(t > duracionCinematica)
        {
            SceneObj.SetActive(false);
            GameplayObj.SetActive(true);
            Destroy(this);
        }
    }
}
