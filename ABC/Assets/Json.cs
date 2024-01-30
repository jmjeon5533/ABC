using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Json : MonoBehaviour
{
    public static Json instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }
    public void JsonLoad()
    {
        var g = GameManager.instance;
        var loadData = PlayerPrefs.GetString("SaveData", "");
        if (loadData.Equals(null) || loadData == "")
        {
            print("Null");
            SaveData info = new SaveData();
            MapData firstMap = new MapData();
            info.maps.Add(firstMap);
            firstMap.MapName = "Name";
            firstMap.nodes.Add(Instantiate(g.baseNode, Vector3.zero, Quaternion.identity));

            g.saveData = info;
        }
        else
        {
            print("Saves");
            var data = JsonUtility.FromJson<SsaveData>(loadData);
            SaveData newdata = new SaveData();
            for (int i = 0; i < data.maps.Count; i++)
            {
                MapData newMap = new MapData();
                print(data.maps[i]);
                newMap.MapName = data.maps[i].MapName;
                for (int j = 0; j < data.maps[i].nodes.Count; j++)
                {
                    var node = data.maps[i].nodes[j];
                    Nodes nodeData = Instantiate(g.baseNode,Vector3.zero, Quaternion.identity);
                    nodeData.index = node.index;
                    nodeData.targetZ = node.index;
                    nodeData.nextDir = node.nextDir;
                    nodeData.bpm = node.bpm;
                    nodeData.flip = node.flip;
                    newMap.nodes.Add(nodeData);
                }
                newdata.maps.Add(newMap);
            }
            g.saveData = newdata;
        }
    }
    public void JsonSave()
    {
        var g = GameManager.instance;
        // string[] strings = new string[3];
        // strings[0] = "djgtae";
        // strings[1] = "dnganrbniraenbre";
        // strings[2] = "gnfqwigbqeribve";
        // var newSaveData = new int[]{1,5,34,5,6,1};
        SsaveData newSaveData = new SsaveData();
        newSaveData.maps = new List<SMapData>();

        for (int i = 0; i < g.saveData.maps.Count; i++)
        {
            SMapData newSMap = new SMapData();
            newSMap.nodes = new List<SNodes>();
            newSMap.MapName = g.saveData.maps[i].MapName;

            for (int j = 0; j < g.saveData.maps[i].nodes.Count; j++)
            {
                SNodes newSNode = new SNodes();
                var node = g.saveData.maps[i].nodes[j];
                newSNode.index = node.index;
                newSNode.targetZ = node.targetZ;
                newSNode.nextDir = node.nextDir;
                newSNode.bpm = node.bpm;
                newSNode.flip = node.flip;
                newSMap.nodes.Add(newSNode);
            }
            
            newSaveData.maps.Add(newSMap);
        }
        string jsonData = JsonUtility.ToJson(newSaveData);
        print(jsonData);

        PlayerPrefs.SetString("SaveData", jsonData);
    }
}
[System.Serializable]
public struct SNodes
{
    public int index;
    public float targetZ;
    public float nextDir;
    public int bpm;
    public bool flip;
}
[System.Serializable]
public struct SMapData
{
    public string MapName;
    public List<SNodes> nodes;
}
[System.Serializable]
public struct SsaveData
{
    public List<SMapData> maps;
}
