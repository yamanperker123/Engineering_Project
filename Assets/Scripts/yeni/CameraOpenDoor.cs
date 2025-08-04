using UnityEngine;

namespace CameraDoorScript
{
    /// <summary>
    /// Oyuncu / kamera önündeki kapıyı E tuşuyla tetikler.
    /// </summary>
    public class CameraOpenDoor : MonoBehaviour
    {
        public float DistanceOpen = 3f;   // Ray uzunluğu (metre)
        // public GameObject text;        // "Press E" UI göstergesi kullanacaksan aç

        void Update()
        {
            RaycastHit hit;
            bool hasDoor =
                Physics.Raycast(transform.position,
                                transform.forward,
                                out hit,
                                DistanceOpen) &&
                hit.transform.GetComponent<DoorInteraction>() != null;

            // text?.SetActive(hasDoor);   // UI isteğe bağlı

            if (hasDoor && Input.GetKeyDown(KeyCode.E))
            {
                hit.transform.GetComponent<DoorInteraction>().ToggleDoor();
            }
        }
    }
}
