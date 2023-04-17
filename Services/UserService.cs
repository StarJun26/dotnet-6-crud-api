namespace WebApi.Services;

using AutoMapper;
using BCrypt.Net;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Logger;
using WebApi.Models.Users;

public class UserService : IUserService
{
    private DataContext _context;
    private readonly IMapper _mapper;
    private readonly ILoggingService _logger;

    public UserService(
        DataContext context,
        IMapper mapper,
        ILoggingService logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _context.Users;
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await getUser(id);
    }

    public async Task CreateUserAsync(CreateRequest model)
    {
        // validate
        if (_context.Users.Any(x => x.Email == model.Email))
            throw new AppException("User with the email '" + model.Email + "' already exists");

        // map model to new user object
        var user = _mapper.Map<User>(model);

        // hash password
        user.PasswordHash = BCrypt.HashPassword(model.Password);

        // save user
        await _context.Users.AddAsync(user);
        int id = await _context.SaveChangesAsync();
        _logger.LogInformation("User {0} successfully created !!", id);
    }

    public async Task UpdateUserAsync(int id, UpdateRequest model)
    {
        var user = await getUser(id);

        // validate
        if (model.Email != user.Email && _context.Users.Any(x => x.Email == model.Email))
            throw new AppException("User with the email '" + model.Email + "' already exists");

        // hash password if it was entered
        if (!string.IsNullOrEmpty(model.Password))
            user.PasswordHash = BCrypt.HashPassword(model.Password);

        // copy model to user and save
        _mapper.Map(model, user);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {0} successfully updated !!", id);
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await getUser(id);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {0} successfully deleted !!", id);
    }

    // helper methods

    private async Task<User> getUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }
}