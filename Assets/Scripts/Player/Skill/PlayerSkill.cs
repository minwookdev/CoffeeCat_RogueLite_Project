using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkill : MonoBehaviour
    {
        [SerializeField, ShowInInspector] private Table_PlayerSkills skillInfo = null;
        public Table_PlayerSkills SkillInfo => skillInfo;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Player player))
            {
                CatLog.Log("OnTriggerEnter2D : Player");
                player.Skill = this;
                gameObject.SetActive(false);
            }
        }
    }
}