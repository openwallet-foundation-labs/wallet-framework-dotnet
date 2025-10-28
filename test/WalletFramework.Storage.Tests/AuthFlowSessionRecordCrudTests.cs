using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.Storage.Database;

namespace WalletFramework.Storage.Tests;

public class AuthFlowSessionRecordCrudTests : IDisposable
{
    public AuthFlowSessionRecordCrudTests() => (_serviceProvider, _dbPath) = TestDbSetup.CreateServiceProvider();

    private readonly ServiceProvider _serviceProvider;
    
    private readonly string _dbPath;

    [Fact]
    public async Task Can_Delete_AuthFlowSessionRecord()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, AuthFlowSessionState>>();

        var session = CreateSampleSession();
        await repository.Add(session);

        await repository.Delete(session.AuthFlowSessionState);
        var fetched = await repository.GetById(session.AuthFlowSessionState);
        fetched.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task Can_Store_And_Retrieve_AuthFlowSessionRecord()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, AuthFlowSessionState>>();

        var session = CreateSampleSession();
        await repository.Add(session);

        var fetched = await repository.GetById(session.AuthFlowSessionState);
        fetched.Match(
            found => { found.AuthFlowSessionState.ToString().Should().Be(session.AuthFlowSessionState.ToString()); },
            () => throw new InvalidOperationException("Record should exist"));
    }

    [Fact]
    public async Task Can_Update_AuthFlowSessionRecord()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, AuthFlowSessionState>>();

        var session = CreateSampleSession();
        await repository.Add(session);

        var updated = session with { SpecVersion = Prelude.Some(42) };
        await repository.Update(updated);

        var fetched = await repository.GetById(updated.AuthFlowSessionState);
        fetched.IsSome.Should().BeTrue();
    }

    public void Dispose()
    {
        TestDbSetup.Cleanup(_serviceProvider, _dbPath);
        GC.SuppressFinalize(this);
    }

    private static AuthFlowSession CreateSampleSession()
    {
        var clientOptions = new ClientOptions("client", "issuer", "https://redirect");

        var sdJwtConfig = new JObject
        {
            { "format", "vc+sd-jwt" },
            { "vct", "com.example.vct" }
        };

        var credentialConfigsSupported = new JObject
        {
            { "example_config", sdJwtConfig }
        };

        var issuerMetadataJson = new JObject
        {
            { IssuerMetadataJsonExtensions.CredentialEndpointJsonKey, "https://issuer/credential" },
            { IssuerMetadataJsonExtensions.CredentialIssuerJsonKey, "https://issuer" },
            { IssuerMetadataJsonExtensions.CredentialConfigsSupportedJsonKey, credentialConfigsSupported }
        };
        var issuerMetadata = IssuerMetadata.ValidIssuerMetadata(issuerMetadataJson).UnwrapOrThrow();

        var authServer = new AuthorizationServerMetadata
        {
            Issuer = "https://as",
            TokenEndpoint = "https://as/token",
            JwksUri = "https://as/jwks",
            AuthorizationEndpoint = "https://as/auth",
            TokenEndpointAuthMethodsSupported = ["client_secret_post"],
            TokenEndpointAuthSigningAlgValuesSupported = ["RS256"]
        };

        var authorizationData = new AuthorizationData(
            clientOptions,
            issuerMetadata,
            authServer,
            Option<OAuthToken>.None,
            new List<CredentialConfigurationId>());

        var state = AuthFlowSessionState.CreateAuthFlowSessionState();
        var session = new AuthFlowSession(state, authorizationData,
            new AuthorizationCodeParameters("challenge", "verifier"), Option<int>.None);
        return session;
    }
}
