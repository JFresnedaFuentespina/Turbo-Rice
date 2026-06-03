using UnityEngine;

public class TestingCityGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private CityGenerator cityGenerator;
    void Start()
    {
        cityGenerator = GetComponent<CityGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            cityGenerator.ResetCity();
        }
    }
}
