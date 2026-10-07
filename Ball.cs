using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private GameObject impact;
    [SerializeField] private AudioSource ImpactSound;

    private void OnCollisionEnter(Collision collision)
    {
        impact.SetActive(true);
        ImpactSound.Play();
        Invoke("ImpactEnd", 0.15f);
    }

    private void ImpactEnd()
    {
        impact.SetActive(false);
    }
}
