using Marketplace.Business.Interfaces;
using Marketplace.Business.Services;
using Marketplace.Database.Entities;
using Marketplace.Database.Repositories;

using Shared;
using Shared.Contracts.CreateBook;

namespace MarketPlaceTests.Business.Services;

[TestFixture]
public class CreateBookLogicTests
{
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public async Task ReturnTrue()
    {
        Assert.Pass();
    }
}
