using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AutoTeleportSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitializeTeleport()
    {
        // Находим XR Origin
        var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin == null) return;

        // 0. Добавляем CharacterController и Capsule/CharacterControllerDriver, чтобы игрок не проходил сквозь стены
        var charController = xrOrigin.GetComponent<CharacterController>();
        if (charController == null)
        {
            charController = xrOrigin.gameObject.AddComponent<CharacterController>();
            charController.height = 1.8f;
            charController.radius = 0.3f;
            charController.center = new Vector3(0, 0.9f, 0);
            Debug.Log("AutoTeleportSetup: Added CharacterController to XR Origin.");
        }

        // Также добавляем Rigidbody с залоченными вращениями, чтобы физика стен толкала CharacterController
        var rb = xrOrigin.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = xrOrigin.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 1. Проверяем TeleportationProvider на XR Origin
        var provider = xrOrigin.GetComponent<TeleportationProvider>();
        if (provider == null)
        {
            provider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            Debug.Log("AutoTeleportSetup: Added TeleportationProvider to XR Origin.");
        }

        // 2. Ищем все TeleportationArea на сцене и привязываем провайдер
        var areas = FindObjectsOfType<TeleportationArea>();
        foreach (var area in areas)
        {
            if (area.teleportationProvider == null)
            {
                area.teleportationProvider = provider;
            }
        }

        // 3. Ищем все TeleportationAnchor на сцене и привязываем провайдер
        var anchors = FindObjectsOfType<TeleportationAnchor>();
        foreach (var anchor in anchors)
        {
            if (anchor.teleportationProvider == null)
            {
                anchor.teleportationProvider = provider;
            }
        }

        // 4. Убеждаемся, что на контроллерах/интеракторах настроен Ray Interactor для телепортации
        var rayInteractors = FindObjectsOfType<XRRayInteractor>();
        foreach (var ray in rayInteractors)
        {
            ray.enabled = true;
        }

        Debug.Log("AutoTeleportSetup: Teleportation & Collision system configured successfully!");
    }
}


