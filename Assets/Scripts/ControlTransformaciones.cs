using UnityEngine;
using System.Collections.Generic;
using TMPro; // Necesario para leer las cajas de texto modernas de Unity

public class ControlTransformaciones : MonoBehaviour
{
    // --- 1. PUNTOS Y LÍNEAS (Tus variables originales) ---
    public List<Vector2> puntosOriginales = new List<Vector2>();
    public List<Vector2> puntosTransformados = new List<Vector2>();
    public LineRenderer lineaOriginal;
    public LineRenderer lineaTransformada;

    // --- 2. REFERENCIAS A LA INTERFAZ (NUEVO) ---
    // Aquí arrastraremos las cajas de texto desde el Inspector de Unity
    [Header("Inputs de los Puntos")]
    public TMP_InputField inputP1X; public TMP_InputField inputP1Y;
    public TMP_InputField inputP2X; public TMP_InputField inputP2Y;
    public TMP_InputField inputP3X; public TMP_InputField inputP3Y;
    public TMP_InputField inputP4X; public TMP_InputField inputP4Y;

    [Header("Inputs de Transformación")]
    public TMP_InputField inputAnguloRotacion;
    // Agrega más según necesites (escalaX, escalaY, etc.)

    // --- 3. FUNCIONES PARA LOS BOTONES (NUEVO) ---

    // Esta función se conectará al botón "APLICAR FIGURA"
    public void AplicarFigura()
    {
        puntosOriginales.Clear();

        // Leemos el texto de la caja P1 y lo convertimos a número flotante
        if (float.TryParse(inputP1X.text, out float x1) && float.TryParse(inputP1Y.text, out float y1))
        {
            puntosOriginales.Add(new Vector2(x1, y1));
        }
        // Puedes repetir este bloque para P2, P3 y P4...

        Debug.Log("Figura aplicada. Total de puntos: " + puntosOriginales.Count);
        // Aquí llamarías a una función para dibujar usando el LineRenderer
    }

    // Esta función se conectará al botón "ROTAR"
    public void RotarFigura()
    {
        if (float.TryParse(inputAnguloRotacion.text, out float angulo))
        {
            Debug.Log("Rotando la figura " + angulo + " grados.");
            // Aquí irá tu lógica de matrices de rotación
        }
        else
        {
            Debug.LogWarning("Por favor ingresa un ángulo válido.");
        }
    }
}