using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text activeFaction;

    // Singleton
    public static UI I;

    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);
    }
}
