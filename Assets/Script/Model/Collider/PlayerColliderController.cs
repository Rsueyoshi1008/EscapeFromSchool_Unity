using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace MVRP.ColliderEvents.Models
{
    public class PlayerColliderController : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<string> CollidedObjectName => _collidedObjectName;
        private readonly StringReactiveProperty _collidedObjectName = new StringReactiveProperty("0");
        void Start()
        {

        }
        void Update()
        {

        }
        void OnTriggerStay(Collider other)
        {
            //  当たったオブジェクトの名前を取得
            _collidedObjectName.Value = this.gameObject.name;
        }
        void OnTriggerExit(Collider other)
        {
            //  当たったオブジェクトの名前を取得
            _collidedObjectName.Value = "0";
        }
    }
}