using UnityEngine;
using UnityEngine.Events;

namespace MVRP.RayCast.Models
{
    public class RayCastManager : MonoBehaviour
    {
        public float rayDistance;
        public UnityAction<RaycastHit> managerRayCast_Name;
        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit,rayDistance))
            {
                //  Castしたオブジェクト名  //
                managerRayCast_Name?.Invoke(hit);
                
            }
            else  managerRayCast_Name?.Invoke(hit);
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
            
        }
    }
}

