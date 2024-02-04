using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using System.IO;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.UIElements.Experimental;
using System.Linq;



public class BuildingScript : MonoBehaviour
{
    int gridSize = 20;
    float tileSize = 1;
    Vector3 origin = new Vector3(0, 0, 0);
    [SerializeField] Transform boatCentre;
    public GridManager gridManager;

    [SerializeField] Camera cam;

    [SerializeField] BuildingObjectSO[] buildingObjectSOs;

    bool previewIsRed;
    [SerializeField] Material previewRed;
    [SerializeField] Material previewBlue;
    BuildingObjectSO selectedObjectSO;
    [SerializeField] GameObject previewObject;
    [SerializeField] GameObject gridNavPreviewObject;
    Vector3 previewObjectTargetCoords;
    Vector3Int previewObjectPrevCoords;
    Vector3 previewObjectStartCoords;
    Vector3 previewObjectOffset;
    float previewObjectLerpTimer;
    float gridRotation;
    [SerializeField] PlayerResources playerResources;

    public enum Rotation{
        forward = 0,
        left = 3,
        right = 1,
        back = 2
        
    }

    Rotation currentRotation = Rotation.forward;
    Vector3Int rotationOffset = new Vector3Int(0, 0, 0);
    Vector3Int tickers;
    int HighestPoint = 1;
    GameObject[,,] checkGrid; 
    int storyLevel = 0;
    [SerializeField] BuildingObjectSO pillarSO;
    [SerializeField] GameObject gridIndicator;
    [SerializeField] CanvasManager canvasManager;
    [SerializeField] PeopleTaskManager peopleManager;


    //Building Helper Variables;
    [SerializeField] GameObject stairsBuildPreview;
    [SerializeField]BuildingObjectSO StairObject;
    bool stairBuildingStarted;
    Vector3Int stairStartCoord; 
    List<GameObject> stairBuildPreviewSections = new List<GameObject>();
    XZ stairLockDirection = XZ.None;

    public int test; 

    List<List<Vector3>> buildCoords = new List<List<Vector3>>(); //buildCoord[i][0] = functional grid placement ...  buildCoord[i][1] = visual placement point

    enum XZ
    {
        None,
        X,
        Z
    }




    // Start is called before the first frame update
    void Start()
    {
        //checkGrid = new GameObject[gridSize, gridSize, gridSize];
        gridManager.Reset(gridSize, gridSize, gridSize);
        origin = boatCentre.position - Quaternion.AngleAxis(gridRotation, Vector3.up) * (new Vector3(gridSize, 0, gridSize) * tileSize/2);
        
        /*
        for(int x = 0; x < gridManager.grid.x; x++){
            for(int z = 0; z < gridManager.grid.z; z++){
                for(int y = 0; y < gridManager.grid.y; y++){
                    //checkGrid[x, y, z] = Instantiate(gridNavPreviewObject, GetWorldPosition(new Vector3(x, y, z)), Quaternion.identity); 
                }
            }
        }
        */

    }

    public Vector3 GetWorldPosition(int x, int y, int z){
        return new Vector3(x, y, z) * tileSize + origin;
    }

    public Vector3 GetWorldPosition(Vector3 coords){
        Vector3 pos = Quaternion.AngleAxis(gridRotation, Vector3.up) * coords;
        pos = pos * tileSize + origin;
        return pos;
    }

    public Vector3 GetWorldPositionCentre(Vector3 coords){
        Vector3 pos = Quaternion.AngleAxis(gridRotation, Vector3.up) * coords;
        pos = pos * tileSize + origin + Vector3.one * tileSize/2;
        return pos;
    }


    public Vector3 GetRelativeWorldPosition(Vector3 coords){
        return new Vector3(coords.x, coords.y, coords.z) * tileSize + origin;
    }

    public Vector3Int GetXYZ(Vector3 worldPos){
        Vector3 recentered = worldPos - origin ;
        recentered = Quaternion.AngleAxis(-gridRotation, Vector3.up) * recentered - (Vector3.one * tileSize/2);
        return new Vector3Int(Mathf.RoundToInt(recentered.x/tileSize), Mathf.RoundToInt(recentered.y/tileSize), Mathf.RoundToInt(recentered.z/tileSize));
    }

