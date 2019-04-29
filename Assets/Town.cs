using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Town : MonoBehaviour
{
    public GameObject PrefabProtester;

    public List<Protester> protesters = new List<Protester>();

    List<Protester> protestersBlue = new List<Protester>();
    List<Protester> protestersWhite = new List<Protester>();
    List<Protester> protestersYellow = new List<Protester>();
    List<Protester> protestersRed = new List<Protester>();
    List<Protester> protestersGray = new List<Protester>();

    List<List<Protester>> groupsBlue = new List<List<Protester>>();
    List<List<Protester>> groupsWhite = new List<List<Protester>>();
    List<List<Protester>> groupsYellow = new List<List<Protester>>();
    List<List<Protester>> groupsRed = new List<List<Protester>>();


    public Camera camera;
    float width = 100, height=100;

    const int MassThreshold = 3;

    // Start is called before the first frame update
    void Start()
    {
        var size = 30;
        width = size;
        height = size;
        var radius = .85f;

        PoissonDiscSampler sampler = new PoissonDiscSampler(width, height, radius);

        foreach (Vector2 sample in sampler.Samples())
        {
            var o = Instantiate(PrefabProtester, new Vector3(sample.x - width / 2, sample.y - height / 2, 0), Quaternion.identity);
            //o.transform.parent = this.transform;
            var p = o.GetComponent<Protester>();
            p.Init();
            p.town = this;
            protesters.Add(p);

            if (p.Color == Color.yellow)
            {
                p.classMembers = protestersYellow;
                p.classGroups = groupsYellow;
            }
            if (p.Color == Color.white)
            {
                p.classMembers = protestersWhite;
                p.classGroups = groupsWhite;
            }
            if (p.Color == Color.blue)
            {
                p.classMembers = protestersBlue;
                p.classGroups = groupsBlue;
            }
            if (p.Color == Color.red)
            {
                p.classMembers = protestersRed;
                p.classGroups = groupsRed;
            }
            if (p.Color == Color.gray)
                p.classMembers = protestersGray;

            p.classMembers.Add(p);
        }

            //for (var i = 0; i < 100; i++)
            //Instantiate(PrefabProtester, new Vector3(i, i, 0), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetButtonDown("Fire1") && !MouseOnObject)
        if (Input.GetMouseButtonDown(0))
        {
            var pos = camera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            Debug.Log("jop @ " + pos);

            var min = float.MaxValue;
            Protester hit = null;
            foreach (var p in protesters)
            {
                var dist = (p.transform.position - pos).sqrMagnitude;
                if (dist < min)
                {
                    min = dist;
                    hit = p;
                }
                if (dist < 1)
                    p.Disperse();
            }
            
            /*
            Debug.Log(min);
            Debug.Log(hit.transform.position);
            Debug.Log(pos);*/
        }
    }

    public bool IsOut(Vector3 pos)
    {
        var offset = 1;
        if (pos.x > this.width / 2 + offset) return true;
        if (pos.x < -this.width / 2 - offset) return true;
        if (pos.y > this.height / 2 + offset) return true;
        if (pos.y < -this.height / 2 - offset) return true;
        return false;
    }

    void OnGUI()
    {
        if (Application.isEditor)  // or check the app debug flag
        {
            var text = getstr(groupsBlue) + getstr(groupsWhite) + getstr(groupsYellow) + getstr(groupsRed);
            //GUI.Label(screenRect, "Debug text");
            GUI.Label(new Rect(0, 0, 1000, 100), text);
        }
    }

    string getstr(List<List<Protester>> list)
    {
        /*if (list.Count == 0)
            return "";
        else
            return list[0].Count + " ";
            */

        return list.Count + " " + list.Count(i => i.Count >= 3) + " " + list.Count(i => i.Count >= 4) + "\n";

    }
}
