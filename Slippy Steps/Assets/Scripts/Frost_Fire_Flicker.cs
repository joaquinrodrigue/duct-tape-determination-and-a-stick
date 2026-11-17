using UnityEngine;
using System.Collections;

public class Frost_Fire_Flicker : MonoBehaviour
{
    [SerializeField] private GameObject frostFireLight;
    [SerializeField] private float flickerSpeed = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (frostFireLight == null)
        {
            Debug.LogError("Frost Fire Light is not assigned in the inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        frostFireLight.GetComponent<Light>().intensity = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) * 2.0f;
    }
}
