using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

public class EntityConvert : MonoBehaviour, IConvertGameObjectToEntity
{
    

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //dstManager.AddComponentData<EntitieComponentData>(entity, entitieData);
    }
}
