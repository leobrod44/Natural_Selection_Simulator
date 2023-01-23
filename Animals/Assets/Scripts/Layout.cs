using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class Layout : MonoBehaviour
{
    public int mapSize;

    public int numberOfPuddles;

    public Area[,] sections;

    public static int MAPSIZE;
    public Severity density;
    public Severity grassRate;
    public Severity obstacleRate;
    public Severity treeRate;
    public Severity plantRate;

    public List<Material> materials;

    #region Nature Accessers

    private UnityEngine.Object[] NatureObjects;
    private List<UnityEngine.Object> Grass { get; set; }
    private List<UnityEngine.Object> Obstacles { get; set; }
    private List<UnityEngine.Object> Trees { get; set; }
    private List<UnityEngine.Object> Plants { get; set; }

    private List<string> plantNames;

    //animals
    public int animalCount;

    private List<Body> animals;

    public List<GameObject> skeletons;

    #endregion
    void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
       
        MAPSIZE = mapSize;
        sections = new Area[mapSize, mapSize];
        LoadNatureElements();
        CreateSections(numberOfPuddles);

        animals = new List<Body>();
        for (int i = 0; i < animalCount; i++)
        {
            CreateAnimal(AnimalType.Quadruped);
        }

    }

    public void CreateAnimal(AnimalType type)
    {
        var x = Random.Range(0, MAPSIZE);
        var y = Random.Range(0, MAPSIZE);

        GameObject skeleton = Instantiate(skeletons[0]);
        skeleton.transform.position = new Vector3(x, 0.5f, y);
        //Determine body characteristics 
        var bodySize = Random.Range(0.2f, 1);
        var eyeSize = Random.Range(0.1f, 0.5f);
        var legSize = Random.Range(0.2f, 0.8f);

        Body newAnimalBody = new Body(skeleton, bodySize, eyeSize, legSize, x, y);
        animals.Add(newAnimalBody);
    }


    private void CreateSections(int numPuddles)
    {
        GeneratePuddles(numPuddles);
        plantNames = Plants.Select(x => x.name).ToList();
        GenerateGrass();
    }
    private void GeneratePuddles(int numPuddles)
    {
        for (int puddleCount = 0; puddleCount < numPuddles; puddleCount++)
        {
            var puddleSeed = UnityEngine.Random.Range(15, mapSize / 3);
            var puddleThickness = UnityEngine.Random.Range(puddleSeed / 2, puddleSeed);
            var puddleSourceX = UnityEngine.Random.Range(0, mapSize);
            var puddleSourceY = UnityEngine.Random.Range(0, mapSize);
            PopulateWaterArea(new Vector3(puddleSourceX, 0, puddleSourceY), puddleThickness, GenerateOrientation());
        }
    }

    private void PopulateWaterArea(Vector3 sourcePos, int sourceThickness, Vector3 orientation)
    {
        bool xOriented = orientation.x != 0;

        var firstTickness = UnityEngine.Random.Range(1, 3) == 1 ? sourceThickness + 1 : sourceThickness - 2;
        var secondTickness = UnityEngine.Random.Range(1, 3) == 1 ? sourceThickness + 1 : sourceThickness - 2;

        if (xOriented)
        {
            for (int i = (int)sourcePos.z - sourceThickness / 2; i < (int)sourcePos.z + sourceThickness / 2; i++)
            {
                if (i < mapSize && i >= 0)
                    sections[(int)sourcePos.x, i] = new WaterArea((int)sourcePos.x, i);
            }
            PopulateWaterAreaInnerXOriented(sourcePos, firstTickness, orientation);
            PopulateWaterAreaInnerXOriented(sourcePos, secondTickness, -orientation);
        }
        else
        {
            for (int i = (int)sourcePos.x - sourceThickness / 2; i < (int)sourcePos.x + sourceThickness / 2; i++)
            {
                if (i < mapSize && i >= 0)
                    sections[i, (int)sourcePos.z] = new WaterArea(i, (int)sourcePos.z);
            }
            PopulateWaterAreaInnerYOriented(sourcePos, firstTickness, orientation);
            PopulateWaterAreaInnerYOriented(sourcePos, secondTickness, -orientation);
        }

    }

    private void PopulateWaterAreaInnerXOriented(Vector3 sourcePos, int sourceThickness, Vector3 orientation)
    {
        if (sourcePos.x >= mapSize || sourcePos.x < 0 || sourcePos.z >= mapSize || sourcePos.z < 0)
        {
            return;
        }
        for (int i = (int)sourcePos.z - sourceThickness / 2; i < (int)sourcePos.z + sourceThickness / 2; i++)
        {
            if (i < mapSize && i >= 0)
                sections[(int)sourcePos.x, i] = new WaterArea((int)sourcePos.x, i);
        }

        var newTickness = UnityEngine.Random.Range(1, 4) == 1 ? sourceThickness + 1 : sourceThickness - 2;

        if (newTickness > 0)
            PopulateWaterAreaInnerXOriented(sourcePos + orientation, newTickness, orientation);
    }

    private void PopulateWaterAreaInnerYOriented(Vector3 sourcePos, int sourceThickness, Vector3 orientation)
    {
        if (sourcePos.x >= mapSize || sourcePos.x < 0 || sourcePos.z >= mapSize || sourcePos.z < 0)
        {
            return;
        }
        for (int i = (int)sourcePos.x - sourceThickness / 2; i < (int)sourcePos.x + sourceThickness / 2; i++)
        {
            if (i < mapSize && i >= 0)
                sections[i, (int)sourcePos.z] = new WaterArea(i, (int)sourcePos.z);
        }

        var newTickness = UnityEngine.Random.Range(1, 4) == 1 ? sourceThickness + 1 : sourceThickness - 2;

        if (newTickness > 0)
            PopulateWaterAreaInnerYOriented(sourcePos + orientation, newTickness, orientation);
    }

    private void GenerateGrass()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (sections[i, j] == null)
                {
                    sections[i, j] = new GrassArea(i, j);
                    GameObject element = GenerateNatureObjects(i, j);
                    if (element != null)
                    {
                        sections[i, j].Element = element;

                        if (plantNames.Contains(element.name.Replace("(Clone)", "")))
                        {
                            sections[i, j].Type = AreaType.Food;
                        }
                        else
                        {
                            sections[i, j].Type = AreaType.Grass;
                        }
                        //else if(Grass.Contains((UnityEngine.Object)element))
                        //{

                        //}

                    }
                }
            }
        }
    }

    private GameObject GenerateNatureObjects(int x , int y)
    {
        float hitRate = (float)density / 5f;
        float successCondition = UnityEngine.Random.Range(0f, 1f);
        if (successCondition > hitRate)
        {
            return null;
        }
        int grass= (int)grassRate;
        int obstacle = (int)obstacleRate;
        int tree = (int)treeRate;
        int  plant = (int)plantRate;
        var total = grass + obstacle + plant+tree;
        if(total ==0)
        {
            return null;
        }
        List<List<UnityEngine.Object>> objects = new List<List<UnityEngine.Object>>();

        for (int i = 0; i < grass; i++)
        {
            objects.Add(Grass);
        }
        for (int i = 0; i < obstacle; i++)
        {
            objects.Add(Obstacles);
        }
        for (int i = 0; i < tree; i++)
        {
            objects.Add(Trees);
        }
        for (int i = 0; i < plant; i++)
        {
            objects.Add(Plants);
        }
        //add tree factor because a lot
        var random = UnityEngine.Random.Range(0, total);
        var scaleChange = UnityEngine.Random.Range(0, 2);
        GameObject newElement = Instantiate(objects[random][UnityEngine.Random.Range(0, objects[random].Count)] as GameObject);
        newElement.transform.position = new Vector3(x, 0, y);
        newElement.transform.localScale += new Vector3(scaleChange, scaleChange, scaleChange);

        return newElement;
    }

    #region helpers
    private Vector3 GenerateOrientation()
    {
        var direction = UnityEngine.Random.Range(0, 2);
        return direction == 0 ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1);
    }

    private void LoadNatureElements()
    {
        NatureObjects = Resources.LoadAll("SimpleNaturePack/Prefabs", typeof(GameObject)); //.Select(x=> x as GameObject);
        Grass = NatureObjects.Where(x => x.name.Contains("Grass")).ToList();
        Obstacles = NatureObjects.Where(x => x.name.Contains("Branch") || x.name.Contains("Rock") || x.name.Contains("Stump")).ToList();
        Trees = NatureObjects.Where(x => x.name.Contains("Tree")).ToList();
        Plants = NatureObjects.Where(x => x.name.Contains("Bush") || x.name.Contains("Mushroom")).ToList();
    }
    #endregion

}
public enum Severity
{
    None,
    Low,
    Medium,
    High,
    VeryHigh
}

public enum AnimalType
{
    Quadruped,
    Biped,
    Insect
}