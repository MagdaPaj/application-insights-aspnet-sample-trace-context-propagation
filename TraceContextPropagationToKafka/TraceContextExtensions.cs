using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TraceContextPropagationToKafka
{
    public static class TraceContextExtensions
    {

        public static bool TrySetOperationContextFromTraceparent(this TelemetryContext ctx, string traceparent)
        {
            var operationId = GetOperationId(traceparent);
            var operationParentId = GetOperationParentId(traceparent);

            if (string.IsNullOrEmpty(operationId) || string.IsNullOrEmpty(operationParentId))
            {
                return false;
            }

            ctx.Operation.Id = operationId;
            ctx.Operation.ParentId = operationParentId;
            return true;
        }

        public static void SetTraceContextWithTracestate(this Activity activity, string tracestate)
        {
            activity.TraceStateString = tracestate;
        }

        // Traceparent is a composition of operationId and operationParentId.
        // It has the following format 00-operationId-operationParentId-00
        // in the Application Insights. To get more details check:
        // https://docs.microsoft.com/en-us/azure/azure-monitor/app/correlation#correlation-headers-using-w3c-tracecontext
        private static string GetOperationId(string traceparent)
        {
            if (string.IsNullOrEmpty(traceparent))
            {
                return null;
            }
            return traceparent.Split('-').ElementAt(1);
        }

        private static string GetOperationParentId(string traceparent)
        {
            if (string.IsNullOrEmpty(traceparent))
            {
                return null;
            }
            return traceparent.Split('-').ElementAt(2);
        }

        public static Dictionary<string, string> PopulateHeaders(this Activity activity)
        {
            var headers = new Dictionary<string, string>();
            var (traceparent, tracestate) = (GetTraceparent(activity), GetTracestate(activity));

            if (!string.IsNullOrEmpty(traceparent))
            {
                headers.Add(HeaderNames.TraceParent, traceparent);
            }

            if (!string.IsNullOrEmpty(tracestate))
            {
                headers.Add(HeaderNames.TraceState, tracestate);
            }

            return headers;
        }

        private static string GetTraceparent(Activity activity)
        {
            return activity?.Id ?? string.Empty;
        }

        private static string GetTracestate(Activity activity)
        {
            return activity?.TraceStateString ?? string.Empty;
        }
    }
}
