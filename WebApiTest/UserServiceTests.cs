using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Moq.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections;
using System.Text.Json;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Logger;
using WebApi.Models.Users;
using WebApi.Services;

namespace WebApiTest
{
    public class UserServiceTests
    {
        //what i'am testing
        private readonly UserService _sut;
        private readonly Mock<IMapper> _mapperMock = new Mock<IMapper>();
        private readonly Mock<DataContext> _dbContextMock = new Mock<DataContext>();
        private readonly Mock<ILoggingService> _loggerMock = new Mock<ILoggingService>();
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public UserServiceTests()
        {
            _sut = new UserService(_dbContextMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetAllUsers_ShoulReturn_ListOfAllUsers()
        {
            // Arrange
            var fileData = File.ReadAllText("C:\\Users\\FOTSING\\source\\repos\\WebApiTest\\Helpers\\TestData.json");

            //NewtonSoft.Json
            //var userData = JsonConvert.DeserializeObject<List<User>>(fileData);            

            //or System.Text.Json
            List<User>? userData = System.Text.Json.JsonSerializer.Deserialize<List<User>>(fileData, _options);
            _dbContextMock.Setup<DbSet<User>>(x => x.Users)
                .ReturnsDbSet(userData);

            //Act
            List<User> users = _sut.GetAllUsers().ToList();
            
            //Assert
            Assert.NotNull(users);
            Assert.Equal(userData?.Count ?? 2, users.Count);
        }


        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            //Arrange
            int customerId = 1;
            var fileData = File.ReadAllText("C:\\Users\\FOTSING\\source\\repos\\WebApiTest\\Helpers\\TestData.json");

            //NewtonSoft.Json
            var userData = JsonConvert.DeserializeObject<List<User>>(fileData);
            _dbContextMock.Setup(x => x.Users.FindAsync(customerId))
                .ReturnsAsync(userData.Find(u => u.Id == customerId) ?? new User());

            //Act
            var user = await _sut.GetUserByIdAsync(customerId);

            //Assert
            Assert.Equal(customerId, user.Id);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        public async Task GetUserByIdAsync_ShouldThrow_WhenUserDoesntExist(int customerId)
        {
            //Arrange
            _dbContextMock.Setup(x => x.Users.FindAsync(It.IsAny<int>())).ReturnsAsync(() => null);
            
            //Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _sut.GetUserByIdAsync(customerId));
            Assert.Contains("User not found", ex.Message);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
        {
            //Arrange
            int customerId = 1;
            var fileData = File.ReadAllText("C:\\Users\\FOTSING\\source\\repos\\WebApiTest\\Helpers\\TestData.json");
            var userData = JsonConvert.DeserializeObject<List<User>>(fileData);
            var testUser = userData.Find(u => u.Id == customerId) ?? new User();
            _dbContextMock.Setup(x => x.Users.FindAsync(customerId))
                .ReturnsAsync(testUser);

            //Act
            await _sut.DeleteUserAsync(customerId);

            //Assert
            _dbContextMock.Verify(x => x.Users.Remove(testUser));
        }

        public static IEnumerable<object[]> GetUpdateRequestData()
        {
            yield return new object[] { 1, new UpdateRequest() };
            yield return new object[] { 3, new UpdateRequest() };
        }
        [Theory]
        [MemberData(nameof(GetUpdateRequestData))]
        public async Task UpdateUserAsync_ShoudUpdateUser_WhenUserExists(int customerId, UpdateRequest model)
        {
            //Arrange
            var fileData = File.ReadAllText("C:\\Users\\FOTSING\\source\\repos\\WebApiTest\\Helpers\\TestData.json");
            var userData = JsonConvert.DeserializeObject<List<User>>(fileData);
            var testUser = userData.Find(u => u.Id == customerId) ?? new User();
            
            //use MockQueryable
            var dbSetMock = userData.BuildMock().BuildMockDbSet();
            dbSetMock.Setup(x => x.FindAsync(customerId))
                .ReturnsAsync(testUser);
            _dbContextMock.Setup(x => x.Users).Returns(dbSetMock.Object);

            //Act
            await _sut.UpdateUserAsync(customerId, model);

            //Assert
            _dbContextMock.Verify(x => x.Users.Update(testUser));
        }

        public class UpdateRequestClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { 1, new UpdateRequest() { Email = "humpdan@outlook.com" } };
                yield return new object[] { 3, new UpdateRequest() { Email = "c.malu@gmail.com" } };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        [Theory]
        [ClassData(typeof(UpdateRequestClassData))]
        public async Task UpdateUserAsync_ShoudThrow_WhenUserEmailAlreadyExists(int customerId, UpdateRequest model)
        {
            //Arrange
            var fileData = File.ReadAllText("C:\\Users\\FOTSING\\source\\repos\\WebApiTest\\Helpers\\TestData.json");
            var userData = JsonConvert.DeserializeObject<List<User>>(fileData);
            var testUser = userData.Find(u => u.Id == customerId) ?? new User();
            
            var dbSetMock = userData.BuildMock().BuildMockDbSet();
            dbSetMock.Setup(x => x.FindAsync(customerId))
                .ReturnsAsync(testUser);
            _dbContextMock.Setup(x => x.Users).Returns(dbSetMock.Object);

            //Assert
            var ex = await Assert.ThrowsAsync<AppException>(async () => await _sut.UpdateUserAsync(customerId, model));
            Assert.StartsWith("User with the email '", ex.Message);
        }

        public class CreateRequestDataGenerator
        {
            public static IEnumerable<object[]> ModelWithInvalidEmail =>
                new List<object[]>
                {
                    new object[] 
                    {
                        new CreateRequest() { Title = "user1",FirstName = "Marcelle",LastName = "Matuba",Role = "Admin",Email = "c.malu@gmail.com",Password = "azerty",ConfirmPassword = "azerty" }
                    }
                };

            public static IEnumerable<object[]> ValidModel =>
                new List<object[]>
                {
                    new object[]
                    {
                        new CreateRequest() { Title = "user2",FirstName = "Maria",LastName = "Christ",Role = "User",Email = "m.builu@gmail.com",Password = "azerty",ConfirmPassword = "azerty" }
                    }
                };
        }
        [Theory]
        [MemberData(nameof(CreateRequestDataGenerator.ValidModel), MemberType = typeof(CreateRequestDataGenerator))]
        public async Task CreateUserAsync_ShouldCreateUser_WhenUserDoesntExist(CreateRequest model)
        {
            //Arrange
            var fileData = File.ReadAllText("C:\\Users\\FOTSING\\source\\repos\\WebApiTest\\Helpers\\TestData.json");
            var userData = JsonConvert.DeserializeObject<List<User>>(fileData);

            var dbSetMock = userData.BuildMock().BuildMockDbSet();
            _dbContextMock.Setup(x => x.Users).Returns(dbSetMock.Object);
            User user = new();
            _mapperMock.Setup(x => x.Map<User>(model)).Returns(user);

            //Act
            await _sut.CreateUserAsync(model);

            //Assert
            //_dbContextMock.Verify(async (x) => await x.Users.AddAsync(user));
            _dbContextMock.Verify((x) => x.Users.AddAsync(user, new CancellationToken()).Result);
        }

        [Theory]
        [MemberData(nameof(CreateRequestDataGenerator.ModelWithInvalidEmail), MemberType = typeof(CreateRequestDataGenerator))]
        public async Task CreateUserAsync_ShouldThrow_WhenUserEmailAlreadyExists(CreateRequest model)
        {
            //Arrange
            var fileData = File.ReadAllText("C:\\Users\\FOTSING\\source\\repos\\WebApiTest\\Helpers\\TestData.json");
            var userData = JsonConvert.DeserializeObject<List<User>>(fileData);

            var dbSetMock = userData.BuildMock().BuildMockDbSet();
            _dbContextMock.Setup(x => x.Users).Returns(dbSetMock.Object);

            //Assert
            var ex = await Assert.ThrowsAsync<AppException>(async () => await _sut.CreateUserAsync(model));
            Assert.Contains("User with the email '", ex.Message);
        }

    }
}