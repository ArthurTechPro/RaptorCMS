﻿using Raptor.Core.Helpers;
using Raptor.Core.Security;
using Raptor.Data.Core;
using Raptor.Data.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Raptor.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<Person> _peopleRepository;
        private readonly IRepository<BusinessEntity> _businessEntityRepository;
        private readonly IRepository<Password> _passwordRepository;
        private readonly IRepository<ForgotPasswordRequest> _forgotPasswordRequestsRepository;
        private readonly IRepository<SocialProfile> _socialProfileRepository;
        private readonly IList<Expression<Func<Person, object>>> _userProperties;

        public UserService(IRepository<Person> peopleRepository, IRepository<BusinessEntity> businessEntityRepository, IRepository<Password> passwordRepository, IRepository<ForgotPasswordRequest> forgotPasswordRequestsRepository, IRepository<SocialProfile> socialProfileRepository) {
            _peopleRepository = peopleRepository;
            _businessEntityRepository = businessEntityRepository;
            _passwordRepository = passwordRepository;
            _forgotPasswordRequestsRepository = forgotPasswordRequestsRepository;
            _socialProfileRepository = socialProfileRepository;

            _userProperties = new List<Expression<Func<Person, object>>> {
                u => u.Password,
                u => u.PhoneNumbers,
                u => u.UserRoles,
                u => u.BusinessEntity,
                u => u.SocialProfile
            };

        }

        /// <summary>
        /// Checks if the user exists for the specified user id
        /// </summary>
        /// <param name="userId">Id of the user whos status is to be verified</param>
        /// <returns>True if the user exists, false otherwise</returns>
        public bool CheckIfUserExists(int userId) {
            return _peopleRepository.Any(userId);
        }

        /// <summary>
        /// Checks if the user exists for the specified email addrsss
        /// </summary>
        /// <param name="emailAddress">Email Address of the user whos status is to be verified</param>
        /// <returns>True if the user exists, false otherwise</returns>
        public bool CheckIfUserExistsByEmail(string emailAddress) {
            return _peopleRepository.Find(u => u.EmailAddress == emailAddress).Any();
        }

        /// <summary>
        /// Checks if the user exists for the specified email addrsss
        /// </summary>
        /// <param name="username">Email Address of the user whos status is to be verified</param>
        /// <returns>True if the user exists, false otherwise</returns>
        public bool CheckIfUserExistsByUsername(string username) {
            return _peopleRepository.Find(u => u.Username == username).Any();
        }

        /// <summary>
        /// Returns the users for the specified id.
        /// </summary>
        /// <param name="id">Id of the user</param>
        /// <returns>User associated with the provided IDs.</returns>
        public Person GetUserById(int id) {
            if (id == 0)
                throw new ArgumentException("User ID cannot be zero.", nameof(id));

            if (!CheckIfUserExists(id))
                throw new ArgumentException($"No user exists for the specified user id = {id}", nameof(id));

            return _peopleRepository
                .IncludeMultiple(_userProperties)
                .SingleOrDefault(u => u.BusinessEntityId == id && !u.IsDeleted);
        }

        /// <summary>
        /// Returns a list of users for the specified IDs.
        /// </summary>
        /// <param name="ids">IDs for whom users are to be returned</param>
        /// <returns>Users for the specified IDs</returns>
        public IList<Person> GetUserByIds(int[] ids) {
            if (ids == null || ids.Length == 0)
                return new List<Person>();

            return _peopleRepository.Find(u => ids.Contains(u.BusinessEntityId) && !u.IsDeleted).ToList();
        }

        /// <summary>
        /// Gets a user with GUID
        /// </summary>
        /// <param name="guid">GUID for who's user is to be found</param>
        /// <returns>User who's guid was specified</returns>
        public Person GetUserByGuid(Guid guid) {
            var businessEntity = _businessEntityRepository
                .SingleOrDefault(b => b.RowGuid == guid);

            if (businessEntity == null)
                throw new ArgumentException("No user can be found for specified guid.", nameof(guid));

            return GetUserById(businessEntity.BusinessEntityId);
        }

        /// <summary>
        /// Gets a user with GUID when it is specified in string format.
        /// </summary>
        /// <param name="guid">GUID for who's user is to be found</param>
        /// <returns>User who's guid was specified</returns>
        public Person GetUserByGuid(string guid) {
            return GetUserByGuid(Guid.Parse(guid));
        }

        /// <summary>
        /// Gets a user with email address
        /// </summary>
        /// <param name="emailAddress">Email Address of the user</param>
        /// <returns>User who's email address was specified</returns>
        public Person GetUserByEmail(string emailAddress) {
            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentException("An Email Address is required. String cannot be null/empty.", nameof(emailAddress));

            if (!CheckIfUserExistsByEmail(emailAddress))
                throw new ArgumentException("No user found for the specified email address.", nameof(emailAddress));

            var user = _peopleRepository
                .IncludeMultiple(_userProperties)
                .SingleOrDefault(u => u.EmailAddress == emailAddress);

            return user;
        }

        /// <summary>
        /// Gets a user with username
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <returns>User with the specified username</returns>
        public Person GetUserByUsername(string username) {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Please provide a username. It cannot be empty.", nameof(username));

            var user = _peopleRepository
                .IncludeMultiple(_userProperties)
                .SingleOrDefault(u => u.Username == username);

            if (user == null)
                throw new ArgumentException($"No user found for the specified username = {username}", nameof(username));

            return user;
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="person">User who is to be deleted</param>
        public void DeleteUser(Person person) {
            if (person == null)
                throw new ArgumentException("User cannot be null.", nameof(person));

            person.IsDeleted = true;
            _peopleRepository.Update(person);
        }

        /// <summary>
        /// Deletes a user by user id
        /// </summary>
        /// <param name="businessEntityId">ID of the user which is to be deleted</param>
        public void DeleteUser(int businessEntityId) {
            if (businessEntityId == 0)
                throw new ArgumentException("Business Entity Id cannot be zero.", nameof(businessEntityId));

            var person = _peopleRepository.GetById(businessEntityId);

            person.IsDeleted = true;
            _peopleRepository.Update(person);
        }

        /// <summary>
        /// Update a user
        /// </summary>
        /// <param name="person">User to be updated</param>
        public void UpdateUser(Person person) {
            if (person == null)
                throw new ArgumentException("User cannot be null.", nameof(person));

            person.DateModifiedUtc = DateTime.UtcNow;
            _peopleRepository.Update(person);
        }

        /// <summary>
        /// Count the number of users
        /// </summary>
        /// <returns>Number of registered users</returns>
        public int CountUsers() {
            return _peopleRepository.GetAll().Count();
        }

        /// <summary>
        /// Updates a user's password
        /// </summary>
        /// <param name="emailAddress">Email Address of the user who's password is to be updated.</param>
        /// <param name="password">New password of the user</param>
        public void UpdatePassword(string emailAddress, string password) {
            var user = GetUserByEmail(emailAddress);

            if (user == null) throw new ArgumentException($"Change Password - no user found for email address specified: {emailAddress}");

            var currentPassword = _passwordRepository.GetById(user.BusinessEntityId);
            var salt = HashGenerator.CreateSalt();

            currentPassword.PasswordSalt = salt;
            currentPassword.PasswordHash = HashGenerator.GenerateHash(password, salt);

            _passwordRepository.Update(currentPassword);
        }

        /// <summary>
        /// Creates a forgot password request
        /// </summary>
        /// <param name="userId">Id of the user for whom the request is to be created</param>
        public void CreateForgotPasswordRequest(int userId) {
            var forgotPasswordRequest = new ForgotPasswordRequest() {
                BusinessEntityId = userId,
                Link = CommonHelper.RandomString(32),
                DateCreatedUtc = DateTime.UtcNow
            };

            _forgotPasswordRequestsRepository.Create(forgotPasswordRequest);
        }

        /// <summary>
        /// Validate a forgot password request
        /// </summary>
        /// <param name="link">Link of the request</param>
        /// <returns>Email Address if request is valid, otherwise nothing.</returns>
        public bool ValidateForgotPasswordRequest(string link) {
            var forgotPasswordRequest = _forgotPasswordRequestsRepository.SingleOrDefault(s => s.Link == link);
            var datesDifference = DateTime.UtcNow - forgotPasswordRequest.DateCreatedUtc;
            return datesDifference.TotalHours <= 24;
        }

        /// <summary>
        /// Get forgot password request
        /// </summary>
        /// <param name="link">Link for which the request is to be fetched</param>
        /// <returns>Forgot Password Request</returns>
        public ForgotPasswordRequest GetForgotPasswordRequest(string link) {
            return _forgotPasswordRequestsRepository.SingleOrDefault(s => s.Link == link);
        }

        /// <summary>
        /// Create social profile
        /// </summary>
        /// <param name="profile">Social profile object to create</param>
        public void CreateSocialProfile(SocialProfile profile){
            _socialProfileRepository.Create(profile);
        }

        /// <summary>
        /// Find social profile by user id
        /// </summary>
        /// <param name="userId">Id of the user whos social profile to get</param>
        /// <returns>SocialProfile of the user</returns>
        public SocialProfile GetSocialProfileById(int userId){
            return _socialProfileRepository.SingleOrDefault(p => p.BusinessEntityId == userId);
        }

        /// <summary>
        /// Returns all users
        /// </summary>
        /// <returns>List of Person Object</returns>
        public IEnumerable<Person> GetAllUsers() {
            return _peopleRepository.GetAll();
        }
    }
}
