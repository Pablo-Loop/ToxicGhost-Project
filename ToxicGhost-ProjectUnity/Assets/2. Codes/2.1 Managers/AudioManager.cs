using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Canales de Audio")]
    [SerializeField] private AudioSource musicaSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Música de Fondo")]
    public AudioClip musicaFondoJuego;

    [Header("SFX - Interfaz y Sistema")]
    public AudioClip sfxCerrarHoja;

    [Header("SFX - Celular y Mensajes")]
    public AudioClip sfxNotificacionCelular; // Tono de mensaje/chat de Maya

    [Header("SFX - Jugador (Gasp)")]
    public AudioClip sfxPerderVida;
    public AudioClip sfxPickUpVida;
    public AudioClip sfxHintSound;

    [Header("SFX - Enemigos (Fantasmas)")]
    public AudioClip sfxMuerteFantasma;
    public AudioClip sfxAtaqueFantasma;
    public AudioClip PolillaSpot;
    public AudioClip sfxScreamer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (musicaFondoJuego != null)
        {
            PlayMusica(musicaFondoJuego);
        }
    }

    #region Métodos de Reproducción

    public void PlayMusica(AudioClip clip)
    {
        if (musicaSource == null || clip == null) return;
        musicaSource.clip = clip;
        musicaSource.loop = true;
        musicaSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // --- Métodos de disparo rápido ---

    public void ReproducirNotificacionCelular()
    {
        PlaySFX(sfxNotificacionCelular);
    }

    public void ReproducirPerderVida()
    {
        PlaySFX(sfxPerderVida);
    }

    public void ReproducirMuerteFantasma()
    {
        PlaySFX(sfxMuerteFantasma);
    }

    public void ReproducirAtaqueFantasma()
    {
        PlaySFX(sfxAtaqueFantasma);
    }

    public void ReproducirScreamer()
    {
        PlaySFX(sfxScreamer);
    }

    public void ReproducirPolillaSpot()
    {
        PlaySFX(PolillaSpot); 
    }

    public void ReproducirHintSound()
    {
        PlaySFX(sfxHintSound);
    }

    #endregion
}