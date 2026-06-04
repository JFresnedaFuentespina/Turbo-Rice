using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private CityGenerator cityGenerator;
    private RouteGenerator routeGenerator;
    Address startAddress;
    Address endAddress;
    void Start()
    {
        cityGenerator = GetComponent<CityGenerator>();
        routeGenerator = GetComponent<RouteGenerator>();

        StartCoroutine(TestRoute());
    }

    IEnumerator TestRoute()
    {
        while (!cityGenerator.cityGenerated)
        {
            yield return null;
        }

        Address startAddress = cityGenerator.GetRandomAddress();
        Address endAddress = cityGenerator.GetRandomAddress();
        
        Debug.Log("Testing route from " + startAddress.streetName + " " + startAddress.number +
                  " to " + endAddress.streetName + " " + endAddress.number);
        List<Road> roadRoute = routeGenerator.GetRoute(startAddress, endAddress);
        if (roadRoute != null)
        {
            Debug.Log("Route found with " + roadRoute.Count + " roads.");
            foreach (Road road in roadRoute)
            {
                Debug.Log(road.streetName + " at grid (" + road.gridX + ", " + road.gridZ + ")");
            }
        }
        else
        {
            Debug.Log("No route found.");
        }
    }
}
