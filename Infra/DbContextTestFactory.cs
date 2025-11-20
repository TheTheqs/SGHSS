// Infra/DbContextTestFactory.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Infra.Persistence;

namespace SGHSS.Tests.Infra
{
    /// <summary>
    /// Fábrica utilitária para criação de SGHSSDbContext configurado para testes.
    /// </summary>
    public static class DbContextTestFactory
    {
        public static SGHSSDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<SGHSSDbContext>()
                .UseInMemoryDatabase(databaseName: $"SGHSS_Test_{Guid.NewGuid()}")
                .Options;

            return new SGHSSDbContext(options);
        }
    }
}
