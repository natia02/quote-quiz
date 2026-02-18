using QuoteQuiz.Application.DTOs.Admin;
using QuoteQuiz.Application.DTOs.Auth;

namespace QuoteQuiz.Application.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto?> UpdateUserAsync(int id, UserDto userDto);
    Task<bool> DisableUserAsync(int id);
    Task<bool> DeleteUserAsync(int id);
}