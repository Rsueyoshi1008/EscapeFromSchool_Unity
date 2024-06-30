using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;
namespace MVRP.Floor.managers
{
    public class FloorManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> floors;
        private int tmpFloorCount = 0;
        void Update()
        {
            
        }
        public void GetFloorCount(int value)
        {
            if(tmpFloorCount != value)
            {
                //  一個前のフロアのLayerを変更
                for(int i = 0; i < floors[tmpFloorCount].transform.childCount; i++)
                {
                    GameObject childObject = floors[tmpFloorCount].transform.GetChild(i).gameObject;
                    childObject.layer = LayerMask.NameToLayer("Default");
                }

                //  現在のフロアのLayerを変更
                for(int i = 0; i < floors[value].transform.childCount; i++)
                {
                    GameObject childObject = floors[value].transform.GetChild(i).gameObject;
                    childObject.layer = LayerMask.NameToLayer("TransparentFX");
                }
                //  今の階層を格納  
                tmpFloorCount = value;
            }
            
            
        }
    }
}

