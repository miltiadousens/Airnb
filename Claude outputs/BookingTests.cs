using Airnb.Domain.Entities;
using Airnb.Domain.Enums;
using Airnb.Domain.Exceptions;
using Airnb.Domain.ValueObjects;

namespace Test.Airnb.Domain.Entities
{
    public class BookingTests
    {
        // ---------- Hjælpere: gyldige standardværdier ----------

        private static readonly Guid ValidGuestId = Guid.NewGuid();
        private static readonly Guid ValidHomeId = Guid.NewGuid();

        // Altid i fremtiden, så ValidateNotInPast ikke fejler
        private static TimeRange FutureRange(int startInDays = 10, int nights = 3)
        {
            var start = DateTime.UtcNow.Date.AddDays(startInDays);
            return new TimeRange(start, start.AddDays(nights));
        }

        private static TimeRange PastRange()
        {
            var start = DateTime.UtcNow.Date.AddDays(-5);
            return new TimeRange(start, start.AddDays(2));
        }

        private static Reciept ValidReciept(decimal amount = 3000m)
            => new Reciept(new Money(amount, "DKK"), "Rengøring");

        private static Booking CreateValidBooking()
            => Booking.Create(
                ValidGuestId,
                ValidHomeId,
                BookingStatus.Pending,
                FutureRange(),
                DateTime.UtcNow,
                2,
                ValidReciept());

        // ---------- Create: gyldige data ----------

