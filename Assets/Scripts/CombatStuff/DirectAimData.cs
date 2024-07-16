using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DirectAimData
{
    public List<HitSpot> allHittableSpots;
    public Vector3 aimedSpot;
    public Vector3Int coords;
    public int pierceNeeded;
}

public enum HitVital
{
    instakill,
    vital,
    normal,
    nonvital,
    miss
}

[System.Serializable]
public class HitSpot
{
    public HitVital hitVital;
    public Transform transform;
    public int totalPierceNeeded;
}
