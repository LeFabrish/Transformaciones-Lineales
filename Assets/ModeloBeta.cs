using UnityEngine;
using System.Collections.Generic; // Permite usar Listas en lugar de Arrays fijos

public class ControladorTransformaciones : MonoBehaviour
{
    // --- 1. PUNTOS DE LA FIGURA (Almacenar puntos en Array/Lista) ---
    // Usamos List porque el usuario puede elegir un triángulo (3 puntos) o un cuadrado (4 puntos)
    public List<Vector2> puntosOriginales = new List<Vector2>();
    public List<Vector2> puntosTransformados = new List<Vector2>();

    // --- 2. DIBUJO Y COMPARACIÓN (Dibujar figura / Comparar figura) ---
    // LineRenderer es la herramienta de Unity para unir puntos con líneas
    public LineRenderer lineaOriginal;
    public LineRenderer lineaTransformada;

    // --- 3. VARIABLES DE ENTRADA (Las 4 ramas de tu diagrama) ---
    public float anguloRotacion;     // Para la rama ROTAR
    public float escalaX, escalaY;   // Para la rama ESCALAR
    public string ejeReflexion;      // Para la rama REFLEJAR (puede ser "X" o "Y")
    public float factorCizallamiento; // Para la rama CIZALLAR (Factor K)

    // --- 4. VARIABLES DE LA MATRIZ (Calcular nueva posición) ---
    // Los 4 componentes de una matriz 2x2
    private float matA, matB;
    private float matC, matD;

    // --- 5. ANIMACIÓN (Iniciar animación progresiva) ---
    // Controla qué tan rápido se mueve la figura de la posición original a la nueva
    public float velocidadAnimacion = 2.0f;
}
