using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HandCursor : MonoBehaviour
{

    private PXCMSenseManager sm;
    private PXCMHandCursorModule cursorModule;
    private PXCMCursorConfiguration cursorConfig;
    private PXCMSession session;
    private PXCMCursorData cursorData;
    private PXCMCursorData.GestureData gestureData;
    private PXCMPoint3DF32 adaptivePoints;
    private PXCMPoint3DF32 coordinates2d;


    //public List<Rect> linea;
    //public Material punto = new Material(Shader.Find("Unlit/Color"));
    //public Texture2D puntoImage;
    public Texture2D cursorImage;
    public Vector3 mousePos;
    public bool click;
    public static HandCursor me;
    public delegate void JugadorEmpiezaDibujar(String id);
    public static event JugadorEmpiezaDibujar Contacto;
    public Vector3 v3 = new Vector3();
    public bool primeraVez = true;

    // variable para mantener conteo de un segundo en el juego
    public float tiempo;


    private void Awake()
    {
        if (me == null)
        {
            me = this;

        }
        else if (me != this)
        {
            Destroy(gameObject);
        }
        cursorImage = Resources.Load("Textures/pen", typeof(Texture2D)) as Texture2D;
    }


    private void Start()
    {
        //linea = new List<Rect>();
        //Time.timeScale = 1;
        ConfigureRealSense();
        Update();
    }


    void OnMouseEnter()
    {
        UnityEngine.Cursor.visible = true;
    }

    public void ConfigureRealSense()
    {
        // Create an instance of the SenseManager
        sm = PXCMSenseManager.CreateInstance();

        // Enable cursor tracking
        sm.EnableHandCursor();

        // Create a session of the RealSense
        session = PXCMSession.CreateInstance();

        // Get an instance of the hand cursor module
        cursorModule = sm.QueryHandCursor();
        // Get an instance of the cursor configuration
        cursorConfig = cursorModule.CreateActiveConfiguration();

        // Make configuration changes and apply them
        cursorConfig.EnableEngagement(true);
        cursorConfig.EnableAllGestures();
        cursorConfig.EnableAllAlerts();
        cursorConfig.ApplyChanges();

        // Initialize the SenseManager pipeline
        sm.Init();
    }

    private void Update()
    {
        StartCoroutine("toUpdate");
    }

    IEnumerator toUpdate()
    {
        bool handInRange = true;

        if (sm.AcquireFrame(true).IsSuccessful())
        {
            // Hand and cursor tracking 
            cursorData = cursorModule.CreateOutput();
            adaptivePoints = new PXCMPoint3DF32();
            coordinates2d = new PXCMPoint3DF32();
            PXCMCursorData.BodySideType bodySide;

            cursorData.Update();

            // Check if alert data has fired
            for (int i = 0; i < cursorData.QueryFiredAlertsNumber(); i++)
            {
                PXCMCursorData.AlertData alertData;
                cursorData.QueryFiredAlertData(i, out alertData);

                if ((alertData.label == PXCMCursorData.AlertType.CURSOR_NOT_DETECTED) ||
                    (alertData.label == PXCMCursorData.AlertType.CURSOR_DISENGAGED) ||
                    (alertData.label == PXCMCursorData.AlertType.CURSOR_OUT_OF_BORDERS))
                {
                    handInRange = false;
                }
                else
                {
                    handInRange = true;
                }
            }


            if (cursorData.IsGestureFired(PXCMCursorData.GestureType.CURSOR_CLICK, out gestureData))
            {
                click = true;
                Debug.Log("Click cambiando a true");
            }

            // Track hand cursor if it's within range
            int detectedHands = cursorData.QueryNumberOfCursors();

            if (detectedHands > 0)
            {
                // Retrieve the cursor data by order-based index
                PXCMCursorData.ICursor iCursor;
                cursorData.QueryCursorData(PXCMCursorData.AccessOrderType.ACCESS_ORDER_NEAR_TO_FAR,
                                           0,
                                           out iCursor);

                adaptivePoints = iCursor.QueryAdaptivePoint();

                // Retrieve controlling body side (i.e., left or right hand)
                bodySide = iCursor.QueryBodySide();

                //Debug.Log("Resolución actual: " + Screen.currentResolution.height + ", " + Screen.currentResolution.width);

                coordinates2d.x = (adaptivePoints.x * Screen.currentResolution.width);
                coordinates2d.y = (adaptivePoints.y * Screen.currentResolution.height);

                mousePos = new Vector3(coordinates2d.x, coordinates2d.y, 20);
                //mousePos = new Vector3(coordinates2d.x, coordinates2d.y, 20);

                //v3 = Camera.main.WorldToScreenPoint(mousePos);
            }
            else
            {
                bodySide = PXCMCursorData.BodySideType.BODY_SIDE_UNKNOWN;
            }

            // Resume next frame processing
            cursorData.Dispose();
            sm.ReleaseFrame();
        }
        yield return null;
    }

    Ray ray;
    RaycastHit hit;

    void OnGUI()
    {
        //mousePos = Camera.main.ScreenToWorldPoint(v3);

        // Cuadrado y posición en la pantalla que ayuda a dibujar el lápiz.
        Rect posa = new Rect(mousePos.x, Screen.currentResolution.height - mousePos.y, cursorImage.width, cursorImage.height);
        GUI.Label(posa, cursorImage);

        ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out hit))
        {
            // Acciones a ejecutar para la escena del paso 1
            if (SceneManager.GetActiveScene().name == "paso 1")
            {
                Debug.Log("Paso 1 krnal");

                if (hit.collider.tag == "salir")
                {
                    SceneManager.LoadScene("eligeTema");
                }
                else if (hit.collider.tag == "punto")
                {
                    var nombre = hit.collider.gameObject.name;
                    nombre = nombre.Substring(5);
                    // Revisa que haya un objeto suscrito al evento
                    if (Contacto != null)
                    {
                        // llama al evento del observador
                        Contacto(nombre);
                    }

                }
                if (hit.collider.tag == "areaDibujable")
                {
                    GameObject.Find("DrawLine").GetComponent<DrawLine>().Dibujar(mousePos);
                }
            }

            // Acciones a ejecutar para la escena del paso 3
            if (SceneManager.GetActiveScene().name == "paso 3")
            {
                if (hit.collider != null)
                {
                    if (hit.transform.tag == "silabaLetra")
                    {
                        // al hacer un click se ejecuta el evento de "arrastrar" la sílaba
                        //if (click)
                        //{
                        //}
                        //click = false;

                        if (hit.transform.gameObject.GetComponent<SpriteRenderer>().sprite == null)
                        {
                            if (llamarCorutina)
                            {
                                devolver_pieza();
                                llamarCorutina = false;
                            }
                        }
                        else if (isCursorPen)
                        {
                            if (llamarCorutina)
                            {
                                agarrar_pieza();
                                llamarCorutina = false;
                            }
                        }
                    }
                    else if (hit.transform.tag == "silabaEspacio")
                    {
                        if (isCursorSyllable)
                        {
                            //StartCoroutine("colocarPieza");
                            if (llamarCorutina)
                            {
                                colocar_pieza();
                                llamarCorutina = false;
                            }
                        }
                    }
                }
            }
        }
    }

    bool isCursorPen = true;
    bool isCursorSyllable = false;
    bool llamarCorutina = true;

    string silabaAgarrada;

    public void agarrar_pieza()
    {
        StartCoroutine("agarrarPieza");
    }

    IEnumerator agarrarPieza()
    {
        if (isCursorPen)
        {
            float tiempoRestante = tiempo;
            while (tiempoRestante > 0)
            {
                Debug.Log("Tiempo restante en agarrar pieza: " + tiempoRestante);
                yield return new WaitForSeconds(1);
                tiempoRestante--;
            }

            ray = Camera.main.ScreenPointToRay(mousePos);
            Debug.Log("Physics.Raycast y su valor: " + Physics.Raycast(ray, out hit));
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Intentando detectar la colisión con el espacio de la sílaba");
                Debug.Log(hit.transform.gameObject.name);
                if (hit.transform.gameObject != null)
                {
                    // Se obtiene el objeto al que toca el Raycast
                    GameObject objetoSilaba = hit.transform.gameObject;
                    // Se obtiene el nombre del source image que tiene la sílaba que se tocó
                    string silaba = objetoSilaba.GetComponent<SpriteRenderer>().sprite.name;
                    silabaAgarrada = silaba;
                    // Se carga la nueva textura y se le da el tamaño adecuado
                    Texture2D imagen = Resources.Load("Textures/" + silaba, typeof(Texture2D)) as Texture2D;
                    cursorImage = imagen;

                    // Atributos de semáforo cambian de estado
                    isCursorPen = false;
                    isCursorSyllable = true;
                    llamarCorutina = true;

                    // Se desactiva la imagen de la sílaba que se agarró con el cursor
                    objetoSilaba.GetComponent<SpriteRenderer>().sprite = null;
                }
            }
            else
            {
                llamarCorutina = true;
            }
        }
        else
        {
            Debug.Log("El cursor es una sílaba");
        }
        yield return null;
    }

    public void colocar_pieza()
    {
        StartCoroutine("colocarPieza");
    }

    IEnumerator colocarPieza()
    {
        if (isCursorSyllable)
        {
            float tiempoRestante = tiempo;
            while (tiempoRestante > 0)
            {
                Debug.Log("Tiempo restante en colocar pieza: " + tiempoRestante);
                yield return new WaitForSeconds(1);
                tiempoRestante--;

            }
            ray = Camera.main.ScreenPointToRay(mousePos);
            Debug.Log("Physics.Raycast y su valor: " + Physics.Raycast(ray, out hit));
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Intentando detectar la colisión con el espacio vacío");
                Debug.Log(hit.transform.gameObject.name);
                if (hit.transform.gameObject != null)
                {
                    Debug.Log("Intentando dejar la pieza en el lugar seleccionado");

                    // Se obtiene el GameObject del espacio vacío, después se coloca la pieza del cursor sobre él
                    GameObject espacioVacio = hit.transform.gameObject;
                    SpriteRenderer imagenSilaba = espacioVacio.GetComponent<SpriteRenderer>();
                    imagenSilaba.sprite = Resources.Load("Sprites/" + silabaAgarrada, typeof(Sprite)) as Sprite;

                    // Una vez colocada la pieza en su lugar, se devuelve al cursor la imagen de la pluma
                    cursorImage = Resources.Load("Textures/pen", typeof(Texture2D)) as Texture2D;

                    // Atributos de semáforo cambian de estado
                    isCursorPen = true;
                    isCursorSyllable = false;
                    llamarCorutina = true;
                }
            }
            else
            {
                llamarCorutina = true;
            }
        }
        else
        {
            Debug.Log("El cursor es una pluma");
        }
        yield return null;
    }

    public void devolver_pieza()
    {
        StartCoroutine("devolverPieza");
    }

    IEnumerator devolverPieza()
    {
        float tiempoRestante = tiempo;
        while (tiempoRestante > 0)
        {
            Debug.Log("Tiempo restante en colocar pieza: " + tiempoRestante);
            yield return new WaitForSeconds(1);
            tiempoRestante--;

        }
        ray = Camera.main.ScreenPointToRay(mousePos);
        Debug.Log("Physics.Raycast y su valor: " + Physics.Raycast(ray, out hit));
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Intentando detectar la colisión con el espacio vacío");
            Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.gameObject != null)
            {
                Debug.Log("Intentando dejar la pieza agarrada en un lugar del tablero de piezas");

                // Se obtiene el GameObject del espacio vacío, después se coloca la pieza del cursor sobre él
                GameObject espacioVacio = hit.transform.gameObject;
                SpriteRenderer imagenSilaba = espacioVacio.GetComponent<SpriteRenderer>();
                imagenSilaba.sprite = Resources.Load("Sprites/" + silabaAgarrada, typeof(Sprite)) as Sprite;

                // Una vez colocada la pieza en su lugar, se devuelve al cursor la imagen de la pluma
                cursorImage = Resources.Load("Textures/pen", typeof(Texture2D)) as Texture2D;
                // Atributos de semáforo cambian de estado
                isCursorPen = true;
                isCursorSyllable = false;
                llamarCorutina = true;
            }
        }
        else
        {
            llamarCorutina = true;
        }
        yield return null;
    }

    public void mostrarError(int windowID)
    {
        if (GUI.Button(new Rect(10, 20, 100, 20), "Hello World"))
            print("Got a click");
    }
}