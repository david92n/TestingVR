using UnityEngine;
using System.Collections;

public class Sword : Weapon
{
    [SerializeField] private float m_swingDetectionSlow = 20f;
    [SerializeField] private float m_swingDetectionFast = 30f;
    [SerializeField] private TransformHelper m_light;
    [SerializeField] private AudioSource m_audioIdle;
    [SerializeField] private AudioSource m_audioActive;
    [SerializeField] private AudioClip m_soundOn;
    [SerializeField] private AudioClip m_soundOff;
    [SerializeField] private AudioClip[] m_soundsSlowSwing;
    [SerializeField] private AudioClip[] m_soundsFastSwing;

    private bool m_isActive = false;
    private bool m_isAnimating = false;

    private float m_swingTimestamp;

    protected override void Awake()
    {
        base.Awake();

        if(m_isActive)
        {
            SetTransformLerp(m_light, 0f);
            m_audioIdle.volume = 1f;
            m_audioIdle.Play();
        }
        else
        {
            SetTransformLerp(m_light, 1f);
            m_audioIdle.volume = 0f;
            m_audioIdle.Stop();
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (!m_attached) return;

        base.Update();

        if (m_device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
        {
            StartCoroutine(ToggleBlade(0.3f));
        }

        if(m_isActive)
        {
            // Detect movement

            float timeSinceLastSwing = Time.time - m_swingTimestamp;

            if(timeSinceLastSwing > 0.5f)
            {
                Vector3 deltaPos = m_transform.position - m_previousPosition;
                float deltaAngle = Quaternion.Angle(m_transform.rotation, m_previousRotation);

                float movement = (deltaPos.magnitude * 1000f) + deltaAngle;

                if (movement > m_swingDetectionSlow)
                {
                    AudioClip[] clips = m_soundsSlowSwing;
                    if (movement > m_swingDetectionFast) clips = m_soundsFastSwing;

                    AudioClip movementClip = clips[Random.Range(0, clips.Length)];
                    m_audioActive.PlayOneShot(movementClip);
                    m_swingTimestamp = Time.time;
                }
            }
        }

        UpdatePreviousPositionAndRotation();
    }

    private void SetVolume(float time, float value)
    {
        m_audioIdle.volume = 1f - value;
    }

    private IEnumerator ToggleBlade(float time)
    {
        if (m_isAnimating) yield break;

        m_isAnimating = true;

        float from = m_isActive ? 0f : 1f;
        float to = m_isActive ? 1f : 0f;


        if (!m_isActive)
        {
            m_audioIdle.volume = 0f;
            m_audioIdle.Play();
        }
        else
        {
            m_audioIdle.volume = 1f;
        }

        m_audioActive.PlayOneShot(m_isActive ? m_soundOff : m_soundOn, 1f);

        yield return StartCoroutine(AnimateTransform(m_light, from, to, time, SetVolume));

        if (m_isActive)
        {
            m_audioIdle.Stop();
            m_audioIdle.volume = 0f;
        }
        else
        {
            m_audioIdle.volume = 1f;
        }

        m_isActive = !m_isActive;
        m_isAnimating = false;
    }
}
