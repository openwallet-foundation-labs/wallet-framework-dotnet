using WalletFramework.Core.Cryptography.Models;

namespace WalletFramework.Core.Cryptography.Abstractions;

/// <summary>
///     Represents a store for managing keys.
///     This interface is intended to be implemented outside of the framework on the device side,
///     allowing flexibility in key generation or retrieval mechanisms.
/// </summary>
public interface IKeyStore
{
    /// <summary>
    ///     Asynchronously generates a key for the specified algorithm and returns the key identifier.
    /// </summary>
    /// <param name="alg">The algorithm for key generation (default is "ES256").</param>
    /// <param name="isPermanent">If false creates an ephemeral key.</param>
    /// <returns>A <see cref="Task{TResult}" /> representing the generated key's identifier as a string.</returns>
    Task<KeyId> GenerateKey(string alg = "ES256", bool isPermanent = true);

    /// <summary>
    ///     Gets the public key of the pair 
    /// </summary>
    /// <param name="keyId">The identifier of the key pair.</param>
    /// <returns>
    ///     The public key
    /// </returns>
    Task<PublicKey> GetPublicKey(KeyId keyId);

    /// <summary>
    ///     Asynchronously signs the given payload using the key identified by the provided key ID.
    /// </summary>
    /// <param name="keyId">The identifier of the key to use for signing.</param>
    /// <param name="payload">The payload to sign.</param>
    /// <returns>A <see cref="Task{TResult}" /> representing the signed payload as a byte array.</returns>
    Task<RawSignature> Sign(KeyId keyId, byte[] payload);
        
    /// <summary>
    ///     Asynchronously deletes the key associated with the provided key ID.
    /// </summary>
    /// <param name="keyId">The identifier of the key that should be deleted</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task DeleteKey(KeyId keyId);
}
