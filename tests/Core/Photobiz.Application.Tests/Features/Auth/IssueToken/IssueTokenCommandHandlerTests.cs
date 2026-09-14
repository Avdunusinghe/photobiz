using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Photobiz.Application.Common.Constants;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Settings;
using Photobiz.Application.Features.Auth.IssueToken;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Auth.IssueToken
{
    public class IssueTokenCommandHandlerTests
    {
        private const string TenantKey = "acme";

        private static readonly JwtSettings Settings = new()
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "unit-test-signing-key-that-is-long-enough-for-hmacsha256",
            ExpiryMinutes = 30
        };

        private readonly IPasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
        private readonly TestDbContext _dbContext = CreateDbContext();
        private readonly IssueTokenCommandHandler _handler;

        public IssueTokenCommandHandlerTests()
        {
            _handler = new IssueTokenCommandHandler(Options.Create(Settings), _dbContext, _passwordHasher);
        }

        private static IssueTokenCommand Command(string username, string password) =>
            new(TenantKey, username, password);

        private static TestDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TestDbContext(options);
        }

        private async Task<User> CreateUserAsync(string username, string password, params string[] roleNames)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = string.Empty,
                FirstName = "Test",
                LastName = "User",
                Email = $"{username}@example.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _dbContext.Users.Add(user);

            foreach (var roleName in roleNames)
            {
                var role = await _dbContext.Roles.SingleOrDefaultAsync(x => x.Name == roleName);
                if (role is null)
                {
                    role = new Role { Id = Guid.NewGuid(), Name = roleName };
                    _dbContext.Roles.Add(role);
                }

                _dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            }

            await _dbContext.SaveChangesAsync();

            return user;
        }

        [Fact]
        public async Task Handle_WithValidCredentials_ReturnsTokenWithExpectedClaimsAndExpiry()
        {
            await CreateUserAsync("someone", "correct-password", RoleNames.Admin);

            var before = DateTime.UtcNow;

            var result = await _handler.Handle(Command("someone", "correct-password"), CancellationToken.None);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

            Assert.Equal(Settings.Issuer, token.Issuer);
            Assert.Equal(Settings.Audience, token.Audiences.Single());
            Assert.Equal("someone", token.Subject);
            Assert.InRange(result.ExpiresAtUtc, before.AddMinutes(Settings.ExpiryMinutes), before.AddMinutes(Settings.ExpiryMinutes).AddSeconds(5));
        }

        [Fact]
        public async Task Handle_WithValidCredentials_IncludesTenantKeyClaim()
        {
            await CreateUserAsync("someone", "correct-password", RoleNames.Admin);

            var result = await _handler.Handle(Command("someone", "correct-password"), CancellationToken.None);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
            var tenantClaim = token.Claims.Single(c => c.Type == TenantClaimTypes.TenantKey);

            Assert.Equal(TenantKey, tenantClaim.Value);
        }

        [Fact]
        public async Task Handle_WithValidCredentials_IncludesRoleClaim()
        {
            await CreateUserAsync("someone", "correct-password", RoleNames.Photographer);

            var result = await _handler.Handle(Command("someone", "correct-password"), CancellationToken.None);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
            var roleClaim = token.Claims.Single(c => c.Type == ClaimTypes.Role);

            Assert.Equal(RoleNames.Photographer, roleClaim.Value);
        }

        [Fact]
        public async Task Handle_WithMultipleRoles_IncludesOneClaimPerRole()
        {
            await CreateUserAsync("someone", "correct-password", RoleNames.Admin, RoleNames.Photographer);

            var result = await _handler.Handle(Command("someone", "correct-password"), CancellationToken.None);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
            var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();

            Assert.Equal([RoleNames.Admin, RoleNames.Photographer], roleClaims);
        }

        [Fact]
        public async Task Handle_CalledTwice_ProducesDifferentTokenIds()
        {
            await CreateUserAsync("someone", "correct-password", RoleNames.Admin);

            var first = await _handler.Handle(Command("someone", "correct-password"), CancellationToken.None);
            var second = await _handler.Handle(Command("someone", "correct-password"), CancellationToken.None);

            var handler = new JwtSecurityTokenHandler();
            var firstJti = handler.ReadJwtToken(first.AccessToken).Id;
            var secondJti = handler.ReadJwtToken(second.AccessToken).Id;

            Assert.NotEqual(firstJti, secondJti);
        }

        [Fact]
        public async Task Handle_WithUnknownUsername_ThrowsAuthenticationFailedException()
        {
            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                _handler.Handle(Command("nobody", "whatever"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithWrongPassword_ThrowsAuthenticationFailedException()
        {
            await CreateUserAsync("someone", "correct-password", RoleNames.Admin);

            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                _handler.Handle(Command("someone", "wrong-password"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithSoftDeletedUser_ThrowsAuthenticationFailedException()
        {
            var user = await CreateUserAsync("someone", "correct-password", RoleNames.Admin);
            user.IsActive = false;
            await _dbContext.SaveChangesAsync();

            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                _handler.Handle(Command("someone", "correct-password"), CancellationToken.None));
        }

        private class TestDbContext : DbContext, IApplicationDbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            {
            }

            public DbSet<User> Users => Set<User>();

            public DbSet<Role> Roles => Set<Role>();

            public DbSet<UserRole> UserRoles => Set<UserRole>();

            public DbSet<Client> Clients => Set<Client>();

            public DbSet<Gallery> Galleries => Set<Gallery>();

            public DbSet<Photo> Photos => Set<Photo>();

            public DbSet<SessionType> SessionTypes => Set<SessionType>();

            public DbSet<Booking> Bookings => Set<Booking>();

            public DbSet<SiteTheme> SiteThemes => Set<SiteTheme>();

            public DbSet<SiteFooterLink> SiteFooterLinks => Set<SiteFooterLink>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
                modelBuilder.Entity<User>().HasQueryFilter(x => x.IsActive);
            }
        }
    }
}
