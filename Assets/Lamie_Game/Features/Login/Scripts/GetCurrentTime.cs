using UnityEngine;
using RTLTMPro;
using System;

public class CurrentTime : MonoBehaviour
{
    public RTLTextMeshPro timeText;

    void Update()
    {
        timeText.text = DateTime.Now.ToString("HH:mm");
    }
}