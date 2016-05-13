using UnityEngine;
using System.Collections;

public class HandObject : MonoBehaviour
{
    [SerializeField] private GameObject m_handObject;

    private int m_deviceIndex = -1;

    private bool m_attachedThisFrame = false;
    private GameObject m_heldObject;

    void Awake()
    {
    }

    public void Detach()
    {
        m_heldObject = null;
        m_handObject.SetActive(true);
    }

    void SetDeviceIndex(int index)
    {
        m_deviceIndex = index;
    }

    void Update()
    {
        SteamVR_Controller.Device device = SteamVR_Controller.Input(m_deviceIndex);

        if (!m_attachedThisFrame && m_heldObject != null && device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
        {
            Weapon weapon = m_heldObject.GetComponentInParent<Weapon>();
            if (weapon != null)
            {
                if(weapon.Detach())
                {
                    m_heldObject = null;
                    m_handObject.SetActive(true);
                }
            }
        }
        m_attachedThisFrame = false;
    }

    void OnTriggerStay(Collider other)
    {
        SteamVR_Controller.Device device = SteamVR_Controller.Input(m_deviceIndex);

        if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
        {
            if (m_heldObject != null) return;

            Weapon weapon = other.GetComponentInParent<Weapon>();
            if(weapon != null)
            {
                if(weapon.Attach(device, transform))
                {
                    m_attachedThisFrame = true;
                    m_heldObject = weapon.gameObject;
                    m_handObject.SetActive(false);
                }
            }
        }
    }
}
