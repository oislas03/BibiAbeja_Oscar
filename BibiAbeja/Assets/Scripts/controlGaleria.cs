// Add this script to a GameObject. The Start() function fetches an
// image from the documentation site.  It is then applied as the
// texture on the GameObject.
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;




public class controlGaleria : MonoBehaviour
{


    List<string> nombreImagenes = new List<string>();
    public string[] paths = new string[4];

    public changeImg imgg = new changeImg();
    List<Imagen> imagenesObj = new List<Imagen>();
    int indice = 1;
    int count = 10;

    string nombreActivo = "";



    void Start()
    {
        //EstadoJuego.estadoJuego.setUsuario(1);
        imagenesObj = EstadoJuego.estadoJuego.cargarImagenesPorNino();



        //Debug.Log();
        foreach (Imagen img in imagenesObj)
        {
            if (!nombreImagenes.Contains(img.nombre))
            {
                nombreImagenes.Add(img.nombre);
            }
        }

        this.nombreActivo = nombreImagenes[0];

        ponerImagen();


    }


    public void CambiarImagenIzq() {
        Debug.Log("indice" + indice);


        int i = nombreImagenes.IndexOf(nombreActivo);

         
        if (indice < count )
        {
            indice += 1;

            ponerImagen();

        }
        else {
            indice = 1;
            if ((i + 1) < nombreImagenes.Count)
            {
                this.nombreActivo = nombreImagenes[i + 1];

                ponerImagen();

            }

        }

       
    }



    public void CambiarImagenDerecha ()
    {
        Debug.Log("indice" + indice);


        int i = nombreImagenes.IndexOf(nombreActivo);


        if (indice <= count&&indice>0)
        {
            indice -= 1;

            ponerImagen();

        }
        else
        {
            indice = 4;
            if ((i) < nombreImagenes.Count && i > 0)
            {
                this.nombreActivo = nombreImagenes[i - 1];

                ponerImagen();

            }
        }

       
    } 

    public void ponerImagen() {
        AudioClip sonido;

        GameObject.Find("nNino").GetComponent<Text>().text = this.nombreActivo;
        sonido = Resources.Load<AudioClip>("Sonidos/Figuras/" + this.nombreActivo);
        GetComponent<AudioSource>().PlayOneShot(sonido);
        count = 0;

        foreach (Imagen img in imagenesObj)
        {

            if (img.nombre.Equals(nombreActivo))
            {

                if (img.numPart == indice) {
                    string imgpath = img.path;
                    imgg.colocarImagen(img.path);
                    
                }
                count++;

            }

        }


    }
}