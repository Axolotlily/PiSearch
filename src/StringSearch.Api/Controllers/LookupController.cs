﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StringSearch.Api.Contracts;
using StringSearch.Api.Contracts.Searches;
using StringSearch.Api.Contracts.Searches.Lookups;
using StringSearch.DataLayer;
using StringSearch.Legacy;
using StringSearch.LegacyWrappers;

namespace StringSearch.Api.Controllers
{
    [Route("api/Lookup")]
    public class LookupController : Controller
    {
        private readonly DigitsWrapper digitsWrapper;
        private readonly SuffixArrayWrapper suffixArrayWrapper;
        private readonly IPrecomputedSearchResults precomputedSearchResults;
        private readonly IDbSearches dbSearches;

        public LookupController(DigitsWrapper digitsWrapper, SuffixArrayWrapper suffixArrayWrapper,
            IPrecomputedSearchResults precomputedSearchResults, IDbSearches dbSearches)
        {
            this.digitsWrapper = digitsWrapper;
            this.suffixArrayWrapper = suffixArrayWrapper;
            this.precomputedSearchResults = precomputedSearchResults;
            this.dbSearches = dbSearches;
        }

        public IActionResult Index(LookupRequest request)
        {
            SearchSummary summary = new SearchSummary(request.Find, request.ResultId, request.MinSuffixArrayIdx,
                request.MaxSuffixArrayIdx, false, request.NumSurroundingDigits,
                Request.HttpContext.Connection.RemoteIpAddress);

            // Time the request being processed
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Do the search
            SuffixArrayRange suffixArrayRange = summary.HasSuffixArrayIndices
                // If suffix array indices have been specified, we don't need to do the search
                ? new SuffixArrayRange(summary.MinSuffixArrayIdx, summary.MaxSuffixArrayIdx,
                    suffixArrayWrapper.SuffixArray, digitsWrapper.Digits)
                // Otherwise we need to perform the suffix array search
                : SearchString.Search(suffixArrayWrapper.SuffixArray, digitsWrapper.Digits, summary.Find,
                    precomputedSearchResults.Results);

            // If there is a result
            LookupResponse response = new LookupResponse();
            if (suffixArrayRange.HasResults)
            {
                response.SuffixArrayMinIdx = suffixArrayRange.Min;
                response.SuffixArrayMaxIdx = suffixArrayRange.Max;
                response.NumResults = (int)(suffixArrayRange.Max - suffixArrayRange.Min + 1);
                response.ResultId = summary.ResultId;

                // Note: This is the only stage where Count & Lookup differ. Count doesn't do this bit.
                long resultStringIndex = suffixArrayRange.SortedValues[summary.ResultId];
                response.ResultStringIndex = resultStringIndex;

                // Get the digits surrounding this search result
                StringBuilder beforeBuilder = new StringBuilder();
                StringBuilder afterBuilder = new StringBuilder();
                if (summary.NumSurroundingDigits > 0)
                {
                    long beforeStartIdx = Math.Max(0, resultStringIndex - request.NumSurroundingDigits);
                    for (long i = beforeStartIdx; i < resultStringIndex; i++)
                    {
                        beforeBuilder.Append(digitsWrapper.Digits[i]);
                    }

                    long afterStartIdx = Math.Min(digitsWrapper.Digits.Length, resultStringIndex + summary.Find.Length);
                    long afterEndIdx = Math.Min(digitsWrapper.Digits.Length,
                        afterStartIdx + request.NumSurroundingDigits);
                    for (long i = afterStartIdx; i < afterEndIdx; i++)
                    {
                        afterBuilder.Append(digitsWrapper.Digits[i]);
                    }
                }
                response.SurroundingDigits = new SurroundingDigits(beforeBuilder.ToString(), afterBuilder.ToString());
            }
            else
            {
                response.NumResults = 0;
            }

            stopwatch.Stop();
            summary.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
            response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

            // Log this search to the database. Defer until after the response is sent to the client
            Response.OnCompleted(async () =>
            {
                await dbSearches.Insert(summary);
            });

            return Ok(response);
        }
    }
}
