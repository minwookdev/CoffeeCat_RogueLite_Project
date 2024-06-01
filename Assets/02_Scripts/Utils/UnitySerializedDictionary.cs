using System;
using System.Collections.Generic;
using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Datas;

namespace CoffeeCat.Utils.SerializedDictionaries { 
    public abstract class UnitySerializedDictionary<Tkey, TValue> : Dictionary<Tkey, TValue>, ISerializationCallbackReceiver {
        [SerializeField, HideInInspector] private List<Tkey> keyData = new List<Tkey>();
        [SerializeField, HideInInspector] private List<TValue> valueData = new List<TValue>();
    
        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            this.Clear();
            for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++) {
                this[this.keyData[i]] = this.valueData[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            this.keyData.Clear();
            this.valueData.Clear();

            foreach (KeyValuePair<Tkey, TValue> keyValuePair in this) {
                this.keyData.Add(keyValuePair.Key);
                this.valueData.Add(keyValuePair.Value);
            }
        }
    }

    // MANAGERS INFORMATION DICTIONARIES
    [Serializable] public class StringPoolInformationDictionary : UnitySerializedDictionary<string, PoolInformation> { }
    [Serializable] public class StringTransformDictionary : UnitySerializedDictionary<string, Transform> { }
    [Serializable] public class StringGameObjectStackDictionary : UnitySerializedDictionary<string, Stack<GameObject>> { }
    [Serializable] public class StringIntDictionary : UnitySerializedDictionary<string, int> { }
    [Serializable] public class StringResourceInformationDictionary : UnitySerializedDictionary<string, ResourceManager.ResourceInformation> { }
    [Serializable] public class StringEffectInformationDictionary : UnitySerializedDictionary<string, EffectManager.EffectInfo> { }
    [Serializable] public class StringAudioClipDictionary : UnitySerializedDictionary<string, AudioClip> { }
    [Serializable] public class StringAudioSourceDictionary : UnitySerializedDictionary <string, AudioSource> { }   

    // DATA DICTIONARIES
    [Serializable] public class StringMonsterStatDictionary : UnitySerializedDictionary<string, MonsterStat> { }
    [Serializable] public class StringMonsterSkillDictionary : UnitySerializedDictionary<string, MonsterSkillStat> { }
    [Serializable] public class StringPlayerStatDictionary : UnitySerializedDictionary<string, PlayerStat> { }
    [Serializable] public class IntPlayerActiveSkillDictionary : UnitySerializedDictionary<int, PlayerSkill> { }
    [Serializable] public class IntPlayerPassiveSkillDictionary : UnitySerializedDictionary<int, PlayerSkill> { }
}
