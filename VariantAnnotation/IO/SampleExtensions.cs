﻿using System.Text;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.IO
{
    public static class SampleExtensions
    {
        public static string GetJsonString(this ISample sample)
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddBoolValue("isEmpty", sample.IsEmpty);
            jsonObject.AddStringValue("genotype", sample.Genotype);
	        jsonObject.AddStringValue("repeatNumbers", sample.RepeatNumbers);
	        jsonObject.AddStringValue("repeatNumberSpans", sample.RepeatNumberSpans);
			jsonObject.AddDoubleValue("variantFreq", sample.VariantFrequency);
            jsonObject.AddIntValue("totalDepth", sample.TotalDepth);
            jsonObject.AddIntValue("genotypeQuality", sample.GenotypeQuality);
            jsonObject.AddIntValue("copyNumber", sample.CopyNumber);
            jsonObject.AddIntValues("alleleDepths", sample.AlleleDepths);
            jsonObject.AddBoolValue("failedFilter", sample.FailedFilter);
            jsonObject.AddIntValues("splitReadCounts", sample.SplitReadCounts);
            jsonObject.AddIntValues("pairedEndReadCounts", sample.PairEndReadCounts);
            jsonObject.AddBoolValue("lossOfHeterozygosity", sample.IsLossOfHeterozygosity);
            jsonObject.AddIntValue("deNovoQuality", sample.DeNovoQuality);

            sb.Append(JsonObject.CloseBrace);
            return sb.ToString();
        }
    }
}