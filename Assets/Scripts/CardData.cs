using UnityEngine;

[CreateAssetMenu(fileName = "NuevaCarta", menuName = "Arena/Carta")]
public class CardData : ScriptableObject
{
    [Header("Información Básica")]
    public string nombreCarta;
    public Sprite icono;
    [TextArea] public string descripcion;

    [Header("Juego")]
    public int costoMana;
    public GameObject prefabUnidad;

    [Header("Multijugador")]
    // NUEVO: Este número debe coincidir con la posición en la lista del CardPlayController
    public int unitId;
}