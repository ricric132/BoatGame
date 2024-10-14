using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Serializable3DArray<T>
{
    [SerializeField]Serializable2DArray<T>[] arr3D;
    public int x;
    public int y;
    public int z;

    public Serializable3DArray(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        arr3D = new Serializable2DArray<T>[z];
        for (int i = 0; i < z; i++)
        {
            arr3D[i] = new Serializable2DArray<T>(x,y);
        }
    }

    public T GetValue(int x, int y, int z)
    {
        return arr3D[z].GetValue(x, y);
    }

    public void UpdateValue(int x, int y, int z, T val)
    {
        arr3D[z].UpdateValue(x, y, val);
    }

    public bool Equals(Serializable3DArray<T> comp)
    {
        if (x != comp.x || y != comp.y || z != comp.z)
        {
            return false;
        }

        for (int curX = 0; curX < x; curX++)
        {
            for (int curY = 0; curY < y; curY++)
            {
                for (int curZ = 0; curZ < z; curZ++)
                {
                    if (!comp.GetValue(curX, curY, curZ).Equals(GetValue(curX, curY, curZ)))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}

[System.Serializable]
public class Serializable2DArray<T>
{
    [SerializeField] Serializable1DArray<T>[] arr2D;

    public Serializable2DArray(int x, int y)
    {
        arr2D = new Serializable1DArray<T>[y];
        for(int i = 0; i < y; i++)
        {
            arr2D[i] = new Serializable1DArray<T>(x);
        }
    }

    public T GetValue(int x, int y)
    {
        return arr2D[y].GetValue(x);
    }

    public void UpdateValue(int x, int y, T val)
    {
        arr2D[y].UpdateValue(x, val);
    }

}

[System.Serializable]
public class Serializable1DArray<T>
{
    [SerializeField]Item<T>[] arr1D;
    public int size;

    public Serializable1DArray(int size)
    {
        arr1D = new Item<T>[size];
        for(int i = 0;i < size; i++)
        {
            arr1D[i] = new Item<T>();
        }
        this.size = size;
    }

    public T GetValue(int x)
    {
        return arr1D[x].data;
    }

    public void UpdateValue(int x, T val)
    {
        arr1D[x].data = val;
    }
}

[System.Serializable]
class Item<T>
{
    public T data;

    public Item()
    {
        data = default(T);
    }
}




