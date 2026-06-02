using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BaseContextPoC;

public class SimpleContextOneFactory : IDesignTimeDbContextFactory<SimpleContextOne>
{
    public SimpleContextOne CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SimpleContextOne>();
        optionsBuilder.UseSqlServer("Server=PA75313003M-MBX;Database=BaseContextPoC;Trusted_Connection=True;TrustServerCertificate=True;");

        return new SimpleContextOne(optionsBuilder.Options);
    }
}

public class SimpleContextTwoFactory : IDesignTimeDbContextFactory<SimpleContextTwo>
{
    public SimpleContextTwo CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SimpleContextTwo>();
        optionsBuilder.UseSqlServer("Server=PA75313003M-MBX;Database=BaseContextPoC;Trusted_Connection=True;TrustServerCertificate=True;");

        return new SimpleContextTwo(optionsBuilder.Options);
    }
}
