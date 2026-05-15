using UnityEngine;

namespace _Project.Code.Scripts.Tutorial
{
    public class TutorialTarget : MonoBehaviour
    {
        [Tooltip("Уникальный ID, указываемый в поле TargetId у TutorialHighlightTarget.")]
        [SerializeField] private string _id;

        public string Id => _id;
    }
}
