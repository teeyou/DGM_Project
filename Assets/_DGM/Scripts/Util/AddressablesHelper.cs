using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesHelper
{
    public static async UniTask<bool> UpdateCatalogsAsync()
    {
        // 카탈로그 업데이트 체크
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates();
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            Debug.Log("Catalog 업데이트 필요: " + string.Join(",", checkHandle.Result));

            // 실제 업데이트
            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
            await updateHandle.Task;

            if (updateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Catalog 업데이트 완료");
                Addressables.Release(updateHandle);
                return true;
            }
            else
            {
                Debug.LogError("Catalog 업데이트 실패");
                Addressables.Release(updateHandle);
                return false;
            }
        }
        else
        {
            Debug.Log("Catalog 최신 상태");
            Addressables.Release(checkHandle);
            return true;
        }
    }
}