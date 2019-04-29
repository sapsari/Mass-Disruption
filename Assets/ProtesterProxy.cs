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

[RequiresEntityConversion]
public class ProtesterProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public float DegreesPerSecond;

    // The MonoBehaviour data is converted to ComponentData on the entity.
    // We are specifically transforming from a good editor representation of the data (Represented in degrees)
    // To a good runtime representation (Represented in radians)
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new ProtesterData
        {
            //**--Speed = .51f + Random.value * .02f,
            Speed = .51f + Random.value * .02f,
            Destination = null
        };
        dstManager.AddComponentData(entity, data);
    }
}
