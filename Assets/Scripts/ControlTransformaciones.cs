using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ControlTransformaciones : MonoBehaviour
{
    public List<Vector2> puntosOriginales = new List<Vector2>();
    public List<Vector2> puntosTransformados = new List<Vector2>();

    public LineRenderer lineaOriginal;
    public LineRenderer lineaTransformada;
    public LineRenderer lineaReflexion;  

    [Header("Inputs de los Puntos")]
    public TMP_InputField inputP1X; public TMP_InputField inputP1Y;
    public TMP_InputField inputP2X; public TMP_InputField inputP2Y;
    public TMP_InputField inputP3X; public TMP_InputField inputP3Y;
    public TMP_InputField inputP4X; public TMP_InputField inputP4Y;
    public TMP_InputField inputP5X; public TMP_InputField inputP5Y;
    public TMP_InputField inputP6X; public TMP_InputField inputP6Y;
    public TMP_InputField inputP7X; public TMP_InputField inputP7Y;
    public TMP_InputField inputP8X; public TMP_InputField inputP8Y;

    [Header("Inputs de Transformación")]
    public TMP_InputField inputAnguloRotacion;
    public TMP_InputField inputFactor;
    public TMP_InputField inputPuntoHomoteciaX; public TMP_InputField inputPuntoHomoteciaY;
    public TMP_InputField inputM; public TMP_InputField inputC;

    // =====================================================================
    // ANIMACIÓN
    // =====================================================================
    [Header("Configuración de Animación")]
    [Tooltip("Duración total de la animación en segundos. Ej: 1.5")]
    public float duracionAnimacion = 1.5f;

    [Tooltip("Tipo de easing para la animación")]
    public TipoEasing tipoEasing = TipoEasing.SmoothStep;

    /// <summary>Referencia a la coroutine activa para poder cancelarla si el usuario lanza otra acción.</summary>
    private Coroutine _coroutineAnimacion;

    /// <summary>Flag para saber si hay una animación en curso (útil para deshabilitar botones desde fuera).</summary>
    public bool EstaAnimando { get; private set; } = false;

    [Header("Referencia del Plano Cartesiano")]
    public Transform centroDelPlano;
    public const float escalaPlanoY = 0.818f;
    public const float escalaPlanoX = 1.0f;

    [Header("Líneas de Proyección (Homotecia)")]
    public GameObject prefabLineaProyeccion;
    private List<GameObject> _lineasProyeccionInstanciadas = new List<GameObject>();

    [Header("Configuración Visual")]
    [Tooltip("Extensión de las líneas de proyección fuera del plano. Ej: 20")]
    public float factorExtensionLineasProyeccion = 20f;


    // =====================================================================
    // TIPOS AUXILIARES
    // =====================================================================

    /// <summary>Opciones de easing para controlar la curva de velocidad de la animación.</summary>
    public enum TipoEasing
    {
        Lineal,      // Velocidad constante
        SmoothStep,  // Suave al inicio y al final (recomendado)
        EaseIn,      // Arranca lento, termina rápido
        EaseOut,     // Arranca rápido, termina lento
        Elastico     // Pequeño rebote al llegar al destino
    }


    // =====================================================================
    // BOTONES — ACCIONES PRINCIPALES
    // =====================================================================

    public void AplicarFigura()
    {
        puntosOriginales.Clear();
        LimpiarLineasProyeccion();
        LeerPunto(inputP1X, inputP1Y);
        LeerPunto(inputP2X, inputP2Y);
        LeerPunto(inputP3X, inputP3Y);
        LeerPunto(inputP4X, inputP4Y);
        LeerPunto(inputP5X, inputP5Y);
        LeerPunto(inputP6X, inputP6Y);
        LeerPunto(inputP7X, inputP7Y);
        LeerPunto(inputP8X, inputP8Y);

        DibujarFiguraOriginal();
        Debug.Log($"[Transformaciones] Figura aplicada con {puntosOriginales.Count} puntos.");
    }

    public void RotarFigura()
    {
        if (!ValidarPuntosMinimos()) return;

        if (!float.TryParse(inputAnguloRotacion.text, out float angulo))
        {
            Debug.LogWarning("[Transformaciones] Ángulo de rotación inválido.");
            return;
        }
        LimpiarLineasProyeccion();
        puntosTransformados.Clear();
        float radianes = angulo * Mathf.Deg2Rad;
        float cosA = Mathf.Cos(radianes);
        float sinA = Mathf.Sin(radianes);

        foreach (Vector2 p in puntosOriginales)
        {
            float xNuevo = p.x * cosA - p.y * sinA;
            float yNuevo = p.x * sinA + p.y * cosA;
            puntosTransformados.Add(new Vector2(xNuevo, yNuevo));
        }

        Debug.Log($"[Transformaciones] Rotando {angulo}°.");
        IniciarAnimacion();
    }

    /// <summary>Calcula la homotecia (escalado) y lanza la animación.</summary>
    public void EscalarFigura()
    {
        if (!ValidarPuntosMinimos()) return;

        if (!float.TryParse(inputFactor.text, out float f))
        {
            Debug.LogWarning("[Transformaciones] Factor de escala inválido.");
            return;
        }

        // Centro de homotecia (por defecto el origen)
        float hx = 0f, hy = 0f;
        if (!string.IsNullOrEmpty(inputPuntoHomoteciaX.text)) float.TryParse(inputPuntoHomoteciaX.text, out hx);
        if (!string.IsNullOrEmpty(inputPuntoHomoteciaY.text)) float.TryParse(inputPuntoHomoteciaY.text, out hy);

        puntosTransformados.Clear();
        foreach (Vector2 p in puntosOriginales)
        {
            float xNuevo = (p.x - hx) * f + hx;
            float yNuevo = (p.y - hy) * f + hy;
            puntosTransformados.Add(new Vector2(xNuevo, yNuevo));
        }

        Debug.Log($"[Transformaciones] Escalando ({f}) desde ({hx},{hy}).");
        DibujarLineasHomotecia(new Vector2(hx, hy));
        IniciarAnimacion();
    }

    public void ReflejarFigura()
    {
        if (!ValidarPuntosMinimos()) return;

        if (!float.TryParse(inputM.text, out float m))
        {
            Debug.LogWarning("[Transformaciones] Valores de m o c inválidos.");
            return;
        }
        float c = 0f;
        if (!float.TryParse(inputC.text, out c)) {}
        LimpiarLineasProyeccion();
        puntosTransformados.Clear();
        float m2 = m * m;
        float divisor = 1f + m2;
        float dos_m = 2f * m;

        foreach (Vector2 p in puntosOriginales)
        {
            // Fórmula de reflexión sobre y = mx + c
            float xNuevo = ((1f - m2) * p.x + dos_m * (p.y - c)) / divisor;
            float yNuevo = (dos_m * p.x + (m2 - 1f) * p.y + 2f * m * c) / divisor;
            puntosTransformados.Add(new Vector2(xNuevo, yNuevo));
        }

        Debug.Log($"[Transformaciones] Reflejando sobre y = {m}x + {c}."); 
        DibujarRectaReflexion(m, c);
        IniciarAnimacion();
    }
    
    // Uso de IA para las funciones de animación y transformación

    // =====================================================================
    // SISTEMA DE ANIMACIÓN 
    // =====================================================================

    public void AnimarTransformacion()
    {
        if (puntosOriginales.Count < 2 || puntosTransformados.Count < 2)
        {
            Debug.LogWarning("[Animación] Primero aplica la figura y luego una transformación.");
            return;
        }
        IniciarAnimacion();
    }
    private void IniciarAnimacion()
    {
        // Si ya hay una animación corriendo, la cancelamos limpiamente
        if (_coroutineAnimacion != null)
        {
            StopCoroutine(_coroutineAnimacion);
        }
        _coroutineAnimacion = StartCoroutine(CoroutineAnimarTransformacion());
    }

    private IEnumerator CoroutineAnimarTransformacion()
    {
        EstaAnimando = true;
        float tiempoTranscurrido = 0f;

        // Inicializamos la línea transformada con la forma original
        lineaTransformada.positionCount = puntosOriginales.Count + 1;

        while (tiempoTranscurrido < duracionAnimacion)
        {
            // Time.deltaTime es el tiempo entre el frame anterior y este.
            // Sumándolo acumulamos el tiempo total transcurrido.
            tiempoTranscurrido += Time.deltaTime;

            // t va de 0.0 (inicio) a 1.0 (fin)
            float t = Mathf.Clamp01(tiempoTranscurrido / duracionAnimacion);

            // Aplicamos la curva de easing elegida
            float tEased = AplicarEasing(t);

            // Actualizamos el LineRenderer interpolando cada punto
            ActualizarLineaInterpolada(tEased);

            // yield return null = "pausa aquí, continúa en el siguiente frame"
            // Esto es lo que hace que la animación sea suave y no bloquee el juego
            yield return null;
        }

        // Nos aseguramos de que al final la figura quede exactamente en los puntos transformados
        ActualizarLineaInterpolada(1f);

        EstaAnimando = false;
        _coroutineAnimacion = null;
        Debug.Log("[Animación] Transformación completada.");
    }

    /// <summary>
    /// Actualiza las posiciones del LineRenderer interpolando entre
    /// los puntos originales y los transformados según el valor t (0 a 1).
    /// </summary>
    private void ActualizarLineaInterpolada(float t)
    {
        int cantidad = puntosOriginales.Count;

        for (int i = 0; i < cantidad; i++)
        {
            // Lerp = Linear intERPolation: mezcla dos vectores según t
            // t=0 → punto original, t=1 → punto transformado, t=0.5 → justo en el medio
            Vector2 puntoActual = Vector2.Lerp(puntosOriginales[i], puntosTransformados[i], t);

            float posX = centroDelPlano.position.x + puntoActual.x * escalaPlanoX;
            float posY = centroDelPlano.position.y + puntoActual.y * escalaPlanoY;

            lineaTransformada.SetPosition(i, new Vector3(posX, posY, -1f));
        }

        // Cerramos la figura (conectamos el último punto con el primero)
        Vector2 cierre = Vector2.Lerp(puntosOriginales[0], puntosTransformados[0], t);
        float cX = centroDelPlano.position.x + cierre.x * escalaPlanoX;
        float cY = centroDelPlano.position.y + cierre.y * escalaPlanoY;
        lineaTransformada.SetPosition(cantidad, new Vector3(cX, cY, -1f));
    }

    /// <summary>
    /// Transforma el valor t lineal (0→1) aplicando la curva de easing seleccionada.
    /// El easing hace que la animación se sienta más natural y menos robótica.
    /// </summary>
    private float AplicarEasing(float t)
    {
        switch (tipoEasing)
        {
            case TipoEasing.Lineal:
                return t;

            case TipoEasing.SmoothStep:
                // Suave al inicio y al final — el más natural para figuras geométricas
                return Mathf.SmoothStep(0f, 1f, t);

            case TipoEasing.EaseIn:
                // Arranca lento (cuadrático)
                return t * t;

            case TipoEasing.EaseOut:
                // Termina lento
                return 1f - (1f - t) * (1f - t);

            case TipoEasing.Elastico:
                // Pequeño rebote al llegar — divertido para homotecia
                if (t == 0f || t == 1f) return t;
                float p = 0.3f;
                return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - p / 4f) * (2f * Mathf.PI) / p) + 1f;

            default:
                return t;
        }
    }


    // =====================================================================
    // DIBUJADO ESTÁTICO
    // =====================================================================

    public void DibujarFiguraOriginal()
    {
        if (puntosOriginales.Count < 2)
        {
            Debug.LogWarning("[Transformaciones] Necesitas al menos 2 puntos para dibujar.");
            return;
        }

        lineaOriginal.positionCount = puntosOriginales.Count + 1;

        for (int i = 0; i < puntosOriginales.Count; i++)
        {
            float posX = centroDelPlano.position.x + puntosOriginales[i].x * escalaPlanoX;
            float posY = centroDelPlano.position.y + puntosOriginales[i].y * escalaPlanoY;
            lineaOriginal.SetPosition(i, new Vector3(posX, posY, -1f));
        }

        // Cierre de la figura
        float cX = centroDelPlano.position.x + puntosOriginales[0].x * escalaPlanoX;
        float cY = centroDelPlano.position.y + puntosOriginales[0].y * escalaPlanoY;
        lineaOriginal.SetPosition(puntosOriginales.Count, new Vector3(cX, cY, -1f));
    }

    public void DibujarFiguraTransformada()
    {
        if (puntosTransformados.Count < 2) return;

        lineaTransformada.positionCount = puntosTransformados.Count + 1;

        for (int i = 0; i < puntosTransformados.Count; i++)
        {
            float posX = centroDelPlano.position.x + puntosTransformados[i].x * escalaPlanoX;
            float posY = centroDelPlano.position.y + puntosTransformados[i].y * escalaPlanoY;
            lineaTransformada.SetPosition(i, new Vector3(posX, posY, -1f));
        }

        float cX = centroDelPlano.position.x + puntosTransformados[0].x * escalaPlanoX;
        float cY = centroDelPlano.position.y + puntosTransformados[0].y * escalaPlanoY;
        lineaTransformada.SetPosition(puntosTransformados.Count, new Vector3(cX, cY, -1f));
    }


    // =====================================================================
    // HOMOTECIA — LÍNEAS DE PROYECCIÓN
    // =====================================================================

    private void DibujarLineasHomotecia(Vector2 puntoHomotecia)
    {
        LimpiarLineasProyeccion();

        for (int i = 0; i < puntosOriginales.Count; i++)
        {
            if (prefabLineaProyeccion == null)
            {
                Debug.LogError("[Homotecia] Falta asignar el prefabLineaProyeccion en el Inspector.");
                return;
            }

            GameObject nuevaLinea = Instantiate(prefabLineaProyeccion, transform);
            LineRenderer lr = nuevaLinea.GetComponent<LineRenderer>();
            lr.positionCount = 2;

            Vector2 dir = (puntosOriginales[i] - puntoHomotecia).normalized;
            if (dir == Vector2.zero) { lr.positionCount = 0; continue; }

            Vector2 ext1 = puntoHomotecia - dir * factorExtensionLineasProyeccion;
            Vector2 ext2 = puntoHomotecia + dir * factorExtensionLineasProyeccion;

            lr.SetPosition(0, new Vector3(
                centroDelPlano.position.x + ext1.x * escalaPlanoX,
                centroDelPlano.position.y + ext1.y * escalaPlanoY,
                -0.5f));

            lr.SetPosition(1, new Vector3(
                centroDelPlano.position.x + ext2.x * escalaPlanoX,
                centroDelPlano.position.y + ext2.y * escalaPlanoY,
                -0.5f));

            _lineasProyeccionInstanciadas.Add(nuevaLinea);
        }
    }

    public void LimpiarLineasProyeccion()
    {
        foreach (GameObject linea in _lineasProyeccionInstanciadas)
            Destroy(linea);
        _lineasProyeccionInstanciadas.Clear();

        if (lineaReflexion != null) lineaReflexion.positionCount = 0;
    }

    /// <summary>Dibuja la recta y = mx + c como referencia visual de la reflexión.</summary>
    private void DibujarRectaReflexion(float m, float c)
    {
        if (lineaReflexion == null)
        {
            Debug.LogError("[Reflexión] Falta asignar lineaReflexion en el Inspector.");
            return;
        }

        float ext = factorExtensionLineasProyeccion;

        lineaReflexion.positionCount = 2;
        lineaReflexion.SetPosition(0, new Vector3(
            centroDelPlano.position.x + (-ext) * escalaPlanoX,
            centroDelPlano.position.y + (m * (-ext) + c) * escalaPlanoY,
            -0.5f));
        lineaReflexion.SetPosition(1, new Vector3(
            centroDelPlano.position.x + ext * escalaPlanoX,
            centroDelPlano.position.y + (m * ext + c) * escalaPlanoY,
            -0.5f));
    }

    // =====================================================================
    // LIMPIEZA Y REINICIO
    // =====================================================================

    public void LimpiarFigura()
    {
        DetenerAnimacion();
        LimpiarLineasProyeccion();
        puntosOriginales.Clear();
        puntosTransformados.Clear();
        lineaOriginal.positionCount = 0;
        lineaTransformada.positionCount = 0;
    }

    public void ReiniciarAFiguraOriginal()
    {
        DetenerAnimacion();
        LimpiarLineasProyeccion();
        puntosTransformados.Clear();
        lineaTransformada.positionCount = 0;
    }

    /// <summary>Detiene cualquier animación en curso de forma segura.</summary>
    private void DetenerAnimacion()
    {
        if (_coroutineAnimacion != null)
        {
            StopCoroutine(_coroutineAnimacion);
            _coroutineAnimacion = null;
        }
        EstaAnimando = false;
    }


    // =====================================================================
    // UTILIDADES PRIVADAS
    // =====================================================================

    private void LeerPunto(TMP_InputField campoX, TMP_InputField campoY)
    {
        if (campoX == null || campoY == null) return;
        if (float.TryParse(campoX.text, out float x) && float.TryParse(campoY.text, out float y))
            puntosOriginales.Add(new Vector2(x, y));
    }

    private bool ValidarPuntosMinimos()
    {
        if (puntosOriginales.Count >= 2) return true;
        Debug.LogWarning("[Transformaciones] Ingresa al menos 2 puntos y presiona 'Aplicar Figura' primero.");
        return false;
    }
}