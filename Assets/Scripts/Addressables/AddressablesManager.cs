using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesManager
{
    /// <summary>
    /// Carga un asset a través del sistema Addressables y devuelve el resultado al callback proporcionado.
    /// </summary>
    /// <typeparam name="T">Tipo de asset a cargar</typeparam>
    /// <param name="address">Dirección del asset en el sistema Addressables</param>
    /// <param name="onLoaded">Callback que se invoca cuando el asset se carga correctamente</param>
    /// <param name="onFailed">Callback opcional que se invoca cuando la carga falla</param>
    /// <returns>El handle de la operación asíncrona</returns>
    public static AsyncOperationHandle<T> LoadAsset<T>(
        string address,
        Action<T> onLoaded,
        Action<string> onFailed = null) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("La dirección del asset no puede ser nula o vacía");
            onFailed?.Invoke("Dirección de asset inválida");
            return default;
        }

        var handle = Addressables.LoadAssetAsync<T>(address);

        handle.Completed += operation =>
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                onLoaded?.Invoke(operation.Result);
            }
            else
            {
                string errorMessage = $"No se pudo cargar el asset: {address}. Error: {operation.OperationException?.Message}";
                Debug.LogWarning(errorMessage);
                onFailed?.Invoke(errorMessage);
            }
        };

        return handle;
    }

    /// <summary>
    /// Libera un asset cargado previamente con Addressables
    /// </summary>
    /// <param name="handle">Handle de la operación de carga</param>
    public static void ReleaseAsset<T>(AsyncOperationHandle<T> handle)
    {
        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }
    }
}