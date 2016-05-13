using UnityEngine;
using System.Collections;

public class Pistol : Weapon
{    
    [SerializeField] private int m_clipSize = 8;

    [SerializeField] private TransformHelper m_magazine;
    [SerializeField] private TransformHelper m_cockingHandle;
    [SerializeField] private TransformHelper m_trigger;
    [SerializeField] private Transform m_bulletSpawn;
    [SerializeField] private Transform m_shellSpawn;
    [SerializeField] private GameObject m_shellPrefab;
    [SerializeField] private GameObject m_magPrefab;

    [SerializeField] private AudioClip m_soundShoot;
    [SerializeField] private AudioClip m_soundClick;
    [SerializeField] private AudioClip m_soundMagRelease;
    [SerializeField] private AudioClip m_soundMagInsert;
    [SerializeField] private AudioClip m_soundHandleRelease;

    private int m_currentAmmo;

    private bool m_canShoot = true;
    private bool m_magIn = true;

    private AudioSource m_audio;
    
    protected override void Awake()
    {
        base.Awake();
        m_currentAmmo = m_clipSize;
        m_audio = GetComponent<AudioSource>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (!m_attached) return;

        base.Update();

        Vector2 triggerInput = m_device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
        SetTransformLerp(m_trigger, triggerInput.x);

        if (triggerInput.x > 0.95f && m_canShoot)
        {
            Shoot();
        }
        else if (triggerInput.x < 0.2f && !m_canShoot)
        {
            m_canShoot = true;
        }

        if (m_device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
        {
            StartCoroutine(DropMag());
        }

        /*
        Quaternion pistolDeltaRot = Quaternion.Inverse(m_previousRotation) * m_transform.rotation;
        print(pistolDeltaRot.eulerAngles);
        */

        UpdatePreviousPositionAndRotation();
    }

    private void Shoot()
    {
        m_canShoot = false;

        if (!m_magIn || m_currentAmmo <= 0)
        {
            m_audio.PlayOneShot(m_soundClick);
            return;
        }

        m_currentAmmo--;

        StartCoroutine(Cocking());
        m_audio.PlayOneShot(m_soundShoot);

        RaycastHit hitInfo;
        if(Physics.Raycast(m_bulletSpawn.position, m_bulletSpawn.forward, out hitInfo))
        {
            if(hitInfo.rigidbody)
            {
                hitInfo.rigidbody.AddForceAtPosition(m_bulletSpawn.forward * 1f, hitInfo.point, ForceMode.Impulse);
            }
        }

        //m_device.TriggerHapticPulse(10000);
    }

    private IEnumerator DropMag()
    {
        if (!m_magIn) yield break;

        m_magIn = false;

        float magDropTime = 0.15f;

        m_audio.PlayOneShot(m_soundMagRelease);
        yield return StartCoroutine(AnimateTransform(m_magazine, 0f, 1f, magDropTime));

        m_magazine.m_transform.gameObject.SetActive(false);

        GameObject magInst = Instantiate(m_magPrefab, m_magazine.m_to.position, m_magazine.m_to.rotation) as GameObject;
        
        Rigidbody rb = magInst.GetComponent<Rigidbody>();
        if(rb != null)
        {
            Vector3 deltaPosition = m_magazine.m_to.position - m_magazine.m_from.position;
            Vector3 dir = deltaPosition.normalized;

            float force = deltaPosition.magnitude / magDropTime;

            Vector3 pistolDeltaPos = m_transform.position - m_previousPosition;

            Vector3 startVelocity = dir * force + (pistolDeltaPos.normalized * pistolDeltaPos.magnitude) / Time.deltaTime;
            rb.AddForce(startVelocity, ForceMode.VelocityChange);

            /*
            Quaternion pistolDeltaRot = Quaternion.Inverse(m_previousRotation) * m_transform.rotation;
            rb.AddTorque(pistolDeltaRot.eulerAngles, ForceMode.VelocityChange);
            */
        }

        yield return new WaitForSeconds(1f);

        m_audio.PlayOneShot(m_soundMagInsert);
        m_magazine.m_transform.gameObject.SetActive(true);
        yield return StartCoroutine(AnimateTransform(m_magazine, 1f, 0f, magDropTime));

        if(m_currentAmmo > 0)
            yield return StartCoroutine(AnimateTransform(m_cockingHandle, 0f, 1f, 0.2f));
        m_audio.PlayOneShot(m_soundHandleRelease);
        yield return StartCoroutine(AnimateTransform(m_cockingHandle, 1f, 0f, 0.1f));

        m_currentAmmo = m_clipSize;

        m_magIn = true;
    }

    private IEnumerator Cocking()
    {
        float cockTime = 0.05f;

        yield return StartCoroutine(AnimateTransform(m_cockingHandle, 0f, 1f, cockTime));

        if (m_shellPrefab != null && m_shellSpawn != null)
        {
            GameObject shellInst = Instantiate(m_shellPrefab, m_shellSpawn.position, m_shellSpawn.rotation) as GameObject;
            Rigidbody rb = shellInst.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddRelativeForce(new Vector3(1, 1, 0) * 0.06f, ForceMode.Impulse);
                rb.AddRelativeTorque(new Vector3(Random.value, Random.value, Random.value), ForceMode.Impulse);
            }
        }
        
        if(m_currentAmmo > 0) yield return StartCoroutine(AnimateTransform(m_cockingHandle, 1f, 0f, cockTime));
    }
}
