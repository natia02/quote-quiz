using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using QuoteQuiz.Application.DTOs.Auth;
using QuoteQuiz.Application.Interfaces.Repositories;
using QuoteQuiz.Application.Interfaces.Services;
using QuoteQuiz.Application.Services;
using QuoteQuiz.Domain.Entities;
using QuoteQuiz.Domain.Exceptions;
using Xunit;

namespace QuoteQuiz.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<User>> _mockUsersRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUsersRepo = new Mock<IRepository<User>>();
        _mockTokenService = new Mock<ITokenService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthService>>();

        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUsersRepo.Object);

        _authService = new AuthService(
            _mockUnitOfWork.Object,
            _mockTokenService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_ValidData_ReturnsAuthResponseWithToken()
    {
        // Arrange
        var dto = new RegisterDto { Username = "john", Email = "john@test.com", Password = "Test1234!" };

        _mockUsersRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockMapper
            .Setup(m => m.Map<AuthResponseDto>(It.IsAny<User>()))
            .Returns(new AuthResponseDto { UserName = "john", Email = "john@test.com", Role = "User" });

        _mockTokenService
            .Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Token.Should().Be("jwt-token");
        result.UserName.Should().Be("john");
        _mockUsersRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyTaken_ThrowsConflictException()
    {
        // Arrange
        var dto = new RegisterDto { Username = "john", Email = "taken@test.com", Password = "Test1234!" };

        _mockUsersRepo
            .SetupSequence(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(true); // email check → conflict

        // Act
        var act = async () => await _authService.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*email*");

        _mockUsersRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_UsernameAlreadyTaken_ThrowsConflictException()
    {
        // Arrange
        var dto = new RegisterDto { Username = "taken", Email = "new@test.com", Password = "Test1234!" };

        _mockUsersRepo
            .SetupSequence(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false)  // email OK
            .ReturnsAsync(true);  // username taken

        // Act
        var act = async () => await _authService.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*username*");

        _mockUsersRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ValidData_CreatesUserWithHashedPassword()
    {
        // Arrange
        var dto = new RegisterDto { Username = "john", Email = "john@test.com", Password = "Test1234!" };

        _mockUsersRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockMapper
            .Setup(m => m.Map<AuthResponseDto>(It.IsAny<User>()))
            .Returns(new AuthResponseDto());

        _mockTokenService
            .Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        User? capturedUser = null;
        _mockUsersRepo
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u);

        // Act
        await _authService.RegisterAsync(dto);

        // Assert — password must be hashed, never stored as plaintext
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe("Test1234!");
        BCrypt.Net.BCrypt.Verify("Test1234!", capturedUser.PasswordHash).Should().BeTrue();
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_WithEmail_ReturnsAuthResponseWithToken()
    {
        // Arrange
        var dto = new LoginDto { EmailOrUsername = "john@test.com", Password = "Test1234!" };
        var user = new User
        {
            Id = 1,
            Username = "john",
            Email = "john@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!"),
            IsActive = true
        };

        _mockUsersRepo
            .SetupSequence(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);   // found by email

        _mockMapper
            .Setup(m => m.Map<AuthResponseDto>(user))
            .Returns(new AuthResponseDto { UserName = "john", Email = "john@test.com", Role = "User" });

        _mockTokenService
            .Setup(t => t.GenerateToken(user))
            .Returns("jwt-token");

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Token.Should().Be("jwt-token");
        result.UserName.Should().Be("john");
    }

    [Fact]
    public async Task LoginAsync_WithUsername_ReturnsAuthResponseWithToken()
    {
        // Arrange — email lookup returns null, username lookup finds the user
        var dto = new LoginDto { EmailOrUsername = "john", Password = "Test1234!" };
        var user = new User
        {
            Id = 1,
            Username = "john",
            Email = "john@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!"),
            IsActive = true
        };

        _mockUsersRepo
            .SetupSequence(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null)  // not found by email
            .ReturnsAsync(user);        // found by username

        _mockMapper
            .Setup(m => m.Map<AuthResponseDto>(user))
            .Returns(new AuthResponseDto { UserName = "john", Email = "john@test.com", Role = "User" });

        _mockTokenService
            .Setup(t => t.GenerateToken(user))
            .Returns("jwt-token");

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Token.Should().Be("jwt-token");
        result.UserName.Should().Be("john");
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange — neither email nor username match
        var dto = new LoginDto { EmailOrUsername = "nobody", Password = "Test1234!" };

        _mockUsersRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _authService.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_AccountDisabled_ThrowsUnauthorizedException()
    {
        // Arrange
        var dto = new LoginDto { EmailOrUsername = "john@test.com", Password = "Test1234!" };
        var user = new User
        {
            Username = "john",
            Email = "john@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!"),
            IsActive = false
        };

        _mockUsersRepo
            .SetupSequence(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _authService.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*disabled*");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var dto = new LoginDto { EmailOrUsername = "john@test.com", Password = "WrongPassword!" };
        var user = new User
        {
            Username = "john",
            Email = "john@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!"),
            IsActive = true
        };

        _mockUsersRepo
            .SetupSequence(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _authService.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_AndWrongPassword_ReturnSameMessage()
    {
        // Arrange — both cases return "Invalid credentials", attacker can't enumerate accounts
        var dto = new LoginDto { EmailOrUsername = "nobody", Password = "anything" };

        _mockUsersRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _authService.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials");
    }

    #endregion
}
