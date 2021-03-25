using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;
using System.Linq;

namespace TraceContextPropagationToKafka
{
    public static class TraceContextExtensions
    {

        public static bool TrySetTraceContextWithTraceparent(this TelemetryContext ctx, string traceparent)
        {
            string operationId, operationParentId;
            try
            {
                operationId = GetOperationId(traceparent);
                operationParentId = GetOperationParentId(traceparent);
            }
            catch
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
        private static string GetOperationId(string traceparent)
        {
            return traceparent.Split('-').ElementAt(1);
        }

        private static string GetOperationParentId(string traceparent)
        {
            return traceparent.Split('-').ElementAt(2);
        }

        public static (string traceparent, string tracestate) GetTraceContext(this Activity activity)
        {
            return (GetTraceparent(activity), GetTracestate(activity));
        }

        private static string GetTraceparent(Activity activity)
        {
            return !string.IsNullOrEmpty(activity?.Id) ? activity.Id : string.Empty;
        }
        private static string GetTracestate(Activity activity)
        {
            return !string.IsNullOrEmpty(activity?.TraceStateString) ? activity.TraceStateString : string.Empty;
        }
    }
}
