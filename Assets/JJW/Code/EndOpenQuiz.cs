using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace JJW.Code
{
    public class EndOpenQuiz : MonoBehaviour
    {
        [SerializeField] private Image endPanel;

        private void Start()
        {
            endPanel.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                endPanel.enabled = true;
                other.gameObject.SetActive(false);
            }
        }
    } 
}


