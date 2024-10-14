using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using static BuildingScript;
using System.Linq;
using TreeEditor;
using Codice.CM.Common.Tree.Partial;
using Codice.Client.BaseCommands;
using System.Collections;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using DataStructures.PriorityQueue;
using System.Security.Policy;

public class WorldBuilderEditor : EditorWindow
{
    public bool active = false;

    public BuildingObjectSO selectedObjectSO;
    public int tileSize = 1;
    public int height = 1; 
    public Vector3 origin = Vector3.zero;
   

    BuildingScript.Rotation currentRotation = BuildingScript.Rotation.forward;
    Vector3Int rotationOffset = new Vector3Int(0, 0, 0);
    Vector3Int tickers;

    mode currentMode;

    GameObject previewObject;
    bool previewIsRed;
    Material previewRed;
    Material previewBlue;

    PrefabCreationSO selectedPrefab;

    Transform parentTransform;

    EditorScriptHelper helper;

    bool[,] visited;
    List<Vector2Int> villages;

    enum mode
    {
        selectMode,
        placeMode
    }
    
    [MenuItem("CustomTools/WorldBuilder")]
    private static void ShowWindow()
    {
        var window = GetWindow<WorldBuilderEditor>();
        window.titleContent = new GUIContent(text: "WorldBuilder");
        window.Show();
    }

    private void OnEnable()
    {
     
    }


    private void OnFocus()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;

        tileSize = 1;
        height = 0;
        origin = Vector3.zero;
        rotationOffset = new Vector3Int(0, 0, 0);

        Debug.Log("focused");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Reset"))
        {
            ClearObjects();
            selectedPrefab.Reset(selectedPrefab.gridDimensions);
        }

        if (GUILayout.Button("Save"))
        {
            SaveToJson();
        }

        if (GUILayout.Button("Load"))
        {
            ClearObjects();
            selectedPrefab.Reset(selectedPrefab.gridDimensions);
            LoadFromJson();
        }


        if (GUILayout.Button("Generate Perlin"))
        {
            float starttime = Time.realtimeSinceStartup;
            ClearObjects();
            selectedPrefab.Reset(selectedPrefab.gridDimensions);
            GenerateIslandPerlin();
            Debug.Log(Time.realtimeSinceStartup - starttime);
        }

