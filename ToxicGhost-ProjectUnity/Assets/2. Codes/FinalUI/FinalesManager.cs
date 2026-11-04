
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalesManager : MonoBehaviour
{
    [Header("Referencias")]
    public ToxicitySystem sistemaToxicidad;
    public GameObject canvasFinales;
    public Image imagenFinal;

    [Header("Imágenes de los finales")]
    public Sprite finalBueno;
    public Sprite finalIntermedio;
    public Sprite finalMalo;

    [Header("Rangos de toxicidad")]
    [Range(0, 100)] public float limiteBueno = 33f;
    [Range(0, 100)] public float limiteIntermedio = 66f;

    public void MostrarFinal()
    {
        if (sistemaToxicidad == null ||
            canvasFinales == null ||
            imagenFinal == null)
        {
            Debug.LogError("Faltan referencias en FinalesManager.");
            return;
        }

        float toxicidad = sistemaToxicidad.toxicidad;

        if (toxicidad <= limiteBueno)
        {
            imagenFinal.sprite = finalBueno;
        }
        else if (toxicidad <= limiteIntermedio)
        {
            imagenFinal.sprite = finalIntermedio;
        }
        else
        {
            imagenFinal.sprite = finalMalo;
        }

        imagenFinal.preserveAspect = true;
        canvasFinales.SetActive(true);

        Debug.Log("Final activado. Toxicidad: " + toxicidad);
    }

        public void VolverAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

        void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            MostrarFinal();
        }
    }
}

