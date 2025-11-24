using System;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using Xunit;

namespace UnoLisServer.Test.HelpersTest
{
    public class PendingRegistrationHelperTest
    {
        private readonly IPendingRegistrationHelper _helper;

        public PendingRegistrationHelperTest()
        {
            _helper = PendingRegistrationHelper.Instance;
        }

        [Fact]
        public void StoreAndRetrieve_ValidData_ShouldReturnData()
        {
            string email = "store@test.com";
            var data = new PendingRegistration
            {
                Nickname = "StoreNick",
                FullName = "Store Name",
                HashedPassword = "Hash"
            };

            _helper.StorePendingRegistration(email, data);
            var retrieved = _helper.GetAndRemovePendingRegistration(email);

            Assert.NotNull(retrieved);
            Assert.Equal("StoreNick", retrieved.Nickname);
            Assert.Equal("Hash", retrieved.HashedPassword);
        }

        [Fact]
        public void GetAndRemove_Twice_ShouldReturnNullSecondTime()
        {
            string email = "once@test.com";
            var data = new PendingRegistration { Nickname = "Once" };
            _helper.StorePendingRegistration(email, data);

            var firstTry = _helper.GetAndRemovePendingRegistration(email);
            var secondTry = _helper.GetAndRemovePendingRegistration(email);

            Assert.NotNull(firstTry);
            Assert.Null(secondTry); 
        }

        [Fact]
        public void GetAndRemove_NonExistent_ShouldReturnNull()
        {
            var result = _helper.GetAndRemovePendingRegistration("ghost@test.com");
            Assert.Null(result);
        }

        [Fact]
        public void Store_OverwriteExisting_ShouldUpdateData()
        {
            string email = "update@test.com";
            var data1 = new PendingRegistration { Nickname = "OldNick" };
            var data2 = new PendingRegistration { Nickname = "NewNick" };

            _helper.StorePendingRegistration(email, data1);
            _helper.StorePendingRegistration(email, data2);
            var result = _helper.GetAndRemovePendingRegistration(email);

            Assert.NotNull(result);
            Assert.Equal("NewNick", result.Nickname);
        }
    }
}