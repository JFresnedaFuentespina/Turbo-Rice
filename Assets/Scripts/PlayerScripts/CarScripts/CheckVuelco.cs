using UnityEngine;

public class CheckVuelco : MonoBehaviour
{
    public CarBehaviour car;
    public Rigidbody rb;

    public float tiempoParaReset = 3f;

    float tiempoVolcado;

    void Update()
    {
        bool estaQuieto =
            rb.linearVelocity.magnitude < 0.5f;

        bool estaVolcado =
            Vector3.Dot(transform.up, Vector3.up) < 0.3f;

        if (estaQuieto && estaVolcado)
        {
            tiempoVolcado += Time.deltaTime;

            if (tiempoVolcado >= tiempoParaReset)
            {
                car.GirarCoche();
                tiempoVolcado = 0f;
            }
        }
        else
        {
            tiempoVolcado = 0f;
        }
    }
}