    public Vector3Int GetRelativeXYZ(Vector3 Pos){
        Vector3 recentered = Pos;
        recentered = Quaternion.AngleAxis(-gridRotation, Vector3.up) * recentered;
        return new Vector3Int(Mathf.RoundToInt(recentered.x/tileSize), Mathf.Min(Mathf.RoundToInt(recentered.y/tileSize), 0), Mathf.RoundToInt(recentered.z/tileSize));
    }
    
    public KeyValuePair<float, int> GetHeight(int story){
        int newStory = Mathf.Clamp(story, 1, HighestPoint+10);
        storyLevel = newStory;
        float height = origin.y + story * tileSize - tileSize/2;
        return new KeyValuePair<float, int>(height, newStory);
    }

    void ToggleGridIndicator(){
        gridIndicator.SetActive(!gridIndicator.activeSelf);
    }


    void Update()
    {
        //if (Application.IsPlaying(gameObject)) { return; }

        if (canvasManager.currentState != CanvasManager.CanvasState.BuildingMode){
            return;
        }

        if(Input.GetKeyDown(KeyCode.D)){
            ToggleGridIndicator();
        }
        
        /*
        for(int x = 0; x < grid.GetLength(0); x++){
            for(int z = 0; z < grid.GetLength(1); z++){
                for(int y = 0; y < grid.GetLength(2); y++){
                    if(pathfinding.Nodes[x, y, z].hasFloor){
                        checkGrid[x, y, z].SetActive(true);
                        for(int xdir = 0; xdir < 3; xdir++){
                            for(int zdir = 0; zdir < 3; zdir++){
                                for(int ydir = 0; ydir < 3; ydir++){
                                    if(x + xdir - 1 < pathfinding.Nodes.GetLength(0) -1 && y + ydir - 1 < pathfinding.Nodes.GetLength(1) - 1 &&  z + zdir - 1 < pathfinding.Nodes.GetLength(2) - 1 && x + xdir - 1 > 0 && y + ydir - 1 > 0 &&  z + zdir - 1 > 0){
                                        if(pathfinding.Nodes[x, y, z].enterableSides[xdir, ydir, zdir] && pathfinding.Nodes[x + xdir - 1, y + ydir - 1, z + zdir - 1].enterableSides[2-xdir, 2-ydir, 2-zdir]){
                                            checkGrid[x, y, z].transform.GetChild(ydir).GetChild(xdir).GetChild(zdir).gameObject.SetActive(true);
                                        }     
                                        else{
                                            checkGrid[x, y, z].transform.GetChild(ydir).GetChild(xdir).GetChild(zdir).gameObject.SetActive(false);
                                        }   
                                    }
                                    else{
                                        checkGrid[x, y, z].transform.GetChild(ydir).GetChild(xdir).GetChild(zdir).gameObject.SetActive(false);
                                    }
                                }   
                            }
                        }
                    }
                    else{
                        checkGrid[x, y, z].SetActive(false);
                    }
                }
            }
        }
        */
        
        
        gridRotation = Vector3.SignedAngle(Vector3.forward, boatCentre.forward, Vector3.up);
        
        origin = boatCentre.position - Quaternion.AngleAxis(gridRotation, Vector3.up) * (new Vector3(gridSize, 0, gridSize) * tileSize/2) ;
        
        if(currentRotation == Rotation.forward) {tickers = new Vector3Int(1, 1, 1);}
        else if(currentRotation == Rotation.right) {tickers = new Vector3Int(1, 1, -1);}
        else if(currentRotation == Rotation.back) {tickers = new Vector3Int(-1, 1, -1);}
        else if(currentRotation == Rotation.left) {tickers = new Vector3Int(-1, 1, 1);}

        if(Input.GetKeyDown(KeyCode.R)){
            CycleRotation();
        }

        if(selectedObjectSO == null){ 
            if(previewObject != null){
                Destroy(previewObject);
            }
            return;
        }
        
        RaycastHit hit; 


        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 8;

        if(Physics.Raycast(ray, out hit, 100f, layerMask)){
           
            Vector3Int normalCheck = GetRelativeXYZ(hit.normal * tileSize);
            
            //if(normalCheck.x > normalCheck.y && normalCheck.x > normalCheck.z){
                //normalCheck
            //}
            
            Vector3Int normalOffset = new Vector3Int(Mathf.Min(normalCheck.x * (selectedObjectSO.x - 1), 0), normalCheck.y * (selectedObjectSO.y - 1), Mathf.Min(normalCheck.z * (selectedObjectSO.z - 1), 0));
            
            Vector3Int resizedRotationalOffset = rotationOffset;

            if(normalCheck.x < 0){
                resizedRotationalOffset.x *= -(normalCheck.x * selectedObjectSO.x);
            }
            else if(normalCheck.z < 0){
                resizedRotationalOffset.z *= -(normalCheck.z * selectedObjectSO.z);
            }
            else if(normalCheck.x > 0){
                resizedRotationalOffset.x *= (normalCheck.x * selectedObjectSO.x);
            }
            else if(normalCheck.z > 0){
                resizedRotationalOffset.z *= (normalCheck.z * selectedObjectSO.z);
            }

            //Debug.Log("normal:" + normalOffset+ "      Rotation:" + resizedRotationalOffset);


            Vector3Int checkCoordsVisual = GetXYZ(hit.point + hit.normal*0.9f*tileSize) + normalOffset + resizedRotationalOffset;
            Vector3Int checkCoordsFuntional = checkCoordsVisual - rotationOffset;
            
            //Debug.Log(checkCoords);
            //previewObjectTargetCoords = worldPos - origin;
            //previewObjectStartCoords = previewObjectOffset;

            if(previewObjectPrevCoords != checkCoordsVisual){
                previewObjectLerpTimer = 0;
                previewObjectTargetCoords = new Vector3(checkCoordsVisual.x , checkCoordsVisual.y, checkCoordsVisual.z) * tileSize;
                previewObjectStartCoords = previewObjectOffset;
                previewObjectPrevCoords = checkCoordsVisual;
            }

            previewObjectLerpTimer = Mathf.Clamp(previewObjectLerpTimer + Time.deltaTime, 0, 0.2f);
            previewObjectOffset = Vector3.Lerp(previewObjectStartCoords, previewObjectTargetCoords, previewObjectLerpTimer/0.2f);
            previewObject.transform.rotation = Quaternion.Euler(0, gridRotation + (int)currentRotation * 90f, 0);
            previewObject.transform.localPosition = -(new Vector3(gridSize, 0, gridSize) * tileSize/2) + previewObjectOffset;

            bool canBuild = CheckCanBuild(checkCoordsFuntional, selectedObjectSO.x, selectedObjectSO.y, selectedObjectSO.z);

            if(canBuild && previewIsRed){
                previewObject.transform.GetChild(0).GetComponent<Renderer>().material = previewBlue;
                previewIsRed = false;
            }
            else if(!canBuild && !previewIsRed){
                previewObject.transform.GetChild(0).GetComponent<Renderer>().material = previewRed;
                previewIsRed = true;
            }

            if (stairBuildingStarted)
            {
                //scrapped stairway drag indicator
                
                previewObject.SetActive(false);
                int xDiff = checkCoordsVisual.x - stairStartCoord.x;
                int zDiff = checkCoordsVisual.z - stairStartCoord.z;
                
                if(Mathf.Abs(xDiff) > Mathf.Abs(zDiff)) 
                {
                    stairLockDirection = XZ.X;
                }
                else if(Mathf.Abs(zDiff) > 0)
                {
                    
                    stairLockDirection = XZ.Z;
                }

                Vector3Int newCheckCoord = stairStartCoord;
                if(stairLockDirection == XZ.X)
                {
                    newCheckCoord = new Vector3Int(checkCoordsFuntional.x, stairStartCoord.y, stairStartCoord.z);
                }
                else if (stairLockDirection == XZ.Z)
                {
                    newCheckCoord = new Vector3Int(stairStartCoord.x, stairStartCoord.y, checkCoordsFuntional.z);
                }

                PreviewStairsHelper(stairStartCoord, newCheckCoord, stairLockDirection);

                if (Input.GetKey(KeyCode.Mouse0) == false)
                {
                    stairBuildingStarted = false;
                    BuildGroup();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0)) {
                if (selectedObjectSO == StairObject)
                {
                    stairBuildingStarted = true;
                    stairStartCoord = checkCoordsFuntional;
                    stairLockDirection = XZ.None;
                }
                else
                {
                    AttemptBuild(checkCoordsFuntional, checkCoordsVisual, canBuild);
                }
            }
        }            
        else{
            previewObject.transform.position = ray.GetPoint(20);
            if(!previewIsRed){
                previewObject.transform.GetChild(0).GetComponent<Renderer>().material = previewRed;
                previewIsRed = true;
            }
        }
        //previewObject.transform.position;
        //Debug.Log(selectedObjectSO.buildingResources.Count);
        //foreach(KeyValuePair<ResourceSO, int> resource in selectedObjectSO.buildingResources){
            //Debug.Log("pog");
            //Debug.Log(resource.Key + " " + resource.Value);
        //}
    }

