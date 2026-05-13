using System;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data.TaskData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.TaskSystem
{
    public class TaskSystemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _goalText;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Color _finalIconColor;
        [SerializeField] private TMP_Text _rewardText;
        [SerializeField] private Image _valuteIcon;
        private TaskIconConfig _iconConfig;

        private ITaskService _taskService;
        private Color _defaultIconColor;

        public void ManualAwake(ITaskService taskService, TaskIconConfig iconConfig)
        {
            _taskService = taskService;
            _iconConfig = iconConfig;
            _defaultIconColor = _iconImage.color;
            _taskService.OnTaskStarted += UpdateView;
            _taskService.OnTaskCompleted += OnTaskCompleted;
            _taskService.OnAllTasksCompleted += MakeFinalView;

            if (_taskService.HasActiveTask)
                UpdateView(_taskService.CurrentTask.Value);
            else
                ClearView();
        }

        private void MakeFinalView()
        {
            ClearView();
            _goalText.text = $"{_taskService.CompletedTasksCount}/{_taskService.GoalTaskIndex}";
            _goalText.enabled = true;
            _valuteIcon.gameObject.SetActive(false);
            _iconImage.sprite = _iconConfig.FinalIcon;
            _iconImage.color = _finalIconColor;
            _iconImage.enabled = true;
            _titleText.text = "---";
            _titleText.enabled = true;
        }

        private void OnDestroy()
        {
            if (_taskService != null)
            {
                _taskService.OnTaskStarted -= UpdateView;
                _taskService.OnTaskCompleted -= OnTaskCompleted;
                _taskService.OnAllTasksCompleted -= MakeFinalView;
            }
        }

        private void UpdateView(TaskData task)
        {
            _iconImage.color = _defaultIconColor;
            _titleText.enabled = true;
            _titleText.text = "Order: " + task.ResultType.ToDisplayString();

            var icon = _iconConfig.GetIcon(task.ResultType);
            _iconImage.sprite = icon;
            _iconImage.enabled = icon != null;

            _valuteIcon.gameObject.SetActive(true);

            _rewardText.enabled = true;
            _rewardText.text = task.CreditReward.ToString();

            _goalText.enabled = true;
            _goalText.text = $"Goal: {_taskService.CompletedTasksCount}/{_taskService.GoalTaskIndex}";

        }

        private void OnTaskCompleted(TaskData _)
        {
            if (!_taskService.HasActiveTask)
                ClearView();
        }

        private void ClearView()
        {
            _titleText.text = string.Empty;
            _titleText.enabled = false;
            _iconImage.sprite = null;
            _iconImage.enabled = false;
            _rewardText.text = string.Empty;
            _rewardText.enabled = false;
            _goalText.text = string.Empty;
            _goalText.enabled = false;
        }
    }
}