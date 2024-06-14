using UnityEngine;
namespace MVRP.Students.Views
{
    public class StudentView : MonoBehaviour
    {
        
        private Canvas studentCanvas;

        void Start()
        {
            studentCanvas = GetComponentInChildren<Canvas>();
            if(studentCanvas == null)
            {
                Debug.LogError("Player Transform is not assigned.");
                return;
            }
            studentCanvas.gameObject.SetActive(false);
        }
        public void IndicationCanvas()
        {
            studentCanvas.gameObject.SetActive(true);
            
        }
        public void HideCanvas()
        {
            studentCanvas.gameObject.SetActive(false);
        }
    }
}

