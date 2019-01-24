﻿using StringSearch.Legacy;
using StringSearch.Legacy.Collections;

namespace StringSearch
{
    public interface IPrecomputedSearchResults
    {
        IBigArray<PrecomputedSearchResult>[] Results { get; }
    }
}
