using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

public class PrefabEntities : MonoBehaviour, IConvertGameObjectToEntity
{
    public static Entity prefabEntity;
    public static EntityManager managerEntity;

    public GameObject prefabGameObject;

    private BlobAssetStore blobAssetStore;

    private void Awake()
    {
        blobAssetStore = new BlobAssetStore();

        managerEntity = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //using ()
        {
            Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                prefabGameObject,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            PrefabEntities.prefabEntity = prefabEntity;
        }
    }
}
