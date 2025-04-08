using System;
using Cysharp.Threading.Tasks;
using Launcher;
using UnityEngine;
using TEngine;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureModule>;

namespace Procedure
{
    /// <summary>
    /// 流程 => 用户尝试更新静态版本
    /// </summary>
    public class ProcedureUpdateVersion : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        private ProcedureOwner _procedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            _procedureOwner = procedureOwner;

            base.OnEnter(procedureOwner);

            LauncherMgr.Show(UIDefine.UILoadUpdate, $"更新静态版本文件...");

            // 检查设备是否能够访问互联网。
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (!IsNeedUpdate())
                {
                    return;
                }
                else
                {
                    Log.Error("The device is not connected to the network");
                    LauncherMgr.ShowMessageBox(LoadText.Instance.Label_Net_UnReachable, MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_Retry,
                        Application.Quit);
                    return;
                }
            }

            LauncherMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_RequestVersionIng);

            // 用户尝试更新静态版本。
            GetStaticVersion().Forget();
        }

        /// <summary>
        /// 向用户尝试更新静态版本。
        /// </summary>
        private async UniTaskVoid GetStaticVersion()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            var operation = _resourceModule.RequestPackageVersionAsync();

            try
            {
                await operation.ToUniTask();

                if (operation.Status == EOperationStatus.Succeed)
                {
                    //线上最新版本operation.PackageVersion
                    _resourceModule.PackageVersion = operation.PackageVersion;
                    Log.Debug($"Updated package Version : from {_resourceModule.GetPackageVersion()} to {operation.PackageVersion}");
                    ChangeState<ProcedureUpdateManifest>(_procedureOwner);
                }
                else
                {
                    OnGetStaticVersionError(operation.Error);
                }
            }
            catch (Exception e)
            {
                OnGetStaticVersionError(e.Message);
            }
        }

        private void OnGetStaticVersionError(string error)
        {
            Log.Error(error);

            if (!IsNeedUpdate())
            {
                return;
            }

            LauncherMgr.ShowMessageBox($"用户尝试更新静态版本失败！点击确认重试 \n \n <color=#FF0000>原因{error}</color>", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Retry
                , () => { ChangeState<ProcedureUpdateVersion>(_procedureOwner); }, UnityEngine.Application.Quit);
        }

        private bool IsNeedUpdate()
        {
            // 如果不能联网且当前游戏非强制(不更新可以进入游戏。)
            if (Settings.UpdateSetting.UpdateStyle == UpdateStyle.Optional)
            {
                Log.Warning("The device is not connected to the network");
                    
                // 获取上次成功记录的版本
                string packageVersion = PlayerPrefs.GetString("GAME_VERSION", string.Empty);
                if(string.IsNullOrEmpty(packageVersion))
                {
                    LauncherMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_Net_UnReachable);
                    LauncherMgr.ShowMessageBox("没有找到本地版本记录，需要更新资源！", MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_Retry,
                        GetStaticVersion().Forget,
                        Application.Quit);
                    return false;
                }
                    
                _resourceModule.PackageVersion = packageVersion;
                    
                LauncherMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_Net_UnReachable);
                LauncherMgr.ShowMessageBox(LoadText.Instance.Label_Net_UnReachable, MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry,
                    GetStaticVersion().Forget,
                    () => { ChangeState<ProcedureInitResources>(_procedureOwner); });
                return false;
            }
            return true;
        }
    }
}