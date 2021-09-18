using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    void OnEnable() => Manager.Register(this);
    void OnDisable() => Manager.Deregister(this);
    
    
    public class Manager : MonoBehaviour
    {
        static Manager _instance;
        public static Manager Instance
        {
            get
            {
                if (!_instance) _instance = new GameObject("BillBoarder", typeof(Manager)).GetComponent<Manager>();
                return _instance;
            }
        }

        private static Transform _camera;
        static Transform camera{get
        {
            if (!_camera) _camera = Camera.main.transform;
            return _camera;
        }}

        private List<BillBoard> billBoards = new List<BillBoard>();

        void Update()
        {
            var camDirection = camera.forward;
            camDirection.y = 0f;
            camDirection = camDirection.normalized;
            foreach (var billBoard in billBoards)
            {
                billBoard.transform.rotation = Quaternion.LookRotation(camDirection, Vector3.up);
            }
        }

        public static void Register(BillBoard billBoard)
        {
            Instance.billBoards.Add(billBoard);
        }

        public static void Deregister(BillBoard billBoard)
        {
            Instance.billBoards.Remove(billBoard);
        }

    }
}
