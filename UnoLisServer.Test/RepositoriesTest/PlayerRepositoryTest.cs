using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Reflection; // Necesario para Reflection
using System.Threading.Tasks;
using Effort;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using Xunit;

namespace UnoLisServer.Test
{
    public class PlayerRepositoryTest
    {
        private readonly DbConnection _connection;

        public PlayerRepositoryTest()
        {
            // -----------------------------------------------------------------------
            // 1. AUTO-DETECCIÓN DE RECURSOS (La solución al error de Metadata)
            // -----------------------------------------------------------------------
            var assembly = typeof(PlayerRepository).Assembly;
            string assemblyName = assembly.GetName().Name;

            // Buscamos el nombre real del recurso incrustado en la DLL
            string resourceName = assembly.GetManifestResourceNames()
                                          .FirstOrDefault(x => x.EndsWith(".csdl"));

            if (resourceName == null)
            {
                throw new InvalidOperationException($"[ERROR CRÍTICO] No se encontró ningún archivo .csdl incrustado en {assemblyName}. Verifica que tu archivo .edmx tenga la propiedad 'Build Action' en 'Embedded Resource'.");
            }

            // El recurso viene como "Namespace.Carpeta.Nombre.csdl".
            // Para la cadena de metadatos, necesitamos el nombre base sin la extensión.
            string baseName = resourceName.Replace(".csdl", "");

            var entityBuilder = new EntityConnectionStringBuilder();

            // Usamos la sintaxis precisa: res://<Assembly>/<FullResourceName>
            entityBuilder.Metadata = $"res://{assemblyName}/{baseName}.csdl|res://{assemblyName}/{baseName}.ssdl|res://{assemblyName}/{baseName}.msl";

            entityBuilder.Provider = "System.Data.SqlClient";
            entityBuilder.ProviderConnectionString = "data source=.;initial catalog=TestDb;integrated security=True";

            // Creamos la conexión en memoria
            _connection = EntityConnectionFactory.CreateTransient(entityBuilder.ToString());

            // -----------------------------------------------------------------------
            // 2. SEMBRADO DE DATOS (SEED DATA)
            // -----------------------------------------------------------------------
            // IMPORTANTE: Usamos la conexión _connection que acabamos de crear
            using (var context = new UNOContext(_connection))
            {
                var player = new Player
                {
                    nickname = "TikiTest",
                    fullName = "Tiki Tester",
                    idPlayer = 1,
                    Account = new List<Account>(),
                    PlayerStatistics = new List<PlayerStatistics>(),
                    SocialNetwork = new List<SocialNetwork>(),
                    AvatarsUnlocked = new List<AvatarsUnlocked>()
                };

                var account = new Account
                {
                    email = "tiki@test.com",
                    password = "hashed_password",
                    Player = player,
                    idAccount = 1,
                    Player_idPlayer = 1
                };

                var stats = new PlayerStatistics
                {
                    wins = 5,
                    matchesPlayed = 10,
                    Player = player,
                    idPlayerStatistics = 1,
                    Player_idPlayer = 1
                };

                var social = new SocialNetwork
                {
                    idRedSocial = 1,
                    tipoRedSocial = "Facebook",
                    linkRedSocial = "fb.com/tiki",
                    Player = player,
                    Player_idPlayer = 1
                };

                var avatar = new Avatar
                {
                    idAvatar = 1,
                    avatarName = "Gato",
                    avatarDescription = "Un gato genial",
                    avatarRarity = "Common"
                };

                var unlocked = new AvatarsUnlocked
                {
                    Player_idPlayer = 1,
                    Avatar_idAvatar = 1,
                    Player = player,
                    Avatar = avatar,
                    unlockedDate = DateTime.Now
                };

                player.Account.Add(account);
                player.PlayerStatistics.Add(stats);
                player.SocialNetwork.Add(social);
                player.AvatarsUnlocked.Add(unlocked);

                context.Player.Add(player);
                context.Account.Add(account);
                context.PlayerStatistics.Add(stats);
                context.SocialNetwork.Add(social);
                context.Avatar.Add(avatar);
                context.AvatarsUnlocked.Add(unlocked);

                context.SaveChanges();
            }
        }

        [Fact]
        public async Task GetPlayerProfile_UserExists_ReturnsBasicInfoAndAccount()
        {
            var repository = new PlayerRepository(() => new UNOContext(_connection));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.Equal("TikiTest", result.nickname);
            Assert.NotNull(result.Account);
            Assert.NotEmpty(result.Account);
            Assert.Equal("tiki@test.com", result.Account.First().email);
        }

        [Fact]
        public async Task GetPlayerProfile_UserExists_ReturnsStatsAndSocials()
        {
            var repository = new PlayerRepository(() => new UNOContext(_connection));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.NotNull(result.PlayerStatistics);
            Assert.Equal(5, result.PlayerStatistics.First().wins);
            Assert.NotNull(result.SocialNetwork);
            Assert.Equal("Facebook", result.SocialNetwork.First().tipoRedSocial);
        }

        [Fact]
        public async Task GetPlayerProfile_UserExists_ReturnsNestedAvatars()
        {
            var repository = new PlayerRepository(() => new UNOContext(_connection));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.NotNull(result.AvatarsUnlocked);
            var unlockedAvatar = result.AvatarsUnlocked.First();
            Assert.NotNull(unlockedAvatar.Avatar);
            Assert.Equal("Gato", unlockedAvatar.Avatar.avatarName);
        }

        [Fact]
        public async Task GetPlayerProfile_UserDoesNotExist_ReturnsNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_connection));
            var result = await repository.GetPlayerProfileByNicknameAsync("GhostUser");

            Assert.Null(result);
        }
    }
}