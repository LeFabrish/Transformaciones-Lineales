using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting; // Necesario para leer las cajas de texto modernas de Unity

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
    public TMP_InputField inputFactorX; public TMP_InputField inputFactorY;
    public TMP_InputField inputM; public TMP_InputField inputC;

    // Matrices de transformación 4 Componentes de una matriz 2 x 2
    private float matA, matB, matC, matD;

    // Animacion
    public float velocidadAnimacion = 2.0f;

    // Referencia del plano cartesiano 
    [Header("Referencia del Plano Cartesiano")]
    public Transform centroDelPlano;
    public float escalaPlanoY = 0.818f; // Multiplicador para convertir unidades de tu sistema a unidades de Unity (ajusta según sea necesario)
    public float escalaPlanoX = 1.0f; // Multiplicador para convertir unidades de tu sistema a unidades de Unity (ajusta según sea necesario)
    public float numeroCualquiera = 0.0f; // porq ue si xd

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
        if (float.TryParse(inputP2X.text, out float x2) && float.TryParse(inputP2Y.text, out float y2))
        {
            puntosOriginales.Add(new Vector2(x2, y2));
        }
        if (float.TryParse(inputP3X.text, out float x3) && float.TryParse(inputP3Y.text, out float y3))
        {
            puntosOriginales.Add(new Vector2(x3, y3));
        }
        if (float.TryParse(inputP4X.text, out float x4) && float.TryParse(inputP4Y.text, out float y4))
        {
            puntosOriginales.Add(new Vector2(x4, y4));
        }

        Debug.Log("Figura aplicada. Total de puntos: " + puntosOriginales.Count + " Numero Random :" + numeroCualquiera);
        // Aquí llamarías a una función para dibujar usando el LineRenderer
        DibujarFiguraOriginal();
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
    // Función para escalar

    public void EscalarFigura()
    {
        if (float.TryParse(inputFactorX.text, out float factorX) && float.TryParse(inputFactorY.text, out float factorY))
        {
            Debug.Log("Escalando la figura " + factorX + "x, " + factorY + "y.");
            // Aquí irá tu lógica de matrices de escalado
        }
        else
        {
            Debug.LogWarning("Por favor ingresa factores de escala válidos.");
        }
    }

    // Funcion para reflejar
    public void ReflejarFigura()
    {
        if (float.TryParse(inputM.text, out float m) && float.TryParse(inputC.text, out float c))
        {
            Debug.Log("Reflejando la figura sobre la línea y = " + m + "x + " + c);
            // Aquí irá tu lógica de matrices de reflexión
        }
        else
        {
            Debug.LogWarning("Por favor ingresa valores válidos para m y c.");
        }
    }

    // --- 4. APARTADO VISUAL (REDENRIZADO) ---

    // Función para dibujar la figura original en el plano cartesiano dado.
    public void DibujarFiguraOriginal()
    {
        // Validamos los puntos para al menos formar una línea (osea P's >= 2)
        if(puntosOriginales.Count < 2)
        {
            Debug.LogWarning("Necesitas al menos 2 puntos para dibujar una figura.");
            return;
        }
        // Le decimos al LineRenderer cuántos puntos va a dibujar
        // Sumamos +1 porque necesitamos al punto de inicio para "cerrar" la figura.
        lineaOriginal.positionCount = puntosOriginales.Count + 1;

        // Como Unity trabaja en 3D, convertimos nuestros Vector2 a Vector3 (con z=0)
        for (int i = 0; i < puntosOriginales.Count; i++)
        {
            // Calculamos la posicion sumando el centro del panel visual
            // Y multiplicamos por la escala para que se vea bien en Unity y calce con la cuadrícula del plano cartesiano
            float posX = centroDelPlano.position.x + puntosOriginales[i].x * escalaPlanoX;
            float posY = centroDelPlano.position.y + puntosOriginales[i].y * escalaPlanoY;

            // Z = -1 para que la linea se dibuje por delante de la imagen de fondo
            lineaOriginal.SetPosition(i, new Vector3(posX, posY, -1f));
        }
        // Cerramos la figura conectando el último punto con el primero
        float cierreX = centroDelPlano.position.x + puntosOriginales[0].x * escalaPlanoX;
        float cierreY = centroDelPlano.position.y + puntosOriginales[0].y * escalaPlanoY;
        lineaOriginal.SetPosition(puntosOriginales.Count, new Vector3(cierreX, cierreY, -1f));
    }

}