﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BuildScript : NetworkBehaviour
{
    //TODO: Re-add preview material
    //Fix placement rotations to be type independent
    //Clean up code (Getcomponents) and overall structure
    //Snapped and move away and click causes mount point to be removed


    public int teamID;
    public bool buildMode;
    public Material invalidMaterial;
    public Material validMaterial;
    public List<GameObject> objects = new List<GameObject>();
    public Transform baseParent;

    Transform camera;
    List<MountPoint> mountPoints = new List<MountPoint>();
    GameObject previewObject;
    BuildPoints previewBuildPoints;
    MeshRenderer meshRend;
    int currentObject;
    Material originalMaterial;    
    List<List<bool>> pointUsed = new List<List<bool>>();
    List<Vector3> placedObjects = new List<Vector3>();

    int objectIndex = -1;
    int vertexIndex = -1;
    int finalObjectIndex = -1;
    int finalVertexIndex = -1;


    void Start()
    {
        camera = Camera.main.transform;
        currentObject = 0;
        if(teamID == 0)
        {
            baseParent = GameObject.Find("Base1Center").transform;
        }
    }

    void Update()
    {
        if (isLocalPlayer && buildMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetpreviewObjectObject(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetpreviewObjectObject(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetpreviewObjectObject(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetpreviewObjectObject(3);
            }
            if (previewObject != null)
            {
                PositionPreview();

                if (Input.GetKeyDown(KeyCode.B))
                {
                    PlaceObject();
                }
            }
        }
    }

    void PositionPreview()
    {
        Ray previewRay = new Ray(camera.position, camera.forward);
        RaycastHit previewHit;
        //meshRend.material = invalidMaterial;
        previewObject.transform.localEulerAngles = new Vector3(0, camera.localEulerAngles.y - 90, 0);
        previewBuildPoints.valid = false;
        if (Physics.Raycast(previewRay, out previewHit, 5f))
        {
            previewObject.transform.position = previewHit.point;

            if (currentObject == 0 && previewHit.collider.gameObject.tag == "Ground")
            {
                //meshRend.material = validMaterial;
                previewBuildPoints.valid = true;
            }

            SnapPreview();
        }
        else
        {
            //Snap and check
            previewObject.transform.position = camera.position + previewBuildPoints.offset + previewRay.direction * 5f;
            SnapPreview();
        }



    }
    void SnapPreview()
    {
        foreach (MountPoint mp in mountPoints)
        {
            for (int j = 0; j <  mp.pointType.Count; j++)
            {
                if (mp.pointType[j] == previewBuildPoints.type)
                {
                    objectIndex = mountPoints.IndexOf(mp);
                    vertexIndex = j;
                    Vector3 v3 = mp.points[vertexIndex];
                    if (!pointUsed[objectIndex][vertexIndex])   
                    {
                        Vector3 newVec3 = mp.parent.TransformPoint(new Vector3(v3.x / (mp.parent.transform.localScale.x), v3.y / (mp.parent.transform.localScale.y), v3.z / (mp.parent.transform.localScale.z)));
                        Debug.DrawRay(newVec3, Vector3.up, Color.red);
                        if (Vector3.Distance(previewObject.transform.position, newVec3) < 2)
                        {
                            bool near = false;
                            foreach(Vector3 nearV3 in placedObjects)
                            {
                                if(Vector3.Distance(nearV3,newVec3) < 0.1f)
                                {
                                    near = true;
                                }
                            }
                            if (!near)
                            {
                                previewObject.transform.position = newVec3 + previewBuildPoints.offset;
                                previewObject.transform.LookAt(mp.parent);
                                previewObject.transform.localEulerAngles = new Vector3(0, previewObject.transform.localEulerAngles.y - 90, 0);
                                if (previewBuildPoints.type == mp.parent.GetComponent<BuildPoints>().type)
                                {
                                    previewObject.transform.localEulerAngles = mp.parent.transform.localEulerAngles;
                                }
                                if (previewBuildPoints.type == BuildPoints.MountType.Wall 
                                    && mp.parent.GetComponent<BuildPoints>().type == BuildPoints.MountType.Door1)
                                {
                                    previewObject.transform.localEulerAngles = mp.parent.transform.localEulerAngles;
                                }
                                if (previewBuildPoints.type == BuildPoints.MountType.Door1
                                   && mp.parent.GetComponent<BuildPoints>().type == BuildPoints.MountType.Wall)
                                {
                                    previewObject.transform.localEulerAngles = mp.parent.transform.localEulerAngles;
                                }
                                //meshRend.material = validMaterial;
                                previewBuildPoints.valid = true;

                                finalObjectIndex = mountPoints.IndexOf(mp);
                                finalVertexIndex = mp.points.IndexOf(v3);
                            }
                        }
                    }
                }
            }
        }
    }

    void SetpreviewObjectObject(int id)
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        currentObject = id;
        previewObject = Instantiate(objects[currentObject], baseParent);
        //meshRend = previewObject.GetComponent<MeshRenderer>();
        //originalMaterial = meshRend.material;
        //meshRend.material = invalidMaterial;
        previewObject.layer = 2;
        previewBuildPoints = previewObject.GetComponent<BuildPoints>();
        previewObject.transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    void PlaceObject()
    {
        if (finalVertexIndex != -1 && finalObjectIndex != -1)
        {
            pointUsed[finalObjectIndex][finalVertexIndex] = true;
        }
        if (previewBuildPoints.valid)
        {
            previewObject.name = (previewBuildPoints.type.ToString() + " Placed");
            BuildPoints bp = previewBuildPoints;
            mountPoints.Add(bp.mounting);
            List<bool> newBools = new List<bool>();
            foreach (Vector3 v3 in bp.mounting.points)
            {
                bool used = false;
                newBools.Add(used);
            }
            pointUsed.Add(newBools);
            //meshRend.material = originalMaterial;
            placedObjects.Add(previewObject.transform.position - previewBuildPoints.offset);
            previewObject.layer = 0;
            CmdSpawnBuildingPart(objects[currentObject].ToString(), previewObject.transform.position.x, previewObject.transform.position.y, 
                previewObject.transform.position.z, previewObject.transform.position, previewObject.transform.rotation);
            previewObject = null;
            //meshRend = null;
        }
    }
    [Command]
    void CmdSpawnBuildingPart(string objectString, float xVal, float yVal, float zVal, Vector3 position, Quaternion rotation)
    {
        Debug.Log("Placing " + objectString + " at X:" + xVal + "; Y:" + yVal + "; Z:" + zVal);
        GameObject instance = Instantiate(objects[currentObject], position, rotation);
        NetworkServer.Spawn(instance);
    }
}
