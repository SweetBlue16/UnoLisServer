using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ProfileEditManager : IProfileEditManager
    {
        private readonly IProfileEditCallback _callback;
        private readonly IPlayerRepository _playerRepository;
        private readonly IVerificationCodeHelper _verificationCodeHelper;
        private readonly INotificationSender _notificationSender;

        public ProfileEditManager() : this(
            new PlayerRepository(),
            VerificationCodeHelper.Instance,
            NotificationSender.Instance,
            null)
        {
        }

        public ProfileEditManager(IPlayerRepository playerRepository,
            IVerificationCodeHelper verificationCodeHelper,
            INotificationSender notificationSender,
            IProfileEditCallback callbackTest = null)
        {
            _playerRepository = playerRepository;
            _callback = callbackTest ?? OperationContext.Current?.GetCallbackChannel<IProfileEditCallback>();
            _verificationCodeHelper = verificationCodeHelper;
            _notificationSender = notificationSender;
        }

        public async void RequestEmailChangeVerification(string nickname, string newEmail)
        {
            ResponseInfo<object> responseInfo = null;
            try
            {
                ProfileEditValidator.ValidateEmail(newEmail);
                var existingAccount = await _playerRepository.GetPlayerProfileByNicknameAsync(nickname);
                if (existingAccount.idPlayer > 0)
                {
                    throw new ValidationException(MessageCode.EmailAlreadyRegistered, "Email is already in use.");
                }

                string code = _verificationCodeHelper.GenerateAndStoreCode(newEmail, CodeType.EmailVerification);
                await _notificationSender.SendAccountVerificationEmailAsync(newEmail, code);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.VerificationCodeSent,
                    true,
                    "Verification code sent successfully."
                );
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[PROFILE] Validation error sending email to '{newEmail}': {valEx.Message}");
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error sending email verification: '{commEx}'.");
                responseInfo = new ResponseInfo<object>(MessageCode.ConnectionFailed, false, "Connection error.");
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Verification code sending failed for '{newEmail}'. Data Store unavailable.", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    "Service unavailable. Please try again later."
                );
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Verification code sending timeout for '{newEmail}'.");
                responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    "Request timed out."
                );
            }
            catch (Exception ex) when (ex.Message == "Data_Conflict")
            {
                Logger.Warn($"[DATA] Constraint violation sending verification code for '{newEmail}'. Possible " +
                    $"duplicate email.");
                responseInfo = new ResponseInfo<object>(
                    MessageCode.ProfileUpdateFailed,
                    false,
                    "Update failed. The information might conflict with another account."
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error sending verification code for '{newEmail}'", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.ProfileUpdateFailed,
                    false,
                    "An internal error occurred."
                );
            }

            try
            {
                if (_callback != null && responseInfo != null)
                {
                    ResponseHelper.SendResponse(_callback.EmailChangeVerificationCodeSentResponse, responseInfo);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Error($"[WCF-FATAL] Failed to send verification code to '{newEmail}'.", sendEx);
            }
        }

        public async void UpdateProfileData(ProfileData data)
        {
            string userNickname = data?.Nickname ?? "Unknown";
            ResponseInfo<ProfileData> responseInfo = null;

            try
            {
                ProfileEditValidator.ValidateProfileFormats(data);

                var currentPlayer = await _playerRepository.GetPlayerProfileByNicknameAsync(userNickname);
                if (currentPlayer.idPlayer == 0)
                {
                    throw new ValidationException(MessageCode.PlayerNotFound, "Player not found.");
                }

                var currentAccount = currentPlayer.Account.FirstOrDefault();
                string currentEmail = currentAccount?.email ?? string.Empty;
                bool emailChanged = !string.Equals(currentEmail, data.Email, StringComparison.OrdinalIgnoreCase);
                if (emailChanged)
                {
                    if (string.IsNullOrWhiteSpace(data.VerificationCode))
                    {
                        throw new ValidationException(MessageCode.VerificationCodeInvalid, "Verification code is required " +
                            "to change email.");
                    }

                    bool isCodeValid = _verificationCodeHelper.ValidateCode(new CodeValidationRequest
                    {
                        Identifier = data.Email,
                        Code = data.VerificationCode,
                        CodeType = (int)CodeType.EmailVerification
                    });

                    if (!isCodeValid)
                    {
                        throw new ValidationException(MessageCode.VerificationCodeInvalid, "The provided verification code is " +
                            "invalid or has expired.");
                    }

                    var duplicateAccount = await _playerRepository.GetPlayerProfileByNicknameAsync(data.Nickname);
                    if (duplicateAccount.idPlayer > 0)
                    {
                        throw new ValidationException(MessageCode.EmailAlreadyRegistered, "Email is already in use.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(data.Password))
                {
                    string currentHash = currentPlayer.Account.FirstOrDefault()?.password;
                    if (!string.IsNullOrEmpty(currentHash) && PasswordHelper.VerifyPassword(data.Password, currentHash))
                    {
                        throw new ValidationException(MessageCode.SamePassword, "New password cannot be the " +
                            "same as the old one.");
                    }
                }

                await _playerRepository.UpdatePlayerProfileAsync(data);

                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileUpdated,
                    true,
                    "Profile updated successfully."
                );
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[PROFILE] Validation error updating '{userNickname}': {valEx.Message}");
                responseInfo = new ResponseInfo<ProfileData>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error with during profile update '{commEx}'.");
                responseInfo = new ResponseInfo<ProfileData>(MessageCode.ConnectionFailed, false, "Connection error.");
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Profile update failed for '{userNickname}'. Data Store unavailable.", ex);
                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.DatabaseError,
                    false,
                    "Service unavailable. Please try again later."
                );
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Profile update timeout for '{userNickname}'.");
                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.Timeout,
                    false,
                    "Request timed out."
                );
            }
            catch (Exception ex) when (ex.Message == "Data_Conflict")
            {
                Logger.Warn($"[DATA] Constraint violation updating profile for '{userNickname}'. Possible " +
                    $"duplicate email.");
                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileUpdateFailed,
                    false,
                    "Update failed. The information might conflict with another account."
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error updating profile for '{userNickname}'", ex);
                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileUpdateFailed,
                    false,
                    "An internal error occurred."
                );
            }

            try
            {
                if (_callback != null && responseInfo != null)
                {
                    ResponseHelper.SendResponse(_callback.ProfileUpdateResponse, responseInfo);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Error($"[WCF-FATAL] Failed to send profile update response to '{userNickname}'.", sendEx);
            }
        }
    }
}