using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float minAlpha = 0.0f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public CanvasGroup GetCanvasGroup()
    {
        return canvasGroup;
    }

    public void FadeIn(float duration) //metodo para fade in
    {
        StartCoroutine(FadeRoutine(canvasGroup.alpha, maxAlpha, duration));
    }

    public void FadeOut(float duration) //metodo para fade out
    {
        StartCoroutine(FadeRoutine(canvasGroup.alpha, minAlpha, duration));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration) //corutina para realizar el fade
    {
        float elapsedTime = 0f; //tiempo transcurrido desde que se inicio la corutina

        while (elapsedTime < duration) //mientras el tiempo transcurrido sea menor a la duración del fade
        {
            //utilizar unscaledDeltaTime significa que el tiempo utilizado por la animación ignora Time.timeScale, así evitamos problemas
            //esto es porque cuando Time.timeScale es 0, Time.deltaTime también pasa a ser 0
            elapsedTime += Time.unscaledDeltaTime;
            //hacemos un Lerp entre el alpha inicial y el final, en función del tiempo transcurrido y la duración
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null; //esperamos un frame
        }

        canvasGroup.alpha = endAlpha; //al terminar la corutina, aseguramos que el alpha final sea exacto
    }
}
