using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    float deltaTime;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        float fps = 1f / deltaTime;
        float ms = deltaTime * 1000f;

        GUI.Label(
            new Rect(10, 10, 300, 40),
            $"{fps:F0} FPS ({ms:F2} ms)"
        );
    }
}