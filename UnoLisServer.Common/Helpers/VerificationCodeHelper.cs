using System;
using System.Collections.Generic;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Models;

namespace UnoLisServer.Common.Helpers
{
    /// <summary>
    /// Class that manages verification codes for various purposes (e.g., account verification, password reset).
    /// </summary>
    public interface IVerificationCodeHelper
    {
        string GenerateAndStoreCode(string identifier, CodeType type);
        bool ValidateCode(CodeValidationRequest request);
        bool CanRequestCode(string identifier, CodeType type);
    }

    public class VerificationCodeHelper : IVerificationCodeHelper
    {
        private static readonly Lazy<VerificationCodeHelper> _instance =
            new Lazy<VerificationCodeHelper>(() => new VerificationCodeHelper());
        private readonly object _lockObject = new object();
        private readonly Dictionary<string, CodeInfo> _codeStorage =
            new Dictionary<string, CodeInfo>();
        private readonly Random _random = new Random();

        public static IVerificationCodeHelper Instance => _instance.Value;

        private VerificationCodeHelper() { }

        sealed class CodeInfo
        {
            public string Code { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime ExpirationTime { get; set; }
            public CodeType Type { get; set; }
        }

        public bool CanRequestCode(string identifier, CodeType type)
        {
            lock (_lockObject)
            {
                if (_codeStorage.TryGetValue(GetKey(identifier, type), out CodeInfo existingCode) &&
                    existingCode.Type == type)
                {
                    return DateTime.UtcNow.Subtract(existingCode.CreationTime).TotalSeconds >= 60;
                }
                return true;
            }
        }

        public string GenerateAndStoreCode(string identifier, CodeType type)
        {
            var key = GetKey(identifier, type);
            lock (_lockObject)
            {
                var code = _random.Next(100000, 999999).ToString("D6");
                var now = DateTime.UtcNow;
                var codeInfo = new CodeInfo
                {
                    Code = code,
                    CreationTime = now,
                    ExpirationTime = now.AddMinutes(5),
                    Type = type
                };
                _codeStorage[key] = codeInfo;
                return code;
            }
        }
        
        public bool ValidateCode(CodeValidationRequest request)
        {
            var key = GetKey(request.Identifier, (CodeType) request.CodeType);

            lock (_lockObject)
            {
                if (!_codeStorage.TryGetValue(key, out CodeInfo codeInfo))
                {
                    return false;
                }

                if (DateTime.UtcNow >= codeInfo.ExpirationTime)
                {
                    return false;
                }

                if (codeInfo.Type != (CodeType) request.CodeType)
                {
                    return false;
                }

                if (codeInfo.Code != request.Code)
                {
                    return false;
                }

                if (request.Consume)
                {
                    _codeStorage.Remove(key);
                }

                return true;
            }
        }

        private static string GetKey(string identifier, CodeType type)
        {
            return $"{type}:{identifier}";
        }
    }
}
