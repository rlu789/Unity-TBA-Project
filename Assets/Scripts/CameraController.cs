using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float dragSpeed = 2;
    private Vector3 dragOrigin;
    private Vector3 offset;
    public enum State
    {
        None,
        Dragging
    }

    // Use this for initialization
    void Start ()
    {
        offset = transform.position - GameObject.FindGameObjectWithTag("Map").transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (_State == State.None && Input.GetMouseButtonDown(0)) InitDrag();

        if (_State == State.Dragging && Input.GetMouseButton(0)) MoveCamera();

        if (_State == State.Dragging && Input.GetMouseButtonUp(0)) FinishDrag();
    }

    #region Calculations
    public State _State = State.None;

    private void InitDrag()
    {
        dragOrigin = Input.mousePosition;

        _State = State.Dragging;
    }

    private void MoveCamera()
    {

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(-pos.x * dragSpeed, -pos.y * dragSpeed, 0);

        transform.Translate(move, Space.World);
    }

    private void FinishDrag()
    {
        _State = State.None;
    }
    #endregion

    public void MoveCameraTo(Vector3 unitPosition)
    {
        transform.position = unitPosition + offset;
    }
}
