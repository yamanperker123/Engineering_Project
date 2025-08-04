// Assets/Scripts/UI/FurnitureUIController.cs
using UnityEngine;

public class FurnitureUIController : MonoBehaviour
{
    [Header("Hierarchy refs")]
    [SerializeField] GameObject guiButtonsRoot;   
    [SerializeField] GameObject furniturePanel;  

    void Awake()
    {
       
        if (furniturePanel)    furniturePanel.SetActive(false);
        if (guiButtonsRoot)    guiButtonsRoot.SetActive(true);
    }

    /* ------------ UI  ------------ */
   
    public void OpenFurniturePanel()
    {
        if (!furniturePanel || !guiButtonsRoot) return;
        furniturePanel.SetActive(true);
        guiButtonsRoot.SetActive(false);
    }

   
    public void CloseFurniturePanel()
    {
        if (!furniturePanel || !guiButtonsRoot) return;
        furniturePanel.SetActive(false);
        guiButtonsRoot.SetActive(true);
    }
}