    bool CheckCanBuild(Vector3Int start, int x, int y, int z){
        if(CheckEnoughResources() == false){
            return false;
        }
        if(Mathf.Max(start.x, start.x + x - 1) > gridManager.grid.x -1 || Mathf.Max(start.z, start.z + z - 1) > gridManager.grid.z - 1|| Mathf.Max(start.y, start.y + y - 1) > gridManager.grid.y - 1|| Mathf.Min(start.x, start.x + x + 1) < 0 || Mathf.Min(start.z, start.z + z + 1) < 0 || Mathf.Min(start.y, start.y + y + 1) < 0){
            return false;
        }
        for(int Xcheck = 0; Xcheck < x; Xcheck++){
            for(int Ycheck = 0; Ycheck < y; Ycheck++){
                for(int Zcheck = 0; Zcheck < z; Zcheck++) {
                    if(gridManager.grid.GetValue(start.x + Xcheck * tickers.x, start.y + Ycheck * tickers.y, start.z + Zcheck * tickers.z).occupied == true){
                        return false;
                    }
                }
            }
        }

        return true;
    }

    bool CheckEnoughResources(){
        foreach(KeyValuePair<ResourceSO, int> resource in selectedObjectSO.buildingResources){
            if(resource.Value > playerResources.GetResourceAmount(resource.Key)){
                return false;
            }
        }

        return true;
    }

