using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileRegistry
{
    public static Dictionary<string, TileBase> TileReg = new Dictionary<string, TileBase>();
    public static Dictionary<string, string> StandardToMinimap = new Dictionary<string, string>();
    public static void LoadTilesFromResources()
    {
        TileBase[] tiles = Resources.LoadAll<TileBase>("Tiles");

        foreach (var tile in tiles)
        {
            if(!StandardToMinimap.ContainsKey(tile.name)){
                if(!tile.name.StartsWith("Minimap")){
                    StandardToMinimap.Add(tile.name, "Minimap" + tile.name);
                    Debug.Log("new minimap tile key : " + tile.name + " , result : " + "Minimap"+tile.name);
                }
            }
            if (!TileReg.ContainsKey(tile.name))
            {
                TileReg.Add(tile.name, tile);
                Debug.Log("new tile name : " + tile.name);
            }
        }
    }

    public static TileBase GetTile(string name)
    {
        return TileReg.TryGetValue(name, out var tile) ? tile : null;
    }

    public static string GetMinimapAlt(string name){
        return StandardToMinimap.TryGetValue(name, out var new_name) ? new_name : "";
    }
}
