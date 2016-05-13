using UnityEngine;
using System.Collections;

public class BulletShell : MonoBehaviour
{
    [SerializeField] private AudioClip[] m_clips;

    private bool m_didHit = false;

    private AudioSource m_audio;

    void Awake()
    {
        m_audio = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (m_didHit) return;
        m_didHit = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb != null) rb.angularVelocity = Vector3.zero;

        m_audio.clip = m_clips[Random.Range(0, m_clips.Length)];
        m_audio.Play();
    }
}
