using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.Tutorial
{
    public enum InputBlockMode
    {
        Auto,
        AlwaysBlock,
        NeverBlock,
    }

    [CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/TutorialStep")]
    public class TutorialStepData : ScriptableObject
    {
        [TextArea]
        public string StepName;
        [TextArea(1, 6)]
        [Tooltip("Текст подсказки, отображаемый игроку")]
        public string InstructionText;

        [Tooltip("Ключ локализации. Если задан, текст берётся из LocalizationConfig по этому ключу")]
        public string LocalizationKey;

        [Tooltip("Список UI-элементов, которые будут подсвечены (незатемнены)")]
        public List<TutorialHighlightTarget> Highlights = new();

        [Tooltip("Блокировать ввод за пределами выделенных зон.\nАвто: блокировка включается, если есть хотя бы один Highlight.")]
        public InputBlockMode BlockInput = InputBlockMode.Auto;

        [Tooltip("Условие перехода к следующему шагу")]
        public StepTrigger Trigger = StepTrigger.ManualButton;

        [Tooltip("Сколько раз должен сработать Trigger, чтобы перейти к следующему шагу")]
        public int TriggerCount = 1;

        [Header("Wait Condition")]
        [Tooltip("Событие, которое должно произойти ПЕРЕД показом этого шага.\nNone — шаг показывается сразу.")]
        public StepTrigger WaitCondition = StepTrigger.None;

        [Tooltip("Сколько раз должен сработать WaitCondition, чтобы шаг стал виден")]
        public int WaitCount = 1;

        [Header("Wave")]
        [Tooltip("Принудительно запустить следующую волну врагов при старте этого шага.")]
        public bool SpawnWaveOnStart;

        [Tooltip("Принудительно запустить следующую волну врагов после завершения этого шага.")]
        public bool SpawnWaveOnComplete;

        [Header("Text Position")]
        [Tooltip("Позиция панели с текстом подсказки")]
        public TextPanelPosition TextPosition = TextPanelPosition.BottomCenter;

        [Tooltip("Нормализованная позиция (0..1) при TextPosition = Custom.\n(0,0) — левый нижний угол, (1,1) — правый верхний.")]
        public Vector2 CustomTextAnchor = new(0.5f, 0.1f);
    }
}
