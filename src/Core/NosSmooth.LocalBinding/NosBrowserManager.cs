//
//  NosBrowserManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Options;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding;

/// <summary>
/// Used for browsing a nostale process data.
/// </summary>
public class NosBrowserManager
{
    /// <summary>
    /// Checks whether the given process is a NosTale client process.
    /// </summary>
    /// <remarks>
    /// This is just a guess based on presence of "NostaleData" directory.
    /// </remarks>
    /// <param name="process">The process to check.</param>
    /// <returns>Whether the process is a NosTale client.</returns>
    public static bool IsProcessNostaleProcess(Process process)
    {
        if (process.MainModule is null)
        {
            return false;
        }

        var processDirectory = Path.GetDirectoryName(process.MainModule.FileName);
        if (processDirectory is null)
        {
            return false;
        }

        return Directory.Exists(Path.Combine(processDirectory, "NostaleData"));
    }

    /// <summary>
    /// Get all running nostale processes.
    /// </summary>
    /// <returns>The nostale processes.</returns>
    public static IEnumerable<Process> GetAllNostaleProcesses()
        => Process
            .GetProcesses()
            .Where(IsProcessNostaleProcess);

    private readonly Dictionary<Type, NostaleObject> _modules;
    private readonly PlayerManagerOptions _playerManagerOptions;
    private readonly SceneManagerOptions _sceneManagerOptions;
    private readonly PetManagerOptions _petManagerOptions;
    private readonly NetworkManagerOptions _networkManagerOptions;
    private readonly UnitManagerOptions _unitManagerOptions;
    private readonly NtClientOptions _ntClientOptions;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="NosBrowserManager"/> class.
    /// </summary>
    /// <param name="playerManagerOptions">The options for obtaining player manager.</param>
    /// <param name="sceneManagerOptions">The scene manager options.</param>
    /// <param name="petManagerOptions">The pet manager options.</param>
    /// <param name="networkManagerOptions">The network manager options.</param>
    /// <param name="unitManagerOptions">The unit manager options.</param>
    /// <param name="ntClientOptions">The nt client options.</param>
    public NosBrowserManager
    (
        IOptionsSnapshot<PlayerManagerOptions> playerManagerOptions,
        IOptionsSnapshot<SceneManagerOptions> sceneManagerOptions,
        IOptionsSnapshot<PetManagerOptions> petManagerOptions,
        IOptionsSnapshot<NetworkManagerOptions> networkManagerOptions,
        IOptionsSnapshot<UnitManagerOptions> unitManagerOptions,
        IOptionsSnapshot<NtClientOptions> ntClientOptions
    )
        : this
        (
            Process.GetCurrentProcess(),
            playerManagerOptions.Value,
            sceneManagerOptions.Value,
            petManagerOptions.Value,
            networkManagerOptions.Value,
            unitManagerOptions.Value,
            ntClientOptions.Value
        )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NosBrowserManager"/> class.
    /// </summary>
    /// <param name="process">The process to browse.</param>
    /// <param name="playerManagerOptions">The options for obtaining player manager.</param>
    /// <param name="sceneManagerOptions">The scene manager options.</param>
    /// <param name="petManagerOptions">The pet manager options.</param>
    /// <param name="networkManagerOptions">The network manager options.</param>
    /// <param name="unitManagerOptions">The unit manager options.</param>
    /// <param name="ntClientOptions">The nt client options.</param>
    public NosBrowserManager
    (
        Process process,
        PlayerManagerOptions? playerManagerOptions = default,
        SceneManagerOptions? sceneManagerOptions = default,
        PetManagerOptions? petManagerOptions = default,
        NetworkManagerOptions? networkManagerOptions = default,
        UnitManagerOptions? unitManagerOptions = default,
        NtClientOptions? ntClientOptions = default
    )
    {
        _modules = new Dictionary<Type, NostaleObject>();
        _playerManagerOptions = playerManagerOptions ?? new PlayerManagerOptions();
        _sceneManagerOptions = sceneManagerOptions ?? new SceneManagerOptions();
        _petManagerOptions = petManagerOptions ?? new PetManagerOptions();
        _networkManagerOptions = networkManagerOptions ?? new NetworkManagerOptions();
        _unitManagerOptions = unitManagerOptions ?? new UnitManagerOptions();
        _ntClientOptions = ntClientOptions ?? new NtClientOptions();
        Process = process;
        Memory = Process.Id == Process.GetCurrentProcess().Id ? new Memory() : new ExternalMemory(process);
        Scanner = new Scanner(process, process.MainModule);
    }

    /// <summary>
    /// The NosTale process.
    /// </summary>
    public Process Process { get; }

    /// <summary>
    /// Gets the memory scanner.
    /// </summary>
    internal Scanner Scanner { get; }

    /// <summary>
    /// Gets the current process memory.
    /// </summary>
    internal IMemory Memory { get; }

    /// <summary>
    /// Gets whether this is a NosTale process or not.
    /// </summary>
    public bool IsNostaleProcess => NosBrowserManager.IsProcessNostaleProcess(Process);

    /// <summary>
    /// Gets whether the player is currently in game.
    /// </summary>
    /// <remarks>
    /// It may be unsafe to access some data if the player is not in game.
    /// </remarks>
    public Optional<bool> IsInGame => PlayerManager.Map(manager => manager.Player.Address != nuint.Zero);

