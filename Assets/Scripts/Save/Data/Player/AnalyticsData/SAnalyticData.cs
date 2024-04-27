﻿using Data;
using Enums;
using System.Collections.Generic;
using Tools;

namespace Save.Data
{
    public class SAnalyticsData
    {
        public int Count;

        public SAnalyticsData()
        {
            Count = 1;
        }

        public virtual object GetValue(EAnalyticsParam analyticsParam)
        {
            ErrorHandler.Warning("Trying to get value " + analyticsParam.ToString() + " but " + this.GetType() + " has no such value");
            return null;
        }

        /// <summary>
        /// Check if values validates a filter on one of theme
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="validateOnNull"></param>
        /// <returns></returns>
        public bool CheckFilter(SAnalyticsFilter filter, bool validateOnNull = false)
        {
            return filter.Check(GetValue(filter.AnalyticParam), validateOnNull);
        }

        /// <summary>
        /// Check if values validates a list of filters
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="validateOnNull"></param>
        /// <returns></returns>
        public bool CheckFilters(List<SAnalyticsFilter> filters, bool validateOnNull = false)
        {
            foreach (SAnalyticsFilter filter in filters)
            {
                if (!filter.Check(GetValue(filter.AnalyticParam), validateOnNull))
                    return false;
            }

            return true;
        }
    }
}