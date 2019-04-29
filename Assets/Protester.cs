//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using Random = UnityEngine.Random;

// Serializable attribute is for editor support.
[System.Serializable]
public struct ProtesterData : IComponentData
{
    //public float RadiansPerSecond;
    public float Speed;
    public Vector3? Destination;
}


// This system updates all entities in the scene with both a RotationSpeed and Rotation component.
public class RotationSpeedSystem : JobComponentSystem
{
    // Use the [BurstCompile] attribute to compile a job with Burst. You may see significant speed ups, so try it!
    [BurstCompile]
    struct ProtestorJob : IJobForEach<Translation, ProtesterData>
    {
        public float DeltaTime;

        // The [ReadOnly] attribute tells the job scheduler that this job will not write to rotSpeed
        public void Execute(ref Translation translation, [ReadOnly] ref ProtesterData data)
        {
            //return;//**--
            //Debug.Log(translation.Value);

            translation.Value += new float3(1.005f, 1.005f, .005f);
            return;//**--

            // Rotate something about its up vector at the speed given by RotationSpeed.
            //**--rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotSpeed.RadiansPerSecond * DeltaTime));
            Vector3 vec3 = translation.Value;
            Protester.UpdateAux(ref vec3, ref data.Destination, data.Speed, DeltaTime);
            translation.Value = vec3;

            //Debug.Log("tr:"+translation.Value);
        }
    }

    // OnUpdate runs on the main thread.
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        //return new JobHandle();

        var job = new ProtestorJob()
        {
            DeltaTime = Time.deltaTime
        };

        return job.Schedule(this, inputDependencies);
    }
}

public class Protester : MonoBehaviour
{
    public Town town;
    SpriteRenderer spriteRenderer;

    public List<Protester> classMembers;
    public List<List<Protester>> classGroups;
    List<Protester> group;


    public Color Color;
    /*{
        get { return this.spriteRenderer.color; }
        set { this.spriteRenderer.color = value; }
    }*/

    Vector3 Position => this.transform.position;

    public void Init()
    {
        var rand = Random.Range(0, 5);
        if (rand == 0)
            this.Color = Color.yellow;
        else if (rand == 1)
            this.Color = Color.white;
        else if (rand == 2)
            this.Color = Color.blue;
        else if (rand == 3)
            this.Color = Color.red;
        else
            this.Color = Color.gray;

    }


    // Start is called before the first frame update
    void Start()
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        /*
        var rand = Random.Range(0, 4);
        if (rand == 0)
            this.Color = Color.yellow;
        else if (rand == 1)
            this.Color = Color.white;
        else if (rand == 2)
            this.Color = Color.blue;
        else
            this.Color = Color.red;*/

        this.spriteRenderer.color = this.Color;

        this.speed = .51f + Random.value * .02f;
    }

    float speed;
    float sqrRadiusMult2 = .4f;
    Vector3? destination;

    internal static void UpdateAux(ref Vector3 Position, ref Vector3? destination, float speed, float dt)
    {
        if (destination.HasValue)
        {
            var delta2 = destination.Value - Position;
            if (delta2.sqrMagnitude < .1f)
                destination = null;
        }

        if (destination == null)
        {
            //**--var angle = Random.value * Mathf.PI * 2;
            var angle = .5f * Mathf.PI * 2;
            //**--var distance = Random.value * 2;
            var distance = .5f * 2;
            var x = Mathf.Cos(angle);
            var y = Mathf.Sin(angle);
            destination = Position + new Vector3(x, y) * distance;
        }

        var delta = destination.Value - Position;

        var deltaNormalized = delta.normalized;

        var newPosition = Position + deltaNormalized * speed * dt;

        var isValid = true;
        /*foreach(var p in town.protesters)
        {
            if (p == this)
                continue;

            if ((newPosition - p.Position).sqrMagnitude < sqrRadiusMult2)
            {
                isValid = false;
                destination = null;
                break;
            }
        }*/


        if (isValid)
            Position = Position + deltaNormalized * speed * dt;
    }

    bool isDispersed;
    float disperseTime;
    public void Disperse(bool isAuto = false)
    {
        Debug.Log("dispersed");

        isDispersed = true;
        disperseTime = Time.time + Random.value * 2 + 3;

        if (isAuto)
            disperseTime = Time.time;// + Random.value + .5f;


        if (this.group != null)
        {
            this.group.Remove(this);
            if (this.group.Count == 0)
                classGroups.Remove(this.group);
            else
                this.group[0].Disperse(isAuto);
            this.group = null;
        }
    }

    bool hasWaited;
    bool isWaiting;
    private IEnumerator Wait()
    {
        hasWaited = true;
        isWaiting = true;
        // process pre-yield
        yield return new WaitForSeconds(Random.value * .8f);
        // process post-yield
        isWaiting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (destination.HasValue)
        {
            var delta2 = destination.Value - Position;
            if (delta2.sqrMagnitude < .1f)
            {
                destination = null;
                hasWaited = false;
            }
        }

        if (isDispersed && Time.time > disperseTime)
            isDispersed = false;

        if (destination == null)
        {
            if (!isDispersed)
            {
                if (!hasWaited)
                {
                    StartCoroutine(Wait());
                    return;
                }

                if (isWaiting)
                    return;

                if (this.group != null && this.group.Count >= 3)
                    return;

                if (this.group != null && this.group.Count < 3)
                {/*
                    this.classGroups.Remove(this.group);
                    foreach (var p in this.group)
                        p.group = null;*/
                    this.Disperse(true);
                }
            }

            int numTries = 0;
            do
            {
                var angle = Random.value * Mathf.PI * 2;
                var distance = Random.value * 2;
                var x = Mathf.Cos(angle);
                var y = Mathf.Sin(angle);
                destination = Position + new Vector3(x, y);
                numTries++;
            } while (town.IsOut(destination.Value) || numTries > 10);
        }

        var delta = destination.Value - Position;

        var deltaNormalized = delta.normalized;

        var newPosition = Position + deltaNormalized * speed * Time.deltaTime;

        var isValid = true;

        if (this.group != null)
            isValid = false;

        if (this.Color != Color.gray && !this.isDispersed && this.group == null)
            //foreach(var p in town.protesters)
            foreach (var p in this.classMembers)
            {
                if (p == this)
                    continue;

                if ((newPosition - p.Position).sqrMagnitude < sqrRadiusMult2)
                {
                    isValid = false;
                    destination = null;
                    hasWaited = false;

                    if (!p.isDispersed)
                    {

                        if (this.group == null && p.group == null)
                        {
                            this.group = new List<Protester>();
                            p.group = this.group;
                            classGroups.Add(group);

                            this.group.Add(this);
                            this.group.Add(p);

                            //StartCoroutine(this.Wait());
                            //StartCoroutine(p.Wait());
                            this.hasWaited = false;
                            p.hasWaited = false;
                            this.destination = null;
                            p.destination = null;
                        }
                        else if (p.group != null)
                        {
                            this.group = p.group;
                            this.group.Add(this);

                            this.hasWaited = false;
                            this.destination = null;
                        }
                        else
                        {
                            p.group = this.group;
                            this.group.Add(p);

                            p.hasWaited = false;
                            p.destination = null;

                            //**-- error??
                        }
                    }


                    break;
                }
            }


        if (isValid)
        this.transform.position = Position + deltaNormalized * speed * Time.deltaTime;

        
    }
}
