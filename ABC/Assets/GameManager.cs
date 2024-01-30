using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public interface IModeInfo
{
    public void Init();
    public void Loop();
}
public class GameMode : IModeInfo
{
    float curOffset;
    float halfRotateCheck;
    float countStartTime;
    public void Init()
    {
        var g = GameManager.instance;
        g.curNodeIndex = 0;
        curOffset = g.Offset;
        countStartTime = curOffset - (60 / g.bpm * g.countDown * 1000);
        g.curRotZ = (g.Offset % 2 + 1) * 180;
        var firstCircle = g.circle[g.curNodeIndex % 2].position = g.saveData.maps[g.mapIndex].nodes[g.curNodeIndex].transform.position;
        g.cam.transform.position = firstCircle + (Vector3.back * 10);
        halfRotateCheck = 0;
        g.inverse = false;
        g.isDead = false;
        g.audioSource.clip = g.bgm;
        g.audioSource.Play();
    }
    public void Loop()
    {
        var g = GameManager.instance;
        var z = g.curRotZ += g.bpm / 120 * Time.deltaTime * 360 * (g.inverse ? 1 : -1);
        curOffset -= Time.deltaTime * 1000;
        if (g.curRotZ >= 360)
        {
            g.curRotZ -= 360;
            halfRotateCheck -= 360;
        }
        else if (g.curRotZ <= 0)
        {
            g.curRotZ += 360;
            halfRotateCheck += 360;
        }
        Vector2 rotatePos = new Vector2(Mathf.Cos(z * Mathf.Deg2Rad) * 1.5f, Mathf.Sin(z * Mathf.Deg2Rad) * 1.5f);

        var firstCircle = g.circle[g.curNodeIndex % 2].position = g.saveData.maps[g.mapIndex].nodes[g.curNodeIndex].transform.position;
        g.circle[(g.curNodeIndex + 1) % 2].position = firstCircle + (Vector3)rotatePos;

        g.cam.transform.position = Vector3.Lerp(g.cam.transform.position, firstCircle + (Vector3.back * 10), 0.1f);

        var count = Mathf.Ceil(curOffset * 0.001f / (60 / g.bpm));
        Debug.Log(count);
        if (count >= 0)
        {
            halfRotateCheck = g.curRotZ;
            if (count < g.countDown && count > 0)
            {
                if (count > 0) g.startCountText.text = count.ToString();
            }
            else if (count == 0) g.startCountText.text = "시작!";
            else g.startCountText.text = "";
        }
        else g.startCountText.text = "";

        var ableAngle = Mathf.DeltaAngle(g.curRotZ, g.saveData.maps[g.mapIndex].nodes[g.curNodeIndex].targetZ);
        if (ableAngle < 45 && ableAngle > -45 && count <= 0)
        {
            if (Input.GetMouseButtonDown(0) && !g.IsClear() && !g.isDead)
            {
                g.saveData.maps[g.mapIndex].nodes[g.curNodeIndex].isCheck = true;
                g.NextNode();
            }
        }
        else
        {
            
            if (!g.saveData.maps[g.mapIndex].nodes[g.curNodeIndex].isCheck && !g.IsClear())
            {
                if (count < 0)
                {
                    Debug.Log("Fail");
                    g.isDead = true;
                }
            }
        }
    }
}
public class EditMode : IModeInfo
{
    RaycastHit2D hit;
    public void Init()
    {
        var g = GameManager.instance;
        g.selectNode = g.saveData.maps[g.mapIndex].nodes[0];
        g.audioSource.Stop();
    }
    public void Loop()
    {
        var g = GameManager.instance;
        g.cam.transform.position = g.selectNode.transform.position + (Vector3.back * 10);
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                Vector3 mouse = g.cam.ScreenToWorldPoint(Input.mousePosition);
                hit = Physics2D.Raycast(mouse, Vector3.forward, Mathf.Infinity, LayerMask.GetMask("Tile"));
                if (hit)
                {
                    g.selectNode = hit.collider.GetComponent<Nodes>();
                }
            }
        }
    }
}
public class Mode
{
    public readonly IModeInfo gameMode;
    public readonly IModeInfo editMode;
    public Mode()
    {
        gameMode = new GameMode();
        editMode = new EditMode();
    }
    public IModeInfo curMode;
    public void Init() => curMode.Init();
    public void Loop() => curMode.Loop();
}
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    [Header("NodeSetting")]
    public float bpm;
    public Mode modes;
    public Nodes baseNode;
    public int curNodeIndex;
    public SaveData saveData;
    public int mapIndex;
    [Header("System")]
    public AudioClip bgm;
    public AudioSource audioSource;
    public Camera cam;
    public GameObject PlayParent;
    public Transform[] circle;
    public bool isDead;
    public int Offset;
    public float countDown;
    public float curRotZ;
    public bool inverse;
    [Header("UI")]
    public Nodes selectNode;
    public Text startCountText;
    public Button[] ModeButton;
    public GameObject EditParent;
    public Button[] EditButton;
    public Button[] propertyAddButton;

    private void Awake()
    {
        instance = this;
        modes = new Mode();
    }
    private void Start()
    {
        Json.instance.JsonLoad();
        curNodeIndex = 0;

        InitButton();
        InitNode();
        InitMode(modes.gameMode);
        modes.Init();
    }
    void InitButton()
    {
        for (int i = 0; i < EditButton.Length; i++)
        {
            var num = i;
            EditButton[num].onClick.AddListener(() =>
            {
                var newNode = new Nodes();
                var n = saveData.maps[mapIndex].nodes;
                var newRotate = (num * 45) - n[n.Count - 1].targetZ;
                print(newRotate);

                if (Mathf.Abs(newRotate) == 180)
                {
                    if (n.Count - 1 <= selectNode.index)
                    {
                        print($"OutOfRange(MaxIndex : {n.Count - 1}, CurIndex{selectNode.index})");
                        selectNode = n[n.Count - 2];
                    }
                    Destroy(n[n.Count - 1].gameObject);
                    n.RemoveAt(n.Count - 1);
                    n[n.Count - 1].nextDir = 0;
                    n[n.Count - 1].nextNode = null;

                }
                else
                {
                    n[n.Count - 1].nextDir = newRotate;
                    saveData.maps[mapIndex].nodes.Add(newNode);
                }

                InitNode();
            });
        }
        ModeButton[0].onClick.AddListener(() =>
        {
            InitMode(modes.gameMode);
        });
        ModeButton[1].onClick.AddListener(() =>
        {
            InitMode(modes.editMode);
        });
        propertyAddButton[0].onClick.AddListener(() =>
        {
            selectNode.bpm = 100;
            InitNode();
        });
        propertyAddButton[1].onClick.AddListener(() =>
        {
            selectNode.flip = !selectNode.flip;
            InitNode();
        });
    }
    private void Update()
    {
        modes.Loop();
    }
    public void InitMode(IModeInfo mode)
    {
        modes.curMode = mode;
        modes.Init();
        var isGame = mode == modes.gameMode;
        EditParent.SetActive(!isGame);
        PlayParent.SetActive(isGame);
    }
    public void InitNode()
    {
        Vector3 beforeNodePos = Vector3.zero;
        float beforeNodeRot = 0;
        var n = saveData.maps[mapIndex].nodes;
        for (int i = 0; i < n.Count; i++)
        {
            beforeNodeRot += saveData.maps[mapIndex].nodes[i].nextDir;
            Nodes obj;
            if (n[i] == null)
            {
                obj = Instantiate(baseNode, beforeNodePos, Quaternion.Euler(new Vector3(0, 0, beforeNodeRot)));
                n[i] = obj;
            }
            else obj = n[i];

            if (i > 0)
            {
                n[i - 1].nextNode = n[i];
                n[i].preNode = n[i - 1];
            }
            n[i].targetZ = beforeNodeRot;
            n[i].isCheck = false;
            obj.transform.position = beforeNodePos;
            obj.transform.eulerAngles = Vector3.forward * obj.targetZ;
            beforeNodePos =
            obj.transform.position + new Vector3(Mathf.Cos(beforeNodeRot * Mathf.Deg2Rad) * 1.5f, Mathf.Sin(beforeNodeRot * Mathf.Deg2Rad) * 1.5f);
            n[i].index = i;
            int initIndex = 0;
            if (obj.bpm != -1)
            {
                var index = i - 1;
                while (true)
                {
                    if (n[index].bpm != -1)
                    {
                        initIndex = n[index].bpm < n[i].bpm ? 1 : 2;
                        print($"{n[index].bpm} : {n[i].bpm}");
                        break;
                    }
                    else if (index == 0)
                    {
                        initIndex = bpm < n[i].bpm ? 1 : 2;
                        print("Index = 0");
                        break;
                    }
                    index--;
                }
            }
            initIndex = obj.flip ? 3 : initIndex;
            n[i].InitIcon(initIndex);
        }
    }
    public void NextNode()
    {
        curNodeIndex++;
        saveData.maps[mapIndex].nodes[curNodeIndex].OnNode();
        curRotZ -= 180;
        if (IsClear())
        {
            print("clear");
        }
    }
    public bool IsClear()
    {
        return curNodeIndex >= saveData.maps[mapIndex].nodes.Count - 1;
    }
}
[System.Serializable]
public class SaveData
{
    public List<MapData> maps = new List<MapData>();
}
[System.Serializable]
public class MapData
{
    public string MapName;
    public List<Nodes> nodes = new List<Nodes>();
}