using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Test.Module.Entities;

public class ModuleEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ModuleEntityConfiguration : IEntityTypeConfiguration<ModuleEntity>
{
    public void Configure(EntityTypeBuilder<ModuleEntity> builder)
    {
        builder.ToTable("ModuleBased", "module");
    }
}