// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    public class FindEntryTest
    {
        [ConditionalFact]
        public virtual void Find_int_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new IntKey { Id = 87 }).Entity,
                context.Attach(new IntKey { Id = 88 }).Entity,
                context.Attach(new IntKey { Id = 89 }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(IntKey), 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<IntKey, int>(88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(IntKey), 99));
            Assert.Null(context.ChangeTracker.FindEntry<IntKey, int>(99));
        }

        [ConditionalFact]
        public virtual void Find_int_alternate_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
                context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
                context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(typeof(AlternateIntKey), nameof(AlternateIntKey.AlternateId), 88).Single().Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateIntKey, int>(nameof(AlternateIntKey.AlternateId), 88).Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(AlternateIntKey), nameof(AlternateIntKey.AlternateId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<AlternateIntKey, int>(nameof(AlternateIntKey.AlternateId), 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_int_foreign_key(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new IntKey { Id = 88 });
            }

            var entities = new[]
            {
                context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 87 }).Entity,
                context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
                context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
                context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 89 }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ForeignIntKey), nameof(ForeignIntKey.IntKeyId), 88)
                .Select(e => e.Entity)
                .Cast<ForeignIntKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ForeignIntKey, int>(nameof(ForeignIntKey.IntKeyId), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ForeignIntKey), nameof(ForeignIntKey.IntKeyId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ForeignIntKey, int>(nameof(ForeignIntKey.IntKeyId), 99));
        }

        [ConditionalFact]
        public virtual void Find_int_non_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 87 }).Entity,
                context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
                context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
                context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 89 }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(IntNonKey), nameof(IntNonKey.Int), 88)
                .Select(e => e.Entity)
                .Cast<IntNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<IntNonKey, int>(nameof(IntNonKey.Int), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(IntNonKey), nameof(IntNonKey.Int), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<IntNonKey, int>(nameof(IntNonKey.Int), 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_int_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowIntKey>(context, "Id", 87),
                CreateShadowEntity<ShadowIntKey>(context, "Id", 88),
                CreateShadowEntity<ShadowIntKey>(context, "Id", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(ShadowIntKey), 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<ShadowIntKey, int>(88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(ShadowIntKey), 99));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowIntKey, int>(99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_int_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 87),
                CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 88),
                CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.GetEntries(typeof(ShadowAlternateIntKey), "AlternateId", 88).Single().Entity);
            Assert.Same(entities[1], context.ChangeTracker.GetEntries<ShadowAlternateIntKey, int>("AlternateId", 88).Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowAlternateIntKey), "AlternateId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowAlternateIntKey, int>("AlternateId", 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_int_foreign_key(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new IntKey { Id = 88 });
            }

            var entities = new[]
            {
                CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 87),
                CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 88),
                CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 88),
                CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 89),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowForeignIntKey), "IntKeyId", 88)
                .Select(e => e.Entity)
                .Cast<ShadowForeignIntKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowForeignIntKey, int>("IntKeyId", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowForeignIntKey), "IntKeyId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowForeignIntKey, int>("IntKeyId", 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_int_non_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowIntNonKey>(context, "Int", 87),
                CreateShadowEntity<ShadowIntNonKey>(context, "Int", 88),
                CreateShadowEntity<ShadowIntNonKey>(context, "Int", 88),
                CreateShadowEntity<ShadowIntNonKey>(context, "Int", 89),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowIntNonKey), "Int", 88)
                .Select(e => e.Entity)
                .Cast<ShadowIntNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowIntNonKey, int>("Int", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowIntNonKey), "Int", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowIntNonKey, int>("Int", 99));
        }

        [ConditionalFact]
        public virtual void Find_int_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new IntKey { Id = 87 }).Entity,
                context.Attach(new IntKey { Id = 88 }).Entity,
                context.Attach(new IntKey { Id = 89 }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(IntKey).FullName!, 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<IntKey, int>(typeof(IntKey).FullName!, 88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(IntKey).FullName!, 99));
            Assert.Null(context.ChangeTracker.FindEntry<IntKey, int>(typeof(IntKey).FullName!, 99));
        }

        [ConditionalFact]
        public virtual void Find_int_alternate_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
                context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
                context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(typeof(AlternateIntKey).FullName!, nameof(AlternateIntKey.AlternateId), 88).Single()
                    .Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateIntKey, int>(
                    typeof(AlternateIntKey).FullName!, nameof(AlternateIntKey.AlternateId), 88).Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(AlternateIntKey).FullName!, nameof(AlternateIntKey.AlternateId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateIntKey, int>(
                    typeof(AlternateIntKey).FullName!, nameof(AlternateIntKey.AlternateId), 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_int_foreign_key_by_name(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new IntKey { Id = 88 });
            }

            var entities = new[]
            {
                context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 87 }).Entity,
                context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
                context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
                context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 89 }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ForeignIntKey).FullName!, nameof(ForeignIntKey.IntKeyId), 88)
                .Select(e => e.Entity)
                .Cast<ForeignIntKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ForeignIntKey, int>(typeof(ForeignIntKey).FullName!, nameof(ForeignIntKey.IntKeyId), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ForeignIntKey).FullName!, nameof(ForeignIntKey.IntKeyId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ForeignIntKey, int>(
                    typeof(ForeignIntKey).FullName!, nameof(ForeignIntKey.IntKeyId), 99));
        }

        [ConditionalFact]
        public virtual void Find_int_non_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 87 }).Entity,
                context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
                context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
                context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 89 }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(IntNonKey).FullName!, nameof(IntNonKey.Int), 88)
                .Select(e => e.Entity)
                .Cast<IntNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<IntNonKey, int>(typeof(IntNonKey).FullName!, nameof(IntNonKey.Int), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(IntNonKey).FullName!, nameof(IntNonKey.Int), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<IntNonKey, int>(typeof(IntNonKey).FullName!, nameof(IntNonKey.Int), 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_int_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowIntKey>(context, "Id", 87),
                CreateShadowEntity<ShadowIntKey>(context, "Id", 88),
                CreateShadowEntity<ShadowIntKey>(context, "Id", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(ShadowIntKey).FullName!, 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<ShadowIntKey, int>(typeof(ShadowIntKey).FullName!, 88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(ShadowIntKey).FullName!, 99));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowIntKey, int>(typeof(ShadowIntKey).FullName!, 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_int_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 87),
                CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 88),
                CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 89),
            };

            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries(
                    typeof(ShadowAlternateIntKey).FullName!, "AlternateId", 88).Single().Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries<ShadowAlternateIntKey, int>(
                    typeof(ShadowAlternateIntKey).FullName!, "AlternateId", 88).Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowAlternateIntKey).FullName!, "AlternateId", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowAlternateIntKey, int>(
                    typeof(ShadowAlternateIntKey).FullName!, "AlternateId", 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_int_foreign_key_by_name(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                CreateShadowEntity<ShadowIntKey>(context, "Id", 88);
            }

            var entities = new[]
            {
                CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 87),
                CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 88),
                CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 88),
                CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 89),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowForeignIntKey).FullName!, "IntKeyId", 88)
                .Select(e => e.Entity)
                .Cast<ShadowForeignIntKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowForeignIntKey, int>(typeof(ShadowForeignIntKey).FullName!, "IntKeyId", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowForeignIntKey).FullName!, "IntKeyId", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowForeignIntKey, int>(
                    typeof(ShadowForeignIntKey).FullName!, "IntKeyId", 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_int_non_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowIntNonKey>(context, "Int", 87),
                CreateShadowEntity<ShadowIntNonKey>(context, "Int", 88),
                CreateShadowEntity<ShadowIntNonKey>(context, "Int", 88),
                CreateShadowEntity<ShadowIntNonKey>(context, "Int", 89),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowIntNonKey).FullName!, "Int", 88)
                .Select(e => e.Entity)
                .Cast<ShadowIntNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowIntNonKey, int>(typeof(ShadowIntNonKey).FullName!, "Int", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowIntNonKey).FullName!, "Int", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowIntNonKey, int>(typeof(ShadowIntNonKey).FullName!, "Int", 99));
        }

        [ConditionalFact]
        public virtual void Find_int_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<IntKey>("IntKeyA").Attach(new IntKey { Id = 87 }).Entity,
                context.Set<IntKey>("IntKeyA").Attach(new IntKey { Id = 88 }).Entity,
                context.Set<IntKey>("IntKeyA").Attach(new IntKey { Id = 89 }).Entity,
                context.Set<IntKey>("IntKeyB").Attach(new IntKey { Id = 87 }).Entity,
                context.Set<IntKey>("IntKeyB").Attach(new IntKey { Id = 88 }).Entity,
                context.Set<IntKey>("IntKeyB").Attach(new IntKey { Id = 89 }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry("IntKeyA", 88)!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry("IntKeyB", 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<IntKey, int>("IntKeyA", 88)!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry<IntKey, int>("IntKeyB", 88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry("IntKeyA", 99));
            Assert.Null(context.ChangeTracker.FindEntry("IntKeyB", 99));
            Assert.Null(context.ChangeTracker.FindEntry<IntKey, int>("IntKeyA", 99));
            Assert.Null(context.ChangeTracker.FindEntry<IntKey, int>("IntKeyB", 99));
        }

        [ConditionalFact]
        public virtual void Find_int_alternate_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<AlternateIntKey>("AlternateIntKeyA")
                    .Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
                context.Set<AlternateIntKey>("AlternateIntKeyA")
                    .Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
                context.Set<AlternateIntKey>("AlternateIntKeyA")
                    .Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
                context.Set<AlternateIntKey>("AlternateIntKeyB")
                    .Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
                context.Set<AlternateIntKey>("AlternateIntKeyB")
                    .Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
                context.Set<AlternateIntKey>("AlternateIntKeyB")
                    .Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries("AlternateIntKeyA", nameof(AlternateIntKey.AlternateId), 88).Single().Entity);
            Assert.Same(
                entities[4],
                context.ChangeTracker.GetEntries("AlternateIntKeyB", nameof(AlternateIntKey.AlternateId), 88).Single().Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateIntKey, int>(
                    "AlternateIntKeyA", nameof(AlternateIntKey.AlternateId), 88).Single().Entity);
            Assert.Same(
                entities[4],
                context.ChangeTracker.GetEntries<AlternateIntKey, int>(
                    "AlternateIntKeyB", nameof(AlternateIntKey.AlternateId), 88).Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries("AlternateIntKeyA", nameof(AlternateIntKey.AlternateId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries("AlternateIntKeyB", nameof(AlternateIntKey.AlternateId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateIntKey, int>(
                    "AlternateIntKeyA", nameof(AlternateIntKey.AlternateId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateIntKey, int>(
                    "AlternateIntKeyB", nameof(AlternateIntKey.AlternateId), 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_int_foreign_key_shared(bool trackPrincipal)
        {
            using var context = new FindContextShared();

            if (trackPrincipal)
            {
                context.Set<IntKey>("IntKeyA").Attach(new IntKey { Id = 88 });
                context.Set<IntKey>("IntKeyB").Attach(new IntKey { Id = 88 });
            }

            var entities = new[]
            {
                context.Set<ForeignIntKey>("ForeignIntKeyA").Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 87 }).Entity,
                context.Set<ForeignIntKey>("ForeignIntKeyA").Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
                context.Set<ForeignIntKey>("ForeignIntKeyA").Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
                context.Set<ForeignIntKey>("ForeignIntKeyA").Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 89 }).Entity,
                context.Set<ForeignIntKey>("ForeignIntKeyB").Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 87 }).Entity,
                context.Set<ForeignIntKey>("ForeignIntKeyB").Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
                context.Set<ForeignIntKey>("ForeignIntKeyB").Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
                context.Set<ForeignIntKey>("ForeignIntKeyB").Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 89 }).Entity,
            };

            var foundA = context.ChangeTracker
                .GetEntries("ForeignIntKeyA", nameof(ForeignIntKey.IntKeyId), 88)
                .Select(e => e.Entity)
                .Cast<ForeignIntKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ForeignIntKey, int>("ForeignIntKeyA", nameof(ForeignIntKey.IntKeyId), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ForeignIntKeyB", nameof(ForeignIntKey.IntKeyId), 88)
                .Select(e => e.Entity)
                .Cast<ForeignIntKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ForeignIntKey, int>("ForeignIntKeyB", nameof(ForeignIntKey.IntKeyId), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ForeignIntKeyA", nameof(ForeignIntKey.IntKeyId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ForeignIntKey, int>("ForeignIntKeyA", nameof(ForeignIntKey.IntKeyId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries("ForeignIntKeyB", nameof(ForeignIntKey.IntKeyId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ForeignIntKey, int>("ForeignIntKeyB", nameof(ForeignIntKey.IntKeyId), 99));
        }

        [ConditionalFact]
        public virtual void Find_int_non_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<IntNonKey>("IntNonKeyA").Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 87 }).Entity,
                context.Set<IntNonKey>("IntNonKeyA").Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
                context.Set<IntNonKey>("IntNonKeyA").Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
                context.Set<IntNonKey>("IntNonKeyA").Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 89 }).Entity,
                context.Set<IntNonKey>("IntNonKeyB").Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 87 }).Entity,
                context.Set<IntNonKey>("IntNonKeyB").Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
                context.Set<IntNonKey>("IntNonKeyB").Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
                context.Set<IntNonKey>("IntNonKeyB").Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 89 }).Entity,
            };

            var foundA = context.ChangeTracker
                .GetEntries("IntNonKeyA", nameof(IntNonKey.Int), 88)
                .Select(e => e.Entity)
                .Cast<IntNonKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<IntNonKey, int>("IntNonKeyA", nameof(IntNonKey.Int), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("IntNonKeyB", nameof(IntNonKey.Int), 88)
                .Select(e => e.Entity)
                .Cast<IntNonKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<IntNonKey, int>("IntNonKeyB", nameof(IntNonKey.Int), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("IntNonKeyA", nameof(IntNonKey.Int), 99));
            Assert.Empty(context.ChangeTracker.GetEntries("IntNonKeyB", nameof(IntNonKey.Int), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<IntNonKey, int>("IntNonKeyA", nameof(IntNonKey.Int), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<IntNonKey, int>("IntNonKeyB", nameof(IntNonKey.Int), 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_int_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyA", "Id", 87),
                CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyA", "Id", 88),
                CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyA", "Id", 89),
                CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyB", "Id", 87),
                CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyB", "Id", 88),
                CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyB", "Id", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry("ShadowIntKeyA", 88)!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry("ShadowIntKeyB", 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<ShadowIntKey, int>("ShadowIntKeyA", 88)!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry<ShadowIntKey, int>("ShadowIntKeyB", 88)!.Entity);

            Assert.Null(context.ChangeTracker.FindEntry("ShadowIntKeyA", 99));
            Assert.Null(context.ChangeTracker.FindEntry("ShadowIntKeyB", 99));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowIntKey, int>("ShadowIntKeyA", 99));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowIntKey, int>("ShadowIntKeyB", 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_int_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyA", "AlternateId", 87),
                CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyA", "AlternateId", 88),
                CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyA", "AlternateId", 89),
                CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyB", "AlternateId", 87),
                CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyB", "AlternateId", 88),
                CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyB", "AlternateId", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.GetEntries("ShadowAlternateIntKeyA", "AlternateId", 88).Single().Entity);
            Assert.Same(entities[4], context.ChangeTracker.GetEntries("ShadowAlternateIntKeyB", "AlternateId", 88).Single().Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries<ShadowAlternateIntKey, int>(
                    "ShadowAlternateIntKeyA", "AlternateId", 88).Single().Entity);
            Assert.Same(
                entities[4], context.ChangeTracker.GetEntries<ShadowAlternateIntKey, int>(
                    "ShadowAlternateIntKeyB", "AlternateId", 88).Single().Entity);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowAlternateIntKeyA", "AlternateId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowAlternateIntKeyB", "AlternateId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowAlternateIntKey, int>("ShadowAlternateIntKeyA", "AlternateId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowAlternateIntKey, int>("ShadowAlternateIntKeyB", "AlternateId", 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_int_foreign_key_shared(bool trackPrincipal)
        {
            using var context = new FindContextShared();

            if (trackPrincipal)
            {
                CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyA", "Id", 88);
                CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyB", "Id", 88);
            }

            var entities = new[]
            {
                CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyA", "IntKeyId", 87),
                CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyA", "IntKeyId", 88),
                CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyA", "IntKeyId", 88),
                CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyA", "IntKeyId", 89),
                CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyB", "IntKeyId", 87),
                CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyB", "IntKeyId", 88),
                CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyB", "IntKeyId", 88),
                CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyB", "IntKeyId", 89),
            };

            var foundA = context.ChangeTracker
                .GetEntries("ShadowForeignIntKeyA", "IntKeyId", 88)
                .Select(e => e.Entity)
                .Cast<ShadowForeignIntKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ShadowForeignIntKey, int>("ShadowForeignIntKeyA", "IntKeyId", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ShadowForeignIntKeyB", "IntKeyId", 88)
                .Select(e => e.Entity)
                .Cast<ShadowForeignIntKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ShadowForeignIntKey, int>("ShadowForeignIntKeyB", "IntKeyId", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowForeignIntKeyA", "IntKeyId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowForeignIntKeyB", "IntKeyId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowForeignIntKey, int>("ShadowForeignIntKeyA", "IntKeyId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowForeignIntKey, int>("ShadowForeignIntKeyB", "IntKeyId", 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_int_non_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyA", "Int", 87),
                CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyA", "Int", 88),
                CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyA", "Int", 88),
                CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyA", "Int", 89),
                CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyB", "Int", 87),
                CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyB", "Int", 88),
                CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyB", "Int", 88),
                CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyB", "Int", 89),
            };

            var foundA = context.ChangeTracker
                .GetEntries("ShadowIntNonKeyA", "Int", 88)
                .Select(e => e.Entity)
                .Cast<ShadowIntNonKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ShadowIntNonKey, int>("ShadowIntNonKeyA", "Int", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ShadowIntNonKeyB", "Int", 88)
                .Select(e => e.Entity)
                .Cast<ShadowIntNonKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ShadowIntNonKey, int>("ShadowIntNonKeyB", "Int", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowIntNonKeyA", "Int", 99));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowIntNonKeyB", "Int", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowIntNonKey, int>("ShadowIntNonKeyA", "Int", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowIntNonKey, int>("ShadowIntNonKeyB", "Int", 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new NullableIntKey { Id = 87 }).Entity,
                context.Attach(new NullableIntKey { Id = 88 }).Entity,
                context.Attach(new NullableIntKey { Id = 89 }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(NullableIntKey), 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<NullableIntKey, int?>(88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(NullableIntKey), 99));
            Assert.Null(context.ChangeTracker.FindEntry<NullableIntKey, int?>(99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_alternate_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
                context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
                context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(typeof(AlternateNullableIntKey), nameof(AlternateNullableIntKey.AlternateId), 88).Single()
                    .Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateNullableIntKey, int?>(nameof(AlternateNullableIntKey.AlternateId), 88).Single()
                    .Entity);
            Assert.Empty(
                context.ChangeTracker.GetEntries(typeof(AlternateNullableIntKey), nameof(AlternateNullableIntKey.AlternateId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<AlternateNullableIntKey, int?>(nameof(AlternateNullableIntKey.AlternateId), 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_nullable_int_foreign_key(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new NullableIntKey { Id = 88 });
            }

            var entities = new[]
            {
                context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 87 }).Entity,
                context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
                context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
                context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 89 }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ForeignNullableIntKey), nameof(ForeignNullableIntKey.NullableIntKeyId), 88)
                .Select(e => e.Entity)
                .Cast<ForeignNullableIntKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ForeignNullableIntKey, int?>(nameof(ForeignNullableIntKey.NullableIntKeyId), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(
                context.ChangeTracker.GetEntries(typeof(ForeignNullableIntKey), nameof(ForeignNullableIntKey.NullableIntKeyId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ForeignNullableIntKey, int?>(nameof(ForeignNullableIntKey.NullableIntKeyId), 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_non_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 87 }).Entity,
                context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
                context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
                context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 89 }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(NullableIntNonKey), nameof(NullableIntNonKey.NullableInt), 88)
                .Select(e => e.Entity)
                .Cast<NullableIntNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<NullableIntNonKey, int?>(nameof(NullableIntNonKey.NullableInt), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(NullableIntNonKey), nameof(NullableIntNonKey.NullableInt), 99));
            Assert.Empty(context.ChangeTracker.GetEntries<NullableIntNonKey, int?>(nameof(NullableIntNonKey.NullableInt), 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_nullable_int_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 87),
                CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 88),
                CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(ShadowNullableIntKey), 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<ShadowNullableIntKey, int?>(88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(ShadowNullableIntKey), 99));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowNullableIntKey, int?>(99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_nullable_int_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 87),
                CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 88),
                CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 89),
            };

            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries(typeof(ShadowAlternateNullableIntKey), "AlternateId", 88).Single().Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries<ShadowAlternateNullableIntKey, int?>("AlternateId", 88).Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowAlternateNullableIntKey), "AlternateId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowAlternateNullableIntKey, int?>("AlternateId", 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_nullable_int_foreign_key(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new NullableIntKey { Id = 88 });
            }

            var entities = new[]
            {
                CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 87),
                CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 88),
                CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 88),
                CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 89),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowForeignNullableIntKey), "NullableIntKeyId", 88)
                .Select(e => e.Entity)
                .Cast<ShadowForeignNullableIntKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowForeignNullableIntKey, int?>("NullableIntKeyId", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowForeignNullableIntKey), "NullableIntKeyId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowForeignNullableIntKey, int?>("NullableIntKeyId", 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_nullable_int_non_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 87),
                CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 88),
                CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 88),
                CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 89),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowNullableIntNonKey), "NullableInt", 88)
                .Select(e => e.Entity)
                .Cast<ShadowNullableIntNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowNullableIntNonKey, int?>("NullableInt", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowNullableIntNonKey), "NullableInt", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowNullableIntNonKey, int?>("NullableInt", 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new NullableIntKey { Id = 87 }).Entity,
                context.Attach(new NullableIntKey { Id = 88 }).Entity,
                context.Attach(new NullableIntKey { Id = 89 }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(NullableIntKey).FullName!, 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<NullableIntKey, int?>(typeof(NullableIntKey).FullName!, 88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(NullableIntKey).FullName!, 99));
            Assert.Null(context.ChangeTracker.FindEntry<NullableIntKey, int?>(typeof(NullableIntKey).FullName!, 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_alternate_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
                context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
                context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(typeof(AlternateNullableIntKey).FullName!, nameof(AlternateNullableIntKey.AlternateId), 88)
                    .Single().Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateNullableIntKey, int?>(
                    typeof(AlternateNullableIntKey).FullName!, nameof(AlternateNullableIntKey.AlternateId), 88).Single().Entity);
            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(AlternateNullableIntKey).FullName!, nameof(AlternateNullableIntKey.AlternateId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateNullableIntKey, int?>(
                    typeof(AlternateNullableIntKey).FullName!, nameof(AlternateNullableIntKey.AlternateId), 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_nullable_int_foreign_key_by_name(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new NullableIntKey { Id = 88 });
            }

            var entities = new[]
            {
                context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 87 }).Entity,
                context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
                context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
                context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 89 }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ForeignNullableIntKey).FullName!, nameof(ForeignNullableIntKey.NullableIntKeyId), 88)
                .Select(e => e.Entity)
                .Cast<ForeignNullableIntKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ForeignNullableIntKey, int?>(
                    typeof(ForeignNullableIntKey).FullName!, nameof(ForeignNullableIntKey.NullableIntKeyId), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ForeignNullableIntKey).FullName!, nameof(ForeignNullableIntKey.NullableIntKeyId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ForeignNullableIntKey, int?>(
                    typeof(ForeignNullableIntKey).FullName!, nameof(ForeignNullableIntKey.NullableIntKeyId), 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_non_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 87 }).Entity,
                context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
                context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
                context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 89 }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(NullableIntNonKey).FullName!, nameof(NullableIntNonKey.NullableInt), 88)
                .Select(e => e.Entity)
                .Cast<NullableIntNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<NullableIntNonKey, int?>(typeof(NullableIntNonKey).FullName!, nameof(NullableIntNonKey.NullableInt), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(NullableIntNonKey).FullName!, nameof(NullableIntNonKey.NullableInt), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<NullableIntNonKey, int?>(
                    typeof(NullableIntNonKey).FullName!, nameof(NullableIntNonKey.NullableInt), 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_nullable_int_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 87),
                CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 88),
                CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(ShadowNullableIntKey).FullName!, 88)!.Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.FindEntry<ShadowNullableIntKey, int?>(typeof(ShadowNullableIntKey).FullName!, 88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(ShadowNullableIntKey).FullName!, 99));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowNullableIntKey, int?>(typeof(ShadowNullableIntKey).FullName!, 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_nullable_int_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 87),
                CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 88),
                CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 89),
            };

            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries(
                    typeof(ShadowAlternateNullableIntKey).FullName!, "AlternateId", 88).Single().Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries<ShadowAlternateNullableIntKey, int?>(
                    typeof(ShadowAlternateNullableIntKey).FullName!, "AlternateId", 88).Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowAlternateNullableIntKey).FullName!, "AlternateId", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowAlternateNullableIntKey, int?>(
                    typeof(ShadowAlternateNullableIntKey).FullName!, "AlternateId", 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_nullable_int_foreign_key_by_name(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 88);
            }

            var entities = new[]
            {
                CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 87),
                CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 88),
                CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 88),
                CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 89),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowForeignNullableIntKey).FullName!, "NullableIntKeyId", 88)
                .Select(e => e.Entity)
                .Cast<ShadowForeignNullableIntKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowForeignNullableIntKey, int?>(typeof(ShadowForeignNullableIntKey).FullName!, "NullableIntKeyId", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowForeignNullableIntKey).FullName!, "NullableIntKeyId", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowForeignNullableIntKey, int?>(
                    typeof(ShadowForeignNullableIntKey).FullName!, "NullableIntKeyId", 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_nullable_int_non_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 87),
                CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 88),
                CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 88),
                CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 89),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowNullableIntNonKey).FullName!, "NullableInt", 88)
                .Select(e => e.Entity)
                .Cast<ShadowNullableIntNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowNullableIntNonKey, int?>(typeof(ShadowNullableIntNonKey).FullName!, "NullableInt", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowNullableIntNonKey).FullName!, "NullableInt", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowNullableIntNonKey, int?>(
                    typeof(ShadowNullableIntNonKey).FullName!, "NullableInt", 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<NullableIntKey>("NullableIntKeyA").Attach(new NullableIntKey { Id = 87 }).Entity,
                context.Set<NullableIntKey>("NullableIntKeyA").Attach(new NullableIntKey { Id = 88 }).Entity,
                context.Set<NullableIntKey>("NullableIntKeyA").Attach(new NullableIntKey { Id = 89 }).Entity,
                context.Set<NullableIntKey>("NullableIntKeyB").Attach(new NullableIntKey { Id = 87 }).Entity,
                context.Set<NullableIntKey>("NullableIntKeyB").Attach(new NullableIntKey { Id = 88 }).Entity,
                context.Set<NullableIntKey>("NullableIntKeyB").Attach(new NullableIntKey { Id = 89 }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry("NullableIntKeyA", 88)!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry("NullableIntKeyB", 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<NullableIntKey, int?>("NullableIntKeyA", 88)!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry<NullableIntKey, int?>("NullableIntKeyB", 88)!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry("NullableIntKeyA", 99));
            Assert.Null(context.ChangeTracker.FindEntry("NullableIntKeyB", 99));
            Assert.Null(context.ChangeTracker.FindEntry<NullableIntKey, int?>("NullableIntKeyA", 99));
            Assert.Null(context.ChangeTracker.FindEntry<NullableIntKey, int?>("NullableIntKeyB", 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_alternate_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<AlternateNullableIntKey>("AlternateNullableIntKeyA")
                    .Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
                context.Set<AlternateNullableIntKey>("AlternateNullableIntKeyA")
                    .Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
                context.Set<AlternateNullableIntKey>("AlternateNullableIntKeyA")
                    .Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
                context.Set<AlternateNullableIntKey>("AlternateNullableIntKeyB")
                    .Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
                context.Set<AlternateNullableIntKey>("AlternateNullableIntKeyB")
                    .Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
                context.Set<AlternateNullableIntKey>("AlternateNullableIntKeyB")
                    .Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries("AlternateNullableIntKeyA", nameof(AlternateNullableIntKey.AlternateId), 88).Single()
                    .Entity);
            Assert.Same(
                entities[4],
                context.ChangeTracker.GetEntries("AlternateNullableIntKeyB", nameof(AlternateNullableIntKey.AlternateId), 88).Single()
                    .Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateNullableIntKey, int?>(
                    "AlternateNullableIntKeyA", nameof(AlternateNullableIntKey.AlternateId), 88).Single().Entity);
            Assert.Same(
                entities[4],
                context.ChangeTracker.GetEntries<AlternateNullableIntKey, int?>(
                    "AlternateNullableIntKeyB", nameof(AlternateNullableIntKey.AlternateId), 88).Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries("AlternateNullableIntKeyA", nameof(AlternateNullableIntKey.AlternateId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries("AlternateNullableIntKeyB", nameof(AlternateNullableIntKey.AlternateId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateNullableIntKey, int?>(
                    "AlternateNullableIntKeyA", nameof(AlternateNullableIntKey.AlternateId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateNullableIntKey, int?>(
                    "AlternateNullableIntKeyB", nameof(AlternateNullableIntKey.AlternateId), 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_nullable_int_foreign_key_shared(bool trackPrincipal)
        {
            using var context = new FindContextShared();

            if (trackPrincipal)
            {
                context.Set<NullableIntKey>("NullableIntKeyA").Attach(new NullableIntKey { Id = 88 });
                context.Set<NullableIntKey>("NullableIntKeyB").Attach(new NullableIntKey { Id = 88 });
            }

            var entities = new[]
            {
                context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyA")
                    .Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 87 }).Entity,
                context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyA")
                    .Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
                context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyA")
                    .Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
                context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyA")
                    .Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 89 }).Entity,
                context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyB")
                    .Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 87 }).Entity,
                context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyB")
                    .Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
                context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyB")
                    .Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
                context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyB")
                    .Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 89 }).Entity,
            };

            var foundA = context.ChangeTracker
                .GetEntries("ForeignNullableIntKeyA", nameof(ForeignNullableIntKey.NullableIntKeyId), 88)
                .Select(e => e.Entity)
                .Cast<ForeignNullableIntKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ForeignNullableIntKey, int?>("ForeignNullableIntKeyA", nameof(ForeignNullableIntKey.NullableIntKeyId), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ForeignNullableIntKeyB", nameof(ForeignNullableIntKey.NullableIntKeyId), 88)
                .Select(e => e.Entity)
                .Cast<ForeignNullableIntKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ForeignNullableIntKey, int?>("ForeignNullableIntKeyB", nameof(ForeignNullableIntKey.NullableIntKeyId), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ForeignNullableIntKeyA", nameof(ForeignNullableIntKey.NullableIntKeyId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ForeignNullableIntKey, int?>(
                    "ForeignNullableIntKeyA", nameof(ForeignNullableIntKey.NullableIntKeyId), 99));
            Assert.Empty(context.ChangeTracker.GetEntries("ForeignNullableIntKeyB", nameof(ForeignNullableIntKey.NullableIntKeyId), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ForeignNullableIntKey, int?>(
                    "ForeignNullableIntKeyB", nameof(ForeignNullableIntKey.NullableIntKeyId), 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_non_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<NullableIntNonKey>("NullableIntNonKeyA")
                    .Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 87 }).Entity,
                context.Set<NullableIntNonKey>("NullableIntNonKeyA")
                    .Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
                context.Set<NullableIntNonKey>("NullableIntNonKeyA")
                    .Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
                context.Set<NullableIntNonKey>("NullableIntNonKeyA")
                    .Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 89 }).Entity,
                context.Set<NullableIntNonKey>("NullableIntNonKeyB")
                    .Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 87 }).Entity,
                context.Set<NullableIntNonKey>("NullableIntNonKeyB")
                    .Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
                context.Set<NullableIntNonKey>("NullableIntNonKeyB")
                    .Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
                context.Set<NullableIntNonKey>("NullableIntNonKeyB")
                    .Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 89 }).Entity,
            };

            var foundA = context.ChangeTracker
                .GetEntries("NullableIntNonKeyA", nameof(NullableIntNonKey.NullableInt), 88)
                .Select(e => e.Entity)
                .Cast<NullableIntNonKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<NullableIntNonKey, int?>("NullableIntNonKeyA", nameof(NullableIntNonKey.NullableInt), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("NullableIntNonKeyB", nameof(NullableIntNonKey.NullableInt), 88)
                .Select(e => e.Entity)
                .Cast<NullableIntNonKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<NullableIntNonKey, int?>("NullableIntNonKeyB", nameof(NullableIntNonKey.NullableInt), 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("NullableIntNonKeyA", nameof(NullableIntNonKey.NullableInt), 99));
            Assert.Empty(context.ChangeTracker.GetEntries("NullableIntNonKeyB", nameof(NullableIntNonKey.NullableInt), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<NullableIntNonKey, int?>("NullableIntNonKeyA", nameof(NullableIntNonKey.NullableInt), 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<NullableIntNonKey, int?>("NullableIntNonKeyB", nameof(NullableIntNonKey.NullableInt), 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_nullable_int_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyA", "Id", 87),
                CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyA", "Id", 88),
                CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyA", "Id", 89),
                CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyB", "Id", 87),
                CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyB", "Id", 88),
                CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyB", "Id", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry("ShadowNullableIntKeyA", 88)!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry("ShadowNullableIntKeyB", 88)!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<ShadowNullableIntKey, int?>("ShadowNullableIntKeyA", 88)!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry<ShadowNullableIntKey, int?>("ShadowNullableIntKeyB", 88)!.Entity);

            Assert.Null(context.ChangeTracker.FindEntry("ShadowNullableIntKeyA", 99));
            Assert.Null(context.ChangeTracker.FindEntry("ShadowNullableIntKeyB", 99));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowNullableIntKey, int?>("ShadowNullableIntKeyA", 99));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowNullableIntKey, int?>("ShadowNullableIntKeyB", 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_nullable_int_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyA", "AlternateId", 87),
                CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyA", "AlternateId", 88),
                CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyA", "AlternateId", 89),
                CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyB", "AlternateId", 87),
                CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyB", "AlternateId", 88),
                CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyB", "AlternateId", 89),
            };

            Assert.Same(entities[1], context.ChangeTracker.GetEntries("ShadowAlternateNullableIntKeyA", "AlternateId", 88).Single().Entity);
            Assert.Same(entities[4], context.ChangeTracker.GetEntries("ShadowAlternateNullableIntKeyB", "AlternateId", 88).Single().Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries<ShadowAlternateNullableIntKey, int?>(
                    "ShadowAlternateNullableIntKeyA", "AlternateId", 88).Single().Entity);
            Assert.Same(
                entities[4], context.ChangeTracker.GetEntries<ShadowAlternateNullableIntKey, int?>(
                    "ShadowAlternateNullableIntKeyB", "AlternateId", 88).Single().Entity);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowAlternateNullableIntKeyA", "AlternateId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowAlternateNullableIntKeyB", "AlternateId", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowAlternateNullableIntKey, int?>("ShadowAlternateNullableIntKeyA", "AlternateId", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowAlternateNullableIntKey, int?>("ShadowAlternateNullableIntKeyB", "AlternateId", 99));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_nullable_int_foreign_key_shared(bool trackPrincipal)
        {
            using var context = new FindContextShared();

            if (trackPrincipal)
            {
                CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyA", "Id", 88);
                CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyB", "Id", 88);
            }

            var entities = new[]
            {
                CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 87),
                CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 88),
                CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 88),
                CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 89),
                CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 87),
                CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 88),
                CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 88),
                CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 89),
            };

            var foundA = context.ChangeTracker
                .GetEntries("ShadowForeignNullableIntKeyA", "NullableIntKeyId", 88)
                .Select(e => e.Entity)
                .Cast<ShadowForeignNullableIntKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ShadowForeignNullableIntKey, int?>("ShadowForeignNullableIntKeyA", "NullableIntKeyId", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ShadowForeignNullableIntKeyB", "NullableIntKeyId", 88)
                .Select(e => e.Entity)
                .Cast<ShadowForeignNullableIntKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ShadowForeignNullableIntKey, int?>("ShadowForeignNullableIntKeyB", "NullableIntKeyId", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowForeignNullableIntKeyA", "NullableIntKeyId", 99));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowForeignNullableIntKeyB", "NullableIntKeyId", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowForeignNullableIntKey, int?>(
                    "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 99));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowForeignNullableIntKey, int?>(
                    "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 99));
        }

        [ConditionalFact]
        public virtual void Find_shadow_nullable_int_non_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyA", "NullableInt", 87),
                CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyA", "NullableInt", 88),
                CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyA", "NullableInt", 88),
                CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyA", "NullableInt", 89),
                CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyB", "NullableInt", 87),
                CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyB", "NullableInt", 88),
                CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyB", "NullableInt", 88),
                CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyB", "NullableInt", 89),
            };

            var foundA = context.ChangeTracker
                .GetEntries("ShadowNullableIntNonKeyA", "NullableInt", 88)
                .Select(e => e.Entity)
                .Cast<ShadowNullableIntNonKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ShadowNullableIntNonKey, int?>("ShadowNullableIntNonKeyA", "NullableInt", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ShadowNullableIntNonKeyB", "NullableInt", 88)
                .Select(e => e.Entity)
                .Cast<ShadowNullableIntNonKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ShadowNullableIntNonKey, int?>("ShadowNullableIntNonKeyB", "NullableInt", 88)
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowNullableIntNonKeyA", "NullableInt", 99));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowNullableIntNonKeyB", "NullableInt", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowNullableIntNonKey, int?>("ShadowNullableIntNonKeyA", "NullableInt", 99));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowNullableIntNonKey, int?>("ShadowNullableIntNonKeyB", "NullableInt", 99));
        }

        [ConditionalFact]
        public virtual void Find_string_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new StringKey { Id = "87" }).Entity,
                context.Attach(new StringKey { Id = "88" }).Entity,
                context.Attach(new StringKey { Id = "89" }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(StringKey), "88")!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<StringKey, string>("88")!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(StringKey), "99"));
            Assert.Null(context.ChangeTracker.FindEntry<StringKey, string>("99"));
        }

        [ConditionalFact]
        public virtual void Find_string_alternate_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "87" }).Entity,
                context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "88" }).Entity,
                context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "89" }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(typeof(AlternateStringKey), nameof(AlternateStringKey.AlternateId), "88").Single().Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateStringKey, string>(nameof(AlternateStringKey.AlternateId), "88").Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(AlternateStringKey), nameof(AlternateStringKey.AlternateId), "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<AlternateStringKey, string>(nameof(AlternateStringKey.AlternateId), "99"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_string_foreign_key(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new StringKey { Id = "88" });
            }

            var entities = new[]
            {
                context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "87" }).Entity,
                context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
                context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
                context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "89" }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ForeignStringKey), nameof(ForeignStringKey.StringKeyId), "88")
                .Select(e => e.Entity)
                .Cast<ForeignStringKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ForeignStringKey, string>(nameof(ForeignStringKey.StringKeyId), "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ForeignStringKey), nameof(ForeignStringKey.StringKeyId), "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<ForeignStringKey, string>(nameof(ForeignStringKey.StringKeyId), "99"));
        }

        [ConditionalFact]
        public virtual void Find_string_non_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "87" }).Entity,
                context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
                context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
                context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "89" }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(StringNonKey), nameof(StringNonKey.String), "88")
                .Select(e => e.Entity)
                .Cast<StringNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<StringNonKey, string>(nameof(StringNonKey.String), "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(StringNonKey), nameof(StringNonKey.String), "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<StringNonKey, string>(nameof(StringNonKey.String), "99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_string_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowStringKey>(context, "Id", "87"),
                CreateShadowEntity<ShadowStringKey>(context, "Id", "88"),
                CreateShadowEntity<ShadowStringKey>(context, "Id", "89"),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(ShadowStringKey), "88")!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<ShadowStringKey, string>("88")!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(ShadowStringKey), "99"));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowStringKey, string>("99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_string_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "87"),
                CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "88"),
                CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "89"),
            };

            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries(typeof(ShadowAlternateStringKey), "AlternateId", "88").Single().Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries<ShadowAlternateStringKey, string>("AlternateId", "88").Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowAlternateStringKey), "AlternateId", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowAlternateStringKey, string>("AlternateId", "99"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_string_foreign_key(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new StringKey { Id = "88" });
            }

            var entities = new[]
            {
                CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "87"),
                CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "88"),
                CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "88"),
                CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "89"),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowForeignStringKey), "StringKeyId", "88")
                .Select(e => e.Entity)
                .Cast<ShadowForeignStringKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowForeignStringKey, string>("StringKeyId", "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowForeignStringKey), "StringKeyId", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowForeignStringKey, string>("StringKeyId", "99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_string_non_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowStringNonKey>(context, "String", "87"),
                CreateShadowEntity<ShadowStringNonKey>(context, "String", "88"),
                CreateShadowEntity<ShadowStringNonKey>(context, "String", "88"),
                CreateShadowEntity<ShadowStringNonKey>(context, "String", "89"),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowStringNonKey), "String", "88")
                .Select(e => e.Entity)
                .Cast<ShadowStringNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowStringNonKey, string>("String", "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowStringNonKey), "String", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowStringNonKey, string>("String", "99"));
        }

        [ConditionalFact]
        public virtual void Find_string_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new StringKey { Id = "87" }).Entity,
                context.Attach(new StringKey { Id = "88" }).Entity,
                context.Attach(new StringKey { Id = "89" }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(StringKey).FullName!, "88")!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<StringKey, string>(typeof(StringKey).FullName!, "88")!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(StringKey).FullName!, "99"));
            Assert.Null(context.ChangeTracker.FindEntry<StringKey, string>(typeof(StringKey).FullName!, "99"));
        }

        [ConditionalFact]
        public virtual void Find_string_alternate_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "87" }).Entity,
                context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "88" }).Entity,
                context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "89" }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(typeof(AlternateStringKey).FullName!, nameof(AlternateStringKey.AlternateId), "88")
                    .Single().Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateStringKey, string>(
                    typeof(AlternateStringKey).FullName!, nameof(AlternateStringKey.AlternateId), "88").Single().Entity);
            Assert.Empty(
                context.ChangeTracker.GetEntries(typeof(AlternateStringKey).FullName!, nameof(AlternateStringKey.AlternateId), "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateStringKey, string>(
                    typeof(AlternateStringKey).FullName!, nameof(AlternateStringKey.AlternateId), "99"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_string_foreign_key_by_name(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(new StringKey { Id = "88" });
            }

            var entities = new[]
            {
                context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "87" }).Entity,
                context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
                context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
                context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "89" }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ForeignStringKey).FullName!, nameof(ForeignStringKey.StringKeyId), "88")
                .Select(e => e.Entity)
                .Cast<ForeignStringKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ForeignStringKey, string>(typeof(ForeignStringKey).FullName!, nameof(ForeignStringKey.StringKeyId), "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ForeignStringKey).FullName!, nameof(ForeignStringKey.StringKeyId), "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ForeignStringKey, string>(
                    typeof(ForeignStringKey).FullName!, nameof(ForeignStringKey.StringKeyId), "99"));
        }

        [ConditionalFact]
        public virtual void Find_string_non_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "87" }).Entity,
                context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
                context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
                context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "89" }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(StringNonKey).FullName!, nameof(StringNonKey.String), "88")
                .Select(e => e.Entity)
                .Cast<StringNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<StringNonKey, string>(typeof(StringNonKey).FullName!, nameof(StringNonKey.String), "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(StringNonKey).FullName!, nameof(StringNonKey.String), "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<StringNonKey, string>(typeof(StringNonKey).FullName!, nameof(StringNonKey.String), "99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_string_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowStringKey>(context, "Id", "87"),
                CreateShadowEntity<ShadowStringKey>(context, "Id", "88"),
                CreateShadowEntity<ShadowStringKey>(context, "Id", "89"),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(ShadowStringKey).FullName!, "88")!.Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.FindEntry<ShadowStringKey, string>(typeof(ShadowStringKey).FullName!, "88")!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(ShadowStringKey).FullName!, "99"));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowStringKey, string>(typeof(ShadowStringKey).FullName!, "99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_string_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "87"),
                CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "88"),
                CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "89"),
            };

            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries(
                    typeof(ShadowAlternateStringKey).FullName!, "AlternateId", "88").Single().Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries<ShadowAlternateStringKey, string>(
                    typeof(ShadowAlternateStringKey).FullName!, "AlternateId", "88").Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowAlternateStringKey).FullName!, "AlternateId", "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowAlternateStringKey, string>(
                    typeof(ShadowAlternateStringKey).FullName!, "AlternateId", "99"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_string_foreign_key_by_name(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                CreateShadowEntity<ShadowStringKey>(context, "Id", "88");
            }

            var entities = new[]
            {
                CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "87"),
                CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "88"),
                CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "88"),
                CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "89"),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowForeignStringKey).FullName!, "StringKeyId", "88")
                .Select(e => e.Entity)
                .Cast<ShadowForeignStringKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowForeignStringKey, string>(typeof(ShadowForeignStringKey).FullName!, "StringKeyId", "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowForeignStringKey).FullName!, "StringKeyId", "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowForeignStringKey, string>(
                    typeof(ShadowForeignStringKey).FullName!, "StringKeyId", "99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_string_non_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowStringNonKey>(context, "String", "87"),
                CreateShadowEntity<ShadowStringNonKey>(context, "String", "88"),
                CreateShadowEntity<ShadowStringNonKey>(context, "String", "88"),
                CreateShadowEntity<ShadowStringNonKey>(context, "String", "89"),
            };

            var found = context.ChangeTracker
                .GetEntries(typeof(ShadowStringNonKey).FullName!, "String", "88")
                .Select(e => e.Entity)
                .Cast<ShadowStringNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            var foundGeneric = context.ChangeTracker
                .GetEntries<ShadowStringNonKey, string>(typeof(ShadowStringNonKey).FullName!, "String", "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGeneric.Count);
            Assert.Contains(entities[1], foundGeneric);
            Assert.Contains(entities[2], foundGeneric);

            Assert.Empty(context.ChangeTracker.GetEntries(typeof(ShadowStringNonKey).FullName!, "String", "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowStringNonKey, string>(typeof(ShadowStringNonKey).FullName!, "String", "99"));
        }

        [ConditionalFact]
        public virtual void Find_string_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<StringKey>("StringKeyA").Attach(new StringKey { Id = "87" }).Entity,
                context.Set<StringKey>("StringKeyA").Attach(new StringKey { Id = "88" }).Entity,
                context.Set<StringKey>("StringKeyA").Attach(new StringKey { Id = "89" }).Entity,
                context.Set<StringKey>("StringKeyB").Attach(new StringKey { Id = "87" }).Entity,
                context.Set<StringKey>("StringKeyB").Attach(new StringKey { Id = "88" }).Entity,
                context.Set<StringKey>("StringKeyB").Attach(new StringKey { Id = "89" }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry("StringKeyA", "88")!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry("StringKeyB", "88")!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<StringKey, string>("StringKeyA", "88")!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry<StringKey, string>("StringKeyB", "88")!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry("StringKeyA", "99"));
            Assert.Null(context.ChangeTracker.FindEntry("StringKeyB", "99"));
            Assert.Null(context.ChangeTracker.FindEntry<StringKey, string>("StringKeyA", "99"));
            Assert.Null(context.ChangeTracker.FindEntry<StringKey, string>("StringKeyB", "99"));
        }

        [ConditionalFact]
        public virtual void Find_string_alternate_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<AlternateStringKey>("AlternateStringKeyA")
                    .Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "87" }).Entity,
                context.Set<AlternateStringKey>("AlternateStringKeyA")
                    .Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "88" }).Entity,
                context.Set<AlternateStringKey>("AlternateStringKeyA")
                    .Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "89" }).Entity,
                context.Set<AlternateStringKey>("AlternateStringKeyB")
                    .Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "87" }).Entity,
                context.Set<AlternateStringKey>("AlternateStringKeyB")
                    .Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "88" }).Entity,
                context.Set<AlternateStringKey>("AlternateStringKeyB")
                    .Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "89" }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries("AlternateStringKeyA", nameof(AlternateStringKey.AlternateId), "88").Single().Entity);
            Assert.Same(
                entities[4],
                context.ChangeTracker.GetEntries("AlternateStringKeyB", nameof(AlternateStringKey.AlternateId), "88").Single().Entity);
            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries<AlternateStringKey, string>(
                    "AlternateStringKeyA", nameof(AlternateStringKey.AlternateId), "88").Single().Entity);
            Assert.Same(
                entities[4],
                context.ChangeTracker.GetEntries<AlternateStringKey, string>(
                    "AlternateStringKeyB", nameof(AlternateStringKey.AlternateId), "88").Single().Entity);
            Assert.Empty(context.ChangeTracker.GetEntries("AlternateStringKeyA", nameof(AlternateStringKey.AlternateId), "99"));
            Assert.Empty(context.ChangeTracker.GetEntries("AlternateStringKeyB", nameof(AlternateStringKey.AlternateId), "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateStringKey, string>(
                    "AlternateStringKeyA", nameof(AlternateStringKey.AlternateId), "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<AlternateStringKey, string>(
                    "AlternateStringKeyB", nameof(AlternateStringKey.AlternateId), "99"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_string_foreign_key_shared(bool trackPrincipal)
        {
            using var context = new FindContextShared();

            if (trackPrincipal)
            {
                context.Set<StringKey>("StringKeyA").Attach(new StringKey { Id = "88" });
                context.Set<StringKey>("StringKeyB").Attach(new StringKey { Id = "88" });
            }

            var entities = new[]
            {
                context.Set<ForeignStringKey>("ForeignStringKeyA")
                    .Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "87" }).Entity,
                context.Set<ForeignStringKey>("ForeignStringKeyA")
                    .Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
                context.Set<ForeignStringKey>("ForeignStringKeyA")
                    .Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
                context.Set<ForeignStringKey>("ForeignStringKeyA")
                    .Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "89" }).Entity,
                context.Set<ForeignStringKey>("ForeignStringKeyB")
                    .Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "87" }).Entity,
                context.Set<ForeignStringKey>("ForeignStringKeyB")
                    .Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
                context.Set<ForeignStringKey>("ForeignStringKeyB")
                    .Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
                context.Set<ForeignStringKey>("ForeignStringKeyB")
                    .Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "89" }).Entity,
            };

            var foundA = context.ChangeTracker
                .GetEntries("ForeignStringKeyA", nameof(ForeignStringKey.StringKeyId), "88")
                .Select(e => e.Entity)
                .Cast<ForeignStringKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ForeignStringKey, string>("ForeignStringKeyA", nameof(ForeignStringKey.StringKeyId), "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ForeignStringKeyB", nameof(ForeignStringKey.StringKeyId), "88")
                .Select(e => e.Entity)
                .Cast<ForeignStringKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ForeignStringKey, string>("ForeignStringKeyB", nameof(ForeignStringKey.StringKeyId), "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ForeignStringKeyA", nameof(ForeignStringKey.StringKeyId), "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ForeignStringKey, string>(
                    "ForeignStringKeyA", nameof(ForeignStringKey.StringKeyId), "99"));
            Assert.Empty(context.ChangeTracker.GetEntries("ForeignStringKeyB", nameof(ForeignStringKey.StringKeyId), "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ForeignStringKey, string>(
                    "ForeignStringKeyB", nameof(ForeignStringKey.StringKeyId), "99"));
        }

        [ConditionalFact]
        public virtual void Find_string_non_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<StringNonKey>("StringNonKeyA").Attach(new StringNonKey { Id = Guid.NewGuid(), String = "87" }).Entity,
                context.Set<StringNonKey>("StringNonKeyA").Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
                context.Set<StringNonKey>("StringNonKeyA").Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
                context.Set<StringNonKey>("StringNonKeyA").Attach(new StringNonKey { Id = Guid.NewGuid(), String = "89" }).Entity,
                context.Set<StringNonKey>("StringNonKeyB").Attach(new StringNonKey { Id = Guid.NewGuid(), String = "87" }).Entity,
                context.Set<StringNonKey>("StringNonKeyB").Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
                context.Set<StringNonKey>("StringNonKeyB").Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
                context.Set<StringNonKey>("StringNonKeyB").Attach(new StringNonKey { Id = Guid.NewGuid(), String = "89" }).Entity,
            };

            var foundA = context.ChangeTracker
                .GetEntries("StringNonKeyA", nameof(StringNonKey.String), "88")
                .Select(e => e.Entity)
                .Cast<StringNonKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<StringNonKey, string>("StringNonKeyA", nameof(StringNonKey.String), "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("StringNonKeyB", nameof(StringNonKey.String), "88")
                .Select(e => e.Entity)
                .Cast<StringNonKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<StringNonKey, string>("StringNonKeyB", nameof(StringNonKey.String), "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("StringNonKeyA", nameof(StringNonKey.String), "99"));
            Assert.Empty(context.ChangeTracker.GetEntries("StringNonKeyB", nameof(StringNonKey.String), "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<StringNonKey, string>("StringNonKeyA", nameof(StringNonKey.String), "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<StringNonKey, string>("StringNonKeyB", nameof(StringNonKey.String), "99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_string_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowStringKey>(context, "ShadowStringKeyA", "Id", "87"),
                CreateShadowEntityShared<ShadowStringKey>(context, "ShadowStringKeyA", "Id", "88"),
                CreateShadowEntityShared<ShadowStringKey>(context, "ShadowStringKeyA", "Id", "89"),
                CreateShadowEntityShared<ShadowStringKey>(context, "ShadowStringKeyB", "Id", "87"),
                CreateShadowEntityShared<ShadowStringKey>(context, "ShadowStringKeyB", "Id", "88"),
                CreateShadowEntityShared<ShadowStringKey>(context, "ShadowStringKeyB", "Id", "89"),
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry("ShadowStringKeyA", "88")!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry("ShadowStringKeyB", "88")!.Entity);
            Assert.Same(entities[1], context.ChangeTracker.FindEntry<ShadowStringKey, string>("ShadowStringKeyA", "88")!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry<ShadowStringKey, string>("ShadowStringKeyB", "88")!.Entity);

            Assert.Null(context.ChangeTracker.FindEntry("ShadowStringKeyA", "99"));
            Assert.Null(context.ChangeTracker.FindEntry("ShadowStringKeyB", "99"));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowStringKey, string>("ShadowStringKeyA", "99"));
            Assert.Null(context.ChangeTracker.FindEntry<ShadowStringKey, string>("ShadowStringKeyB", "99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_string_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowAlternateStringKey>(context, "ShadowAlternateStringKeyA", "AlternateId", "87"),
                CreateShadowEntityShared<ShadowAlternateStringKey>(context, "ShadowAlternateStringKeyA", "AlternateId", "88"),
                CreateShadowEntityShared<ShadowAlternateStringKey>(context, "ShadowAlternateStringKeyA", "AlternateId", "89"),
                CreateShadowEntityShared<ShadowAlternateStringKey>(context, "ShadowAlternateStringKeyB", "AlternateId", "87"),
                CreateShadowEntityShared<ShadowAlternateStringKey>(context, "ShadowAlternateStringKeyB", "AlternateId", "88"),
                CreateShadowEntityShared<ShadowAlternateStringKey>(context, "ShadowAlternateStringKeyB", "AlternateId", "89"),
            };

            Assert.Same(entities[1], context.ChangeTracker.GetEntries("ShadowAlternateStringKeyA", "AlternateId", "88").Single().Entity);
            Assert.Same(entities[4], context.ChangeTracker.GetEntries("ShadowAlternateStringKeyB", "AlternateId", "88").Single().Entity);
            Assert.Same(
                entities[1], context.ChangeTracker.GetEntries<ShadowAlternateStringKey, string>(
                    "ShadowAlternateStringKeyA", "AlternateId", "88").Single().Entity);
            Assert.Same(
                entities[4], context.ChangeTracker.GetEntries<ShadowAlternateStringKey, string>(
                    "ShadowAlternateStringKeyB", "AlternateId", "88").Single().Entity);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowAlternateStringKeyA", "AlternateId", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowAlternateStringKeyB", "AlternateId", "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowAlternateStringKey, string>("ShadowAlternateStringKeyA", "AlternateId", "99"));
            Assert.Empty(
                context.ChangeTracker.GetEntries<ShadowAlternateStringKey, string>("ShadowAlternateStringKeyB", "AlternateId", "99"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_string_foreign_key_shared(bool trackPrincipal)
        {
            using var context = new FindContextShared();

            if (trackPrincipal)
            {
                CreateShadowEntityShared<ShadowStringKey>(context, "ShadowStringKeyA", "Id", "88");
                CreateShadowEntityShared<ShadowStringKey>(context, "ShadowStringKeyB", "Id", "88");
            }

            var entities = new[]
            {
                CreateShadowEntityShared<ShadowForeignStringKey>(context, "ShadowForeignStringKeyA", "StringKeyId", "87"),
                CreateShadowEntityShared<ShadowForeignStringKey>(context, "ShadowForeignStringKeyA", "StringKeyId", "88"),
                CreateShadowEntityShared<ShadowForeignStringKey>(context, "ShadowForeignStringKeyA", "StringKeyId", "88"),
                CreateShadowEntityShared<ShadowForeignStringKey>(context, "ShadowForeignStringKeyA", "StringKeyId", "89"),
                CreateShadowEntityShared<ShadowForeignStringKey>(context, "ShadowForeignStringKeyB", "StringKeyId", "87"),
                CreateShadowEntityShared<ShadowForeignStringKey>(context, "ShadowForeignStringKeyB", "StringKeyId", "88"),
                CreateShadowEntityShared<ShadowForeignStringKey>(context, "ShadowForeignStringKeyB", "StringKeyId", "88"),
                CreateShadowEntityShared<ShadowForeignStringKey>(context, "ShadowForeignStringKeyB", "StringKeyId", "89"),
            };

            var foundA = context.ChangeTracker
                .GetEntries("ShadowForeignStringKeyA", "StringKeyId", "88")
                .Select(e => e.Entity)
                .Cast<ShadowForeignStringKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ShadowForeignStringKey, string>("ShadowForeignStringKeyA", "StringKeyId", "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ShadowForeignStringKeyB", "StringKeyId", "88")
                .Select(e => e.Entity)
                .Cast<ShadowForeignStringKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ShadowForeignStringKey, string>("ShadowForeignStringKeyB", "StringKeyId", "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowForeignStringKeyA", "StringKeyId", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowForeignStringKeyB", "StringKeyId", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowForeignStringKey, string>("ShadowForeignStringKeyA", "StringKeyId", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowForeignStringKey, string>("ShadowForeignStringKeyB", "StringKeyId", "99"));
        }

        [ConditionalFact]
        public virtual void Find_shadow_string_non_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowStringNonKey>(context, "ShadowStringNonKeyA", "String", "87"),
                CreateShadowEntityShared<ShadowStringNonKey>(context, "ShadowStringNonKeyA", "String", "88"),
                CreateShadowEntityShared<ShadowStringNonKey>(context, "ShadowStringNonKeyA", "String", "88"),
                CreateShadowEntityShared<ShadowStringNonKey>(context, "ShadowStringNonKeyA", "String", "89"),
                CreateShadowEntityShared<ShadowStringNonKey>(context, "ShadowStringNonKeyB", "String", "87"),
                CreateShadowEntityShared<ShadowStringNonKey>(context, "ShadowStringNonKeyB", "String", "88"),
                CreateShadowEntityShared<ShadowStringNonKey>(context, "ShadowStringNonKeyB", "String", "88"),
                CreateShadowEntityShared<ShadowStringNonKey>(context, "ShadowStringNonKeyB", "String", "89"),
            };

            var foundA = context.ChangeTracker
                .GetEntries("ShadowStringNonKeyA", "String", "88")
                .Select(e => e.Entity)
                .Cast<ShadowStringNonKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundGenericA = context.ChangeTracker
                .GetEntries<ShadowStringNonKey, string>("ShadowStringNonKeyA", "String", "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericA.Count);
            Assert.Contains(entities[1], foundGenericA);
            Assert.Contains(entities[2], foundGenericA);

            var foundB = context.ChangeTracker
                .GetEntries("ShadowStringNonKeyB", "String", "88")
                .Select(e => e.Entity)
                .Cast<ShadowStringNonKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[5], foundB);
            Assert.Contains(entities[6], foundB);

            var foundGenericB = context.ChangeTracker
                .GetEntries<ShadowStringNonKey, string>("ShadowStringNonKeyB", "String", "88")
                .Select(e => e.Entity)
                .ToList();

            Assert.Equal(2, foundGenericB.Count);
            Assert.Contains(entities[5], foundGenericB);
            Assert.Contains(entities[6], foundGenericB);

            Assert.Empty(context.ChangeTracker.GetEntries("ShadowStringNonKeyA", "String", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries("ShadowStringNonKeyB", "String", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowStringNonKey, string>("ShadowStringNonKeyA", "String", "99"));
            Assert.Empty(context.ChangeTracker.GetEntries<ShadowStringNonKey, string>("ShadowStringNonKeyB", "String", "99"));
        }

        [ConditionalFact]
        public virtual void Find_composite_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "87",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "89",
                        Foo = "foo"
                    }).Entity,
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(CompositeKey), new object[] { 1, "88", "foo" })!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(CompositeKey), new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_composite_alternate_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "87",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "88",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "89",
                        AlternateFoo = "foo"
                    }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(
                    typeof(AlternateCompositeKey),
                    new[]
                    {
                        nameof(AlternateCompositeKey.AlternateId1),
                        nameof(AlternateCompositeKey.AlternateId2),
                        nameof(AlternateCompositeKey.AlternateFoo)
                    },
                    new object[] { 1, "88", "foo" }).Single().Entity);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(AlternateCompositeKey),
                    new[]
                    {
                        nameof(AlternateCompositeKey.AlternateId1),
                        nameof(AlternateCompositeKey.AlternateId2),
                        nameof(AlternateCompositeKey.AlternateFoo)
                    },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_composite_foreign_key(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    });
            }

            var entities = new[]
            {
                context.Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "87",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "88",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "88",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "89",
                        CompositeKeyFoo = "foo"
                    }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(
                    typeof(ForeignCompositeKey),
                    new[]
                    {
                        nameof(ForeignCompositeKey.CompositeKeyId1),
                        nameof(ForeignCompositeKey.CompositeKeyId2),
                        nameof(ForeignCompositeKey.CompositeKeyFoo)
                    },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ForeignCompositeKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ForeignCompositeKey),
                    new[]
                    {
                        nameof(ForeignCompositeKey.CompositeKeyId1),
                        nameof(ForeignCompositeKey.CompositeKeyId2),
                        nameof(ForeignCompositeKey.CompositeKeyFoo)
                    },
                    new object[] { 1, "88", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_composite_non_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "87",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "88",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "88",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "89",
                        Foo = "foo"
                    }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(
                    typeof(ForeignCompositeKey),
                    new[]
                    {
                        nameof(CompositeNonKey.Int),
                        nameof(CompositeNonKey.String),
                        nameof(CompositeNonKey.Foo)
                    },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<CompositeNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ForeignCompositeKey),
                    new[]
                    {
                        nameof(CompositeNonKey.Int),
                        nameof(CompositeNonKey.String),
                        nameof(CompositeNonKey.Foo)
                    },
                    new object[] { 1, "88", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_composite_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowCompositeKey>(
                    context,
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntity<ShadowCompositeKey>(
                    context,
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowCompositeKey>(
                    context,
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "89", "foo" })
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry(typeof(ShadowCompositeKey), new object[] { 1, "88", "foo" })!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(ShadowCompositeKey), new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_composite_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowAlternateCompositeKey>(
                    context,
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntity<ShadowAlternateCompositeKey>(
                    context,
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowAlternateCompositeKey>(
                    context,
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "89", "foo" })
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(
                    typeof(ShadowAlternateCompositeKey),
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "88", "foo" }).Single().Entity);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ShadowAlternateCompositeKey),
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_composite_foreign_key(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    });
            }

            var entities = new[]
            {
                CreateShadowEntity<ShadowForeignCompositeKey>(
                    context,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntity<ShadowForeignCompositeKey>(
                    context,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowForeignCompositeKey>(
                    context,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowForeignCompositeKey>(
                    context,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "89", "foo" })
            };

            var found = context.ChangeTracker
                .GetEntries(
                    typeof(ShadowForeignCompositeKey),
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ShadowForeignCompositeKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ShadowForeignCompositeKey),
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_composite_non_key()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowCompositeNonKey>(
                    context,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntity<ShadowCompositeNonKey>(
                    context,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowCompositeNonKey>(
                    context,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowCompositeNonKey>(
                    context,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "89", "foo" })
            };

            var found = context.ChangeTracker
                .GetEntries(
                    typeof(ShadowCompositeNonKey),
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ShadowCompositeNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ShadowCompositeNonKey),
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_composite_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "87",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "89",
                        Foo = "foo"
                    }).Entity,
            };

            Assert.Same(
                entities[1], context.ChangeTracker.FindEntry(typeof(CompositeKey).FullName!, new object[] { 1, "88", "foo" })!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(CompositeKey).FullName!, new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_composite_alternate_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "87",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "88",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "89",
                        AlternateFoo = "foo"
                    }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(
                    typeof(AlternateCompositeKey).FullName!,
                    new[]
                    {
                        nameof(AlternateCompositeKey.AlternateId1),
                        nameof(AlternateCompositeKey.AlternateId2),
                        nameof(AlternateCompositeKey.AlternateFoo)
                    },
                    new object[] { 1, "88", "foo" }).Single().Entity);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(AlternateCompositeKey).FullName!,
                    new[]
                    {
                        nameof(AlternateCompositeKey.AlternateId1),
                        nameof(AlternateCompositeKey.AlternateId2),
                        nameof(AlternateCompositeKey.AlternateFoo)
                    },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_composite_foreign_key_by_name(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                context.Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    });
            }

            var entities = new[]
            {
                context.Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "87",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "88",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "88",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "89",
                        CompositeKeyFoo = "foo"
                    }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(
                    typeof(ForeignCompositeKey).FullName!,
                    new[]
                    {
                        nameof(ForeignCompositeKey.CompositeKeyId1),
                        nameof(ForeignCompositeKey.CompositeKeyId2),
                        nameof(ForeignCompositeKey.CompositeKeyFoo)
                    },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ForeignCompositeKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ForeignCompositeKey).FullName!,
                    new[]
                    {
                        nameof(ForeignCompositeKey.CompositeKeyId1),
                        nameof(ForeignCompositeKey.CompositeKeyId2),
                        nameof(ForeignCompositeKey.CompositeKeyFoo)
                    },
                    new object[] { 1, "88", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_composite_non_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                context.Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "87",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "88",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "88",
                        Foo = "foo"
                    }).Entity,
                context.Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "89",
                        Foo = "foo"
                    }).Entity,
            };

            var found = context.ChangeTracker
                .GetEntries(
                    typeof(ForeignCompositeKey).FullName!,
                    new[]
                    {
                        nameof(CompositeNonKey.Int),
                        nameof(CompositeNonKey.String),
                        nameof(CompositeNonKey.Foo)
                    },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<CompositeNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ForeignCompositeKey).FullName!,
                    new[]
                    {
                        nameof(CompositeNonKey.Int),
                        nameof(CompositeNonKey.String),
                        nameof(CompositeNonKey.Foo)
                    },
                    new object[] { 1, "88", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_composite_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowCompositeKey>(
                    context,
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntity<ShadowCompositeKey>(
                    context,
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowCompositeKey>(
                    context,
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "89", "foo" })
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.FindEntry(typeof(ShadowCompositeKey).FullName!, new object[] { 1, "88", "foo" })!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry(typeof(ShadowCompositeKey).FullName!, new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_composite_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowAlternateCompositeKey>(
                    context,
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntity<ShadowAlternateCompositeKey>(
                    context,
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowAlternateCompositeKey>(
                    context,
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "89", "foo" })
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(
                    typeof(ShadowAlternateCompositeKey).FullName!,
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "88", "foo" }).Single().Entity);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ShadowAlternateCompositeKey).FullName!,
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_composite_foreign_key_by_name(bool trackPrincipal)
        {
            using var context = new FindContext();

            if (trackPrincipal)
            {
                CreateShadowEntity<ShadowCompositeKey>(
                    context,
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "88", "foo" });
            }

            var entities = new[]
            {
                CreateShadowEntity<ShadowForeignCompositeKey>(
                    context,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntity<ShadowForeignCompositeKey>(
                    context,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowForeignCompositeKey>(
                    context,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowForeignCompositeKey>(
                    context,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "89", "foo" })
            };

            var found = context.ChangeTracker
                .GetEntries(
                    typeof(ShadowForeignCompositeKey).FullName!,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ShadowForeignCompositeKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ShadowForeignCompositeKey).FullName!,
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_composite_non_key_by_name()
        {
            using var context = new FindContext();
            var entities = new[]
            {
                CreateShadowEntity<ShadowCompositeNonKey>(
                    context,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntity<ShadowCompositeNonKey>(
                    context,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowCompositeNonKey>(
                    context,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntity<ShadowCompositeNonKey>(
                    context,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "89", "foo" })
            };

            var found = context.ChangeTracker
                .GetEntries(
                    typeof(ShadowCompositeNonKey).FullName!,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ShadowCompositeNonKey>()
                .ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains(entities[1], found);
            Assert.Contains(entities[2], found);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    typeof(ShadowCompositeNonKey).FullName!,
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_composite_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<CompositeKey>("CompositeKeyA").Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "87",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeKey>("CompositeKeyA").Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeKey>("CompositeKeyA").Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "89",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeKey>("CompositeKeyB").Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "87",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeKey>("CompositeKeyB").Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeKey>("CompositeKeyB").Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "89",
                        Foo = "foo"
                    }).Entity
            };

            Assert.Same(entities[1], context.ChangeTracker.FindEntry("CompositeKeyA", new object[] { 1, "88", "foo" })!.Entity);
            Assert.Same(entities[4], context.ChangeTracker.FindEntry("CompositeKeyB", new object[] { 1, "88", "foo" })!.Entity);
            Assert.Null(context.ChangeTracker.FindEntry("CompositeKeyA", new object[] { 1, "99", "foo" }));
            Assert.Null(context.ChangeTracker.FindEntry("CompositeKeyB", new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_composite_alternate_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<AlternateCompositeKey>("AlternateCompositeKeyA").Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "87",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Set<AlternateCompositeKey>("AlternateCompositeKeyA").Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "88",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Set<AlternateCompositeKey>("AlternateCompositeKeyA").Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "89",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Set<AlternateCompositeKey>("AlternateCompositeKeyB").Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "87",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Set<AlternateCompositeKey>("AlternateCompositeKeyB").Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "88",
                        AlternateFoo = "foo"
                    }).Entity,
                context.Set<AlternateCompositeKey>("AlternateCompositeKeyB").Attach(
                    new AlternateCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        AlternateId1 = 1,
                        AlternateId2 = "89",
                        AlternateFoo = "foo"
                    }).Entity,
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(
                    "AlternateCompositeKeyA",
                    new[]
                    {
                        nameof(AlternateCompositeKey.AlternateId1),
                        nameof(AlternateCompositeKey.AlternateId2),
                        nameof(AlternateCompositeKey.AlternateFoo)
                    },
                    new object[] { 1, "88", "foo" }).Single().Entity);

            Assert.Same(
                entities[4],
                context.ChangeTracker.GetEntries(
                    "AlternateCompositeKeyB",
                    new[]
                    {
                        nameof(AlternateCompositeKey.AlternateId1),
                        nameof(AlternateCompositeKey.AlternateId2),
                        nameof(AlternateCompositeKey.AlternateFoo)
                    },
                    new object[] { 1, "88", "foo" }).Single().Entity);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "AlternateCompositeKeyA",
                    new[]
                    {
                        nameof(AlternateCompositeKey.AlternateId1),
                        nameof(AlternateCompositeKey.AlternateId2),
                        nameof(AlternateCompositeKey.AlternateFoo)
                    },
                    new object[] { 1, "99", "foo" }));

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "AlternateCompositeKeyB",
                    new[]
                    {
                        nameof(AlternateCompositeKey.AlternateId1),
                        nameof(AlternateCompositeKey.AlternateId2),
                        nameof(AlternateCompositeKey.AlternateFoo)
                    },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_composite_foreign_key_shared(bool trackPrincipal)
        {
            using var context = new FindContextShared();

            if (trackPrincipal)
            {
                context.Set<CompositeKey>("CompositeKeyA").Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    });
                context.Set<CompositeKey>("CompositeKeyB").Attach(
                    new CompositeKey
                    {
                        Id1 = 1,
                        Id2 = "88",
                        Foo = "foo"
                    });
            }

            var entities = new[]
            {
                context.Set<ForeignCompositeKey>("ForeignCompositeKeyA").Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "87",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Set<ForeignCompositeKey>("ForeignCompositeKeyA").Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "88",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Set<ForeignCompositeKey>("ForeignCompositeKeyA").Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "88",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Set<ForeignCompositeKey>("ForeignCompositeKeyA").Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "89",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Set<ForeignCompositeKey>("ForeignCompositeKeyB").Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "87",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Set<ForeignCompositeKey>("ForeignCompositeKeyB").Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "88",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Set<ForeignCompositeKey>("ForeignCompositeKeyB").Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "88",
                        CompositeKeyFoo = "foo"
                    }).Entity,
                context.Set<ForeignCompositeKey>("ForeignCompositeKeyB").Attach(
                    new ForeignCompositeKey
                    {
                        Id = Guid.NewGuid(),
                        CompositeKeyId1 = 1,
                        CompositeKeyId2 = "89",
                        CompositeKeyFoo = "foo"
                    }).Entity,
            };

            var foundA = context.ChangeTracker
                .GetEntries(
                    "ForeignCompositeKeyA",
                    new[]
                    {
                        nameof(ForeignCompositeKey.CompositeKeyId1),
                        nameof(ForeignCompositeKey.CompositeKeyId2),
                        nameof(ForeignCompositeKey.CompositeKeyFoo)
                    },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ForeignCompositeKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundB = context.ChangeTracker
                .GetEntries(
                    "ForeignCompositeKeyB",
                    new[]
                    {
                        nameof(ForeignCompositeKey.CompositeKeyId1),
                        nameof(ForeignCompositeKey.CompositeKeyId2),
                        nameof(ForeignCompositeKey.CompositeKeyFoo)
                    },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ForeignCompositeKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[4], foundB);
            Assert.Contains(entities[7], foundB);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "ForeignCompositeKeyA",
                    new[]
                    {
                        nameof(ForeignCompositeKey.CompositeKeyId1),
                        nameof(ForeignCompositeKey.CompositeKeyId2),
                        nameof(ForeignCompositeKey.CompositeKeyFoo)
                    },
                    new object[] { 1, "88", "foo" }));

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "ForeignCompositeKeyB",
                    new[]
                    {
                        nameof(ForeignCompositeKey.CompositeKeyId1),
                        nameof(ForeignCompositeKey.CompositeKeyId2),
                        nameof(ForeignCompositeKey.CompositeKeyFoo)
                    },
                    new object[] { 1, "88", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_composite_non_key_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                context.Set<CompositeNonKey>("CompositeNonKeyA").Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "87",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeNonKey>("CompositeNonKeyA").Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "88",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeNonKey>("CompositeNonKeyA").Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "88",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeNonKey>("CompositeNonKeyA").Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "89",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeNonKey>("CompositeNonKeyB").Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "87",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeNonKey>("CompositeNonKeyB").Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "88",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeNonKey>("CompositeNonKeyB").Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "88",
                        Foo = "foo"
                    }).Entity,
                context.Set<CompositeNonKey>("CompositeNonKeyB").Attach(
                    new CompositeNonKey
                    {
                        Id = Guid.NewGuid(),
                        Int = 1,
                        String = "89",
                        Foo = "foo"
                    }).Entity
            };

            var foundA = context.ChangeTracker
                .GetEntries(
                    "CompositeNonKeyA",
                    new[]
                    {
                        nameof(CompositeNonKey.Int),
                        nameof(CompositeNonKey.String),
                        nameof(CompositeNonKey.Foo)
                    },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<CompositeNonKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundB = context.ChangeTracker
                .GetEntries(
                    "CompositeNonKeyB",
                    new[]
                    {
                        nameof(CompositeNonKey.Int),
                        nameof(CompositeNonKey.String),
                        nameof(CompositeNonKey.Foo)
                    },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<CompositeNonKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[4], foundB);
            Assert.Contains(entities[7], foundB);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "CompositeNonKeyA",
                    new[]
                    {
                        nameof(CompositeNonKey.Int),
                        nameof(CompositeNonKey.String),
                        nameof(CompositeNonKey.Foo)
                    },
                    new object[] { 1, "88", "foo" }));

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "CompositeNonKeyB",
                    new[]
                    {
                        nameof(CompositeNonKey.Int),
                        nameof(CompositeNonKey.String),
                        nameof(CompositeNonKey.Foo)
                    },
                    new object[] { 1, "88", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_composite_key_by_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowCompositeKey>(
                    context,
                    "ShadowCompositeKeyA",
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntityShared<ShadowCompositeKey>(
                    context,
                    "ShadowCompositeKeyA",
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowCompositeKey>(
                    context,
                    "ShadowCompositeKeyA",
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "89", "foo" }),
                CreateShadowEntityShared<ShadowCompositeKey>(
                    context,
                    "ShadowCompositeKeyB",
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntityShared<ShadowCompositeKey>(
                    context,
                    "ShadowCompositeKeyB",
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowCompositeKey>(
                    context,
                    "ShadowCompositeKeyB",
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "89", "foo" })
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.FindEntry("ShadowCompositeKeyA", new object[] { 1, "88", "foo" })!.Entity);

            Assert.Same(
                entities[4],
                context.ChangeTracker.FindEntry("ShadowCompositeKeyB", new object[] { 1, "88", "foo" })!.Entity);

            Assert.Null(context.ChangeTracker.FindEntry("ShadowCompositeKeyA", new object[] { 1, "99", "foo" }));
            Assert.Null(context.ChangeTracker.FindEntry("ShadowCompositeKeyB", new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_alternate_composite_key_by_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowAlternateCompositeKey>(
                    context,
                    "ShadowAlternateCompositeKeyA",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntityShared<ShadowAlternateCompositeKey>(
                    context,
                    "ShadowAlternateCompositeKeyA",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowAlternateCompositeKey>(
                    context,
                    "ShadowAlternateCompositeKeyA",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "89", "foo" }),
                CreateShadowEntityShared<ShadowAlternateCompositeKey>(
                    context,
                    "ShadowAlternateCompositeKeyB",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntityShared<ShadowAlternateCompositeKey>(
                    context,
                    "ShadowAlternateCompositeKeyB",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowAlternateCompositeKey>(
                    context,
                    "ShadowAlternateCompositeKeyB",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "89", "foo" })
            };

            Assert.Same(
                entities[1],
                context.ChangeTracker.GetEntries(
                    "ShadowAlternateCompositeKeyA",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "88", "foo" }).Single().Entity);

            Assert.Same(
                entities[4],
                context.ChangeTracker.GetEntries(
                    "ShadowAlternateCompositeKeyB",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "88", "foo" }).Single().Entity);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "ShadowAlternateCompositeKeyA",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "99", "foo" }));

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "ShadowAlternateCompositeKeyB",
                    new[] { "AlternateId1", "AlternateId2", "AlternateFoo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Find_shadow_composite_foreign_key_by_shared(bool trackPrincipal)
        {
            using var context = new FindContextShared();

            if (trackPrincipal)
            {
                CreateShadowEntityShared<ShadowCompositeKey>(
                    context,
                    "ShadowCompositeKeyA",
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "88", "foo" });
                CreateShadowEntityShared<ShadowCompositeKey>(
                    context,
                    "ShadowCompositeKeyB",
                    new[] { "Id1", "Id2", "Foo" },
                    new object[] { 1, "88", "foo" });
            }

            var entities = new[]
            {
                CreateShadowEntityShared<ShadowForeignCompositeKey>(
                    context,
                    "ShadowCompositeKeyA",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntityShared<ShadowForeignCompositeKey>(
                    context,
                    "ShadowCompositeKeyA",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowForeignCompositeKey>(
                    context,
                    "ShadowCompositeKeyA",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowForeignCompositeKey>(
                    context,
                    "ShadowCompositeKeyA",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "89", "foo" }),
                CreateShadowEntityShared<ShadowForeignCompositeKey>(
                    context,
                    "ShadowCompositeKeyB",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntityShared<ShadowForeignCompositeKey>(
                    context,
                    "ShadowCompositeKeyB",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowForeignCompositeKey>(
                    context,
                    "ShadowCompositeKeyB",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowForeignCompositeKey>(
                    context,
                    "ShadowCompositeKeyB",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "89", "foo" })
            };

            var foundA = context.ChangeTracker
                .GetEntries(
                    "ShadowCompositeKeyA",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ShadowForeignCompositeKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundB = context.ChangeTracker
                .GetEntries(
                    "ShadowCompositeKeyB",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ShadowForeignCompositeKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[4], foundB);
            Assert.Contains(entities[7], foundB);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "ShadowCompositeKeyA",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "99", "foo" }));

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "ShadowCompositeKeyB",
                    new[] { "CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_shadow_composite_non_key_by_shared()
        {
            using var context = new FindContextShared();
            var entities = new[]
            {
                CreateShadowEntityShared<ShadowCompositeNonKey>(
                    context,
                    "ShadowCompositeNonKeyA",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntityShared<ShadowCompositeNonKey>(
                    context,
                    "ShadowCompositeNonKeyA",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowCompositeNonKey>(
                    context,
                    "ShadowCompositeNonKeyA",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowCompositeNonKey>(
                    context,
                    "ShadowCompositeNonKeyA",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "89", "foo" }),
                CreateShadowEntityShared<ShadowCompositeNonKey>(
                    context,
                    "ShadowCompositeNonKeyB",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "87", "foo" }),
                CreateShadowEntityShared<ShadowCompositeNonKey>(
                    context,
                    "ShadowCompositeNonKeyB",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowCompositeNonKey>(
                    context,
                    "ShadowCompositeNonKeyB",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" }),
                CreateShadowEntityShared<ShadowCompositeNonKey>(
                    context,
                    "ShadowCompositeNonKeyB",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "89", "foo" })
            };

            var foundA = context.ChangeTracker
                .GetEntries(
                    "ShadowCompositeNonKeyA",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ShadowCompositeNonKey>()
                .ToList();

            Assert.Equal(2, foundA.Count);
            Assert.Contains(entities[1], foundA);
            Assert.Contains(entities[2], foundA);

            var foundB = context.ChangeTracker
                .GetEntries(
                    "ShadowCompositeNonKeyB",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "88", "foo" })
                .Select(e => e.Entity)
                .Cast<ShadowCompositeNonKey>()
                .ToList();

            Assert.Equal(2, foundB.Count);
            Assert.Contains(entities[4], foundB);
            Assert.Contains(entities[7], foundB);

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "ShadowCompositeNonKeyA",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "99", "foo" }));

            Assert.Empty(
                context.ChangeTracker.GetEntries(
                    "ShadowCompositeNonKeyB",
                    new[] { "Int", "String", "Foo" },
                    new object[] { 1, "99", "foo" }));
        }

        [ConditionalFact]
        public virtual void Find_base_type_tracked()
        {
            using var context = new FindContext();
            var entity = context.Attach(new BaseType { Id = 88 }).Entity;

            Assert.Same(entity, context.ChangeTracker.FindEntry(typeof(BaseType), 88)!.Entity);
        }

        [ConditionalFact]
        public virtual void Find_base_type_tracked_generic()
        {
            using var context = new FindContext();
            var entity = context.Attach(new BaseType { Id = 88 }).Entity;

            Assert.Same(entity, context.ChangeTracker.FindEntry<BaseType, int>(88)!.Entity);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_base_type_not_tracked()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry(typeof(BaseType), 99));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_base_type_not_tracked_generic()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry<BaseType, int>(99));
        }

        [ConditionalFact]
        public virtual void Find_derived_type_tracked()
        {
            using var context = new FindContext();
            var entity = context.Attach(new DerivedType { Id = 88 }).Entity;

            Assert.Same(entity, context.ChangeTracker.FindEntry(typeof(DerivedType), 88)!.Entity);
        }

        [ConditionalFact]
        public virtual void Find_derived_type_tracked_generic()
        {
            using var context = new FindContext();
            var entity = context.Attach(new DerivedType { Id = 88 }).Entity;

            Assert.Same(entity, context.ChangeTracker.FindEntry<DerivedType, int>(88)!.Entity);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_derived_type_not_tracked()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry(typeof(DerivedType), 99));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_derived_type_not_tracked_generic()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry<DerivedType, int>(99));
        }

        [ConditionalFact]
        public virtual void Find_base_type_using_derived_set_tracked()
        {
            using var context = new FindContext();
            context.Attach(new BaseType { Id = 88 });

            Assert.Null(context.ChangeTracker.FindEntry(typeof(DerivedType), 88));
        }

        [ConditionalFact]
        public virtual void Find_base_type_using_derived_set_tracked_generic()
        {
            using var context = new FindContext();
            context.Attach(new BaseType { Id = 88 });

            Assert.Null(context.ChangeTracker.FindEntry<DerivedType, int>(88));
        }

        [ConditionalFact]
        public virtual void Find_derived_type_using_base_set_tracked()
        {
            using var context = new FindContext();
            var entity = context.Attach(new DerivedType { Id = 88 }).Entity;

            Assert.Same(entity, context.ChangeTracker.FindEntry(typeof(BaseType), 88)!.Entity);
        }

        [ConditionalFact]
        public virtual void Find_derived_type_using_base_set_tracked_generic()
        {
            using var context = new FindContext();
            var entity = context.Attach(new DerivedType { Id = 88 }).Entity;

            Assert.Same(entity, context.ChangeTracker.FindEntry<BaseType, int>(88)!.Entity);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_key()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry(typeof(IntKey), new object?[] { null }));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_key_generic()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry<IntKey>(new object?[] { null }));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_nullable_key()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry(typeof(NullableIntKey), new object?[] { null }));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_nullable_key_generic()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry<NullableIntKey>(new object?[] { null }));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_in_composite_key()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry(typeof(CompositeKey), new object?[] { 77, null }));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_in_composite_key_generic()
        {
            using var context = new FindContext();

            Assert.Null(context.ChangeTracker.FindEntry<CompositeKey>(new object?[] { 77, null }));
        }

        [ConditionalFact]
        public virtual void Throws_for_multiple_values_passed_for_simple_key()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.FindNotCompositeKey("IntKey", 2),
                Assert.Throws<ArgumentException>(() => context.ChangeTracker.FindEntry(typeof(IntKey), new object[] { 77, 88 })).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_multiple_values_passed_for_simple_key_generic()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.FindNotCompositeKey("IntKey", 2),
                Assert.Throws<ArgumentException>(() => context.ChangeTracker.FindEntry<IntKey>(new object[] { 77, 88 })).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_wrong_number_of_values_for_composite_key()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.FindValueCountMismatch("CompositeKey", 3, 1),
                Assert.Throws<ArgumentException>(() => context.ChangeTracker.FindEntry<CompositeKey>(new object[] { 77 })).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_wrong_number_of_values_for_composite_key_generic()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.FindValueCountMismatch("CompositeKey", 3, 1),
                Assert.Throws<ArgumentException>(() => context.ChangeTracker.FindEntry<CompositeKey>(new object[] { 77 })).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_type_for_simple_key()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
                Assert.Throws<ArgumentException>(() => context.ChangeTracker.FindEntry(typeof(IntKey), "77")).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_type_for_simple_key_generic()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
                Assert.Throws<ArgumentException>(() => context.ChangeTracker.FindEntry<IntKey, string>("77")).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_type_for_composite_key()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
                Assert.Throws<ArgumentException>(() => context.ChangeTracker.FindEntry(typeof(CompositeKey), new object[] { 77, 88, "X" }))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_type_for_composite_key_generic()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
                Assert.Throws<ArgumentException>(() => context.ChangeTracker.FindEntry<CompositeKey>(new object[] { 77, 88, "X" }))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_entity_type()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.InvalidSetType(nameof(Random)),
                Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.FindEntry(typeof(Random), 77)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_entity_type_generic()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.InvalidSetType(nameof(Random)),
                Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.FindEntry<Random, int>(77)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_entity_type_with_different_namespace()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.InvalidSetSameTypeWithDifferentNamespace(
                    typeof(NamespaceII.ShadowIntKey).DisplayName(), typeof(ShadowIntKey).DisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () => context.ChangeTracker.FindEntry(typeof(NamespaceII.ShadowIntKey), 77)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_entity_type_with_different_namespace_generic()
        {
            using var context = new FindContext();

            Assert.Equal(
                CoreStrings.InvalidSetSameTypeWithDifferentNamespace(
                    typeof(NamespaceII.ShadowIntKey).DisplayName(), typeof(ShadowIntKey).DisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () => context.ChangeTracker.FindEntry<NamespaceII.ShadowIntKey, int>(77)).Message);
        }

        private TEntity CreateShadowEntity<TEntity>(FindContext context, string propertyName, object value)
            where TEntity : class, new()
        {
            var entry = context.Entry(new TEntity());
            entry.Property(propertyName).CurrentValue = value;
            entry.State = EntityState.Added;
            entry.State = EntityState.Unchanged;
            return entry.Entity;
        }

        private TEntity CreateShadowEntity<TEntity>(FindContext context, string[] propertyNames, object[] values)
            where TEntity : class, new()
        {
            var entry = context.Entry(new TEntity());
            for (var i = 0; i < propertyNames.Length; i++)
            {
                entry.Property(propertyNames[i]).CurrentValue = values[i];
            }
            entry.State = EntityState.Added;
            entry.State = EntityState.Unchanged;
            return entry.Entity;
        }

        private TEntity CreateShadowEntityShared<TEntity>(
            FindContextShared context,
            string entityTypeName,
            string propertyName,
            object value)
            where TEntity : class, new()
        {
            var entry = context.Entry(entityTypeName, new TEntity());
            entry.Property(propertyName).CurrentValue = value;
            entry.State = EntityState.Added;
            entry.State = EntityState.Unchanged;
            return entry.Entity;
        }

        private TEntity CreateShadowEntityShared<TEntity>(
            FindContextShared context, string entityTypeName, string[] propertyNames, object[] values)
            where TEntity : class, new()
        {
            var entry = context.Entry(entityTypeName, new TEntity());
            for (var i = 0; i < propertyNames.Length; i++)
            {
                entry.Property(propertyNames[i]).CurrentValue = values[i];
            }
            entry.State = EntityState.Added;
            entry.State = EntityState.Unchanged;
            return entry.Entity;
        }

        protected class BaseType
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string? Foo { get; set; }
        }

        protected class DerivedType : BaseType
        {
            public string? Boo { get; set; }
        }

        protected class IntKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }
        }

        protected class AlternateIntKey
        {
            public Guid Id { get; set; }
            public int AlternateId { get; set; }
        }

        protected class ForeignIntKey
        {
            public Guid Id { get; set; }
            public int IntKeyId { get; set; }
            public IntKey? IntKey { get; set; }
        }

        protected class IntNonKey
        {
            public Guid Id { get; set; }
            public int? Int { get; set; }
        }

        protected class ShadowIntKey
        {
        }

        protected class ShadowAlternateIntKey
        {
            public Guid Id { get; set; }
        }

        protected class ShadowForeignIntKey
        {
            public Guid Id { get; set; }
            public ShadowIntKey? IntKey { get; set; }
        }

        protected class ShadowIntNonKey
        {
            public Guid Id { get; set; }
        }

        protected class NullableIntKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int? Id { get; set; }
        }

        protected class AlternateNullableIntKey
        {
            public Guid Id { get; set; }
            public int? AlternateId { get; set; }
        }

        protected class ForeignNullableIntKey
        {
            public Guid Id { get; set; }
            public int? NullableIntKeyId { get; set; }
            public NullableIntKey? NullableIntKey { get; set; }
        }

        protected class NullableIntNonKey
        {
            public Guid Id { get; set; }
            public int? NullableInt { get; set; }
        }

        protected class ShadowNullableIntKey
        {
        }

        protected class ShadowAlternateNullableIntKey
        {
            public Guid Id { get; set; }
        }

        protected class ShadowForeignNullableIntKey
        {
            public Guid Id { get; set; }
            public ShadowNullableIntKey? NullableIntKey { get; set; }
        }

        protected class ShadowNullableIntNonKey
        {
            public Guid Id { get; set; }
        }

        protected class StringKey
        {
            public string Id { get; set; } = null!;
        }

        protected class AlternateStringKey
        {
            public Guid Id { get; set; }
            public string AlternateId { get; set; } = null!;
        }

        protected class ForeignStringKey
        {
            public Guid Id { get; set; }
            public string? StringKeyId { get; set; }
            public StringKey? StringKey { get; set; }
        }

        protected class StringNonKey
        {
            public Guid Id { get; set; }
            public string? String { get; set; }
        }

        protected class ShadowStringKey
        {
        }

        protected class ShadowAlternateStringKey
        {
            public Guid Id { get; set; }
        }

        protected class ShadowForeignStringKey
        {
            public Guid Id { get; set; }
            public ShadowStringKey? StringKey { get; set; }
        }

        protected class ShadowStringNonKey
        {
            public Guid Id { get; set; }
        }

        protected class CompositeKey
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; } = null!;
            public string Foo { get; set; } = null!;
        }

        protected class AlternateCompositeKey
        {
            public Guid Id { get; set; }
            public int AlternateId1 { get; set; }
            public string AlternateId2 { get; set; } = null!;
            public string AlternateFoo { get; set; } = null!;
        }

        protected class ForeignCompositeKey
        {
            public Guid Id { get; set; }
            public int? CompositeKeyId1 { get; set; }
            public string? CompositeKeyId2 { get; set; }
            public string? CompositeKeyFoo { get; set; }
            public CompositeKey? CompositeKey { get; set; }
        }

        protected class CompositeNonKey
        {
            public Guid Id { get; set; }
            public int? Int { get; set; }
            public string? String { get; set; }
            public string? Foo { get; set; }
        }

        protected class ShadowCompositeKey
        {
        }

        protected class ShadowAlternateCompositeKey
        {
            public Guid Id { get; set; }
        }

        protected class ShadowForeignCompositeKey
        {
            public Guid Id { get; set; }
            public ShadowCompositeKey? CompositeKey { get; set; }
        }

        protected class ShadowCompositeNonKey
        {
            public Guid Id { get; set; }
        }

        private class FindContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public FindContext()
            {
                _serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<IntKey>();
                modelBuilder.Entity<AlternateIntKey>().HasAlternateKey(e => e.AlternateId);
                modelBuilder.Entity<ForeignIntKey>();
                modelBuilder.Entity<IntNonKey>();

                modelBuilder.Entity<ShadowIntKey>().Property<int>("Id").ValueGeneratedNever();
                modelBuilder.Entity<ShadowAlternateIntKey>(
                    b =>
                    {
                        b.Property<int>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });
                modelBuilder.Entity<ShadowForeignIntKey>();
                modelBuilder.Entity<ShadowIntNonKey>().Property<int>("Int");

                modelBuilder.Entity<NullableIntKey>();
                modelBuilder.Entity<AlternateNullableIntKey>().HasAlternateKey(e => e.AlternateId);
                modelBuilder.Entity<ForeignNullableIntKey>();
                modelBuilder.Entity<NullableIntNonKey>();

                modelBuilder.Entity<ShadowNullableIntKey>().Property<int?>("Id").ValueGeneratedNever();
                modelBuilder.Entity<ShadowAlternateNullableIntKey>(
                    b =>
                    {
                        b.Property<int?>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });
                modelBuilder.Entity<ShadowForeignNullableIntKey>();
                modelBuilder.Entity<ShadowNullableIntNonKey>().Property<int?>("NullableInt");

                modelBuilder.Entity<StringKey>();
                modelBuilder.Entity<AlternateStringKey>().HasAlternateKey(e => e.AlternateId);
                modelBuilder.Entity<ForeignStringKey>();
                modelBuilder.Entity<StringNonKey>();

                modelBuilder.Entity<ShadowStringKey>().Property<string>("Id").ValueGeneratedNever();
                modelBuilder.Entity<ShadowAlternateStringKey>(
                    b =>
                    {
                        b.Property<string>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });
                modelBuilder.Entity<ShadowForeignStringKey>();
                modelBuilder.Entity<ShadowStringNonKey>().Property<string>("String");

                modelBuilder.Entity<CompositeKey>()
                    .HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Foo
                        });

                modelBuilder.Entity<AlternateCompositeKey>()
                    .HasAlternateKey(
                        e => new
                        {
                            e.AlternateId1,
                            e.AlternateId2,
                            e.AlternateFoo
                        });

                modelBuilder.Entity<ForeignCompositeKey>()
                    .HasOne(e => e.CompositeKey)
                    .WithMany()
                    .HasForeignKey(
                        e => new
                        {
                            e.CompositeKeyId1,
                            e.CompositeKeyId2,
                            e.CompositeKeyFoo
                        });

                modelBuilder.Entity<CompositeNonKey>();

                modelBuilder.Entity<ShadowCompositeKey>(
                    b =>
                    {
                        b.Property<int>("Id1");
                        b.Property<string>("Id2");
                        b.Property<string>("Foo");
                        b.HasKey("Id1", "Id2", "Foo");
                    });

                modelBuilder.Entity<ShadowAlternateCompositeKey>(
                    b =>
                    {
                        b.Property<int>("AlternateId1");
                        b.Property<string>("AlternateId2");
                        b.Property<string>("AlternateFoo");
                        b.HasAlternateKey("AlternateId1", "AlternateId2", "AlternateFoo");
                    });

                modelBuilder.Entity<ShadowForeignCompositeKey>()
                    .HasOne(e => e.CompositeKey)
                    .WithMany()
                    .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");

                modelBuilder.Entity<ShadowCompositeNonKey>().Property<string>("String");
                modelBuilder.Entity<ShadowCompositeNonKey>(
                    b =>
                    {
                        b.Property<int?>("Int");
                        b.Property<string?>("String");
                        b.Property<string?>("Foo");
                    });

                modelBuilder.Entity<BaseType>();
                modelBuilder.Entity<DerivedType>();
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(FindContext));
        }

        private class FindContextShared : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public FindContextShared()
            {
                _serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.SharedTypeEntity<IntKey>("IntKeyA");
                modelBuilder.SharedTypeEntity<IntKey>("IntKeyB");
                modelBuilder.SharedTypeEntity<AlternateIntKey>("AlternateIntKeyA").HasAlternateKey(e => e.AlternateId);
                modelBuilder.SharedTypeEntity<AlternateIntKey>("AlternateIntKeyB").HasAlternateKey(e => e.AlternateId);

                modelBuilder.SharedTypeEntity<ForeignIntKey>("ForeignIntKeyA").HasOne("IntKeyA", "IntKey").WithMany();
                modelBuilder.SharedTypeEntity<ForeignIntKey>("ForeignIntKeyB").HasOne("IntKeyB", "IntKey").WithMany();

                modelBuilder.SharedTypeEntity<IntNonKey>("IntNonKeyA");
                modelBuilder.SharedTypeEntity<IntNonKey>("IntNonKeyB");

                modelBuilder.SharedTypeEntity<ShadowIntKey>("ShadowIntKeyA").Property<int>("Id").ValueGeneratedNever();
                modelBuilder.SharedTypeEntity<ShadowIntKey>("ShadowIntKeyB").Property<int>("Id").ValueGeneratedNever();

                modelBuilder.SharedTypeEntity<ShadowAlternateIntKey>(
                    "ShadowAlternateIntKeyA",
                    b =>
                    {
                        b.Property<int>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });
                modelBuilder.SharedTypeEntity<ShadowAlternateIntKey>(
                    "ShadowAlternateIntKeyB",
                    b =>
                    {
                        b.Property<int>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });

                modelBuilder.SharedTypeEntity<ShadowForeignIntKey>("ShadowForeignIntKeyA").HasOne("ShadowIntKeyA", "IntKey").WithMany();
                modelBuilder.SharedTypeEntity<ShadowForeignIntKey>("ShadowForeignIntKeyB").HasOne("ShadowIntKeyB", "IntKey").WithMany();

                modelBuilder.SharedTypeEntity<ShadowIntNonKey>("ShadowIntNonKeyA").Property<int>("Int");
                modelBuilder.SharedTypeEntity<ShadowIntNonKey>("ShadowIntNonKeyB").Property<int>("Int");

                modelBuilder.SharedTypeEntity<NullableIntKey>("NullableIntKeyA");
                modelBuilder.SharedTypeEntity<NullableIntKey>("NullableIntKeyB");
                modelBuilder.SharedTypeEntity<AlternateNullableIntKey>("AlternateNullableIntKeyA").HasAlternateKey(e => e.AlternateId);
                modelBuilder.SharedTypeEntity<AlternateNullableIntKey>("AlternateNullableIntKeyB").HasAlternateKey(e => e.AlternateId);

                modelBuilder.SharedTypeEntity<ForeignNullableIntKey>("ForeignNullableIntKeyA").HasOne("NullableIntKeyA", "NullableIntKey")
                    .WithMany();
                modelBuilder.SharedTypeEntity<ForeignNullableIntKey>("ForeignNullableIntKeyB").HasOne("NullableIntKeyB", "NullableIntKey")
                    .WithMany();

                modelBuilder.SharedTypeEntity<NullableIntNonKey>("NullableIntNonKeyA");
                modelBuilder.SharedTypeEntity<NullableIntNonKey>("NullableIntNonKeyB");

                modelBuilder.SharedTypeEntity<ShadowNullableIntKey>("ShadowNullableIntKeyA").Property<int?>("Id").ValueGeneratedNever();
                modelBuilder.SharedTypeEntity<ShadowNullableIntKey>("ShadowNullableIntKeyB").Property<int?>("Id").ValueGeneratedNever();

                modelBuilder.SharedTypeEntity<ShadowAlternateNullableIntKey>(
                    "ShadowAlternateNullableIntKeyA",
                    b =>
                    {
                        b.Property<int?>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });
                modelBuilder.SharedTypeEntity<ShadowAlternateNullableIntKey>(
                    "ShadowAlternateNullableIntKeyB",
                    b =>
                    {
                        b.Property<int?>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });

                modelBuilder.SharedTypeEntity<ShadowForeignNullableIntKey>("ShadowForeignNullableIntKeyA")
                    .HasOne("ShadowNullableIntKeyA", "NullableIntKey").WithMany();
                modelBuilder.SharedTypeEntity<ShadowForeignNullableIntKey>("ShadowForeignNullableIntKeyB")
                    .HasOne("ShadowNullableIntKeyB", "NullableIntKey").WithMany();

                modelBuilder.SharedTypeEntity<ShadowNullableIntNonKey>("ShadowNullableIntNonKeyA").Property<int?>("NullableInt");
                modelBuilder.SharedTypeEntity<ShadowNullableIntNonKey>("ShadowNullableIntNonKeyB").Property<int?>("NullableInt");

                modelBuilder.SharedTypeEntity<StringKey>("StringKeyA");
                modelBuilder.SharedTypeEntity<StringKey>("StringKeyB");
                modelBuilder.SharedTypeEntity<AlternateStringKey>("AlternateStringKeyA").HasAlternateKey(e => e.AlternateId);
                modelBuilder.SharedTypeEntity<AlternateStringKey>("AlternateStringKeyB").HasAlternateKey(e => e.AlternateId);

                modelBuilder.SharedTypeEntity<ForeignStringKey>("ForeignStringKeyA").HasOne("StringKeyA", "StringKey").WithMany();
                modelBuilder.SharedTypeEntity<ForeignStringKey>("ForeignStringKeyB").HasOne("StringKeyB", "StringKey").WithMany();

                modelBuilder.SharedTypeEntity<StringNonKey>("StringNonKeyA");
                modelBuilder.SharedTypeEntity<StringNonKey>("StringNonKeyB");

                modelBuilder.SharedTypeEntity<ShadowStringKey>("ShadowStringKeyA").Property<string>("Id").ValueGeneratedNever();
                modelBuilder.SharedTypeEntity<ShadowStringKey>("ShadowStringKeyB").Property<string>("Id").ValueGeneratedNever();

                modelBuilder.SharedTypeEntity<ShadowAlternateStringKey>(
                    "ShadowAlternateStringKeyA",
                    b =>
                    {
                        b.Property<string>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });
                modelBuilder.SharedTypeEntity<ShadowAlternateStringKey>(
                    "ShadowAlternateStringKeyB",
                    b =>
                    {
                        b.Property<string>("AlternateId");
                        b.HasAlternateKey("AlternateId");
                    });

                modelBuilder.SharedTypeEntity<ShadowForeignStringKey>("ShadowForeignStringKeyA").HasOne("ShadowStringKeyA", "StringKey")
                    .WithMany();
                modelBuilder.SharedTypeEntity<ShadowForeignStringKey>("ShadowForeignStringKeyB").HasOne("ShadowStringKeyB", "StringKey")
                    .WithMany();

                modelBuilder.SharedTypeEntity<ShadowStringNonKey>("ShadowStringNonKeyA").Property<string>("String");
                modelBuilder.SharedTypeEntity<ShadowStringNonKey>("ShadowStringNonKeyB").Property<string>("String");

                modelBuilder.SharedTypeEntity<CompositeKey>("CompositeKeyA")
                    .HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Foo
                        });
                modelBuilder.SharedTypeEntity<CompositeKey>("CompositeKeyB")
                    .HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Foo
                        });

                modelBuilder.SharedTypeEntity<AlternateCompositeKey>("AlternateCompositeKeyA")
                    .HasAlternateKey(
                        e => new
                        {
                            e.AlternateId1,
                            e.AlternateId2,
                            e.AlternateFoo
                        });
                modelBuilder.SharedTypeEntity<AlternateCompositeKey>("AlternateCompositeKeyB")
                    .HasAlternateKey(
                        e => new
                        {
                            e.AlternateId1,
                            e.AlternateId2,
                            e.AlternateFoo
                        });

                modelBuilder.SharedTypeEntity<ForeignCompositeKey>("ForeignCompositeKeyA")
                    .HasOne("CompositeKeyA", "CompositeKey")
                    .WithMany()
                    .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");
                modelBuilder.SharedTypeEntity<ForeignCompositeKey>("ForeignCompositeKeyB")
                    .HasOne("CompositeKeyB", "CompositeKey")
                    .WithMany()
                    .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");

                modelBuilder.SharedTypeEntity<CompositeNonKey>("CompositeNonKeyA");
                modelBuilder.SharedTypeEntity<CompositeNonKey>("CompositeNonKeyB");

                modelBuilder.SharedTypeEntity<ShadowCompositeKey>("ShadowCompositeKeyA",
                    b =>
                    {
                        b.Property<int>("Id1");
                        b.Property<string>("Id2");
                        b.Property<string>("Foo");
                        b.HasKey("Id1", "Id2", "Foo");
                    });
                modelBuilder.SharedTypeEntity<ShadowCompositeKey>("ShadowCompositeKeyB",
                    b =>
                    {
                        b.Property<int>("Id1");
                        b.Property<string>("Id2");
                        b.Property<string>("Foo");
                        b.HasKey("Id1", "Id2", "Foo");
                    });

                modelBuilder.SharedTypeEntity<ShadowAlternateCompositeKey>("ShadowAlternateCompositeKeyA",
                    b =>
                    {
                        b.Property<int>("AlternateId1");
                        b.Property<string>("AlternateId2");
                        b.Property<string>("AlternateFoo");
                        b.HasAlternateKey("AlternateId1", "AlternateId2", "AlternateFoo");
                    });
                modelBuilder.SharedTypeEntity<ShadowAlternateCompositeKey>("ShadowAlternateCompositeKeyB",
                    b =>
                    {
                        b.Property<int>("AlternateId1");
                        b.Property<string>("AlternateId2");
                        b.Property<string>("AlternateFoo");
                        b.HasAlternateKey("AlternateId1", "AlternateId2", "AlternateFoo");
                    });

                modelBuilder.SharedTypeEntity<ShadowForeignCompositeKey>("ShadowForeignCompositeKeyA")
                    .HasOne("ShadowCompositeKeyA", "CompositeKey")
                    .WithMany()
                    .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");
                modelBuilder.SharedTypeEntity<ShadowForeignCompositeKey>("ShadowForeignCompositeKeyB")
                    .HasOne("ShadowCompositeKeyB", "CompositeKey")
                    .WithMany()
                    .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");

                modelBuilder.SharedTypeEntity<ShadowCompositeNonKey>("ShadowCompositeNonKeyA").Property<string>("String");
                modelBuilder.SharedTypeEntity<ShadowCompositeNonKey>("ShadowCompositeNonKeyB").Property<string>("String");

                modelBuilder.SharedTypeEntity<ShadowCompositeNonKey>("ShadowCompositeNonKeyA",
                    b =>
                    {
                        b.Property<int?>("Int");
                        b.Property<string?>("String");
                        b.Property<string?>("Foo");
                    });
                modelBuilder.SharedTypeEntity<ShadowCompositeNonKey>("ShadowCompositeNonKeyB",
                    b =>
                    {
                        b.Property<int?>("Int");
                        b.Property<string?>("String");
                        b.Property<string?>("Foo");
                    });
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(FindContextShared));
        }
    }
}

namespace Microsoft.EntityFrameworkCore.NamespaceII
{
    internal class ShadowIntKey
    {
    }
}
