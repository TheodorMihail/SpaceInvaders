using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace Base.Systems
{
    /// <summary>
    /// Interface for Screen components that manage the full MVC lifecycle.
    /// Screens can be awaited to handle async UI flows and state transitions.
    /// </summary>
    public interface IScreen : IInitializable, IDisposable
    {
        const string ScreensContainerID = "ScreenContainer";

        UniTask WaitForClosure();
        void CloseScreen();
    }

    /// <summary>
    /// Screen interface that supports returning a result upon closure.
    /// </summary>
    public interface IScreenWithResult<TResult> : IScreen
        where TResult : IScreenResult
    {
        TResult GetResult();
        void SetResult(TResult result);
    }

    /// <summary>
    /// Screen interface that supports receiving typed parameters.
    /// Parameters are set before initialization.
    /// </summary>
    public interface IScreenWithParams<TParam> : IScreen
        where TParam : IScreenParam
    {
        void SetParameter(TParam parameter);
    }

    /// <summary>
    /// Marker interface for screen result data structures.
    /// </summary>
    public interface IScreenResult { }

    /// <summary>
    /// Marker interface for screen parameter data structures.
    /// </summary>
    public interface IScreenParam { }


    /// <summary>
    /// Base Screen class that automatically creates and manages Model, View, and Controller.
    /// View prefabs are loaded using the AddressablePathAttribute on the View class.
    /// Provides async/await support for handling UI flows and state transitions.
    /// </summary>
    public abstract class Screen<M, V, C> : IScreen
        where M : IModel
        where V : IView
        where C : IController
    {
        [Inject] private readonly IFactory _factory;
        [Inject] private readonly IAddressablesManager _addressablesManager;
        [Inject] private readonly IErrorManager _errorManager;
        [Inject] private readonly Transform _screenContainer;

        protected M _model;
        protected V _view;
        protected C _controller;

        private UniTaskCompletionSource _screenClosedTcs = new UniTaskCompletionSource();

        public async void Initialize()
        {
            await CreateMVC();

            if (_controller == null)
                return;

            _controller.Initialize();
        }

        public void Dispose()
        {
            if (_controller == null)
                return;

            _controller.Dispose();
        }

        public UniTask WaitForClosure()
        {
            return _screenClosedTcs.Task;
        }

        public void CloseScreen()
        {
            _screenClosedTcs.TrySetResult();
        }

        /// <summary>
        /// Creates Model, View, and Controller instances with automatic prefab loading.
        /// </summary>
        protected async UniTask CreateMVC()
        {
            try
            {
                var addressablePath = GetAddressablesPath<V>();
                var prefab = await _addressablesManager.LoadPrefab(addressablePath);

                if (prefab == null)
                {
                    await _errorManager.ShowErrorDialog($"Failed to load screen: {GetType().Name}");
                    CloseScreen();
                    return;
                }

                _view = _factory.CreateFromPrefab(prefab, _screenContainer).GetComponent<V>();
                _model = _factory.CreateNewObject<M>();
                _controller = _factory.CreateNewObject<C>(this, _model, _view);
                MVCCreated();
            }
            catch (Exception ex)
            {
                _errorManager.LogError($"Error creating screen {GetType().Name}", ex);
                await _errorManager.ShowErrorDialog("An error occurred while loading the screen.");
                CloseScreen();
            }
        }

        /// <summary>
        /// Called after MVC components are created and ready.
        /// Override to perform custom initialization logic.
        /// </summary>
        protected virtual void MVCCreated()
        {
        }

        private string GetAddressablesPath<T>() where T : IView
        {
            var attribute = typeof(T).GetCustomAttribute<AddressablePathAttribute>();
            return attribute?.Path;
        }
    }
}
