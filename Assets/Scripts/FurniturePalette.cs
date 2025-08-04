
using UnityEngine;
using UnityEngine.UI;


public class FurniturePalette : MonoBehaviour
{
    [Tooltip("Sahnedeki PlanEditor referansı")]
    [SerializeField] PlanEditor editor;

    [System.Serializable]
    public struct Entry
    {
        public Button     uiButton;   // ikonlu UI buton
        public GameObject prefab;     // sahneye eklenecek prefab
    }

    [Header("Entries (Inspector’dan doldur)")]
    public Entry[] entries;

void Awake()
{
    foreach (var e in entries)
    {
        // e.uiButton gameObject’ine Outline ekle (eğer yoksa)
        var outline = e.uiButton.GetComponent<Outline>();
        if (outline == null)
            outline = e.uiButton.gameObject.AddComponent<Outline>();

        outline.enabled = false;                     // başlangıçta kapalı
        outline.effectColor    = new Color(0.25f, 0.85f, 1f);
        outline.effectDistance = new Vector2(4, 4);

        // mevcut tıklama dinleyicisini koru
        Entry cap = e;
        cap.uiButton.onClick.AddListener(() => OnIconClick(cap.prefab, cap.uiButton));
    }
}

    /*──────────────────────────────────────*/
    void OnIconClick(GameObject prefab, Button btn)
    {
        if (!editor || !prefab) return;

        //  Seçimi PlanEditor’a aktar
        editor.SetSelectedFurniture(prefab);   

        // seçilen butonu vurgula
        Highlight(btn);
    }

void Highlight(Button selected)
{
    foreach (var e in entries)
    {
        // renk geçişi
        var colors = e.uiButton.colors;
        colors.normalColor = (e.uiButton == selected) ? new Color(0.25f, 0.85f, 1f) : Color.white;
        e.uiButton.colors  = colors;

        // Outline
        var outline = e.uiButton.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = (e.uiButton == selected);
    }
}

}
