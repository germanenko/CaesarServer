using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency.Enum
{
    public class OperationFailureCodeAttribute : Attribute
    {
        public OperationFailureCode OperationFailureCode { get; }

        public OperationFailureCodeAttribute(OperationFailureCode operationFailureCode)
        {
            OperationFailureCode = operationFailureCode;
        }
    }


    public enum ErrorCode
    {
        [OperationFailureCode(OperationFailureCode.Validation)]
        TitleRequired = 101,

        [OperationFailureCode(OperationFailureCode.Validation)]
        DescriptionRequired = 102,

        [OperationFailureCode(OperationFailureCode.Conflict)]
        VersionMismatch = 201,

        [OperationFailureCode(OperationFailureCode.Conflict)]
        DuplicateRequest = 202,

        [OperationFailureCode(OperationFailureCode.Infrastructure)]
        Infrastructure = 300,

        [OperationFailureCode(OperationFailureCode.AccessDenied)]
        WriteDenied = 401,

        [OperationFailureCode(OperationFailureCode.AccessDenied)]
        ReadDenied = 402,
    }

    public static class ErrorCodeExtensions
    {
        public static OperationFailureCode GetErrorKind(this ErrorCode errorCode)
        {
            var field = errorCode.GetType().GetField(errorCode.ToString());
            var attribute = field.GetCustomAttribute<OperationFailureCodeAttribute>();
            return attribute?.OperationFailureCode ?? OperationFailureCode.Infrastructure;
        }
    }
}
