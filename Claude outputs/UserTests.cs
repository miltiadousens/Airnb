using Airnb.Domain.Entities;
using Airnb.Domain.Exceptions;

namespace Airnb.Tests.Domain
{
    public class UserTests
    {
        // Gyldige standardværdier, så hver test kun ændrer det, den tester
        private const string ValidEmail = "test@eksempel.dk";
        private const string ValidName = "Test Testesen";
        private const string ValidHash = "AQAAAAIAAYagAAAAEhashedpassword";

        // ---------- Oprettelse med gyldige data ----------

        [Fact]
        public void Create_MedGyldigeData_OpretterBruger()
        {
            var user = User.Create(ValidEmail, ValidName, ValidHash);

            Assert.Equal(ValidEmail, user.Email);
            Assert.Equal(ValidName, user.Name);
            Assert.Equal(ValidHash, user.PasswordHash);
        }

        [Fact]
        public void Create_GiverBrugerenEtId()
        {
            var user = User.Create(ValidEmail, ValidName, ValidHash);

            Assert.NotEqual(Guid.Empty, user.Id);
        }

        [Fact]
        public void Create_ToBrugere_FaarForskelligeId()
        {
            var user1 = User.Create(ValidEmail, ValidName, ValidHash);
            var user2 = User.Create(ValidEmail, ValidName, ValidHash);

            Assert.NotEqual(user1.Id, user2.Id);
        }

        // ---------- Normalisering af email og navn ----------

        [Fact]
        public void Create_EmailMedStoreBogstaverOgMellemrum_BliverNormaliseret()
        {
            var user = User.Create("  Test@Eksempel.DK  ", ValidName, ValidHash);

            Assert.Equal("test@eksempel.dk", user.Email);
        }

        [Fact]
        public void Create_NavnMedMellemrum_BliverTrimmet()
        {
            var user = User.Create(ValidEmail, "  Test Testesen  ", ValidHash);

            Assert.Equal("Test Testesen", user.Name);
        }

        // ---------- Email-validering ----------

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_TomEmail_KasterDomainException(string email)
        {
            var ex = Assert.Throws<DomainException>(() => User.Create(email, ValidName, ValidHash));

            Assert.Equal("Email må ikke være tom.", ex.Message);
        }

        [Theory]
        [InlineData("test")]                  // intet @
        [InlineData("test@")]                 // intet domæne
        [InlineData("@eksempel.dk")]          // intet navn før @
        [InlineData("test@eksempel")]         // intet punktum i domænet
        [InlineData("test@@eksempel.dk")]     // to @
        [InlineData("te st@eksempel.dk")]     // mellemrum i midten
        public void Create_UgyldigtEmailFormat_KasterDomainException(string email)
        {
            var ex = Assert.Throws<DomainException>(() => User.Create(email, ValidName, ValidHash));

            Assert.Equal("Email har ikke et gyldigt format.", ex.Message);
        }

        [Theory]
        [InlineData("a@b.dk")]
        [InlineData("fornavn.efternavn@firma.com")]
        [InlineData("test+airnb@gmail.com")]
        public void Create_GyldigeEmailFormater_Accepteres(string email)
        {
            var user = User.Create(email, ValidName, ValidHash);

            Assert.Equal(email.ToLowerInvariant(), user.Email);
        }

        // ---------- Navn-validering ----------

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_TomtNavn_KasterDomainException(string name)
        {
            var ex = Assert.Throws<DomainException>(() => User.Create(ValidEmail, name, ValidHash));

            Assert.Equal("Navn må ikke være tomt.", ex.Message);
        }

        // ---------- Password-hash-validering ----------

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_TomPasswordHash_KasterDomainException(string? hash)
        {
            var ex = Assert.Throws<DomainException>(() => User.Create(ValidEmail, ValidName, hash!));

            Assert.Equal("Password hash mangler.", ex.Message);
        }

        // ---------- ChangePassword ----------

        [Fact]
        public void ChangePassword_MedGyldigHash_OpdatererHash()
        {
            var user = User.Create(ValidEmail, ValidName, ValidHash);

            user.ChangePassword("NyHash123");

            Assert.Equal("NyHash123", user.PasswordHash);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ChangePassword_MedTomHash_KasterDomainException(string newHash)
        {
            var user = User.Create(ValidEmail, ValidName, ValidHash);

            Assert.Throws<DomainException>(() => user.ChangePassword(newHash));
        }
    }
}
