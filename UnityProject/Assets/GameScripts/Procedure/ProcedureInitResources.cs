using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Launcher;
using TEngine;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureModule>;

namespace Procedure
{
    public class ProcedureInitResources : ProcedureBase
    {
        private bool _initResourcesComplete = false;

        public override bool UseNativeDialog => true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _initResourcesComplete = false;

            LauncherMgr.Show(UIDefine.UILoadUpdate, "初始化资源中...");

            // 注意：使用单机模式并初始化资源前，需要先构建 AssetBundle 并复制到 StreamingAssets 中，否则会产生 HTTP 404 错误
            ModuleSystem.GetModule<IUpdateDriver>().StartCoroutine(InitResources(procedureOwner));
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!_initResourcesComplete)
            {
                // 初始化资源未完成则继续等待
                return;
            }

            if ((_resourceModule.PlayMode == EPlayMode.HostPlayMode || _resourceModule.PlayMode == EPlayMode.WebPlayMode))
            {
                //线上最新版本operation.PackageVersion
                Log.Debug($"Updated package Version : from {_resourceModule.GetPackageVersion()} to {_resourceModule.PackageVersion}");
                ChangeState<ProcedureUpdateManifest>(procedureOwner);
                return;
            }

            ChangeState<ProcedurePreload>(procedureOwner);
        }

        /// <summary>
        /// 编辑器与单机保持相同流程。
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitResources(ProcedureOwner procedureOwner)
        {
            string packageVersion;
            
            // 1. 获取资源清单的版本信息
            var operation1 = _resourceModule.RequestPackageVersionAsync();
            yield return operation1;
            if (operation1.Status != EOperationStatus.Succeed)
            {
                OnInitResourcesError(procedureOwner, operation1.Error);
                yield break;
            }

            packageVersion = operation1.PackageVersion;
            _resourceModule.PackageVersion = packageVersion;
            
            Log.Info($"Init resource package version : {packageVersion}");
            
            // 2. 传入的版本信息更新资源清单
            var operation2 = _resourceModule.UpdatePackageManifestAsync(packageVersion);
            yield return operation2;
            if (operation2.Status != EOperationStatus.Succeed)
            {
                OnInitResourcesError(procedureOwner, operation2.Error);
                yield break;
            }
            
            _initResourcesComplete = true;
        }
        
        private void OnInitResourcesError(ProcedureOwner procedureOwner, string message)
        {
            Log.Error(message);
            LauncherMgr.ShowMessageBox($"初始化资源失败！点击确认重试 \n <color=#FF0000>{message}</color>", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Retry
                , () =>
                {
                    ModuleSystem.GetModule<IUpdateDriver>().StartCoroutine(InitResources(procedureOwner));
                }, UnityEngine.Application.Quit);
        }
    }
}