    public void SelectBuildingObject(BuildingObjectSO building){
        selectedObjectSO = building;
        Destroy(previewObject);
        previewObject = Instantiate(selectedObjectSO.preview);
        previewObject.transform.parent = boatCentre;
    }

    void PreviewStairsHelper(Vector3Int startCoord, Vector3Int endCoord, XZ dir)
    {
        stairsBuildPreview.SetActive(true);
        ClearGameObjectList(stairBuildPreviewSections);
        buildCoords.Clear();
           
        if(dir == XZ.X)
        {
            if(endCoord.x - startCoord.x > 0)
            {
                SetRotation(Rotation.forward);
                for(int i = 0; i < endCoord.x - startCoord.x; i++)
                {
                    stairBuildPreviewSections.Add(Instantiate(selectedObjectSO.preview, GetWorldPosition(new Vector3(startCoord.x+i, startCoord.y+i, startCoord.z)+rotationOffset), Quaternion.Euler(0, gridRotation + (int)currentRotation * 90f, 0)));
                    Vector3 pos = new Vector3(startCoord.x + i, startCoord.y + i, startCoord.z);
                    buildCoords.Add(new List<Vector3>(){ pos, pos+rotationOffset});
                }
            }
            else
            {
                SetRotation(Rotation.back);
                for (int i = 0; i > endCoord.x - startCoord.x; i--)
                {
                    stairBuildPreviewSections.Add(Instantiate(selectedObjectSO.preview, GetWorldPosition(new Vector3(startCoord.x + i, startCoord.y - i, startCoord.z) + rotationOffset), Quaternion.Euler(0, gridRotation + (int)currentRotation * 90f, 0)));
                    Vector3 pos = new Vector3(startCoord.x + i, startCoord.y - i, startCoord.z);
                    buildCoords.Add(new List<Vector3>() { pos, pos + rotationOffset });
                }
            }
        }

        if (dir == XZ.Z)
        {
            if (endCoord.z - startCoord.z > 0)
            {
                SetRotation(Rotation.left);
                for (int i = 0; i < endCoord.z - startCoord.z; i++)
                {
                    stairBuildPreviewSections.Add(Instantiate(selectedObjectSO.preview, GetWorldPosition(new Vector3(startCoord.x, startCoord.y + i, startCoord.z + i)+rotationOffset), Quaternion.Euler(0, gridRotation + (int)currentRotation * 90f, 0)));
                    Vector3 pos = new Vector3(startCoord.x, startCoord.y + i, startCoord.z + i);
                    buildCoords.Add(new List<Vector3>() { pos, pos + rotationOffset });
                }
            }
            else
            {
                SetRotation(Rotation.right);
                for (int i = 0; i > endCoord.z - startCoord.z; i--)
                {
                    stairBuildPreviewSections.Add(Instantiate(selectedObjectSO.preview, GetWorldPosition(new Vector3(startCoord.x, startCoord.y - i, startCoord.z + i) + rotationOffset), Quaternion.Euler(0, gridRotation + (int)currentRotation * 90f, 0)));
                    Vector3 pos = new Vector3(startCoord.x, startCoord.y - i, startCoord.z + i);
                    buildCoords.Add(new List<Vector3>() { pos, pos + rotationOffset});
                }
            }
        }
    }
    void BuildGroup()
    {
        foreach (List<Vector3> preview in buildCoords)
        {
            bool canBuild = CheckCanBuild(new Vector3Int((int)preview[0].x, (int)preview[0].y, (int)preview[0].z), selectedObjectSO.x, selectedObjectSO.y, selectedObjectSO.z);
            AttemptBuild(new Vector3Int((int)preview[0].x, (int)preview[0].y, (int)preview[0].z), preview[1], canBuild);
        }
    }

