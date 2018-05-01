﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CommandLine.Utilities;
using Compression.FileHandling;
using Compression.Utilities;
using ErrorHandling.Exceptions;
using Jasix.DataStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OptimizedCore;


namespace Jasix
{
    public sealed class IndexCreator : IDisposable
    {
        #region members
        private readonly BgzipTextReader _reader;
        private readonly Stream _writeStream;
        private const string SectionToIndex = "positions";
        private readonly HashSet<string> _processedChromosome;

        private readonly Benchmark _chromBenchmark;
        private readonly Benchmark _benchmark;

        #endregion

        public IndexCreator(BlockGZipStream readStream, Stream writeStream)
        {
            _reader              = new BgzipTextReader(readStream);
            _writeStream         = writeStream;
            _processedChromosome = new HashSet<string>();
            _chromBenchmark      = new Benchmark();
            _benchmark           = new Benchmark();
        }

        public IndexCreator(string fileName)
            : this(
                new BlockGZipStream(FileUtilities.GetReadStream(fileName), CompressionMode.Decompress),
                FileUtilities.GetCreateStream(fileName + JasixCommons.FileExt))
        {}

        public void CreateIndex()
        {
            var searchTag = $"\"{SectionToIndex}\":[";
            const string headerTag = "{\"header\":";
            var index = new JasixIndex();
            string line;

            //skipping lines before the sectionToIndex arrives
            while ((line = _reader.ReadLine()) != null)
            {
                if (line.StartsWith(headerTag))
                    index.HeaderLine = ExtractHeader(line);
                if (line.EndsWith(searchTag)) break;
            }

            // we need the location before accessing the line
            var fileLoc = _reader.Position;

            string previousChr = "";
            int previousPos = 0;
            while ((line = _reader.ReadLine()) != null)
            {
                if (line.OptimizedStartsWith(']')) break;
                line = line.TrimEnd(',');
                var chrPos = GetChromPosition(line);

                CheckSorting(chrPos.chr, chrPos.position, previousChr, previousPos);

                index.Add(chrPos.chr, chrPos.position, chrPos.end, fileLoc);
                fileLoc = _reader.Position;
                previousChr = chrPos.chr;
                previousPos = chrPos.position;

            }

            index.Write(_writeStream);

            Console.WriteLine();

            var peakMemoryUsageBytes = MemoryUtilities.GetPeakMemoryUsage();
            var wallTimeSpan = _benchmark.GetElapsedTime();
            Console.WriteLine();
            if (peakMemoryUsageBytes > 0) Console.WriteLine("Peak memory usage: {0}", MemoryUtilities.ToHumanReadable(peakMemoryUsageBytes));
            Console.WriteLine("Time: {0}", Benchmark.ToHumanReadable(wallTimeSpan));
        }

        private static string ExtractHeader(string line)
        {
            string res = null;
            var reader = new JsonTextReader(new StringReader(line));
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName) continue;

                // Load each object from the stream and do something with it
                var obj = JToken.ReadFrom(reader);
                res = obj.ToString(Formatting.None);
                break;
            }
            return res;
        }

        // ReSharper disable once UnusedParameter.Local
        private void CheckSorting(string chr, int pos, string previousChr, int previousPos)
        {
            if (chr != previousChr && _processedChromosome.Contains(chr))
            {
                throw new UserErrorException($"the Json file is not sorted at {chr}: {pos}");
            }

            if (chr == previousChr && pos < previousPos)
            {
                throw new UserErrorException($"the Json file is not sorted at {chr}: {pos}");
            }

            if (chr == previousChr || previousChr == "") return;

            Console.WriteLine($"Ref Sequence {previousChr} indexed in {Benchmark.ToHumanReadable(_chromBenchmark.GetElapsedTime())}");
            _chromBenchmark.Reset();
        }

        internal static (string chr, int position, int end) GetChromPosition(string line)
        {
            JsonSchema jsonEntry;
            try
            {
                jsonEntry = JsonConvert.DeserializeObject<JsonSchema>(line);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error in line:\n{line}");
                throw;
            }

			var end = Utilities.GetJsonEntryEnd(jsonEntry);

            return (jsonEntry.chromosome, jsonEntry.position, end);
        }

        public void Dispose()
        {
            _writeStream.Flush();
            _reader.Dispose();
            _writeStream.Dispose();
        }
    }
}
