using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Unity.VisualScripting; // Necesario para leer las cajas de texto modernas de Unity

public class ControlTransformaciones : MonoBehaviour
{
    // Variables 
    public List<Vector2> puntosOriginales = new List<Vector2>();
    public List<Vector2> puntosTransformados = new List<Vector2>();
    public LineRenderer lineaOriginal;
    public LineRenderer lineaTransformada;

    [Header("Inputs de los Puntos")]
    public TMP_InputField inputP1X; public TMP_InputField inputP1Y;
    public TMP_InputField inputP2X; public TMP_InputField inputP2Y;
    public TMP_InputField inputP3X; public TMP_InputField inputP3Y;
    public TMP_InputField inputP4X; public TMP_InputField inputP4Y;

    [Header("Inputs de Transformación")]
    public TMP_InputField inputAnguloRotacion;
    public TMP_InputField inputFactorX; public TMP_InputField inputFactorY;
    public TMP_InputField inputM; public TMP_InputField inputC;

    // Debido que necesitamos una matriz de transformación general para cada tipo de transformación.
    private float matA, matB, matC, matD; 
    public const float velocidadAnimacion = 2.0f;

    [Header("Referencia del Plano Cartesiano")]
    public Transform centroDelPlano;
    public const float escalaPlanoY = 0.818f; 
    public const float escalaPlanoX = 1.0f;
    public float numeroCualquiera = 0.0f;

    // --- BONTONES ---
    public void AplicarFigura()
    {
        puntosOriginales.Clear();
        // Guardamos los puntos 
        if (float.TryParse(inputP1X.text, out float x1) && float.TryParse(inputP1Y.text, out float y1))
        {
            puntosOriginales.Add(new Vector2(x1, y1));
        }
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

        DibujarFiguraOriginal();
        Debug.Log("Figura aplicada. Total de puntos: " + puntosOriginales.Count + " Numero Random :" + numeroCualquiera);
    }
    public void RotarFigura()
    {
        if(puntosOriginales.Count < 2)
        {

            Debug.Log("Porfavor ingrese al menos dos puntos para rotar la figura ");
            return;
        }
        if (float.TryParse(inputAnguloRotacion.text, out float angulo))
        {
            Debug.Log("Rotando la figura " + angulo + " grados.");
            puntosTransformados.Clear();
            float radianes = angulo * Mathf.Deg2Rad;
            float cosA = Mathf.Cos(radianes);
            float senA = Mathf.Sin(radianes);

            for(int i = 0; i < puntosOriginales.Count; i++)
            {
                float xOriginal = puntosOriginales[i].x;
                float yOriginal = puntosOriginales[i].y;

                float xNuevo = xOriginal * cosA - yOriginal * senA;
                float yNuevo = xOriginal * senA + yOriginal * cosA; 
                puntosTransformados.Add(new Vector2(xNuevo, yNuevo));
            }

            DibujarFiguraTransformada();
        }
        else
        {
            Debug.LogWarning("Por favor ingresa un ángulo válido.");
        }
    }
    public void EscalarFigura()
    {
        if (puntosOriginales.Count <2 )
        {
            Debug.LogWarning("Por favor ingresa al menos un punto para escalar la figura.");
            return;
        }
        if (float.TryParse(inputFactorX.text, out float factorX) && float.TryParse(inputFactorY.text, out float factorY))
        {
            Debug.Log("Escalando la figura " + factorX + "x, " + factorY + "y.");
            puntosTransformados.Clear();

            for (int i = 0; i < puntosOriginales.Count; i++)
            {
                float xNuevo = puntosOriginales[i].x * factorX;
                float yNuevo = puntosOriginales[i].y * factorY;
                puntosTransformados.Add(new Vector2(xNuevo, yNuevo));
            }
            DibujarFiguraTransformada();

        }
        else
        {
            Debug.LogWarning("Por favor ingresa factores de escala válidos.");
            // Mostrar ventana de error o mensaje al usuario
            // -- Completar codigo ---
        }
    }

    // Funcion para reflejar
    public void ReflejarFigura()
    {
        if(puntosOriginales.Count < 2)
        {
            Debug.LogWarning("Por favor ingresa al menos dos puntos para reflejar la figura.");
            return;
        }
        if (float.TryParse(inputM.text, out float m) && float.TryParse(inputC.text, out float c))
        {
            Debug.Log("Reflejando la figura sobre la línea y = " + m + "x + " + c);

            puntosTransformados.Clear();
            float mCuadrado = m * m;
            float divisor = (1f + mCuadrado);
            float Dosm = 2f * m;
            float mCuadradoMenos1 = mCuadrado - 1f;
            float unoMenosMCuadrado = 1f - mCuadrado;

            for(int i = 0; i < puntosOriginales.Count; i++)
            {
                float x = puntosOriginales[i].x;
                float y= puntosOriginales[i].y;

                float xNuevo = ((unoMenosMCuadrado * x) + (Dosm * (y - c))) / divisor; 
                float yNuevo = ((Dosm * x)+ (mCuadradoMenos1 * y) + (2f*c))/divisor;

                puntosTransformados.Add(new Vector2(xNuevo, yNuevo));
            }
            DibujarFiguraTransformada();
        }
        else
        {
            Debug.LogWarning("Por favor ingresa valores válidos para m y c.");
            // Mostrar ventana de error o mensaje al usuario
            // -- Completar codigo ---
        }
    }
    public void DibujarFiguraOriginal()
    {
        if(puntosOriginales.Count < 2) // Validamos cant de puntos
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
    
    public void DibujarFiguraTransformada()
    {
        if (puntosTransformados.Count < 2) // Validamos cant de puntos
        {
            Debug.LogWarning("Necesitas al menos 2 puntos para dibujar una figura.");
            return;
        }
        // Le decimos al LineRenderer cuántos puntos va a dibujar
        // Sumamos +1 porque necesitamos al punto de inicio para "cerrar" la figura.
        lineaTransformada.positionCount = puntosTransformados.Count + 1;

        // Como Unity trabaja en 3D, convertimos nuestros Vector2 a Vector3 (con z=0)
        for (int i = 0; i < puntosTransformados.Count; i++)
        {
            // Calculamos la posicion sumando el centro del panel visual
            // Y multiplicamos por la escala para que se vea bien en Unity y calce con la cuadrícula del plano cartesiano
            float posX = centroDelPlano.position.x + puntosTransformados[i].x * escalaPlanoX;
            float posY = centroDelPlano.position.y + puntosTransformados[i].y * escalaPlanoY;

            // Z = -1 para que la linea se dibuje por delante de la imagen de fondo
            lineaTransformada.SetPosition(i, new Vector3(posX, posY, -1f));
        }
        // Cerramos la figura conectando el último punto con el primero
        float cierreX = centroDelPlano.position.x + puntosTransformados[0].x * escalaPlanoX;
        float cierreY = centroDelPlano.position.y + puntosTransformados[0].y * escalaPlanoY;
        lineaTransformada.SetPosition(puntosTransformados.Count, new Vector3(cierreX, cierreY, -2f));

    }

}