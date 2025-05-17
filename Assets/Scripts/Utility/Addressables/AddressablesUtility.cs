using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UtilityAddressables
{
    public static class AddressablesUtility
    {
        /// <summary>
        /// Carga un asset a través del sistema Addressables y devuelve el resultado al callback proporcionado.
        /// Verifica primero si la dirección es válida pero confía en el sistema Addressables para validar la existencia.
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
                string errorMessage = "La dirección del asset no puede ser nula o vacía";
                Debug.LogWarning(errorMessage);
                onFailed?.Invoke(errorMessage);
                return default;
            }

            try
            {
                // Intentamos cargar directamente y dejamos que el sistema de Addressables maneje los errores
                var handle = Addressables.LoadAssetAsync<T>(address);

                handle.Completed += operation =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                        onLoaded?.Invoke(operation.Result);
                    else
                    {
                        string errorMessage = $"No se pudo cargar el asset: {address}. Error: {operation.OperationException?.Message}";
                        Debug.LogWarning(errorMessage);
                        onFailed?.Invoke(errorMessage);
                    }
                };

                return handle;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error al intentar cargar el asset: {address}. Exception: {ex.Message}";
                Debug.LogWarning(errorMessage);
                onFailed?.Invoke(errorMessage);
                return default;
            }
        }

        /// <summary>
        /// Método alternativo que verifica la existencia del addressable antes de cargarlo.
        /// </summary>
        public static void CheckThenLoadAsset<T>(
            string address,
            Action<T> onLoaded,
            Action<string> onFailed = null) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
            {
                string errorMessage = "La dirección del asset no puede ser nula o vacía";
                Debug.LogWarning(errorMessage);
                onFailed?.Invoke(errorMessage);
                return;
            }

            // Primero verificamos si el addressable existe
            Addressables.LoadResourceLocationsAsync(address).Completed += locationsHandle =>
            {
                try
                {
                    if (locationsHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        var locations = locationsHandle.Result;
                        if (locations != null && locations.Count > 0)
                        {
                            // Si existe, procedemos a cargarlo
                            var loadHandle = Addressables.LoadAssetAsync<T>(address);

                            loadHandle.Completed += operation =>
                            {
                                if (operation.Status == AsyncOperationStatus.Succeeded)
                                    onLoaded?.Invoke(operation.Result);
                                else
                                {
                                    string errorMessage = $"No se pudo cargar el asset: {address}. Error: {operation.OperationException?.Message}";
                                    Debug.LogWarning(errorMessage);
                                    onFailed?.Invoke(errorMessage);
                                }
                            };
                        }
                        else
                        {
                            string errorMessage = $"El addressable '{address}' no existe en ningún grupo";
                            Debug.LogWarning(errorMessage);
                            onFailed?.Invoke(errorMessage);
                        }
                    }
                    else
                    {
                        string errorMessage = $"Error al verificar la existencia del addressable: {address}";
                        Debug.LogWarning(errorMessage);
                        onFailed?.Invoke(errorMessage);
                    }

                    // Liberamos el handle de verificación de ubicación
                    Addressables.Release(locationsHandle);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Excepción al verificar/cargar el addressable: {address}. Error: {ex.Message}";
                    Debug.LogWarning(errorMessage);
                    onFailed?.Invoke(errorMessage);

                    // Aseguramos que el handle se libere incluso en caso de error
                    ReleaseAsset(locationsHandle);
                }
            };
        }

        /// <summary>
        /// Libera un asset cargado previamente con Addressables
        /// </summary>
        /// <param name="handle">Handle de la operación de carga</param>
        public static void ReleaseAsset<T>(AsyncOperationHandle<T> handle)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
    }
}