using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Configurar juego")]
    [Tooltip("Estamos en el descanso entre preguntas")] public float descanso;
    [Tooltip("Errores máximos permitidos")] public int erroresMaximos = 3;
    [Tooltip("Multiplicador de velocidad de descanso")][Range(0.0001f,5f)] public float speedMult = .25f;
    [Header("Imagenes")]

    [Tooltip("Panel inicial")] public GameObject panelLogo;
    [Tooltip("Panel de respuesta")] public GameObject panelRespuesta;
    [Tooltip("Botón cerrar")] public GameObject btnCerrar;
    [Tooltip("Botón reiniciar")] public GameObject btnReiniciar;
    [Tooltip("Botón pista")] public GameObject btnPista;
    [Header("Configurar texto")]
    public TextMeshProUGUI textoFinReloj;
    [Tooltip("Texto de respuesta correcta")]
    public TextMeshProUGUI textoCorrecto;
    [Tooltip("Texto de respuesta incorrecta")]
    public TextMeshProUGUI textoErroneo;
    [Tooltip("Texto de partida perdida")]
    public TextMeshProUGUI textoGameOver;
    [Tooltip("Texto de pregunta actual")]
    public TextMeshProUGUI textoPregunta;
    [Tooltip("Texto de pista actual")]
    public TextMeshProUGUI textoPista;
    [Tooltip("Texto fin de partida ganada")]
    public TextMeshProUGUI textoFinal;
    [Tooltip("Texto contador de preguntas")]
    public TextMeshProUGUI textoContador;
    [Header("Configurar preguntas y respuesta")]
    [Tooltip("Prefab de botón de respuesta")] public GameObject prefabBoton;
    [Tooltip("Contenedor de las preguntas")] public Transform contenedorPreguntas;
    [Tooltip("Lista de preguntas")] public List<Preguntas> preguntas;
    [Header("Debug")]
    [Tooltip("Estamos en pausa")] public bool pausa;
    [Tooltip("Pregunta Actual")] public int preguntaActual;
    [Tooltip("Preguntas totales")] public int preguntasTotales;
    [Tooltip("Contador de errores")] public int errores = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeRight;
        Time.timeScale = 1;
        panelRespuesta.gameObject.SetActive(false);
        PausarJuego();
        preguntasTotales = preguntas.Count;
        DesactivarTexto();
        EstadoBotones(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (errores >= erroresMaximos)
        {
            GameOver();
        }
        else
        {
            if (pausa)
            {
                if (descanso > 0) descanso -= Time.deltaTime * speedMult;
                else
                {
                    ReanudarPartida();
                    DesactivarTexto();
                    if (preguntas.Count > 0)
                    {
                        BorrarBotones();
                        CargarPregunta();
                    }
                    else
                    {
                        FinDelJuego();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Se llama el fin de partida por perdida
    /// </summary>
    private void GameOver()
    {
        DesactivarTexto();
        textoGameOver.enabled = true;
        Time.timeScale = 0;
        EstadoBotones(true);
    }

    /// <summary>
    /// Se llama el fin del juego por ganar la partida
    /// </summary>
    private void FinDelJuego()
    {
        textoFinal.enabled = true;
        panelLogo.gameObject.SetActive(true);
        Time.timeScale = 0;
        EstadoBotones(true);
    }

    /// <summary>
    /// Desactiva todos los mensajes
    /// </summary>
    private void DesactivarTexto()
    {
        textoFinReloj.enabled = false;
        textoCorrecto.enabled = false;
        textoErroneo.enabled = false;
        textoGameOver.enabled = false;
        textoFinal.enabled = false;
    }

    /// <summary>
    /// Pausa el juego entre rondas
    /// </summary>
    public void PausarJuego()
    {
        pausa = true;
        descanso = 1;
        if (preguntaActual == 0) panelLogo.gameObject.SetActive(true);
        else panelRespuesta.gameObject.SetActive(true);
    }

    /// <summary>
    /// Se retoma la partida
    /// </summary>
    private void ReanudarPartida()
    {
        pausa = false;
        panelLogo.gameObject.SetActive(false);
        panelRespuesta.gameObject.SetActive(false);
    }

    /// <summary>
    /// Botón respuesta correcta
    /// </summary>
    public void RespuestaCorrecta()
    {
        textoCorrecto.enabled = true;
    }

    /// <summary>
    /// Botón Respuesta Incorrecta
    /// </summary>
    public void RespuestaErronea()
    {
        textoErroneo.enabled = true;
        errores++;
    }

    /// <summary>
    /// Carga la siguente Pregunta
    /// </summary>
    private void CargarPregunta()
    {
        // Primero cargamos una pregunta
        int aleatorio = Random.Range(0, preguntas.Count);
        var pregunta = preguntas[aleatorio];
        textoPregunta.text = pregunta.textoPregunta;
        textoPista.text = pregunta.textoPista;
        textoPista.gameObject.SetActive(false);
        btnPista.gameObject.SetActive(true);
        int i = 0;
        // Para cada respuesta en la lista creamos botón, cambiamos texto y añadimos funciones
        foreach (var respuesta in pregunta.respuestas)
        {
            GameObject nuevaRespuesta = Instantiate(prefabBoton, contenedorPreguntas);
            if (nuevaRespuesta.TryGetComponent<Button>(out var boton))
            {
                boton.onClick.AddListener(PausarJuego);
                if (i == pregunta.respuestaCorrecta)
                {
                    boton.onClick.AddListener(RespuestaCorrecta);
                }
                else
                {
                    boton.onClick.AddListener(RespuestaErronea);
                }
                boton.onClick.AddListener(BorrarBotones);
            }
            var texto = nuevaRespuesta.GetComponentInChildren<TextMeshProUGUI>();
            texto.text = respuesta;
            i++;
        }
        // Por ultimo quitasmos la pregunta de la lista y aumentamos contador de preguntas
        preguntas.Remove(pregunta);
        preguntaActual++;
        textoContador.text = $"Pregunta {preguntaActual} de {preguntasTotales}";
    }

    /// <summary>
    /// Borra todos los botones de la ronda anterior
    /// </summary>
    private void BorrarBotones()
    {
        foreach (Transform child in contenedorPreguntas)
        {
            Destroy(child.gameObject);
        }
    }

    private void EstadoBotones(bool estado)
    {
        btnCerrar.SetActive(estado);
        btnReiniciar.SetActive(estado);
    }
}
