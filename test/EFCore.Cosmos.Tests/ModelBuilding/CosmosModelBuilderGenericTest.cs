// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Xunit.Sdk;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class CosmosModelBuilderGenericTest : ModelBuilderGenericTest
{
    public class CosmosGenericNonRelationship : GenericNonRelationship
    {
        public override void Properties_can_set_row_version()
            => Assert.Equal(
                CosmosStrings.NonETagConcurrencyToken(nameof(Quarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    () => base.Properties_can_set_row_version()).Message);

        public override void Properties_can_be_made_concurrency_tokens()
            => Assert.Equal(
                CosmosStrings.NonETagConcurrencyToken(nameof(Quarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    () => base.Properties_can_be_made_concurrency_tokens()).Message);

        public override void Properties_can_have_provider_type_set_for_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveConversion<byte[]>());

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down);
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<string>("__id").HasConversion(null);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks));

            Assert.Null(entityType.FindProperty("Up").GetProviderClrType());
            Assert.Same(typeof(byte[]), entityType.FindProperty("Down").GetProviderClrType());
            Assert.Null(entityType.FindProperty("Charm").GetProviderClrType());
            Assert.Same(typeof(byte[]), entityType.FindProperty("Strange").GetProviderClrType());
        }

        public override void Properties_can_be_set_to_generate_values_on_Add()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.Property(e => e.Up).ValueGeneratedOnAddOrUpdate();
                    b.Property(e => e.Down).ValueGeneratedNever();
                    b.Property<int>("Charm").Metadata.ValueGenerated = ValueGenerated.OnUpdateSometimes;
                    b.Property<string>("Strange").ValueGeneratedNever();
                    b.Property<int>("Top").ValueGeneratedOnAddOrUpdate();
                    b.Property<string>("Bottom").ValueGeneratedOnUpdate();
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks));
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty(Customer.IdProperty.Name).ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Up").ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Down").ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdateSometimes, entityType.FindProperty("Charm").ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Strange").ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Top").ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdate, entityType.FindProperty("Bottom").ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Partition_key_is_added_to_the_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { nameof(Customer.Id), nameof(Customer.AlternateKey) },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void Partition_key_is_added_to_the_alternate_key_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Single(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id_in_pk()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .HasKey(c => c.Name);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_id_is_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey));
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>().ToJsonProperty("id");

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey) },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
        }

        public override void Primitive_collections_can_be_made_concurrency_tokens()
            => Assert.Equal(
                CosmosStrings.NonETagConcurrencyToken(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    () => base.Primitive_collections_can_be_made_concurrency_tokens()).Message);

        [ConditionalFact]
        public virtual void Primitive_collections_key_is_added_to_the_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.Notes)
                .PrimitiveCollection(b => b.Notes);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.Id), nameof(Customer.Notes) },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.Notes) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName)!;
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_primitive_collection_mapped_to_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .PrimitiveCollection(c => c.Notes)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Single(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_primitive_collection_to_id_in_pk()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .PrimitiveCollection(c => c.Notes)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .HasKey(c => c.Notes);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
            => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
    }

    public class CosmosGenericComplexType : GenericComplexType
    {
        public override void Can_set_complex_property_annotation()
        {
            var modelBuilder = CreateModelBuilder();

            var complexPropertyBuilder = modelBuilder
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer)
                .HasTypeAnnotation("foo", "bar")
                .HasPropertyAnnotation("foo2", "bar2")
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders);

            var model = modelBuilder.FinalizeModel();
            var complexProperty = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single();

            Assert.Equal("bar", complexProperty.ComplexType["foo"]);
            Assert.Equal("bar2", complexProperty["foo2"]);
            Assert.Equal(typeof(Customer).Name, complexProperty.Name);
            Assert.Equal(
                @"Customer (Customer) Required
  ComplexType: ComplexProperties.Customer#Customer
    Properties: "
                + @"
      AlternateKey (Guid) Required
      Id (int) Required
      Name (string)
      Notes (List<string>)", complexProperty.ToDebugString(), ignoreLineEndingDifferences: true);
        }

        public override void Properties_can_have_provider_type_set_for_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveConversion<byte[]>());

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>(
                    b =>
                    {
                        b.Property<string>("__id").HasConversion(null);
                        b.ComplexProperty(
                            e => e.Quarks,
                            b =>
                            {
                                b.Property(e => e.Up);
                                b.Property(e => e.Down);
                                b.Property<int>("Charm");
                                b.Property<string>("Strange");
                            });
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Null(complexType.FindProperty("Up").GetProviderClrType());
            Assert.Same(typeof(byte[]), complexType.FindProperty("Down").GetProviderClrType());
            Assert.Null(complexType.FindProperty("Charm").GetProviderClrType());
            Assert.Same(typeof(byte[]), complexType.FindProperty("Strange").GetProviderClrType());
        }

        [ConditionalFact]
        public virtual void Partition_key_is_added_to_the_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { nameof(Customer.Id), nameof(Customer.AlternateKey) },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void Partition_key_is_added_to_the_alternate_key_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Single(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id_in_pk()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .HasKey(c => c.Name);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_id_is_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey));
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>().ToJsonProperty("id");

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer));

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey) },
                entity.FindPrimaryKey().Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
            => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
    }

    public class CosmosGenericInheritance : GenericInheritance
    {
        public override void Base_type_can_be_discovered_after_creating_foreign_keys_on_derived()
        {
            var mb = CreateModelBuilder();
            mb.Entity<AL>();
            mb.Entity<L>();

            var mutableEntityTypes = mb.Model.GetEntityTypes().Where(e => e.ClrType == typeof(Q)).ToList();

            Assert.Equal(2, mutableEntityTypes.Count);

            foreach (var mutableEntityType in mutableEntityTypes)
            {
                var mutableProperty = mutableEntityType.FindProperty(nameof(Q.ID));

                Assert.Equal(ValueGenerated.Never, mutableProperty.ValueGenerated);
            }
        }

        public override void Relationships_on_derived_types_are_discovered_first_if_base_is_one_sided()
            // Base discovered as owned
            => Assert.Throws<NullReferenceException>(
                () => base.Relationships_on_derived_types_are_discovered_first_if_base_is_one_sided());

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
            => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
    }

    public class CosmosGenericOneToMany : GenericOneToMany
    {
        public override void Navigation_to_shared_type_is_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionNavigationToSharedType>();

            var model = modelBuilder.FinalizeModel();

            var principal = model.FindEntityType(typeof(CollectionNavigationToSharedType));
            var owned = principal.FindNavigation(nameof(CollectionNavigationToSharedType.Navigation)).TargetEntityType;
            Assert.True(owned.IsOwned());
            Assert.True(owned.HasSharedClrType);
            Assert.Equal(
                "CollectionNavigationToSharedType.Navigation#Dictionary<string, object>",
                owned.DisplayName());
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
            => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
    }

    public class CosmosGenericManyToOne : GenericManyToOne
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
            => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
    }

    public class CosmosGenericOneToOne : GenericOneToOne
    {
        public override void Navigation_to_shared_type_is_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ReferenceNavigationToSharedType>();

            var model = modelBuilder.FinalizeModel();

            var principal = model.FindEntityType(typeof(ReferenceNavigationToSharedType));
            var owned = principal.FindNavigation(nameof(ReferenceNavigationToSharedType.Navigation)).TargetEntityType;
            Assert.True(owned.IsOwned());
            Assert.True(owned.HasSharedClrType);
            Assert.Equal(
                "ReferenceNavigationToSharedType.Navigation#Dictionary<string, object>",
                owned.DisplayName());
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
            => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
    }

    public class CosmosGenericManyToMany : GenericManyToMany
    {
        [ConditionalFact]
        public virtual void Can_use_shared_type_as_join_entity_with_partition_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinType",
                    e => e.HasOne<NavDependent>().WithMany().HasForeignKey("DependentId", "PartitionId"),
                    e => e.HasOne<ManyToManyNavPrincipal>().WithMany().HasForeignKey("PrincipalId", "PartitionId"),
                    e =>
                    {
                        e.HasPartitionKey("PartitionId");
                    });

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType("JoinType");
            Assert.NotNull(joinType);
            Assert.Equal(2, joinType.GetForeignKeys().Count());
            Assert.Equal(3, joinType.FindPrimaryKey().Properties.Count);
            Assert.Equal(6, joinType.GetProperties().Count());
            Assert.Equal("DbContext", joinType.GetContainer());
            Assert.Equal("PartitionId", joinType.GetPartitionKeyPropertyName());
            Assert.Equal("PartitionId", joinType.FindPrimaryKey().Properties.Last().Name);
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_join_entity_with_partition_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Ignore(e => e.Dependents);
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals);

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType("ManyToManyNavPrincipalNavDependent");
            Assert.NotNull(joinType);
            Assert.Equal(2, joinType.GetForeignKeys().Count());
            Assert.Equal(3, joinType.FindPrimaryKey().Properties.Count);
            Assert.Equal(6, joinType.GetProperties().Count());
            Assert.Equal("DbContext", joinType.GetContainer());
            Assert.Equal("PartitionId", joinType.GetPartitionKeyPropertyName());
            Assert.Equal("PartitionId", joinType.FindPrimaryKey().Properties.Last().Name);
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_join_entity_with_partition_keys_changed()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Property<string>("Partition2Id");
                    mb.HasPartitionKey("Partition2Id");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("Partition2Id");
                    mb.HasPartitionKey("Partition2Id");
                });

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType("ManyToManyNavPrincipalNavDependent");
            Assert.NotNull(joinType);
            Assert.Equal(2, joinType.GetForeignKeys().Count());
            Assert.Equal(3, joinType.FindPrimaryKey().Properties.Count);
            Assert.Equal(6, joinType.GetProperties().Count());
            Assert.Equal("DbContext", joinType.GetContainer());
            Assert.Equal("Partition2Id", joinType.GetPartitionKeyPropertyName());
            Assert.Equal("Partition2Id", joinType.FindPrimaryKey().Properties.Last().Name);
        }

        public override void Join_type_is_automatically_configured_by_convention()
            // Cosmos many-to-many. Issue #23523.
            => Assert.Equal(
                CoreStrings.NavigationNotAdded(
                    nameof(ImplicitManyToManyA), nameof(ImplicitManyToManyA.Bs), "List<ImplicitManyToManyB>"),
                Assert.Throws<InvalidOperationException>(
                    () => base.Join_type_is_automatically_configured_by_convention()).Message);

        public override void ForeignKeyAttribute_configures_the_properties()
            // Cosmos many-to-many. Issue #23523.
            => Assert.Equal(
                CoreStrings.NavigationNotAdded(
                    nameof(CategoryWithAttribute), nameof(CategoryWithAttribute.Products), "ICollection<ProductWithAttribute>"),
                Assert.Throws<InvalidOperationException>(
                    () => base.ForeignKeyAttribute_configures_the_properties()).Message);

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
            => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
    }

    public class CosmosGenericOwnedTypes : GenericOwnedTypes
    {
        public override void Deriving_from_owned_type_throws()
            // On Cosmos the base type starts as owned
            => Assert.Contains(
                "(No exception was thrown)",
                Assert.Throws<ThrowsException>(
                    () => base.Deriving_from_owned_type_throws()).Message);

        public override void Configuring_base_type_as_owned_throws()
            // On Cosmos the base type starts as owned
            => Assert.Contains(
                "(No exception was thrown)",
                Assert.Throws<ThrowsException>(
                    () => base.Deriving_from_owned_type_throws()).Message);

        [ConditionalFact]
        public virtual void Reference_type_is_discovered_as_owned()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OneToOneOwnerWithField>(
                e =>
                {
                    e.Property(p => p.Id);
                    e.Property(p => p.AlternateKey);
                    e.Property(p => p.Description);
                    e.HasKey(p => p.Id);
                });

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(OneToOneOwnerWithField));
            Assert.Equal(typeof(OneToOneOwnerWithField).FullName, owner.Name);
            var ownership = owner.FindNavigation(nameof(OneToOneOwnerWithField.OwnedDependent)).ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(OneToOneOwnerWithField.OwnedDependent), ownership.PrincipalToDependent.Name);
            Assert.Equal(nameof(OneToOneOwnedWithField.OneToOneOwner), ownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(OneToOneOwnerWithField.Id), ownership.PrincipalKey.Properties.Single().Name);
            var owned = ownership.DeclaringEntityType;
            Assert.Single(owned.GetForeignKeys());
            Assert.NotNull(model.FindEntityType(typeof(OneToOneOwnedWithField)));
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(OneToOneOwnedWithField)));
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
            => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
    }
}
