using System;
using System.Collections;
using System.Collections.Generic;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;

namespace Reflex.Components
{
    [DefaultExecutionOrder(int.MinValue + 1000)]
    internal sealed class SelfInjector : MonoBehaviour
    {
        [SerializeField] private InjectionStrategy _injectionStrategy = InjectionStrategy.Recursive;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.1f);
            var container = gameObject.scene.GetSceneContainer();
            InjectContainer(container);

            //List<Core.Container> containers = new List<Core.Container>();
            //Debug.Log("Scene count " + UnityEngine.SceneManagement.SceneManager.sceneCount);
            ////get all active scenes
            //for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i ++)
            //{
            //    var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            //    if (scene.isLoaded)
            //    {
            //        Debug.Log("Retrieving container for Scene " + scene.name);
            //        var container = scene.GetSceneContainer();
            //        if (container != null)
            //        {
            //            InjectContainer(container);
            //        }
            //    }
            //}
        }

        void InjectContainer(Core.Container sceneContainer)
        {
            switch (_injectionStrategy)
            {
                case InjectionStrategy.Single:
                    GameObjectInjector.InjectSingle(gameObject, sceneContainer);
                    break;
                case InjectionStrategy.Object:
                    GameObjectInjector.InjectObject(gameObject, sceneContainer);
                    break;
                case InjectionStrategy.Recursive:
                    GameObjectInjector.InjectRecursive(gameObject, sceneContainer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(_injectionStrategy.ToString());
            }
        }

        private enum InjectionStrategy
        {
            Single,
            Object,
            Recursive
        }
    }
}