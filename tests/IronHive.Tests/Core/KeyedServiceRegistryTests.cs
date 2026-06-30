namespace IronHive.Tests.Core;

// KeyedServiceRegistry was removed as part of the DI architecture redesign.
// The registry layer (IProviderRegistry/IStorageRegistry) has been replaced with
// IReadOnlyDictionary<string, T> passed directly to service constructors via HiveServiceBuilder.
// These tests have been removed as the tested class no longer exists.
