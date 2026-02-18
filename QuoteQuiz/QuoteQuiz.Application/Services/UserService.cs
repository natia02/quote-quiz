using AutoMapper;
using QuoteQuiz.Application.DTOs.Admin;
using QuoteQuiz.Application.Interfaces.Repositories;
using QuoteQuiz.Application.Interfaces.Services;
using QuoteQuiz.Domain.Entities;
using QuoteQuiz.Domain.Enums;
using QuoteQuiz.Domain.Exceptions;

namespace QuoteQuiz.Application.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return null;

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        if (await _unitOfWork.Users.AnyAsync(u => u.Email == createUserDto.Email))
            throw new ConflictException("User with this email already exists");

        if (await _unitOfWork.Users.AnyAsync(u => u.Username == createUserDto.UserName))
            throw new ConflictException("User with this username already exists");

        if (!Enum.TryParse<UserRole>(createUserDto.Role, out var userRole))
            throw new ValidationException("Invalid role specified");

        var user = new User
        {
            Username = createUserDto.UserName,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            Role = userRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UserDto userDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return null;

        if (await _unitOfWork.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id))
            throw new ConflictException("Email already in use by another user");

        if (await _unitOfWork.Users.AnyAsync(u => u.Username == userDto.UserName && u.Id != id))
            throw new ConflictException("Username already in use by another user");

        user.Username = userDto.UserName;
        user.Email = userDto.Email;
        user.IsActive = userDto.IsActive;

        if (Enum.TryParse<UserRole>(userDto.Role, out var role))
            user.Role = role;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> DisableUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return false;

        user.IsActive = false;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return false;

        // Check if user has created quotes
        var hasQuotes = await _unitOfWork.Quotes.AnyAsync(q => q.CreatedByUserId == id);
        if (hasQuotes)
            throw new ConflictException("Cannot delete user who has created quotes. Disable instead.");

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}