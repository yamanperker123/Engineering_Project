using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPanelToggle : MonoBehaviour
{
    [SerializeField] GameObject roomPanel;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && roomPanel)
            roomPanel.SetActive(!roomPanel.activeSelf);
    }
}
