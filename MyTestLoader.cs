// -----------------------------------------------------------------------
//  <copyright file="MyTestLoader.cs" company="Tencent">
//  Copyright (C) Tencent. All Rights Reserved.
//  </copyright>
//  <author>felixhou(后轩)</author>
//  <summary></summary>
// -----------------------------------------------------------------------
namespace Assets.AssetBundleSample.Scripts
{
    using UnityEngine;

    using System.Collections.Generic;

    public class MyTestLoader : MonoBehaviour
    {
        public enum LoadingProgress
        {
            None,
            Running,
            Finish
        }

        private AssetBundleManifest rootManifest;

        private string rootPath;

        // 异步加载测试
        private Dictionary<string, AssetBundleCreateRequest> m_dictABLoadRequest;

        private AssetBundleCreateRequest tankCreateRequest;
        private LoadingProgress m_eAsyncLoadFinish = LoadingProgress.None;

        private bool LoadMainfest()
        {
            string manifest = rootPath + "Windows";

            AssetBundle ab = AssetBundle.LoadFromFile(manifest);
            if (ab == null)
            {
                return false;
            }

            rootManifest = (AssetBundleManifest)ab.LoadAsset("AssetBundleManifest");
            return true;
        }

        void Start()
        {
            rootPath = Application.dataPath + "/../AssetBundles/Windows/";

            LoadMainfest();

            m_dictABLoadRequest = new Dictionary<string, AssetBundleCreateRequest>();

            //this.LoadTank();
            this.LoadTankAsync();
        }

        private void LoadTank()
        {
            string tankModel = "tankgo";

            AssetBundle ab = LoadAssetFromBundle(tankModel, true);
            GameObject goTank = ab.LoadAsset<GameObject>("TankGo");
            GameObject.Instantiate(goTank);
        }

        private void LoadTankAsync()
        {
            string tankModel = "tankgo";

            this.m_eAsyncLoadFinish = LoadingProgress.Running;
            tankCreateRequest = LoadAssetFromBundleAsync(tankModel, true);
        }

        void Update()
        {
            if (this.m_eAsyncLoadFinish == LoadingProgress.Running)
            {
                if (m_dictABLoadRequest.Count > 0)
                {
                    Dictionary<string, AssetBundleCreateRequest>.Enumerator iter = m_dictABLoadRequest.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        if (iter.Current.Value.isDone)
                        {
                            m_dictABLoadRequest.Remove(iter.Current.Key);
                            break;
                        }
                    }
                }
                else
                {
                    this.m_eAsyncLoadFinish = LoadingProgress.Finish;
                }
            }

            if (this.m_eAsyncLoadFinish == LoadingProgress.Finish)
            {
                this.m_eAsyncLoadFinish = LoadingProgress.None;

                OnTankAsyncLoadingFinish();
            }
        }

        private void OnTankAsyncLoadingFinish()
        {
            AssetBundle bundle = tankCreateRequest.assetBundle;

            GameObject goTank = bundle.LoadAsset<GameObject>("TankGo");
            GameObject.Instantiate(goTank);
        }

        private AssetBundleCreateRequest LoadAssetFromBundleAsync(string assetBundleName, bool bLoadDepends)
        {
            if (assetBundleName == null)
            {
                return null;
            }

            string fullPath = rootPath + assetBundleName;
            AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync(fullPath);
            if (createRequest == null)
            {
                return null;
            }
            m_dictABLoadRequest[assetBundleName] = createRequest;

            if (bLoadDepends)
            {
                string[] dependsPath = rootManifest.GetAllDependencies(assetBundleName);
                for (int i = 0; i < dependsPath.Length; i++)
                {
                    this.LoadAssetFromBundleAsync(dependsPath[i], false);
                }
            }
            return createRequest;
        }

        private AssetBundle LoadAssetFromBundle(string assetBundleName, bool bLoadDepends)
        {
            if (assetBundleName == null)
            {
                return null;
            }

            string fullPath = rootPath + assetBundleName;

            AssetBundle ab = AssetBundle.LoadFromFile(fullPath);

            if (bLoadDepends)
            {
                string[] dependsPath = rootManifest.GetAllDependencies(assetBundleName);
                for (int i = 0; i < dependsPath.Length; i++)
                {
                    LoadAssetFromBundle(dependsPath[i], false);
                }
            }
            
            return ab;
        }
    }
}