        [Fact]
        public void Create_MedGyldigeData_SaetterAlleVaerdier()
        {
            var timeRange = FutureRange();
            var createdTime = DateTime.UtcNow;
            var reciept = ValidReciept();

            var booking = Booking.Create(
                ValidGuestId, ValidHomeId, BookingStatus.Pending,
                timeRange, createdTime, 4, reciept);

            Assert.Equal(ValidGuestId, booking.GuestProfileId);
            Assert.Equal(ValidHomeId, booking.HomeId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Equal(timeRange, booking.TimeRange);
            Assert.Equal(createdTime, booking.CreatedTime);
            Assert.Equal(4, booking.NumberOfGuests);
            Assert.Equal(reciept, booking.Reciept);
        }

        [Fact]
        public void Create_GiverBookingenEtId()
        {
            var booking = CreateValidBooking();

            Assert.NotEqual(Guid.Empty, booking.Id);
        }

        [Fact]
        public void Create_PrisPaaNul_Accepteres()
        {
            var booking = Booking.Create(
                ValidGuestId, ValidHomeId, BookingStatus.Pending,
                FutureRange(), DateTime.UtcNow, 1, ValidReciept(0m));

            Assert.Equal(0m, booking.Reciept.StartPrice.Amount);
        }

        // ---------- Create: ugyldige data ----------

        [Fact]
        public void Create_StartIFortiden_KasterDomainException()
        {
            var ex = Assert.Throws<DomainException>(() => Booking.Create(
                ValidGuestId, ValidHomeId, BookingStatus.Pending,
                PastRange(), DateTime.UtcNow, 2, ValidReciept()));

            Assert.Equal("Starttiden kan ikke være i fortiden.", ex.Message);
        }

        [Fact]
        public void Create_TomGuestId_KasterDomainException()
        {
            var ex = Assert.Throws<DomainException>(() => Booking.Create(
                Guid.Empty, ValidHomeId, BookingStatus.Pending,
                FutureRange(), DateTime.UtcNow, 2, ValidReciept()));

            Assert.Equal("GuestId cannot be empty.", ex.Message);
        }

        [Fact]
        public void Create_TomHomeId_KasterDomainException()
        {
            var ex = Assert.Throws<DomainException>(() => Booking.Create(
                ValidGuestId, Guid.Empty, BookingStatus.Pending,
                FutureRange(), DateTime.UtcNow, 2, ValidReciept()));

            Assert.Equal("HomeId cannot be empty.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Create_IngenEllerNegativeGaester_KasterDomainException(int numberOfGuests)
        {
            var ex = Assert.Throws<DomainException>(() => Booking.Create(
                ValidGuestId, ValidHomeId, BookingStatus.Pending,
                FutureRange(), DateTime.UtcNow, numberOfGuests, ValidReciept()));

            Assert.Equal("NumberOfGuests must be greater than zero.", ex.Message);
        }

        [Fact]
        public void Create_RecieptErNull_KasterDomainException()
        {
            var ex = Assert.Throws<DomainException>(() => Booking.Create(
                ValidGuestId, ValidHomeId, BookingStatus.Pending,
                FutureRange(), DateTime.UtcNow, 2, null!));

            Assert.Equal("Reciept cannot be null.", ex.Message);
        }

        [Fact]
        public void Create_NegativPris_KasterDomainException()
        {
            var ex = Assert.Throws<DomainException>(() => Booking.Create(
                ValidGuestId, ValidHomeId, BookingStatus.Pending,
                FutureRange(), DateTime.UtcNow, 2, ValidReciept(-1m)));

            Assert.Equal("Price cannot be negative.", ex.Message);
        }

        // ---------- UpdateDetails: gyldige ændringer ----------

        [Fact]
        public void UpdateDetails_MedGyldigeData_OpdatererAlleVaerdier()
        {
            var booking = CreateValidBooking();
            var newRange = FutureRange(startInDays: 20, nights: 5);
            var newReciept = ValidReciept(5000m);

            booking.UpdateDetails(BookingStatus.Confirmed, newRange, 3, newReciept);

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.Equal(newRange, booking.TimeRange);
            Assert.Equal(3, booking.NumberOfGuests);
            Assert.Equal(newReciept, booking.Reciept);
        }

        [Fact]
        public void UpdateDetails_SammePeriode_Accepteres()
        {
            var booking = CreateValidBooking();

            // Et nyt TimeRange-objekt med samme datoer er "ens", fordi TimeRange er en record
            var sameRange = new TimeRange(booking.TimeRange.Start, booking.TimeRange.End);

            booking.UpdateDetails(BookingStatus.Confirmed, sameRange, 2, ValidReciept());

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
        }

        [Fact]
        public void UpdateDetails_AendrerIkkeIdEllerCreatedTime()
        {
            var booking = CreateValidBooking();
            var idBefore = booking.Id;
            var createdBefore = booking.CreatedTime;

            booking.UpdateDetails(BookingStatus.Confirmed, FutureRange(30), 2, ValidReciept());

            Assert.Equal(idBefore, booking.Id);
            Assert.Equal(createdBefore, booking.CreatedTime);
        }

        // ---------- UpdateDetails: ugyldige ændringer ----------

        [Fact]
        public void UpdateDetails_NyPeriodeIFortiden_KasterDomainException()
        {
            var booking = CreateValidBooking();

            var ex = Assert.Throws<DomainException>(() =>
                booking.UpdateDetails(BookingStatus.Pending, PastRange(), 2, ValidReciept()));

            Assert.Equal("Starttiden kan ikke være i fortiden.", ex.Message);
        }

        [Fact]
        public void UpdateDetails_NyPeriodeIFortiden_AendrerIkkeBookingen()
        {
            var booking = CreateValidBooking();
            var rangeBefore = booking.TimeRange;

            Assert.Throws<DomainException>(() =>
                booking.UpdateDetails(BookingStatus.Confirmed, PastRange(), 2, ValidReciept()));

            // Tjekket sker før noget bliver sat, så bookingen er uændret
            Assert.Equal(rangeBefore, booking.TimeRange);
            Assert.Equal(BookingStatus.Pending, booking.Status);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UpdateDetails_IngenGaester_KasterDomainException(int numberOfGuests)
        {
            var booking = CreateValidBooking();

            Assert.Throws<DomainException>(() =>
                booking.UpdateDetails(BookingStatus.Pending, FutureRange(), numberOfGuests, ValidReciept()));
        }

        [Fact]
        public void UpdateDetails_NegativPris_KasterDomainException()
        {
            var booking = CreateValidBooking();

            Assert.Throws<DomainException>(() =>
                booking.UpdateDetails(BookingStatus.Pending, FutureRange(), 2, ValidReciept(-500m)));
        }

        [Fact]
        public void UpdateDetails_RecieptErNull_KasterDomainException()
        {
            var booking = CreateValidBooking();

            Assert.Throws<DomainException>(() =>
                booking.UpdateDetails(BookingStatus.Pending, FutureRange(), 2, null!));
        }
    }
}
