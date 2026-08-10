using FarmStay.Application.BackgroundJobs;
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Auth;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Auth;
using FarmStay.Domain.Entities;
using FarmStay.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace FarmStay.Application.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserMembershipRepository _userMembershipRepository;
        private readonly IFarmHouseRepository _farmHouseRepository;


        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IEmailQueue _emailQueue;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IWhatsAppQueue _whatsAppQueue;
        private readonly IRoleRepository _roleRepository;

        private readonly IPasswordService _passwordService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IUserMembershipRepository userMembershipRepository,
            IFarmHouseRepository farmHouseRepository,
            IJwtService jwtService,
            IEmailService emailService,
            IEmailQueue emailQueue,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IUserOtpRepository userOtpRepository,
            IWhatsAppService whatsAppService,
            IPasswordService passwordService,
            ILogger<UserService> logger,
            IWhatsAppQueue whatsAppQueue,
            IRoleRepository roleRepository
            )
        {
            _userRepository = userRepository;
            _userMembershipRepository = userMembershipRepository;
            _farmHouseRepository = farmHouseRepository;

            _jwtService = jwtService;
            _emailService = emailService;
            _emailQueue = emailQueue;

            _logger = logger;

            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _userOtpRepository = userOtpRepository;
            _whatsAppService = whatsAppService;
            _passwordService = passwordService;
            _whatsAppQueue = whatsAppQueue;
            _roleRepository = roleRepository;
        }

        //public async Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Customer registration request received for Email: {Email}", dto.Email);

        //        // Step 1
        //        // Validate FarmHouse from Header

        //        var farmHouseIdHeader = _httpContextAccessor.HttpContext?
        //            .Request
        //            .Headers["FarmHouseId"]
        //            .FirstOrDefault();

        //        if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
        //        {
        //            _logger.LogWarning("FarmHouseId header is missing or invalid.");

        //            return new ApiResponse<RegisterResponseDto>
        //            {
        //                Success = false,
        //                Message = "Invalid FarmHouse."
        //            };
        //        }

        //        var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

        //        if (farmHouse == null)
        //        {
        //            _logger.LogWarning("Invalid FarmHouse. FarmHouseId: {FarmHouseId}", farmHouseId);

        //            return new ApiResponse<RegisterResponseDto>
        //            {
        //                Success = false,
        //                Message = "Invalid FarmHouse."
        //            };
        //        }

        //        _logger.LogInformation("FarmHouse validated successfully. FarmHouseId: {FarmHouseId}", farmHouseId);

        //        // Step 2
        //        // Check Existing Email

        //        var emailUser = await _userRepository.GetByEmailAsync(dto.Email.Trim().ToLower(), farmHouseId);

        //        var mobileUser = await _userRepository.GetByMobileNumberAsync(dto.MobileNumber.Trim(), farmHouseId);

        //        // Both belong to the same user

        //        // Both Email and Mobile exist

        //        if (emailUser != null && mobileUser != null)
        //        {
        //            // Both belong to the same user

        //            if (emailUser.UserId == mobileUser.UserId)
        //            {
        //                _logger.LogInformation(
        //                    "Existing user found with both email and mobile. UserId: {UserId}",
        //                    emailUser.UserId);

        //                // Step 3
        //                // Check Membership

        //                var membership = await _userMembershipRepository
        //                    .GetMembershipAsync(emailUser.UserId, farmHouseId);

        //                if (membership != null)
        //                {
        //                    _logger.LogWarning(
        //                        "User is already registered in FarmHouse. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
        //                        emailUser.UserId,
        //                        farmHouseId);

        //                    return new ApiResponse<RegisterResponseDto>
        //                    {
        //                        Success = false,
        //                        Message = "User is already registered in this FarmHouse."
        //                    };
        //                }
        //                else
        //                {
        //                    _logger.LogInformation(
        //                        "Existing user found but membership not found. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
        //                        emailUser.UserId,
        //                        farmHouseId);

        //                    return new ApiResponse<RegisterResponseDto>
        //                    {
        //                        Success = true,
        //                        Message = "Membership not found."
        //                    };
        //                }
        //            }
        //            else
        //            {
        //                _logger.LogWarning(
        //                    "Email and mobile belong to different users. Email: {Email}, Mobile: {MobileNumber}",
        //                    dto.Email,
        //                    dto.MobileNumber);

        //                return new ApiResponse<RegisterResponseDto>
        //                {
        //                    Success = false,
        //                    Message = "Email and mobile number belong to different accounts."
        //                };
        //            }
        //        }

        //        if (emailUser != null)
        //        {
        //            _logger.LogWarning("Email already registered. Email: {Email}", dto.Email);

        //            return new ApiResponse<RegisterResponseDto>
        //            {
        //                Success = false,
        //                Message = "Email is already registered. Please use a different email."
        //            };
        //        }

        //        // Step 3
        //        // Check Existing Mobile Number



        //        if (mobileUser != null)
        //        {
        //            _logger.LogWarning("Mobile number already registered. Mobile: {MobileNumber}", dto.MobileNumber);

        //            return new ApiResponse<RegisterResponseDto>
        //            {
        //                Success = false,
        //                Message = "Mobile number is already registered. Please use a different mobile number."
        //            };
        //        }

        //        // Step 4
        //        // Create New Customer

        //        _logger.LogInformation("Creating new customer for Email: {Email}", dto.Email);

        //        var user = new User
        //        {
        //            FullName = dto.FullName.Trim(),
        //            Email = dto.Email.Trim().ToLower(),
        //            MobileNumber = dto.MobileNumber.Trim(),

        //            PasswordHash = _passwordService.HashPassword(dto.Password),

        //            IsEmailVerified = false,
        //            IsMobileVerified = false,
        //            IsActive = true,
        //            IsDeleted = false,

        //            CreatedDate = DateTime.UtcNow,
        //            FarmHouseId = farmHouse.FarmHouseId
        //        };

        //        await _userRepository.AddAsync(user);
        //        await _unitOfWork.SaveChangesAsync();

        //        _logger.LogInformation(
        //            "New customer created successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
        //            user.UserId,
        //            farmHouse.FarmHouseId);

        //        return new ApiResponse<RegisterResponseDto>
        //        {
        //            Success = true,
        //            Message = "Customer registered successfully.",
        //            Data = new RegisterResponseDto
        //            {
        //                UserId = user.UserId,
        //                FullName = user.FullName,
        //                Email = user.Email,
        //                MobileNumber = user.MobileNumber,
        //                FarmHouseId = user.FarmHouseId,
        //                IsEmailVerificationSent = false,
        //                IsMobileOtpSent = false
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "An error occurred while registering customer for Email: {Email}",
        //            dto.Email);

        //        return new ApiResponse<RegisterResponseDto>
        //        {
        //            Success = false,
        //            Message = "An unexpected error occurred while registering the customer."
        //        };
        //    }
        //}


        public async Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                "Customer registration request received for Email: {Email}",
                dto.Email);

                // Step 1
                // Validate FarmHouse from Header

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning("FarmHouseId header is missing or invalid.");

                    return new ApiResponse<RegisterResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<RegisterResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                _logger.LogInformation(
                    "FarmHouse validated successfully. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                // Step 2
                // Check Existing Email and Mobile

                var email = dto.Email.Trim().ToLower();
                var mobile = dto.MobileNumber.Trim();

                var emailUser = await _userRepository.GetByEmailAsync(email, farmHouseId);

                var mobileUser = await _userRepository.GetByMobileNumberAsync(mobile, farmHouseId);

                // Step 3
                // Both Email and Mobile Exist

                if (emailUser != null && mobileUser != null)
                {
                    // Same User

                    if (emailUser.UserId == mobileUser.UserId)
                    {
                        _logger.LogInformation(
                            "Existing user found with both email and mobile. UserId: {UserId}",
                            emailUser.UserId);

                        // Check Membership

                        var membership = await _userMembershipRepository.GetMembershipAsync(
                            emailUser.UserId,
                            farmHouseId);

                        if (membership != null)
                        {
                            _logger.LogWarning(
                                "User is already registered in FarmHouse. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                                emailUser.UserId,
                                farmHouseId);

                            return new ApiResponse<RegisterResponseDto>
                            {
                                Success = false,
                                Message = "User is already registered in this FarmHouse."
                            };
                        }

                        _logger.LogInformation(
                            "Membership not found. Starting verification flow. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                            emailUser.UserId,
                            farmHouseId);

                        await StartVerificationFlowAsync(emailUser, farmHouse);

                        return new ApiResponse<RegisterResponseDto>
                        {
                            Success = true,
                            Message = "Verification initiated. Please verify your email and mobile number.",
                            Data = new RegisterResponseDto
                            {
                                UserId = emailUser.UserId,
                                FullName = emailUser.FullName,
                                Email = emailUser.Email,
                                MobileNumber = emailUser.MobileNumber,
                                FarmHouseId = emailUser.FarmHouseId,
                                IsEmailVerificationSent = true,
                                IsMobileOtpSent = true
                            }
                        };
                    }

                    // Different Users

                    _logger.LogWarning(
                        "Email and mobile belong to different users. Email: {Email}, Mobile: {MobileNumber}",
                        email,
                        mobile);

                    return new ApiResponse<RegisterResponseDto>
                    {
                        Success = false,
                        Message = "Email and mobile number belong to different accounts."
                    };
                }

                // Step 4
                // Email Already Exists

                if (emailUser != null)
                {
                    _logger.LogWarning(
                        "Email already registered. Email: {Email}",
                        email);

                    return new ApiResponse<RegisterResponseDto>
                    {
                        Success = false,
                        Message = "Email is already registered. Please use a different email."
                    };
                }

                // Step 5
                // Mobile Already Exists

                if (mobileUser != null)
                {
                    _logger.LogWarning(
                        "Mobile number already registered. Mobile: {MobileNumber}",
                        mobile);

                    return new ApiResponse<RegisterResponseDto>
                    {
                        Success = false,
                        Message = "Mobile number is already registered. Please use a different mobile number."
                    };
                }

                // Step 6
                // Create New Customer

                _logger.LogInformation(
                    "Creating new customer for Email: {Email}",
                    email);

                var user = new User
                {
                    FullName = dto.FullName.Trim(),
                    Email = email,
                    MobileNumber = mobile,

                    PasswordHash = _passwordService.HashPassword(dto.Password),

                    IsEmailVerified = false,
                    IsMobileVerified = false,

                    IsActive = true,
                    IsDeleted = false,

                    CreatedDate = DateTime.UtcNow,
                    FarmHouseId = farmHouse.FarmHouseId
                };

                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "New customer created successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    farmHouse.FarmHouseId);

                // Step 7
                // Start Verification Flow

                await StartVerificationFlowAsync(user, farmHouse);

                _logger.LogInformation(
                    "Verification flow initiated successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    farmHouse.FarmHouseId);

                return new ApiResponse<RegisterResponseDto>
                {
                    Success = true,
                    Message = "Registration successful. Please verify your email and mobile number.",
                    Data = new RegisterResponseDto
                    {
                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        MobileNumber = user.MobileNumber,
                        FarmHouseId = user.FarmHouseId,
                        IsEmailVerificationSent = true,
                        IsMobileOtpSent = true
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while registering customer for Email: {Email}",
                    dto.Email);

                return new ApiResponse<RegisterResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while registering the customer."
                };
            }

        }

        public async Task<ApiResponse<bool>> VerifyEmailAsync(VerifyEmailRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                "Email verification request received. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                dto.UserId,
                dto.FarmHouseId);

                // Step 1
                // Validate FarmHouse

                var farmHouse = await _farmHouseRepository.GetByIdAsync(dto.FarmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse during email verification. FarmHouseId: {FarmHouseId}",
                        dto.FarmHouseId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 2
                // Find User By FarmHouseId + UserId + Token

                var user = await _userRepository.GetByEmailVerificationTokenAsync(
                    dto.UserId,
                    dto.FarmHouseId,
                    dto.Token);

                if (user == null)
                {
                    _logger.LogWarning(
                        "Invalid email verification token. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        dto.UserId,
                        dto.FarmHouseId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid verification link."
                    };
                }

                // Step 3
                // Check Token Expiry

                if (!user.VerificationTokenExpiry.HasValue || user.VerificationTokenExpiry.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning(
                        "Email verification token expired. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Verification link has expired."
                    };
                }

                // Step 4
                // Verify Email and Invalidate Token

                user.IsEmailVerified = true;

                user.EmailVerificationToken = null;

                user.VerificationTokenExpiry = null;

                user.ModifiedDate = DateTime.UtcNow;

                user.ModifiedBy = user.UserId;

                await _userRepository.UpdateAsync(user);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Email verified successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    user.FarmHouseId);

                // Step 5
                // If Mobile Already Verified, Create Membership

                if (user.IsMobileVerified)
                {
                    await CreateCustomerMembershipAsync(user, farmHouse);

                    _logger.LogInformation(
                        "Customer membership created after email verification. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        user.UserId,
                        farmHouse.FarmHouseId);
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Email verified successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while verifying email. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    dto.UserId,
                    dto.FarmHouseId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while verifying email."
                };
            }

        }


        public async Task<ApiResponse<bool>> VerifyOtpAsync(VerifyOtpRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                "OTP verification request received. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                dto.UserId,
                dto.FarmHouseId);

                // Step 1
                // Validate FarmHouse

                var farmHouse = await _farmHouseRepository.GetByIdAsync(dto.FarmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 2
                // Validate User

                var user = await _userRepository.GetByIdAsync(dto.UserId);

                if (user == null || user.FarmHouseId != dto.FarmHouseId)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Step 3
                // Get Active OTP

                var otp = await _userOtpRepository.GetByOtpAsync(
                    dto.UserId,
                    dto.OtpCode,
                    OtpPurpose.Register);

                if (otp == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid OTP."
                    };
                }

                // Step 4
                // Check OTP Expiry

                if (otp.ExpiryDate < DateTime.UtcNow)
                {
                    otp.IsActive = false;
                    otp.ModifiedDate = DateTime.UtcNow;
                    otp.ModifiedBy = user.UserId;

                    await _userOtpRepository.UpdateAsync(otp);
                    await _unitOfWork.SaveChangesAsync();

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "OTP has expired."
                    };
                }

                // Step 5
                // Verify Mobile

                otp.IsUsed = true;
                otp.IsActive = false;
                otp.ModifiedDate = DateTime.UtcNow;
                otp.ModifiedBy = user.UserId;

                await _userOtpRepository.UpdateAsync(otp);

                user.IsMobileVerified = true;
                user.ModifiedDate = DateTime.UtcNow;
                user.ModifiedBy = user.UserId;

                await _userRepository.UpdateAsync(user);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Mobile verified successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    user.FarmHouseId);

                // Step 6
                // If Email Already Verified, Create Membership

                if (user.IsEmailVerified)
                {
                    await CreateCustomerMembershipAsync(user, farmHouse);

                    _logger.LogInformation(
                        "Customer membership created after mobile verification. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        user.UserId,
                        farmHouse.FarmHouseId);
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Mobile number verified successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while verifying OTP. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    dto.UserId,
                    dto.FarmHouseId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while verifying OTP."
                };
            }

        }

        private async Task StartVerificationFlowAsync(User user, FarmHouse farmHouse)
        {
            try
            {
                _logger.LogInformation(
                "Starting verification flow. UserId: {UserId}, FarmHouseId: {FarmHouseId}", user.UserId,farmHouse.FarmHouseId);

                // Step 1
                // Generate Email Verification Token

                var emailVerificationToken = Guid.NewGuid().ToString("N");

                var tokenExpiry = DateTime.UtcNow.AddHours(24);

                _logger.LogInformation(
                    "Email verification token generated. UserId: {UserId}, Expiry: {Expiry}",
                    user.UserId,
                    tokenExpiry);

                // Step 2
                // Save Email Verification Token

                user.EmailVerificationToken = emailVerificationToken;

                user.VerificationTokenExpiry = tokenExpiry;

                user.ModifiedDate = DateTime.UtcNow;

                user.ModifiedBy = user.UserId;

                await _userRepository.UpdateAsync(user);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Email verification token saved successfully. UserId: {UserId}",
                    user.UserId);

                // Step 3
                // Generate Mobile OTP

                var otpCode = RandomNumberGenerator
                    .GetInt32(100000, 1000000)
                    .ToString();

                var otpExpiry = DateTime.UtcNow.AddMinutes(5);

                _logger.LogInformation(
                    "Mobile verification OTP generated. UserId: {UserId}, Expiry: {Expiry}",
                    user.UserId,
                    otpExpiry);

                // Step 4
                // Invalidate Existing Active OTP

                var existingOtp = await _userOtpRepository.GetActiveOtpAsync(user.UserId,OtpPurpose.Register);

                if (existingOtp != null)
                {
                    existingOtp.IsActive = false;

                    existingOtp.ModifiedDate = DateTime.UtcNow;
                    existingOtp.ModifiedBy = user.UserId;

                    await _userOtpRepository.UpdateAsync(existingOtp);

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(
                        "Existing active OTP invalidated. UserId: {UserId}",
                        user.UserId);
                }
                else
                {
                    _logger.LogInformation(
                        "No active OTP found to invalidate. UserId: {UserId}",
                        user.UserId);
                }

                // Step 5
                // Save New OTP

                var userOtp = new UserOtp
                {
                    UserId = user.UserId,
                    MobileNumber = user.MobileNumber,
                    OtpCode = otpCode,
                    Purpose = OtpPurpose.Register,
                    ExpiryDate = otpExpiry,
                    IsUsed = false,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = user.UserId
                };

                await _userOtpRepository.AddAsync(userOtp);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "New registration OTP saved successfully. UserId: {UserId}",
                    user.UserId);

                // Step 6
                // Build Email Verification Link and Queue Email

                var verificationLink =
                                    $"https://localhost:7081/api/Auth/verify-email?farmHouseId={farmHouse.FarmHouseId}&userId={user.UserId}&token={emailVerificationToken}";
                var emailSubject = "Verify Your FarmStay Account";

                var emailBody = $@"
                                <html>
                                <body>
                                    <h2>Welcome to {farmHouse.FarmHouseName}</h2>

                                    <p>Please verify your email address by clicking the button below:</p>

                                    <p>
                                        <a href=""{verificationLink}""
                                           style=""
                                               background-color:#16a34a;
                                               color:white;
                                               padding:12px 20px;
                                               text-decoration:none;
                                               border-radius:6px;
                                               display:inline-block;
                                           "">
                                            Verify Email
                                        </a>
                                    </p>

                                    <p>This link will expire in 24 hours.</p>

                                    <p>If you did not create this account, please ignore this email.</p>

                                    <br/>
                                    <p>Thanks,<br/>{farmHouse.FarmHouseName}</p>
                                </body>
                                </html>";

                _emailQueue.Enqueue(new EmailJob
                {
                    To = user.Email,
                    Subject = emailSubject,
                    HtmlBody = emailBody,
                    IsHtml = true
                });

                _logger.LogInformation(
                    "Email verification link queued successfully. UserId: {UserId}",
                    user.UserId);

                // Step 7
                // Queue WhatsApp OTP

                var whatsAppMessage =
                    $"Your FarmStay verification OTP is {otpCode}. It is valid for 5 minutes.";

                _whatsAppQueue.Enqueue(new WhatsAppJob
                {
                    MobileNumber = user.MobileNumber,
                    Message = whatsAppMessage
                });

                _logger.LogInformation(
                    "WhatsApp OTP queued successfully. UserId: {UserId}, Mobile: {MobileNumber}",
                    user.UserId,
                    user.MobileNumber);

                _logger.LogInformation(
                    "Verification flow initiated successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    farmHouse.FarmHouseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while starting verification flow. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    farmHouse.FarmHouseId);

                throw;
            }
        }

        private async Task CreateCustomerMembershipAsync(User user, FarmHouse farmHouse)
        {
            try
            {
                _logger.LogInformation(
                "Creating customer membership. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                user.UserId,
                farmHouse.FarmHouseId);

                // Step 1
                // Check Existing Membership

                var existingMembership = await _userMembershipRepository.GetMembershipAsync(
                    user.UserId,
                    farmHouse.FarmHouseId);

                if (existingMembership != null)
                {
                    _logger.LogInformation(
                        "Customer membership already exists. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        user.UserId,
                        farmHouse.FarmHouseId);

                    return;
                }

                // Step 2
                // Get Customer Role

                var customerRole = await _roleRepository.GetByRoleNameAsync("Customer");

                if (customerRole == null)
                {
                    _logger.LogError(
                        "Customer role not found. FarmHouseId: {FarmHouseId}",
                        farmHouse.FarmHouseId);

                    throw new InvalidOperationException("Customer role not found.");
                }

                // Step 3
                // Create Membership

                var membership = new UserMembership
                {
                    UserId = user.UserId,
                    FarmHouseId = farmHouse.FarmHouseId,
                    RoleId = customerRole.RoleId,

                    IsActive = true,
                    IsDeleted = false,

                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = user.UserId
                };

                await _userMembershipRepository.AddAsync(membership);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Customer membership created successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}, MembershipId: {MembershipId}",
                    user.UserId,
                    farmHouse.FarmHouseId,
                    membership.UserMembershipId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while creating customer membership. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId, farmHouse.FarmHouseId);

                throw;
            }
        }

    }

}
