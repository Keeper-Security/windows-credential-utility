using WinCredentialManager;


namespace WinCredentialManagerTests
{
    public class ParsingConfigTests
    {
        public string Base64TestString = "eyJ1c2VybmFtZSI6InVzZXIiLCAicGFzc3dvcmQiOiJwYXNzIn0=";

        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        public string ParentDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        #pragma warning restore CS8602 // Dereference of a possibly null reference.

        [Fact]
        public void TestJsonInput()
        {
            string json = "{\"username\":\"user\", \"password\":\"pass\"}";

            string result = Parsing.ParseConfig(json);

            Assert.Equal(Base64TestString, result);
        }

        [Fact]
        public void TestBase64Input()
        {
            string result = Parsing.ParseConfig(Base64TestString);

            Assert.Equal(Base64TestString, result);
        }

        [Fact]
        public void TestFileInput()
        {
            string filePath = Path.Combine(ParentDirectory, "test-config.json");

            string result = Parsing.ParseConfig(filePath);

            Assert.Equal(Base64TestString, result);
        }

        [Fact]
        public void TestThrowException()
        {
            string invalidString = "invalidString";

            Action act = () => Parsing.ParseConfig(invalidString);

            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void TestThrowExceptionFile()
        {
            string filePath = Path.Combine(ParentDirectory, "invalid-file.txt");

            Action act = () => Parsing.ParseConfig(filePath);

            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void TestThrowExceptionFileType()
        {
            string filePath = Path.Combine(ParentDirectory, "invalid-file.json");

            Action act = () => Parsing.ParseConfig(filePath);

            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void TestThrowExceptionJson()
        {
            string invalidJson = "{\"username\":\"user\", \"password\":\"pass\"";

            Action act = () => Parsing.ParseConfig(invalidJson);

            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void TestThrowExceptionBase64()
        {
            string invalidBase64 = "invalidBase64";

            Action act = () => Parsing.ParseConfig(invalidBase64);

            Assert.Throws<ArgumentException>(act);
        }
    }
}