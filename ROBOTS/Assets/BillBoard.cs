using System.Collections;
using System.Collections.Generic;
using elZach.Common;
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
        static Transform cam{get
        {
            if (!_camera) _camera = Camera.main.transform;
            return _camera;
        }}

        private List<BillBoard> billBoards = new List<BillBoard>();
        static bool quitting;
        void OnEnable()
        {
            Application.quitting += () => quitting = true;
        }
        
        void Update()
        {
            // var camDirection = camera.forward;
            // camDirection.y = 0f;
            // camDirection = camDirection.normalized;
            var camPosition = cam.position;
            foreach (var billBoard in billBoards)
            {
                var dir = (billBoard.transform.position - camPosition).normalized;
                //dir = dir.XZDirection();
                billBoard.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }

        public static void Register(BillBoard billBoard)
        {
            if (quitting) return;
            Instance.billBoards.Add(billBoard);
        }

        public static void Deregister(BillBoard billBoard)
        {
            if (quitting) return;
            Instance.billBoards.Remove(billBoard);
        }

    }
}
