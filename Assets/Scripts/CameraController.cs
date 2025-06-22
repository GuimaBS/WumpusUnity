using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float baseAltura = 8f;          
    public float alturaPorTile = 1.2f;      
    public float alturaMinima = 8f;        

    void Start()
    {
        int xSize = PlayerPrefs.GetInt("mapX", 5);
        int ySize = PlayerPrefs.GetInt("mapY", 5);

        // Centro do mapa
        float centroX = (xSize - 1) / 2f;
        float centroZ = (ySize - 1) / 2f;

        // Ajuste de altura mais generoso
        int tamanhoMax = Mathf.Max(xSize, ySize);
        float altura = baseAltura + Mathf.Pow(tamanhoMax, 1.15f);
        altura = Mathf.Max(altura, alturaMinima);

        // Posiciona a câmera
        transform.position = new Vector3(centroX, altura, centroZ);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}