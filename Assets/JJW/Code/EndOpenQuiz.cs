using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JJW.Code
{
    public class EndOpenQuiz : MonoBehaviour
    {
        [SerializeField] private Image endPanel;
        [SerializeField] private Button _oButton;
        [SerializeField] private Button _xButton;
        [SerializeField] private TextMeshProUGUI _problemText;

        private void Start()
        {
            endPanel.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                endPanel.enabled = true;
                _oButton.gameObject.SetActive(true);
                _xButton.gameObject.SetActive(true);
                _problemText.gameObject.SetActive(true);
                other.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            endPanel.enabled = false;
            _oButton.gameObject.SetActive(false);
            _xButton.gameObject.SetActive(false);
            _problemText.gameObject.SetActive(false);
        }
    } 
}


