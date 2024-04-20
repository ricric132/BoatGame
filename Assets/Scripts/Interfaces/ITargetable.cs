using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
    DirectAimData GetLocation();
    void WillHit(bool hit);
    

}
