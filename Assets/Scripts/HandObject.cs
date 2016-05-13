using UnityEngine;
using System.Collections;

public class HandObject : MonoBehaviour
{
    [SerializeField] private GameObject m_handObject;

    private SteamVR_TrackedObject m_trackedObject;

    private GameObject m_heldObject;

    void Awake()
    {
        m_trackedObject = GetComponent<SteamVR_TrackedObject>();
    }

    public void Detach()
    {
        m_heldObject = null;
        m_handObject.SetActive(true);
    }

    void SetDeviceIndex(int index)
    {
        print("oy set device index magic: " + index);
    }

    void OnTriggerStay(Collider other)
    {
        SteamVR_Controller.Device device = SteamVR_Controller.Input((int)m_trackedObject.index);

        if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
        {
            if (m_heldObject != null) return;

            Weapon weapon = other.GetComponentInParent<Weapon>();
            if(weapon != null)
            {
                if(weapon.Attach(m_trackedObject, transform))
                {
                    m_heldObject = weapon.gameObject;
                    m_handObject.SetActive(false);
                }
            }
        }
    }
}
