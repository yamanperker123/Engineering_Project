using System.Collections;
using UnityEngine;


public class DoorInteraction : MonoBehaviour
{
    [Header("Açılma Ayarları")]
    public float openAngle = 90f;   // Y ekseninde döneceği açı
    public float openSpeed = 2f;    // Lerp çarpanı
    public bool  isOpen    = false; // Başlangıç durumu

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine  _currentCoroutine;

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation   = Quaternion.Euler(
            transform.eulerAngles + new Vector3(0f, openAngle, 0f));
    }

    /// <summary> kapıyı aç / kapat </summary>
    public void ToggleDoor()
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        Quaternion target = isOpen ? _closedRotation : _openRotation;
        _currentCoroutine = StartCoroutine(RotateDoor(target));
        isOpen = !isOpen;
    }

    private IEnumerator RotateDoor(Quaternion target)
    {
        while (Quaternion.Angle(transform.rotation, target) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation, target, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.rotation = target;
        _currentCoroutine  = null;
    }
}
