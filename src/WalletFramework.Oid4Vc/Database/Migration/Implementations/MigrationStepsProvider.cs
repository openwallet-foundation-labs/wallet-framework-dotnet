using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using OneOf;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.CredentialSet;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.Database.Migration.Abstraction;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Services;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Database.Migration.Implementations;

internal class MigrationStepsProvider(
    IAgentProvider agentProvider,
    ICredentialSetStorage credentialSetStorage,
    IMdocStorage mdocStorage,
    IWalletRecordService walletRecordService,
    IOid4VpRecordService oid4VpRecordService,
    ISdJwtVcHolderService sdJwtVcHolderService) : IMigrationStepsProvider
{
    public IEnumerable<MigrationStep> Get()
    {
        var sdJwtStep = new MigrationStep(
            1,
            2,
            typeof(SdJwtRecord),
            async records =>
            {
                var sdJwtRecords = records.OfType<SdJwtRecord>().ToList();
                foreach (var record in sdJwtRecords)
                {
                    if (string.IsNullOrWhiteSpace(record.CredentialSetId))
                    {
                        var credentialSetRecord = new CredentialSetRecord();
                        credentialSetRecord.AddSdJwtData(record);
                        await credentialSetStorage.Add(credentialSetRecord);
                        record.CredentialSetId = credentialSetRecord.CredentialSetId;
                    }
                    
                    var context = await agentProvider.GetContextAsync();
                    await sdJwtVcHolderService.UpdateAsync(context, record);
                }

                return sdJwtRecords;
            },
            async () =>
            {
                var query = SearchQuery.Less(
                    nameof(MdocRecord.RecordVersion),
                    "2");

                var context = await agentProvider.GetContextAsync();
                var records = await sdJwtVcHolderService.ListAsync(context, query);
                return records.Any() 
                    ? records.Cast<RecordBase>().ToList() 
                    : Option<IEnumerable<RecordBase>>.None;
            });

        var mdocStep = new MigrationStep(
            1,
            2,
            typeof(MdocRecord),
            async records =>
            {
                var mdocRecords = records.OfType<MdocRecord>().ToList();
                foreach (var record in mdocRecords)
                {
                    if (string.IsNullOrWhiteSpace(record.CredentialSetId))
                    {
                        var credentialSetRecord = new CredentialSetRecord();
                        credentialSetRecord.AddMdocData(record);
                        await credentialSetStorage.Add(credentialSetRecord);
                        record.CredentialSetId = credentialSetRecord.CredentialSetId;
                    }
                    
                    await mdocStorage.Update(record);
                }

                return mdocRecords;
            }, 
            async () =>
            {
                var query = SearchQuery.Not(SearchQuery.Greater(nameof(MdocRecord.RecordVersion), "1"));
                var someQuery = Option<ISearchQuery>.Some(query);

                var mdocs = await mdocStorage.List(someQuery);
                return
                    from mdocRecords in mdocs
                    select mdocRecords.Cast<RecordBase>();
            });
        
        var presentationRecordStep = new MigrationStep(
            1,
            2,
            typeof(OidPresentationRecord),
            async records =>
            {
                var result = new List<OidPresentationRecord>();
                
                var context = await agentProvider.GetContextAsync();
                
                var presentationRecords = records.OfType<Oid4Vp.Models.v1.OidPresentationRecord>();
                foreach (var presentationRecord in presentationRecords)
                {
                    var presentedCredentialSets = new List<PresentedCredentialSet>();

                    foreach (var presentedCredential in presentationRecord.PresentedCredentials)
                    {
                        var record = Option<OneOf<SdJwtRecord, MdocRecord>>.None;
                        
                        var sdJwtRecord = await walletRecordService.GetAsync<SdJwtRecord>(context.Wallet, presentedCredential.CredentialId);
                        if (sdJwtRecord != null)
                        {
                            record = Option<OneOf<SdJwtRecord, MdocRecord>>.Some(sdJwtRecord);
                        }
                        else
                        {
                            var mDocRecord = await walletRecordService.GetAsync<MdocRecord>(context.Wallet, presentedCredential.CredentialId);
                            if (mDocRecord != null)
                            {
                                record = Option<OneOf<SdJwtRecord, MdocRecord>>.Some(mDocRecord);
                            }
                        }

                        if (record.IsNone)
                            continue;

                        var presentedRecord = record.Value();
                        presentedRecord.Match(
                            sdJwt =>
                            {
                                presentedCredentialSets.Add(
                                    new PresentedCredentialSet
                                    {
                                        CredentialSetId = CredentialSetId.ValidCredentialSetId(sdJwt.CredentialSetId)
                                            .UnwrapOrThrow(),
                                        SdJwtCredentialType = Vct.ValidVct(sdJwt.Vct).UnwrapOrThrow(),
                                        PresentedClaims = presentedCredential.PresentedClaims
                                    });

                                return Unit.Default;
                            },
                            mDoc =>
                            {
                                presentedCredentialSets.Add(
                                    new PresentedCredentialSet
                                    {
                                        CredentialSetId = CredentialSetId.ValidCredentialSetId(mDoc.CredentialSetId)
                                            .UnwrapOrThrow(),
                                        MDocCredentialType = mDoc.DocType,
                                        PresentedClaims = presentedCredential.PresentedClaims
                                    });

                                return Unit.Default;
                            });
                    }

                    var oidPresentationRecord = new OidPresentationRecord
                    {
                        Id = Guid.NewGuid().ToString(),
                        ClientId = presentationRecord.ClientId,
                        ClientMetadata = presentationRecord.ClientMetadata,
                        Name = presentationRecord.Name,
                        PresentedCredentialSets = presentedCredentialSets
                    };
                    
                    await oid4VpRecordService.StoreAsync(context,oidPresentationRecord);

                    await walletRecordService.DeleteAsync<Oid4Vp.Models.v1.OidPresentationRecord>(context.Wallet,
                        presentationRecord.Id);
                    
                    result.Add(oidPresentationRecord);
                }

                return result;
            }, 
            async () =>
            {
                var query = SearchQuery.Less(nameof(OidPresentationRecord.RecordVersion), "2");

                var context = await agentProvider.GetContextAsync();
                return await walletRecordService.SearchAsync<Oid4Vp.Models.v1.OidPresentationRecord>(context.Wallet, query, null, 100);
            });
        
        var addCredentialFormatToSdJwtStep = new MigrationStep(
            2,
            3,
            typeof(SdJwtRecord),
            async records =>
            {
                var sdJwtRecords = records.OfType<SdJwtRecord>().ToList();
                foreach (var record in sdJwtRecords)
                {
                    if (string.IsNullOrWhiteSpace(record.Format))
                    {
                        record.Format = FormatFun.CreateSdJwtVcFormat();
                    }
                    
                    var context = await agentProvider.GetContextAsync();
                    await sdJwtVcHolderService.UpdateAsync(context, record);
                }
        
                return sdJwtRecords;
            },
            async () =>
            {
                var query = SearchQuery.Equal("~" + nameof(SdJwtRecord.RecordVersion), "2");
        
                var context = await agentProvider.GetContextAsync();
                var records = await sdJwtVcHolderService.ListAsync(context, query);
                return records.Any() 
                    ? records.Cast<RecordBase>().ToList() 
                    : Option<IEnumerable<RecordBase>>.None;
            });

        return [sdJwtStep, mdocStep, presentationRecordStep, addCredentialFormatToSdJwtStep];
    }
}
