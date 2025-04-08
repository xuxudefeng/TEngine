using System.Collections.Generic;
using System.Linq;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    [CustomEditor(typeof(UpdateSetting), true)]
    public class UpdateSettingEditor : UnityEditor.Editor
    {
        public List<string> HotUpdateAssemblies = new() {};
        public List<string> AOTMetaAssemblies = new() {};
        
        private void OnEnable()
        {
            // 获取当前编辑的 ScriptableObject 实例
            UpdateSetting updateSetting = (UpdateSetting)target;
            if (updateSetting != null)
            {
                HotUpdateAssemblies.AddRange(updateSetting.HotUpdateAssemblies);
                AOTMetaAssemblies.AddRange(updateSetting.AOTMetaAssemblies);
            }
        }

        public override void OnInspectorGUI()
        {
            // 记录对象修改前的状态
            EditorGUI.BeginChangeCheck();

            // 绘制默认的 Inspector 界面
            base.OnInspectorGUI();

            // 检测是否有字段被修改
            if (EditorGUI.EndChangeCheck())
            {
                // 获取当前编辑的 ScriptableObject 实例
                UpdateSetting updateSetting = (UpdateSetting)target;

                // 标记对象为“已修改”，确保修改能被保存
                EditorUtility.SetDirty(updateSetting);
                
                bool isHotChanged = HotUpdateAssemblies.SequenceEqual(updateSetting.HotUpdateAssemblies);
                bool isAOTChanged = AOTMetaAssemblies.SequenceEqual(updateSetting.AOTMetaAssemblies);
                if (isHotChanged)
                {
                    HybridCLRSettings.Instance.hotUpdateAssemblies = updateSetting.HotUpdateAssemblies.ToArray();
                    Debug.Log("HotUpdateAssemblies changed");
                }
                if (isAOTChanged)
                {
                    HybridCLRSettings.Instance.patchAOTAssemblies = updateSetting.AOTMetaAssemblies.ToArray();
                    Debug.Log("AOTMetaAssemblies changed");
                }
            }
        }
    }
}