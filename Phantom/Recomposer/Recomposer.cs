﻿using System.Collections.Generic;
using Phantom.CodonInformation;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf.VariantCreator;

namespace Phantom.Recomposer
{
    public sealed class Recomposer : IRecomposer
    {
        private readonly PositionProcessor _positionProcessor;

        private Recomposer(PositionProcessor positionProcessor) => _positionProcessor = positionProcessor;

        public static IRecomposer Create(ISequenceProvider sequenceProvider, ITranscriptAnnotationProvider taProvider, IVariantIdCreator vidCreator)
        {
            var transcriptIntervalArrays = taProvider.TranscriptIntervalArrays;
            var geneIntervalForest       = GeneForestGenerator.GetGeneForest(transcriptIntervalArrays);
            var codonInfoProvider        = new CodonInfoProvider(transcriptIntervalArrays);
            var variantGenerator         = new VariantGenerator(sequenceProvider, vidCreator);
            var positionBuffer           = new PositionBuffer(codonInfoProvider, geneIntervalForest);
            return new Recomposer(new PositionProcessor(positionBuffer, variantGenerator));
        }

        public IEnumerable<ISimplePosition> ProcessSimplePosition(ISimplePosition simplePosition) =>
            simplePosition == null
                ? _positionProcessor.ProcessBufferedPositions()
                : _positionProcessor.Process(simplePosition);
    }
}