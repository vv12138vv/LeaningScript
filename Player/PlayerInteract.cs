using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInteract : MonoBehaviour
{
    private GameObject _camera;
    [SerializeField]
    private float RayLength = 2f;
    [SerializeField]
    private LayerMask layerMask;

    private PlayerUI _playerUI;
    private InputManager _input;

    private void Awake()
    {

    }

    private void Start()
    {
        _camera = GetComponent<PlayerController>()._mainCamera;
        _playerUI = GetComponent<PlayerUI>();
        _input = GetComponent<InputManager>();
    }

    private void Update()
    {   
        _playerUI.UpdateText(string.Empty);
        //create a ray at the center of the camera,shooting outwards
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        Debug.DrawRay(ray.origin,ray.direction*RayLength,Color.green);
        RaycastHit hitInfo;//use to store collision info
        if (Physics.Raycast(ray, out hitInfo, RayLength, layerMask))
        {
            if (hitInfo.collider.GetComponent<Interactable>() != null)
            {
                Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
                _playerUI.UpdateText(interactable.promptMessage);
                if (_input.isInteract)
                {
                    interactable.BaseInteract();
                }
            }
        }
        
    }
}
