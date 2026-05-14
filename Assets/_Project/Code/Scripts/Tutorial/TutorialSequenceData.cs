using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialSequence", menuName = "Tutorial/TutorialSequence")]
    public class TutorialSequenceData : ScriptableObject
    {
        public bool DisableBuilding;

        public bool DisableUpgrades;

        public List<TutorialStepData> Steps = new();
    }
}
