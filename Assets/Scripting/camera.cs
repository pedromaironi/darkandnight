using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    public Transform target; // El objetivo a seguir (el personaje)
    public float smoothing = 5f; // Suavizado del seguimiento de la cámara

    private Vector3 offset; // Desplazamiento inicial entre la cámara y el objetivo

    void Start()
    {
        // Calcula el desplazamiento inicial entre la cámara y el objetivo
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Calcula la nueva posición de la cámara usando el desplazamiento inicial
            Vector3 targetPosition = target.position + offset;

            // Interpola suavemente la posición de la cámara hacia la posición objetivo
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }
}