    void DragBuildHelper(Vector3Int startCoord, Vector3Int endCoord)
    {

    }

    void ClearGameObjectList(List<GameObject> list) 
    {
        if(list == null)
        {
            return;
        }
        if(list.Count == 0)
        {
            return;
        }
        foreach (GameObject gameObject in list)
        {
            Destroy(gameObject);
        }
        list.Clear();
    }

    public void AttemptBuild(Vector3Int checkCoord, Vector3 placeCoord, bool canBuild){
        if(!canBuild){
            return;
        }
        foreach(KeyValuePair<ResourceSO, int> resource in selectedObjectSO.buildingResources){
            playerResources.ChangeResourceAmount(resource.Key, -resource.Value);
        }

        GameObject buildScripts = Instantiate(selectedObjectSO.scriptInstancePrefab);
        gridManager.buildingScripts.Add(buildScripts);

        for (int x = 0; x < selectedObjectSO.x; x++){
            for(int y = 0; y < selectedObjectSO.y; y++){
                for(int z = 0; z < selectedObjectSO.z; z++) {

                    Vector3Int currentCheckCoords = new Vector3Int(checkCoord.x + x * tickers.x, checkCoord.y + y * tickers.y, checkCoord.z + z * tickers.z);
                    gridManager.grid.GetValue(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z).occupied = true;
                    gridManager.grid.GetValue(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z).section = selectedObjectSO.sections.GetValue(x, y, z);
                    gridManager.grid.GetValue(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z).section.masterScripts = buildScripts;

                    GameObject built = Instantiate(selectedObjectSO.sections.GetValue(x, y, z).prefab, GetWorldPosition(placeCoord), Quaternion.Euler(0, gridRotation + (int)currentRotation * 90f, 0));
                    built.transform.parent = boatCentre;


                    AddOccupiedtoPathfindingNode(currentCheckCoords, selectedObjectSO.sections.GetValue(x, y, z).walkableDirs);


                    if (y == 0){
                        BuildPillar(currentCheckCoords.x, currentCheckCoords.y - 1, currentCheckCoords.z);
                    }

                    if(y == selectedObjectSO.y - 1 && selectedObjectSO.topWalkable){
                        AddFloorToPathfindNode(new Vector3Int(currentCheckCoords.x, currentCheckCoords.y + 1, currentCheckCoords.z));
                    }

                    if(selectedObjectSO.walkableLevels.Contains(y)){
                        AddFloorToPathfindNode(new Vector3Int(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z));
                    }

                    if(checkCoord.y + y > HighestPoint){
                        HighestPoint = checkCoord.y + y;
                    }
                }
            }
        }

        
        

        
        if(selectedObjectSO.buildingName == "Fishing Shack")
        {
            FishingSpotScript script = buildScripts.GetComponent<FishingSpotScript>();
            peopleManager.allFishingSpots.Add(script);

            buildScripts.GetComponent<BuildingTaskInfo>().gridPos = new Vector3Int((int)placeCoord.x, (int)placeCoord.y, (int)placeCoord.z);
        }
        else if(selectedObjectSO.buildingName == "Storage Space")
        {
            StorageScript script = buildScripts.GetComponent<StorageScript>();
            
            peopleManager.allStorageSpots.Add(new Vector3Int((int)placeCoord.x, (int)placeCoord.y, (int)placeCoord.z), script);


            buildScripts.GetComponent<BuildingTaskInfo>().gridPos = new Vector3Int((int)placeCoord.x, (int)placeCoord.y, (int)placeCoord.z);
        }
        
    }

    
    public void AttemptBuild(Vector3Int checkCoord, Vector3 placeCoord, bool canBuild, BuildingObjectSO toBuild)
    {
        /*
        if (!canBuild)
        {
            return;
        }
        foreach (KeyValuePair<ResourceSO, int> resource in toBuild.buildingResources)
        {
            playerResources.ChangeResourceAmount(resource.Key, -resource.Value);
        }

        //set entryways
        Dictionary<Vector3Int, Vector3Int[]> doors = new Dictionary<Vector3Int, Vector3Int[]>();
        foreach (EntryWay entryWay in toBuild.entryWays)
        {
            doors.Add(entryWay.position, entryWay.directions);
        }

        for (int x = 0; x < toBuild.x; x++)
        {
            for (int y = 0; y < toBuild.y; y++)
            {
                for (int z = 0; z < toBuild.z; z++)
                {
                    Vector3Int currentCheckCoords = new Vector3Int(checkCoord.x + x * tickers.x, checkCoord.y + y * tickers.y, checkCoord.z + z * tickers.z);

                    gridManager.grid.GetValue(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z).occupied = true;
                    gridManager.grid.GetValue(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z).section = selectedObjectSO.sections.GetValue(x, y, z);
                    gridManager.grid.GetValue(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z).section.wholeBuild = selectedObjectSO;


                    if (doors.ContainsKey(new Vector3Int(x, y, z)))
                    {
                        AddOccupiedtoPathfindingNode(currentCheckCoords, doors[new Vector3Int(x, y, z)]);
                    }
                    else
                    {
                        AddOccupiedtoPathfindingNode(currentCheckCoords);
                    }

                    if (y == 0)
                    {
                        BuildPillar(currentCheckCoords.x, currentCheckCoords.y - 1, currentCheckCoords.z);
                    }

                    if (y == toBuild.y - 1 && toBuild.topWalkable)
                    {
                        AddFloorToPathfindNode(new Vector3Int(currentCheckCoords.x, currentCheckCoords.y + 1, currentCheckCoords.z));
                    }

                    if (toBuild.walkableLevels.Contains(y))
                    {
                        AddFloorToPathfindNode(new Vector3Int(currentCheckCoords.x, currentCheckCoords.y, currentCheckCoords.z));
                    }

                    if (checkCoord.y + y > HighestPoint)
                    {
                        HighestPoint = checkCoord.y + y;
                    }
                }
            }
        }
    



        GameObject built = Instantiate(toBuild.prefab, GetWorldPosition(placeCoord), Quaternion.Euler(0, gridRotation + (int)currentRotation * 90f, 0));
        built.transform.parent = boatCentre;

        if (toBuild.buildingName == "Fishing Shack")
        {
            FishingSpotScript script = built.GetComponent<FishingSpotScript>();
            peopleManager.allFishingSpots.Add(script);

            built.GetComponent<BuildingTaskInfo>().gridPos = new Vector3Int((int)placeCoord.x, (int)placeCoord.y, (int)placeCoord.z);
        }
        else if (toBuild.buildingName == "Storage Space")
        {
            StorageScript script = built.GetComponent<StorageScript>();

            peopleManager.allStorageSpots.Add(new Vector3Int((int)placeCoord.x, (int)placeCoord.y, (int)placeCoord.z), script);


            built.GetComponent<BuildingTaskInfo>().gridPos = new Vector3Int((int)placeCoord.x, (int)placeCoord.y, (int)placeCoord.z);
        }
        */
    }
    
