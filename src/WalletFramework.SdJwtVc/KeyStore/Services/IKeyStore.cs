namespace WalletFramework.SdJwtVc.KeyStore.Services
{
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
        /// <returns>A <see cref="Task{TResult}" /> representing the generated key's identifier as a string.</returns>
        Task<string> GenerateKey(string alg = "ES256");

        /// <summary>
        ///     Asynchronously creates a proof of possession for a specific key, based on the provided audience and nonce.
        /// </summary>
        /// <param name="keyId">The identifier of the key to be used in creating the proof of possession.</param>
        /// <param name="audience">The intended recipient of the proof. Typically represents the entity that will verify it.</param>
        /// <param name="nonce">
        ///     A unique token, typically used to prevent replay attacks by ensuring that the proof is only used once.
        /// </param>
        /// <param name="type">The type of the proof. (For example "openid4vci-proof+jwt")</param>
        /// <param name="sdHash">Base64url-encoded hash digest over the Issuer-signed JWT and the selected Disclosures for integrity protection</param>
        /// <returns>
        ///     A <see cref="Task{TResult}" /> representing the asynchronous operation. When evaluated, the task's result contains
        ///     the proof.
        /// </returns>
        Task<string> GenerateKbProofOfPossessionAsync(string keyId, string audience, string nonce, string type, string? sdHash = null);
        
        /// <summary>
        ///     Asynchronously creates a DPoP Proof JWT for a specific key, based on the provided audience, nonce and access token.
        /// </summary>
        /// <param name="keyId">The identifier of the key to be used in creating the proof of possession.</param>
        /// <param name="audience">The intended recipient of the proof. Typically represents the entity that will verify it.</param>
        /// <param name="nonce">A unique token, typically used to prevent replay attacks by ensuring that the proof is only used once.</param>
        /// <param name="accessToken">The access token, that the DPoP Proof JWT is bound to</param>
        /// <returns>
        ///     A <see cref="Task{TResult}" /> representing the asynchronous operation. When evaluated, the task's result contains
        ///     the DPoP Proof JWT.
        /// </returns>
        Task<string> GenerateDPopProofOfPossessionAsync(string keyId, string audience, string? nonce, string? accessToken);

        /// <summary>
        ///     Asynchronously loads a key by its identifier and returns it as a JSON Web Key (JWK) containing the public key
        ///     information.
        /// </summary>
        /// <param name="keyId">The identifier of the key to load.</param>
        /// <returns>A <see cref="Task{TResult}" /> representing the loaded key as a JWK string.</returns>
        Task<string> LoadKey(string keyId);

        /// <summary>
        ///     Asynchronously signs the given payload using the key identified by the provided key ID.
        /// </summary>
        /// <param name="keyId">The identifier of the key to use for signing.</param>
        /// <param name="payload">The payload to sign.</param>
        /// <returns>A <see cref="Task{TResult}" /> representing the signed payload as a byte array.</returns>
        Task<byte[]> Sign(string keyId, byte[] payload);
        
        /// <summary>
        ///     Asynchronously deletes the key associated with the provided key ID.
        /// </summary>
        /// <param name="keyId">The identifier of the key that should be deleted</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task DeleteKey(string keyId);
    }
}
