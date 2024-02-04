using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

public class WorldBuilderEditor : EditorWindow
{
    public SavedWorldMapSO map;
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

    Serializable3DArray<GridObject> grid;

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
            map.Reset();
        }

        if (GUILayout.Button("Save"))
        {
            SaveToJson();
        }

        if (GUILayout.Button("Load"))
        {
            LoadFromJson();
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
        EditorGUILayout.LabelField("Selected Map", GUILayout.Width(100));
        map = (SavedWorldMapSO)EditorGUILayout.ObjectField(map, typeof(SavedWorldMapSO), false, GUILayout.Width(200));
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

        EditorGUILayout.LabelField("Placed blocks : " + map.savedPlaceObjects.Count.ToString());


       

        this.Repaint();
    }

    public void OnSceneGUI(SceneView sceneView)
    {
        if (currentRotation == BuildingScript.Rotation.forward) { tickers = new Vector3Int(1, 1, 1); }
        else if (currentRotation == BuildingScript.Rotation.right) { tickers = new Vector3Int(1, 1, -1); }
        else if (currentRotation == BuildingScript.Rotation.back) { tickers = new Vector3Int(-1, 1, -1); }
        else if (currentRotation == BuildingScript.Rotation.left) { tickers = new Vector3Int(-1, 1, 1); }

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        int layerMask = 1 << 8;
        RaycastHit hit;
        bool canBuild = false;

        Vector3Int checkCoordsVisual = Vector3Int.zero;
        Vector3Int checkCoordsFuntional = Vector3Int.zero;

        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            Vector3Int normalCheck = GetXYZ(hit.normal * tileSize * 0.9f);

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


            checkCoordsVisual = GetXYZ(hit.point + hit.normal * 0.9f * tileSize) + normalOffset + resizedRotationalOffset;
            checkCoordsFuntional = checkCoordsVisual - rotationOffset;


            previewObject.transform.position = checkCoordsVisual;

            canBuild = CheckCanBuild(checkCoordsFuntional, selectedObjectSO.x, selectedObjectSO.y, selectedObjectSO.z);

        }
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
            if(Event.current.button == 0) {
                if (currentMode == mode.placeMode)
                {
                    AttemptBuild(checkCoordsFuntional, canBuild, selectedObjectSO);
                }
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

    
    void AttemptBuild(Vector3Int checkCoord, bool canBuild, BuildingObjectSO toBuild)
    {
        /*
        if (!canBuild)
        {
            return;
        }
        SavedPlaceObject obj = new SavedPlaceObject(selectedObjectSO, currentRotation, checkCoord);
        //Instantiate(toBuild.prefab, GetWorldPosition(checkCoord + rotationOffset), Quaternion.Euler(0, (int)currentRotation * 90f, 0));
        
        GameObject cur = (GameObject)PrefabUtility.InstantiatePrefab(toBuild.prefab);
        
        cur.transform.position = GetWorldPosition(checkCoord + rotationOffset);
        cur.transform.rotation = Quaternion.Euler(0, (int)currentRotation * 90f, 0);

 

        
        for (int x = 0; x < selectedObjectSO.x; x++)
        {
            for (int y = 0; y < selectedObjectSO.y; y++)
            {
                for (int z = 0; z < selectedObjectSO.z; z++)
                {
                    Vector3Int currentCheckCoords = new Vector3Int(checkCoord.x + x * tickers.x, checkCoord.y + y * tickers.y, checkCoord.z + z * tickers.z);
                    AddCoord(currentCheckCoords);
                }
            }
        }
        */

      
    }

    
    bool CheckCanBuild(Vector3Int start, int x, int y, int z)
    {
        for (int Xcheck = 0; Xcheck < x; Xcheck++)
        {
            for (int Ycheck = 0; Ycheck < y; Ycheck++)
            {
                for (int Zcheck = 0; Zcheck < z; Zcheck++)
                {
                    if (map.occupied.Contains(new Vector3Int(start.x + Xcheck * tickers.x, start.y + Ycheck * tickers.y, start.z + Zcheck * tickers.z)) == true)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
    

    public void AddCoord(Vector3Int coord)
    {
        map.occupied.Add(coord);
        if (map.gridDimensions.x < coord.x)
        {
            map.gridDimensions.x = coord.x;
        }
        if (map.gridDimensions.y < coord.y)
        {
            map.gridDimensions.y = coord.y;
        }
        if (map.gridDimensions.z < coord.z)
        {
            map.gridDimensions.z = coord.z;
        }

        if (coord.x < 0)
        {
            map.gridDimensions.x -= coord.x;
            //origin += new Vector3(-coord.x, 0, 0) * tileSize;
            foreach(SavedPlaceObject obj in map.savedPlaceObjects)
            {
                obj.pos += new Vector3Int(-coord.x, 0, 0);
            }
        }
        if (coord.y < 0)
        {
            map.gridDimensions.y -= coord.y;
            //origin += new Vector3(0, -coord.y, 0) * tileSize;
            foreach (SavedPlaceObject obj in map.savedPlaceObjects)
            {
                obj.pos += new Vector3Int(0, -coord.y, 0);
            }
        }
        if (coord.z < 0)
        {
            map.gridDimensions.z -= coord.z;
            //origin += new Vector3(0, 0, -coord.z) * tileSize;
            foreach (SavedPlaceObject obj in map.savedPlaceObjects)
            {
                obj.pos += new Vector3Int(0, 0, -coord.z);
            }
        }
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

    void LoadFromJson()
    {
        
        string filePath = Application.persistentDataPath + "/MapData.json";
        string mapData = System.IO.File.ReadAllText(filePath);
        Debug.Log(mapData);
        selectedObjectSO = JsonUtility.FromJson<BuildingObjectSO>(mapData);
        Debug.Log("loaded");

    }

    void SaveToJson()
    {   
        string mapData = JsonUtility.ToJson(selectedObjectSO);
        Debug.Log(selectedObjectSO.buildingName);
        string filePath = Application.persistentDataPath + "/MapData.json";
        Debug.Log(filePath);
        System.IO.File.WriteAllText(filePath, mapData);
        
    }
}
