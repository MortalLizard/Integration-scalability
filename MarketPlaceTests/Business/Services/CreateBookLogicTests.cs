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
   
    [Test]
    public async Task ShouldFailIntentionally()
    {
        Assert.Fail("This test is designed to fail intentionally.");
    }

    [Test]
    public async Task ShouldPassIntentionally()
    {
        Assert.That(true);
        Assert.Pass("This test is designed to Pass intentionally.");
    }
}
