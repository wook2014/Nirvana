﻿using System.IO;
using Genome;
using UnitTests.TestUtilities;
using VariantAnnotation.TranscriptAnnotation;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
	public sealed class BreakEndUtilitiesTests
    {
        [Theory]
        [InlineData(28722335, "T", "[3:115024109[T", true, "3", 115024109, false)]
        [InlineData(31410878, "C", "]6:42248252]C", true, "6", 42248252, true)]
        [InlineData(31561816, "C", "CGATCTCAT[6:41297838[", false, "6", 41297838, false)]
        [InlineData(84461562, "A", "A]8:100990100]", false, "8", 100990100, true)]
        [InlineData(32518102, "C", "C]HLA-DRB1*10:01:01:12922]", false, "HLA-DRB1*10:01:01", 12922, true)]
        public void CreateFromTranslocation_Nominal(int position, string refAllele, string altAllele,
            bool expectedOnReverseStrand, string expectedPartnerChr, int expectedPartnerPosition,
            bool expectedPartnerOnReverseStrand)
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, position, position, refAllele, altAllele, VariantType.translocation_breakend);
            BreakEndAdjacency[] adjacencies = BreakEndUtilities.CreateFromTranslocation(variant, ChromosomeUtilities.RefNameToChromosome);

            Assert.NotNull(adjacencies);
            Assert.Single(adjacencies);

            var observed = adjacencies[0];
            Assert.Equal(expectedOnReverseStrand,        observed.Origin.OnReverseStrand);
            Assert.Equal(expectedPartnerChr,             observed.Partner.Chromosome.EnsemblName);
            Assert.Equal(expectedPartnerPosition,        observed.Partner.Position);
            Assert.Equal(expectedPartnerOnReverseStrand, observed.Partner.OnReverseStrand);
        }

        [Fact]
        public void CreateFromTranslocation_InvalidAltAllele_ThrowException()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 100, 100, "A", "A{3:115024109{T", VariantType.translocation_breakend);
                // ReSharper disable once UnusedVariable
                BreakEndAdjacency[] adjacencies = BreakEndUtilities.CreateFromTranslocation(variant, ChromosomeUtilities.RefNameToChromosome);
            });
        }

        [Fact]
        public void CreateFromSymbolicAllele_Deletion()
        {
            var interval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 1594584, 1660503);
            BreakEndAdjacency[] adjacencies = BreakEndUtilities.CreateFromSymbolicAllele(interval, VariantType.deletion);

            Assert.NotNull(adjacencies);
            Assert.Equal(2, adjacencies.Length);

            var observed = adjacencies[0];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed.Origin.Chromosome.EnsemblName);
            Assert.Equal(1594583,           observed.Origin.Position);
            Assert.False(observed.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed.Partner.Chromosome.EnsemblName);
            Assert.Equal(1660504,           observed.Partner.Position);
            Assert.False(observed.Partner.OnReverseStrand);

            var observed2 = adjacencies[1];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed2.Origin.Chromosome.EnsemblName);
            Assert.Equal(1660504,           observed2.Origin.Position);
            Assert.True(observed2.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed2.Partner.Chromosome.EnsemblName);
            Assert.Equal(1594583,           observed2.Partner.Position);
            Assert.True(observed2.Partner.OnReverseStrand);
        }

        [Fact]
        public void CreateFromSymbolicAllele_Duplication()
        {
            var interval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 37820921, 38404543);
            BreakEndAdjacency[] adjacencies = BreakEndUtilities.CreateFromSymbolicAllele(interval, VariantType.tandem_duplication);

            Assert.NotNull(adjacencies);
            Assert.Equal(2, adjacencies.Length);

            var observed = adjacencies[0];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed.Origin.Chromosome.EnsemblName);
            Assert.Equal(38404543, observed.Origin.Position);
            Assert.False(observed.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed.Partner.Chromosome.EnsemblName);
            Assert.Equal(37820920, observed.Partner.Position);
            Assert.False(observed.Partner.OnReverseStrand);

            var observed2 = adjacencies[1];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed2.Origin.Chromosome.EnsemblName);
            Assert.Equal(37820920, observed2.Origin.Position);
            Assert.True(observed2.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed2.Partner.Chromosome.EnsemblName);
            Assert.Equal(38404543, observed2.Partner.Position);
            Assert.True(observed2.Partner.OnReverseStrand);
        }

        [Fact]
        public void CreateFromSymbolicAllele_Inversion()
        {
            var interval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 63989116, 64291267);
            BreakEndAdjacency[] adjacencies = BreakEndUtilities.CreateFromSymbolicAllele(interval, VariantType.inversion);

            Assert.NotNull(adjacencies);
            Assert.Equal(2, adjacencies.Length);

            var observed = adjacencies[0];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed.Origin.Chromosome.EnsemblName);
            Assert.Equal(63989115, observed.Origin.Position);
            Assert.False(observed.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed.Partner.Chromosome.EnsemblName);
            Assert.Equal(64291267, observed.Partner.Position);
            Assert.True(observed.Partner.OnReverseStrand);

            var observed2 = adjacencies[1];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed2.Origin.Chromosome.EnsemblName);
            Assert.Equal(64291268, observed2.Origin.Position);
            Assert.True(observed2.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, observed2.Partner.Chromosome.EnsemblName);
            Assert.Equal(63989116, observed2.Partner.Position);
            Assert.False(observed2.Partner.OnReverseStrand);
        }

        [Fact]
        public void CreateFromSymbolicAllele_UnhandledVariantType_ReturnNull()
        {
            var interval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 63989116, 64291267);
            BreakEndAdjacency[] adjacencies = BreakEndUtilities.CreateFromSymbolicAllele(interval, VariantType.complex_structural_alteration);

            Assert.Null(adjacencies);
        }
    }
}