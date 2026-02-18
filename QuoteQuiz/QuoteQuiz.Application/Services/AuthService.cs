using AutoMapper;
using Microsoft.Extensions.Logging;
using QuoteQuiz.Application.DTOs.Auth;
using QuoteQuiz.Application.Interfaces.Repositories;
using QuoteQuiz.Application.Interfaces.Services;
using QuoteQuiz.Domain.Entities;
using QuoteQuiz.Domain.Enums;
using QuoteQuiz.Domain.Exceptions;

namespace QuoteQuiz.Application.Services;

public class AuthService : IAuthService
{
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        if (await _unitOfWork.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            _logger.LogWarning("Registration failed: email {Email} is already taken", registerDto.Email);
            throw new ConflictException("User with this email already exists");
        }

        if (await _unitOfWork.Users.AnyAsync(u => u.Username == registerDto.Username))
        {
            _logger.LogWarning("Registration failed: username {UserName} is already taken", registerDto.Username);
            throw new ConflictException("User with this username already exists");
        }

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {UserName} registered successfully", user.Username);

        var response = _mapper.Map<AuthResponseDto>(user);
        response.Token = _tokenService.GenerateToken(user);

        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == loginDto.EmailOrUsername)
                   ?? await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Username == loginDto.EmailOrUsername);

        if (user == null)
        {
            _logger.LogWarning("Login failed: no user found for {EmailOrUsername}", loginDto.EmailOrUsername);
            throw new UnauthorizedException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: account {EmailOrUsername} is disabled", loginDto.EmailOrUsername);
            throw new UnauthorizedException("Your account has been disabled");
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: wrong password for {EmailOrUsername}", loginDto.EmailOrUsername);
            throw new UnauthorizedException("Invalid credentials");
        }

        _logger.LogInformation("User {UserName} logged in successfully", user.Username);

        var response = _mapper.Map<AuthResponseDto>(user);
        response.Token = _tokenService.GenerateToken(user);

        return response;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _unitOfWork.Users.AnyAsync(u => u.Email == email);
    }
}