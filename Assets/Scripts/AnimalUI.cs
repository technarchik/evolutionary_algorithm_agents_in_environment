using System.Collections.Generic;
using UnityEngine;

public class AnimalUI : MonoBehaviour
{
    public List<GameObject> animals;
    public GeneticAlgorithm1 geneticAlgorithm1;
    public Transform field;

    public int columns = 5;
    public int rows = 5;

    public Vector2 fieldSize = new Vector2(500, 500); // размер поля


    void PlaceAnimals()
    {
        Debug.Log("Placing animals");
        float cellWidth = fieldSize.x / columns;
        float cellHeight = fieldSize.y / rows;

        for (int i = 0; i < animals.Count; i++)
        {
            int x = i % columns;
            int y = i / columns;

            Vector2 position = new Vector2(
                x * cellWidth + cellWidth / 2,
                -(y * cellHeight + cellHeight / 2)
            );

            GameObject animal = Instantiate(animals[i], field);
            animal.GetComponent<RectTransform>().anchoredPosition = position;
        }
    }

    private void OnEnable()
    {
        GeneticAlgorithm1.PopulationCreated += PlaceAnimals;
    }
    private void OnDisable()
    {
        GeneticAlgorithm1.PopulationCreated -= PlaceAnimals;
    }
}
