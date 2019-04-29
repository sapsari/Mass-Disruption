using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Mass
{
    float startTime;
    public List<Protester> protesters;

    public Mass()
    {
        this.protesters = new List<Protester>();
        this.startTime = Time.time;
    }

    public float TimePassed => Time.time - startTime;
}

public class Town : MonoBehaviour
{
    public GameObject PrefabProtester;

    public List<Protester> protesters = new List<Protester>();

    List<Protester> protestersBlue = new List<Protester>();
    List<Protester> protestersWhite = new List<Protester>();
    List<Protester> protestersYellow = new List<Protester>();
    List<Protester> protestersRed = new List<Protester>();
    List<Protester> protestersGray = new List<Protester>();

    List<Mass> groupsBlue = new List<Mass>();
    List<Mass> groupsWhite = new List<Mass>();
    List<Mass> groupsYellow = new List<Mass>();
    List<Mass> groupsRed = new List<Mass>();


    public Camera camera;
    float width = 100, height=100;

    public const int MassThreshold = 3;

    int Score = 0, Support = 100;

    enum GameState { Over, Running};
    GameState state;

    float gameStartTime;

    Vector2 getRandPos =>
        new Vector2((Random.value - .5f) * (width / 2), (Random.value - .5f) * (height / 2));


    // Start is called before the first frame update
    void StartGame()
    {
        var size = 30;
        width = size;
        height = size;
        //var radius = .85f;
        var radius = 1.085f;

        PoissonDiscSampler sampler = new PoissonDiscSampler(width, height, radius);

        foreach (Vector2 sample in sampler.Samples())
        {
            Summon(sample);
        }

        Score = 0;
        Support = 100;
        gameStartTime = Time.time;

        //for (var i = 0; i < 100; i++)
        //Instantiate(PrefabProtester, new Vector3(i, i, 0), Quaternion.identity);

        state = GameState.Running;
    }

    private void Summon(Vector2 sample)
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

    int UpdateSupportAux(List<Mass> lists)
    {
        var sum = 0;
        foreach(var mass in lists)
        {
            var count = mass.protesters.Count;

            if (count == 3 && mass.TimePassed > 5)
                sum += 1;

            if (count == 4 && mass.TimePassed > 3)
                sum += 2;

            if (count == 5 && mass.TimePassed > 1)
                sum += 3;

            if (count > 5)
                //sum += count - 2;
                sum += 4;// count - 2;
        }

        return sum;
    }


    void UpdateSupport()
    {
        Support -= UpdateSupportAux(groupsBlue);
        Support -= UpdateSupportAux(groupsWhite);
        Support -= UpdateSupportAux(groupsYellow);
        Support -= UpdateSupportAux(groupsRed);
    }

    void EndGame()
    {
        Debug.Log("end");

        state = GameState.Over;

        foreach(var p in protesters)
            Destroy(p.gameObject);

        protesters.Clear();

        protestersBlue.Clear();
        protestersWhite.Clear();
        protestersYellow.Clear();
        protestersRed.Clear();
        protestersGray.Clear();

        groupsBlue.Clear();
        groupsWhite.Clear();
        groupsYellow.Clear();
        groupsRed.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
            Support = -1;


        if (Support > 0 && state == GameState.Running)
        {
            //var _score = (int)Time.time;
            var _score = (int)(Time.time - gameStartTime);
            if (_score != Score)
            {
                Score = _score;
                if (Score%2==0)
                UpdateSupport();

                //Summon(getRandPos);//**--//**--
            }
        }
        else if (state == GameState.Running)
        {
            //state = GameState.Over;
            EndGame();
        }

        if (state == GameState.Over) return;
        

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
            var text = 
                getstr("blue", protestersBlue, groupsBlue) + 
                getstr("whit", protestersWhite, groupsWhite) + 
                getstr("yelw", protestersYellow, groupsYellow) + 
                getstr("redd", protestersRed, groupsRed);
            //GUI.Label(screenRect, "Debug text");
            GUI.Label(new Rect(333, 0, 1000, 100), text);
        }

        if (state == GameState.Running)
        {


            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 40;
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.UpperRight;
            //GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "DENEME", style);
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Score: " + Score, style);

            style.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Support: " + Support, style);
        }

        else
        {
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 40;
            style.normal.textColor = Color.black;
            //style.alignment = TextAnchor.UpperRight;
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(0, 0, Screen.width, Screen.height/2), "Game Over\nScore: " + Score, style);


            var bs = new GUIStyle(GUI.skin.button);
            bs.fontSize = 40;
            bs.normal.textColor = Color.black;
            if (GUI.Button(new Rect(Screen.width/4, Screen.height/2, Screen.width/2, Screen.height/4), "Start", bs))
                StartGame();

        }
    }

    

    string getstr(string color, List<Protester> cl, List<Mass> list)
    {
        /*if (list.Count == 0)
            return "";
        else
            return list[0].Count + " ";
            */

        return
            color + " " + cl.Count + " "+
            list.Count + " " + list.Count(i => i.protesters.Count >= 3) + " " + list.Count(i => i.protesters.Count >= 4) + "\n";

    }
}
