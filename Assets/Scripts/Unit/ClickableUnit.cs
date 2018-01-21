using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableUnit : MonoBehaviour {
    public GameObject mapObject, unit, cam;
    public TileMap t;

    void Start()
    {
        mapObject = GameObject.FindWithTag("Map");
        t = mapObject.GetComponent<TileMap>();
        cam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void OnMouseDown()
    {
        t.selectedUnit = unit;
        Debug.Log("fesdv");
        cam.GetComponent<CameraController>().MoveCameraTo(transform.position);
    }
}