    void BuildPillar(int x, int y, int z){
        if(y < 0){
            return;
        }
        for(int checkX = -2; checkX < 3; checkX++){
            for(int checkZ = -2; checkZ < 3; checkZ++) {
                if(checkX + x > gridManager.grid.x -1 || checkZ + z > gridManager.grid.z - 1|| checkX + x < 0 || checkZ + z < 0){
                        continue;
                }

                if(gridManager.grid.GetValue(checkX + x, y, checkZ + z).pillared){
                    //check connected
                    return;
                }
            }
        }
        while(true){
            if(y < 0){
                return;
            }
            if(gridManager.grid.GetValue(x, y, z).occupied){
                return;
            }
            
            gridManager.grid.GetValue(x, y, z).pillared = true;
            
            GameObject built = Instantiate(pillarSO.prefab, GetWorldPosition(new Vector3(x, y, z)), Quaternion.identity);
            built.transform.parent = boatCentre;
            y--;
        }

    }

    void AddOccupiedtoPathfindingNode(Vector3Int occupiedSpot, Vector3Int[] walkableDirs){
        Vector3Int[] rotatedWalkableDirs = new Vector3Int[walkableDirs.Length]; 
        gridManager.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.hasFloor = true;

        for (int i = 0; i < walkableDirs.Length; i++){
            rotatedWalkableDirs[i] = RotateVectors(walkableDirs[i], currentRotation);
        }        
        
        foreach (Vector3Int coord in rotatedWalkableDirs){
            //Debug.Log("Walkable" + coord + currentRotation.ToString());
        }

        for(int x = 0; x < 3; x++){
            for(int y = 0; y < 3; y++){
                for(int z = 0; z < 3; z++) {
                    if(rotatedWalkableDirs.Contains(new Vector3Int(x - 1, y - 1, z - 1))){
                        gridManager.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.enterableSides[x, y, z] = true;
                        gridManager.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.walls[x, y, z] = false;
                    }
                    else{
                        gridManager.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.enterableSides[x, y, z] = false;
                        gridManager.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.walls[x, y, z] = true;
                    }
                }
            }
        }
        //UpdateSuroundingPathfindingNodes(occupiedSpot);
    }

