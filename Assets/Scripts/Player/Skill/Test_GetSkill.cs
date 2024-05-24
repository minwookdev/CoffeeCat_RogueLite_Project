using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace CoffeeCat
{
    public class Test_GetSkill : MonoBehaviour
    {
        [SerializeField] private PlayerSkillsKey skillKey = PlayerSkillsKey.NONE;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Player player))
            {
                player.Skill = skillKey;
                gameObject.SetActive(false);
            }
        }
    }
}