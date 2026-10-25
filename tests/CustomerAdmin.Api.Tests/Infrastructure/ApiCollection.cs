namespace CustomerAdmin.Api.Tests.Infrastructure;

/// <summary>
/// All test classes in this collection share one ApiFactory, so the database container starts only once.
/// Tests in one collection run one after another.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "API";
}