    void AddOccupiedtoPathfindingNode(Vector3Int occupiedSpot)
    {
        gridManager.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.hasFloor = true;


        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    gridManager.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.enterableSides[x, y, z] = false;
                    gridManager.grid.GetValue(occupiedSpot.x, occupiedSpot.y, occupiedSpot.z).pathfindingNode.walls[x, y, z] = true;
                }
            }
        }
        //UpdateSuroundingPathfindingNodes(occupiedSpot);
    }

    Vector3Int RotateVectors(Vector3Int vectorToTurn, Rotation rotation){
        if(rotation == Rotation.forward){
            return vectorToTurn;
        }
        if(rotation == Rotation.right){
            return new Vector3Int(-vectorToTurn.z, vectorToTurn.y, -vectorToTurn.x);
        }
        if(rotation == Rotation.back){
            return new Vector3Int(-vectorToTurn.x, vectorToTurn.y, -vectorToTurn.z);
        }
        if(rotation == Rotation.left){
            return new Vector3Int(vectorToTurn.z, vectorToTurn.y, vectorToTurn.x);
        }

        return new Vector3Int(0, 0, 0);


    }

    void AddFloorToPathfindNode(Vector3Int floorSpot){
        if(gridManager.grid.GetValue(floorSpot.x, floorSpot.y, floorSpot.z).pathfindingNode.hasFloor){
            return;
        }

        gridManager.grid.GetValue(floorSpot.x, floorSpot.y, floorSpot.z).pathfindingNode.hasFloor = true;

        for(int x = 0; x < 3; x++){
            for(int y = 0; y < 3; y++){
                for(int z = 0; z < 3; z++) {
                    gridManager.grid.GetValue(floorSpot.x, floorSpot.y, floorSpot.z).pathfindingNode.enterableSides[x , 1, z] = true;
                    gridManager.grid.GetValue(floorSpot.x, floorSpot.y, floorSpot.z).pathfindingNode.enterableSides[x , 0, z] = true;
                }
            }
        }

        /*
        pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].hasFloor = true;
        
        //Debug.Log(floorSpot + pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].hasFloor.ToString());
        for(int x = -1; x < 2; x++){
            for(int y = -1; y < 2; y++){
                for(int z = -1; z < 2; z++) {
                    
                    if(floorSpot.x + x > grid.GetLength(0) -1 || floorSpot.z + z > grid.GetLength(1) - 1|| floorSpot.y + y > grid.GetLength(2) - 1|| floorSpot.x + x < 0 || floorSpot.z + z < 0 || floorSpot.y + y < 0){
                        continue;
                    }

                    if(y == -1){
                        pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , 1, z+1] = true;
                        pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].walls[x+1 , 0, z+1] = true;
                        pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].walls[x+1 , 2, z+1] = true;
                    }

                    UpdateSuroundingPathfindingNodes(new Vector3Int(floorSpot.x + x, floorSpot.y + y, floorSpot.z + z));
                    if(pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z+ z].enterableSides[-x+1, -y+1, -z+1] && !pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z+ z].walls[-x+1, -y+1, -z+1] && pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z + z].hasFloor){
                        if(y == 1){
                            if(!pathfinding.Nodes[floorSpot.x, floorSpot.y + 1, floorSpot.z].hasFloor){
                                pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , y+1, z+1] = true;
                            }
                            else{
                                pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , y+1, z+1] = false;
                            }
                       }
                       else{
                            pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , y+1, z+1] = true;
                       }
                    }
                    else if(pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z+ z].walls[-x+1, -y+1, -z+1] || !pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z + z].hasFloor){
                        pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , y+1, z+1] = false;
                    }
                }
            }
        }
        */

    }

    /*
    void UpdateSuroundingPathfindingNodes(Vector3Int floorSpot){
        for(int x = -1; x < 2; x++){
            for(int y = -1; y < 2; y++){
                for(int z = -1; z < 2; z++) {
                    if(floorSpot.x + x > grid.GetLength(0) -1 || floorSpot.z + z > grid.GetLength(1) - 1|| floorSpot.y + y > grid.GetLength(2) - 1|| floorSpot.x + x < 0 || floorSpot.z + z < 0 || floorSpot.y + y < 0){
                        continue;
                    }

                    if(pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z+ z].enterableSides[-x+1, -y+1, -z+1] && !pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z+ z].walls[-x+1, -y+1, -z+1] && pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z + z].hasFloor){
                       if(y == 1){
                            if(!pathfinding.Nodes[floorSpot.x, floorSpot.y + 1, floorSpot.z].hasFloor){
                                pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , y+1, z+1] = true;
                            }
                            else{
                                pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , y+1, z+1] = false;
                            }
                       }
                       else{
                            pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , y+1, z+1] = true;
                       }
                    }
                    else if(pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z+ z].walls[-x+1, -y+1, -z+1] || !pathfinding.Nodes[floorSpot.x + x, floorSpot.y + y, floorSpot.z + z].hasFloor){
                        pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x+1 , y+1, z+1] = false;
                    }
                }
            }
        }

        for(int x = 0; x < 3; x++){
            for(int y = 0; y < 3; y++){
                for(int z = 0; z < 3; z++) {
                    if(pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].walls[x, y, z]){
                        pathfinding.Nodes[floorSpot.x, floorSpot.y, floorSpot.z].enterableSides[x, y, z] = false;
                    }
                }
            }
        }

    }
    */

    void CycleRotation(){
        if(currentRotation == Rotation.forward) 
        {
            currentRotation = Rotation.right;
            rotationOffset = new Vector3Int(0, 0, 1);
        }
        else if(currentRotation == Rotation.right) 
        {
            currentRotation = Rotation.back;
            rotationOffset = new Vector3Int(1, 0, 1);
        }
        else if(currentRotation == Rotation.back) 
        {
            currentRotation = Rotation.left;
            rotationOffset = new Vector3Int(1, 0, 0);
        }
        else if(currentRotation == Rotation.left) 
        {
            currentRotation = Rotation.forward;
            rotationOffset = new Vector3Int(0, 0, 0);
        }
    }

    void SetRotation(Rotation newRotation)
    {
        currentRotation = newRotation;
        if (currentRotation == Rotation.right)
        {
            rotationOffset = new Vector3Int(0, 0, 1);
        }
        else if (currentRotation == Rotation.back)
        {
            rotationOffset = new Vector3Int(1, 0, 1);
        }
        else if (currentRotation == Rotation.left)
        {
            rotationOffset = new Vector3Int(1, 0, 0);
        }
        else if (currentRotation == Rotation.forward)
        {
            rotationOffset = new Vector3Int(0, 0, 0);
        }
    }




}
