using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    public class Player_Town : MonoBehaviour
    {
        [SerializeField] private PlayerAddressablesKey playerName = PlayerAddressablesKey.NONE;
        [SerializeField] private Transform tr = null;
        private PlayerStat stat;
        private Rigidbody2D rigid = null;

        private void Start()
        {
            rigid = GetComponent<Rigidbody2D>();
            stat = DataManager.Inst.PlayerStats.DataDictionary[playerName.ToKey()];
        }

        private void OnEnable()
        {
            InputManager.BindDirectionInputUpdateEvent(Move);
            InputManager.BindDirectionInputEndEvent(MoveEnd);
        }

        private void OnDisable()
        {
            InputManager.ReleaseDirectionInputUpdateEvent(Move);
            InputManager.ReleaseDirectionInputEndEvent(MoveEnd);
        }

        private void Move(Vector2 direction)
        {
            rigid.velocity = new Vector2(direction.x, direction.y) * stat.MoveSpeed;

            if (direction.x != 0f || direction.y != 0f)
            {
                SwitchingPlayerDirection(rigid.velocity.x < 0);
            }
        }

        private void MoveEnd()
        {
            rigid.velocity = Vector2.zero;
        }

        private void SwitchingPlayerDirection(bool isSwitching)
        {
            // Default Direction is Right
            // isSwitching : true -> Left, false -> Right
            var lossyScale = tr.lossyScale;
            tr.localScale = isSwitching switch
            {
                true  => new Vector3(-2f, lossyScale.y, lossyScale.z),
                false => new Vector3(2f, lossyScale.y, lossyScale.z)
            };
        }
    }
}