//using FarmStay.Domain.Entities;

//namespace FarmStay.Application.Interfaces.Repositories
//{
//    public interface IUserRepository
//    {
//        Task AddAsync(User user);

//        Task<User?> GetByIdAsync(int userId);

//        Task<User?> GetByEmailAsync(string email);

//        Task<User?> GetByEmailOrMobileAsync(string email, string mobileNumber);
//        Task<User?> GetByVerificationTokenAsync(string token);

//        Task<User?> GetByPasswordResetTokenAsync(string token);

//        Task<List<User>> GetAllAsync();

//        Task UpdateAsync(User user);
//    }
//}


using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);

        Task<User?> GetByIdAsync(int userId);


        Task<User?> GetByEmailOrMobileAsync(string email, string mobileNumber);


        Task<User?> GetByMobileNumberAsync(string mobileNumber,int farmhouseId);

        Task<User?> GetByEmailVerificationTokenAsync( int userId, int farmHouseId, string token);
        Task<User?> GetByEmailAsync(string email, int farmhouseId);

        Task<List<User>> GetAllAsync();

        Task UpdateAsync(User user);
    }


}
