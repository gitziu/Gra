using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[Serializable]
public class TileData {
    public string TileName;
    public Vector3Int position;

    public TileData(Vector3Int pos, string name){
        this.position = pos;
        this.TileName = name;
    }
}


[Serializable]
public class LevelData {
    //public List<TileData> tiles = new List<TileData>();
    public Board tiles = new Board();
}

[Serializable]
public class Board {
    public string this[Vector3Int position] {
        get {
            return tiles.TryGetValue(position, out string val) ? val : "";
        }
        set {
            if(value == ""){
                tiles.Remove(position);
                return;
            }
            tiles[position] = value; 
        }
    }

    private Dictionary<Vector3Int, string> tiles = new Dictionary<Vector3Int, string>();
}