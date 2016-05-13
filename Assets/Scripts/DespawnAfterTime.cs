using UnityEngine;
using System.Collections;

public class DespawnAfterTime : MonoBehaviour
{
    [SerializeField] private float m_despawnTime = 10f;

    private float m_spawnTime;

    void Awake()
    {
        m_spawnTime = Time.time;
    }

    void Update()
    {
        if(Time.time - m_spawnTime >= m_despawnTime)
        {
            Destroy(gameObject);
        }
    }
}
