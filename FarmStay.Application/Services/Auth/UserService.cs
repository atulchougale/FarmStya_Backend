using FarmStay.Application.BackgroundJobs;
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Common.Constants;
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

        private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;

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
            IRoleRepository roleRepository,
            IUserRefreshTokenRepository userRefreshTokenRepository
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
            _userRefreshTokenRepository = userRefreshTokenRepository;
        }

        


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
                                IsMobileOtpSent = true,
                                IsEmailVerified = emailUser.IsEmailVerified,
                                IsMobileVerified = emailUser.IsMobileVerified,

                                Message = "Please verify your email and mobile number."
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
                        IsMobileOtpSent = true,
                        IsEmailVerified = user.IsEmailVerified,
                        IsMobileVerified = user.IsMobileVerified,
                        Message = "Please verify your email and mobile number."
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

        public async Task<ApiResponse<VerificationStatusResponseDto>> VerifyEmailAsync( VerifyEmailRequestDto dto)
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

                    return new ApiResponse<VerificationStatusResponseDto>
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

                    return new ApiResponse<VerificationStatusResponseDto>
                    {
                        Success = false,
                        Message = "Invalid verification link."
                    };
                }

                // Step 3
                // Check Token Expiry

                if (!user.VerificationTokenExpiry.HasValue ||
                    user.VerificationTokenExpiry.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning(
                        "Email verification token expired. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<VerificationStatusResponseDto>
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

                // Step 6
                // Return Current Verification Status

                return new ApiResponse<VerificationStatusResponseDto>
                {
                    Success = true,
                    Message = "Email verified successfully.",
                    Data = new VerificationStatusResponseDto
                    {
                        IsEmailVerified = user.IsEmailVerified,
                        IsMobileVerified = user.IsMobileVerified
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while verifying email. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    dto.UserId,
                    dto.FarmHouseId);

                return new ApiResponse<VerificationStatusResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while verifying email."
                };
            }
        }

        public async Task<ApiResponse<VerificationStatusResponseDto>> VerifyOtpAsync( VerifyOtpRequestDto dto)
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
                    _logger.LogWarning(
                        "Invalid FarmHouse during OTP verification. FarmHouseId: {FarmHouseId}",
                        dto.FarmHouseId);

                    return new ApiResponse<VerificationStatusResponseDto>
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
                    _logger.LogWarning(
                        "User not found or does not belong to FarmHouse. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        dto.UserId,
                        dto.FarmHouseId);

                    return new ApiResponse<VerificationStatusResponseDto>
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Step 3
                // Check whether mobile is already verified

                if (user.IsMobileVerified)
                {
                    _logger.LogWarning(
                        "OTP verification requested for already verified mobile. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<VerificationStatusResponseDto>
                    {
                        Success = false,
                        Message = "Mobile number is already verified.",
                        Data = new VerificationStatusResponseDto
                        {
                            IsEmailVerified = user.IsEmailVerified,
                            IsMobileVerified = user.IsMobileVerified
                        }
                    };
                }

                // Step 4
                // Get Active OTP

                var otp = await _userOtpRepository.GetByOtpAsync(
                    dto.UserId,
                    dto.OtpCode,
                    OtpPurpose.Register);

                if (otp == null)
                {
                    _logger.LogWarning(
                        "Invalid OTP. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        dto.UserId,
                        dto.FarmHouseId);

                    return new ApiResponse<VerificationStatusResponseDto>
                    {
                        Success = false,
                        Message = "Invalid OTP."
                    };
                }

                // Step 5
                // Check OTP Expiry

                if (otp.ExpiryDate < DateTime.UtcNow)
                {
                    otp.IsActive = false;
                    otp.ModifiedDate = DateTime.UtcNow;
                    otp.ModifiedBy = user.UserId;

                    await _userOtpRepository.UpdateAsync(otp);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogWarning(
                        "OTP expired. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<VerificationStatusResponseDto>
                    {
                        Success = false,
                        Message = "OTP has expired."
                    };
                }

                // Step 6
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

                // Step 7
                // If Email Already Verified, Create Membership

                if (user.IsEmailVerified)
                {
                    await CreateCustomerMembershipAsync(user, farmHouse);

                    _logger.LogInformation(
                        "Customer membership created after mobile verification. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        user.UserId,
                        farmHouse.FarmHouseId);
                }

                // Step 8
                // Return Current Verification Status

                return new ApiResponse<VerificationStatusResponseDto>
                {
                    Success = true,
                    Message = "Mobile number verified successfully.",
                    Data = new VerificationStatusResponseDto
                    {
                        IsEmailVerified = user.IsEmailVerified,
                        IsMobileVerified = user.IsMobileVerified
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while verifying OTP. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    dto.UserId,
                    dto.FarmHouseId);

                return new ApiResponse<VerificationStatusResponseDto>
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

                //var verificationLink = $"https://localhost:7081/api/Auth/verify-email?farmHouseId={farmHouse.FarmHouseId}&userId={user.UserId}&token={emailVerificationToken}";

                var verificationLink = $"http://localhost:4200/verify-email?farmHouseId={farmHouse.FarmHouseId}&userId={user.UserId}&token={emailVerificationToken}";

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


        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Login request received for Email: {Email}",
                    dto.Email);

                // ============================================================
                // Step 1
                // Validate FarmHouse from Header
                // ============================================================

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                _logger.LogInformation(
                    "FarmHouse validated successfully. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                // ============================================================
                // Step 2
                // Find User
                // ============================================================

                var email = dto.Email.Trim().ToLower();

                var user = await _userRepository
                    .GetByEmailAsync(email, farmHouseId);

                if (user == null)
                {
                    _logger.LogWarning(
                        "Login failed. User not found. Email: {Email}, FarmHouseId: {FarmHouseId}",
                        email,
                        farmHouseId);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }

                // ============================================================
                // Step 3
                // Check User Status
                // ============================================================

                if (!user.IsActive || user.IsDeleted)
                {
                    _logger.LogWarning(
                        "Inactive or deleted user login attempt. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Your account is inactive."
                    };
                }

                // ============================================================
                // Step 4
                // Verify Password
                // ============================================================

                var passwordValid = _passwordService.VerifyPassword(
                    dto.Password,
                    user.PasswordHash);

                if (!passwordValid)
                {
                    _logger.LogWarning(
                        "Invalid password. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }

                // ============================================================
                // Step 5
                // Check Membership
                // ============================================================

                var membership = await _userMembershipRepository
                    .GetMembershipAsync(
                        user.UserId,
                        farmHouseId);

                // ------------------------------------------------------------
                // Membership Not Found
                // Verification is still pending
                // ------------------------------------------------------------

                if (membership == null)
                {
                    _logger.LogInformation(
                        "Membership not found. Starting verification flow. " +
                        "UserId: {UserId}, FarmHouseId: {FarmHouseId}, " +
                        "IsEmailVerified: {IsEmailVerified}, IsMobileVerified: {IsMobileVerified}",
                        user.UserId,
                        farmHouseId,
                        user.IsEmailVerified,
                        user.IsMobileVerified);

                    // Send new Email verification link + Mobile OTP
                    await StartVerificationFlowAsync(user, farmHouse);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Please verify your email and mobile number to activate your account.",
                        Data = new LoginResponseDto
                        {
                            UserId = user.UserId,
                            FullName = user.FullName,
                            Email = user.Email,
                            MobileNumber = user.MobileNumber,

                            FarmHouseId = farmHouse.FarmHouseId,
                            FarmHouseName = farmHouse.FarmHouseName,

                            IsEmailVerified = user.IsEmailVerified,
                            IsMobileVerified = user.IsMobileVerified,
                            RequiresVerification = true
                        }
                    };
                }

                // ============================================================
                // Step 6
                // Get Role
                // ============================================================

                var role = await _roleRepository
                    .GetByIdAsync(membership.RoleId);

                if (role == null)
                {
                    _logger.LogError(
                        "Role not found. RoleId: {RoleId}, UserId: {UserId}",
                        membership.RoleId,
                        user.UserId);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Unable to login at the moment."
                    };
                }

                // ============================================================
                // Step 7
                // Generate Tokens
                // ============================================================

                var accessToken = _jwtService.GenerateAccessToken(
                    user,
                    membership,
                    farmHouse,
                    role);

                var refreshToken = _jwtService.GenerateRefreshToken();

                // ============================================================
                // Step 8
                // Save Refresh Token
                // ============================================================

                var refreshTokenEntity = new UserRefreshToken
                {
                    UserId = user.UserId,

                    RefreshTokenHash =
                        _passwordService.HashPassword(refreshToken),

                    ExpiryDate = DateTime.UtcNow.AddDays(30),

                    IsRevoked = false,
                    IsActive = true,
                    IsDeleted = false,

                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = user.UserId
                };

                await _userRefreshTokenRepository
                    .AddAsync(refreshTokenEntity);

                // ============================================================
                // Step 9
                // Update Last Login
                // ============================================================

                membership.LastLoginDate = DateTime.UtcNow;

                await _userMembershipRepository
                    .UpdateAsync(membership);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Login successful. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    farmHouseId);

                // ============================================================
                // Final Successful Login Response
                // ============================================================

                return new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Login successful.",
                    Data = new LoginResponseDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,

                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        MobileNumber = user.MobileNumber,

                        FarmHouseId = farmHouse.FarmHouseId,
                        FarmHouseName = farmHouse.FarmHouseName,

                        RoleId = role.RoleId,
                        RoleName = role.RoleName,

                        IsOwner = role.RoleName == "FarmOwner",

                        IsEmailVerified = user.IsEmailVerified,
                        IsMobileVerified = user.IsMobileVerified
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while logging in user with Email: {Email}",
                    dto.Email);

                return new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while logging in."
                };
            }
        }


        // Forgot password 
        //public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto dto)
        //{
        //    try
        //    {
        //        _logger.LogInformation(
        //            "Forgot password request received. Method: {Method}",
        //            dto.Method);

        //        // Step 1
        //        // Validate FarmHouse from Header

        //        var farmHouseIdHeader = _httpContextAccessor.HttpContext?
        //            .Request
        //            .Headers["FarmHouseId"]
        //            .FirstOrDefault();

        //        if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
        //        {
        //            _logger.LogWarning(
        //                "FarmHouseId header is missing or invalid.");

        //            return new ApiResponse<bool>
        //            {
        //                Success = false,
        //                Message = "Invalid FarmHouse."
        //            };
        //        }

        //        var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

        //        if (farmHouse == null)
        //        {
        //            _logger.LogWarning(
        //                "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
        //                farmHouseId);

        //            return new ApiResponse<bool>
        //            {
        //                Success = false,
        //                Message = "Invalid FarmHouse."
        //            };
        //        }

        //        // Step 2
        //        // Email Reset Flow

        //        if (dto.Method == ForgotPasswordMethod.Email)
        //        {
        //            var email = dto.Email.Trim().ToLower();

        //            var user = await _userRepository.GetByEmailAsync(
        //                email,
        //                farmHouseId);

        //            if (user == null)
        //            {
        //                _logger.LogWarning(
        //                    "Forgot password email not found. Email: {Email}, FarmHouseId: {FarmHouseId}",
        //                    email,
        //                    farmHouseId);

        //                return new ApiResponse<bool>
        //                {
        //                    Success = true,
        //                    Message = "If the account exists, password reset instructions have been sent.",
        //                    Data = true
        //                };
        //            }

        //            // Generate Password Reset Token

        //            var resetToken = Guid.NewGuid().ToString("N");

        //            var resetTokenExpiry = DateTime.UtcNow.AddHours(1);

        //            user.PasswordResetToken = resetToken;
        //            user.PasswordResetTokenExpiry = resetTokenExpiry;

        //            user.ModifiedDate = DateTime.UtcNow;
        //            user.ModifiedBy = user.UserId;

        //            await _userRepository.UpdateAsync(user);
        //            await _unitOfWork.SaveChangesAsync();

        //            // Build Reset Password Link

        //            var resetLink =
        //                $"https://localhost:7081/api/Auth/reset-password-email" +
        //                $"?farmHouseId={farmHouseId}" +
        //                $"&userId={user.UserId}" +
        //                $"&token={resetToken}";

        //            var emailSubject = "Reset Your FarmStay Password";

        //            var emailBody = $@"
        //        <html>
        //        <body>
        //            <h2>Password Reset</h2>

        //            <p>Hello {user.FullName},</p>

        //            <p>
        //                We received a request to reset your FarmStay password.
        //            </p>

        //            <p>
        //                <a href=""{resetLink}""
        //                   style=""
        //                       background-color:#16a34a;
        //                       color:white;
        //                       padding:12px 20px;
        //                       text-decoration:none;
        //                       border-radius:6px;
        //                       display:inline-block;
        //                   "">
        //                    Reset Password
        //                </a>
        //            </p>

        //            <p>This link will expire in 1 hour.</p>

        //            <p>
        //                If you did not request a password reset,
        //                please ignore this email.
        //            </p>

        //            <br/>
        //            <p>
        //                Thanks,<br/>
        //                {farmHouse.FarmHouseName}
        //            </p>
        //        </body>
        //        </html>";

        //            _emailQueue.Enqueue(new EmailJob
        //            {
        //                To = user.Email,
        //                Subject = emailSubject,
        //                HtmlBody = emailBody,
        //                IsHtml = true
        //            });

        //            _logger.LogInformation(
        //                "Password reset email queued successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
        //                user.UserId,
        //                farmHouseId);

        //            return new ApiResponse<bool>
        //            {
        //                Success = true,
        //                Message = "Password reset link has been sent to your email.",
        //                Data = true
        //            };
        //        }

        //        // Step 3
        //        // WhatsApp OTP Flow

        //        if (dto.Method == ForgotPasswordMethod.WhatsAppOtp)
        //        {
        //            var mobileNumber = dto.MobileNumber.Trim();

        //            var user = await _userRepository.GetByMobileNumberAsync(
        //                mobileNumber,
        //                farmHouseId);

        //            if (user == null)
        //            {
        //                _logger.LogWarning(
        //                    "Forgot password mobile number not found. MobileNumber: {MobileNumber}, FarmHouseId: {FarmHouseId}",
        //                    mobileNumber,
        //                    farmHouseId);

        //                return new ApiResponse<bool>
        //                {
        //                    Success = true,
        //                    Message = "If the account exists, password reset instructions have been sent.",
        //                    Data = true
        //                };
        //            }

        //            // Generate OTP

        //            var otpCode = RandomNumberGenerator
        //                .GetInt32(100000, 1000000)
        //                .ToString();

        //            var otpExpiry = DateTime.UtcNow.AddMinutes(5);

        //            // Invalidate Existing Forgot Password OTP

        //            var existingOtp = await _userOtpRepository.GetActiveOtpAsync(
        //                user.UserId,
        //                OtpPurpose.ForgotPassword);

        //            if (existingOtp != null)
        //            {
        //                existingOtp.IsActive = false;
        //                existingOtp.ModifiedDate = DateTime.UtcNow;
        //                existingOtp.ModifiedBy = user.UserId;

        //                await _userOtpRepository.UpdateAsync(existingOtp);
        //            }

        //            // Save New OTP

        //            var userOtp = new UserOtp
        //            {
        //                UserId = user.UserId,
        //                MobileNumber = user.MobileNumber,
        //                OtpCode = otpCode,
        //                Purpose = OtpPurpose.ForgotPassword,
        //                ExpiryDate = otpExpiry,
        //                IsUsed = false,
        //                IsActive = true,
        //                IsDeleted = false,
        //                CreatedDate = DateTime.UtcNow,
        //                CreatedBy = user.UserId
        //            };

        //            await _userOtpRepository.AddAsync(userOtp);

        //            await _unitOfWork.SaveChangesAsync();

        //            // Queue WhatsApp OTP

        //            var whatsAppMessage =
        //                $"Your FarmStay password reset OTP is {otpCode}. It is valid for 5 minutes.";

        //            _whatsAppQueue.Enqueue(new WhatsAppJob
        //            {
        //                MobileNumber = user.MobileNumber,
        //                Message = whatsAppMessage
        //            });

        //            _logger.LogInformation(
        //                "Password reset WhatsApp OTP queued successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
        //                user.UserId,
        //                farmHouseId);

        //            return new ApiResponse<bool>
        //            {
        //                Success = true,
        //                Message = "Password reset OTP has been sent to your WhatsApp.",
        //                Data = true
        //            };
        //        }

        //        // Invalid Method

        //        _logger.LogWarning(
        //            "Invalid forgot password method. Method: {Method}",
        //            dto.Method);

        //        return new ApiResponse<bool>
        //        {
        //            Success = false,
        //            Message = "Invalid password reset method."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "An error occurred during forgot password.");

        //        return new ApiResponse<bool>
        //        {
        //            Success = false,
        //            Message = "An unexpected error occurred while processing forgot password."
        //        };
        //    }
        //}


        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Forgot password request received. Method: {Method}",
                    dto.Method);

                // ============================================================
                // Step 1
                // Validate FarmHouse from Header
                // ============================================================

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<bool>
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

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // ============================================================
                // Step 2
                // Email Reset Flow
                // ============================================================

                if (dto.Method == ForgotPasswordMethod.Email)
                {
                    var email = dto.Email.Trim().ToLower();

                    var user = await _userRepository.GetByEmailAsync(
                        email,
                        farmHouseId);

                    // --------------------------------------------------------
                    // User Not Found
                    // --------------------------------------------------------

                    if (user == null)
                    {
                        _logger.LogWarning(
                            "Forgot password email not found. Email: {Email}, FarmHouseId: {FarmHouseId}",
                            email,
                            farmHouseId);

                        // Security:
                        // Do not reveal whether the email exists or not.

                        return new ApiResponse<bool>
                        {
                            Success = true,
                            Message = "If the account exists, password reset instructions have been sent.",
                            Data = true
                        };
                    }

                    // --------------------------------------------------------
                    // Generate Password Reset Token
                    // --------------------------------------------------------

                    var resetToken = Guid.NewGuid().ToString("N");

                    var resetTokenExpiry = DateTime.UtcNow.AddHours(1);

                    user.PasswordResetToken = resetToken;
                    user.PasswordResetTokenExpiry = resetTokenExpiry;

                    user.ModifiedDate = DateTime.UtcNow;
                    user.ModifiedBy = user.UserId;

                    await _userRepository.UpdateAsync(user);

                    await _unitOfWork.SaveChangesAsync();

                    // ========================================================
                    // Build FRONTEND Reset Password Link
                    // ========================================================

                    var resetLink =
                        $"https://localhost:4200/reset-password" +
                        $"?farmHouseId={farmHouseId}" +
                        $"&userId={user.UserId}" +
                        $"&token={Uri.EscapeDataString(resetToken)}";

                    // ========================================================
                    // Email
                    // ========================================================

                    var emailSubject = "Reset Your FarmStay Password";

                    var emailBody = $@"
<html>
<body>

    <h2>Password Reset</h2>

    <p>Hello {user.FullName},</p>

    <p>
        We received a request to reset your FarmStay password.
    </p>

    <p>
        Click the button below to create a new password:
    </p>

    <p>
        <a href=""{resetLink}""
           style=""
               background-color:#c4512d;
               color:white;
               padding:12px 20px;
               text-decoration:none;
               border-radius:6px;
               display:inline-block;
               font-weight:600;
           "">
            Reset Password
        </a>
    </p>

    <p>
        This link will expire in 1 hour.
    </p>

    <p>
        If you did not request a password reset,
        please ignore this email.
    </p>

    <br/>

    <p>
        Thanks,<br/>
        {farmHouse.FarmHouseName}
    </p>

</body>
</html>";

                    // ========================================================
                    // Queue Email
                    // ========================================================

                    _emailQueue.Enqueue(new EmailJob
                    {
                        To = user.Email,
                        Subject = emailSubject,
                        HtmlBody = emailBody,
                        IsHtml = true
                    });

                    _logger.LogInformation(
                        "Password reset email queued successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        user.UserId,
                        farmHouseId);

                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Password reset link has been sent to your email.",
                        Data = true
                    };
                }

                // ============================================================
                // Step 3
                // WhatsApp OTP Flow
                // ============================================================

                if (dto.Method == ForgotPasswordMethod.WhatsAppOtp)
                {
                    var mobileNumber = dto.MobileNumber.Trim();

                    var user = await _userRepository.GetByMobileNumberAsync(
                        mobileNumber,
                        farmHouseId);

                    // --------------------------------------------------------
                    // User Not Found
                    // --------------------------------------------------------

                    if (user == null)
                    {
                        _logger.LogWarning(
                            "Forgot password mobile number not found. MobileNumber: {MobileNumber}, FarmHouseId: {FarmHouseId}",
                            mobileNumber,
                            farmHouseId);

                        return new ApiResponse<bool>
                        {
                            Success = true,
                            Message = "If the account exists, password reset instructions have been sent.",
                            Data = true
                        };
                    }

                    // --------------------------------------------------------
                    // Generate OTP
                    // --------------------------------------------------------

                    var otpCode = RandomNumberGenerator
                        .GetInt32(100000, 1000000)
                        .ToString();

                    var otpExpiry = DateTime.UtcNow.AddMinutes(5);

                    // --------------------------------------------------------
                    // Invalidate Existing Forgot Password OTP
                    // --------------------------------------------------------

                    var existingOtp = await _userOtpRepository.GetActiveOtpAsync(
                        user.UserId,
                        OtpPurpose.ForgotPassword);

                    if (existingOtp != null)
                    {
                        existingOtp.IsActive = false;
                        existingOtp.ModifiedDate = DateTime.UtcNow;
                        existingOtp.ModifiedBy = user.UserId;

                        await _userOtpRepository.UpdateAsync(existingOtp);
                    }

                    // --------------------------------------------------------
                    // Save New OTP
                    // --------------------------------------------------------

                    var userOtp = new UserOtp
                    {
                        UserId = user.UserId,
                        MobileNumber = user.MobileNumber,
                        OtpCode = otpCode,
                        Purpose = OtpPurpose.ForgotPassword,
                        ExpiryDate = otpExpiry,
                        IsUsed = false,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = user.UserId
                    };

                    await _userOtpRepository.AddAsync(userOtp);

                    await _unitOfWork.SaveChangesAsync();

                    // --------------------------------------------------------
                    // Queue WhatsApp OTP
                    // --------------------------------------------------------

                    var whatsAppMessage =
                        $"Your FarmStay password reset OTP is {otpCode}. It is valid for 5 minutes.";

                    _whatsAppQueue.Enqueue(new WhatsAppJob
                    {
                        MobileNumber = user.MobileNumber,
                        Message = whatsAppMessage
                    });

                    _logger.LogInformation(
                        "Password reset WhatsApp OTP queued successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        user.UserId,
                        farmHouseId);

                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Password reset OTP has been sent to your WhatsApp.",
                        Data = true
                    };
                }

                // ============================================================
                // Invalid Method
                // ============================================================

                _logger.LogWarning(
                    "Invalid forgot password method. Method: {Method}",
                    dto.Method);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid password reset method."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred during forgot password.");

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while processing forgot password."
                };
            }
        }


        public async Task<ApiResponse<bool>> ResetPasswordEmailAsync( ResetPasswordByEmailDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Email password reset request received. UserId: {UserId}",
                    dto.UserId);

                // Step 1
                // Validate FarmHouse from Header

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 2
                // Find User

                var user = await _userRepository.GetByIdAsync(dto.UserId);

                if (user == null ||
                    user.FarmHouseId != farmHouseId)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid password reset link."
                    };
                }

                // Step 3
                // Validate Reset Token

                if (string.IsNullOrWhiteSpace(user.PasswordResetToken) ||
                    user.PasswordResetToken != dto.Token)
                {
                    _logger.LogWarning(
                        "Invalid password reset token. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid password reset link."
                    };
                }

                // Step 4
                // Check Token Expiry

                if (!user.PasswordResetTokenExpiry.HasValue ||
                    user.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning(
                        "Password reset token expired. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Password reset link has expired."
                    };
                }

                // Step 5
                // Validate Password

                if (string.IsNullOrWhiteSpace(dto.NewPassword) ||
                    string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password and confirm password are required."
                    };
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password and confirm password do not match."
                    };
                }

                // Step 6
                // Update Password

                user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);

                // Invalidate Reset Token

                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;

                user.ModifiedDate = DateTime.UtcNow;
                user.ModifiedBy = user.UserId;

                await _userRepository.UpdateAsync(user);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Password reset successfully through email. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    farmHouseId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password reset successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while resetting password through email. UserId: {UserId}",
                    dto.UserId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while resetting password."
                };
            }
        }


        public async Task<ApiResponse<bool>> ResetPasswordOtpAsync( ResetPasswordOtpDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "OTP password reset request received. MobileNumber: {MobileNumber}",
                    dto.MobileNumber);

                // Step 1
                // Validate FarmHouse from Header

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 2
                // Find User by Mobile + FarmHouse

                var mobileNumber = dto.MobileNumber.Trim();

                var user = await _userRepository.GetByMobileNumberAsync(
                    mobileNumber,
                    farmHouseId);

                if (user == null)
                {
                    _logger.LogWarning(
                        "OTP password reset user not found. MobileNumber: {MobileNumber}, FarmHouseId: {FarmHouseId}",
                        mobileNumber,
                        farmHouseId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid OTP."
                    };
                }

                // Step 3
                // Get Forgot Password OTP

                var otp = await _userOtpRepository.GetByOtpAsync(
                    user.UserId,
                    dto.OtpCode.Trim(),
                    OtpPurpose.ForgotPassword);

                if (otp == null)
                {
                    _logger.LogWarning(
                        "Invalid password reset OTP. UserId: {UserId}",
                        user.UserId);

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
                // Validate Password

                if (string.IsNullOrWhiteSpace(dto.NewPassword) ||
                    string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password and confirm password are required."
                    };
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password and confirm password do not match."
                    };
                }

                // Step 6
                // Update Password

                user.PasswordHash =
                    _passwordService.HashPassword(dto.NewPassword);

                user.ModifiedDate = DateTime.UtcNow;
                user.ModifiedBy = user.UserId;

                // Step 7
                // Mark OTP as Used

                otp.IsUsed = true;
                otp.IsActive = false;
                otp.ModifiedDate = DateTime.UtcNow;
                otp.ModifiedBy = user.UserId;

                await _userRepository.UpdateAsync(user);
                await _userOtpRepository.UpdateAsync(otp);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Password reset successfully through WhatsApp OTP. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    farmHouseId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password reset successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while resetting password through OTP. MobileNumber: {MobileNumber}",
                    dto.MobileNumber);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while resetting password."
                };
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync( ChangePasswordDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Change password request received.");

                // Step 1
                // Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized."
                    };
                }

                // Step 2
                // Validate FarmHouse from Header

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 3
                // Get User

                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null ||
                    user.FarmHouseId != farmHouseId)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Step 4
                // Check User Status

                if (!user.IsActive || user.IsDeleted)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Your account is inactive."
                    };
                }

                // Step 5
                // Verify Current Password

                var currentPasswordValid =
                    _passwordService.VerifyPassword(
                        dto.CurrentPassword,
                        user.PasswordHash);

                if (!currentPasswordValid)
                {
                    _logger.LogWarning(
                        "Invalid current password. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Current password is incorrect."
                    };
                }

                // Step 6
                // Validate New Password

                if (string.IsNullOrWhiteSpace(dto.NewPassword) ||
                    string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password and confirm password are required."
                    };
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password and confirm password do not match."
                    };
                }

                // Step 7
                // Prevent Same Password

                if (_passwordService.VerifyPassword(
                    dto.NewPassword,
                    user.PasswordHash))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password must be different from current password."
                    };
                }

                // Step 8
                // Update Password

                user.PasswordHash =
                    _passwordService.HashPassword(dto.NewPassword);

                user.ModifiedDate = DateTime.UtcNow;
                user.ModifiedBy = user.UserId;

                await _userRepository.UpdateAsync(user);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Password changed successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    user.UserId,
                    farmHouseId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password changed successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while changing password.");

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while changing password."
                };
            }
        }


        public async Task<ApiResponse<ProfileDto>> GetProfileAsync()
        {
            try
            {
                _logger.LogInformation("Profile request received.");

                // Step 1
                // Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return new ApiResponse<ProfileDto>
                    {
                        Success = false,
                        Message = "Unauthorized."
                    };
                }

                // Step 2
                // Get FarmHouse from Header

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<ProfileDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<ProfileDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 3
                // Get Membership with User, FarmHouse and Role

                var membership = await _userMembershipRepository
                    .GetMembershipAsync(userId, farmHouseId);

                if (membership == null ||
                    membership.User == null ||
                    membership.FarmHouse == null ||
                    membership.Role == null)
                {
                    return new ApiResponse<ProfileDto>
                    {
                        Success = false,
                        Message = "User membership not found."
                    };
                }

                // Step 4
                // Validate User Status

                if (!membership.User.IsActive ||
                    membership.User.IsDeleted)
                {
                    return new ApiResponse<ProfileDto>
                    {
                        Success = false,
                        Message = "Your account is inactive."
                    };
                }

                // Step 5
                // Prepare Profile

                var profile = new ProfileDto
                {
                    UserId = membership.User.UserId,
                    FullName = membership.User.FullName,
                    Email = membership.User.Email,
                    MobileNumber = membership.User.MobileNumber,

                    FarmHouseId = membership.FarmHouse.FarmHouseId,
                    FarmHouseName = membership.FarmHouse.FarmHouseName,

                    RoleId = membership.Role.RoleId,
                    RoleName = membership.Role.RoleName,

                    IsOwner = membership.FarmHouse.OwnerUserId == membership.UserId
                };

                _logger.LogInformation(
                    "Profile retrieved successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    userId,
                    farmHouseId);

                return new ApiResponse<ProfileDto>
                {
                    Success = true,
                    Message = "Profile retrieved successfully.",
                    Data = profile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while retrieving profile.");

                return new ApiResponse<ProfileDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while retrieving profile."
                };
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
        {
            try
            {
                _logger.LogInformation("Refresh token request received.");

                // Step 1
                // Validate input

                if (string.IsNullOrWhiteSpace(dto.AccessToken) ||
                    string.IsNullOrWhiteSpace(dto.RefreshToken))
                {
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Access token and refresh token are required."
                    };
                }

                // Step 2
                // Get principal from expired Access Token

                var principal = _jwtService.GetPrincipalFromExpiredToken(
                    dto.AccessToken);

                if (principal == null)
                {
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid access token."
                    };
                }

                // Step 3
                // Get UserId from JWT

                var userIdClaim = principal
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid access token."
                    };
                }

                // Step 4
                // Get FarmHouseId from JWT

                var farmHouseIdClaim = principal
                    .FindFirst(JwtClaimNames.FarmHouseId)?
                    .Value;

                if (!int.TryParse(farmHouseIdClaim, out int farmHouseId))
                {
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse context."
                    };
                }

                // Step 5
                // Get user's refresh tokens

                var refreshTokens =
                    await _userRefreshTokenRepository
                        .GetByUserIdAsync(userId);

                // Step 6
                // Find matching refresh token
                // Refresh token is stored using PasswordService hash

                var existingToken = refreshTokens
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        !x.IsRevoked)
                    .FirstOrDefault(x =>
                        _passwordService.VerifyPassword(
                            dto.RefreshToken,
                            x.RefreshTokenHash));

                if (existingToken == null)
                {
                    _logger.LogWarning(
                        "Invalid refresh token. UserId: {UserId}",
                        userId);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid refresh token."
                    };
                }

                // Step 7
                // Check Refresh Token Expiry

                if (existingToken.ExpiryDate <= DateTime.UtcNow)
                {
                    existingToken.IsActive = false;
                    existingToken.ModifiedDate = DateTime.UtcNow;
                    existingToken.ModifiedBy = userId;

                    await _userRefreshTokenRepository.UpdateAsync(
                        existingToken);

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogWarning(
                        "Refresh token expired. UserId: {UserId}, RefreshTokenId: {RefreshTokenId}",
                        userId,
                        existingToken.UserRefreshTokenId);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Refresh token has expired."
                    };
                }

                // Step 8
                // Get Membership

                var membership =
                    await _userMembershipRepository.GetMembershipAsync(
                        userId,
                        farmHouseId);

                if (membership == null ||
                    membership.User == null ||
                    membership.FarmHouse == null ||
                    membership.Role == null)
                {
                    _logger.LogWarning(
                        "Membership not found. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        userId,
                        farmHouseId);

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "User membership not found."
                    };
                }

                // Step 9
                // Validate User

                if (!membership.User.IsActive ||
                    membership.User.IsDeleted)
                {
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Your account is inactive."
                    };
                }

                // Step 10
                // Generate New Access Token

                var newAccessToken =
                    _jwtService.GenerateAccessToken(
                        membership.User,
                        membership,
                        membership.FarmHouse,
                        membership.Role);

                // Step 11
                // Generate New Refresh Token

                var newRefreshToken =
                    _jwtService.GenerateRefreshToken();

                // Step 12
                // Hash New Refresh Token

                var newRefreshTokenHash =
                    _passwordService.HashPassword(
                        newRefreshToken);

                // Step 13
                // Revoke Old Refresh Token

                existingToken.IsRevoked = true;
                existingToken.IsActive = false;
                existingToken.RevokedDate = DateTime.UtcNow;
                existingToken.ModifiedDate = DateTime.UtcNow;
                existingToken.ModifiedBy = userId;

                // Step 14
                // Create New Refresh Token

                var newToken = new UserRefreshToken
                {
                    UserId = userId,

                    RefreshTokenHash = newRefreshTokenHash,

                    ExpiryDate = DateTime.UtcNow.AddDays(30),

                    DeviceName = existingToken.DeviceName,
                    Browser = existingToken.Browser,
                    OperatingSystem = existingToken.OperatingSystem,
                    IPAddress = existingToken.IPAddress,

                    IsRevoked = false,
                    IsActive = true,
                    IsDeleted = false,

                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _userRefreshTokenRepository.AddAsync(newToken);

                await _unitOfWork.SaveChangesAsync();

                // Step 15
                // Link Old Token to New Token

                existingToken.ReplacedByTokenId =
                    newToken.UserRefreshTokenId;

                await _userRefreshTokenRepository.UpdateAsync(
                    existingToken);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Refresh token rotated successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    userId,
                    farmHouseId);

                // Step 16
                // Return New Tokens

                return new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Token refreshed successfully.",
                    Data = new LoginResponseDto
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,

                        UserId = membership.User.UserId,
                        FullName = membership.User.FullName,
                        Email = membership.User.Email,

                        FarmHouseId =
                            membership.FarmHouse.FarmHouseId,

                        FarmHouseName =
                            membership.FarmHouse.FarmHouseName,

                        RoleId =
                            membership.Role.RoleId,

                        RoleName =
                            membership.Role.RoleName,

                        IsOwner =
                            membership.FarmHouse.OwnerUserId ==
                            membership.UserId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while refreshing token.");

                return new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while refreshing the token."
                };
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(LogoutDto dto)
        {
            try
            {
                _logger.LogInformation("Logout request received.");

                // Step 1
                // Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized."
                    };
                }

                // Step 2
                // Validate Refresh Token

                if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Refresh token is required."
                    };
                }

                // Step 3
                // Hash Refresh Token

                var refreshTokens = await _userRefreshTokenRepository.GetByUserIdAsync(userId);

                var refreshToken = refreshTokens.FirstOrDefault(x =>
                    _passwordService.VerifyPassword(
                        dto.RefreshToken,
                        x.RefreshTokenHash));

                if (refreshToken == null ||
                    refreshToken.UserId != userId ||
                    refreshToken.IsDeleted)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid refresh token."
                    };
                }

                // Step 5
                // Check Token Status

                if (refreshToken.IsRevoked ||
                    !refreshToken.IsActive)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Refresh token is already inactive."
                    };
                }

                // Step 6
                // Revoke Refresh Token

                refreshToken.IsRevoked = true;
                refreshToken.IsActive = false;
                refreshToken.RevokedDate = DateTime.UtcNow;
                refreshToken.ModifiedDate = DateTime.UtcNow;
                refreshToken.ModifiedBy = userId;

                await _userRefreshTokenRepository.UpdateAsync(refreshToken);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Logout successful. UserId: {UserId}, RefreshTokenId: {RefreshTokenId}",
                    userId,
                    refreshToken.UserRefreshTokenId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Logout successful.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while logging out.");

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while logging out."
                };
            }
        }

        public async Task<ApiResponse<bool>> ResendOtpAsync(ResendOtpRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Resend OTP request received. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    dto.UserId,
                    dto.FarmHouseId);

                // Step 1
                // Validate FarmHouse

                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(dto.FarmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse during resend OTP. FarmHouseId: {FarmHouseId}",
                        dto.FarmHouseId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 2
                // Validate User

                var user = await _userRepository
                    .GetByIdAsync(dto.UserId);

                if (user == null || user.FarmHouseId != dto.FarmHouseId)
                {
                    _logger.LogWarning(
                        "User not found or does not belong to FarmHouse. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        dto.UserId,
                        dto.FarmHouseId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Step 3
                // Check whether mobile is already verified

                if (user.IsMobileVerified)
                {
                    _logger.LogWarning(
                        "Resend OTP requested for already verified mobile. UserId: {UserId}",
                        user.UserId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Mobile number is already verified."
                    };
                }

                // Step 4
                // Invalidate existing active registration OTP

                var existingOtp = await _userOtpRepository
                    .GetActiveOtpAsync(
                        user.UserId,
                        OtpPurpose.Register);

                if (existingOtp != null)
                {
                    existingOtp.IsActive = false;
                    existingOtp.ModifiedDate = DateTime.UtcNow;
                    existingOtp.ModifiedBy = user.UserId;

                    await _userOtpRepository.UpdateAsync(existingOtp);

                    _logger.LogInformation(
                        "Existing registration OTP invalidated. UserId: {UserId}",
                        user.UserId);
                }

                // Step 5
                // Generate new OTP

                var otpCode = RandomNumberGenerator
                    .GetInt32(100000, 1000000)
                    .ToString();

                var otpExpiry = DateTime.UtcNow.AddMinutes(5);

                // Step 6
                // Save new OTP

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
                    "New registration OTP saved successfully. UserId: {UserId}, Expiry: {Expiry}",
                    user.UserId,
                    otpExpiry);

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
                    "Resend OTP queued successfully. UserId: {UserId}, Mobile: {MobileNumber}",
                    user.UserId,
                    user.MobileNumber);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "OTP resent successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while resending OTP. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    dto.UserId,
                    dto.FarmHouseId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while resending OTP."
                };
            }
        }
    }



}
