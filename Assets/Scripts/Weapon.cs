using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    [System.Serializable]
    protected class TransformHelper
    {
        public Transform m_transform;
        public Transform m_from;
        public Transform m_to;
    }

    protected delegate void AnimationCallback(float time, float value);

    protected SteamVR_Controller.Device m_device;

    protected bool m_attached = false;

    protected Transform m_transform;

    protected Vector3 m_previousPosition;
    protected Vector3 m_previousForward;
    protected Quaternion m_previousRotation;

    protected virtual void Awake()
    {
        m_transform = transform;
        m_previousPosition = m_transform.position;
        m_previousForward = m_transform.forward;
        m_previousRotation = m_transform.rotation;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (!m_attached) return;
    }

    /// <summary>
    /// Put this at the end of the update loop.
    /// </summary>
    protected void UpdatePreviousPositionAndRotation()
    {
        m_previousPosition = m_transform.position;
        m_previousForward = m_transform.forward;
        m_previousRotation = m_transform.rotation;
    }

    public bool Attach(SteamVR_Controller.Device device, Transform parent)
    {
        if (m_attached) return false;

        m_attached = true;

        m_transform.SetParent(parent, false);
        m_transform.localPosition = Vector3.zero;
        m_transform.localRotation = Quaternion.identity;

        m_device = device;

        return true;
    }

    public bool Detach()
    {
        if (!m_attached) return false;

        HandObject hand = m_transform.parent.GetComponent<HandObject>();
        if (hand != null) hand.Detach();

        m_transform.SetParent(null, true);
        m_attached = false;
        m_device = null;

        return true;
    }

    protected void SetTransformLerp(TransformHelper transformHelper, float value)
    {
        transformHelper.m_transform.localPosition = Vector3.Lerp(transformHelper.m_from.localPosition, transformHelper.m_to.localPosition, value);
        transformHelper.m_transform.localRotation = Quaternion.Lerp(transformHelper.m_from.localRotation, transformHelper.m_to.localRotation, value);
        transformHelper.m_transform.localScale = Vector3.Lerp(transformHelper.m_from.localScale, transformHelper.m_to.localScale, value);
    }

    protected IEnumerator AnimateTransform(TransformHelper transformHelper, float from, float to, float duration, AnimationCallback animCallback = null)
    {
        float startTime = Time.time;
        SetTransformLerp(transformHelper, from);
        while (Time.time - startTime <= duration)
        {
            float time = (Time.time - startTime) / duration;
            float value = Mathf.Lerp(from, to, time);
            SetTransformLerp(transformHelper, value);
            if (animCallback != null) animCallback(time, value);
            yield return new WaitForEndOfFrame();
        }
        SetTransformLerp(transformHelper, to);
    }
}
