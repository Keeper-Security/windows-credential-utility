using WinCredentialManager;

namespace WinCredentialManagerTests
{
    public class CredentialManagerTests : IDisposable
    {
        public string ApplicationName { get; } = "testApp";
        public string UserName { get; } = "testUser";
        public string Configuration { get; } = "testConfig";

 
        [Fact]
        public void TestWriteReadCredential()
        {            
            CredentialManager.WriteCredential(ApplicationName, UserName, Configuration);

            Credential? cred = CredentialManager.ReadCredential(ApplicationName);

            Assert.NotNull(cred);
            Assert.Equal("testApp", cred.ApplicationName);
            Assert.Equal("testUser", cred.UserName);
            Assert.Equal("testConfig", cred.Password);
        }

        public void Dispose()
        {
            CredentialManager.DeleteCredential("testApp");
        }
    }   
}
