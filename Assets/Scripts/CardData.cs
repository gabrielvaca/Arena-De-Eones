using UnityEngine;

// Esto a�ade una opci�n en el men� de Unity para crear cartas nuevas f�cilmente
[CreateAssetMenu(fileName = "NuevaCarta", menuName = "Arena/Carta")]
public class CardData : ScriptableObject
{
    [Header("Informaci�n B�sica")]
    public string nombreCarta;
    public Sprite icono; // La imagen para la UI
    [TextArea] public string descripcion;

    [Header("Juego")]
    public int costoMana;
    public GameObject prefabUnidad; // El modelo 3D que aparecerá en la arena
}