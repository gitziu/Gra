using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[Serializable]
public class TileData{
    public string TileName;
    public Vector3Int position;

    public TileData(Vector3Int pos, string name){
        this.position = pos;
        this.TileName = name;
    }
}


[Serializable]
public class LevelData{
    public string levelID = "";
    public List<TileData> tiles = new List<TileData>();
}