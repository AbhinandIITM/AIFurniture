using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class TaptoPlace : MonoBehaviour
{
    [SerializeField]
    GameObject placedprefab;

    GameObject spawnedObject;

    ARRaycastManager aRRaycastmanager;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        aRRaycastmanager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if(Input.touchCount==0)
        {
            return;
        }

        if (aRRaycastmanager.Raycast(Input.GetTouch(0).position,hits,TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(placedprefab, hitPose.position, hitPose.rotation);
            }
            else
            {
                spawnedObject.transform.position = hitPose.position;
                spawnedObject.transform.rotation = hitPose.rotation;
            }
        }
    }
}
