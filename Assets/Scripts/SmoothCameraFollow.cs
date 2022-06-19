using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed;
    [NonSerialized] public Rect CameraBounds = Rect.zero;
    [SerializeField] private new Camera camera;

    private float _z;
    private Transform _playerTransform;
    private void Start()
    {
        _playerTransform = Player.Instance.transform;
        _z = transform.position.z;
    }

    private void Update()
    {
        var newPos = Vector2.Lerp(transform.position, _playerTransform.position, followSpeed * Time.deltaTime);
        if (CameraBounds != Rect.zero)
        {
            var cameraRect = camera.pixelRect;
            var cameraTopRight = camera.ScreenToWorldPoint(new Vector2(cameraRect.xMax, cameraRect.yMax));
            var cameraBottomLeft = camera.ScreenToWorldPoint(Vector3.zero);
            var cameraWidthInWorldCoords = cameraTopRight.x - cameraBottomLeft.x;
            var cameraHeightInWorldCoords = cameraTopRight.y - cameraBottomLeft.y;
            
            newPos.x = Mathf.Clamp(newPos.x, CameraBounds.x + cameraWidthInWorldCoords / 2, CameraBounds.x + CameraBounds.width - cameraWidthInWorldCoords / 2);
            newPos.y = Mathf.Clamp(newPos.y, CameraBounds.y + cameraHeightInWorldCoords / 2, CameraBounds.y + CameraBounds.height - cameraHeightInWorldCoords / 2);
        }

        transform.position = new Vector3(newPos.x, newPos.y, _z);
    }
}
