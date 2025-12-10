using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SplashScreenManager : MonoBehaviour
{
    [Header("Configuración de Carga")]
    [SerializeField] private float minDisplayTime = 2.5f;

    [Header("Configuración de Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip splashSound;

    [Header("Configuración de Animación")]
    [Tooltip("Objeto que se va a animar.")]
    [SerializeField] private RectTransform elementToZoom;
    [SerializeField] private float zoomDuration = 2.0f;
    [SerializeField] private float targetScale = 1.2f;

    private const string MainMenuSceneName = "MenuPrincipal";

    void Start()
    {
        if (elementToZoom == null)
        {
            Debug.LogWarning("No se asignó 'elementToZoom'. Usando la Transformación de este objeto.");
            elementToZoom = GetComponent<RectTransform>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        StartCoroutine(LoadMainMenuAfterDelay());
    }

    private IEnumerator LoadMainMenuAfterDelay()
    {
        if (splashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(splashSound);
        }

        StartCoroutine(AnimateZoom(elementToZoom, targetScale, zoomDuration));

        yield return new WaitForSeconds(minDisplayTime);

        SceneManager.LoadScene(MainMenuSceneName);
    }

    private IEnumerator AnimateZoom(RectTransform rectTransform, float targetScale, float duration)
    {
        float time = 0f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, 1f);

        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = endScale;
    }
}