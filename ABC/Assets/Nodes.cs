using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nodes : MonoBehaviour
{
    public int index;
    public float targetZ;
    public float nextDir;
    public Nodes nextNode;
    public Nodes preNode;
    public GameObject[] propertyIcon;

    public int bpm = -1;
    public bool flip;
    
    public void OnNode()
    {
        Speed();
        Inverse();
    }
    void Speed()
    {
        if(bpm == -1) return;
        GameManager.instance.bpm = bpm;
    }
    void Inverse()
    {
        var g = GameManager.instance;
        g.inverse = flip ? !g.inverse : g.inverse;
    }
    public void InitIcon(int index)
    {
        for(int i = 0; i < propertyIcon.Length; i++)
        {
            propertyIcon[i].SetActive(false);
        }
        propertyIcon[index].SetActive(true);
    }
}