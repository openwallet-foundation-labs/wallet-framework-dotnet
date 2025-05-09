namespace WalletFramework.Oid4Vc.Qes.Authorization;

public record InputDescriptorTransactionData(string InputDescriptorId, List<Uc5QesTransactionData> TransactionData);
