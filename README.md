# Wallet Framework for .NET

Wallet Framework for .NET is an open framework for building digital identity wallets. The framework was initiated as [Aries Framework .NET](https://github.com/hyperledger/aries-framework-dotnet) in the Hyperledger Foundation and was forked to express the goal to broaden the supported identity protocols, especially with regard to [OID4VC](https://openid.net/openid4vc/) and [SD-JWT](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-selective-disclosure-jwt-05).

## Table of Contents <!-- omit in toc -->

- [Wallet Framework for .NET](#wallet-framework-for-net)
  - [Roadmap](#roadmap)
  - [Protocols](#protocols)
  - [OpenID for Verifiable Credentials (OID4VC)](#openid-for-verifiable-credentials-oid4vc)
  - [Hyperledger Aries](#hyperledger-aries)
  - [Credential Formats](#credential-formats)
  - [Quickstart Guide](#quickstart-guide)
    - [Prerequisites](#prerequisites)
    - [Create new web application](#create-new-web-application)
    - [Add the framework dependencies](#add-the-framework-dependencies)
    - [Register the agent middleware](#register-the-agent-middleware)
  - [Demo](#demo)
  - [Testing](#testing)
    - [Install libindy library](#install-libindy-library)
    - [Run an indy node pool on localhost](#run-an-indy-node-pool-on-localhost)
    - [Run an indy node pool on server](#run-an-indy-node-pool-on-server)
    - [Run the tests](#run-the-tests)
  - [License](#license)

## Roadmap
- OpenID4VC Support
- SD-JWT VC Support
- Replacing the indy-sdk


## Protocols

## OpenID for Verifiable Credentials ([OID4VC](https://openid.net/sg/openid4vc/specifications/))

| Protocol                                  | Link                                                                                 | State          |
| ----------------------------------------- | ------------------------------------------------------------------------------------ | -------------- |
| OpenID for Verifiable Credential Issuance | [OID4VCI](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) | :construction: |
| OpenID for Verifiable Presentations       | [OID4VP](https://openid.net/specs/openid-4-verifiable-presentations-1_0-ID2.html)    | :construction: |
| Self-Issued OpenID Provider v2            | [SIOPv2](https://openid.net/specs/openid-connect-self-issued-v2-1_0-ID1.html)        | :construction: |

## Hyperledger Aries

**AIP 1.0**
| Protocol                                                                                                                                              | State               |
| ----------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- |
| [0019-encryption-envelope](https://github.com/hyperledger/aries-rfcs/tree/9b0aaa39df7e8bd434126c4b33c097aae78d65bf/features/0019-encryption-envelope) | :white_check_mark:  |
| [0160-connection-protocol](https://github.com/hyperledger/aries-rfcs/tree/4d9775490359e234ab8d1c152bca6f534e92a38d/features/0160-connection-protocol) | :white_check_mark:  |
| [0036-issue-credential](https://github.com/hyperledger/aries-rfcs/tree/bb42a6c35e0d5543718fb36dd099551ab192f7b0/features/0036-issue-credential)       | :white_check_mark:  |
| [0037-present-proof](https://github.com/hyperledger/aries-rfcs/tree/4fae574c03f9f1013db30bf2c0c676b1122f7149/features/0037-present-proof)             | :white_check_mark:  |
| [0056-service-decorator](https://github.com/hyperledger/aries-rfcs/tree/527849ec3aa2a8fd47a7bb6c57f918ff8bcb5e8c/features/0056-service-decorator)     | :white_check_mark:  |
| [0025-didcomm-transports](https://github.com/hyperledger/aries-rfcs/tree/b490ebe492985e1be9804fc0763119238b2e51ab/features/0025-didcomm-transports)   | Http supported      |
| [0015-acks](https://github.com/hyperledger/aries-rfcs/tree/5cc750f0fe18e3918401489066566f22474e25a8/features/0015-acks)                               | Partially supported |
| [0035-report-problem](https://github.com/hyperledger/aries-rfcs/tree/89d14c15ab35b667e7a9d04fe42d4d48b10468cf/features/0035-report-problem)           | Partially supported |


**AIP 2.0**
| Protocol                                                                                                                                  | State              |
| ----------------------------------------------------------------------------------------------------------------------------------------- | ------------------ |
| [0023-did-exchange](https://github.com/hyperledger/aries-rfcs/tree/bf3d796cc33ce78ed7cde7f5422b10719a68be21/features/0023-did-exchange)   | :white_check_mark: |
| [0048-trust-ping](https://github.com/hyperledger/aries-rfcs/tree/4e78319e5f79df2003ddf37f8f497d0fae20cc63/features/0048-trust-ping)       | :white_check_mark: |
| [0095-basic-message](https://github.com/hyperledger/aries-rfcs/tree/b3a3942ef052039e73cd23d847f42947f8287da2/features/0095-basic-message) | :white_check_mark: |
|                                                                                                                                           |                    |

## Credential Formats

| Credential Format                   | Link                                                                                                                                      | State              |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ------------------ |
| AnonCreds 1.0                       | [Anonymous Credential Protocol](https://hyperledger-indy.readthedocs.io/projects/hipe/en/latest/text/0109-anoncreds-protocol/README.html) | :white_check_mark: |
| SD-JWT-based Verifiable Credentials | [SD-JWT VC](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-sd-jwt-vc-00)                                                          | :construction:     |


## Quickstart Guide

The framework fully leverages the [.NET Core hosting model](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0) with full integration of dependency injection, configuration and hosting services.

### Prerequisites

- Install [.NET Core](https://dotnet.microsoft.com/download)
- Install [libindy for your platform](https://github.com/hyperledger/indy-sdk/#installing-the-sdk)

### Create new web application

Using your favorite editor, create new web project. You can also create a project from the console.

```bash
dotnet new web -o AriesAgent
```

To setup your agent use the `Startup.cs` file to configure the framework.

### Add the framework dependencies

Use the `IServiceCollection` extensions to add the dependent services to your application in the `ConfigureServices(IServiceCollection services)` method. Upon startup, the framework will create and configure your agent.

```c#
services.AddAriesFramework(builder =>
{
    builder.RegisterAgent(options =>
    {
        options.EndpointUri = "http://localhost:5000/";
    });
});
```

> Note: If you'd like your agent to be accessible publically, use Ngrok to setup a public host and use that as the `EndpointUri`.
> When changing the endpoints, make sure you clear any previous wallets with the old configuration. Wallet data files are located in `~/.indy_client/wallet`

For a list of all configuration options, check the [AgentOptions.cs](https://github.com/hyperledger/aries-framework-dotnet/blob/master/src/Hyperledger.Aries/Configuration/AgentOptions.cs) file.

### Register the agent middleware

When running web applications, register the agent middleware in the `Configure(IApplicationBuilder app, IWebHostEnvironment env)` method. This will setup a middleware in the AspNetCore pipeline that will respond to incoming agent messages.

```c#
app.UseAriesFramework();
```

That's it. Run your project.

## Demo

With [Docker](https://www.docker.com) installed, run

```lang=bash
docker-compose up
```

This will create an agent network with a pool of 4 indy nodes and 2 agents able to communicate with each other in the network.
Navigate to [http://localhost:7000](http://localhost:7000) and [http://localhost:8000](http://localhost:8000) to create and accept connection invitations between the different agents.


## Testing

To run the unit tests, the following dependencies also must be installed: 
- Docker

### Install libindy library
Follow the build instructions for your OS on the [Hyperledger Indy SDK](https://github.com/hyperledger/indy-sdk) Readme. 

For macOS, if you get a `'indy' DLL not found exception`, move the built `libindy.dylib` file to the `test/Hyperledger.Aries.Tests/bin/Debug/netcoreapp3.1/` directory to explicitly add it to the path. 


### Run an indy node pool on localhost
```
docker build --build-arg pool_ip=127.0.0.1 -f docker/indy-pool.dockerfile -t indy_pool docker/
docker run -itd -p 9701-9709:9701-9709 indy_pool
```

### Run an indy node pool on server
```
# replace <ip_address> with server IP address
docker build --build-arg pool_ip=<ip_address> -f docker/indy-pool.dockerfile -t indy_pool docker/
docker run -itd -p <ip_address>:9701-9709:9701-9709 indy_pool
```

### Run the tests
First, edit the keyword in the `scripts/tester.sh` file to select the tests you want to run. Then, run the script
```
scripts/tester.sh 
```

## License

[Apache License Version 2.0](https://github.com/hyperledger/aries-cloudagent-python/blob/master/LICENSE)
