using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Json : MonoBehaviour
{
    public static Json instance {get; private set;}
    private void Awake()
    {
        instance = this;
    }
    public void JsonLoad()
    {
        var g = GameManager.instance;
        var loadData = PlayerPrefs.GetString("SaveData", "");
        if(loadData.Equals(null) || loadData == "")
        {
            print("Null");
            SaveData info = new SaveData();
            MapData firstMap = new MapData();
            info.maps.Add(firstMap);
            firstMap.MapName = "Name";
            firstMap.nodes.Add(Instantiate(g.baseNode,Vector3.zero,Quaternion.identity));

            g.saveData = info;
        }
        else
        {
            print("Saves");
            g.saveData = JsonUtility.FromJson<SaveData>(loadData);
        }
    }
    public void JsonSave()
    {
        var g = GameManager.instance;
        string jsonData = JsonUtility.ToJson(g.saveData);

        PlayerPrefs.SetString("SaveData",jsonData);
    }
}
