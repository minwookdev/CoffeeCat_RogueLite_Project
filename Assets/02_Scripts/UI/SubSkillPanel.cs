using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoffeeCat.UI
{
    public class SubSkillPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject descriptionPanel = null;
        [SerializeField] private TextMeshProUGUI skillName = null;
        [SerializeField] private TextMeshProUGUI skillLevel = null;
        private bool getSkill = false;

        private void Start()
        {
            var rect = descriptionPanel.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-17f, 0f);
        }

        public void SetSkillInfo(PlayerSubAttackSkill skill)
        {
            getSkill = true;
            skillName.text = skill.SkillName;
            skillLevel.text = $"Lv.{skill.SkillLevel}";
        }
        public void SetSkillInfo(PlayerSubStatSkill skill)
        {
            getSkill = true;
            skillName.text = skill.SkillName;
            skillLevel.text = $"Lv.{skill.SkillLevel}";
        }
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (getSkill)
                descriptionPanel.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            descriptionPanel.SetActive(false);
        }
    }
}