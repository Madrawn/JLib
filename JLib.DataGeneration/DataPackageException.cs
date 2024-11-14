using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DataGeneration;

/// <summary>
/// indicates that something went wrong while working with data packages.
/// </summary>
public abstract class DataPackageException : JLibException
{
    public Type DataPackageType { get; }

    private DataPackageException(Type dataPackageType, string message) : base(message)
    {
        DataPackageType = dataPackageType;
        Data[nameof(DataPackageType)] = dataPackageType;
    }
    /// <summary>
    /// indicates that something went wrong during the <see cref="DataPackage"/> constructor execution.
    /// </summary>
    public abstract class InitializationException : DataPackageException
    {
        private InitializationException(Type dataPackageType, string message) : base(dataPackageType, message)
        {

        }

        /// <summary>
        /// indicates that the data package has been created during an invalid lifecycle.<br/>
        /// dataPackages may only be created during <see cref="DataPackageExtensions.IncludeDataPackages(IServiceProvider,DataPackageType[])"/> or <see cref="DataPackageExtensions.IncludeDataPackages{T}(IServiceProvider)"/>
        /// </summary>
        public abstract class InvalidAccessException : DataPackageException
        {
            private InvalidAccessException(Type dataPackageType, string message) : base(dataPackageType, $"Invalid {dataPackageType.FullName(true)} Initialization: {message}")
            {

            }
            /// <summary>
            /// indicates that this dataPackage has been created before <see cref="DataPackageExtensions.IncludeDataPackages(IServiceProvider,DataPackageType[])"/> has been called.
            /// </summary>
            public sealed class PreInitializationInstantiationException : InvalidAccessException
            {
                internal PreInitializationInstantiationException(DataPackage cause) : base(cause.GetType(),
                    "inject directly after provider build using 'JLib.DataGeneration.DataPackageExtensions.IncludeDataPackages'.")
                {

                }
            }

            /// <summary>
            /// indicates that this dataPackage has been created after <see cref="DataPackageExtensions.IncludeDataPackages(IServiceProvider,DataPackageType[])"/> has been called.
            /// </summary>
            public sealed class PostInitializationAccessException : InvalidAccessException
            {
                internal PostInitializationAccessException(DataPackage cause) : base(cause.GetType(),
                    "this type package has not been included when calling 'JLib.DataGeneration.DataPackageExtensions.IncludeDataPackages'.")
                {

                }
            }
            /// <summary>
            /// indicates that this dataPackage has an invalid <see cref="DataPackageInitState"/>, indicating a framework bug.
            /// </summary>
            public sealed class InvalidInitStateException : InvalidAccessException
            {
                internal DataPackageInitState State { get; }
                internal InvalidInitStateException(DataPackage cause, DataPackageInitState state) : base(cause.GetType(),
                    "this type package has a undefined init state, which should not happen, indicating a framework bug.")
                {
                    State = state;
                    Data[nameof(state)] = state;
                }
            }
        }

    }
}