    /// <summary>
    /// Gets the network manager.
    /// </summary>
    public Optional<NetworkManager> NetworkManager => GetModule<NetworkManager>();

    /// <summary>
    /// Gets the network manager.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the browser is not initialized or there was an error with initialization of unit manager.</exception>
    public Optional<UnitManager> UnitManager => GetModule<UnitManager>();

    /// <summary>
    /// Gets the nt client.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the browser is not initialized or there was an error with initialization of nt client.</exception>
    public Optional<NtClient> NtClient => GetModule<NtClient>();

    /// <summary>
    /// Gets the player manager.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the browser is not initialized or there was an error with initialization of player manager.</exception>
    public Optional<PlayerManager> PlayerManager => GetModule<PlayerManager>();

    /// <summary>
    /// Gets the scene manager.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the browser is not initialized or there was an error with initialization of scene manager.</exception>
    public Optional<SceneManager> SceneManager => GetModule<SceneManager>();

    /// <summary>
    /// Gets the pet manager list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the browser is not initialized or there was an error with initialization of pet manager list.</exception>
    public Optional<PetManagerList> PetManagerList => GetModule<PetManagerList>();

    /// <summary>
    /// Initialize the nos browser modules.
    /// </summary>
    /// <remarks>
    /// Needed to use all of the classes from NosTale.
    /// </remarks>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result Initialize()
    {
        if (!IsNostaleProcess)
        {
            return new NotNostaleProcessError(Process);
        }

        NostaleObject Map<T>(T val)
            where T : NostaleObject
        {
            return val;
        }

        _initialized = true;
        return HandleResults
        (
            (typeof(UnitManager), () => Structs.UnitManager.Create(this, _unitManagerOptions).Map(Map)),
            (typeof(NetworkManager), () => Structs.NetworkManager.Create(this, _networkManagerOptions).Map(Map)),
            (typeof(PlayerManager), () => Structs.PlayerManager.Create(this, _playerManagerOptions).Map(Map)),
            (typeof(SceneManager), () => Structs.SceneManager.Create(this, _sceneManagerOptions).Map(Map)),
            (typeof(PetManagerList), () => Structs.PetManagerList.Create(this, _petManagerOptions).Map(Map)),
            (typeof(NtClient), () => Structs.NtClient.Create(this, _ntClientOptions).Map(Map))
        );
    }

    /// <summary>
    /// Gets whether a hook or browser module is present/loaded.
    /// Returns false in case pattern was not found.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <returns>Whether the module is present.</returns>
    public bool IsModuleLoaded<TModule>()
        where TModule : NostaleObject
        => IsModuleLoaded(typeof(TModule));

    /// <summary>
    /// Gets whether a hook or browser module is present/loaded.
    /// Returns false in case pattern was not found.
    /// </summary>
    /// <param name="moduleType">The type of the module.</typeparam>
    /// <returns>Whether the module is present.</returns>
    public bool IsModuleLoaded(Type moduleType)
    {
        return GetModule(moduleType).IsPresent;
    }

    /// <summary>
    /// Get module of the specified type.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <returns>The module.</returns>
    /// <exception cref="InvalidOperationException">Thrown in case the manager was not initialized.</exception>
    public Optional<TModule> GetModule<TModule>()
        where TModule : NostaleObject
    {
        if (!_initialized)
        {
            throw new InvalidOperationException
            (
                $"Could not get {typeof(TModule)}. The browser manager is not initialized. Did you forget to call NosBrowserManager.Initialize?"
            );
        }

        if (!_modules.TryGetValue(typeof(TModule), out var nosObject) || nosObject is not TModule typed)
        {
            return Optional<TModule>.Empty;
        }

        return typed;
    }

    /// <summary>
    /// Get module of the specified type.
    /// </summary>
    /// <param name="moduleType">The type of the module.</typeparam>
    /// <returns>The module.</returns>
    /// <exception cref="InvalidOperationException">Thrown in case the manager was not initialized.</exception>
    public Optional<NostaleObject> GetModule(Type moduleType)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException
            (
                $"Could not get {moduleType.Name}. The browser manager is not initialized. Did you forget to call NosBrowserManager.Initialize?"
            );
        }

        if (!_modules.TryGetValue(moduleType, out var nosObject))
        {
            return Optional<NostaleObject>.Empty;
        }

        return nosObject;
    }

    private Result HandleResults(params (Type Type, Func<Result<NostaleObject>> Builder)[] objects)
    {
        Result<NostaleObject> HandleSafe(Func<Result<NostaleObject>> builder)
        {
            try
            {
                return builder();
            }
            catch (Exception e)
            {
                return e;
            }
        }

        List<IResult> errorResults = new List<IResult>();
        foreach (var obj in objects)
        {
            var createdResult = HandleSafe(obj.Builder);

            if (!createdResult.IsSuccess)
            {
                errorResults.Add
                (
                    Result.FromError
                    (
                        new CouldNotInitializeModuleError(obj.Type, createdResult.Error),
                        createdResult
                    )
                );
            }
            else if (createdResult.IsDefined(out var created))
            {
                _modules.Add(obj.Type, created);
            }
        }

        return errorResults.Count switch
        {
            0 => Result.FromSuccess(),
            1 => (Result)errorResults[0],
            _ => new AggregateError(errorResults)
        };
    }
}