        if (GUILayout.Button("Generate WFC"))
        {
            ClearObjects();
            selectedPrefab.Reset(selectedPrefab.gridDimensions);
            GenerateIslandWFC();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Selected Object", GUILayout.Width(100));
        BuildingObjectSO prev = selectedObjectSO;
        selectedObjectSO = (BuildingObjectSO)EditorGUILayout.ObjectField(selectedObjectSO, typeof(BuildingObjectSO), false, GUILayout.Width(200));
        if(prev != selectedObjectSO)
        {
            DestroyImmediate(previewObject);
            previewObject = Instantiate(selectedObjectSO.preview);
        }
        EditorGUILayout.EndHorizontal();


        Texture2D objectTexture = AssetPreview.GetAssetPreview(selectedObjectSO.prefab);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Preview", GUILayout.Width(100));
        EditorGUILayout.ObjectField(objectTexture, typeof(Texture2D), false, GUILayout.Height(100),GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Mode", GUILayout.Width(100));
        currentMode = (mode)EditorGUILayout.EnumPopup(currentMode);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Rotation", GUILayout.Width(100));
        currentRotation = (BuildingScript.Rotation)EditorGUILayout.EnumPopup(currentRotation);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Preview Object", GUILayout.Width(100));
        previewObject = (GameObject)EditorGUILayout.ObjectField(previewObject, typeof(GameObject), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Preview object default color", GUILayout.Width(100));
        previewBlue = (Material)EditorGUILayout.ObjectField(previewBlue, typeof(Material), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Preview object blocked color", GUILayout.Width(100));
        previewRed = (Material)EditorGUILayout.ObjectField(previewRed, typeof(Material), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Selected Parent", GUILayout.Width(100));
        parentTransform = (Transform)EditorGUILayout.ObjectField(parentTransform, typeof(Transform), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Helper Script", GUILayout.Width(100));
        helper = (EditorScriptHelper)EditorGUILayout.ObjectField(helper, typeof(EditorScriptHelper), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        selectedPrefab.gridDimensions = EditorGUILayout.Vector3Field("Grid Dimension", selectedPrefab.gridDimensions, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();





        //this.Repaint();
    }

    public void OnSceneGUI(SceneView sceneView)
    {
        if (!active)
        {
            return;
        }

        selectedPrefab = parentTransform.gameObject.GetComponent<GridHolder>().selectedPrefab;
        origin = parentTransform.position;
        

        if (currentRotation == Rotation.forward) { tickers = new Vector3Int(1, 1, 1); rotationOffset = new Vector3Int(0, 0, 0); }
        else if (currentRotation == Rotation.right) { tickers = new Vector3Int(1, 1, -1); rotationOffset = new Vector3Int(0, 0, 1); }
        else if (currentRotation == Rotation.back) { tickers = new Vector3Int(-1, 1, -1); rotationOffset = new Vector3Int(1, 0, 1); }
        else if (currentRotation == Rotation.left) { tickers = new Vector3Int(-1, 1, 1); rotationOffset = new Vector3Int(1, 0, 0); }

        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.R) {
                CycleRotation();
            }
            
        }

        if (selectedObjectSO == null)
        {
            if (previewObject != null)
            {
                Destroy(previewObject);
            }
            return;
        }

        

        RaycastHit hit;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        int layerMask = 1 << 8;

        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {

            if (currentMode == mode.placeMode)
            {
                previewObject.SetActive(true);
                Vector3Int normalCheck = GetXYZ(hit.normal * tileSize);

                //if(normalCheck.x > normalCheck.y && normalCheck.x > normalCheck.z){
                //normalCheck
                //}

                Vector3Int normalOffset = new Vector3Int(Mathf.Min(normalCheck.x * (selectedObjectSO.x - 1), 0), normalCheck.y * (selectedObjectSO.y - 1), Mathf.Min(normalCheck.z * (selectedObjectSO.z - 1), 0));

                Vector3Int resizedRotationalOffset = rotationOffset;

                if (normalCheck.x < 0)
                {
                    resizedRotationalOffset.x *= -(normalCheck.x * selectedObjectSO.x);
                }
                else if (normalCheck.z < 0)
                {
                    resizedRotationalOffset.z *= -(normalCheck.z * selectedObjectSO.z);
                }
                else if (normalCheck.x > 0)
                {
                    resizedRotationalOffset.x *= (normalCheck.x * selectedObjectSO.x);
                }
                else if (normalCheck.z > 0)
                {
                    resizedRotationalOffset.z *= (normalCheck.z * selectedObjectSO.z);
                }

                //Debug.Log("normal:" + normalOffset+ "      Rotation:" + resizedRotationalOffset);


                Vector3Int checkCoordsVisual = GetXYZ(hit.point + hit.normal * 0.9f * tileSize) + normalOffset + resizedRotationalOffset;
                Vector3Int checkCoordsFuntional = checkCoordsVisual - rotationOffset;

                //Debug.Log(checkCoords);
                //previewObjectTargetCoords = worldPos - origin;
                //previewObjectStartCoords = previewObjectOffset;

                previewObject.transform.position = checkCoordsVisual;
                previewObject.transform.rotation = Quaternion.Euler(0, (int)currentRotation * 90f, 0);

                bool canBuild = CheckCanBuild(checkCoordsFuntional, selectedObjectSO.x, selectedObjectSO.y, selectedObjectSO.z);

                if (canBuild && previewIsRed)
                {
                    previewObject.transform.GetChild(0).GetComponent<Renderer>().material = previewBlue;
                    previewIsRed = false;
                }
                else if (!canBuild && !previewIsRed)
                {
                    previewObject.transform.GetChild(0).GetComponent<Renderer>().material = previewRed;
                    previewIsRed = true;
                }


                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {

                        AttemptBuild(checkCoordsFuntional, checkCoordsVisual, canBuild, selectedObjectSO);
                        
                    }
                }
            }
            else
            {
                previewObject.SetActive(false);
            }
        }
        else
        {
            previewObject.transform.position = ray.GetPoint(20);
            if (!previewIsRed)
            {
                previewObject.transform.GetChild(0).GetComponent<Renderer>().material = previewRed;
                previewIsRed = true;
            }
        }

    }

    
    private Vector3Int GetTargetPoint(Vector2 currentMousePosition)
    {
        var cam = Camera.current;
        Debug.Log(cam.name);
        if (cam == null) { return Vector3Int.zero; }
        var ray = HandleUtility.GUIPointToWorldRay(currentMousePosition);
        var hPlane = new Plane(Vector3.up, Vector3.up * height * tileSize);
        if (!hPlane.Raycast(ray, out var enter)) { return Vector3Int.zero; }
        var hit = ray.GetPoint(enter);
        Debug.Log(hit);
        Debug.Log(GetXYZ(hit));
        return GetXYZ(hit);
    }
    

    Vector3Int GetXYZ(Vector3 worldPos)
    {
        Vector3 recentered = worldPos - origin;
        return new Vector3Int(Mathf.FloorToInt(recentered.x / tileSize), Mathf.FloorToInt(recentered.y / tileSize), Mathf.FloorToInt(recentered.z / tileSize));
    }
    
    bool CheckCanBuild(Vector3Int start, int x, int y, int z)
    {
        if(start.x + x >= selectedPrefab.grid.grid.x || start.y + y >= selectedPrefab.grid.grid.y || start.z + z >= selectedPrefab.grid.grid.z)
        {
            return false;
        }
        for (int Xcheck = 0; Xcheck < x; Xcheck++)
        {
            for (int Ycheck = 0; Ycheck < y; Ycheck++)
            {
                for (int Zcheck = 0; Zcheck < z; Zcheck++)
                {
                    if (selectedPrefab.grid.grid.GetValue(Xcheck + start.x, Ycheck + start.y, Zcheck+start.z).occupied == true)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
    


    public void AttemptBuild(Vector3Int checkCoord, Vector3 placeCoord, bool canBuild, BuildingObjectSO toBuild, Rotation rot = Rotation.none)
    {
        
        if (rot != Rotation.none)
        {
            SetRotation(rot);
        }
        if (!canBuild)
        {
            return;
        }

        selectedPrefab.savedObjects.Add(new SaveGridObject(toBuild.ID, checkCoord.x, checkCoord.y, checkCoord.z, currentRotation));

        for (int x = 0; x < toBuild.x; x++)
        {
            for (int y = 0; y < toBuild.y; y++)
            {
                for (int z = 0; z < toBuild.z; z++)
                {
                    Vector3Int currentCheckCoords = new Vector3Int(checkCoord.x + x * tickers.x, checkCoord.y + y * tickers.y, checkCoord.z + z * tickers.z);
                    selectedPrefab.grid.grid.GetValue(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z).occupied = true;
                    selectedPrefab.grid.grid.GetValue(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z).section = toBuild.sections.GetValue(x, y, z);

                    GameObject built = (GameObject)PrefabUtility.InstantiatePrefab(toBuild.sections.GetValue(x, y, z).prefab);
                    built.transform.position = GetWorldPosition(placeCoord);
                    built.transform.rotation = Quaternion.Euler(0, (int)currentRotation * 90f, 0);
                    built.transform.SetParent(parentTransform);
                    built.GetComponent<BuildingSectionScript>().coords = currentCheckCoords;
                    built.GetComponent<BuildingSectionScript>().buildingSectionSO = toBuild.sections.GetValue(x, y, z);
                    built.GetComponent<BuildingSectionScript>().SetHP(toBuild.sections.GetValue(x, y, z).maxHp);

                    AddOccupiedtoPathfindingNode(currentCheckCoords, toBuild.sections.GetValue(x, y, z).walkableDirs);

                    /*
                    if (y == 0)
                    {
                        BuildPillar(currentCheckCoords.x, currentCheckCoords.y - 1, currentCheckCoords.z);
                    }
                    */

                    if (y == toBuild.y - 1 && toBuild.topWalkable)
                    {
                        AddFloorToPathfindNode(new Vector3Int(currentCheckCoords.x, currentCheckCoords.y + 1, currentCheckCoords.z));
                    }

                    if (toBuild.walkableLevels.Contains(y))
                    {
                        AddFloorToPathfindNode(new Vector3Int(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z));
                    }
                }
            }
        }
        
    }

    void AddOccupiedtoPathfindingNode(Vector3Int occupiedSpot, Vector3Int[] walkableDirs)
    {
        Vector3Int[] rotatedWalkableDirs = new Vector3Int[walkableDirs.Length];
        selectedPrefab.grid.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.hasFloor = true;

        for (int i = 0; i < walkableDirs.Length; i++)
        {
            rotatedWalkableDirs[i] = RotateVectors(walkableDirs[i], currentRotation);
        }

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (rotatedWalkableDirs.Contains(new Vector3Int(x - 1, y - 1, z - 1)))
                    {
                        selectedPrefab.grid.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.enterableSides.UpdateValue(x, y, z, true);
                        selectedPrefab.grid.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.walls.UpdateValue(x, y, z, false);
                    }
                    else
                    {
                        selectedPrefab.grid.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.enterableSides.UpdateValue(x, y, z, false);
                        selectedPrefab.grid.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.walls.UpdateValue(x, y, z, true);
                    }
                }
            }
        }
        //UpdateSuroundingPathfindingNodes(occupiedSpot);
    }

    void AddFloorToPathfindNode(Vector3Int floorSpot)
    {
        if (selectedPrefab.grid.grid.GetValue(floorSpot.x, floorSpot.y, floorSpot.z).pathfindingNode.hasFloor)
        {
            return;
        }

        selectedPrefab.grid.grid.GetValue(floorSpot.x, floorSpot.y, floorSpot.z).pathfindingNode.hasFloor = true;

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    selectedPrefab.grid.grid.GetValue(floorSpot.x, floorSpot.y, floorSpot.z).pathfindingNode.enterableSides.UpdateValue(x, 1, z, true);
                    selectedPrefab.grid.grid.GetValue(floorSpot.x, floorSpot.y, floorSpot.z).pathfindingNode.enterableSides.UpdateValue(x, 0, z, true);
                }
            }
        }
    }

    Vector3Int RotateVectors(Vector3Int vectorToTurn, Rotation rotation)
    {
        if (rotation == Rotation.forward)
        {
            return vectorToTurn;
        }
        if (rotation == Rotation.right)
        {
            return new Vector3Int(-vectorToTurn.z, vectorToTurn.y, -vectorToTurn.x);
        }
        if (rotation == Rotation.back)
        {
            return new Vector3Int(-vectorToTurn.x, vectorToTurn.y, -vectorToTurn.z);
        }
        if (rotation == Rotation.left)
        {
            return new Vector3Int(vectorToTurn.z, vectorToTurn.y, vectorToTurn.x);
        }

        return new Vector3Int(0, 0, 0);
    }


    public Vector3 GetWorldPosition(Vector3 coords)
    {
        Vector3 pos = coords * tileSize + origin;
        return pos;
    }

    void SetRotation(BuildingScript.Rotation newRotation)
    {
        currentRotation = newRotation;
        if (currentRotation == BuildingScript.Rotation.right)
        {
            rotationOffset = new Vector3Int(0, 0, 1);
        }
        else if (currentRotation == BuildingScript.Rotation.back)
        {
            rotationOffset = new Vector3Int(1, 0, 1);
        }
        else if (currentRotation == BuildingScript.Rotation.left)
        {
            rotationOffset = new Vector3Int(1, 0, 0);
        }
        else if (currentRotation == BuildingScript.Rotation.forward)
        {
            rotationOffset = new Vector3Int(0, 0, 0);
        }
    }
    void CycleRotation()
    {
        if (currentRotation == Rotation.forward)
        {
            currentRotation = Rotation.right;
            rotationOffset = new Vector3Int(0, 0, 1);
        }
        else if (currentRotation == Rotation.right)
        {
            currentRotation = Rotation.back;
            rotationOffset = new Vector3Int(1, 0, 1);
        }
        else if (currentRotation == Rotation.back)
        {
            currentRotation = Rotation.left;
            rotationOffset = new Vector3Int(1, 0, 0);
        }
        else if (currentRotation == Rotation.left)
        {
            currentRotation = Rotation.forward;
            rotationOffset = new Vector3Int(0, 0, 0);
        }
    }

    public Vector3Int RotToOffset(Rotation Rotation)
    {
        if (Rotation == Rotation.right)
        {
            return new Vector3Int(0, 0, 1);
        }
        else if (Rotation == Rotation.back)
        {
            return new Vector3Int(1, 0, 1);
        }
        else if (Rotation == Rotation.left)
        {
            return new Vector3Int(1, 0, 0);
        }
        else
        {
            return new Vector3Int(0, 0, 0);
        }
    }


    public void SaveToJson()
    {
        Serializable1DArray<SaveGridObject> savedBuildings = new Serializable1DArray<SaveGridObject>(selectedPrefab.savedObjects.Count);

        for (int i = 0; i < selectedPrefab.savedObjects.Count; i++)
        {
            savedBuildings.UpdateValue(i, selectedPrefab.savedObjects[i]);
        }

        string mapData = JsonUtility.ToJson(savedBuildings);
        string filePath = Application.persistentDataPath + "/" + selectedPrefab.jsonName + ".json" ;
        Debug.Log(filePath);
        System.IO.File.WriteAllText(filePath, mapData);
    }

    public void LoadFromJson()
    {
        string filePath = Application.persistentDataPath + "/" + selectedPrefab.jsonName + ".json";
        string mapData = System.IO.File.ReadAllText(filePath);
        Debug.Log(mapData);
        Serializable1DArray<SaveGridObject> savedBuildings = JsonUtility.FromJson<Serializable1DArray<SaveGridObject>>(mapData);
        Debug.Log("loaded");

        for (int i = 0; i < savedBuildings.size; i++)
        {
            SaveGridObject obj = savedBuildings.GetValue(i);
            Vector3Int coord = new Vector3Int(obj.x, obj.y, obj.z);
            Vector3 coordWOffset = new Vector3(obj.x, obj.y, obj.z) + RotToOffset(obj.rot);
            AttemptBuild(coord, coordWOffset, true, helper.IDtoBuildingMap[obj.buildingID], obj.rot);
        }
    }

    public void ClearObjects()
    {
        GameObject toBeDestroyed = new GameObject();

        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            parentTransform.GetChild(i).SetParent(toBeDestroyed.transform);
        }

        DestroyImmediate(toBeDestroyed);
    }


    public void GenerateIslandPerlin()
    {
        Debug.Log("generating");
        int seaLevel = 3;

        float stepH = 0.05f;
        Vector2 offsetH = new Vector2(Random.Range(0, 10000), Random.Range(0, 10000));

        float stepC = 0.075f;
        Vector2 offsetC = new Vector2(Random.Range(0, 10000), Random.Range(0, 10000));

        float stepG = 0.05f;
        Vector2 offsetG = new Vector2(Random.Range(0, 10000), Random.Range(0, 10000));

        Vector3 dimensions = selectedPrefab.gridDimensions;

        int[,] heightMap = new int[(int)dimensions.x, (int)dimensions.z];

        float[,] greeneryMap = new float[(int)dimensions.x, (int)dimensions.z];

        float[,] civilisationMap = new float[(int)dimensions.x, (int)dimensions.z];

        visited = new bool[(int)dimensions.x, (int)dimensions.z];

        villages = new List<Vector2Int>();

        for (int x = 0; x < (int)dimensions.x; x++)
        {
            for (int z = 0; z < (int)dimensions.z; z++)
            {
                heightMap[x, z] = HeightMapCurve(Mathf.PerlinNoise(offsetH.x + x * stepH, offsetH.y + z * stepH));
                civilisationMap[x, z] = Mathf.PerlinNoise(offsetC.x + x * stepC, offsetC.y + z * stepC);
                greeneryMap[x, z] = Mathf.PerlinNoise(offsetG.x + x * stepG, offsetG.y + z * stepG);

                //Debug.Log(offset.x + x * step + "," + offset.y + z * step);
            }
        }

        for (int x = 0; x < (int)dimensions.x; x++)
        {
            for (int z = 0; z < (int)dimensions.z; z++) 
            {
                if (civilisationMap[x, z] > 0.8 && heightMap[x,z] >= 2)
                {
                    int counter = 0;

                    GetVillageSize(x, z, ref civilisationMap, ref counter);

                    if(counter > 5)
                    {
                        villages.Add(new Vector2Int(x, z));
                    }
                }
            }
        }

        
        HashSet<Vector2Int> pathTiles = new HashSet<Vector2Int>();
        if(villages.Count > 1)
        {
            List<Vector2Int> shuffled  = villages.OrderBy(x => Random.value).ToList();

            for(int i  = 0; i < shuffled.Count -1; i++)
            {
                Debug.Log(shuffled[i]);
                pathTiles.UnionWith(CreatePath(ref heightMap, pathTiles, shuffled[i], shuffled[i + 1]));
            }

            pathTiles.UnionWith(CreatePath(ref heightMap, pathTiles, shuffled[shuffled.Count - 1], shuffled[0]));

        }
        

        for (int x = 0; x < (int)dimensions.x; x++)
        {
            for (int z = 0; z < (int)dimensions.z; z++)
            {
                
                int cur = heightMap[x, z];
                int lowest = heightMap[x, z];

                if (x > 0 && heightMap[x-1, z] < lowest)
                {
                    lowest = heightMap[x - 1, z];
                }

                if (z > 0 && heightMap[x, z - 1] < lowest)
                {
                    lowest = heightMap[x, z - 1];
                }

                if (x < (int)dimensions.x -1 && heightMap[x + 1, z] < lowest)
                {
                    lowest = heightMap[x + 1, z];
                }

                if (z < (int)dimensions.z - 1 && heightMap[x, z + 1] < lowest)
                {
                    lowest = heightMap[x, z + 1];
                }

                
                if (pathTiles.Contains(new Vector2Int(x, z)))
                {
                    AttemptBuild(new Vector3Int(x, heightMap[x, z], z), new Vector3(x, heightMap[x, z], z), true, helper.path);

                }
                else if (lowest < 2 && cur <= 2)
                {
                    AttemptBuild(new Vector3Int(x, heightMap[x, z], z), new Vector3(x, heightMap[x, z], z), true, helper.sand);

                }
                else if (cur > 9)
                {
                    AttemptBuild(new Vector3Int(x, heightMap[x, z], z), new Vector3(x, heightMap[x, z], z), true, helper.snow);

                }
                else if (cur > 7)
                {
                    AttemptBuild(new Vector3Int(x, heightMap[x, z], z), new Vector3(x, heightMap[x, z], z), true, helper.rockTile);

                }
                else
                {
                    if (greeneryMap[x, z] < 0.15)
                    {
                        AttemptBuild(new Vector3Int(x, heightMap[x, z], z), new Vector3(x, heightMap[x, z], z), true, helper.dirt);
                    }
                    else if (greeneryMap[x, z] < 0.25)
                    {
                        AttemptBuild(new Vector3Int(x, heightMap[x, z], z), new Vector3(x, heightMap[x, z], z), true, helper.dirtMix);
                    }
                    else
                    {
                        AttemptBuild(new Vector3Int(x, heightMap[x, z], z), new Vector3(x, heightMap[x, z], z), true, helper.grass);
                    }
                }


                if (civilisationMap[x, z] > 0.95 && cur > 2)
                {
                    if (Random.value > 0.5)
                    {
                        AttemptBuild(new Vector3Int(x, heightMap[x, z] + 1, z), new Vector3(x, heightMap[x, z] + 1, z), CheckCanBuild(new Vector3Int(x, heightMap[x, z] + 1, z), 3, 1, 3), helper.fountain);
                    }
                }
                else if (civilisationMap[x, z] > 0.9 && cur > 2)
                {
                    if (Random.value > 0.5)
                    {
                        AttemptBuild(new Vector3Int(x, heightMap[x, z] + 1, z), new Vector3(x, heightMap[x, z] + 1, z), CheckCanBuild(new Vector3Int(x, heightMap[x, z] + 1, z), 2, 2, 2), helper.largeBuilding);
                    }
                }
                else if (civilisationMap[x, z] > 0.8 && cur > 2)
                {
                    if(Random.value > 0.5)
                    {
                        AttemptBuild(new Vector3Int(x, heightMap[x, z] + 1, z), new Vector3(x, heightMap[x, z] + 1, z), CheckCanBuild(new Vector3Int(x, heightMap[x, z] + 1, z), 1, 1, 1), helper.smallBuilding);
                    }
                }
                else if (greeneryMap[x, z] > 0.5 && cur > 2 && !pathTiles.Contains(new Vector2Int(x, z)))
                {
                    AttemptBuild(new Vector3Int(x, heightMap[x, z] + 1, z), new Vector3(x, heightMap[x, z] + 1, z), true, helper.smallTree);
                }

                while (cur - 1 > lowest)
                {
                    AttemptBuild(new Vector3Int(x, cur-1, z), new Vector3(x, cur-1, z), true, helper.rockTile);
                    cur--;
                }
                


            }
        }

        Debug.Log("done");
    }

    void GetVillageSize(int x, int z, ref float[,] civilisationMap, ref int counter)
    {
        if (visited[x, z])
        {
            return;
        }

        counter++;
        visited[x, z] = true;

        if (x < civilisationMap.GetLength(0) -1 && civilisationMap[x + 1, z] > 0.8){
            GetVillageSize(x + 1, z, ref civilisationMap, ref counter); 
        }

        if (x > 0 && civilisationMap[x - 1, z] > 0.8)
        {
            GetVillageSize(x - 1, z, ref civilisationMap, ref counter);
        }

        if (z < civilisationMap.GetLength(0) - 1 && civilisationMap[x, z + 1] > 0.8)
        {
            GetVillageSize(x, z + 1, ref civilisationMap, ref counter);
        }

        if (z > 0 && civilisationMap[x, z - 1] > 0.8)
        {
            GetVillageSize(x, z - 1, ref civilisationMap, ref counter);
        }
    }

    class PathCreationNode
    {
        public PathCreationNode prev;
        public int x;
        public int z;
        public int vertMove;
        public int distTravelled;

        public PathCreationNode(int x, int z, int vertMove, PathCreationNode prev, int distTravelled)
        {
            this.x = x;
            this.z = z;
            this.vertMove = vertMove;
            this.prev = prev;
            this.distTravelled = distTravelled;
        }

    }

    HashSet<Vector2Int> CreatePath(ref int[,]heightMap, HashSet<Vector2Int> existingPaths, Vector2Int start, Vector2Int end)
    {
        bool[,] visited = new bool[heightMap.GetLength(0), heightMap.GetLength(1)];
        PathCreationNode final = null;

        PriorityQueue<PathCreationNode, int> q = new PriorityQueue<PathCreationNode, int>(0);
        q.Insert(new PathCreationNode(start.x, start.y, 0, null, 0), 0);
        
        while (q.Size() > 0)
        {
            PathCreationNode node = q.Pop();

            if(node.x == end.x && node.z == end.y)
            {
                final = node;
                break;
            }         

            if (visited[node.x, node.z] || heightMap[node.x, node.z] < 2)
            {
                continue;
            }
            visited[node.x, node.z] = true;

            if (existingPaths.Contains(new Vector2Int(node.x, node.z)))
            {
                Vector2Int closestToTarget = new Vector2Int(node.x, node.z);
                int dist = Mathf.Abs(node.x - end.x) + Mathf.Abs(node.z - end.y);

                foreach (Vector2Int path in existingPaths)
                {
                    if (Mathf.Abs(path.x - end.x) + Mathf.Abs(path.y - end.y) < dist)
                    {
                        closestToTarget = path;
                        dist = Mathf.Abs(path.x - end.x) + Mathf.Abs(path.y - end.y);
                    }
                }

                PathCreationNode newNode = new PathCreationNode(closestToTarget.x, closestToTarget.y, node.vertMove, node, node.distTravelled);
                q.Insert(newNode, newNode.vertMove * 3 + newNode.distTravelled);
            }

            if (node.x < heightMap.GetLength(0) - 1 && heightMap[node.x + 1, node.z] > 0.8)
            {
                PathCreationNode newNode = new PathCreationNode(node.x + 1, node.z, node.vertMove + Mathf.Abs(heightMap[node.x, node.z] - heightMap[node.x + 1, node.z]), node, node.distTravelled + 1);
                q.Insert(newNode, newNode.vertMove * 3 + newNode.distTravelled);
            }

            if (node.x > 0 && heightMap[node.x - 1, node.z] > 0.8)
            {
                PathCreationNode newNode = new PathCreationNode(node.x - 1, node.z, node.vertMove + Mathf.Abs(heightMap[node.x, node.z] - heightMap[node.x - 1, node.z]), node, node.distTravelled + 1);
                q.Insert(newNode, newNode.vertMove * 3 + newNode.distTravelled);
            }

            if (node.z < heightMap.GetLength(0) - 1 && heightMap[node.x, node.z + 1] > 0.8)
            {
                PathCreationNode newNode = new PathCreationNode(node.x, node.z + 1, node.vertMove + Mathf.Abs(heightMap[node.x, node.z] - heightMap[node.x, node.z + 1]), node, node.distTravelled + 1);
                q.Insert(newNode, newNode.vertMove * 3 + newNode.distTravelled);
            }

            if (node.z > 0 && heightMap[node.x, node.z - 1] > 0.8)
            {
                PathCreationNode newNode = new PathCreationNode(node.x, node.z - 1, node.vertMove + Mathf.Abs(heightMap[node.x, node.z] - heightMap[node.x, node.z - 1]), node, node.distTravelled + 1);
                q.Insert(newNode, newNode.vertMove * 3 + newNode.distTravelled);
            }
        }

        HashSet<Vector2Int> result = new HashSet<Vector2Int>();

        while(final != null)
        {
            result.Add(new Vector2Int(final.x, final.z));
            final = final.prev;
        }

        return result;
    }

    int HeightMapCurve(float raw)
    {
        if(raw < 0.3)
        {
           return Mathf.Max(Mathf.FloorToInt(raw * 10), 0);
        }

        if(raw < 0.4)
        {
            return 3;
        }

        if(raw < 0.5)
        {
            return 4;
        }

        if(raw < 0.6)
        {
            return 5;
        }

        if(raw < 0.65)
        {
            return 6;
        }

        if(raw < 0.7)
        {
            return 7;
        }

        if(raw < 0.8)
        {
            return 8;
        }

        return 9 + Mathf.FloorToInt(Mathf.Min((raw-0.8f) * 70, 11));
    }




    void GenerateIslandWFC()
    {
        List<WFCTile> allTiles = new List<WFCTile>();
        foreach(var so in helper.allWFCTiles)
        {
            allTiles.Add(new WFCTile(so));
        }

        Vector3 dimensions = selectedPrefab.gridDimensions;
        WFCCell[,,] WFCGrid = new WFCCell[(int)dimensions.x, (int)dimensions.y, (int)dimensions.z];
        int[,] topped = new int[(int)dimensions.x, (int)dimensions.z];

        PriorityQueue<Vector3Int, int> pq = new PriorityQueue<Vector3Int, int>(0);

        for (int x = 0; x < WFCGrid.GetLength(0); x++)
        {
            for (int y = 0; y < WFCGrid.GetLength(1) - 1; y++)
            { 
                for (int z = 0; z < WFCGrid.GetLength(2); z++)
                {
                    WFCGrid[x, y, z] = new WFCCell(allTiles);
                    topped[x, z] = 100;
                    pq.Insert(new Vector3Int(x, y, z), WFCGrid[x, y, z].possible);
                    Debug.Log(WFCGrid[x, y, z].possible);
                }
            }
        }

        pq.Insert(new Vector3Int(0, 0, 0), 0);


        while (pq.Size() > 0)
        {
            Vector3Int index = pq.Pop();

           

            WFCCell cur = WFCGrid[index.x, index.y, index.z];

            Debug.Log(index + " - " + cur.possible);

            if (cur.collapsed || index.y > topped[index.x, index.z])
            {
                continue;
            }

            WFCTile tile = cur.CollapseCell();

            if(tile == null) 
            {
                AttemptBuild(index, index, true, helper.errorTile);

            }
            if(tile.SO == helper.empty || tile.SO == helper.civEmpty)
            {
                topped[index.x, index.z] = index.y;
            }

            if (tile != null)
            {
                AttemptBuild(index, index, true, tile.SO.buildingObject);
                //Debug.Log(tile.SO.name + " ");
                    
                if (index.x + 1 < WFCGrid.GetLength(0) && !WFCGrid[index.x + 1, index.y, index.z].collapsed)
                {
                    WFCGrid[index.x + 1, index.y, index.z].UpdatePossible(Direction.left, tile.SO);


                    pq.Insert(new Vector3Int(index.x + 1, index.y, index.z), WFCGrid[index.x + 1, index.y, index.z].possible);
                     

                }

                if (index.x > 0 && !WFCGrid[index.x - 1, index.y, index.z].collapsed)
                {
                    WFCGrid[index.x - 1, index.y, index.z].UpdatePossible(Direction.right, tile.SO);

                    pq.Insert(new Vector3Int(index.x - 1, index.y, index.z), WFCGrid[index.x - 1, index.y, index.z].possible);
                    
                        
                }


                if (index.y + 1 < WFCGrid.GetLength(1) - 1 && !WFCGrid[index.x, index.y + 1, index.z].collapsed)
                {
                    WFCGrid[index.x, index.y + 1, index.z].UpdatePossible(Direction.down, tile.SO);
                    pq.Insert(new Vector3Int(index.x, index.y + 1, index.z), WFCGrid[index.x, index.y + 1, index.z].possible);

                }


                if (index.y > 0 && !WFCGrid[index.x, index.y - 1, index.z].collapsed)
                {
                    WFCGrid[index.x, index.y - 1, index.z].UpdatePossible(Direction.up, tile.SO);

                    pq.Insert(new Vector3Int(index.x, index.y - 1, index.z), WFCGrid[index.x, index.y - 1, index.z].possible);
                    
                }



                if (index.z + 1 < WFCGrid.GetLength(2) && !WFCGrid[index.x, index.y, index.z + 1].collapsed)
                {
                    WFCGrid[index.x, index.y, index.z + 1].UpdatePossible(Direction.back, tile.SO);

                    pq.Insert(new Vector3Int(index.x, index.y, index.z + 1), WFCGrid[index.x, index.y, index.z + 1].possible);
                    
                }

                if (index.z > 0 && !WFCGrid[index.x, index.y, index.z - 1].collapsed)
                {
                    WFCGrid[index.x, index.y, index.z - 1].UpdatePossible(Direction.front, tile.SO);

                    pq.Insert(new Vector3Int(index.x, index.y, index.z - 1), WFCGrid[index.x, index.y, index.z - 1].possible);
                }

                    
            }
        }
        
        
       
    }

}
