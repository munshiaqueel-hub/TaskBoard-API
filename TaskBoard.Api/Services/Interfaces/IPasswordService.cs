public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string hashed, string password);
}