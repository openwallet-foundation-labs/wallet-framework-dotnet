using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Storage.Models;
using Hyperledger.Indy.NonSecretsApi;
using Hyperledger.Indy.WalletApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hyperledger.Aries.Storage
{
    /// <inheritdoc />
    public class DefaultWalletRecordService : IWalletRecordService
    {
        private readonly JsonSerializerSettings _jsonSettings;

        /// <summary>Initializes a new instance of the <see cref="DefaultWalletRecordService"/> class.</summary>
        public DefaultWalletRecordService()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new AgentEndpointJsonConverter(),
                    new AttributeFilterConverter()
                }
            };
        }

        /// <inheritdoc />
        public virtual Task AddAsync<T>(Wallet wallet, T record, Func<T, JObject>? encode = null) where T : RecordBase, new()
        {
            record.CreatedAtUtc = DateTime.UtcNow;
            
            var properties = record
                .GetType()
                .GetProperties()
                .Where(info => Attribute.IsDefined(info, typeof(RecordTagAttribute)));
            
            foreach (var property in properties)
            {
                var value = property.GetValue(record);
                record.SetTag(property.Name, value.ToString(), false);
            }

            var recordJson = encode is null 
                ? record.ToJson(_jsonSettings) 
                : encode(record).ToString();

            return NonSecrets.AddRecordAsync(wallet,
                record.TypeName,
                record.Id,
                recordJson,
                record.Tags.ToJson());
        }

        /// <inheritdoc />
        public virtual async Task<List<T>> SearchAsync<T>(
            Wallet wallet,
            ISearchQuery? query = null,
            SearchOptions? options = null,
            int count = 10,
            int skip = 0,
            Func<JObject, T>? decode = null) where T : RecordBase, new()
        {
            using var search = await NonSecrets.OpenSearchAsync(
                wallet,
                new T().TypeName,
                (query ?? SearchQuery.Empty).ToJson(),
                (options ?? new SearchOptions()).ToJson()
          );
            
            if(skip > 0) {
                await search.NextAsync(wallet, skip);
            }
            
            var searchResultStr = await search.NextAsync(wallet, count);
            var searchResult = JsonConvert.DeserializeObject<SearchResult>(searchResultStr, _jsonSettings);

            if (searchResult?.Records is null)
            {
                return new List<T>();
            }

            var records = searchResult.Records.Select(searchItem =>
            {
                T record;
                if (decode is null)
                {
                    record = JsonConvert.DeserializeObject<T>(searchItem.Value, _jsonSettings)!;
                }
                else
                {
                    var json = JObject.Parse(searchItem.Value);
                    record = decode(json);
                }

                foreach (var tag in searchItem.Tags)
                    record.Tags[tag.Key] = tag.Value;

                return record;
            });

            return records.ToList();
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(Wallet wallet, RecordBase record)
        {
            record.UpdatedAtUtc = DateTime.UtcNow;

            await NonSecrets.UpdateRecordValueAsync(wallet,
                record.TypeName,
                record.Id,
                record.ToJson(_jsonSettings));

            await NonSecrets.UpdateRecordTagsAsync(wallet,
                record.TypeName,
                record.Id,
                record.Tags.ToJson(_jsonSettings));
        }

        public async Task Update<T>(Wallet wallet, T record, Func<T, JObject>? encode = null) where T : RecordBase
        {
            record.UpdatedAtUtc = DateTime.UtcNow;

            var recordJson = encode is null 
                ? record.ToJson(_jsonSettings) 
                : encode(record).ToString();

            await NonSecrets.UpdateRecordValueAsync(wallet,
                record.TypeName,
                record.Id,
                recordJson);

            await NonSecrets.UpdateRecordTagsAsync(wallet,
                record.TypeName,
                record.Id,
                record.Tags.ToJson(_jsonSettings));
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync<T>(Wallet wallet, string id, Func<JObject, T>? decode = null) where T : RecordBase, new()
        {
            try
            {
                var searchItemJson = await NonSecrets.GetRecordAsync(wallet,
                    new T().TypeName,
                    id,
                    new SearchOptions().ToJson());

                if (searchItemJson == null)
                {
                    return null;
                }

                var item = JsonConvert.DeserializeObject<SearchItem>(searchItemJson, _jsonSettings)!;

                T record;
                if (decode is null)
                {
                    record = JsonConvert.DeserializeObject<T>(item.Value, _jsonSettings)!;
                }
                else
                {
                    var json = JObject.Parse(item.Value);
                    record = decode(json);
                }

                foreach (var tag in item.Tags)
                    record.Tags[tag.Key] = tag.Value;

                return record;
            }
            catch (WalletItemNotFoundException)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync<T>(Wallet wallet, string id) where T : RecordBase, new()
        {
            try
            {
                var record = await GetAsync<T>(wallet, id);
                var typeName = record.TypeName;

                await NonSecrets.DeleteRecordTagsAsync(
                    wallet: wallet,
                    type: typeName,
                    id: id,
                    tagsJson: record.Tags.Select(x => x.Key).ToArray().ToJson());
                await NonSecrets.DeleteRecordAsync(
                    wallet: wallet,
                    type: typeName,
                    id: id);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Couldn't delete record: {e}");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Delete(Wallet wallet, RecordBase record)
        {
            try
            {
                var typeName = record.TypeName;

                await NonSecrets.DeleteRecordTagsAsync(
                    wallet: wallet,
                    type: typeName,
                    id: record.Id,
                    tagsJson: record.Tags.Select(x => x.Key).ToArray().ToJson());
                await NonSecrets.DeleteRecordAsync(
                    wallet: wallet,
                    type: typeName,
                    id: record.Id);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Couldn't delete record: {e}");
                return false;
            }
        }
    }
}
