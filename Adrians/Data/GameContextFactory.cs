using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Adrians.Data;

public sealed class GameContextFactory : IDesignTimeDbContextFactory<GameContext>
{
    public GameContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=adriansdb;User=adrian;Password=supersecret;TreatTinyAsBoolean=false";

        var optionsBuilder = new DbContextOptionsBuilder<GameContext>();
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new GameContext(optionsBuilder.Options);
    }
}